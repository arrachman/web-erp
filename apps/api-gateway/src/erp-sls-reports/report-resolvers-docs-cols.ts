/**
 * Shared column definitions + helpers for Sales document-list resolvers.
 */

import { ReportColumn } from './report-types';

export const DOC_COLS_MONEY: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'customer', header: 'Pelanggan', type: 'text' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const DOC_COLS_SIMPLE: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'customer', header: 'Pelanggan', type: 'text' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const DOC_COLS_ADVANCE: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'customer', header: 'Pelanggan', type: 'text' },
  { key: 'amount', header: 'Jumlah', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const DOC_COLS_AR: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'partner', header: 'Partner', type: 'text' },
  { key: 'amount', header: 'Jumlah', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const DOC_COLS_INVOICE: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'customer', header: 'Pelanggan', type: 'text' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
  { key: 'tax1Amount', header: 'PPN', type: 'money' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const DOC_COLS_FREIGHT: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'customer', header: 'Pelanggan', type: 'text' },
  { key: 'otherCostAmount', header: 'Biaya Lain', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

export const DOC_COLS_GRAND: ReportColumn[] = [
  { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
  { key: 'docDate', header: 'Tanggal', type: 'date' },
  { key: 'customer', header: 'Pelanggan', type: 'text' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
  { key: 'status', header: 'Status', type: 'status' },
];

export function sumMoney(rows: Record<string, unknown>[], key: string): number {
  return rows.reduce((s, r) => s + (typeof r[key] === 'number' ? (r[key] as number) : 0), 0);
}
