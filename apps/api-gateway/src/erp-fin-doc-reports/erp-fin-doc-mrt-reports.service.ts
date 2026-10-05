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
import { FIN_DOC_MRT_CONFIGS } from './fin-doc-mrt-configs-map';
import type { FinDatasetConfig } from './fin-doc-mrt-configs';

const DEFAULT_LIMIT = 2000;

const CURRENCY_SUFFIX: Record<string, string> = {
  IDR: 'Rupiah',
  USD: 'Dollar Amerika',
  SGD: 'Dollar Singapura',
  EUR: 'Euro',
  JPY: 'Yen Jepang',
};

/**
 * Finance DOCUMENT dataset provider (Wave G2). Claims the `fin.`
 * prefix for registry renders; statement/card builders (Wave G3)
 * extend FIN_DOC_MRT_CONFIGS or register a more specific provider.
 * Terbilang is stamped in TS (the engine's terbilang, replacing the
 * legacy MySQL f_nominal) per FinTerbilangSpec in the configs.
 */
@Injectable()
export class ErpFinDocMrtReportsService implements ReportDataProvider, OnModuleInit {
  readonly prefix = 'fin.';
  private readonly logger = new Logger(ErpFinDocMrtReportsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly registry: ReportProviderRegistry,
  ) {}

  onModuleInit(): void {
    this.registry.register(this);
  }

  canHandle(reportKey: string): boolean {
    return reportKey.startsWith('fin.');
  }

  async build(req: ReportRenderRequest): Promise<ReportData> {
    const config = FIN_DOC_MRT_CONFIGS[req.reportKey];
    const datasets: Record<string, Array<Record<string, unknown>>> = {};
    for (const ds of req.datasets) {
      const cfg = config?.datasets[ds.name];
      if (!cfg || cfg.empty || !cfg.from) {
        datasets[ds.name] = [];
        continue;
      }
      const rows = await this.runDataset(cfg, ds.columns, req.params);
      datasets[ds.name] = this.stampTerbilang(rows, cfg, ds.columns);
    }
    return { datasets };
  }

  /** Legacy f_nominal(amount, currency) stamped per document group. */
  private stampTerbilang(
    rows: Array<Record<string, unknown>>,
    cfg: FinDatasetConfig,
    declaredColumns: string[],
  ): Array<Record<string, unknown>> {
    const spec = cfg.terbilang;
    if (!spec || !declaredColumns.includes(spec.column)) return rows;
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
    return rows;
  }

  private async runDataset(
    cfg: FinDatasetConfig,
    declaredColumns: string[],
    params: Record<string, unknown>,
  ): Promise<Array<Record<string, unknown>>> {
    // Wave G3: `?` placeholders inside select/from/where/groupBy/orderBy
    // are bound from cfg.bindParams in walk order (select in declared
    // column order, then from, where, groupBy, orderBy — the final SQL
    // text order). Absent params bind NULL so alignment never shifts.
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
