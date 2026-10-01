/**
 * Sales (M5) reports API client.
 *
 * Wraps `GET /sls/reports/:key` for both JSON (on-screen view) and file
 * streaming (xlsx/pdf/docx export). Types mirror the backend `ReportDataset`
 * contract. JSON goes through the shared `apiGet` wrapper; downloads reuse the
 * shared `downloadFile` helper (same BASE_URL + `credentials:'include'` cookie
 * auth, Content-Disposition filename, ErpApiError on non-ok).
 */

import { apiGet, downloadFile } from './client';

// ─── ReportDataset contract (mirrors backend) ─────────────────────────────────

export type ReportCellType =
  | 'text'
  | 'number'
  | 'money'
  | 'qty'
  | 'percent'
  | 'date'
  | 'status';

export type ReportCellAlign = 'left' | 'right' | 'center';

export interface ReportColumn {
  key: string;
  header: string;
  type: ReportCellType;
  align?: ReportCellAlign;
}

export interface ReportSummaryItem {
  label: string;
  value: string | number | null;
  type?: ReportCellType;
}

export interface ReportDataset {
  key: string;
  title: string;
  columns: ReportColumn[];
  rows: Record<string, unknown>[];
  summary: ReportSummaryItem[];
  filters: Record<string, unknown>;
  generatedAt: string;
  total: number;
}

export type ReportExportFormat = 'xlsx' | 'pdf' | 'docx';

/** Query filters accepted by both the JSON and export endpoints. */
export interface ReportFilters {
  dateFrom?: string;
  dateTo?: string;
  asOfDate?: string;
  warehouseId?: string;
  itemId?: string;
  partnerId?: string;
  salesmanId?: string;
  status?: string;
  search?: string;
  page?: number;
  limit?: number;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

/** Drop undefined/empty filter values so the query string stays clean. */
function toQuery(
  filters: ReportFilters,
): Record<string, string | number | undefined> {
  const out: Record<string, string | number | undefined> = {};
  for (const [key, value] of Object.entries(filters)) {
    if (value === undefined || value === null || value === '') continue;
    out[key] = value as string | number;
  }
  return out;
}

/** Default download filename when the server omits Content-Disposition. */
function fallbackFileName(key: string, format: ReportExportFormat): string {
  return `${key}.${format}`;
}

// ─── API calls ────────────────────────────────────────────────────────────────

interface ReportResponse {
  success?: boolean;
  data: ReportDataset;
}

/**
 * Fetch a report as a JSON `ReportDataset` for on-screen rendering. The backend
 * may wrap the payload in `{ data }` (ApiResponse) or return it bare — handle
 * both.
 */
export async function getReportData(
  key: string,
  filters: ReportFilters,
): Promise<ReportDataset> {
  const res = await apiGet<ReportResponse | ReportDataset>(
    `/sls/reports/${key}`,
    toQuery(filters),
  );
  if (res && typeof res === 'object' && 'data' in res && (res as ReportResponse).data) {
    return (res as ReportResponse).data;
  }
  return res as ReportDataset;
}

/**
 * Trigger a browser download of the report in the given file format. Streams
 * from `GET /sls/reports/:key/export?format=…` with the same filters, reusing
 * the cookie-authenticated `downloadFile` helper (derives the filename from the
 * Content-Disposition header, falls back to `${key}.${format}`). Throws with the
 * API error message on a non-ok response.
 */
export async function downloadReport(
  key: string,
  format: ReportExportFormat,
  filters: ReportFilters,
): Promise<void> {
  await downloadFile(
    `/sls/reports/${key}/export`,
    { ...toQuery(filters), format },
    fallbackFileName(key, format),
  );
}
