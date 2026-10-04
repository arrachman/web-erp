// A4 Pajak Pengadaan API — subledger PPN per transaksi, faktur pajak +
// ekspor Coretax, bukti potong PPh 22/23 + rekonsiliasi, laporan bulanan.
// Endpoints: /tax-subledger (controller erp/tax-subledger)

import { apiGet, apiPost, downloadFile } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export type TaxEntryType =
  | 'PPN_KELUARAN'
  | 'PPN_MASUKAN'
  | 'PPH_21'
  | 'PPH_22'
  | 'PPH_23'
  | 'PPH_4_2'
  | 'PPH_25'
  | 'PPH_26'
  | 'OTHER';

export const TAX_ENTRY_TYPE_LABELS: Record<TaxEntryType, string> = {
  PPN_KELUARAN: 'PPN Keluaran',
  PPN_MASUKAN: 'PPN Masukan',
  PPH_21: 'PPh 21',
  PPH_22: 'PPh 22',
  PPH_23: 'PPh 23',
  PPH_4_2: 'PPh 4(2)',
  PPH_25: 'PPh 25',
  PPH_26: 'PPh 26',
  OTHER: 'Lainnya',
};

export interface TaxEntry {
  id: string;
  module: string;
  sourceDocType: string | null;
  sourceId: string | null;
  docNumber: string;
  transactionDate: string;
  partnerId: string | null;
  partnerNpwp: string | null;
  partnerName: string | null;
  taxId: string;
  taxEntryType: TaxEntryType;
  dpp: string;
  taxRate: string;
  taxAmount: string;
  fakturNumber: string | null;
  fakturDate: string | null;
  status: 'DRAFT' | 'CONFIRMED' | 'REPORTED' | 'CANCELLED';
  tax?: { id: string; code: string; name: string } | null;
}

export interface WithholdingRow extends TaxEntry {
  certifiedTotal: string;
  diff: string;
  reconcileStatus: 'LENGKAP' | 'BELUM_ADA_BUKTI' | 'KURANG' | 'LEBIH';
  certificates?: WhtCertificate[];
}

export interface WhtCertificate {
  id: string;
  certNumber: string;
  pphType: TaxEntryType;
  transactionDate: string;
  partnerId: string;
  partnerName: string;
  dpp: string;
  rate: string;
  amountWithheld: string;
  status: 'ISSUED' | 'CANCELLED';
  taxEntryId: string | null;
  notes: string | null;
}

export interface MonthlyReport {
  year: number;
  month: number;
  ppnKeluaran: { count: number; dpp: number; tax: number };
  ppnMasukan: { count: number; dpp: number; tax: number };
  ppnNetto: number;
  pph22: { count: number; dpp: number; tax: number; certified: number };
  pph23: { count: number; dpp: number; tax: number; certified: number };
  reportedCount: number;
  draftCount: number;
}

export interface ListTaxEntriesParams {
  page?: number;
  limit?: number;
  search?: string;
  taxEntryType?: TaxEntryType;
  status?: string;
  year?: number;
  month?: number;
}

export function listTaxEntries(params?: ListTaxEntriesParams): Promise<PaginatedResponse<TaxEntry>> {
  return apiGet<PaginatedResponse<TaxEntry>>(
    '/tax-subledger/entries',
    params as unknown as Record<string, string | number | boolean | undefined>,
  );
}

export async function syncTaxSubledger(): Promise<{ created: number; skipped: number }> {
  const res = await apiPost<ApiResponse<{ created: number; skipped: number }>>('/tax-subledger/sync', {});
  return res.data;
}

export async function createTaxEntry(body: Record<string, unknown>): Promise<TaxEntry> {
  const res = await apiPost<ApiResponse<TaxEntry>>('/tax-subledger/entries', body);
  return res.data;
}

export async function assignFaktur(id: string, fakturNumber: string, fakturDate?: string): Promise<void> {
  await apiPost(`/tax-subledger/entries/${id}/faktur`, { fakturNumber, fakturDate });
}

export async function setTaxEntryStatus(id: string, status: string): Promise<void> {
  await apiPost(`/tax-subledger/entries/${id}/status`, { status });
}

export async function getWithholding(): Promise<WithholdingRow[]> {
  const res = await apiGet<ApiResponse<WithholdingRow[]>>('/tax-subledger/withholding');
  return res.data;
}

export async function listCertificates(): Promise<WhtCertificate[]> {
  const res = await apiGet<ApiResponse<WhtCertificate[]>>('/tax-subledger/certificates');
  return res.data;
}

export async function createCertificate(body: Record<string, unknown>): Promise<WhtCertificate> {
  const res = await apiPost<ApiResponse<WhtCertificate>>('/tax-subledger/certificates', body);
  return res.data;
}

export async function cancelCertificate(id: string): Promise<void> {
  await apiPost(`/tax-subledger/certificates/${id}/cancel`, {});
}

export async function getMonthlyReport(year: number, month: number): Promise<MonthlyReport> {
  const res = await apiGet<ApiResponse<MonthlyReport>>('/tax-subledger/monthly-report', { year, month });
  return res.data;
}

export async function downloadCoretax(year: number, month: number): Promise<void> {
  await downloadFile(
    '/tax-subledger/coretax-export',
    { year, month },
    `coretax-ppn-keluaran-${year}-${String(month).padStart(2, '0')}.csv`,
  );
}

export async function downloadMonthlyReport(year: number, month: number): Promise<void> {
  await downloadFile(
    '/tax-subledger/monthly-report/export',
    { year, month },
    `laporan-pajak-${year}-${String(month).padStart(2, '0')}.csv`,
  );
}
