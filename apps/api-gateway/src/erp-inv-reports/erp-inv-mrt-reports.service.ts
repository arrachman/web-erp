/**
 * Inventory dataset provider (Wave G4). Claims exactly the report
 * keys present in INV_MRT_CONFIGS (SQL configs) or INV_MRT_COMPUTED
 * (TS replay families) — including the 25 inventory-analytics keys
 * registered under fin.* (the Finance provider claims only its own
 * config keys, so the sets never overlap).
 *
 * SQL configs execute like the md/fin providers (whitelist configs,
 * bound params, unmapped declared fields → NULL, 2,000-row cap).
 * Computed families replay the derived stock-event stream with
 * moving-average costing (inv-mrt-events.ts).
 */

import { Injectable, Logger, OnModuleInit } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import type { ReportData } from '../erp-report-engine/engine-types-v2';
import {
  ReportProviderRegistry,
  normalizeRow,
  type ReportDataProvider,
  type ReportRenderRequest,
} from '../erp-report-registry/report-data-provider';
import { EVENTS_SELECT, type InvDatasetConfig } from './inv-mrt-configs';
import { INV_MRT_COMPUTED, INV_MRT_CONFIGS } from './inv-mrt-configs-map';
import {
  fetchStockEvents,
  filterEvents,
  replay,
  type StockEvent,
} from './inv-mrt-events';
import { toKsRow, toMsRow, toPdRow } from './inv-mrt-shapers';

const DEFAULT_LIMIT = 2000;

@Injectable()
export class ErpInvMrtReportsService implements ReportDataProvider, OnModuleInit {
  readonly prefix = 'inv.';
  private readonly logger = new Logger(ErpInvMrtReportsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly registry: ReportProviderRegistry,
  ) {}

  onModuleInit(): void {
    this.registry.register(this);
  }

  canHandle(reportKey: string): boolean {
    return reportKey in INV_MRT_CONFIGS || reportKey in INV_MRT_COMPUTED;
  }

  async build(req: ReportRenderRequest): Promise<ReportData> {
    const config = INV_MRT_CONFIGS[req.reportKey];
    const family = INV_MRT_COMPUTED[req.reportKey];
    const datasets: Record<string, Array<Record<string, unknown>>> = {};
    for (const ds of req.datasets) {
      if (family && ds.name === 'DS1') {
        datasets[ds.name] = await this.buildComputed(family, ds.columns, req.params);
        continue;
      }
      const cfg = config?.datasets[ds.name];
      if (!cfg || cfg.empty || !cfg.from) {
        datasets[ds.name] = [];
        continue;
      }
      datasets[ds.name] = await this.runDataset(cfg, ds.columns, req.params);
    }
    return { datasets };
  }

  /* ---------------- computed (replay) families ---------------- */

  private async buildComputed(
    family: string,
    cols: string[],
    params: Record<string, unknown>,
  ): Promise<Array<Record<string, unknown>>> {
    if (family === 'stok-pivot') return this.buildStokPivot(cols);
    const events = replay(await fetchStockEvents(this.prisma));
    const filtered = filterEvents(events, {
      periodStart: strParam(params.period_start),
      periodEnd: strParam(params.period_end),
      warehouse: strParam(params.warehouse),
      partner: strParam(params.partner),
      division: strParam(params.division),
      costCenter: strParam(params.cost_center),
      project: strParam(params.project),
    });
    const seq = new Map<string, number>();
    const nextSeq = (ev: StockEvent): number => {
      const k = `${ev.itemId}|${ev.whId ?? 0}`;
      const n = (seq.get(k) ?? 0) + 1;
      seq.set(k, n);
      return n;
    };
    let rows: Array<Record<string, unknown>>;
    if (family === 'ks' || family === 'ks-summary') {
      rows = filtered.map((ev) => toKsRow(ev, cols, nextSeq(ev)));
      if (family === 'ks-summary') this.fillMutasiSummary(rows);
    } else if (family === 'ms') {
      rows = filtered.map((ev) => toMsRow(ev, cols, nextSeq(ev)));
    } else if (family === 'ms-sumber') {
      rows = filtered.map((ev) => this.toMsSumberRow(ev, cols, nextSeq(ev)));
    } else {
      rows = filtered.map((ev) => toPdRow(ev, cols));
    }
    return rows.slice(0, DEFAULT_LIMIT);
  }

  /** fin.nilaipersediaangudangdetail: per item×warehouse window aggregates on every row. */
  private fillMutasiSummary(rows: Array<Record<string, unknown>>): void {
    const groups = new Map<string, Array<Record<string, unknown>>>();
    for (const row of rows) {
      const k = `${row.ksidbarang}|${row.ksgudang}`;
      const g = groups.get(k);
      if (g) g.push(row);
      else groups.set(k, [row]);
    }
    for (const g of groups.values()) {
      const jml = g.reduce((s, r) => s + (Number(r.ksjmlmasuk) || 0) - (Number(r.ksjmlkeluar) || 0), 0);
      const nilai = g.reduce(
        (s, r) => s + (Number(r.ksnilaimasuk) || 0) - (Number(r.ksnilaikeluar) || 0),
        0,
      );
      for (const r of g) {
        if ('ksjmlmutasi' in r) r.ksjmlmutasi = jml;
        if ('ksnilaimutasi' in r) r.ksnilaimutasi = nilai;
        if ('kshargamutasi' in r) r.kshargamutasi = jml !== 0 ? nilai / jml : null;
      }
    }
  }

  private toMsSumberRow(
    ev: StockEvent,
    cols: string[],
    seq: number,
  ): Record<string, unknown> {
    const row = toMsRow(ev, cols, seq);
    const pivot = sumberPivotColumn(ev);
    const fill: Record<string, unknown> = {
      mssaldoawal: ev.openingQty,
      mssaldoakhir: ev.balanceQty,
      nilaisaldoawal: ev.openingValue,
      nilaisaldoakhir: ev.balanceValue,
      mshargajual: ev.price,
    };
    if (pivot) fill[pivot] = ev.signedQty;
    for (const [k, v] of Object.entries(fill)) if (k in row) row[k] = v;
    return row;
  }

  /** inv.stok: per-item pivot of balances over the first 10 warehouses (by id). */
  private async buildStokPivot(
    cols: string[],
  ): Promise<Array<Record<string, unknown>>> {
    const rows = await this.prisma.$queryRaw<Array<Record<string, unknown>>>(
      Prisma.raw(`
        SELECT e.item_id, e.wh_id, SUM(e.qty) AS bal
        FROM (${EVENTS_SELECT}) e
        GROUP BY e.item_id, e.wh_id
      `),
    );
    const warehouses = await this.prisma.erpWarehouse.findMany({
      where: { deletedAt: null },
      select: { id: true, code: true },
      orderBy: { id: 'asc' },
      take: 10,
    });
    const items = await this.prisma.erpItem.findMany({
      where: { deletedAt: null },
      select: {
        id: true, code: true, name: true, baseUnitId: true, primarySupplierId: true,
      },
    });
    const units = await this.prisma.erpUnit.findMany({ select: { id: true, name: true } });
    const partners = await this.prisma.erpPartner.findMany({
      select: { id: true, name: true },
    });
    const unitName = new Map(units.map((u) => [u.id.toString(), u.name]));
    const partnerName = new Map(partners.map((p) => [p.id.toString(), p.name]));
    const balByItem = new Map<string, Map<string, number>>();
    for (const r of rows) {
      const itemKey = String(r.item_id);
      const m = balByItem.get(itemKey) ?? new Map<string, number>();
      m.set(String(r.wh_id), Number(r.bal) || 0);
      balByItem.set(itemKey, m);
    }
    const out: Array<Record<string, unknown>> = [];
    for (const item of items) {
      const bals = balByItem.get(item.id.toString());
      if (!bals) continue;
      const row: Record<string, unknown> = {};
      for (const c of cols) row[c] = null;
      row.kodebarang = item.code;
      row.namabarang = item.name;
      row.idbarang = Number(item.id);
      row.satuan = item.baseUnitId ? unitName.get(item.baseUnitId.toString()) ?? null : null;
      row.supplier = item.primarySupplierId
        ? partnerName.get(item.primarySupplierId.toString()) ?? null
        : null;
      let total = 0;
      warehouses.forEach((w, idx) => {
        const bal = bals.get(w.id.toString()) ?? 0;
        total += bal;
        if (`g${idx + 1}` in row) row[`g${idx + 1}`] = w.code;
        if (`j${idx + 1}` in row) row[`j${idx + 1}`] = bal;
      });
      row.total = total;
      out.push(row);
    }
    return out.slice(0, DEFAULT_LIMIT);
  }

  /* ---------------- SQL config executor (md/fin pattern) ---------------- */

  private async runDataset(
    cfg: InvDatasetConfig,
    declaredColumns: string[],
    params: Record<string, unknown>,
  ): Promise<Array<Record<string, unknown>>> {
    const bindList = cfg.bindParams ?? [];
    let bindIdx = 0;
    const bindRaw = (text: string): Prisma.Sql => {
      if (!text.includes('?')) return Prisma.raw(text);
      const fragments = text.split('?');
      let composed = Prisma.sql`${Prisma.raw(fragments[0])}`;
      for (let i = 1; i < fragments.length; i++) {
        const def = bindList[bindIdx++];
        let value: unknown = null;
        if (def) {
          const raw = params[def.name];
          if (raw !== undefined && raw !== null && raw !== '') {
            value = def.kind === 'number' ? Number(raw) : String(raw);
            if (def.kind === 'number' && Number.isNaN(value as number)) value = null;
          }
        }
        composed = Prisma.sql`${composed}${value}${Prisma.raw(fragments[i])}`;
      }
      return composed;
    };
    const selectParts = declaredColumns.map((col) => {
      const expr = cfg.select[col];
      return Prisma.sql`${bindRaw(expr ?? 'NULL')} AS ${Prisma.raw(`"${col}"`)}`;
    });
    if (selectParts.length === 0) return [];
    let sql = Prisma.sql`SELECT ${Prisma.join(selectParts, ', ')} FROM ${bindRaw(cfg.from!)} WHERE 1 = 1`;
    if (cfg.deletedAlias) {
      sql = Prisma.sql`${sql} AND ${Prisma.raw(cfg.deletedAlias)}.deleted_at IS NULL`;
    }
    if (cfg.where) sql = Prisma.sql`${sql} AND (${bindRaw(cfg.where)})`;
    for (const [paramName, filter] of Object.entries(cfg.paramFilters ?? {})) {
      const raw = params[paramName];
      if (raw === undefined || raw === null || raw === '') continue;
      const value = filter.kind === 'number' ? Number(raw) : String(raw);
      if (filter.kind === 'number' && Number.isNaN(value as number)) continue;
      const fragments = filter.sql.split('?');
      let composed = Prisma.sql`${Prisma.raw(fragments[0])}`;
      for (let i = 1; i < fragments.length; i++) {
        composed = Prisma.sql`${composed}${value}${Prisma.raw(fragments[i])}`;
      }
      sql = Prisma.sql`${sql} AND (${composed})`;
    }
    if (cfg.groupBy) sql = Prisma.sql`${sql} GROUP BY ${bindRaw(cfg.groupBy)}`;
    if (cfg.orderBy) sql = Prisma.sql`${sql} ORDER BY ${bindRaw(cfg.orderBy)}`;
    sql = Prisma.sql`${sql} LIMIT ${DEFAULT_LIMIT}`;
    try {
      const rows = await this.prisma.$queryRaw<Array<Record<string, unknown>>>(sql);
      return rows.map(normalizeRow);
    } catch (err) {
      this.logger.error(
        `Dataset query gagal: ${err instanceof Error ? err.message : String(err)}`,
      );
      throw err;
    }
  }
}

function strParam(v: unknown): string | undefined {
  if (v === undefined || v === null || v === '') return undefined;
  return String(v);
}

const SUMBER_PIVOT: Record<string, string> = {
  OPENING: 'msm3ib',
  TRANSFER: 'msm3ts',
  TRANSFER_RECEIPT: 'msm3rs',
};

/** Legacy per-source pivot column for mutasistokrekapsumber (heuristic by source/type). */
function sumberPivotColumn(ev: StockEvent): string | null {
  const byType = SUMBER_PIVOT[ev.type];
  if (byType) return byType;
  const src = (ev.source ?? '').toUpperCase();
  if (ev.type === 'RETURN') return src.includes('SLS') || src.includes('DO') ? 'msm5sr' : 'msm4prt';
  if (ev.type === 'ISSUE') {
    if (src.includes('GRN') || src.includes('PUR')) return 'msm4grn';
    if (src.includes('DELIVERY') || src.includes('DO')) return 'msm5do';
    return 'msm5si';
  }
  return null;
}
