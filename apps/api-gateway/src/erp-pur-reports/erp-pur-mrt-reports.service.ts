/**
 * Purchasing dataset provider (Wave G5). Claims exactly the report
 * keys present in PUR_MRT_CONFIGS — including the honest-empty ones
 * (contracts m4_pf, PI exchange m4_pie, PO↔SI premium linkage), which
 * render header-only. Unconfigured pur.* keys keep the registry's
 * generic empty fallback.
 *
 * SQL configs execute like the md/fin/inv providers (whitelist
 * configs, bound params, unmapped declared fields → NULL, 2,000-row
 * cap). Terbilang (legacy MySQL f_nominal) is stamped in TS via the
 * engine's terbilang() per PurTerbilangSpec in the configs.
 */

import { Injectable, Logger, OnModuleInit } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import type { ReportData } from '../erp-report-engine/engine-types-v2';
import { terbilang } from '../erp-report-engine/terbilang';
import {
  ReportProviderRegistry,
  normalizeRow,
  type ReportDataProvider,
  type ReportRenderRequest,
} from '../erp-report-registry/report-data-provider';
import { PUR_MRT_CONFIGS } from './pur-mrt-configs-map';
import type { PurDatasetConfig } from './pur-mrt-configs';

const DEFAULT_LIMIT = 2000;

const CURRENCY_SUFFIX: Record<string, string> = {
  IDR: 'Rupiah',
  USD: 'Dollar Amerika',
  SGD: 'Dollar Singapura',
  EUR: 'Euro',
  JPY: 'Yen Jepang',
};

@Injectable()
export class ErpPurMrtReportsService implements ReportDataProvider, OnModuleInit {
  readonly prefix = 'pur.';
  private readonly logger = new Logger(ErpPurMrtReportsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly registry: ReportProviderRegistry,
  ) {}

  onModuleInit(): void {
    this.registry.register(this);
  }

  canHandle(reportKey: string): boolean {
    return reportKey in PUR_MRT_CONFIGS;
  }

  async build(req: ReportRenderRequest): Promise<ReportData> {
    const config = PUR_MRT_CONFIGS[req.reportKey];
    const datasets: Record<string, Array<Record<string, unknown>>> = {};
    for (const ds of req.datasets) {
      const cfg = config?.datasets[ds.name];
      if (!cfg || cfg.empty || !cfg.from) {
        datasets[ds.name] = [];
        continue;
      }
      // Terbilang amount/currency columns are selected even when the
      // template only declares the spelled-out column itself, then
      // stripped again after stamping.
      const extra: string[] = [];
      for (const spec of cfg.terbilang ?? []) {
        if (!ds.columns.includes(spec.amountColumn)) extra.push(spec.amountColumn);
        if (spec.currencyColumn && !ds.columns.includes(spec.currencyColumn)) {
          extra.push(spec.currencyColumn);
        }
      }
      const rows = await this.runDataset(cfg, [...ds.columns, ...extra], req.params);
      const stamped = this.stampTerbilang(rows, cfg, ds.columns);
      for (const row of stamped) for (const col of extra) delete row[col];
      datasets[ds.name] = stamped;
    }
    return { datasets };
  }

  /** Legacy f_nominal(amount, currency) stamped per document group. */
  private stampTerbilang(
    rows: Array<Record<string, unknown>>,
    cfg: PurDatasetConfig,
    declaredColumns: string[],
  ): Array<Record<string, unknown>> {
    for (const spec of cfg.terbilang ?? []) {
      if (!declaredColumns.includes(spec.column)) continue;
      const groups = new Map<string, Array<Record<string, unknown>>>();
      for (const row of rows) {
        const key = String(row[spec.groupByColumn] ?? '');
        const group = groups.get(key);
        if (group) group.push(row);
        else groups.set(key, [row]);
      }
      for (const group of groups.values()) {
        const total =
          spec.mode === 'sum'
            ? group.reduce((sum, r) => sum + (Number(r[spec.amountColumn]) || 0), 0)
            : Number(group[0]?.[spec.amountColumn]) || 0;
        const code = spec.currencyColumn
          ? String(group[0]?.[spec.currencyColumn] ?? 'IDR')
          : 'IDR';
        const text = `${terbilang(total)} ${CURRENCY_SUFFIX[code] ?? code}`;
        for (const row of group) row[spec.column] = text;
      }
    }
    return rows;
  }

  private async runDataset(
    cfg: PurDatasetConfig,
    declaredColumns: string[],
    params: Record<string, unknown>,
  ): Promise<Array<Record<string, unknown>>> {
    // `?` placeholders inside select/from/where/groupBy/orderBy are
    // bound from cfg.bindParams in walk order (select in declared
    // column order, then from, where, groupBy, orderBy — the final
    // SQL text order). Absent params bind NULL so alignment never
    // shifts.
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
