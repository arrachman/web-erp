/**
 * Adapts the `ReportDataset` shape shared by Sales / Purchasing / Inventory reports
 * into the report-engine bind model. Unlike finance `ReportDocument`, a ReportDataset
 * is already flat (rows are plain records) — no sections to flatten.
 *
 * Defined structurally so each module can pass its own `ReportDataset` without a type
 * import cycle.
 */

import type { RenderContext } from './engine-types';
import type { ColumnType, TableColumnDef } from './template-builder';

export interface EngineDatasetColumn {
  key: string;
  header: string;
  type?: string;
  align?: 'left' | 'right' | 'center';
}

export interface EngineDataset {
  key: string;
  title: string;
  columns: EngineDatasetColumn[];
  rows: Record<string, unknown>[];
  summary?: { label: string; value: string | number }[];
  generatedAt?: string;
}

function mapType(type: string | undefined): ColumnType {
  switch (type) {
    case 'money':
      return 'money';
    case 'qty':
    case 'number':
      return 'number';
    case 'percent':
      return 'percent';
    case 'date':
      return 'date';
    default:
      return 'text';
  }
}

/** Dataset-style columns ({key, header, type, align}) → engine column defs. */
export function columnsToDefs(columns: EngineDatasetColumn[]): TableColumnDef[] {
  return columns.map((c) => ({
    key: c.key,
    label: c.header,
    type: mapType(c.type),
    align: c.align,
  }));
}

/** Dataset columns → engine column defs (drives auto-materialized band layout). */
export function datasetColumns(dataset: EngineDataset): TableColumnDef[] {
  return columnsToDefs(dataset.columns);
}

/** Dataset → engine render context (report-level data + flat rows). */
export function datasetContext(dataset: EngineDataset): RenderContext {
  const metaText = (dataset.summary ?? [])
    .map((s) => `${s.label}: ${s.value}`)
    .join('     ');
  return {
    report: {
      title: dataset.title,
      subtitle: '',
      metaText,
      summary: dataset.summary ?? [],
    },
    rows: dataset.rows,
    company: {},
    now: dataset.generatedAt ? new Date(dataset.generatedAt) : new Date(),
  };
}
