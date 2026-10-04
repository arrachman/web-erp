import { apiGet, downloadFile } from './client';

export type ReportExportFormat = 'xlsx' | 'pdf' | 'docx';

export interface ReportColumn {
  key: string;
  header: string;
  type: 'text' | 'number' | 'money' | 'qty' | 'percent' | 'date' | 'status';
  align?: 'left' | 'right' | 'center';
}

export interface ReportSummaryItem {
  label: string;
  value: string | number;
  type?: ReportColumn['type'];
}

export interface ReportChart {
  kind: 'bar' | 'donut';
  title: string;
  labels: string[];
  values: number[];
}

export interface ReportDataset {
  key: string;
  title: string;
  columns: ReportColumn[];
  rows: Record<string, unknown>[];
  summary: ReportSummaryItem[];
  charts?: ReportChart[];
  filters: Record<string, unknown>;
  generatedAt: string;
  total: number;
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

export interface ReportCatalogItem {
  key: string;
  title: string;
  group: 'transaction';
}

function toQuery(filters: ReportFilters): Record<string, string | number | undefined> {
  return {
    dateFrom: filters.dateFrom,
    dateTo: filters.dateTo,
    partnerId: filters.partnerId,
    status: filters.status,
    search: filters.search,
    page: filters.page,
    limit: filters.limit,
  };
}

export function getReportData(key: string, filters: ReportFilters): Promise<ReportDataset> {
  return apiGet<ReportDataset>(`/fin/doc-reports/${key}`, toQuery(filters));
}

export function getDocumentReportCatalog(): Promise<ReportCatalogItem[]> {
  return apiGet<ReportCatalogItem[]>('/fin/doc-reports');
}

export function downloadReport(
  key: string,
  format: ReportExportFormat,
  filters: ReportFilters,
): Promise<void> {
  return downloadFile(
    `/fin/doc-reports/${key}/export`,
    { ...toQuery(filters), format },
    `${key}.${format}`,
  );
}
