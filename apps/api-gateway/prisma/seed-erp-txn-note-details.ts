/**
 * Seed 100+ transaction note details into md_transaction_note_details.
 * Depends on md_transaction_notes — run seed-erp-transaction-notes.ts first.
 * Run: npm run db:seed:txn-note-details
 * Idempotent: skips if table already has >=1 row.
 */
import { PrismaClient } from '@prisma/client';

const prisma = new PrismaClient();

// Details per note code — each array = detail lines for that note
const DETAILS_BY_CODE: Record<string, string[]> = {
  'TN-001': [
    'Barang diantar ke alamat yang tertera pada Surat Jalan.',
    'Pengantar membawa dokumen identitas dan Surat Jalan asli.',
    'Serah terima hanya dilakukan kepada penerima yang terotorisasi.',
    'Dokumentasi foto diambil saat serah terima sebagai bukti.',
  ],
  'TN-002': [
    'Pembeli hadir dengan membawa salinan PO yang sudah dikonfirmasi.',
    'Pengambilan dilakukan pada jam operasional gudang (08.00–16.00 WIB).',
    'Barang tidak dapat ditahan lebih dari 7 hari sejak konfirmasi siap.',
    'Angkutan dari lokasi penjual menjadi tanggung jawab pembeli sepenuhnya.',
  ],
  'TN-011': [
    'Pembayaran diterima dalam bentuk uang tunai IDR saja.',
    'Bukti kwitansi diterbitkan segera setelah pembayaran diterima.',
    'Pecahan uang di atas Rp 100.000 diverifikasi keasliannya.',
    'Kembalian disiapkan maksimal Rp 500.000 per transaksi.',
  ],
  'TN-012': [
    'Transfer ditujukan ke rekening BCA atau Mandiri perusahaan.',
    'Konfirmasi transfer dikirim via WhatsApp ke nomor yang tertera.',
    'Nama pengirim harus sesuai dengan nama perusahaan di dokumen PO.',
    'Pembayaran dianggap sah setelah dana masuk ke rekening penerima.',
    'Bukti transfer yang tidak jelas akan diverifikasi 1×24 jam.',
  ],
  'TN-013': [
    'Uang muka 50% dibayarkan paling lambat 2 hari setelah PO diterima.',
    'Invoice uang muka diterbitkan sebelum pembayaran dilakukan.',
    'Tanpa uang muka, slot produksi tidak dapat dijadwalkan.',
    'Uang muka tidak dapat dikembalikan jika pembeli membatalkan pesanan.',
  ],
  'TN-021': [
    'Garansi berlaku untuk cacat manufaktur yang terbukti dari pabrik.',
    'Kerusakan fisik akibat pemakaian tidak tercakup dalam garansi.',
    'Kartu garansi harus diisi dan diserahkan saat serah terima barang.',
    'Klaim garansi diproses maksimal 14 hari kerja sejak laporan masuk.',
    'Penggantian unit baru berlaku jika unit rusak tidak dapat diperbaiki.',
  ],
  'TN-022': [
    'Nomor invoice dan tanggal pembelian wajib dicantumkan dalam klaim.',
    'Foto kondisi barang rusak dikirim via email sebelum pengiriman.',
    'Biaya pengiriman klaim ke pusat servis ditanggung pembeli.',
    'Bukti pembelian tidak dapat digantikan dengan screenshot chat.',
  ],
  'TN-031': [
    'Lembar data teknis produk dilampirkan sebagai bagian dari kontrak.',
    'Penyimpangan spesifikasi > 5% dianggap tidak sesuai kontrak.',
    'Pembeli berhak melakukan inspeksi teknis sebelum serah terima.',
    'Sertifikat kesesuaian standar diserahkan bersama dokumen pengiriman.',
    'Revisi spesifikasi harus disepakati tertulis oleh kedua belah pihak.',
  ],
  'TN-041': [
    'Kardus luar dilabeli dengan tanda "Fragile" untuk barang pecah belah.',
    'Setiap unit dibungkus plastik PE sebelum dimasukkan ke bubble wrap.',
    'Dimensi karton dan berat kotor dicantumkan di sisi luar kemasan.',
    'Kemasan rusak saat penerimaan wajib didokumentasikan dan dilaporkan.',
  ],
  'TN-051': [
    'Harga dalam penawaran berlaku 30 hari dari tanggal dokumen.',
    'Perubahan harga akibat kenaikan bahan baku diberitahukan 14 hari sebelumnya.',
    'Harga tidak termasuk ongkos kirim kecuali disepakati tertulis.',
    'Pembelian di luar kuotasi resmi tidak dapat diklaim harga khusus.',
    'Selisih harga akibat perubahan kurs USD diselesaikan berdasarkan kurs BI.',
  ],
  'TN-061': [
    'Gratis ongkir untuk wilayah Jakarta, Bogor, Depok, Tangerang, Bekasi.',
    'Wilayah luar Jabodetabek dikenakan biaya pengiriman sesuai tarif ekspedisi.',
    'Estimasi tiba 1–3 hari kerja untuk area Jawa; 3–7 hari luar Jawa.',
    'Keterlambatan akibat force majeure tidak menjadi tanggung jawab penjual.',
  ],
  'TN-071': [
    'Faktur pajak diterbitkan setelah pembayaran 100% diterima.',
    'Nomor NPWP pembeli wajib diserahkan sebelum faktur diterbitkan.',
    'Faktur pajak dikirim via email dalam format PDF dan fisik.',
    'Koreksi faktur pajak dilayani maksimal 30 hari setelah penerbitan.',
    'Faktur tidak dapat diterbitkan retroaktif lebih dari satu bulan.',
  ],
  'TN-081': [
    'Setiap work order produksi memiliki nomor batch unik untuk traceability.',
    'QC inline dilakukan setiap 2 jam selama proses produksi berlangsung.',
    'Non-conformance product dipisahkan dan dilabeli sebagai "Hold".',
    'Laporan produksi harian diserahkan ke supervisor paling lambat 17.00.',
  ],
  'TN-091': [
    'Sarung tangan, kacamata, dan sepatu safety wajib dipakai di lantai produksi.',
    'Pelanggaran K3 pertama diberikan peringatan tertulis.',
    'Area produksi bebas dari bahan makanan dan minuman.',
    'APAR tersedia di setiap 15 meter di zona produksi.',
    'Pelatihan K3 wajib diikuti karyawan baru sebelum mulai bekerja.',
  ],
  'TN-096': [
    'Nomor hotline layanan teknis: tersedia di lembar kontak dokumen.',
    'Pertanyaan teknis via email direspons maksimal 1 hari kerja.',
    'Kunjungan on-site dijadwalkan minimal 3 hari sebelumnya.',
    'Biaya kunjungan di luar area kota dikenakan biaya perjalanan aktual.',
  ],
};

// Extra standalone details to reach 100+ total
const EXTRA_DETAILS = [
  { noteCode: 'TN-003', content: 'Ekspedisi rekanan yang disepakati tercantum dalam Lampiran A kontrak.' },
  { noteCode: 'TN-003', content: 'Nomor resi pengiriman dikirimkan ke pembeli maks 4 jam setelah barang diserahkan.' },
  { noteCode: 'TN-004', content: 'Tarif ongkos kirim berdasarkan berat aktual atau volume (mana yang lebih besar).' },
  { noteCode: 'TN-004', content: 'Klaim ongkos kirim didukung bukti struk ekspedisi asli.' },
  { noteCode: 'TN-014', content: 'Sisa pembayaran harus lunas minimum 1 hari sebelum jadwal pengiriman.' },
  { noteCode: 'TN-015', content: 'PPN dihitung berdasarkan DPP (Dasar Pengenaan Pajak) sesuai ketentuan UU HPP.' },
  { noteCode: 'TN-016', content: 'PPN 11% ditambahkan ke harga pokok dan tertera terpisah di invoice.' },
  { noteCode: 'TN-017', content: 'Diskon tidak berlaku bersamaan dengan promo atau event khusus.' },
  { noteCode: 'TN-019', content: 'Giro mundur diserahkan paling lambat H-2 sebelum tanggal pengiriman.' },
  { noteCode: 'TN-020', content: 'Surat penagihan dikirim via email dan pos untuk tagihan yang melewati jatuh tempo.' },
  { noteCode: 'TN-023', content: 'Garansi batal jika label segel pada unit terbukti telah dibuka oleh pihak lain.' },
  { noteCode: 'TN-024', content: 'Formulir retur diunduh dari portal pelanggan dan dilampirkan saat pengiriman balik.' },
  { noteCode: 'TN-025', content: 'Unit yang sudah dipasang/diinstall tidak memenuhi syarat retur standar.' },
  { noteCode: 'TN-026', content: 'Ongkos kirim retur dapat di-refund jika cacat terbukti berasal dari pabrik.' },
  { noteCode: 'TN-027', content: 'Penggantian dilakukan setelah unit rusak diterima dan diverifikasi tim QC.' },
  { noteCode: 'TN-032', content: 'Inspeksi visual dilakukan dengan metode AQL 2.5 sesuai MIL-STD-1916.' },
  { noteCode: 'TN-033', content: 'Toleransi berlaku untuk komponen non-safety-critical saja.' },
  { noteCode: 'TN-034', content: 'Sertifikat food-grade diserahkan bersama setiap batch pengiriman.' },
  { noteCode: 'TN-035', content: 'Nomor SNI produk dicantumkan pada kemasan luar produk.' },
  { noteCode: 'TN-042', content: 'Bubble wrap minimal 2 lapis untuk unit dengan nilai > Rp 500.000.' },
  { noteCode: 'TN-043', content: 'Barcode label menggunakan format GS1-128 atau QR Code standar.' },
  { noteCode: 'TN-044', content: 'Sertifikat fumigasi pallet disertakan untuk pengiriman ekspor.' },
  { noteCode: 'TN-052', content: 'MOQ dapat dinegosiasikan untuk pelanggan korporat dengan kontrak tahunan.' },
  { noteCode: 'TN-053', content: 'Lead time dapat diperpendek menjadi 7 hari kerja dengan biaya ekspres 15%.' },
  { noteCode: 'TN-054', content: 'Revisi spesifikasi dikenakan biaya engineering Rp 250.000/jam.' },
  { noteCode: 'TN-055', content: 'Biaya pembatalan ditagihkan via credit note pada pembelian berikutnya.' },
  { noteCode: 'TN-072', content: 'Surat Jalan rangkap 3: arsip penjual, pembeli, dan pengantar.' },
  { noteCode: 'TN-073', content: 'Bill of Lading (B/L) dikirim via email dalam 24 jam setelah kapal berangkat.' },
  { noteCode: 'TN-074', content: 'Barang tanpa nomor PO yang valid akan ditolak di loading dock penjual.' },
  { noteCode: 'TN-075', content: 'Rekonsiliasi dilakukan setiap tanggal 28 melalui portal B2B perusahaan.' },
  { noteCode: 'TN-082', content: 'Material yang ditolak QC dikembalikan ke supplier dalam 5 hari kerja.' },
  { noteCode: 'TN-083', content: 'Dashboard MES dapat diakses tim pembeli dengan login tamu atas permintaan.' },
  { noteCode: 'TN-084', content: 'Kapasitas dapat meningkat 20% dengan pemberitahuan 30 hari sebelumnya.' },
  { noteCode: 'TN-092', content: 'Manifest limbah B3 tersedia untuk audit lingkungan setiap saat.' },
  { noteCode: 'TN-093', content: 'Temuan inspeksi K3 yang belum ditutup akan dieskalasi ke manajemen.' },
  { noteCode: 'TN-094', content: 'Laporan uji emisi tahunan diserahkan kepada otoritas lingkungan setempat.' },
  { noteCode: 'TN-097', content: 'SLA respons diperpanjang menjadi 48 jam pada hari libur nasional.' },
  { noteCode: 'TN-098', content: 'Spare part non-stok dapat dipesan dengan lead time 3–5 minggu.' },
  { noteCode: 'TN-099', content: 'Biaya perjalanan dihitung dari kantor pusat penjual ke lokasi pembeli.' },
  { noteCode: 'TN-100', content: 'Manual tersedia dalam Bahasa Indonesia dan Bahasa Inggris.' },
  { noteCode: 'TN-100', content: 'Versi digital manual dapat diunduh via QR code yang tertera pada unit.' },
];

async function main() {
  const count = await prisma.erpTransactionNoteDetail.count({ where: { deletedAt: null } });
  if (count > 0) {
    console.log(`md_transaction_note_details already has ${count} rows — skip.`);
    return;
  }

  // Fetch all seeded transaction notes
  const notes = await prisma.erpTransactionNote.findMany({
    where: { deletedAt: null },
    select: { id: true, code: true },
  });
  const noteMap = new Map(notes.map(n => [n.code, n.id]));

  const records: { transactionNoteId: bigint; sortOrder: number; content: string }[] = [];

  // Main details block
  for (const [code, lines] of Object.entries(DETAILS_BY_CODE)) {
    const noteId = noteMap.get(code);
    if (!noteId) continue;
    lines.forEach((content, idx) => {
      records.push({ transactionNoteId: noteId, sortOrder: idx + 1, content });
    });
  }

  // Extra details
  for (const { noteCode, content } of EXTRA_DETAILS) {
    const noteId = noteMap.get(noteCode);
    if (!noteId) continue;
    const existing = records.filter(r => r.transactionNoteId === noteId);
    records.push({ transactionNoteId: noteId, sortOrder: existing.length + 1, content });
  }

  console.log(`Inserting ${records.length} transaction note details…`);
  let inserted = 0;
  // createMany with BigInt requires chunked insert
  for (const rec of records) {
    await prisma.erpTransactionNoteDetail.create({ data: rec });
    inserted++;
  }
  console.log(`Done. Inserted ${inserted} rows.`);
}

main()
  .catch(err => { console.error(err); process.exit(1); })
  .finally(() => prisma.$disconnect());
