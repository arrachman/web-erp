# Rencana Kerja Fase 2 — Percetakan & Distribusi

**Status:** rencana kerja (belum dimulai) — disusun 2026-10-04 setelah Fase 1/MVP selesai & live.
**Dasar scope:** `docs/bahtera-madani-mvp-scope.md` §8 (Fase 2 = Percetakan & distribusi).
**Aturan main:** mengikuti `CLAUDE.md` proyek (penamaan tabel/model, list page §2.7, guard, commit atomik) dan pelajaran Fase 1: setiap perubahan backend dibangun di repo ini **dan** disinkronkan ke salinan live `sentient-factory` pada workstream yang sama — sinkronisasi adalah bagian dari Definition of Done, bukan pekerjaan susulan. Data yang belum tersedia dari klien diisi provisional bertanda sesuai §6.

## 1. Fondasi yang sudah ada (hasil audit 2026-10-04)

| Aset | Kondisi | Dipakai untuk |
|---|---|---|
| `erp-mfg-boms` + model BOM/Input/Output | Ada (generik) | Basis resep job cetak (P2) |
| `erp-mfg-work-orders` + WO/Input/Output/Activity/RouteCard, Material Issue/Return, Production Entry | Ada (generik) | Kerangka job & konsumsi material (P2, P5) |
| Skema `erp-pln` (reorder policy, demand forecast, MRP run) | Ada | Pendukung penjadwalan (P3) |
| `erp-sls-packing-lists` (dengan posting service) | Ada | Basis packing (P6) |
| Freight Receivable/Payable (RP/PP) + jurnal | Live sejak Fase 1 | Biaya kirim & tagihan angkut (P7) |
| Order Hub (channel-ready) + A3 dokumen/BAST | Live | Ujung hilir job & bukti terima (P2, P7, P8) |
| Profil katalog D1 (`isCustomPrint`, kategori CETAK) | Live | Penanda item cetakan (P1, P2) |
| Menu `M6` Production | Root nonaktif; anak (BOM, WO) utuh | Diaktifkan bertahap per gelombang |

Yang **belum ada sama sekali**: mesin/estimasi cetak, master mesin & kapasitas, workflow pre-press, dataset variable-data, HPP per job, master kendaraan/trip, aplikasi sales lapangan, modul lot/batch.

## 2. Keputusan & data yang mengunci (open decisions §9 dokumen scope)

| # | Keputusan/data | Pengaruh | Asumsi provisional agar bisa mulai (§6) |
|---|---|---|---|
| 1 | Porsi omzet cetak vs distribusi | Bobot P1–P5 vs P6–P8 | Seimbang; urutan gelombang tidak berubah |
| 5 | Daftar mesin cetak + kapasitas/jam | P3 tidak bisa final tanpa ini | 2 mesin contoh bertanda provisional |
| 9 | Scope seragam: make-or-buy | Seragam masuk job cetak atau beli jadi | Beli jadi (tetap jalur pembelian Fase 1) |
| 10 | Barcode/alokasi eksklusif wajib di pilot? | Kedalaman P6 | Packing per kelas dulu, barcode menyusul |

Data klien yang diminta paralel (tidak memblokir): tarif standar pre-press/finishing, daftar mesin asli + kapasitas, format packing per sekolah, daftar kendaraan & wilayah kirim, daftar nama siswa per kelas (untuk VDP & packing per siswa — data asli hanya dari sekolah; contoh/dummy bertanda provisional untuk pengujian).

## 3. Workstream

Ukuran relatif kasar: S (kecil) · M (sedang) · L (besar) · XL (sangat besar).

### P1 — Estimasi cetak (M)
- **Tujuan:** dari spesifikasi (ukuran, halaman, kertas, warna, finishing, oplah) menghasilkan harga pokok estimasi + harga jual, dan menjadi sumber item CETAK pada penawaran.
- **Bangun:** tabel `mfg_print_estimates` + `mfg_print_estimate_lines` (komponen: kertas, tinta, plat, pre-press, finishing, makloon, overhead, margin); modul `erp-mfg-print-estimates`; halaman estimasi + aksi "jadikan penawaran" (mengisi quotation yang sudah ada); komponen biaya master sederhana (tarif per komponen, bisa provisional).
- **Dependensi:** D1 katalog (item CETAK), quotation Fase 1.
- **DoD:** satu estimasi contoh dihitung, diterbitkan ke penawaran, PDF penawaran A3 memuatnya; harga estimasi tersimpan sebagai snapshot (tidak berubah saat tarif berubah).

### P2 — Job cetak & pre-press (L)
- **Tujuan:** job sebagai satuan kerja cetak: lahir dari SO/penawaran yang disetujui, berjalan lewat tahap pre-press → cetak → finishing → QC → selesai, dengan konsumsi material riil.
- **Bangun:** perluasan WO generik dengan profil cetak (`mfg_print_jobs` menempel ke WO: spesifikasi cetak, file master, checklist pre-press, status tahap); route card per tahap memakai `ErpMfgWorkOrderRouteCard`; material issue/return yang ada dipakai apa adanya; hasil jadi masuk stok lewat Production Entry → item CETAK.
- **Dependensi:** P1 (estimasi menjadi acuan job), menu M6 diaktifkan.
- **DoD:** satu job E2E: SO cetak → job → issue kertas → produksi → barang jadi bertambah di gudang → job selesai; tahap pre-press tidak bisa dilewati tanpa checklist tercentang.

### P3 — Penjadwalan produksi (M)
- **Tujuan:** setiap job terjadwal di mesin dengan kapasitas; konflik terlihat sebelum terjadi.
- **Bangun:** master `mfg_machines` (nama, jenis, kapasitas/jam, jam kerja) — data awal provisional per §2; tabel jadwal `mfg_job_schedules` (job × mesin × rentang waktu); tampilan papan jadwal mingguan; validasi bentrok sederhana; MRP `pln` dipakai untuk cek kesiapan material job.
- **Dependensi:** P2; data mesin asli menyusul (jadwal tidak bergantung nilainya, hanya master-nya).
- **DoD:** job bisa dijadwalkan & digeser; dua job di mesin sama pada jam sama tertolak/terperingatkan.

### P4 — Variable data printing (M)
- **Tujuan:** job personalisasi (nama siswa/kelas/sekolah di rapor, sertifikat, buku induk) punya dataset teraudit: berapa baris, dari file apa, versi ke berapa.
- **Bangun:** `mfg_vdp_datasets` + `mfg_vdp_rows` (impor CSV/XLSX per job, validasi kolom wajib, jumlah baris = jumlah cetak variabel); dataset membekukan versi saat job mulai cetak; tidak mencetak file hasil — hanya data & hitungannya yang dikelola ERP.
- **Dependensi:** P2. Data siswa asli dari sekolah; untuk uji pakai data dummy bertanda provisional.
- **DoD:** impor dataset 1 kelas, qty job variabel = jumlah baris valid; dataset terkunci setelah tahap cetak mulai.

### P5 — HPP per job & makloon (L)
- **Tujuan:** biaya aktual per job terkumpul dan dibandingkan ke estimasi; inilah yang membuka "laba per job cetak" di E1 yang tertunda dari Fase 1.
- **Bangun:** akumulasi biaya dari material issue riil (harga bergerak), alokasi biaya tahap (tarif per tahap dari P3/route card), dan biaya makloon = jasa luar lewat PO jasa (modul pembelian yang ada, item jasa) tertaut ke job; jurnal WIP → barang jadi mengikuti pola posting Fase 1 (Dr/Cr seimbang, periode terbuka); laporan varians estimasi vs aktual per job; API/feed untuk E1 profit per job.
- **Dependensi:** P2 (wajib), P1 (pembanding), P3 (tarif tahap — bisa tarif flat provisional dulu).
- **DoD:** job selesai punya HPP aktual; E1 menampilkan laba job itu; selisih estimasi vs aktual terlihat per komponen.

### P6 — Packing per siswa (M)
- **Tujuan:** dari DO, packing dipecah per siswa/kelas/sekolah sehingga sekolah menerima per nama, bukan gelondongan.
- **Bangun:** perluas `erp-sls-packing-lists`: unit pack per siswa (nama, kelas, isi item), status packed per unit, cetak label per unit (memakai renderer dokumen A3); ringkasan per DO tetap ada untuk surat jalan.
- **Dependensi:** DO Fase 1; data siswa/kelas (bisa dari dataset P4 bila job cetak, atau impor tersendiri).
- **DoD:** satu DO ter-packing per siswa untuk 1 kelas contoh; DO tidak bisa POST bila ada unit wajib belum packed (mode peringatan dulu, pengetatan menyusul).

### P7 — Pengiriman & armada (M)
- **Tujuan:** pengiriman terencana sebagai trip: satu kendaraan, banyak DO/sekolah, status muat → berangkat → tiba, biaya trip tertelusur.
- **Bangun:** master kendaraan `sls_vehicles` (data provisional dulu), `sls_delivery_trips` + `sls_delivery_trip_stops` (DO per stop, urutan, status tiba + penerima — mengumpan BAST/acceptance A3); biaya trip dicatat dan bisa menjadi dasar Freight Payable; peta tidak dibangun (cukup alamat & urutan manual).
- **Dependensi:** DO + A3 BAST; RP/PP yang sudah live.
- **DoD:** satu trip contoh mengantar 2 DO ke 2 sekolah; acceptance di stop menggerakkan tahap Order Hub ke DITERIMA.

### P8 — Aplikasi sales lapangan (L)
- **Tujuan:** sales di lapangan bisa membuat order & mencatat kunjungan dari ponsel tanpa menunggu admin.
- **Bangun:** BUKAN aplikasi native terpisah — PWA responsif di frontend yang sama: layar ringkas buat SO (channel SALES, masuk Order Hub), catat kunjungan (menulis `md_school_activities` A1), lihat piutang sekolahnya. Login & role yang sama; mode offline penuh di luar scope (antre simpan sederhana bila memungkinkan).
- **Dependensi:** Order Hub + A1 (keduanya live).
- **DoD:** sales membuat SO dari ponsel → muncul di Order Hub sebagai BARU; kunjungan tercatat dan menggerakkan lastVisitAt sekolah.

### T1 — Lot/batch & FEFO (track teknis, M) — opsional/paralel
Mengikuti `docs/implementation-plan.md` Phase 2 (modul `erp-inv-lots`, alokasi FEFO, lot di GRN/DO/movement). Nilai utamanya untuk distribusi barang dengan tanggal kedaluwarsa/versi kurikulum; bukan syarat gerbang gelombang mana pun — jadwalkan bila distribusi membutuhkannya.

## 4. Urutan gelombang

| Gelombang | Isi | Gate keluar |
|---|---|---|
| G1 — Inti cetak | P1 → P2 (+ aktivasi menu M6) | Job cetak E2E pertama lulus uji |
| G2 — Biaya & kemas | P5, P6 | HPP job tampil di E1; packing per siswa dipakai 1 DO uji |
| G3 — Jadwal & personal | P3, P4 | Papan jadwal hidup; dataset VDP terkunci per job |
| G4 — Lapangan | P7, P8 | Trip pertama + order sales dari ponsel masuk Hub |

T1 berjalan paralel kapan pun ada kapasitas. Setiap gelombang ditutup dengan: uji E2E terdokumentasi, commit di repo ini + sinkronisasi live terverifikasi, dan pembaruan rekap progres.

## 5. Aktivasi menu & rollout

- G1: aktifkan root `M6` Production (migration data, pola Fase 1) — anak BOM/WO sudah aktif.
- Menu baru per workstream (Estimasi Cetak, Job Cetak, Jadwal Produksi, Mesin, Packing per Siswa, Armada/Trip) dibuat sebagai migration data dengan grant awal SUPERADMIN; peran lain dibuka setelah uji gelombang.
- Tidak ada menu Fase 3/4 yang diaktifkan pada fase ini.

## 6. Risiko & mitigasi

| Risiko | Mitigasi |
|---|---|
| Workflow cetak dipaksakan ke WO generik → bengkak & rapuh | Profil cetak sebagai lapisan di atas WO (P2), bukan fork WO |
| Utang sinkronisasi web-erp ↔ sentient-factory menumpuk lagi | Sinkronisasi masuk DoD tiap workstream; build + restart + smoke test sebelum commit SF |
| Data mesin/tarif/siswa asli terlambat | Semua bisa provisional bertanda (§6) kecuali data siswa untuk produksi nyata — VDP produksi menunggu data sekolah asli |
| Scope melebar ke Fase 3 (portal/payment) | Daftar "di luar Fase 2" di §8 dokumen scope berlaku penuh; permintaan portal dicatat, tidak dikerjakan |

## 7. Gerbang akhir Fase 2

Di akhir Fase 2, putuskan fondasi **yayasan** sesuai dokumen scope §7: bila tetap tidak ada pelanggan grup (satu yayasan ≥2 sekolah), yayasan tidak dibangun; bila ada, masuk Fase 3/B1 dengan model relasi opsional yayasan → banyak sekolah (transaksi tetap di sekolah).

## Progres

- **P1 Estimasi Cetak — SELESAI & LIVE (2026-10-04).** Tabel mfg_print_estimates(+lines) (migration 015), modul erp-mfg-print-estimates (CRUD + hitung server + konversi ke Penawaran SQ), menu M6 Production aktif + M6.TX.EST, halaman /manufacturing/print-estimates. Terverifikasi E2E: estimasi EST000001 biaya Rp800.000 + margin 25% -> harga Rp1.000.000 -> penawaran SQ000001 (data uji dibersihkan). Berikutnya: P2 Job Cetak & Pre-press (Gelombang 1).

- **P2 Job Cetak & Pre-press — SELESAI & LIVE (2026-10-05).** Tabel mfg_print_jobs (migration 016), modul erp-mfg-print-jobs: job membuat Work Order via service WO generik + profil cetak (tahap PRE_PRESS -> CETAK -> FINISHING -> QC -> SELESAI, checklist pre-press 5 item sebagai gerbang tahap CETAK, log tahap). Menu M6.TX.JOB, halaman /manufacturing/print-jobs. Terverifikasi E2E: gerbang menolak pindah tahap sebelum checklist lengkap (400), alur s/d SELESAI lulus, checklist terkunci setelah pre-press; data uji dibersihkan. Gelombang 1 SELESAI. Berikutnya Gelombang 2: P5 HPP per job & P6 Packing per siswa.

## Progres Gelombang 2 (2026-10-05)

- **P5 HPP per job — SELESAI & LIVE.** Migration `20261004_017_erp_job_costs`: tabel `mfg_job_cost_entries` (job_id, entry_date, cost_type enum MATERIAL/TENAGA_KERJA/OVERHEAD/MAKLOON/LAIN, stage, item/qty/unit_cost/amount server-computed) + kolom penanda posting di `mfg_print_jobs`. Modul `erp-mfg-job-costs`: CRUD entri (terkunci setelah jurnal diposting), ringkasan HPP (aktual vs estimasi P1: varians + %, HPP per eksemplar, margin vs harga estimasi), laporan HPP semua job (menu `M6.TX.JOBCOST` `/manufacturing/job-costs`), jurnal penyelesaian Dr 1133.01.001 Persediaan Barang Jadi / Cr 1132.01.001 Persediaan Barang Dalam Proses (akun dicari by code) via `buildLedgerRows` (source `PRODUCTION`, source_doc_type `mfg_print_jobs`, guard periode terbuka) + void jurnal (assert periode sumber + hapus baris + buka kunci). Panel "Biaya & HPP" tertanam di detail Job Cetak. Terverifikasi E2E live: estimasi 800.000 → aktual 780.000 (varians −2,5%, HPP/eks 1.560, margin 220.000), posting jurnal `WO…-HPP` balance, kunci entri aktif, void bersih; data smoke dihapus & counter EST/WO direset. Catatan desain: akumulasi biaya dicatat di entri job (bukan jurnal per entri); jurnal hanya saat penyelesaian. Tarik otomatis dari material issue + makloon via service PO = penyempurnaan lanjutan.
- **P6 Packing per siswa — SELESAI & LIVE.** Migration `20261004_018_erp_packing_units`: tabel `sls_packing_units` (packing_list_id, sequence_no, student_name, class_name, contents JSONB snapshot baris packing list, status enum PENDING/PACKED, packed_at/by) + menu `M5.TX.PACKUNITS` `/sales/packing-units`. Modul `erp-sls-packing-units`: overview packing list + progres, generate unit dari roster (array `students[]` atau teks CSV `Nama,Kelas`), toggle packed, hapus unit. Frontend: pemilih packing list + progres, textarea roster, tabel unit, ekspor CSV label. Terverifikasi E2E live: 3 unit dari roster CSV dengan isi snapshot item packing list, 2 ditandai PACKED → progres 2/3; data smoke dihapus. Catatan: gerbang "DO tidak bisa POST bila unit wajib belum packed" belum diterapkan (perlu keputusan mode warning/blocking) — v1 mode pantau saja.
- **GELOMBANG 2 SELESAI (P5 + P6).** Berikutnya Gelombang 3: P3 Penjadwalan produksi + P4 Variable Data Printing.

## Progres Gelombang 3 (2026-10-05)

- **P3 Penjadwalan produksi — SELESAI & LIVE.** Migration `20261005_019_erp_print_schedules`: tabel `mfg_machines` (jenis OFFSET/DIGITAL/FINISHING/LAIN, kapasitas/jam, jam kerja, status ACTIVE/MAINTENANCE/INACTIVE) + `mfg_job_schedules` (job × mesin × rentang waktu, tahap, status TERJADWAL/BERJALAN/SELESAI/BATAL, aktual mulai/selesai). Master mesin di-seed **2 unit contoh bertanda provisional** (MCN-OFFSET-01 8.000 lbr/jam, MCN-DIGITAL-01 1.200 lbr/jam, `legacy_code='provisional'`, nama berprefix [PROVISIONAL]) sesuai asumsi rencana §2 #5 — diganti daftar mesin asli klien bila tersedia. Modul `erp-mfg-schedules`: CRUD mesin + jadwal dengan **validasi bentrok keras** (jadwal aktif tumpang tindih pada mesin sama → 400 + rincian job penabrak), geser jadwal (cek ulang), transisi status. Halaman `/manufacturing/schedules` (menu `M6.TX.SCHED`): papan jadwal per mesin dengan filter rentang tanggal. E2E live terverifikasi: jadwal dibuat, bentrok ditolak 400, geser OK, status mengalir ke SELESAI dengan aktual tercatat. Belum: cek kesiapan material via MRP `erp-pln` (penyempurnaan lanjutan).
- **P4 Variable Data Printing — SELESAI & LIVE.** Migration `20261005_020_erp_vdp_datasets`: tabel `mfg_vdp_datasets` (per job: nama, file sumber, versi, kolom + kolom wajib, hitungan baris/valid, status DRAFT/TERKUNCI) + `mfg_vdp_rows` (data JSONB per baris, is_valid + error_note). Modul `erp-mfg-vdp`: buat dataset, impor baris via CSV (pemisah ,/;) atau `rows[]` — menggantikan baris lama & menaikkan versi; validasi kolom wajib per baris; daftar baris terpaginasi + filter invalid; kunci manual. **Kait pembekuan:** layanan Job Cetak kini mengunci semua dataset DRAFT job begitu tahapnya masuk CETAK; dataset terkunci menolak impor/ubah/hapus (400). Halaman `/manufacturing/vdp` (menu `M6.TX.VDP`). E2E live terverifikasi: impor 4 baris → 3 valid/1 invalid ("Kolom kosong: Nama"), impor ulang → versi 2 semua valid, job ke CETAK → dataset TERKUNCI, impor saat terkunci ditolak 400. Sesuai rencana: ERP hanya mengelola data & hitungannya, file hasil cetak tidak dibuat. Data siswa uji = dummy smoke, sudah dihapus; data produksi hanya dari sekolah.
- **GELOMBANG 3 SELESAI (P3 + P4).** Berikutnya Gelombang 4: P7 Pengiriman & armada + P8 Aplikasi sales lapangan (PWA).


## Progres Gelombang 4 (2026-10-05) — Fase 2 TUNTAS

- **P7 Pengiriman & armada SELESAI & live.** Migration `20261005_021`: tabel
  `sls_vehicles`, `sls_delivery_trips`, `sls_delivery_trip_stops`; penomoran
  TRP; menu M5.TX.TRIP. **2 kendaraan contoh provisional** menunggu daftar
  armada asli. Trip: satu kendaraan, banyak DO sebagai stop berurutan,
  status DRAFT → MUAT → BERANGKAT → SELESAI; biaya BBM/tol/lain tercatat
  (dasar Freight Payable). **Acceptance terverifikasi E2E**: stop ditandai
  TIBA dengan nama penerima → acceptance tertulis di Delivery Report (BAST)
  → tahap Order Hub kedua order contoh bergerak ke **DITERIMA**. DO yang
  sudah masuk trip aktif tidak bisa dipakai trip lain.
- **P8 Aplikasi sales lapangan SELESAI & live.** Bukan aplikasi terpisah:
  halaman mobile-first `/sales/field` di frontend yang sama (menu M5.TX.FIELD,
  migration `20261005_022`) + manifest PWA + service worker berlingkup
  `/app/sales/` (API tidak pernah di-cache). Sales mencatat kunjungan
  (endpoint aktivitas A1 — terverifikasi `lastVisitAt` sekolah bergerak),
  membuat order cepat ber-channel **SALES** (terverifikasi muncul di Order
  Hub sebagai **BARU**), dan melihat piutang sekolahnya. Kunjungan yang
  gagal terkirim masuk antrean lokal (localStorage) dan disinkronkan saat
  online. Mode offline penuh di luar scope sesuai rencana.
- **Fase 2 selesai: 8/8 workstream (P1–P8) live.** Sisa tindak lanjut yang
  tercatat: cek kesiapan material via MRP `erp-pln` (P3), tarik biaya aktual
  otomatis dari material issue + makloon via service PO (P5), gerbang
  DO-post untuk packing wajib (P6), ganti data provisional (mesin,
  kendaraan, supplier, harga) dengan data asli klien, dan **gerbang
  keputusan yayasan di akhir Fase 2** sebelum masuk Fase 3 (rencana Fase 3
  sudah ada: `docs/fase-3-rencana-kerja.md`). Track T1 (lot/batch & FEFO)
  tetap opsional/paralel.

## Keputusan Gerbang Akhir Fase 2 (2026-10-05)

Pemilik memutuskan: **yayasan tidak dibangun — permanen; per-sekolah saja.**
Gerbang di §6 dokumen ini dengan demikian tertutup: Fase 3 berjalan tanpa
entitas/relasi yayasan, dan W7 pada `docs/fase-3-rencana-kerja.md` berlaku
tanpa komponen B1. Sekolah tetap entitas utama untuk semua transaksi,
harga, dan portal.
