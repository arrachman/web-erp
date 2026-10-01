/**
 * Purchasing report registry + dispatcher. Composes all resolver builders into
 * one keyed map and turns (key, filters) into a uniform ReportDataset. The
 * controller (view + export) and the exporters all consume the result of
 * getDataset().
 */

import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { buildPurReports } from './pur-report-resolvers';
import {
  ReportCatalogItem,
  ReportDataset,
  ReportDef,
  ReportFilters,
} from './report-types';

@Injectable()
export class PurReportsService {
  private readonly registry: Map<string, ReportDef>;

  constructor(private readonly prisma: PrismaService) {
    const defs = buildPurReports({ prisma: this.prisma });
    this.registry = new Map(defs.map((d) => [d.key, d]));
  }

  /** Catalog of available reports (key, title, group) for discovery/UI. */
  list(): ReportCatalogItem[] {
    return [...this.registry.values()].map(({ key, title, group }) => ({
      key,
      title,
      group,
    }));
  }

  private def(key: string): ReportDef {
    const def = this.registry.get(key);
    if (!def) throw new NotFoundException(`Laporan '${key}' tidak ditemukan.`);
    return def;
  }

  /** Column definitions for a report, without resolving rows (design-time use). */
  getColumns(key: string) {
    return this.def(key).columns;
  }

  /** Resolve a report to its uniform dataset (used by view AND export). */
  async getDataset(key: string, filters: ReportFilters): Promise<ReportDataset> {
    const def = this.def(key);
    const { rows, summary, total } = await def.resolve(filters);
    return {
      key: def.key,
      title: def.title,
      columns: def.columns,
      rows,
      summary: summary ?? [],
      filters,
      total: total ?? rows.length,
      generatedAt: new Date().toISOString(),
    };
  }
}
