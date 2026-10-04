/** Uniform contract for Finance document reports. */

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
  align?: 'left' | 'right' | 'center';
}

export interface ReportSummaryItem {
  label: string;
  value: string | number;
  type?: ReportColType;
}

export interface ReportChart {
  kind: 'bar' | 'donut';
  title: string;
  labels: string[];
  values: number[];
}

export interface ReportFilters {
  dateFrom?: string;
  dateTo?: string;
  asOfDate?: string;
  partnerId?: string;
  status?: string;
  search?: string;
  page?: number;
  limit?: number;
}

export interface ReportDataset {
  key: string;
  title: string;
  columns: ReportColumn[];
  rows: Record<string, unknown>[];
  summary: ReportSummaryItem[];
  charts?: ReportChart[];
  filters: ReportFilters;
  generatedAt: string;
  total: number;
}

export type ReportGroup = 'transaction';
export type ReportFormat = 'xlsx' | 'pdf' | 'docx';

export interface ReportCatalogItem {
  key: string;
  title: string;
  group: ReportGroup;
}

export interface ReportDef {
  key: string;
  title: string;
  group: ReportGroup;
  columns: ReportColumn[];
  resolve: (filters: ReportFilters) => Promise<{
    rows: Record<string, unknown>[];
    summary?: ReportSummaryItem[];
    total?: number;
    charts?: ReportChart[];
  }>;
}
