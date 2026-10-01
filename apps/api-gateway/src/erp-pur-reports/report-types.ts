/**
 * Uniform report contract for Warehouse (M3) reports. Every report — whether a
 * transaction list or a computed stock aggregation — resolves to the SAME
 * `ReportDataset` shape, so the on-screen table, the Excel/PDF/Word exporters,
 * and the frontend all consume one structure. Adding a report = adding one
 * `ReportDef` to a resolver builder; no per-report plumbing.
 */

/** Semantic cell type → drives alignment + number/date formatting in view & export. */
export type ReportColType =
  | 'text'
  | 'number'
  | 'money'
  | 'qty'
  | 'percent'
  | 'date'
  | 'status';

export interface ReportColumn {
  key: string;
  header: string;
  type: ReportColType;
  /** Override the default alignment derived from `type`. */
  align?: 'left' | 'right' | 'center';
}

/** One aggregate shown in the report header/footer summary bar. */
export interface ReportSummaryItem {
  label: string;
  value: string | number;
  type?: ReportColType;
}

/** Query inputs common to all reports (each report uses the subset it needs). */
export interface ReportFilters {
  dateFrom?: string;
  dateTo?: string;
  asOfDate?: string;
  warehouseId?: string;
  branchId?: string;
  itemId?: string;
  partnerId?: string;
  status?: string;
  search?: string;
  page?: number;
  limit?: number;
}

/** The resolved dataset — fed identically to the table view and the exporters. */
export interface ReportDataset {
  key: string;
  title: string;
  columns: ReportColumn[];
  rows: Record<string, unknown>[];
  summary: ReportSummaryItem[];
  filters: ReportFilters;
  generatedAt: string;
  /** Total matching rows (for paginated reports); equals rows.length otherwise. */
  total: number;
}

/** Catalog grouping for the report list endpoint. */
export type ReportGroup = 'transaction' | 'stock' | 'item';

/** A single report definition contributed by a resolver builder. */
export interface ReportDef {
  key: string;
  title: string;
  group: ReportGroup;
  columns: ReportColumn[];
  resolve: (filters: ReportFilters) => Promise<{
    rows: Record<string, unknown>[];
    summary?: ReportSummaryItem[];
    total?: number;
  }>;
}

export type ReportFormat = 'xlsx' | 'pdf' | 'docx';

/** Lightweight catalog entry returned by GET /erp/pur/reports. */
export interface ReportCatalogItem {
  key: string;
  title: string;
  group: ReportGroup;
}
