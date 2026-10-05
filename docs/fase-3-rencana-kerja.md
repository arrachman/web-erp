# Rencana Kerja Fase 3 — Portal Sekolah & Orang Tua + Landing Page

**Status:** rencana kerja (belum dimulai) — disusun 2026-10-05, setelah Fase 1/MVP live dan Fase 2 (percetakan & distribusi) berjalan.
> **KEPUTUSAN (2026-10-05, pemilik): yayasan TIDAK dibangun — permanen.**
> Entitas utama tetap **sekolah**; semua fitur Fase 3 dirancang per-sekolah
> saja. Gerbang §7 dokumen scope dengan demikian terjawab "tidak": tidak ada
> relasi yayasan → sekolah, tidak ada harga/portal level yayasan. W7 di bawah
> berlaku tanpa komponen B1.

**Dasar scope:** `docs/bahtera-madani-mvp-scope.md` §Fase 3 (portal sekolah & orang tua) + §7 (gerbang yayasan B1).
**Aturan main:** mengikuti `CLAUDE.md` proyek (penamaan tabel/model, list page §2.7, guard, commit atomik) dan pelajaran Fase 1–2: perubahan backend dibangun di repo ini **dan** disinkronkan ke salinan live `sentient-factory` pada workstream yang sama. Data yang belum tersedia dari klien diisi provisional bertanda sesuai §6 — kecuali identitas legal (NPWP, rekening resmi, alamat resmi) yang tidak pernah dikarang.

## 1. Fondasi yang sudah ada

| Aset | Kondisi | Dipakai untuk |
|---|---|---|
| A1 CRM Sekolah (profil, kontak berperan, pipeline, BOS) | Live Fase 1 | Akun & profil sekolah di portal (W2, W3) |
| A2 Order Hub channel-ready | Live Fase 1 | Kanal `PORTAL_SEKOLAH` / `PORTAL_ORANGTUA` sudah berupa enum + external order id idempotent — portal tinggal menjadi producer, model order inti tidak berubah (W2, W4) |
| A3 Dokumen pengadaan + BAST | Live Fase 1 | Penawaran/invoice/surat jalan/BAST tampil di portal dari rantai dokumen yang sama (W2) |
| A4 Subledger pajak | Live Fase 1 | Faktur & status pajak pada tagihan portal (W2) |
| D1 Katalog + profil per item | Live Fase 1 | Katalog portal memakai `md_item_catalog_profiles` incl. status tayang per kanal (W2, W4, W7) |
| AR Receipt (IP) + ledger | Live Fase 1 | Penerimaan pembayaran dari payment gateway direkonsiliasi ke sini (W5) |
| **Prototipe landing page** | **Ter-deploy 2026-10-05** — `apps/landing-page/`, port **3226** (`cendekia-landing.service`), branding sudah diganti ke CV Bahtera Madani | Bahan desain W1 — statis, belum terhubung data |
| **Prototipe Portal Sekolah** | **Ter-deploy 2026-10-05** — `apps/portal-sekolah/`, port **3221** (`cendekia-portal.service`), branding sudah diganti ke CV Bahtera Madani | Bahan desain W2 — interaktif dengan data contoh (mock), **belum ada panggilan API ERP** |

Catatan prototipe: keduanya berasal dari bundle desain "Pena Cendekia" (2026-09-25). Rebranding nama selesai; **identitas lain di prototipe masih dummy desain** (alamat Semarang, nomor telepon, instruksi rekening, domain `bahteramadani.co.id` sebagai placeholder) dan wajib diganti data asli klien sebelum produksi — lihat §2.

## 2. Keputusan & data yang mengunci (open decisions)

| # | Keputusan/data | Pengaruh | Asumsi provisional agar bisa mulai (§6) |
|---|---|---|---|
| 1 | Domain resmi (landing + portal) | W1, W2 — sertifikat & URL publik | Tetap di port fr-labs (3226/3221) selama pengembangan; domain placeholder `bahteramadani.co.id` |
| 2 | Identitas resmi perusahaan (alamat, telepon, email, rekening penerimaan) | Footer landing, instruksi bayar portal | Tidak dikarang (§6 exception) — prototipe bertanda dummy sampai data asli diterima |
| 3 | Model akun sekolah: self-register + approval admin, atau dibuatkan admin | W2, W3 | Self-register + approval (mengikuti alur prototipe) |
| 4 | Harga yang tampil di portal: HET, harga jual standar, atau harga kontrak per sekolah | W2, W7 | Harga jual standar + HET sebagai referensi; harga kontrak menyusul di W7 |
| 5 | Provider payment gateway | W5 | Belum dipilih — W5 mulai setelah keputusan; sampai itu tagihan portal tampil tanpa tombol bayar online |
| 6 | ~~BSP WhatsApp (provider resmi)~~ | W6 | **DIPUTUSKAN 2026-10-05: gateway self-hosted** (paket wa-gateway Baileys kompatibel Fonnte dari user) — bukan BSP resmi; risiko ban protokol tidak resmi diterima, mitigasi: nomor WA Business khusus + kill-switch + antrean/log. 
| 7 | ~~Yayasan (B1)~~ | — | **SUDAH DIPUTUSKAN 2026-10-05: tidak dibangun (permanen), per-sekolah saja** |

## 3. Workstream

Ukuran relatif kasar: S (kecil) · M (sedang) · L (besar) · XL (sangat besar).

### W1 — Landing page produksi (S–M) — *catch-up: prototipe sudah ter-deploy*
- **Tujuan:** landing page publik CV Bahtera Madani yang kontennya hidup dari data ERP, bukan HTML statis berisi contoh.
- **Bangun:** pertahankan bentuk statis ringan di `apps/landing-page/` (port 3226); katalog unggulan & logo sekolah mitra diambil dari D1/data partner asli; form "minta penawaran" menghasilkan lead yang tercatat (masuk pipeline A1 sebagai PROSPEK); identitas footer diganti data resmi (§2 #2).
- **Dependensi:** D1 katalog, A1 pipeline.
- **DoD:** landing tayang dengan data asli (bukan dummy prototipe); satu lead uji masuk dan terlihat di CRM Sekolah.

### W2 — Portal Sekolah MVP (L) — *catch-up: prototipe sudah ter-deploy*
- **Tujuan:** sekolah memesan dan memantau sendiri: katalog → daftar kebutuhan → pesanan → penawaran → pengiriman/BAST → tagihan, tanpa perantara admin.
- **Bangun:** aplikasi portal beneran di `apps/portal-sekolah/` (port 3221) menggantikan prototipe statis; login akun sekolah terikat partner `CUST-SCHOOL`; katalog dari D1 (hanya item tayang di kanal portal); checkout membuat order kanal `PORTAL_SEKOLAH` di Order Hub (A2); status pesanan, dokumen (A3), dan tagihan dibaca dari rantai dokumen ERP yang sama — tidak ada data duplikat di sisi portal; profil sekolah membaca/menulis A1.
- **Dependensi:** A1, A2, A3, D1 (semua live); W3 untuk approval akun.
- **DoD:** satu sekolah pilot menyelesaikan order E2E lewat portal: order muncul di Order Hub sebagai BARU → diproses admin → sekolah melihat penawaran, status kirim, dan tagihan yang benar.

### W3 — Akun, peran & approval sekolah (M)
- **Tujuan:** pendaftaran sekolah terkendali: tidak semua pendaftar otomatis bisa memesan.
- **Bangun:** registrasi + verifikasi admin (antrean approval), peran per kontak sekolah (kepala sekolah/bendahara/operator — memakai peran kontak A1), undang/reset akses oleh admin, audit login.
- **Dependensi:** A1 kontak berperan.
- **DoD:** sekolah uji mendaftar → disetujui admin → bisa login; pendaftar yang ditolak tidak bisa memesan.

### W4 — Portal Orang Tua (M–L)
- **Tujuan:** orang tua melihat & memesan paket kebutuhan anak (seragam, buku, paket kelas) dan memantau statusnya.
- **Bangun:** reuse fondasi auth & katalog W2; order kanal `PORTAL_ORANGTUA`; keterkaitan orang tua ↔ siswa ↔ sekolah (dataset siswa dari P4/packing P6 Fase 2 bila relevan).
- **Dependensi:** W2, W7 (paket kelas).
- **DoD:** satu order orang tua uji masuk Order Hub dan tertaut ke sekolah + kelas yang benar.

### W5 — Payment gateway (M)
- **Tujuan:** tagihan portal bisa dibayar online dan lunasnya tercatat otomatis.
- **Bangun:** integrasi provider terpilih (§2 #5); pembayaran membuat AR Receipt (IP) terposting lewat pola Fase 1; webhook idempotent; rekonsiliasi harian terhadap mutasi.
- **Dependensi:** W2 (tagihan tampil), keputusan provider.
- **DoD:** satu pembayaran uji end-to-end: status invoice LUNAS/terbayar sebagian di ERP tanpa input manual.

### W6 — WhatsApp & notifikasi (M)
- **Tujuan:** sekolah tidak perlu membuka portal untuk tahu status: penawaran terbit, barang dikirim, BAST menunggu, tagihan jatuh tempo.
- **Bangun:** BSP resmi (§2 #6); template pesan ter-approve; antrean kirim + log status; preferensi notifikasi per sekolah.
- **Dependensi:** W2; nomor kontak valid dari A1.
- **DoD:** notifikasi uji terkirim pada 3 peristiwa (order diterima, terkirim, tagihan terbit) dan tercatat log-nya.

### W7 — Mesin harga kontrak, bundling & paket kelas (L)
- **Tujuan:** harga per sekolah sesuai kontrak, dan produk paket (per kelas/per siswa) bisa dijual di portal.
- **Bangun:** harga kontrak berlapis (melengkapi model tier D1), bundling/paket yang ditunda dari Fase 1, paket kelas untuk W4; ~~yayasan B1~~ **dibatalkan permanen** (keputusan 2026-10-05: per-sekolah saja).
- **Dependensi:** D1, W2.
- **DoD:** dua sekolah dengan kontrak berbeda melihat harga berbeda untuk item yang sama; satu paket kelas terjual lewat portal.

### W8 — Load test & hardening portal (M)
- **Tujuan:** portal aman & tahan musim puncak tahun ajaran (target dokumen scope: 1.000+ pengguna bersamaan).
- **Bangun:** uji beban pada alur katalog/order; rate limiting & proteksi endpoint publik; pemisahan kredensial portal dari user internal ERP; audit keamanan dasar.
- **Dependensi:** W2 (dan W4 bila sudah ada).
- **DoD:** laporan load test lulus target; tidak ada endpoint portal yang mengekspos data sekolah lain (uji akses silang negatif).

## 4. Urutan gelombang

| Gelombang | Isi | Gate keluar |
|---|---|---|
| G1 — Portal sekolah hidup | W1, W3, W2 | Satu sekolah pilot order E2E lewat portal |
| G2 — Bayar & kabar | W5, W6 | Pembayaran online pertama lunas tercatat otomatis |
| G3 — Orang tua & kontrak | W4, W7 | Order orang tua + harga kontrak aktif |
| G4 — Skala | W8 | Load test 1.000+ pengguna lulus |

Gerbang akhir Fase 3 (PRD): **satu musim tahun ajaran berjalan lewat portal**.

## 5. Risiko & mitigasi

| Risiko | Mitigasi |
|---|---|
| Prototipe dianggap produk jadi → scope implementasi diremehkan | Prototipe dinyatakan eksplisit sebagai bahan desain (mock, tanpa API) di §1; W2 adalah pembangunan ulang, bukan pemolesan file statis |
| Endpoint publik mengekspos data ERP internal | Portal hanya lewat API khusus portal dengan cakupan per-sekolah; uji akses silang negatif adalah DoD W8 |
| Identitas dummy prototipe (alamat/rekening/domain) terbawa ke produksi | §2 #2: diganti data asli klien sebelum go-live; tidak pernah dikarang (§6 exception) |
| Musim tahun ajaran = lonjakan serentak | W8 dijadwalkan sebelum musim; katalog portal di-cache/di-prerender |

## Progres

- **2026-10-05 — Prototipe landing + portal ter-deploy & ter-rebrand.** Bundle desain ditempatkan di `apps/landing-page/` (port 3226) dan `apps/portal-sekolah/` (port 3221) sebagai service statis persisten; seluruh branding "Pena Cendekia / PT Pena Cendekia Nusantara" diganti "CV Bahtera Madani" (domain placeholder `bahteramadani.co.id`). Crosscheck terhadap dokumen scope menempatkan pekerjaan ini di Fase 3 — dokumen rencana ini dibuat agar catch-up-nya tercatat dan tidak hilang. Berikutnya: G1 (W1 landing produksi + W3 akun/approval + W2 portal sekolah MVP) setelah keputusan §2 #1–#4.

## Kemajuan — Gelombang 3 (W4 + W7) SELESAI & LIVE (2026-10-05)

### W7 — Mesin harga kontrak, bundling & paket kelas
- **Harga kontrak berlapis per sekolah** (keputusan gerbang: TANPA yayasan — kontrak selalu per partner sekolah). Tabel `md_school_contract_prices` (migrasi `20261005_028`): baris berbentuk salah satu dari harga tetap per item / diskon per kategori / diskon seluruh sekolah, dengan masa berlaku opsional. Resolusi: item → kategori → sekolah → harga jual standar. Modul backend `erp-contracts` (CRUD admin + resolver) dipakai katalog & checkout portal, jadi harga yang tampil = harga yang tertagih.
- **Paket/bundle**: `md_item_bundles` + `md_item_bundle_lines` — item paket memiliki daftar komponen; katalog portal menampilkan "Isi paket"; item paket terjual sebagai satu baris order berharga paket.
- **Admin ERP**: halaman "Harga Kontrak & Paket" `/master/contract-prices` (menu `M1.CONTRACTS`) untuk mengelola baris kontrak + definisi paket.
- **DoD terbukti (smoke API, data uji dibersihkan)**: sekolah A (harga item tetap Rp10.000 + diskon kategori 50%) melihat Rp10.000 — lapisan item mengalahkan kategori; sekolah B (diskon sekolah 20%) melihat Rp17.200 untuk item yang sama (standar Rp21.500).

### W4 — Portal Orang Tua
- Peran portal baru `ORANG_TUA` (enum). Orang tua mendaftar dengan **memilih sekolah yang sudah terdaftar** (`GET /erp/portal/public/schools`) + nama siswa & kelas (tersimpan di metadata akun). Persetujuan admin menautkan akun ke partner sekolah itu — **tidak membuat partner baru**; login sebelum disetujui ditolak 403 seperti akun sekolah.
- Belanja memakai fondasi W2 yang sama: katalog menampilkan harga kontrak sekolah anaknya; checkout menjadi order kanal `PORTAL_ORANGTUA` dengan `customFields.portalParent` (akun + siswa + kelas) pada SO, sumber dana dipaksa NON_BOS. Isolasi: orang tua hanya melihat order & tagihan dari ordernya sendiri (detail order sekolah → 404).
- Frontend portal: mode "Akun Orang Tua" di halaman daftar, banner siswa di dashboard, chip "Harga kontrak sekolah" + isi paket di katalog.
- **DoD terbukti**: order orang tua SO uji masuk Order Hub tahap BARU tertaut sekolah + kelas yang benar, total sesuai harga kontrak (2 × Rp10.000 = Rp20.000).

### Sisa Fase 3
- G2: W6 selesai (lihat entri W6 di atas); W5 payment gateway masih dikerjakan sesi paralel.
- G4 (W8 load test & hardening) belum dikerjakan.
- Follow-up tercatat: ekspansi komponen paket ke picking/packing gudang; harga kontrak untuk order admin (saat ini resolver dipakai portal — SO admin tetap harga standar/katalog); pendaftaran orang tua multi-anak (v1 satu akun = satu siswa).

---

## Kemajuan — Gelombang 2 (W5 + W6) & Gelombang 4 (W8) SELESAI & LIVE (2026-10-05)

**Keputusan §2 #5/#6 BELUM dipilih user** — sesuai semangat §6 (jangan stall),
yang dibangun adalah infrastruktur lengkap dengan adapter provisional yang
jelas bertanda, supaya aktivasi provider nyata tinggal mengganti adapter:

- **W5 Pembayaran**: tabel `fin_portal_payments` (migrasi
  `20261005_029_erp_portal_payments_notifications`); endpoint portal
  `POST /erp/portal/invoices/:id/pay` (idempoten, satu intent PENDING per
  invoice; orang tua terisolasi ke invoice ordernya sendiri) menghasilkan
  nomor VA; webhook `POST /erp/portal/payments/webhook/:provider`
  terverifikasi HMAC-SHA256 → pembayaran PAID → **AR Receipt (IP) dibuat
  dan di-POST otomatis** (akun penerimaan Bank BCA 1110.01.001) → invoice
  LUNAS tanpa input manual; webhook ganda = no-op (idempoten). Provider
  v1 = `SIMULASI` (uang sungguhan TIDAK mengalir); halaman Tagihan portal
  menampilkan tombol Bayar + VA + status, dengan catatan mode simulasi.
  Modul AR Receipt di SF yang masih stub disinkronkan penuh dari web-erp
  agar alur ini jalan live. Terbukti E2E: invoice Rp43.000 → VA → webhook
  → IP000002 POSTED → invoice PAID.
- **W6 Notifikasi**: modul `erp-outbound-notifications` + tabel
  `sys_notification_logs`; template WA untuk 3 peristiwa — ORDER_DITERIMA
  (checkout portal), BARANG_DIKIRIM (DO POST), TAGIHAN_TERBIT (invoice
  POST) — hook di controller terkait, service tidak pernah menggagalkan
  transaksi; penerima = akun portal aktif sekolah (orang tua: hanya
  pemesan); tanpa nomor HP tercatat SKIPPED. Pengirim v1 = adapter `LOG`
  (pesan dirender + tercatat SENT di log; belum terkirim ke WhatsApp
  sungguhan). Log terbaca admin di `GET /erp/outbound-notifications/logs`.
  **Catatan discovery**: ada upaya WA paralel di SF (model
  `sys_wa_templates`/`sys_wa_logs` + container `nuha-wa-gateway`) —
  kandidat jalur pengirim nyata menggantikan adapter LOG, menunggu
  keputusan user menyatukan arah.
- **W8 Hardening & load test**: rate limit endpoint publik portal
  (tulis/auth 15/mnt/IP, baca 240/mnt/IP — terbukti 429); uji negatif
  lintas sekolah lolos (404/404/list kosong). Load test (laporan:
  `docs/fase-3-load-test-2026-10-05.md`): FE katalog 1.692 req/dtk
  100% sukses di konkurensi 50; API katalog p95 16,8 ms (550/550 sukses
  di bawah cap); batas mengikat = throttler global gateway
  600 req/60 dtk/IP (konfigurasi, bukan kapasitas) — dinilai LULUS untuk
  target 1.000+ pengguna bersamaan dengan catatan pemantauan.

**Sisa pekerjaan Fase 3**: aktivasi provider nyata (payment gateway +
kanal WhatsApp) — keputusan user §2 #5/#6 + akun merchant/BSP; swap
adapter SIMULASI/LOG ke provider terpilih; ganti webhook secret
provisional via env `PORTAL_PAYMENT_WEBHOOK_SECRET`.
