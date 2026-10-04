# Scope MVP — CV Bahtera Madani

> Status: **scope produk otoritatif** untuk deployment CV Bahtera Madani.
> Sumber: `temp/PRD-ERP-CV-Bahtera-Madani-A4.pdf`, ditelaah 2026-10-04.
> Implementasi tetap memakai platform **Senti ERP**; kebutuhan sekolah adalah
> kapabilitas vertikal di atas domain generik, bukan fork aplikasi atau duplikasi
> tabel transaksi.

## 1. Interpretasi MVP

PRD tidak memberi label “MVP” secara eksplisit. Untuk project ini, **MVP = Fase 1
“Fondasi & pengadaan sekolah”** karena fase tersebut adalah rilis pertama yang
memiliki hasil bisnis dan gerbang penerimaan mandiri.

MVP mencakup delapan kapabilitas:

1. **A1 — CRM Sekolah**
2. **A2 — Order Hub multi-kanal**
3. **A3 — Dokumen pengadaan**
4. **A4 — Pajak pengadaan**
5. **D1 — Katalog produk**
6. **D2 — Pembelian & supplier**
7. **D3 — Persediaan multi-gudang**
8. **E1 — Keuangan & akuntansi**

**Hasil MVP:** semua pesanan fase awal tercatat di satu tempat, dokumen BOS dapat
dibuat otomatis, dan piutang dapat dipantau.

**Gerbang penerimaan MVP:** satu periode BOS berjalan penuh di sistem tanpa
rekap operasional di Excel. Ini harus dibuktikan dengan skenario end-to-end dan
rekonsiliasi, bukan hanya keberadaan menu atau tabel.

> **Keputusan 2026-10-04 — Yayasan ditunda, sekolah jadi entitas utama.**
> PRD menggambarkan hierarki yayasan → unit sekolah, tapi Yayasan **bukan**
> syarat gerbang MVP. Transaksi (order, invoice, BAST, pengiriman, sumber dana
> BOS, piutang) selalu melekat pada **sekolah**, bukan yayasan. Yayasan
> hanyalah relasi opsional satu-ke-banyak untuk negosiasi grup, kontrak pusat,
> dan laporan konsolidasi. Karena biaya integrasi foundation (tabel yayasan,
> hierarki, perubahan CRM/pipeline/filter/piutang) tinggi dan manfaatnya
> baru terasa saat ada pelanggan grup, yayasan **dipindahkan ke Fase 3/B1**
> (lihat §7). MVP-1 = sekolah saja, tanpa yayasan.

## 2. Batas kanal pada MVP

PRD menyebut empat kanal pada diagram dan lima sumber input pada rincian A2:
SIPLah, portal sekolah, portal orang tua, sales, dan admin. Namun portal sekolah
serta portal orang tua baru dijadwalkan pada Fase 3.

Agar roadmap tidak kontradiktif, batas MVP ditetapkan sebagai berikut:

- **Aktif pada MVP:** input admin, input sales, dan impor/tarik pesanan SIPLah
  sesuai akses API merchant yang tersedia.
- **Order Hub wajib channel-ready:** setiap order menyimpan sumber kanal dan
  referensi eksternal yang idempotent.
- **Belum membuat portal pada MVP:** portal sekolah dan portal orang tua menjadi
  producer baru bagi Order Hub pada Fase 3 tanpa mengganti model order inti.
- Bila API SIPLah tidak tersedia, MVP memakai impor file ekspor dan kontrol
  rekonsiliasi harian sebagaimana mitigasi di PRD.

## 3. Scope fungsional MVP

### A1 — CRM Sekolah

Minimum yang harus tersedia:

- profil sekolah berbasis NPSN;
- jenjang, negeri/swasta, wilayah, jumlah siswa per kelas, estimasi pagu BOS;
- banyak kontak dengan peran kepala sekolah, bendahara, operator, dan TU;
- riwayat pesanan lintas kanal dan catatan kunjungan/negosiasi;
- pipeline prospek → penawaran → pesanan → terkirim → lunas;
- penanda periode BOS tahap 1/tahap 2;
- peringatan sekolah belum belanja dan kontrak harga akan berakhir.

> Yayasan (parent organizational, satu-ke-banyak sekolah) **tidak ada di
> MVP-1**. Lihat §7 — ditunda ke Fase 3/B1. MVP-1 hanya membutuhkan profil
> sekolah, bukan profil yayasan.

### A2 — Order Hub multi-kanal

Minimum yang harus tersedia:

- satu antrean order lintas kanal dengan identitas kanal dan external order ID;
- status bisnis: baru → dikonfirmasi → diproduksi/disiapkan → dikirim → diterima
  (BAST) → ditagih → lunas;
- item dagangan mengalir ke gudang;
- item cetak custom dapat menandai kebutuhan job produksi, tetapi eksekusi
  produksi penuh tetap Fase 2;
- impor/API SIPLah idempotent dan dapat direkonsiliasi dengan pencairan serta
  fee marketplace;
- sumber dana BOS/non-BOS dan tahun/tahap anggaran tersimpan pada order;
- audit perubahan status dan referensi ke dokumen sales, delivery, invoice,
  receipt, serta BAST.

### A3 — Dokumen pengadaan

Minimum yang harus tersedia:

- template dan nomor berurutan untuk surat penawaran, surat pesanan, invoice,
  kuitansi, surat jalan, BAST, dan lampiran foto serah terima;
- varian paket BOS dan non-BOS;
- generate satu dokumen atau satu paket PDF;
- tanda tangan/stempel digital dengan audit siapa, kapan, dan versi dokumen;
- arsip per sekolah dan tahun anggaran;
- dokumen dihasilkan dari data transaksi yang sama, tanpa input ulang.

### A4 — Pajak pengadaan

Minimum yang harus tersedia:

- perhitungan dan subledger PPN per transaksi;
- nomor/tanggal faktur pajak dan ekspor/integrasi Coretax melalui mekanisme yang
  disepakati (PJAP atau format ekspor);
- pencatatan PPh 22/23 yang dipotong bendahara;
- unggah dan status bukti potong;
- rekonsiliasi nilai dipotong dengan bukti yang diterima;
- laporan pajak bulanan untuk konsultan pajak.

### D1 — Katalog produk

Minimum yang harus tersedia:

- kategori buku teks, LKS, ATK, alat peraga, seragam/atribut, produk cetak,
  dan paket;
- atribut penerbit, jenjang, kelas, kurikulum, mata pelajaran, ukuran, dan HET;
- foto, deskripsi, serta status tayang per kanal;
- pembedaan barang dagangan dan item cetak custom;
- data harga dasar yang dapat digunakan transaksi MVP.

Mesin harga berlapis penuh, kontrak yayasan/sekolah, promo, produk eksklusif,
dan bundling portal termasuk **Fase 3/B1**, bukan syarat gerbang MVP, kecuali
harga aktual yang dibutuhkan transaksi awal.

### D2 — Pembelian & supplier

Minimum yang harus tersedia:

- PO, penerimaan barang, purchase invoice, retur, dan pembayaran vendor;
- three-way match PO ↔ penerimaan ↔ invoice;
- rabat penerbit dan barang konsinyasi bila dipakai pada data go-live;
- saran pembelian minimum berdasarkan stok minimum; forecast musiman lanjutan
  dapat memakai domain planning setelah histori cukup;
- posting stok/AP/GL atomik dan idempotent.

### D3 — Persediaan multi-gudang

Minimum yang harus tersedia:

- stok per gudang/lokasi dengan klasifikasi bahan baku, barang jadi, barang
  dagangan, dan konsinyasi;
- penerimaan, pengeluaran, transfer, retur, opening stock, stock count, dan
  adjustment;
- reservasi/penguncian stok untuk order agar tidak oversell;
- alokasi eksklusif sekolah bila ada barang khusus sekolah;
- barcode minimum untuk identifikasi/picking bila perangkat go-live memakai
  pemindai;
- rekonsiliasi nilai stok terhadap akun kontrol GL.

### E1 — Keuangan & akuntansi

Minimum yang harus tersedia:

- chart of accounts, fiscal period, document numbering, account determination,
  serta jurnal otomatis dari sales, purchasing, dan inventory;
- AR per sekolah dengan aging dan reminder jatuh tempo;
- AP supplier dan jadwal pembayaran;
- rekonsiliasi SIPLah, kas/bank, dan payment receipt;
- laporan laba rugi, neraca, arus kas, serta rekonsiliasi subledger;
- laba per sekolah, produk, wilayah, dan lini distribusi/percetakan sejauh data
  biaya MVP tersedia;
- anggaran vs realisasi per periode.

Laba **per job cetak** baru menjadi lengkap setelah HPP job Fase 2 tersedia.

## 4. Fondasi lintas-kapabilitas yang wajib ikut MVP

Walaupun tidak ditulis sebagai modul Fase 1, kapabilitas ini merupakan
prasyarat operasional dan non-fungsional MVP:

- identity, role, permission per menu/aksi, dan scope cabang/gudang;
- 2FA internal sebelum production go-live;
- audit harga, order, stok, dan jurnal minimal lima tahun;
- fiscal period, penomoran dokumen, currency, tax, akun kontrol, dan opening
  balances;
- backup terenkripsi harian serta uji pemulihan bulanan;
- transaksi posting atomik dan idempotent;
- perlindungan data siswa sesuai UU PDP; MVP tidak menyimpan data siswa kecuali
  benar-benar diperlukan sebelum Fase 3;
- responsif di web dan siap multi-gudang/multi-cabang;
- uji beban untuk volume internal/import yang disepakati. Target 1.000+ user
  portal bersamaan menjadi gate Fase 3 ketika portal dibangun.

## 5. Pemetaan ke project saat ini

| Kapabilitas | Yang sudah tersedia | Gap menuju MVP | Status |
|---|---|---|---|
| Fondasi admin/master | Auth ERP, user/role/permission, scope cabang/gudang/lokasi, audit, fiscal period, numbering, setting, item/partner/account/tax/currency | 2FA, verifikasi retensi audit/backup, kelengkapan data go-live | **Sebagian besar ada** |
| A1 CRM sekolah | Partner, alamat, kontak, kategori, salesman, dimensi wilayah | NPSN, profil BOS/sekolah, role kontak baku, kunjungan/negosiasi, pipeline, alert | **Gap besar** |
| A2 Order Hub | Sales quotation/order/delivery/invoice/receipt dan workflow generik | model kanal, external ID/idempotency, import/API SIPLah, status hub/BAST, pencairan dan fee SIPLah | **Fondasi ada, hub belum ada** |
| A3 Dokumen | Report Studio/report engine, attachment transaksi, numbering | paket dokumen pengadaan, template BOS, e-sign/stempel, BAST + foto, arsip sekolah/tahun | **Fondasi renderer ada** |
| A4 Pajak | Master pajak, posting PPN, model `fin_tax_entries` dan withholding certificate | service/API/UI tax subledger, PPh 22/23, bukti potong, Coretax/export, rekonsiliasi dan laporan bulanan | **Model DB ada, aplikasi belum** |
| D1 Katalog | Item/category/brand/class/media/attachment, harga dasar/tier model | atribut sekolah (penerbit/jenjang/kurikulum/mapel/HET), visibility per kanal, flag custom print; paket penuh ditunda Fase 3 | **Sebagian ada** |
| D2 Pembelian | PR/RFQ/bid/PO/GRN/PI/return/advance/payment, posting stok/GL dan outstanding | validasi end-to-end, rabat/konsinyasi sesuai kebutuhan, reorder minimum | **Kuat, perlu hardening** |
| D3 Persediaan | Multi-gudang, movement, opening, count, adjustment, bin/lot/reservation pada model | alokasi eksklusif sekolah, barcode/picking minimum, aplikasi reservation/lot yang belum lengkap, rekonsiliasi stok-GL | **Kuat, ada gap vertikal** |
| E1 Keuangan | Cash/bank, journal, ledger, AR/AP receipt/payment, giro, FX, aging dan laporan keuangan | rekonsiliasi SIPLah/payment source, reminder WA, profitabilitas sekolah/lini, stock reconciliation, bank rec application | **Kuat, perlu integrasi/reporting** |

Status “sudah tersedia” berarti komponen kode/model ditemukan, **bukan** otomatis
lulus production. Alur kritis tetap harus diverifikasi terhadap database nyata,
status workflow, fiscal period, retry idempotent, reversal, dan jurnal seimbang.

## 6. Urutan implementasi MVP

### MVP-0 — Baseline dan data siap posting

1. Verifikasi data konfigurasi wajib: fiscal period, numbering, currency, tax,
   akun kontrol/default, item-account, partner-account, warehouse, dan opening.
2. Luluskan skenario P2P dan O2C yang sudah didefinisikan di
   `docs/implementation-plan.md`.
3. Lengkapi test posting stok/GL, reversal/void, outstanding, dan rekonsiliasi.

### MVP-1 — Model sekolah dan CRM

1. Tambahkan profil sekolah di atas master partner.
2. Tambahkan NPSN, atribut sekolah/BOS, role kontak, activity/visit, serta
   pipeline.
3. Sediakan pencarian, filter BOS, alert, dan histori lintas order.

### MVP-2 — Order Hub dan intake SIPLah

1. Jadikan sales order sebagai dokumen transaksi inti yang direferensikan hub;
   jangan membuat engine stok/GL kedua.
2. Tambahkan record intake/order-channel dengan idempotency key, external status,
   import batch, error/retry, serta mapping ke order.
3. Implementasikan input admin/sales dan adapter SIPLah API atau file.
4. Tambahkan sumber dana, periode BOS, fee/pencairan, dan status BAST/tagihan/lunas.

### MVP-3 — Dokumen dan pajak pengadaan

1. Bangun template paket dokumen di atas Report Studio/report engine existing.
2. Tambahkan generator paket BOS/non-BOS, tanda tangan/stempel, foto BAST, dan
   arsip sekolah/tahun.
3. Aktifkan model tax entry/WHT melalui service/API/UI, posting otomatis dari
   invoice/receipt, bukti potong, rekonsiliasi, serta ekspor Coretax.

### MVP-4 — Hardening katalog, purchasing, inventory, dan finance

1. Lengkapi atribut katalog sekolah dan visibility kanal minimum.
2. Selesaikan reservation, exclusive allocation, barcode minimum, dan
   stock-to-GL reconciliation.
3. Luluskan three-way match dan skenario partial P2P.
4. Tambahkan SIPLah/payment reconciliation, reminder AR, serta laporan margin
   per sekolah/produk/wilayah.

### MVP-5 — Pilot satu periode BOS

1. Migrasikan master dan opening balances yang sudah direkonsiliasi.
2. Jalankan satu cohort sekolah dari order intake hingga lunas dan arsip dokumen.
3. Rekonsiliasi order, stok, AR/AP, pajak, bank/pencairan, dan GL.
4. Gerbang lulus hanya bila operasional periode pilot tidak membutuhkan rekap
   Excel paralel selain file sumber impor SIPLah yang memang menjadi adapter.

## 7. Yayasan — kapan & mengapa (dipindahkan dari MVP-1)

**Keputusan 2026-10-04:** Yayasan **tidak** masuk MVP-1. Alasan:

1. **Transaksi selalu melekat pada sekolah.** Order, invoice, BAST, pengiriman,
   sumber dana BOS, dan piutang semua berbasis sekolah. Yayasan bukan pelaku
   transaksi — hanya parent organisasi.
2. **Manfaat yayasan baru terasa saat ada pelanggan grup.** Yayasan diperlukan
   untuk negosiasi kontrak pusat, CRM grup (satu kontak pengurus → banyak
   sekolah), dan laporan konsolidasi (omzet/piutang/potensi seluruh sekolah).
   Bila semua pelanggan adalah sekolah mandiri, yayasan hanya biaya tanpa
   nilai.
3. **Biaya integrasi foundation tinggi.** Tabel yayasan, hierarki
   yayasan→sekolah, perubahan CRM/pipeline/filter/piutang, dan perubahan
   arsitektur transaksi (sekolah ↔ yayasan) harus dilakukan sebelum go-live.
   Kesalahan di sini mengganggu gerbang MVP.

**Kapan yayasan diaktifkan (Fase 3/B1):**
- Sudah ada pelanggan grup (satu yayasan mengelola ≥2 sekolah);
- Ada kebutuhan laporan konsolidasi atau negosiasi kontrak tingkat yayasan;
- Ada keputusan bisnis yang diambil di level yayasan, bukan per sekolah.

**Model yang direkomendasikan bila diaktifkan (biaya kecil):**
- Relasi opsional `yayasan → banyak sekolah` (bukan hierarki wajib).
- Transaksi tetap melekat pada sekolah; yayasan hanya untuk agregasi dan
  konteks relasi (negosiasi, kontrak, laporan konsolidasi).
- Jangan buat yayasan menggantikan sekolah sebagai pelaku transaksi.

**Jika di akhir Fase 2 masih tidak ada pelanggan grup:** yayasan dapat
dihapus selamanya dan tidak perlu dibangun.

## 8. Di luar MVP

### Fase 2 — Percetakan & distribusi

Estimasi cetak, job/pre-press, penjadwalan produksi, variable data printing, HPP
per job/makloon, packing per siswa, pengiriman/armada, dan aplikasi sales lapangan.
Model BOM/WO generic yang sudah ada tidak berarti kapabilitas percetakan ini
selesai; workflow print-specific harus dibangun pada fase ini.

### Fase 3 — Portal sekolah & orang tua

Mesin harga kontrak penuh, portal yayasan/sekolah, portal orang tua, dashboard
sekolah, payment gateway, WhatsApp, paket kelas, approval sekolah, serta load test
1.000+ pengguna bersamaan.

### Fase 4 — Optimasi & AI

SDM/payroll, fixed assets/perawatan, dashboard lanjutan, forecasting AI, asisten
WhatsApp, OCR, drafting penawaran, dan anomaly detection dengan human approval.

### Di luar versi pertama

- marketplace publik untuk umum;
- aplikasi kasir toko ritel;
- manufaktur di luar percetakan;
- integrasi langsung ke ARKAS.

## 9. Open decisions sebelum estimasi final

Jawaban wajib dicatat sebelum workstream terkait dikunci:

1. porsi omzet percetakan vs distribusi;
2. jumlah sekolah aktif/swasta dan jumlah user internal;
3. marketplace SIPLah yang dipakai dan akses API merchant;
4. volume order normal/puncak serta target pilot BOS;
5. mesin cetak dan kapasitas (untuk Fase 2);
6. skema bagi hasil koperasi/sekolah;
7. sistem/data lama dan kualitas migrasinya;
8. rencana cabang/gudang kota lain;
9. scope seragam dan make-or-buy;
10. apakah rabat, konsinyasi, barcode, alokasi eksklusif, serta PPh 22/23 wajib
    pada pilot pertama atau dapat diluncurkan bertahap selama masih dalam MVP.
