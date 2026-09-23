import type { RsTplKey } from './types';

export interface RsT {
  reportName: string; dragHint: string;
  data: string; relations: string; params: string; funcs: string;
  export: string; clipboard: string; paste: string; cut: string; copy: string; delete: string;
  font: string; alignment: string; borders: string; textFormat: string; style: string; styleHint: string;
  pageSize: string; margins: string; pageSetup: string; orientation: string; portrait: string; landscape: string; show: string;
  grid: string; snapGrid: string; showGrid: string; gridSize: string; spacing: string; arrange: string;
  zoom: string; fit: string; panels: string;
  properties: string; reportTree: string; dictionary: string; selectComp: string;
  groupBy: string; height: string;
  noSel: string; relHint: string;
}

export function tr(id: boolean): RsT {
  return {
    reportName: id ? 'Nama laporan' : 'Report name', dragHint: id ? 'Seret field ke kanvas' : 'Drag fields to canvas',
    data: 'Data', relations: id ? 'Relasi' : 'Relations', params: id ? 'Parameter' : 'Parameters', funcs: id ? 'Fungsi' : 'Functions',
    export: id ? 'Ekspor' : 'Export', clipboard: id ? 'Papan Klip' : 'Clipboard', paste: id ? 'Tempel' : 'Paste', cut: id ? 'Potong' : 'Cut', copy: id ? 'Salin' : 'Copy', delete: id ? 'Hapus' : 'Delete',
    font: id ? 'Font' : 'Font', alignment: id ? 'Perataan' : 'Alignment', borders: id ? 'Garis' : 'Borders', textFormat: id ? 'Format Teks' : 'Text Format', style: 'Style', styleHint: id ? 'Terapkan gaya' : 'Apply preset',
    pageSize: id ? 'Ukuran' : 'Size', margins: id ? 'Margin' : 'Margins', pageSetup: id ? 'Atur Halaman' : 'Page Setup', orientation: id ? 'Orientasi' : 'Orientation', portrait: id ? 'Potret' : 'Portrait', landscape: id ? 'Lanskap' : 'Landscape', show: id ? 'Tampilan' : 'Show',
    grid: id ? 'Grid' : 'Grid', snapGrid: id ? 'Rekat ke Grid' : 'Snap to Grid', showGrid: id ? 'Tampilkan Grid' : 'Show Grid', gridSize: id ? 'Ukuran' : 'Size', spacing: id ? 'Jarak' : 'Spacing', arrange: id ? 'Susun' : 'Arrange',
    zoom: 'Zoom', fit: id ? 'Muat' : 'Fit', panels: id ? 'Panel' : 'Panels',
    properties: id ? 'Properti' : 'Properties', reportTree: id ? 'Pohon Laporan' : 'Report Tree', dictionary: id ? 'Kamus' : 'Dictionary', selectComp: id ? '(pilih komponen)' : '(select component)',
    groupBy: id ? 'Kelompokkan (Group by)' : 'Group by', height: id ? 'Tinggi' : 'Height',
    noSel: id ? 'Pilih elemen pada kanvas, atau pilih komponen di atas, untuk mengubah propertinya.' : 'Select an element on the canvas, or pick a component above, to edit its properties.',
    relHint: id ? 'Atur relasi antar tabel. Klik untuk mengubah INNER/LEFT.' : 'Configure table relations. Click to toggle INNER/LEFT.',
  };
}

export function templateOptions(id: boolean): Array<{ v: string; label: string }> {
  return [
    ['invoice', id ? 'Invoice' : 'Invoice'],
    ['sales', id ? 'Laporan Penjualan' : 'Sales Report'],
    ['purchasing', id ? 'Laporan Pembelian' : 'Purchasing Report'],
    ['finance', id ? 'Laba Rugi' : 'Profit & Loss'],
    ['customers', id ? 'Daftar Pelanggan' : 'Customer List'],
  ].map((o) => ({ v: o[0], label: o[1] }));
}

export function defName(tpl: RsTplKey, id: boolean): string {
  const m: Record<RsTplKey, string> = {
    invoice: id ? 'Invoice Penjualan' : 'Sales Invoice',
    sales: id ? 'Laporan Penjualan Bulanan' : 'Monthly Sales Report',
    purchasing: id ? 'Laporan Pembelian' : 'Purchasing Report',
    finance: id ? 'Laporan Laba Rugi' : 'Profit & Loss Statement',
    customers: id ? 'Daftar Pelanggan' : 'Customer List',
  };
  return m[tpl];
}

export const DATASOURCE_NAME: Record<RsTplKey, string> = {
  invoice: 'SalesDB · InvoiceLines',
  sales: 'SalesDB · Invoices',
  purchasing: 'PurchasingDB · POLines',
  finance: 'FinanceDB · GLEntries',
  customers: 'SalesDB · Customers',
};

/** Group-by option binds available per template. */
export const GROUP_OPTIONS: Record<RsTplKey, string[]> = {
  sales: ['', 'Customers.Name', 'Products.Name', 'Invoices.Date'],
  purchasing: ['', 'Vendors.Name', 'Products.Name'],
  customers: ['', 'Customers.City'],
  invoice: [''],
  finance: [''],
};

export const FMT_SAMPLES: Record<string, string> = {
  General: 'Abc 123', Number: '1.234,56', Currency: 'Rp 1.234.567',
  Date: '17 Jun 2026', Time: '14:30', Percentage: '11,0%',
};

export function fmtLabel(f: string, id: boolean): string {
  return ({
    General: id ? 'Umum' : 'General', Number: id ? 'Angka' : 'Number',
    Currency: id ? 'Mata Uang' : 'Currency', Date: id ? 'Tanggal' : 'Date',
    Time: id ? 'Waktu' : 'Time', Percentage: id ? 'Persen' : 'Percentage',
  } as Record<string, string>)[f] || f;
}

export function bandLabel(type: string, id: boolean): string {
  const M: Record<string, [string, string]> = {
    ReportHeader: ['Judul Laporan', 'Report Header'], PageHeader: ['Header Halaman', 'Page Header'],
    ColumnHeader: ['Header Kolom', 'Column Header'], Detail: ['Detail', 'Detail'],
    GroupHeader: ['Header Grup', 'Group Header'], GroupFooter: ['Footer Grup', 'Group Footer'],
    ColumnFooter: ['Footer Kolom', 'Column Footer'], PageFooter: ['Footer Halaman', 'Page Footer'],
    ReportFooter: ['Footer Laporan', 'Report Footer'],
  };
  const x = M[type] || [type, type];
  return id ? x[0] : x[1];
}
