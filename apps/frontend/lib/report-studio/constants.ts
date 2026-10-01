import type { RsTplKey } from './types';

/** Default Group-by binding per template. */
export const GROUP_DEF: Partial<Record<RsTplKey, string>> = {
  sales: 'Customers.Name',
  purchasing: 'Vendors.Name',
  customers: 'Customers.City',
};

/** Style presets applied from the ribbon "Style" dropdown. */
export interface RsStylePreset { size: number; bold: boolean; color: string; bg?: string; }
export const STYLES: Record<string, RsStylePreset> = {
  Title: { size: 20, bold: true, color: '#14181f' },
  Subtitle: { size: 13, bold: true, color: '#6b7280' },
  Heading: { size: 11, bold: true, color: '#374151' },
  Header: { size: 10, bold: true, color: '#ffffff', bg: '#1f2937' },
  Total: { size: 12, bold: true, color: '#14181f' },
  Muted: { size: 9, bold: false, color: '#6b7280' },
  Body: { size: 10, bold: false, color: '#14181f' },
};

export interface RsSchemaTable { name: string; fields: Array<[string, string]>; }
export interface RsSchemaDb { name: string; tables: RsSchemaTable[]; }

export const SCHEMA: RsSchemaDb[] = [
  { name: 'SalesDB', tables: [
    { name: 'Customers', fields: [['CustomerID', '#'], ['Name', 'T'], ['City', 'T'], ['Country', 'T'], ['Email', 'T'], ['Phone', 'T']] },
    { name: 'Invoices', fields: [['InvoiceID', '#'], ['InvoiceNo', 'T'], ['Date', 'D'], ['DueDate', 'D'], ['Status', 'T'], ['Subtotal', '$'], ['Tax', '$'], ['Total', '$']] },
    { name: 'InvoiceLines', fields: [['LineID', '#'], ['Description', 'T'], ['Qty', '#'], ['Price', '$'], ['Discount', '$'], ['Amount', '$']] },
    { name: 'Products', fields: [['ProductID', '#'], ['SKU', 'T'], ['Name', 'T'], ['Category', 'T'], ['Unit', 'T'], ['Price', '$']] },
  ] },
  { name: 'PurchasingDB', tables: [
    { name: 'Vendors', fields: [['VendorID', '#'], ['Name', 'T'], ['City', 'T'], ['Term', 'T']] },
    { name: 'PurchaseOrders', fields: [['POID', '#'], ['PONo', 'T'], ['Date', 'D'], ['Status', 'T'], ['Total', '$']] },
    { name: 'POLines', fields: [['POLineID', '#'], ['Description', 'T'], ['Qty', '#'], ['Cost', '$'], ['Amount', '$']] },
  ] },
  { name: 'FinanceDB', tables: [
    { name: 'Accounts', fields: [['AccountID', '#'], ['Code', 'T'], ['Name', 'T'], ['Type', 'T']] },
    { name: 'GLEntries', fields: [['EntryID', '#'], ['Date', 'D'], ['Debit', '$'], ['Credit', '$'], ['Amount', '$'], ['Memo', 'T']] },
  ] },
];

export interface RsRelation { id: string; left: string; right: string; opt: boolean; }
export const RELATIONS: RsRelation[] = [
  { id: 'r1', left: 'Invoices.CustomerID', right: 'Customers.CustomerID', opt: false },
  { id: 'r2', left: 'InvoiceLines.InvoiceID', right: 'Invoices.InvoiceID', opt: false },
  { id: 'r3', left: 'InvoiceLines.ProductID', right: 'Products.ProductID', opt: true },
  { id: 'r4', left: 'PurchaseOrders.VendorID', right: 'Vendors.VendorID', opt: false },
  { id: 'r5', left: 'POLines.POID', right: 'PurchaseOrders.POID', opt: false },
  { id: 'r6', left: 'GLEntries.AccountID', right: 'Accounts.AccountID', opt: false },
];

export interface RsParam { name: string; val: string; }
export const PARAMS: RsParam[] = [
  { name: '@TanggalDari', val: '01 Jun 2026' },
  { name: '@TanggalSampai', val: '30 Jun 2026' },
  { name: '@Cabang', val: 'Jakarta' },
  { name: '@MataUang', val: 'IDR' },
  { name: '@Pelanggan', val: 'Semua' },
];

export const SWATCHES: string[] = [
  '#14181f', '#374151', '#6b7280', '#9aa3b2', '#2563eb', '#1d4ed8',
  '#dc2626', '#ea580c', '#16a34a', '#0891b2', '#7c3aed', '#ffffff',
];

/** Page dimensions in px (portrait) keyed by size. */
export const PAGE_DIMS: Record<string, [number, number]> = {
  a4: [794, 1123], letter: [816, 1056], legal: [816, 1344],
};
export const MARGINS: Record<string, number> = { normal: 40, narrow: 18, wide: 64 };

export const PX_PER_CM = 37.7953;
