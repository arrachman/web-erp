/**
 * Evaluation scope for the Stimulsoft expression evaluator (sti-expr).
 * The layout engine builds one scope per band instance: current row per
 * dataset, aggregate row-sets per band name, system variables, company (c)
 * and report (r) context objects.
 */

import type { DataRow } from './engine-types-v2';
import type { ReportFormatSettings } from './report-format';

export interface DatasetScope {
  /** Current row for {DS.field} resolution (undefined = no current row). */
  row?: DataRow;
  /** All rows of the dataset in the current render (aggregate fallback). */
  rows: DataRow[];
}

export interface AggRow {
  row: DataRow;
  line: number;
}

export interface AggScopeEntry {
  dataset: string;
  rows: AggRow[];
}

export interface StiScope {
  datasets: Record<string, DatasetScope>;
  /** Band name → rows visible to aggregates (Sum/SumIf/CountIf…) in this scope. */
  bands: Record<string, AggScopeEntry>;
  primaryDataset?: string;
  c?: Record<string, unknown>;
  r?: Record<string, unknown>;
  params?: Record<string, unknown>;
  vars: {
    PageNumber: number | string;
    TotalPageCount: number | string;
    Line: number;
    Time: Date;
    Today: Date;
  };
  format: ReportFormatSettings;
  /** Stateful running-sum hook maintained by the layout engine. */
  sumRunning?: (dataset: string | undefined, field: string) => number;
}

/** Per-row override context used while evaluating aggregate arguments. */
export interface RowContext {
  rows: Map<string, DataRow>;
  line?: number;
}

/** Case-insensitive property read (legacy templates mix DS1/ds1, .Fromat/.format). */
export function ciGet(obj: Record<string, unknown> | undefined, key: string): unknown {
  if (!obj) return undefined;
  if (key in obj) return obj[key];
  const lower = key.toLowerCase();
  for (const k of Object.keys(obj)) {
    if (k.toLowerCase() === lower) return obj[k];
  }
  return undefined;
}

export function ciDataset(scope: StiScope, name: string): DatasetScope | undefined {
  if (scope.datasets[name]) return scope.datasets[name];
  const lower = name.toLowerCase();
  for (const k of Object.keys(scope.datasets)) {
    if (k.toLowerCase() === lower) return scope.datasets[k];
  }
  return undefined;
}

export function ciBand(scope: StiScope, name: string): AggScopeEntry | undefined {
  if (scope.bands[name]) return scope.bands[name];
  const lower = name.toLowerCase();
  for (const k of Object.keys(scope.bands)) {
    if (k.toLowerCase() === lower) return scope.bands[k];
  }
  return undefined;
}
