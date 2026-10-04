/** Registry and dispatcher for Finance transaction-document reports. */

import { Injectable, NotFoundException } from '@nestjs/common';
import { PrismaService } from '../prisma/prisma.service';
import { buildFinDocumentReports } from './report-configs';
import { ReportCatalogItem, ReportDataset, ReportDef, ReportFilters } from './report-types';

@Injectable()
export class FinDocReportsService {
  private readonly registry: Map<string, ReportDef>;

  constructor(private readonly prisma: PrismaService) {
    const definitions = buildFinDocumentReports(this.prisma);
    this.registry = new Map(definitions.map((definition) => [definition.key, definition]));
  }

  list(): ReportCatalogItem[] {
    return [...this.registry.values()].map(({ key, title, group }) => ({ key, title, group }));
  }

  private definition(key: string): ReportDef {
    const definition = this.registry.get(key);
    if (!definition) throw new NotFoundException(`Laporan '${key}' tidak ditemukan.`);
    return definition;
  }

  async getDataset(key: string, filters: ReportFilters): Promise<ReportDataset> {
    const definition = this.definition(key);
    const { rows, summary, total, charts } = await definition.resolve(filters);
    return {
      key: definition.key,
      title: definition.title,
      columns: definition.columns,
      rows,
      summary: summary ?? [],
      ...(charts ? { charts } : {}),
      filters,
      total: total ?? rows.length,
      generatedAt: new Date().toISOString(),
    };
  }
}
