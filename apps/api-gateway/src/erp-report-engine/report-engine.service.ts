/**
 * ReportEngineService — resolves the active template bound to a report and renders
 * it (+ {@link RenderContext}) to a PDF Buffer via @react-pdf/renderer. Injected into
 * the per-module export services (fin/sls/pur/inv) to back template-driven PDF output.
 *
 * Templates may be "auto" (presentation-only, no bands) — in that case the band
 * layout is materialized from the report's live columns at render time. Explicit
 * band templates (authored in the Report Designer) are rendered as-is.
 */

import { Injectable, Logger } from '@nestjs/common';
import { renderToBuffer } from '@react-pdf/renderer';
import type { ReactElement } from 'react';
import { PrismaService } from '../prisma/prisma.service';
import type {
  Margins,
  Orientation,
  PageSize,
  ReportTemplate,
  RenderContext,
} from './engine-types';
import { buildReportPdf } from './pdf-document';
import { buildTableTemplate, type TableColumnDef } from './template-builder';

/** Loose shape of the stored `templateJson` (auto or explicit). */
interface StoredTemplate {
  auto?: boolean;
  name?: string;
  pageSize?: PageSize;
  orientation?: Orientation;
  margins?: Margins;
  bands?: unknown[];
  [k: string]: unknown;
}

@Injectable()
export class ReportEngineService {
  private readonly logger = new Logger(ReportEngineService.name);

  constructor(private readonly prisma: PrismaService) {}

  async renderPdf(template: ReportTemplate, ctx: RenderContext): Promise<Buffer> {
    const element = buildReportPdf(template, ctx) as ReactElement;
    // @react-pdf types renderToBuffer against its own DocumentElement; the tree is
    // built with createElement so we hand it the element directly.
    return renderToBuffer(element as never);
  }

  /** Render but never throw — caller falls back to the legacy renderer. */
  async tryRenderPdf(template: ReportTemplate, ctx: RenderContext): Promise<Buffer | null> {
    try {
      return await this.renderPdf(template, ctx);
    } catch (err) {
      this.logger.error(
        `Template render failed (${template.name ?? template.id ?? 'unknown'}): ${
          err instanceof Error ? err.message : String(err)
        }`,
      );
      return null;
    }
  }

  /** The active template bound to a report key, or null if none. */
  async resolveActiveTemplate(reportKey: string): Promise<StoredTemplate | null> {
    const rec = await this.prisma.erpRptTemplate.findFirst({
      where: { reportKey, isActive: true, deletedAt: null },
      orderBy: { updatedAt: 'desc' },
      select: { templateJson: true },
    });
    if (!rec) return null;
    const json = rec.templateJson as unknown;
    return json && typeof json === 'object' ? (json as StoredTemplate) : null;
  }

  /** Turn a stored template into a renderable one — materializing bands from the
   * report's columns when the stored template has none (auto mode). */
  private materialize(stored: StoredTemplate, columns: TableColumnDef[]): ReportTemplate {
    if (Array.isArray(stored.bands) && stored.bands.length > 0) {
      return stored as unknown as ReportTemplate;
    }
    return buildTableTemplate({
      name: stored.name ?? 'Report',
      module: 'fin',
      columns,
      pageSize: stored.pageSize,
      orientation: stored.orientation,
      margins: stored.margins,
    });
  }

  /**
   * Resolve + render a report's bound template to PDF. Returns null when no active
   * template is bound or rendering fails — the caller then uses the legacy renderer.
   */
  async renderReport(
    reportKey: string,
    columns: TableColumnDef[],
    ctx: RenderContext,
  ): Promise<Buffer | null> {
    // Exact per-report template first, then the module-wide default (`<module>.__default`).
    let stored = await this.resolveActiveTemplate(reportKey);
    if (!stored) {
      const module = reportKey.split('.')[0];
      stored = await this.resolveActiveTemplate(`${module}.__default`);
    }
    if (!stored) return null;
    return this.tryRenderPdf(this.materialize(stored, columns), ctx);
  }

  /**
   * Render a raw stored-template object (auto or explicit) with the given columns +
   * context — used by the designer's "Preview PDF". Returns null on bad input/failure.
   */
  async renderStoredTemplate(
    stored: unknown,
    columns: TableColumnDef[],
    ctx: RenderContext,
  ): Promise<Buffer | null> {
    if (!stored || typeof stored !== 'object') return null;
    return this.tryRenderPdf(this.materialize(stored as StoredTemplate, columns), ctx);
  }
}
