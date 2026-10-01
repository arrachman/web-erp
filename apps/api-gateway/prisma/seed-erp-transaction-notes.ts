/**
 * Seed realistic transaction notes into md_transaction_notes.
 * 110 entries covering all common ERP/manufacturing contexts in Indonesia.
 * Run standalone: npx ts-node prisma/seed-erp-transaction-notes.ts
 * Or imported and called from seed-erp.ts via seedTransactionNotes(prisma).
 */
import { PrismaClient } from '@prisma/client';

type Note = {
  code: string;
  name: string;
  legacyCode?: string;
};

const TRANSACTION_NOTES: Note[] = [
  // ── Penerimaan Barang (TN-001 – TN-009) ─────────────────────────────────────
  { code: 'TN-001', name: 'Barang diterima sesuai PO',                                    legacyCode: 'RCV-01' },
  { code: 'TN-002', name: 'Barang diterima sebagian, sisa menyusul',                      legacyCode: 'RCV-02' },
  { code: 'TN-003', name: 'Barang diterima dengan kondisi baik',                          legacyCode: 'RCV-03' },
  { code: 'TN-004', name: 'Barang diterima dengan kerusakan minor, dikonfirmasi supplier', legacyCode: 'RCV-04' },
  { code: 'TN-005', name: 'Penerimaan barang sample/trial',                               legacyCode: 'RCV-05' },
  { code: 'TN-006', name: 'Penerimaan barang konsinyasi',                                 legacyCode: 'RCV-06' },
  { code: 'TN-007', name: 'Penerimaan retur dari pelanggan',                              legacyCode: 'RCV-07' },
  { code: 'TN-008', name: 'Penerimaan barang impor — dokumen lengkap',                    legacyCode: 'RCV-08' },
  { code: 'TN-009', name: 'Penerimaan barang pengganti/klaim garansi',                    legacyCode: 'RCV-09' },

  // ── Pengiriman Barang (TN-010 – TN-019) ─────────────────────────────────────
  { code: 'TN-010', name: 'Pengiriman sesuai SO',                                         legacyCode: 'SHP-01' },
  { code: 'TN-011', name: 'Pengiriman sebagian, sisa diproses terpisah',                  legacyCode: 'SHP-02' },
  { code: 'TN-012', name: 'Pengiriman dengan angsuran bertahap',                          legacyCode: 'SHP-03' },
  { code: 'TN-013', name: 'Pengiriman urgent/express',                                    legacyCode: 'SHP-04' },
  { code: 'TN-014', name: 'Pengiriman via ekspedisi rekanan',                             legacyCode: 'SHP-05' },
  { code: 'TN-015', name: 'Pengiriman ke gudang cabang',                                  legacyCode: 'SHP-06' },
  { code: 'TN-016', name: 'Pengiriman sampel kepada prospek',                             legacyCode: 'SHP-07' },
  { code: 'TN-017', name: 'Pengiriman COD — bayar saat terima',                           legacyCode: 'SHP-08' },
  { code: 'TN-018', name: 'Pengiriman ekspor — dokumen kepabeanan lengkap',               legacyCode: 'SHP-09' },
  { code: 'TN-019', name: 'Pengiriman dengan asuransi kargo',                             legacyCode: 'SHP-10' },

  // ── Pembayaran (TN-020 – TN-029) ─────────────────────────────────────────────
  { code: 'TN-020', name: 'Pembayaran tunai',                                             legacyCode: 'PAY-01' },
  { code: 'TN-021', name: 'Pembayaran transfer bank',                                     legacyCode: 'PAY-02' },
  { code: 'TN-022', name: 'Pembayaran giro mundur',                                       legacyCode: 'PAY-03' },
  { code: 'TN-023', name: 'Pembayaran cicilan/angsuran',                                  legacyCode: 'PAY-04' },
  { code: 'TN-024', name: 'Pembayaran DP 50%',                                            legacyCode: 'PAY-05' },
  { code: 'TN-025', name: 'Pelunasan sisa hutang',                                        legacyCode: 'PAY-06' },
  { code: 'TN-026', name: 'Pembayaran dengan diskon early payment',                       legacyCode: 'PAY-07' },
  { code: 'TN-027', name: 'Pembayaran via virtual account',                               legacyCode: 'PAY-08' },
  { code: 'TN-028', name: 'Pembayaran via QRIS/dompet digital',                           legacyCode: 'PAY-09' },
  { code: 'TN-029', name: 'Penyelesaian klaim asuransi',                                  legacyCode: 'PAY-10' },

  // ── Produksi (TN-030 – TN-039) ───────────────────────────────────────────────
  { code: 'TN-030', name: 'Material dikeluarkan untuk produksi',                          legacyCode: 'PRD-01' },
  { code: 'TN-031', name: 'Hasil produksi masuk gudang',                                  legacyCode: 'PRD-02' },
  { code: 'TN-032', name: 'Produksi batch #1',                                            legacyCode: 'PRD-03' },
  { code: 'TN-033', name: 'Rework/perbaikan barang reject',                               legacyCode: 'PRD-04' },
  { code: 'TN-034', name: 'Barang scrap/tidak layak pakai',                               legacyCode: 'PRD-05' },
  { code: 'TN-035', name: 'Work in Progress (WIP) dipindah ke lini berikutnya',           legacyCode: 'PRD-06' },
  { code: 'TN-036', name: 'Produksi selesai — QC lulus',                                  legacyCode: 'PRD-07' },
  { code: 'TN-037', name: 'Produksi ditunda — menunggu material',                         legacyCode: 'PRD-08' },
  { code: 'TN-038', name: 'Produksi trial/percobaan formula baru',                        legacyCode: 'PRD-09' },
  { code: 'TN-039', name: 'Overhead produksi dialokasikan ke batch',                      legacyCode: 'PRD-10' },

  // ── Inventori (TN-040 – TN-049) ──────────────────────────────────────────────
  { code: 'TN-040', name: 'Stok opname selesai',                                          legacyCode: 'INV-01' },
  { code: 'TN-041', name: 'Penyesuaian stok fisik vs sistem',                             legacyCode: 'INV-02' },
  { code: 'TN-042', name: 'Transfer antar gudang',                                        legacyCode: 'INV-03' },
  { code: 'TN-043', name: 'Transfer antar lokasi dalam gudang',                           legacyCode: 'INV-04' },
  { code: 'TN-044', name: 'Barang masuk dari pembelian lokal',                            legacyCode: 'INV-05' },
  { code: 'TN-045', name: 'Barang masuk dari impor',                                      legacyCode: 'INV-06' },
  { code: 'TN-046', name: 'Stok minimum tercapai — trigger reorder',                      legacyCode: 'INV-07' },
  { code: 'TN-047', name: 'Barang kadaluarsa/expired dipisah untuk disposal',             legacyCode: 'INV-08' },
  { code: 'TN-048', name: 'Relabeling/repacking barang gudang',                           legacyCode: 'INV-09' },
  { code: 'TN-049', name: 'Saldo awal persediaan (opening stock)',                        legacyCode: 'INV-10' },

  // ── Retur (TN-050 – TN-059) ──────────────────────────────────────────────────
  { code: 'TN-050', name: 'Retur ke supplier — tidak sesuai spesifikasi',                 legacyCode: 'RTN-01' },
  { code: 'TN-051', name: 'Retur ke supplier — cacat produksi',                           legacyCode: 'RTN-02' },
  { code: 'TN-052', name: 'Retur dari pelanggan — salah kirim',                           legacyCode: 'RTN-03' },
  { code: 'TN-053', name: 'Retur dari pelanggan — rusak saat pengiriman',                 legacyCode: 'RTN-04' },
  { code: 'TN-054', name: 'Retur dari pelanggan — tidak sesuai pesanan',                  legacyCode: 'RTN-05' },
  { code: 'TN-055', name: 'Retur ke supplier — kelebihan pengiriman',                     legacyCode: 'RTN-06' },
  { code: 'TN-056', name: 'Retur internal — salah alokasi gudang',                        legacyCode: 'RTN-07' },
  { code: 'TN-057', name: 'Retur dari pelanggan — produk kadaluarsa',                     legacyCode: 'RTN-08' },
  { code: 'TN-058', name: 'Retur ke supplier — menunggu penggantian',                     legacyCode: 'RTN-09' },
  { code: 'TN-059', name: 'Retur dari pelanggan — klaim garansi produk',                  legacyCode: 'RTN-10' },

  // ── Keuangan & Piutang/Hutang (TN-060 – TN-069) ─────────────────────────────
  { code: 'TN-060', name: 'Pelunasan piutang pelanggan',                                  legacyCode: 'FIN-01' },
  { code: 'TN-061', name: 'Pembayaran hutang supplier tepat waktu',                       legacyCode: 'FIN-02' },
  { code: 'TN-062', name: 'Pembayaran hutang dengan diskon early payment',                legacyCode: 'FIN-03' },
  { code: 'TN-063', name: 'Koreksi selisih kurs valuta asing',                            legacyCode: 'FIN-04' },
  { code: 'TN-064', name: 'Write-off piutang tidak tertagih',                             legacyCode: 'FIN-05' },
  { code: 'TN-065', name: 'Pencairan deposito jaminan supplier',                          legacyCode: 'FIN-06' },
  { code: 'TN-066', name: 'Pengembalian kelebihan bayar pelanggan',                       legacyCode: 'FIN-07' },
  { code: 'TN-067', name: 'Penerimaan bunga bank',                                        legacyCode: 'FIN-08' },
  { code: 'TN-068', name: 'Biaya bank dipotong otomatis dari rekening',                   legacyCode: 'FIN-09' },
  { code: 'TN-069', name: 'Pencatatan penerimaan uang muka penjualan',                    legacyCode: 'FIN-10' },

  // ── Quality Control (TN-070 – TN-079) ────────────────────────────────────────
  { code: 'TN-070', name: 'QC lulus — barang dilepas ke gudang',                          legacyCode: 'QC-01' },
  { code: 'TN-071', name: 'QC gagal — barang dikembalikan ke produksi',                   legacyCode: 'QC-02' },
  { code: 'TN-072', name: 'QC sampel — uji random dari batch',                            legacyCode: 'QC-03' },
  { code: 'TN-073', name: 'QC incoming — inspeksi barang masuk dari supplier',            legacyCode: 'QC-04' },
  { code: 'TN-074', name: 'Karantina barang menunggu hasil lab',                          legacyCode: 'QC-05' },
  { code: 'TN-075', name: 'Barang diluluskan dengan catatan minor',                       legacyCode: 'QC-06' },
  { code: 'TN-076', name: 'Inspeksi akhir sebelum pengiriman ke pelanggan',               legacyCode: 'QC-07' },
  { code: 'TN-077', name: 'Uji kualitas bahan baku awal produksi',                        legacyCode: 'QC-08' },
  { code: 'TN-078', name: 'Toleransi penyimpangan disetujui manajer QC',                  legacyCode: 'QC-09' },
  { code: 'TN-079', name: 'Sertifikasi batch produksi diterbitkan',                       legacyCode: 'QC-10' },

  // ── Pembelian & Pengadaan (TN-080 – TN-089) ──────────────────────────────────
  { code: 'TN-080', name: 'PO diterbitkan atas dasar PR yang disetujui',                  legacyCode: 'PUR-01' },
  { code: 'TN-081', name: 'Perubahan PO — penyesuaian kuantitas',                         legacyCode: 'PUR-02' },
  { code: 'TN-082', name: 'Perubahan PO — penyesuaian harga',                             legacyCode: 'PUR-03' },
  { code: 'TN-083', name: 'PO dibatalkan atas permintaan buyer',                          legacyCode: 'PUR-04' },
  { code: 'TN-084', name: 'Pembelian lokal — tidak perlu LC',                             legacyCode: 'PUR-05' },
  { code: 'TN-085', name: 'Pembelian impor — L/C dibuka ke bank',                         legacyCode: 'PUR-06' },
  { code: 'TN-086', name: 'Kontrak pembelian tahunan — blanket PO',                       legacyCode: 'PUR-07' },
  { code: 'TN-087', name: 'Pengadaan darurat — prosedur fast-track',                      legacyCode: 'PUR-08' },
  { code: 'TN-088', name: 'Pembelian aset tetap — prosedur kapex',                        legacyCode: 'PUR-09' },
  { code: 'TN-089', name: 'Pembelian bahan penolong/supplies non-produksi',               legacyCode: 'PUR-10' },

  // ── Penjualan (TN-090 – TN-099) ──────────────────────────────────────────────
  { code: 'TN-090', name: 'SO dikonfirmasi pelanggan',                                    legacyCode: 'SAL-01' },
  { code: 'TN-091', name: 'SO direvisi atas permintaan pelanggan',                        legacyCode: 'SAL-02' },
  { code: 'TN-092', name: 'SO dibatalkan — produk tidak tersedia',                        legacyCode: 'SAL-03' },
  { code: 'TN-093', name: 'Diskon khusus disetujui manajer penjualan',                   legacyCode: 'SAL-04' },
  { code: 'TN-094', name: 'Penjualan tender/lelang pemerintah',                           legacyCode: 'SAL-05' },
  { code: 'TN-095', name: 'Penjualan via distributor/agen resmi',                         legacyCode: 'SAL-06' },
  { code: 'TN-096', name: 'Faktur pajak diterbitkan bersama SI',                          legacyCode: 'SAL-07' },
  { code: 'TN-097', name: 'Penjualan ekspor — PIB/PEB dilampirkan',                       legacyCode: 'SAL-08' },
  { code: 'TN-098', name: 'Klaim diskon volume akhir periode',                            legacyCode: 'SAL-09' },
  { code: 'TN-099', name: 'Invoice final dikirim ke pelanggan via email',                 legacyCode: 'SAL-10' },

  // ── Umum & Lain-lain (TN-100 – TN-110) ──────────────────────────────────────
  { code: 'TN-100', name: 'Koreksi jurnal — kesalahan pencatatan',                        legacyCode: 'GEN-01' },
  { code: 'TN-101', name: 'Penutupan periode fiskal bulan berjalan',                      legacyCode: 'GEN-02' },
  { code: 'TN-102', name: 'Rekonsiliasi saldo antar modul',                               legacyCode: 'GEN-03' },
  { code: 'TN-103', name: 'Penyesuaian harga pokok akhir tahun',                          legacyCode: 'GEN-04' },
  { code: 'TN-104', name: 'Alokasi biaya bersama ke unit produk',                         legacyCode: 'GEN-05' },
  { code: 'TN-105', name: 'Transaksi antar divisi — internal billing',                    legacyCode: 'GEN-06' },
  { code: 'TN-106', name: 'Penyesuaian biaya pengiriman setelah faktur',                  legacyCode: 'GEN-07' },
  { code: 'TN-107', name: 'Pembatalan transaksi — persetujuan supervisor',                legacyCode: 'GEN-08' },
  { code: 'TN-108', name: 'Catatan khusus — perlu verifikasi lebih lanjut',               legacyCode: 'GEN-09' },
  { code: 'TN-109', name: 'Dokumen pendukung terlampir di email',                         legacyCode: 'GEN-10' },
  { code: 'TN-110', name: 'Transaksi selesai — tidak ada tindak lanjut',                  legacyCode: 'GEN-11' },
];

export async function seedTransactionNotes(prisma: PrismaClient): Promise<void> {
  let count = 0;
  for (const note of TRANSACTION_NOTES) {
    await prisma.erpTransactionNote.upsert({
      where:  { code: note.code },
      create: {
        code:       note.code,
        name:       note.name,
        isActive:   true,
        legacyCode: note.legacyCode ?? null,
      },
      update: {},
    });
    count++;
  }
  console.log(`✓ md_transaction_notes (${count} entries)`);
}

// ── Standalone runner ────────────────────────────────────────────────────────
async function main(): Promise<void> {
  const prisma = new PrismaClient();
  try {
    console.log('Seeding md_transaction_notes…');
    await seedTransactionNotes(prisma);
    const total = await prisma.erpTransactionNote.count();
    console.log(`Done. md_transaction_notes now has ${total} rows.`);
  } catch (err) {
    console.error(err);
    process.exit(1);
  } finally {
    await prisma.$disconnect();
  }
}

// Run only when executed directly (not imported)
if (require.main === module) {
  void main();
}
