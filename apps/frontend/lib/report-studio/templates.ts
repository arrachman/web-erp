import type { RsElement, RsElKind, RsBand, RsBandType, RsReport, RsTplKey } from './types';

type ElOpt = Partial<Omit<RsElement, 'id' | 'kind' | 'x' | 'y' | 'w' | 'h'>>;
type BandOpt = Partial<Pick<RsBand, 'bg' | 'canGrow' | 'canShrink' | 'printAll'>>;

export interface RsFactories {
  E: (kind: RsElKind, x: number, y: number, w: number, h: number, o?: ElOpt) => RsElement;
  B: (type: RsBandType, h: number, els?: RsElement[], o?: BandOpt) => RsBand;
}

/** Element / Band factories backed by a shared uid generator. */
export function createFactories(nextId: () => number): RsFactories {
  const E: RsFactories['E'] = (kind, x, y, w, h, o = {}) => ({
    id: 'e' + nextId(), kind, x, y, w, h,
    text: o.text || '', bind: o.bind || '', size: o.size || 10,
    bold: !!o.bold, italic: !!o.italic, underline: !!o.underline, strike: !!o.strike,
    align: o.align || 'left', valign: o.valign || 'middle',
    color: o.color || '', bg: o.bg || '', mono: !!o.mono, font: o.font || '', format: o.format || 'General',
    bTop: !!o.bTop, bBottom: !!o.bBottom, bLeft: !!o.bLeft, bRight: !!o.bRight,
    bColor: o.bColor || '#1f2937', bWidth: o.bWidth || 1,
    canGrow: o.canGrow !== false, canShrink: !!o.canShrink, wordWrap: !!o.wordWrap, enabled: o.enabled !== false,
  });
  const B: RsFactories['B'] = (type, h, els, o = {}) => ({
    id: 'b' + nextId(), type, h, els: els || [], bg: o.bg || '',
    canGrow: o.canGrow !== false, canShrink: !!o.canShrink, printAll: o.printAll !== false,
  });
  return { E, B };
}

export function buildReport(key: RsTplKey, nextId: () => number): RsReport {
  const { E, B } = createFactories(nextId);
  const pageFooter = (): RsBand => B('PageFooter', 22, [
    E('label', 40, 5, 260, 14, { text: 'Dicetak 17 Jun 2026 14:30 — ReportStudio', size: 8, color: '#9aa3b2' }),
    E('label', 600, 5, 80, 14, { text: 'Halaman', size: 8, align: 'right', color: '#9aa3b2' }),
    E('expr', 686, 5, 22, 14, { bind: 'PageNumber()', size: 8, align: 'center', color: '#9aa3b2' }),
    E('label', 708, 5, 10, 14, { text: '/', size: 8, align: 'center', color: '#9aa3b2' }),
    E('expr', 718, 5, 22, 14, { bind: 'TotalPages()', size: 8, align: 'center', color: '#9aa3b2' }),
  ]);

  if (key === 'invoice') return { bands: [
    B('ReportHeader', 152, [
      E('label', 40, 18, 320, 28, { text: 'ACME CORPORATION', size: 20, bold: true }),
      E('label', 40, 49, 320, 14, { text: 'Jl. Jend. Sudirman Kav. 1, Jakarta 10220', size: 9, color: '#6b7280' }),
      E('label', 40, 64, 320, 14, { text: 'NPWP 01.234.567.8-901.000', size: 9, color: '#6b7280' }),
      E('label', 494, 14, 260, 34, { text: 'INVOICE', size: 30, bold: true, align: 'right' }),
      E('label', 470, 92, 128, 13, { text: 'No. Invoice', size: 9, align: 'right', color: '#6b7280' }),
      E('field', 600, 89, 154, 16, { bind: 'Invoices.InvoiceNo', size: 11, bold: true, align: 'right' }),
      E('label', 470, 110, 128, 13, { text: 'Tanggal', size: 9, align: 'right', color: '#6b7280' }),
      E('field', 600, 108, 154, 15, { bind: 'Invoices.Date', size: 10, align: 'right' }),
      E('label', 470, 127, 128, 13, { text: 'Jatuh Tempo', size: 9, align: 'right', color: '#6b7280' }),
      E('field', 600, 125, 154, 15, { bind: 'Invoices.DueDate', size: 10, align: 'right' }),
      E('label', 40, 92, 200, 13, { text: 'TAGIHAN KEPADA', size: 9, bold: true, color: '#6b7280' }),
      E('field', 40, 108, 330, 18, { bind: 'Customers.Name', size: 13, bold: true }),
      E('field', 40, 128, 330, 15, { bind: 'Customers.City', size: 10, color: '#374151' }),
    ]),
    B('ColumnHeader', 26, [
      E('label', 40, 5, 320, 16, { text: 'Deskripsi', size: 10, bold: true, color: '#ffffff' }),
      E('label', 360, 5, 80, 16, { text: 'Qty', size: 10, bold: true, align: 'right', color: '#ffffff' }),
      E('label', 452, 5, 148, 16, { text: 'Harga', size: 10, bold: true, align: 'right', color: '#ffffff' }),
      E('label', 606, 5, 148, 16, { text: 'Jumlah', size: 10, bold: true, align: 'right', color: '#ffffff' }),
    ], { bg: '#1f2937' }),
    B('Detail', 24, [
      E('field', 40, 4, 320, 16, { bind: 'Products.Name', size: 10 }),
      E('field', 360, 4, 80, 16, { bind: 'InvoiceLines.Qty', size: 10, align: 'right' }),
      E('field', 452, 4, 148, 16, { bind: 'InvoiceLines.Price', size: 10, align: 'right' }),
      E('field', 606, 4, 148, 16, { bind: 'InvoiceLines.Amount', size: 10, align: 'right' }),
    ]),
    B('ReportFooter', 150, [
      E('label', 452, 12, 148, 16, { text: 'Subtotal', size: 10, align: 'right', color: '#6b7280' }),
      E('expr', 606, 12, 148, 16, { bind: 'Sum(InvoiceLines.Amount)', size: 10, align: 'right' }),
      E('label', 452, 32, 148, 16, { text: 'PPN 11%', size: 10, align: 'right', color: '#6b7280' }),
      E('expr', 606, 32, 148, 16, { bind: 'Sum(InvoiceLines.Amount)*0.11', size: 10, align: 'right' }),
      E('line', 452, 55, 302, 2, { bg: '#1f2937' }),
      E('label', 452, 62, 148, 18, { text: 'TOTAL', size: 12, bold: true, align: 'right' }),
      E('expr', 606, 61, 148, 18, { bind: 'Sum(InvoiceLines.Amount)*1.11', size: 12, bold: true, align: 'right' }),
      E('label', 40, 84, 300, 13, { text: 'CATATAN PEMBAYARAN', size: 9, bold: true, color: '#6b7280' }),
      E('label', 40, 100, 380, 42, { text: 'Transfer ke BCA 123-456-7890 a.n. Acme Corporation. Mohon cantumkan nomor invoice pada berita transfer. Terima kasih.', size: 9, color: '#374151', wordWrap: true }),
    ]),
  ] };

  if (key === 'sales') return { bands: [
    B('ReportHeader', 72, [
      E('label', 40, 16, 400, 24, { text: 'LAPORAN PENJUALAN', size: 20, bold: true }),
      E('label', 40, 46, 500, 14, { text: 'Periode 01 Jun 2026 – 30 Jun 2026  ·  Cabang Jakarta  ·  Mata Uang IDR', size: 10, color: '#6b7280' }),
      E('label', 494, 18, 260, 16, { text: 'Acme Corporation', size: 11, bold: true, align: 'right' }),
    ]),
    B('ColumnHeader', 24, [
      E('label', 40, 4, 90, 16, { text: 'Tanggal', size: 9.5, bold: true, color: '#374151' }),
      E('label', 135, 4, 120, 16, { text: 'No. Invoice', size: 9.5, bold: true, color: '#374151' }),
      E('label', 255, 4, 200, 16, { text: 'Pelanggan', size: 9.5, bold: true, color: '#374151' }),
      E('label', 455, 4, 90, 16, { text: 'Qty', size: 9.5, bold: true, align: 'right', color: '#374151' }),
      E('label', 600, 4, 154, 16, { text: 'Total', size: 9.5, bold: true, align: 'right', color: '#374151' }),
    ], { bg: '#eef1f6' }),
    B('GroupHeader', 24, [
      E('label', 40, 5, 80, 15, { text: 'Pelanggan:', size: 10, bold: true, color: '#2563eb' }),
      E('field', 122, 5, 430, 15, { bind: 'Customers.Name', size: 11, bold: true }),
    ], { bg: '#eaf1fb' }),
    B('Detail', 22, [
      E('field', 40, 3, 90, 16, { bind: 'Invoices.Date', size: 9.5 }),
      E('field', 135, 3, 120, 16, { bind: 'Invoices.InvoiceNo', size: 9.5 }),
      E('field', 255, 3, 200, 16, { bind: 'Customers.Name', size: 9.5 }),
      E('field', 455, 3, 90, 16, { bind: 'InvoiceLines.Qty', size: 9.5, align: 'right' }),
      E('field', 600, 3, 154, 16, { bind: 'InvoiceLines.Amount', size: 9.5, align: 'right' }),
    ]),
    B('GroupFooter', 22, [
      E('line', 40, 2, 714, 1, { bg: '#c7d3e2' }),
      E('label', 300, 5, 255, 15, { text: 'Subtotal', size: 10, align: 'right', bold: true, color: '#6b7280' }),
      E('expr', 600, 5, 154, 15, { bind: 'Sum(InvoiceLines.Amount)', size: 10, align: 'right', bold: true }),
    ]),
    B('ReportFooter', 44, [
      E('line', 40, 8, 714, 2, { bg: '#1f2937' }),
      E('label', 300, 16, 255, 18, { text: 'TOTAL PENJUALAN', size: 11, bold: true, align: 'right' }),
      E('expr', 600, 15, 154, 18, { bind: 'Sum(InvoiceLines.Amount)', size: 12, bold: true, align: 'right' }),
    ]),
    pageFooter(),
  ] };

  if (key === 'purchasing') return { bands: [
    B('ReportHeader', 72, [
      E('label', 40, 16, 440, 24, { text: 'LAPORAN PEMBELIAN', size: 20, bold: true }),
      E('label', 40, 46, 500, 14, { text: 'Periode 01 Jun 2026 – 30 Jun 2026  ·  Status: Diterima', size: 10, color: '#6b7280' }),
      E('label', 494, 18, 260, 16, { text: 'Acme Corporation', size: 11, bold: true, align: 'right' }),
    ]),
    B('ColumnHeader', 24, [
      E('label', 40, 4, 90, 16, { text: 'Tanggal', size: 9.5, bold: true, color: '#374151' }),
      E('label', 135, 4, 110, 16, { text: 'No. PO', size: 9.5, bold: true, color: '#374151' }),
      E('label', 245, 4, 210, 16, { text: 'Vendor', size: 9.5, bold: true, color: '#374151' }),
      E('label', 455, 4, 90, 16, { text: 'Qty', size: 9.5, bold: true, align: 'right', color: '#374151' }),
      E('label', 600, 4, 154, 16, { text: 'Nilai', size: 9.5, bold: true, align: 'right', color: '#374151' }),
    ], { bg: '#eef1f6' }),
    B('GroupHeader', 24, [
      E('label', 40, 5, 70, 15, { text: 'Vendor:', size: 10, bold: true, color: '#2563eb' }),
      E('field', 112, 5, 440, 15, { bind: 'Vendors.Name', size: 11, bold: true }),
    ], { bg: '#eaf1fb' }),
    B('Detail', 22, [
      E('field', 40, 3, 90, 16, { bind: 'PurchaseOrders.Date', size: 9.5 }),
      E('field', 135, 3, 110, 16, { bind: 'PurchaseOrders.PONo', size: 9.5 }),
      E('field', 245, 3, 210, 16, { bind: 'Vendors.Name', size: 9.5 }),
      E('field', 455, 3, 90, 16, { bind: 'POLines.Qty', size: 9.5, align: 'right' }),
      E('field', 600, 3, 154, 16, { bind: 'POLines.Amount', size: 9.5, align: 'right' }),
    ]),
    B('GroupFooter', 22, [
      E('line', 40, 2, 714, 1, { bg: '#c7d3e2' }),
      E('label', 300, 5, 255, 15, { text: 'Subtotal', size: 10, align: 'right', bold: true, color: '#6b7280' }),
      E('expr', 600, 5, 154, 15, { bind: 'Sum(POLines.Amount)', size: 10, align: 'right', bold: true }),
    ]),
    B('ReportFooter', 44, [
      E('line', 40, 8, 714, 2, { bg: '#1f2937' }),
      E('label', 300, 16, 255, 18, { text: 'TOTAL PEMBELIAN', size: 11, bold: true, align: 'right' }),
      E('expr', 600, 15, 154, 18, { bind: 'Sum(POLines.Amount)', size: 12, bold: true, align: 'right' }),
    ]),
    pageFooter(),
  ] };

  if (key === 'finance') return { bands: [
    B('ReportHeader', 74, [
      E('label', 40, 14, 300, 14, { text: 'ACME CORPORATION', size: 10, bold: true, color: '#6b7280' }),
      E('label', 40, 30, 440, 24, { text: 'LAPORAN LABA RUGI', size: 19, bold: true }),
      E('label', 40, 58, 440, 14, { text: 'Periode 01 Januari – 30 Juni 2026  (dalam Rupiah)', size: 10, color: '#6b7280' }),
    ]),
    B('ColumnHeader', 24, [
      E('label', 40, 4, 400, 16, { text: 'KETERANGAN', size: 9.5, bold: true, color: '#374151' }),
      E('label', 554, 4, 200, 16, { text: 'JUMLAH', size: 9.5, bold: true, align: 'right', color: '#374151' }),
    ], { bg: '#eef1f6' }),
    B('Detail', 22, [
      E('field', 40, 3, 440, 16, { bind: 'Accounts.Name', size: 10 }),
      E('field', 554, 3, 200, 16, { bind: 'GLEntries.Amount', size: 10, align: 'right' }),
    ]),
    B('ReportFooter', 46, [
      E('line', 40, 8, 714, 2, { bg: '#1f2937' }),
      E('label', 300, 15, 255, 20, { text: 'LABA BERSIH', size: 12, bold: true, align: 'right' }),
      E('expr', 554, 14, 200, 20, { bind: 'Sum(GLEntries.Amount)', size: 13, bold: true, align: 'right' }),
    ]),
    pageFooter(),
  ] };

  return { bands: [
    B('ReportHeader', 58, [
      E('label', 40, 16, 440, 24, { text: 'DAFTAR PELANGGAN', size: 20, bold: true }),
      E('label', 40, 46, 400, 14, { text: 'Database: SalesDB · Customers', size: 10, color: '#6b7280' }),
    ]),
    B('ColumnHeader', 24, [
      E('label', 40, 4, 70, 16, { text: 'ID', size: 9.5, bold: true, color: '#374151' }),
      E('label', 110, 4, 200, 16, { text: 'Nama', size: 9.5, bold: true, color: '#374151' }),
      E('label', 310, 4, 150, 16, { text: 'Kota', size: 9.5, bold: true, color: '#374151' }),
      E('label', 460, 4, 180, 16, { text: 'Email', size: 9.5, bold: true, color: '#374151' }),
      E('label', 640, 4, 114, 16, { text: 'Telepon', size: 9.5, bold: true, color: '#374151' }),
    ], { bg: '#eef1f6' }),
    B('GroupHeader', 24, [
      E('label', 40, 5, 60, 15, { text: 'Kota:', size: 10, bold: true, color: '#2563eb' }),
      E('field', 102, 5, 400, 15, { bind: 'Customers.City', size: 11, bold: true }),
    ], { bg: '#eaf1fb' }),
    B('Detail', 22, [
      E('field', 40, 3, 70, 16, { bind: 'Customers.CustomerID', size: 9.5 }),
      E('field', 110, 3, 200, 16, { bind: 'Customers.Name', size: 9.5 }),
      E('field', 310, 3, 150, 16, { bind: 'Customers.City', size: 9.5 }),
      E('field', 460, 3, 180, 16, { bind: 'Customers.Email', size: 9.5 }),
      E('field', 640, 3, 114, 16, { bind: 'Customers.Phone', size: 9.5 }),
    ]),
    B('GroupFooter', 22, [
      E('line', 40, 2, 714, 1, { bg: '#c7d3e2' }),
      E('label', 430, 5, 180, 15, { text: 'Jumlah', size: 10, align: 'right', color: '#6b7280' }),
      E('expr', 620, 5, 134, 15, { bind: 'Count()', size: 10, align: 'right', bold: true }),
    ]),
    B('ReportFooter', 36, [
      E('line', 40, 8, 714, 2, { bg: '#dfe3e9' }),
      E('label', 430, 14, 180, 16, { text: 'Total Pelanggan', size: 10, align: 'right', color: '#6b7280' }),
      E('expr', 620, 14, 134, 16, { bind: 'Count()', size: 11, bold: true, align: 'right' }),
    ]),
    pageFooter(),
  ] };
}
