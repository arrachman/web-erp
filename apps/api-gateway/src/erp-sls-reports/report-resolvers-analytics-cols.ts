/** Column definitions for Sales analytics reports. */

import { ReportColumn } from './report-types';

export const SUMMARY_COLS: ReportColumn[] = [
  { key: 'period', header: 'Periode', type: 'text' },
  { key: 'count', header: 'Jml Dokumen', type: 'number' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
  { key: 'tax', header: 'PPN', type: 'money' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
];

export const BY_CUSTOMER_COLS: ReportColumn[] = [
  { key: 'customerCode', header: 'Kode Pelanggan', type: 'text' },
  { key: 'customerName', header: 'Nama Pelanggan', type: 'text' },
  { key: 'invoiceCount', header: 'Jml Faktur', type: 'number' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
  { key: 'tax', header: 'PPN', type: 'money' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
];

export const BY_SALESMAN_COLS: ReportColumn[] = [
  { key: 'salesmanCode', header: 'Kode Sales', type: 'text' },
  { key: 'salesmanName', header: 'Nama Sales', type: 'text' },
  { key: 'invoiceCount', header: 'Jml Faktur', type: 'number' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
];

export const BY_ITEM_COLS: ReportColumn[] = [
  { key: 'itemCode', header: 'Kode Item', type: 'text' },
  { key: 'itemName', header: 'Nama Item', type: 'text' },
  { key: 'qty', header: 'Total Qty', type: 'qty' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
];

export const BY_PROJECT_COLS: ReportColumn[] = [
  { key: 'projectCode', header: 'Kode Proyek', type: 'text' },
  { key: 'projectName', header: 'Nama Proyek', type: 'text' },
  { key: 'qty', header: 'Total Qty', type: 'qty' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
];

export const BY_DIVISION_COLS: ReportColumn[] = [
  { key: 'divisionCode', header: 'Kode Divisi', type: 'text' },
  { key: 'divisionName', header: 'Nama Divisi', type: 'text' },
  { key: 'invoiceCount', header: 'Jml Faktur', type: 'number' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
];

export const BY_CC_COLS: ReportColumn[] = [
  { key: 'ccCode', header: 'Kode Cost Center', type: 'text' },
  { key: 'ccName', header: 'Nama Cost Center', type: 'text' },
  { key: 'qty', header: 'Total Qty', type: 'qty' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
];

export const BY_ITEM_CAT_COLS: ReportColumn[] = [
  { key: 'categoryCode', header: 'Kode Kategori', type: 'text' },
  { key: 'categoryName', header: 'Nama Kategori', type: 'text' },
  { key: 'qty', header: 'Total Qty', type: 'qty' },
  { key: 'subtotal', header: 'Subtotal', type: 'money' },
];

export const REV_COLLECTION_COLS: ReportColumn[] = [
  { key: 'period', header: 'Periode', type: 'text' },
  { key: 'invoiced', header: 'Ditagih', type: 'money' },
  { key: 'collected', header: 'Diterima', type: 'money' },
  { key: 'outstanding', header: 'Outstanding', type: 'money' },
];

export const BY_GROUP_COLS: ReportColumn[] = [
  { key: 'groupCode', header: 'Kode Grup', type: 'text' },
  { key: 'groupName', header: 'Nama Grup', type: 'text' },
  { key: 'customerCount', header: 'Jml Pelanggan', type: 'number' },
  { key: 'invoiceCount', header: 'Jml Faktur', type: 'number' },
  { key: 'grandTotal', header: 'Grand Total', type: 'money' },
];
