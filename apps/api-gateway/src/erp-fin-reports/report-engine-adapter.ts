/**
 * Adapts a finance {@link ReportDocument} into the report-engine's bind model:
 *  - columns → {@link TableColumnDef} (drives auto-materialized band layout),
 *  - sections/rows/subtotals/grandTotal → a flat row list bound to the data band,
 *  - title/subtitle/meta → report-level context for header/footer bands.
 *
 * Section headings, subtotals and the grand total are flagged `__bold` so a template
 * can emphasise them; heading text is placed in the first column's cell.
 */

import type { RenderContext } from '../erp-report-engine/engine-types';
import type { ColumnType, TableColumnDef } from '../erp-report-engine/template-builder';
import type { ReportColumn, ReportDocument } from './report-types';

function mapType(type: ReportColumn['type']): ColumnType {
  if (type === 'number') return 'money';
  if (type === 'date') return 'date';
  return 'text';
}

/** Finance report columns → engine column defs. */
export function finReportColumns(doc: ReportDocument): TableColumnDef[] {
  return doc.columns.map((c) => ({
    key: c.key,
    label: c.label,
    type: mapType(c.type),
    align: c.align,
    width: c.width,
  }));
}

/** Finance report → engine render context (report-level data + flat rows). */
export function finReportContext(doc: ReportDocument): RenderContext {
  const firstKey = doc.columns[0]?.key ?? 'col0';
  const metaText = doc.meta.map((m) => `${m.label}: ${m.value}`).join('\n');
  const rows: Record<string, unknown>[] = [];

  for (const section of doc.sections) {
    if (section.heading) {
      rows.push({ [firstKey]: section.heading, __bold: 1, __heading: 1 });
    }
    for (const r of section.rows) {
      rows.push({ ...r.cells, __bold: r.bold ? 1 : 0 });
    }
    if (section.subtotal) {
      rows.push({ ...section.subtotal.cells, __bold: 1, __subtotal: 1 });
    }
  }
  if (doc.grandTotal) {
    rows.push({ ...doc.grandTotal.cells, __bold: 1, __grandTotal: 1 });
  }

  return {
    report: {
      title: doc.title,
      subtitle: doc.subtitle ?? '',
      metaText,
      meta: doc.meta,
    },
    rows,
    company: {},
    now: new Date(),
  };
}
