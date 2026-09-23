/**
 * Finance reports API client.
 *
 * Wraps `GET /fin/reports/<report>` for both JSON (view) and file streaming
 * (xlsx/pdf/docx export). Types mirror the backend `report-types.ts`
 * ReportDocument contract.
 */

import { apiGet, downloadFile } from './client';

// ─── ReportDocument contract (mirrors backend report-types.ts) ────────────────

export type CellAlign = 'left' | 'right' | 'center';

export interface ReportColumn {
  key: string;
  label: string;
  align?: CellAlign;
  type?: 'text' | 'number' | 'date';
  width?: number;
}

export interface ReportRow {
  cells: Record<string, string | number | null>;
  bold?: boolean;
  indent?: number;
}

export interface ReportSection {
  heading?: string;
  rows: ReportRow[];
  subtotal?: ReportRow;
}

export interface ReportDocument {
  key: string;
  title: string;
  subtitle?: string;
  meta: { label: string; value: string }[];
  columns: ReportColumn[];
  sections: ReportSection[];
  grandTotal?: ReportRow;
}

// ─── Report keys + params ─────────────────────────────────────────────────────

export type ReportKey =
  | 'trial-balance'
  | 'movement-balance'
  | 'income-statement'
  | 'balance-sheet'
  | 'equity-changes'
  | 'general-ledger'
  | 'cash-flow'
  | 'daily-cash-bank'
  | 'ar-card'
  | 'ar-aging'
  | 'ap-card'
  | 'ap-aging'
  | 'giro-maturity'
  | 'budget-realization';

export type ExportFormat = 'xlsx' | 'pdf' | 'docx';

/** Query params per report — range reports use from/to, balance-sheet uses asOf. */
export interface ReportParams {
  from?: string;
  to?: string;
  asOf?: string;
  accountId?: string;
}

// ─── API calls ────────────────────────────────────────────────────────────────

interface ReportResponse {
  success: boolean;
  data: ReportDocument;
}

/** Fetch a report as a JSON ReportDocument for on-screen rendering. */
export async function getReport(
  report: ReportKey,
  params: ReportParams,
): Promise<ReportDocument> {
  const res = await apiGet<ReportResponse>(`/fin/reports/${report}`, {
    ...params,
    format: 'json',
  });
  return res.data;
}

/** Default download filename when the server omits Content-Disposition. */
function fallbackFileName(report: ReportKey, format: ExportFormat): string {
  const ext = format === 'docx' ? 'doc' : format;
  return `${report}.${ext}`;
}

/**
 * Trigger a browser download of the report in the given file format.
 * Streams from the same endpoint with `format=xlsx|pdf|docx`.
 */
export async function downloadReport(
  report: ReportKey,
  params: ReportParams,
  format: ExportFormat,
): Promise<void> {
  await downloadFile(
    `/fin/reports/${report}`,
    { ...params, format },
    fallbackFileName(report, format),
  );
}
