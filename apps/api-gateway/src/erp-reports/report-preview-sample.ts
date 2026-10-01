/**
 * Representative sample dataset for the Report Designer "Preview PDF" — lets a user
 * see a template's page layout, header/footer, fonts, orientation and column styling
 * with realistic dummy data, without running a real report.
 */
import type { EngineDataset } from '../erp-report-engine/dataset-adapter';

export function sampleDataset(): EngineDataset {
  return {
    key: 'preview',
    title: 'Contoh Laporan (Preview)',
    columns: [
      { key: 'kode', header: 'Kode', type: 'text' },
      { key: 'nama', header: 'Nama', type: 'text' },
      { key: 'tanggal', header: 'Tanggal', type: 'date', align: 'center' },
      { key: 'qty', header: 'Qty', type: 'qty', align: 'right' },
      { key: 'debit', header: 'Debit', type: 'money', align: 'right' },
      { key: 'kredit', header: 'Kredit', type: 'money', align: 'right' },
    ],
    rows: [
      { kode: '1-1000', nama: 'Kas', tanggal: '2026-06-01', qty: 1, debit: 5000000, kredit: 0 },
      { kode: '1-1200', nama: 'Bank BCA', tanggal: '2026-06-03', qty: 1, debit: 12500000, kredit: 0 },
      { kode: '1-1300', nama: 'Piutang Usaha', tanggal: '2026-06-05', qty: 8, debit: 7800000.5, kredit: 0 },
      { kode: '2-1000', nama: 'Hutang Usaha', tanggal: '2026-06-07', qty: 4, debit: 0, kredit: 9300000 },
      { kode: '4-1000', nama: 'Pendapatan Penjualan', tanggal: '2026-06-09', qty: 25, debit: 0, kredit: 16000000.75 },
      { kode: '5-1000', nama: 'Beban Operasional', tanggal: '2026-06-11', qty: 12, debit: 3200000, kredit: 0 },
    ],
    summary: [
      { label: 'Total Debit', value: 28500000.5 },
      { label: 'Total Kredit', value: 25300000.75 },
    ],
    generatedAt: new Date().toISOString(),
  };
}
