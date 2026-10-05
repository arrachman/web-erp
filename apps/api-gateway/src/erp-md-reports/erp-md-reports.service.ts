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
import { MD_REPORT_CONFIGS } from './md-report-configs-map';
import type { MdDatasetConfig } from './md-report-configs';

const DEFAULT_LIMIT = 2000;

/**
 * Master Data dataset provider (Wave G1). Executes the whitelisted
 * configs in md-report-configs-map.ts via bound-parameter raw SQL.
 * Declared template fields without a config mapping are selected as
 * NULL so every binding resolves; datasets without an ERP equivalent
 * render empty (see config notes).
 */
@Injectable()
export class ErpMdReportsService implements ReportDataProvider, OnModuleInit {
  readonly prefix = 'md.';
  private readonly logger = new Logger(ErpMdReportsService.name);

  constructor(
    private readonly prisma: PrismaService,
    private readonly registry: ReportProviderRegistry,
  ) {}

  onModuleInit(): void {
    this.registry.register(this);
  }

  canHandle(reportKey: string): boolean {
    return reportKey.startsWith('md.');
  }

  async build(req: ReportRenderRequest): Promise<ReportData> {
    const config = MD_REPORT_CONFIGS[req.reportKey];
    const datasets: Record<string, Array<Record<string, unknown>>> = {};
    for (const ds of req.datasets) {
      const cfg = config?.datasets[ds.name];
      if (!cfg || cfg.empty || !cfg.from) {
        datasets[ds.name] = [];
        continue;
      }
      datasets[ds.name] = await this.runDataset(cfg, ds.columns, req.params);
    }
    // Helper dataset a few label templates bind (company currency).
    if (req.datasets.some((d) => d.name.toLowerCase() === 'matauangfungsional')) {
      datasets.MataUangFungsional = [{ MataUangFungsional: await this.companyCurrency() }];
    }
    return { datasets };
  }

  private async companyCurrency(): Promise<string> {
    try {
      const row = await this.prisma.erpSetting.findFirst({
        where: { group: 'company', key: 'currency' },
        select: { value: true },
      });
      return row?.value ?? 'IDR';
    } catch {
      return 'IDR';
    }
  }

  private async runDataset(
    cfg: MdDatasetConfig,
    declaredColumns: string[],
    params: Record<string, unknown>,
  ): Promise<Array<Record<string, unknown>>> {
    const selectParts = declaredColumns.map((col) => {
      const expr = cfg.select[col];
      return Prisma.sql`${Prisma.raw(expr ?? 'NULL')} AS ${Prisma.raw(`"${col}"`)}`;
    });
    if (selectParts.length === 0) return [];
    let sql = Prisma.sql`SELECT ${Prisma.join(selectParts, ', ')} FROM ${Prisma.raw(cfg.from!)} WHERE 1 = 1`;
    if (cfg.deletedAlias) {
      sql = Prisma.sql`${sql} AND ${Prisma.raw(cfg.deletedAlias)}.deleted_at IS NULL`;
    }
    if (cfg.where) sql = Prisma.sql`${sql} AND (${Prisma.raw(cfg.where)})`;
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
    if (cfg.orderBy) sql = Prisma.sql`${sql} ORDER BY ${Prisma.raw(cfg.orderBy)}`;
    sql = Prisma.sql`${sql} LIMIT ${DEFAULT_LIMIT}`;
    try {
      const rows = await this.prisma.$queryRaw<Array<Record<string, unknown>>>(sql);
      return rows.map(normalizeRow);
    } catch (err) {
      this.logger.error(`Dataset query gagal: ${err instanceof Error ? err.message : String(err)}`);
      throw err;
    }
  }
}
