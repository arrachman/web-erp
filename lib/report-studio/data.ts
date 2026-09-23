import type { RsReportData, RsTplKey } from './types';

/** Sample (mock) datasets used for live preview & export, keyed by template. */
export function buildData(key: RsTplKey): RsReportData {
  if (key === 'invoice') {
    const ctx: Record<string, string> = {
      'Invoices.InvoiceNo': 'INV-2026-0042', 'Invoices.Date': '17 Jun 2026', 'Invoices.DueDate': '01 Jul 2026',
      'Customers.Name': 'PT Maju Bersama Sejahtera', 'Customers.City': 'Jakarta Selatan, DKI Jakarta',
    };
    const L: Array<[string, number, number, number]> = [
      ['Laptop Bisnis Pro 14"', 2, 12500000, 25000000],
      ['Docking Station USB-C', 2, 1850000, 3700000],
      ['Monitor 27" QHD', 3, 3200000, 9600000],
      ['Keyboard Mekanikal', 5, 950000, 4750000],
      ['Lisensi Office 365 (tahun)', 10, 1200000, 12000000],
      ['Jasa Instalasi & Setup', 1, 2500000, 2500000],
    ];
    return { headerCtx: ctx, rows: L.map((r) => ({ 'Products.Name': r[0], 'InvoiceLines.Qty': r[1], 'InvoiceLines.Price': r[2], 'InvoiceLines.Amount': r[3] })) };
  }
  if (key === 'sales') {
    const D = ['02 Jun', '03 Jun', '05 Jun', '07 Jun', '09 Jun', '11 Jun', '13 Jun', '15 Jun', '18 Jun', '20 Jun', '22 Jun', '24 Jun', '26 Jun', '28 Jun', '29 Jun', '30 Jun'];
    const C = ['PT Maju Bersama', 'CV Sentosa Abadi', 'PT Global Niaga', 'Toko Sumber Rejeki', 'PT Cahaya Mandiri', 'UD Berkah Jaya', 'PT Sinar Terang', 'CV Mitra Usaha'];
    const P = ['Laptop Bisnis Pro', 'Monitor 27" QHD', 'Printer Laser', 'Router Enterprise', 'Server Rack 2U', 'Switch 48-Port'];
    const rows: RsReportData['rows'] = [];
    for (let i = 0; i < 16; i++) {
      const qty = 2 + (i * 3) % 9; const amt = 1500000 + (i * 1234567) % 18000000;
      rows.push({ 'Invoices.Date': D[i] + ' 2026', 'Invoices.InvoiceNo': 'INV-2026-' + String(20 + i).padStart(4, '0'), 'Customers.Name': C[i % C.length], 'Products.Name': P[i % P.length], 'InvoiceLines.Qty': qty, 'InvoiceLines.Amount': amt });
    }
    return { headerCtx: {}, rows };
  }
  if (key === 'purchasing') {
    const D = ['01 Jun', '04 Jun', '06 Jun', '08 Jun', '10 Jun', '12 Jun', '14 Jun', '16 Jun', '19 Jun', '21 Jun', '23 Jun', '27 Jun'];
    const V = ['PT Distribusi Elektronik', 'CV Komponen Nusantara', 'PT Teknologi Andal', 'UD Logistik Cepat', 'PT Sumber Komputer'];
    const P = ['Laptop Bisnis Pro', 'Monitor 27" QHD', 'SSD NVMe 1TB', 'RAM 16GB DDR5', 'Power Supply 750W'];
    const rows: RsReportData['rows'] = [];
    for (let i = 0; i < 12; i++) {
      const qty = 5 + (i * 4) % 20; const amt = 2200000 + (i * 987654) % 14000000;
      rows.push({ 'PurchaseOrders.Date': D[i] + ' 2026', 'PurchaseOrders.PONo': 'PO-2026-' + String(10 + i).padStart(4, '0'), 'Vendors.Name': V[i % V.length], 'Products.Name': P[i % P.length], 'POLines.Qty': qty, 'POLines.Amount': amt });
    }
    return { headerCtx: {}, rows };
  }
  if (key === 'finance') {
    const F: Array<[string, number | null]> = [
      ['PENDAPATAN', null], ['  Penjualan Barang', 1842000000], ['  Pendapatan Jasa', 236000000], ['  Pendapatan Lain', 48000000],
      ['HARGA POKOK PENJUALAN', null], ['  Pembelian Barang', -1120000000], ['  Biaya Produksi', -184000000],
      ['BEBAN OPERASIONAL', null], ['  Gaji & Tunjangan', -312000000], ['  Sewa & Utilitas', -96000000], ['  Pemasaran', -74000000], ['  Penyusutan', -38000000],
      ['PENDAPATAN/BEBAN LAIN', null], ['  Beban Bunga', -22000000],
    ];
    return { headerCtx: {}, rows: F.map((r) => ({ 'Accounts.Name': r[0], 'GLEntries.Amount': (r[1] === null ? '' : r[1]) })) };
  }
  const N = ['PT Maju Bersama Sejahtera', 'CV Sentosa Abadi', 'PT Global Niaga Utama', 'Toko Sumber Rejeki', 'PT Cahaya Mandiri', 'UD Berkah Jaya', 'PT Sinar Terang Abadi', 'CV Mitra Usaha', 'PT Karya Nusantara', 'Toko Elektronik Jaya', 'PT Andalan Prima', 'CV Sukses Makmur', 'PT Dwi Tunggal', 'UD Rahmat Sejahtera', 'PT Bintang Timur', 'CV Harapan Baru'];
  const KT = ['Jakarta', 'Surabaya', 'Bandung', 'Medan', 'Semarang', 'Makassar', 'Yogyakarta', 'Denpasar'];
  const rows = N.map((n, i) => ({
    'Customers.CustomerID': 'C' + String(1001 + i), 'Customers.Name': n, 'Customers.City': KT[i % KT.length],
    'Customers.Email': 'cs@' + n.split(' ')[1].toLowerCase() + '.co.id', 'Customers.Phone': '021-' + (5550000 + i * 131).toString().slice(0, 7),
  }));
  return { headerCtx: {}, rows };
}
