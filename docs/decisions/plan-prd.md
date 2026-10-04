# DECISIONS — Plan PRD rantai dokumen (Fase 0–4) + scope MVP Bahtera Madani

> Bagian dari decision log Web-ERP. Dipindahkan dari `DECISIONS.md`
> (2026-10-04) agar file indeks ramping; **isi entri tidak diubah** dan
> nomor `§` dipertahankan sebagai anchor stabil. Indeks semua entri:
> [`DECISIONS.md`](../../DECISIONS.md).

---

## § Plan — PRD Pengembangan ERP (rantai dokumen + anti posting ganda) (2026-10-01)

Sumber: `temp/PRD Pengembangan ERP.md` (Oct 1, 2026, @Patungin). Plan 4 fase
high-level, disetujui user (gate per-fase untuk transaksi "belum didefinisikan",
bukan skip total). Tujuan: saldo stok & GL selalu cocok dengan dokumen sumber
tanpa posting ganda, rantai dokumen terlacak, 5 modul transaksi terdefinisi.

**Status existing sebelum plan ini (jangan rebuild):** SO (Sales Order) sudah
full (form+grid+backend, TIDAK posting GL — lihat § Sales Order). Purchasing
M4 baseline forms sudah ada (§ Purchasing M4). CR/CD/RM/SM (Kas Masuk/Keluar,
Bank Masuk/Keluar) sudah full shared cash-bank engine (§ Kas Masuk dst).
Config grid + Form Builder sudah ada untuk semua famili (§ Kustomisasi Grid,
§ Form Builder). Fase di bawah **melanjutkan**, bukan mengulang, fondasi ini.

### Fase 0 — Fondasi rantai dokumen & anti posting ganda (gate pertama)

Semua modul bergantung pola yang sama → dikerjakan sekali, dipakai semua fase
berikutnya. Tidak menunggu jawaban open question (berlaku generik):

1. Model referensi dokumen sumber→turunan generik (per baris, bukan per
   header) + perhitungan sisa outstanding (qty & nominal) real-time.
2. State machine status 5-nilai PRD (Draft/Diajukan/Posted/Closed/Batal) —
   **selaraskan** dengan `ErpDocumentStatus` 7-nilai existing (lihat § Kas
   Masuk), jangan reintroduce varian baru; petakan Diajukan→NEED_APPROVE,
   Batal→VOID/CANCELLED.
3. Aturan anti posting ganda sebagai *backend guard* reusable: dokumen turunan
   dari sumber yang sudah posting stok/jurnal → field gudang/qty/akun dikunci,
   tidak memicu posting baru. Implementasi di layer backend (bukan FE-only
   validation).
4. Pembatalan generik: jurnal & mutasi stok balik bertanggal pembatalan,
   diblok bila (a) sudah punya turunan belum dibatalkan, (b) periode tertutup.
5. Laporan rekonsiliasi kontrol: subledger AR/AP vs akun kontrol GL, stok
   sistem vs akun kontrol persediaan — dibutuhkan untuk migrasi data lama juga.

**Gerbang Fase 0 → Fase 1:** FR-FIN-04 (tutup periode blok posting baru) dan
FR-FIN-06 (opening balance CoA seimbang dgn opening AR/AP/stock) harus lulus
test sebelum fase berikutnya mulai memposting dokumen nyata.

### Fase 1 — Sales core (dokumen yang aturannya sudah jelas di PRD)

Urutan sesuai rantai: SQ → SO (done) → AS/IP (posting jurnal, tanpa stok) →
PI/PL (dokumen bantu, tanpa posting) → DO (posting stok keluar) → DR (stok
hanya jika gudang transit — **butuh keputusan FR-SLS-03 sebelum DR dibangun**)
→ SI (posting jurnal+stok bila tanpa DO; jurnal-only bila dari DO, via Fase 0
guard) → RNR (stok masuk retur) → SR (jurnal; stok hanya jika tanpa RNR) →
IC (tanpa posting, agregator) → PV (jurnal pelunasan, bisa multi-SI + kurs).
Opening AR Balance paralel dengan SI/PV (FR-FIN-06 dependency).

**Gate:** RP (Freight Receivable) **di luar Fase 1** — tunggu jawaban "apakah
ongkos kirim masuk HPP" (PRD pertanyaan terbuka). SIE (Invoice Swap) **di luar
Fase 1** — tunggu klarifikasi makna (tukar faktur ke pelanggan lain vs ganti
faktur salah).

### Fase 2 — Purchasing core (paralel dengan Fase 1 setelah Fase 0 selesai)

PR → RFQ → BS (sekaligus putuskan nasib RQ: gabung RFQ atau BS — PRD
pertanyaan terbuka, **putuskan sebelum BS dibangun**) → PO → AP (jurnal uang
muka) → GRN (posting stok masuk; FR-FIN perlu klarifikasi: GRN posting jurnal
persediaan penuh atau qty-only — **gate sebelum GRN**) → PI Purchasing (jurnal;
stok hanya jika tanpa GRN) → DNR (stok keluar retur) → PRT (direct: jurnal
langsung potong PI; undirect: masuk saldo VPP) → VPP (agregator, tanpa
posting) → VP (jurnal pelunasan vendor, perlu konfirmasi FR-FIN-03 soal
Bank Disbursement otomatis). Opening AP Balance paralel.

**Catatan penamaan:** kode `PI` dipakai dua kali (Proforma Invoice Sales vs
Purchase Invoice Purchasing) — PRD tandai sebagai pertanyaan terbuka. Default
kerja: **disambiguasi secara internal** (field/kode berbeda di DB, mis.
`SLS_PI` vs `PUR_PI`), label menu tetap sesuai modul masing-masing — eskalasi
ke user hanya bila ditemukan konflik penomoran nyata saat implementasi.

**Gate:** PP (Freight Payable) **di luar Fase 2**, sama alasan dengan RP.

### Fase 3 — Warehouse, Finance lanjutan, Production

- **Warehouse:** MR → TS (keluar gudang asal) → RS (masuk gudang tujuan,
  terima sebagian — FR-WH-02) → SP (hitung fisik) → SA (dari SP: auto-selisih,
  FR-WH-03) → IB (opening stock, paralel dgn Fase 0 gate). Metode valuasi
  (rata-rata tertimbang vs FIFO, FR-WH-05 + pertanyaan terbuka) **wajib
  diputuskan sebelum SA/GRN pertama posting nilai** — pengaturan sekali,
  dipakai semua transaksi stok.
  - **Gate, di luar Fase 3:** PA (Price Adjustment), RF (Fuel Refill), DC
    (Time Sheet/Daily Check), RW (Receipt Weigher) — semua "belum
    didefinisikan" di PRD, proses bisnis harus dikonfirmasi dulu.
- **Finance lanjutan:** General/Adjustment/Memorial Journal, Receipt/Send
  Giro + Clearing (RM/SM sudah ada, lengkapi clearing jika belum), FX
  Revaluation (FR-FIN-05), Opening Balance CoA, Cash/Bank Transfer.
  - **Gate:** Receipt Memo / Send Memo — tunggu klarifikasi beda dengan
    SR/PRT/jurnal umum (pertanyaan terbuka).
- **Production (seluruhnya gated):** BOM lalu WO — PRD eksplisit bilang
  "alurnya belum ada di diagram kerja, usulan awal untuk didiskusikan".
  **Jangan mulai sebelum FR-PRD-01..05 dikonfirmasi user**, khususnya versi
  BOM dan sumber WO (manual vs dari SO).

### Fase 4 — Non-fungsional, migrasi, rilis

1. Hak akses per menu+aksi+cabang/gudang (perluas `adm_role_menus` existing
   bila belum granular per-aksi).
2. Approval per jenis dokumen dengan batas nominal per approver.
3. Jejak audit (siapa/kapan/nilai lama-baru) — cek `sys_audit_logs` existing,
   perluas cakupan ke semua transaksi baru di fase 1-3.
4. Penomoran tanpa nomor lompat saat concurrent save — audit
   `sys_document_numberings` existing untuk locking yang benar.
5. Label menu: kode lama tampil di samping nama baru selama masa transisi
   (mitigasi risiko "salah menu").
6. Jalankan laporan rekonsiliasi (dari Fase 0) terhadap data lama sebelum
   migrasi, koreksi lewat SA/jurnal penyesuaian.
7. Cetak per dokumen (Report Studio existing, § Desainer Laporan — reuse).
8. Impor saldo awal dari spreadsheet dengan validasi.

### Pertanyaan terbuka PRD yang jadi gate (ringkas, jawaban = prasyarat fase terkait)

| Pertanyaan | Blocking fase/transaksi |
| --- | --- |
| DO & SI sama-sama gerak stok — aturan "dokumen pertama posting" berlaku? | Fase 1 (SI) |
| GRN posting jurnal persediaan penuh atau qty-only? | Fase 2 (GRN) |
| DR ubah stok kapan — ada gudang transit? | Fase 1 (DR) |
| RQ digabung RFQ, BS, atau dihapus? | Fase 2 (BS) |
| RP/PP — ongkos kirim masuk HPP? | Fase 1 (RP) / Fase 2 (PP), keduanya di luar scope sampai dijawab |
| SIE = tukar faktur ke pelanggan lain atau ganti faktur salah? | Fase 1 (SIE), di luar scope sampai dijawab |
| Proses bisnis RF/DC/RW? | Fase 3 Warehouse gate, di luar scope sampai dijawab |
| PV/VP otomatis bikin Bank Receipt/Disbursement? | Fase 2 (VP), Fase 1 (PV) soft-gate |
| Beda Receipt Memo/Send Memo vs SR/PRT/jurnal umum? | Fase 3 Finance, di luar scope sampai dijawab |
| Valuasi stok: rata-rata tertimbang atau FIFO? | Fase 3 (SA, IB), dan transitif Fase 1/2 (SI/GRN yang posting nilai) |
| Kode PI dipakai 2x (Sales vs Purchasing) — perlu dibedakan? | Default: disambiguasi internal, eskalasi hanya bila konflik nyata |
| Nama "Payment Receipt" pindah dari PV ke IP — komunikasi ke user lama? | Fase 4 (label transisi) |

Saat fase terkait tiba dan jawaban belum ada → **stop, tanya user** (skill
`erp` disiplin interaksi #1), jangan asumsikan diam-diam.

### Koreksi premis plan setelah riset kode nyata (2026-10-01, sore)

Riset lapangan (bukan asumsi) menemukan premis Fase 0 di atas **salah**:
backend untuk **hampir semua ~40 transaksi PRD sudah punya folder
service/controller nyata** di `apps/api-gateway/src/erp-{sls,pur,inv,fin}-*`
(990-1045 baris per modul Sales), termasuk Invoice Swap (SIE) dan Freight
Payable (PP) yang PRD tandai "belum didefinisikan". DECISIONS.md historis
(berhenti di SO + Purchasing baseline, 2026-06-02/03) sudah tertinggal jauh
dari kode aktual.

**Temuan konkret (jangan rebuild, lanjutkan):**
- `ErpDocumentStatus` 11-nilai + `adm_role_doc_policies` (role→allowed status
  per document_type) sudah ada sejak migration `20260607_00x` — ini infra
  approval Fase 4, sudah lebih maju dari rencana.
- Pola posting GL generik **sudah established**: `CashBankPostingService`
  (ad-hoc per modul) dan `erp-inv-gl/inv-gl-posting.helpers.ts`
  (`buildLedgerRows`/`LedgerBase`/`LedgerLeg`/`reverseInvLedger` — murni,
  Prisma-agnostic, dipakai 3 modul inventory). **Helper ini yang dipakai
  ulang**, bukan ditulis dari nol, untuk posting GL modul lain.
- Stok = **derived view** `inv_stock_balances` dari movement `POSTED`
  (bukan ledger terpisah); `InvStockMovementPostingService` sudah implement
  moving-average costing + `glPostingEnabled` toggle (default OFF) + GL
  ISSUE/RETURN. Lebih maju dari perkiraan Fase 3.
- **Tapi** sebagian besar posting GL transaksi Sales/Purchasing masih
  **sengaja NO-OP berlabel TODO** (konfirmasi: SI, DO, Invoice Swap) —
  komentar di kode eksplisit "to be implemented when GL spec confirmed".
- Field rantai dokumen `sourceLineId`/`sourceDocType` **ada di schema**
  (tiap line table) tapi **TIDAK dipakai di service manapun** — di-grep 0
  match. Header-level reference (mis. `ErpSlsInvoice.orderId`/
  `deliveryOrderId`) ada FK langsung, tapi tanpa outstanding-qty tracking
  (tidak ada kolom `remainingQty`/`invoicedQty` dsb di schema manapun).
- Account resolution sudah termodel lengkap: `ErpItem.salesAccountId`
  (fallback `ErpItemCategory.salesAccountId`), `ErpPartner.receivableAccountId`,
  `ErpTax.saleAccountId`/`purchaseAccountId` — dipakai account-resolution SI
  (lihat di bawah).

**Keputusan user:** jangan susun ulang plan 4-fase dulu secara global.
Kerjakan **satu transaksi nyata dulu** sebagai pilot bukti-konsep, lalu
lanjut bertahap. Pilot pertama = **Sales Invoice (SI) GL posting**.

### § SI (Sales Invoice) GL posting — pilot pertama Fase 0 (2026-10-01)

`src/erp-sls-invoices/sls-invoice-posting.service.ts` — ganti NO-OP jadi
posting nyata, reuse `buildLedgerRows`/`reverseInvLedger` dari
`erp-inv-gl/inv-gl-posting.helpers.ts` (override `source: 'SALES'` setelah
build, helper generiknya Prisma-agnostic jadi aman dipakai lintas modul).

Pola jurnal (sesuai tabel "Pola jurnal usulan" §Aturan posting di atas):
- **Dr** Piutang Usaha — `invoice.receivableAccountId`, fallback
  `customer.receivableAccountId`; error eksplisit bila keduanya kosong.
- **Cr** Penjualan per baris — `item.salesAccountId`, fallback
  `item.category.salesAccountId`; error per-baris bila kosong (bukan silent
  skip).
- **Cr** PPN Keluaran per baris — `tax.saleAccountId` (dari `line.tax1Id`/
  `tax2Id`), override oleh `invoice.tax1AccountId`/`tax2AccountId` bila diisi
  manual di header.
- **Dr** Diskon Penjualan — `invoice.discountAccountId`, hanya bila
  `discountAmount > 0`; error bila diskon ada tapi akun belum diset.
- Balance check + append-on-post + hard-delete-on-reverse mewarisi
  `buildLedgerRows`/`reverseInvLedger` (sama seperti pola inventory).
- `arLedgerEntryId` di-stamp ke baris ledger Piutang Usaha yang baru dibuat
  (dipakai laporan AR aging — sudah disiapkan kolomnya, baru sekarang diisi).

**Belum disentuh pilot ini (scope sengaja kecil):** DO/GRN stock posting,
outstanding-qty tracking, guard anti-double-posting antar dokumen,
sourceLineId wiring saat create DO/SI dari SO. Itu langkah berikutnya,
transaksi-per-transaksi, bukan infra generik sekaligus — ikuti urutan Fase 1
di atas setelah pilot ini divalidasi user.

### § DO (Delivery Order) stock posting — langkah kedua Fase 1 (2026-10-01)

`src/erp-sls-delivery-orders/sls-delivery-order-posting.service.ts` — ganti
NO-OP jadi posting stok nyata ke `inv_stock_movements` (sesuai "Barang keluar
ke pelanggan diposting oleh DO" di §Aturan anti posting ganda). Konfirmasi
riset: GRN juga NO-OP dengan alasan sama ("deferred to a dedicated pass that
wires inv_* stock movements") — jadi DO ini sekaligus **membuka jalan pola**
yang sama akan dipakai GRN berikutnya.

**Pola:**
- POST → buat 1 `ErpInvStockMovement` (`movementType: ISSUE`,
  `status/postingStatus: POSTED`) + 1 baris per baris DO (qty, unit, warehouse
  dari line atau fallback header), lalu delegasikan GL valuation (COGS/
  inventory, digerbang `glPostingEnabled` setting) ke
  `InvStockMovementPostingService.postMovement` yang **sudah ada** — tidak
  menulis ulang logic costing.
- **Traceability tanpa migrasi:** `ErpInvStockMovement` tidak punya kolom FK
  balik ke dokumen sumber (ia transaksi independen, beda dari
  `ErpFinLedgerEntry` yang punya `sourceDocType`/`sourceId`). Daripada
  menambah migrasi skema untuk pilot kecil ini, link disimpan di
  `metadata: { sourceDocType: 'sls_delivery_orders', sourceId }` (JSON,
  sudah ada kolomnya di semua model) — dipakai `reverseLedger` untuk mencari
  movement yang harus dihapus saat DO di-REOPEN. **Catatan untuk Fase 0
  lanjutan:** kalau pola traceability generik via kolom FK (bukan metadata
  ad-hoc) jadi kebutuhan lintas banyak dokumen, itu saatnya migrasi skema
  `relatedDocType`/`relatedDocId` di `ErpInvStockMovement` — belum sekarang.
- **Doc numbering:** movement type `ISSUE` di `DOC_CODE_BY_TYPE` existing
  sudah dipakai kode `RF` (untuk transaksi Fuel Refill yang di-gate PRD) —
  BUKAN dipakai untuk DO. DO posting ini membuat movement langsung lewat
  Prisma (bukan lewat `ErpInvStockMovementsService.create`), dengan kode
  dokumen sendiri **`DOI`** (DO Issue), reuse `erpDocumentNumbering` fallback
  count-based yang sama persis dengan pola SI/DO/dst.
- Module wiring: `ErpInvStockMovementsModule` sekarang **export**
  `InvStockMovementPostingService` juga (sebelumnya cuma `...Service`), dan
  `ErpSlsDeliveryOrdersModule` meng-import modul itu.
- **Diverifikasi end-to-end terhadap database nyata** (bukan cuma typecheck):
  create DO dummy → `postToLedger` → 1 movement + 1 line ter-create,
  `status=POSTED` → `reverseLedger` → movement terhapus bersih. Script test
  dihapus setelah lulus (bukan bagian permanen repo).

**Belum disentuh:** GRN stock posting (pola identik, giliran berikutnya),
guard supaya SI yang dibuat DARI DO tidak posting stok lagi (FR-SLS-02 —
perlu dulu SI punya cara "dibuat dari DO" yang belum ada sama sekali di
service SI saat ini), outstanding-qty tracking.

### § GRN (Goods Receipt) stock + GL posting — langkah ketiga Fase 1/2 (2026-10-01)

`src/erp-pur-goods-receipts/pur-goods-receipt-posting.service.ts` — ganti
NO-OP jadi posting nyata (sesuai "Barang masuk dari vendor diposting oleh
GRN" di §Aturan anti posting ganda). Dua bagian, keduanya baru:

1. **Stock movement (qty, QC-gated):** 1 `ErpInvStockMovement` per GRN
   (`movementType: TRANSFER_RECEIPT`, bukan `ISSUE`/`RETURN`), 1 baris per
   baris GRN **hanya untuk `acceptedQty > 0`** (rejected/quarantine qty tidak
   menambah stok — field QC `acceptedQty`/`rejectedQty`/`quarantineQty` sudah
   ada di schema sejak awal tapi belum pernah dipakai). Movement masuk ke
   `destinationWarehouseId` dari header/line GRN.
   - **Kenapa `TRANSFER_RECEIPT`, bukan dipaksa lewat
     `InvStockMovementPostingService.postMovement` seperti DO:** engine GL
     otomatis punya type `ISSUE`→(Dr COGS/Cr Inventory) dan
     `RETURN`→kebalikannya (Dr Inventory/Cr **COGS**). Memakai `RETURN` untuk
     GRN akan salah secara akuntansi (pembelian itu Dr Inventory/Cr **Hutang/
     Accrual**, bukan Cr COGS). `TRANSFER_RECEIPT` sengaja **tidak**
     men-trigger auto-GL di `postMovement` ("TRANSFER / TRANSFER_RECEIPT /
     REQUEST: no GL valuation" — lihat komentar `inv-stock-movement-posting.
     service.ts`), jadi dipakai murni untuk update `inv_stock_balances`
     (derived view), GL-nya ditulis manual (poin 2). **Kalau butuh pola ini
     lagi untuk transaksi lain**, jangan asumsikan semua movement bisa lewat
     `postMovement` — cek dulu arah akuntansinya cocok ISSUE/RETURN atau
     tidak.
2. **GL accrual** (reuse `buildLedgerRows`/`reverseInvLedger`, pola sama
   seperti SI):
   - **Dr** Persediaan — `line.inventoryAccountId`, fallback `item.
     inventoryAccountId`.
   - **Cr** Barang Diterima Belum Ditagih (GR/IR Accrual) —
     `line.accruedPayableAccountId`, fallback header `grn.payableAccountId`.
   - Nominal = `acceptedQty × netUnitCost` (unit cost bersih setelah diskon
     baris, fungsi `netUnitCost` dipertahankan dari kode lama).
3. Item cost stamping (`purchasePrice`/`lastHpp`/seed `averageCost`)
   dipertahankan persis seperti sebelumnya — tidak diubah.

**Doc numbering movement:** kode baru **`GRI`** (GRN Issue→in, dibuat manual
lewat Prisma seperti pola `DOI` di DO, bukan lewat
`ErpInvStockMovementsService.create`).

Module wiring: **tidak perlu** import `ErpInvStockMovementsModule` (tidak
pakai `InvStockMovementPostingService` sama sekali — beda dari DO).

**Diverifikasi end-to-end terhadap database nyata:** GRN qty=10,
acceptedQty=8, rejectedQty=2 → movement hanya punya 1 baris qty=8 (bukan 10)
→ 2 baris ledger balanced Dr 120000 (8×15000) akun persediaan / Cr 120000
akun accrual (fallback header) → item `purchasePrice`/`lastHpp` ter-stamp
15000 → reverse menghapus movement + ledger rows bersih. Script test dihapus
setelah lulus.

**Belum disentuh:** PI Purchasing yang dibuat dari GRN tidak boleh menambah
stok lagi (FR-PUR-03 — PI service belum punya cara "dibuat dari GRN" sama
sekali, sama seperti SI/DO), outstanding-qty tracking per baris PO.

### § RNR (Return Receipt) + SR (Sales Return) posting — langkah keempat/kelima Fase 1 (2026-10-01)

Dua transaksi, pasangan sama seperti DO+SI (§Aturan anti posting ganda:
"Barang retur dari pelanggan diposting oleh RNR", FR-SLS-05/06 — SR dari RNR
tidak menggerakkan stok lagi, SR tanpa RNR menggerakkan stok sendiri).
**Catatan: guard "SR dari RNR tidak posting stok lagi" BELUM dibangun** —
bagian ini baru posting stok RNR dan jurnal SR secara independen; keduanya
masih bisa double-post kalau user posting SR yang sama dari RNR DAN tanpa
RNR. Guard anti-double-posting itu infra Fase 0 yang masih tertunda.

**RNR** (`src/erp-sls-return-receipts/sls-return-receipt-posting.service.ts`):
stock movement `movementType: RETURN` ke `inv_stock_movements`, 1 movement +
1 baris per baris RNR, delegasi penuh ke
`InvStockMovementPostingService.postMovement` (**beda dari GRN**: untuk RNR,
`RETURN` type di engine existing — Dr Inventory/Cr COGS — secara akuntansi
**benar**, karena RNR membalikkan ISSUE yang sudah di-debit COGS saat DO,
bukan transaksi baru seperti pembelian). Doc number kode baru **`RNRI`**.
Module `ErpSlsReturnReceiptsModule` sekarang import `ErpInvStockMovementsModule`.

**SR** (`src/erp-sls-returns/sls-return-posting.service.ts`): jurnal GL,
mirror persis `SlsInvoicePostingService` tapi debit/kredit terbalik:
- **Dr** Retur Penjualan per baris — `item.salesReturnAccountId`, fallback
  `item.category.salesAccountId`.
- **Dr** PPN Keluaran per baris (retur) — `tax.saleAccountId`, override
  header `tax1AccountId`/`tax2AccountId`.
- **Cr** Piutang Usaha — `return.receivableAccountId`, fallback
  `customer.receivableAccountId`.
- Reuse `buildLedgerRows`/`reverseInvLedger` yang sama, `source: 'SALES'`.

**Diverifikasi end-to-end terhadap database nyata** (dua script test terpisah,
dihapus setelah lulus): RNR → 1 movement RETURN ter-create, reverse bersih.
SR → 2 baris ledger balanced (Dr 50000 Retur / Cr 50000 AR, akun sesuai item
& header), reverse bersih.

**Dilewati sementara (level kerja beda, bukan unit sepadan):** AS (Customer
Advance) dan IP (Payment Receipt) — ditelusuri tapi **tidak dikerjakan**.
Temuan: `ErpFinArReceipt` (AR Receipt, tujuan `arReceiptId` di AS) belum
punya `transition`/workflow/posting service sama sekali — baru CRUD polos
(create/findAll/findOne/update/remove, tanpa POST/REOPEN). AS sendiri juga
**tidak punya field akun kas/bank** di schema maupun DTO — desainnya
sengaja mendelegasikan sisi kas ke AR Receipt terkait
(`ErpSlsCustomerAdvance.arReceiptId`), tapi rantai itu putus karena AR
Receipt belum dibangun. `ErpFinSettlementAllocation` (alokasi AR
Receipt/AP Payment ke invoice-invoice tertentu, FK wajib ke
`fin_ledger_entries.id`) juga ada di schema tanpa logic apapun di service.
Membangun AS/IP/PV dengan benar = membangun dulu AR Receipt
workflow+posting+allocation — unit kerja jauh lebih besar dari SI/DO/GRN/
RNR/SR (yang semua "isi NO-OP yang polanya sudah ada"). **Jangan
dikerjakan sambil lalu** — perlu desain allocation flow dulu, eskalasi ke
user sebelum mulai.

### § PI Purchasing (Purchase Invoice) stock + GL posting — langkah keenam Fase 2 (2026-10-01)

`src/erp-pur-invoices/pur-invoice-posting.service.ts` — ganti NO-OP jadi
posting nyata, 2 jalur eksklusif berdasar `invoice.goodsReceiptId`
(FR-PUR-03/04 persis "PI dibuat dari GRN tidak menambah stok lagi, PI tanpa
GRN menambah stok sendiri"):

- **PATH A — dari GRN** (`goodsReceiptId` terisi): **tidak** ada stock
  movement baru (barang & accrual sudah diposting GRN). GL murni
  reklasifikasi: **Dr** GR/IR Accrual (`line.accruedPayableAccountId`,
  wajib diisi — error eksplisit kalau kosong) + **Dr** PPN Masukan
  (`tax.purchaseAccountId`, override header) → **Cr** Utang Usaha
  (`invoice.payableAccountId`, fallback `supplier.payableAccountId`).
- **PATH B — tanpa GRN** (`goodsReceiptId` null, pembelian langsung):
  stock movement sendiri (pola identik GRN: `TRANSFER_RECEIPT`, qty-only,
  **tidak** lewat `InvStockMovementPostingService.postMovement` karena arah
  GL auto-nya salah untuk pembelian — alasan sama seperti GRN) + GL penuh
  **Dr** Persediaan (`line.inventoryAccountId`, fallback item) + Dr PPN
  Masukan → **Cr** Utang Usaha.
- Doc number movement PATH B: kode baru **`PII`**.
- **`matchStatus` (3-way match) SENGAJA tidak dijadikan gate posting** —
  field itu ada di schema + enum `ErpMatchStatus` tapi **tidak pernah
  di-set** oleh service manapun (selalu default `PENDING`). Komentar lama di
  file ini mengklaim "gated behind 3-way match MATCHED/WAIVED" tapi itu
  aspirational, bukan infra yang nyata — kalau dipaksa jadi gate, PI tidak
  akan pernah bisa di-POST (matchStatus selalu PENDING selamanya). 3-way
  matching = fitur terpisah yang lebih besar, di luar scope unit kerja ini.
  **Catatan untuk siapa pun yang membangun 3-way match nanti:** field sudah
  ada, servicenya belum.

**Diverifikasi end-to-end terhadap database nyata, kedua jalur:** PATH A
(reuse GRN existing demi FK valid) → 2 baris ledger balanced Dr 120000
Accrual/Cr 120000 AP, **nol** stock movement tercipta, reverse bersih.
PATH B → 1 stock movement qty=6 tercipta, 2 baris ledger balanced Dr 90000
Persediaan/Cr 90000 AP, reverse bersih di kedua movement+ledger. Script
test dihapus setelah lulus.

**Belum disentuh:** PRT (Purchase Return, mirror SR tapi Purchasing — direct
vs undirect mode per FR-PUR-05), DNR (Return Shipment, mirror RNR tapi
barang keluar ke vendor). Levelnya sepadan SI/DO/GRN/RNR/SR/PI — kandidat
langkah berikutnya.

### § PRT/DNR (Purchase Return) stock + GL posting — langkah ketujuh Fase 2 (2026-10-01)

`src/erp-pur-returns/pur-return-posting.service.ts`. **Beda struktural dari
Sales:** di Purchasing, DNR (Return Shipment) dan PRT (Purchase Return) itu
**satu model `ErpPurReturn`**, dibedakan field `returnType` (enum
`ErpPurchaseReturnType`: `RETURN_TO_VENDOR` / `DEBIT_NOTE`) — bukan dua tabel
terpisah seperti RNR/SR di Sales. Jangan cari 2 model Purchasing yang
terpisah untuk ini.

- **`RETURN_TO_VENDOR`** (= DNR, barang fisik keluar ke vendor): posting
  stock movement (`movementType: ISSUE`, qty-wise barang keluar) **+** jurnal
  reversal. Movement **sengaja tidak** lewat
  `InvStockMovementPostingService.postMovement` (sama alasan GRN/PI: arah
  auto-GL `ISSUE` = Dr COGS/Cr Inventory mengasumsikan penjualan, padahal
  retur pembelian itu Dr **Utang Usaha**/Cr **Persediaan**) — GL ditulis
  manual. Doc number movement kode baru **`DNRI`**.
- **`DEBIT_NOTE`** (= PRT tanpa gerak fisik): **tidak ada** stock movement
  sama sekali, jurnal saja.
- **GL (kedua tipe):** **Dr** Utang Usaha (`header.payableAccountId`,
  fallback `supplier.payableAccountId`) + **Cr** persediaan/retur
  pembelian — `RETURN_TO_VENDOR` pakai `line.inventoryAccountId` (reverse
  langsung ke akun persediaan), `DEBIT_NOTE` pakai
  `header.returnPurchaseAccountId` fallback `item.purchaseReturnAccountId`
  (tidak ada stok fisik, jadi bukan akun persediaan) — **Cr** PPN Masukan
  reversal per baris (kebalikan dari PI).
- **Di luar scope (sengaja):** FR-PUR-05 direct/undirect — apakah retur ini
  langsung memotong sisa bayar `invoiceId` tertentu (direct) atau masuk
  saldo umum ditarik VPP nanti (undirect). Itu **outstanding-amount
  tracking**, infra Fase 0 yang masih belum dibangun sama sekali di
  manapun — unit kerja ini cuma GL+stok, sama persis scope SI/DO/GRN/RNR/
  SR/PI sebelumnya.

**Diverifikasi end-to-end terhadap database nyata, kedua `returnType`:**
`RETURN_TO_VENDOR` → 1 movement ISSUE qty=4, ledger Dr 60000 AP/Cr 60000
Persediaan, reverse bersih. `DEBIT_NOTE` → nol movement, ledger Dr 30000
AP/Cr 30000 akun retur pembelian item, reverse bersih.

**Tujuh transaksi selesai total sejak awal sesi ini:** SI, DO, GRN, RNR, SR,
PI, PRT/DNR — semua reuse `buildLedgerRows`/`reverseInvLedger`, semua
tervalidasi end-to-end terhadap database nyata (bukan cuma typecheck), semua
pola call-site (`postToLedger`/`reverseLedger` dipanggil dari
`transition()` POST/REOPEN) tidak diubah. **Belum ada satupun** guard
anti-double-posting lintas dokumen (mis. SR dari RNR vs tanpa RNR,
disebutkan di §Aturan anti posting ganda) — itu tetap infra Fase 0 yang
tertunda, dicatat berulang di tiap section agar tidak terlupa saat
lanjut.

### § AR Receipt (IP — Payment Receipt) — dibangun dari nol, bukan isi NO-OP (2026-10-01)

Unit kerja beda level dari tujuh transaksi sebelumnya: `ErpFinArReceipt`
sebelumnya **CRUD polos** (create/findAll/findOne/update/remove, tanpa
`transition`/workflow/posting sama sekali — lihat temuan di § AS/IP di
atas). Dibangun penuh atas konfirmasi user: workflow §2.7 + posting GL +
allocation ke invoice.

**File baru:**
- `dto/transition-ar-receipt.dto.ts` — state machine standar (SUBMIT/
  APPROVE/REJECT/POST/REOPEN), sama pola semua modul lain.
- `ar-receipt.helpers.ts` — `NEXT`/`EDITABLE`/`buildArReceiptWhere`. **Beda
  dari pola SI/DO/dkk: `NEXT` di sini PUNYA entry `POSTED: { REOPEN:
  'DRAFT' }`.** Ditemukan saat testing: SI/DO/GRN/dkk TIDAK punya entry itu
  (`NEXT['POSTED']` kosong di semua helper mereka) — artinya kalau dicoba,
  REOPEN dari status POSTED di modul-modul itu akan selalu gagal
  "Aksi REOPEN tidak valid dari status POSTED" kecuali call site transition()
  mereka tidak pernah benar-benar mengandalkan guard `NEXT` untuk REOPEN
  (cek lagi: transition() semua modul punya branch `if (dto.action ===
  A.REOPEN)` TERPISAH SETELAH cek `next = NEXT[status]?.[action]` di awal
  — jadi REOPEN **harus lolos cek NEXT dulu** sebelum branch itu jalan).
  **Ini kemungkinan bug laten pre-existing di SI/DO/GRN/RNR/SR/PI/PRT**: REOPEN
  dari POSTED akan ditolak di semua 7 modul yang dikerjakan sesi ini **kecuali**
  `inv-stock-movement.helpers.ts` (satu-satunya yang punya `POSTED: {
  REOPEN: 'DRAFT' }`). **Belum diperbaiki di 7 modul itu** — di luar scope AR
  Receipt, tapi dicatat sebagai temuan untuk pass perbaikan berikutnya
  (cukup tambah satu baris `POSTED: { [A.REOPEN]: 'DRAFT' }` di tiap
  `*.helpers.ts` yang belum punya).
- `ar-receipt-posting.service.ts` — GL + allocation.

**Desain kunci — allocation sebagai draft-di-metadata, bukan row langsung:**
`ErpFinSettlementAllocation.ledgerEntryId` adalah **NOT NULL**, jadi row
alokasi tidak bisa dibuat sebelum ledger entry-nya ada. Solusi: intent
alokasi (invoiceId+amount+lineNo) disimpan di `receipt.metadata.
draftAllocations` saat `create()`/`update()` (pola yang sama dengan
`metadata.sourceDocType/sourceId` di DO/GRN — simpan di JSON saat tidak ada
kolom FK terstruktur). Row `ErpFinSettlementAllocation` **hanya dibuat saat
POST** (materialize dari draft, linked ke `ledgerEntryId` baru), dan
**dihapus saat REOPEN** (karena ledger-nya juga dihapus) — draft di metadata
tetap ada untuk re-POST tanpa perlu re-input.

**GL posting:** **Dr** Kas/Bank per instrument (`instrument.bankAccountId`,
cara bayar CASH/TRANSFER/CARD/OTHER — **GIRO sengaja dikeluarkan dari DTO
`ArReceiptInstrumentMethodDto`**, karena giro FR-FIN-02 "belum menambah
saldo bank" sampai dicairkan via clearing terpisah, bukan instrumen kas/bank
langsung) → **Cr** Piutang Usaha per allocation (`invoice.
receivableAccountId`, fallback `customer.receivableAccountId`). Validasi:
total instrument harus = total allocation = `header.amount`; alokasi ke
satu invoice tidak boleh melebihi sisa outstanding (dihitung on-the-fly dari
`SUM(allocations.amount WHERE invoiceRef = invoiceId)` lintas semua AR
Receipt lain, **bukan** kolom tersimpan — belum ada `remainingAmount` di
manapun, sama seperti semua gap outstanding-tracking sebelumnya).

**Efek samping POST/REOPEN:** `ErpSlsInvoice.settlementStatus` otomatis
di-resettle (`UNPAID`/`PARTIAL`/`PAID`) berdasar total alokasi lintas semua
AR Receipt untuk invoice itu, bukan hanya receipt yang sedang diproses.

**Di luar scope (belum dibangun):** AS (Customer Advance) masih terputus
dari AR Receipt (`arReceiptId` di AS tetap tidak terisi otomatis) — AS perlu
pass terpisah untuk membuat AR Receipt otomatis saat AS di-POST, lalu
posting AS sendiri jadi reklasifikasi seperti PI dari GRN. PV (AR Payment,
pelunasan multi-invoice dengan kurs) akan **reuse pola yang sama persis**
(instrument+allocation+metadata-draft) — giliran berikutnya kalau mau
lanjut pola ini.

**Diverifikasi end-to-end terhadap database nyata, siklus penuh:** SI
POSTED (grandTotal 100000) → AR Receipt dibuat (instrument TRANSFER 60000,
alokasi ke SI 60000) → SUBMIT→APPROVE→POST → 2 baris ledger balanced (Dr
60000 Bank/Cr 60000 AR) + 1 row allocation ter-link ke ledger row AR yang
benar + SI `settlementStatus` jadi `PARTIAL` → REOPEN → ledger+allocation
terhapus bersih + SI balik `UNPAID` → SUBMIT→APPROVE→POST lagi → ledger
terbentuk ulang dari draft metadata (tanpa re-input) — membuktikan desain
draft-di-metadata bekerja untuk siklus reopen/repost berulang.

### § Fix bug laten: REOPEN dari status POSTED tertolak di 16 modul (2026-10-01)

Ditemukan saat membangun AR Receipt (lihat section di atas): `NEXT` map di
setiap `*.helpers.ts` hanya punya `APPROVED: { POST, REOPEN }` — **tidak
ada** entry `POSTED: { REOPEN }`. `transition()` di semua service mengecek
`next = NEXT[status]?.[action]` di awal dan **throw kalau falsy, SEBELUM**
sampai ke branch `if (action === REOPEN)`. Akibatnya: memanggil REOPEN saat
dokumen sudah `POSTED` **selalu gagal** dengan "Aksi REOPEN tidak valid dari
status POSTED" — padahal itu justru skenario paling umum (user POST dulu,
baru nanti perlu reopen untuk koreksi). Satu-satunya modul yang sudah benar
dari awal: `erp-inv-stock-movements` (punya `POSTED: { REOPEN: 'DRAFT' }`).

**Diperbaiki (tambah 1 baris `POSTED: { [A.REOPEN]: 'DRAFT' }` per file,
mekanis, tanpa ambiguitas) di 16 modul:** SI, DO, GRN, RNR, SR, PI, PRT
(tujuh yang disentuh sesi ini) + PR (`erp-pur-requisitions`), PO
(`erp-pur-orders`), AS (`erp-sls-customer-advances`), DR
(`erp-sls-delivery-reports`), SQ (`erp-sls-quotations`), SIE
(`erp-sls-invoice-swaps`), PL (`erp-sls-packing-lists`), PI-Proforma
(`erp-sls-proforma-invoices`), SO (`erp-sls-orders`).

**Sengaja TIDAK disentuh:** `erp-mfg-boms`/`erp-mfg-work-orders` (BOM/WO) —
punya bentuk `NEXT` yang berbeda (tidak ada aksi POST sama sekali di
APPROVED), dan modul Production ini di-gate total oleh PRD (§Fase 3 Plan,
"usulan awal, belum ada di diagram kerja") — di luar scope relevan sekarang,
workflow-nya sendiri belum final.

**Diverifikasi:** typecheck bersih di semua 16 file; test nyata terhadap
database — DO di-POST lalu `NEXT['POSTED']['REOPEN']` dicek langsung (guard
yang sama dipakai `transition()`), hasil `'DRAFT'` (sebelumnya `undefined`).

### § PV ternyata = AR Receipt — tidak ada unit kerja terpisah (2026-10-01)

Sebelum membangun AP Payment, dicek: PRD membedakan **IP (Payment Receipt,
dari SO langsung)** vs **PV (AR Payment, dari IC/SI/SR/AS/IP, multi-invoice
+ kurs)** sebagai dua alur. Tapi **tidak ada model/folder PV terpisah** di
codebase — tidak ada `erp-sls-*-payments` atau sejenis. Konfirmasi: `fin_ar_receipts`
(yang jadi dasar AR Receipt yang baru dibangun) **sudah generik** — multi-
allocation ke invoice manapun + `exchangeRate` header — cukup untuk
menangani kedua kebutuhan IP dan PV sekaligus, dibedakan hanya lewat field
opsional `source` (default `'IP'`, user bisa kirim `'PV'` saat create untuk
flow dari IC). **Jadi PV sudah selesai** sebagai efek dari membangun AR
Receipt — tidak ada unit kerja tambahan.

Juga dicek IC (`erp-sls-ar-collections`): sudah punya `transition()` sendiri
tapi `postingStatus` sengaja tetap `UNPOSTED` (ada TODO comment) — ini
**benar secara desain**, FR-SLS-07 PRD eksplisit "IC tidak memposting
jurnal. IC hanya mengelompokkan dokumen yang ditagih dan menjadi sumber
PV." Tidak perlu diperbaiki.

### § VP (Vendor Payment / AP Payment) — reuse pola AR Receipt, dibangun dari nol (2026-10-01)

`ErpFinApPayment` levelnya sama seperti `ErpFinArReceipt` sebelum dikerjakan
— CRUD polos 237 baris, tanpa transition/posting. Dibangun penuh, mirror
`ArReceiptPostingService` dengan arah dibalik (Dr Utang Usaha, bukan Cr
Piutang Usaha) + tambahan FX gain/loss & term discount (field header
`fxGainLossAccountId`/`termDiscountAccountId` sudah ada di schema sejak
awal — "added for VP/VPP reuse" sesuai komentar schema — belum dipakai
sampai sekarang, FR-PUR-08 "mencatat selisih kurs bila ada").

**File baru:** `dto/transition-ap-payment.dto.ts`, `ap-payment.helpers.ts`
(include `POSTED: { REOPEN: 'DRAFT' }` sejak awal — lesson dari bug fix di
atas, tidak perlu ditemukan ulang), `ap-payment-posting.service.ts`. DTO
create ditulis ulang dengan `instruments[]` + `allocations[]` (allocation
ke `pur_invoices`, bukan `sls_invoices`) + field opsional per-allocation
`fxGainLossAmount`/`termDiscountAmount`. Pola allocation-draft-di-metadata
**identik** AR Receipt (alasan sama: `ledgerEntryId` NOT NULL).

**Identitas balance (PENTING, beda dari kelihatannya sepintas):**
```
Dr Utang Usaha (allocationTotal)  [+ Dr Selisih Kurs rugi, bila fx < 0]
= [Cr Selisih Kurs laba, bila fx > 0] + Cr Potongan Termin + Cr Kas/Bank (instrumentTotal)

→ instrumentTotal = allocationTotal − fxGainLossNet − termDiscountNet
```
**Bug sempat salah tulis saat development** (`instrumentTotal + fxNet +
termDiscountNet == allocationTotal` — salah tanda, gagal di test pertama
dengan pesan error yang jelas "95000 + (-3000) + 2000 ≠ 100000") — diperbaiki
sebelum commit ke rumus yang benar di atas. Dicatat di sini supaya kalau PI/
SI/transaksi lain nanti butuh pola serupa (instrument vs allocation dengan
offset tambahan), jangan asumsikan tanda offset itu simetris — turunkan
dari struktur jurnal dulu, baru tulis validasi.

**Diverifikasi end-to-end terhadap database nyata:** PI POSTED (grandTotal
100000, PATH B tanpa GRN) → VP dibuat (instrument TRANSFER 101000, alokasi
100000 ke PI + fx −3000 + term discount 2000) → SUBMIT→APPROVE→POST → 4
baris ledger balanced (Dr AP 100000, Dr FX rugi 3000, Cr Discount 2000, Cr
Bank 101000 — total 103000=103000) → PI `settlementStatus` jadi `PAID` →
REOPEN → ledger+allocation terhapus bersih + PI balik `UNPAID`.

**Sepuluh unit kerja total selesai sesi ini** (7 isi-NO-OP + AR Receipt +
bug fix 16 modul + AP Payment). Sisa besar yang belum disentuh: AS→AR
Receipt auto-create, outstanding-qty tracking generik, guard anti-double-
posting generik, VPP (payment schedule, agregator AP mirip IC — kemungkinan
sama seperti IC, sengaja tanpa posting), semua transaksi ter-gate PRD.

### § Checkpoint — status Fase 1/2 Sales+Purchasing inti (2026-10-01)

Audit menyeluruh semua modul Sales+Purchasing untuk pastikan tidak ada
transaksi lain yang "TODO posting" terlewat. Hasil (cek marker "Intentionally
no"/"TODO.*posting" per file):

**Posting sudah nyata (dikerjakan sesi ini):** SI, DO, GRN, RNR, SR, PI, PRT/
DNR, IP (AR Receipt), VP (AP Payment).

**"Intentionally no" — BENAR & FINAL, sesuai tabel PRD (Jurnal=Tidak,
Stok=Tidak), bukan TODO terlewat:** SQ, SO, PI-Proforma (Sales), PL, DR, IC
(agregator, FR-SLS-07), PR, PO, VPP (agregator, FR-PUR-07 — mirror IC).
**Jangan "perbaiki" ini di masa depan tanpa cek ulang PRD** — no-op-nya
memang desain, bukan utang teknis.

**Masih gap, dicatat jelas di section masing-masing:** AS (Customer Advance)
— field akun kas/bank tidak ada by design, perlu auto-create AR Receipt
saat AS di-POST (unit kerja baru, belum dikerjakan). SIE (Invoice Swap) —
di-gate PRD, nunggu klarifikasi bisnis.

**Fase 1 (Sales) & Fase 2 (Purchasing) inti PRD — status: SELESAI** kecuali
AS dan outstanding-tracking/anti-double-posting generik (infra Fase 0 yang
dari awal direncanakan terpisah). RP/PP/SIE tetap di luar scope sesuai gate
plan awal.

### § AS (Customer Advance) GL posting — direct, BUKAN via AR Receipt (2026-10-01)

Rencana awal (dari § AR Receipt di atas): "AS perlu auto-create AR Receipt
saat di-POST, posting AS jadi reklasifikasi seperti PI dari GRN." **Rencana
ini SALAH setelah dicek ulang** — dibatalkan, diganti pendekatan direct.

**Kenapa AR Receipt tidak cocok:** `ErpFinSettlementAllocation` yang dipakai
AR Receipt WAJIB alokasi ke `sls_invoices` outstanding tertentu
(`invoiceRef`) — AS belum punya invoice untuk dilunasi (dia justru uang
muka SEBELUM ada SI). Memaksakan AS lewat AR Receipt berarti harus bikin
allocation row tanpa invoice nyata, melanggar validasi yang sudah ditulis
(`invoice tidak ditemukan`). AS adalah **Dr Kas/Bank, Cr Uang Muka
Penjualan (liability)** — beda akun kredit total dari AR Receipt (Cr
Piutang Usaha). Jadi AS diposting **langsung**, mirror SI/SR (self-
contained, tidak reuse AR Receipt).

**Gap skema ditemukan:** `sls_customer_advances` **tidak punya kolom**
`bankAccountId` maupun `advanceAccountId` sama sekali (beda dari SI yang
punya `receivableAccountId`/`discountAccountId` dkk eksplisit). Diselesaikan
tanpa migrasi: dua field baru di `CreateSlsCustomerAdvanceDto`/
`UpdateSlsCustomerAdvanceDto` (`bankAccountId`, `advanceAccountId`,
keduanya opsional saat create — supaya AS bisa diinput di DRAFT sebelum
akun diketahui), disimpan di `metadata` (pola sama dengan DO/GRN
traceability), **wajib diisi saat POST** (posting service throw error
eksplisit kalau kosong, bukan silent fallback).

**GL:** Dr `metadata.bankAccountId` / Cr `metadata.advanceAccountId`,
nominal = `advance.amount`. Reuse `buildLedgerRows`/`reverseInvLedger`.

**Diverifikasi end-to-end terhadap database nyata:** AS dibuat (amount
5000000, bankAccountId+advanceAccountId terisi) → SUBMIT→APPROVE→POST → 2
baris ledger balanced (Dr 5000000 Bank/Cr 5000000 Uang Muka Penjualan) →
REOPEN → ledger terhapus bersih.

**Sebelas unit kerja total selesai sesi ini.** Fase 1 (Sales) PRD sekarang
**benar-benar selesai** kecuali SIE (di-gate). AS menutup gap terakhir yang
tercatat di checkpoint sebelumnya.

### § AP (Vendor Advance) GL posting — mirror AS, tabel dibagi dengan VP (2026-10-01)

Temuan struktural penting: `erp-pur-vendor-advances` (AP, PRD) **tidak
punya model Prisma sendiri** — ia menulis ke **`fin_ap_payments` yang sama
dengan VP**, dibedakan hanya kolom `source = 'AP'` (filter wajib di semua
query `findRaw`/`findAll`). Levelnya sama seperti VP sebelum dikerjakan:
`transition()` ada tapi `postingStatus` sengaja stuck `UNPOSTED` dengan
TODO comment, **dan** `NEXT` map-nya (ditulis inline di service, bukan file
`*.helpers.ts` terpisah) **juga tidak punya `POSTED: { REOPEN }`** — bug
yang sama dengan temuan sebelumnya, di lokasi berbeda yang belum ter-grep
saat audit 16-modul (karena bukan file `*.helpers.ts`).

**Kenapa TIDAK reuse `ApPaymentPostingService` (posting VP):** sama alasan
AS vs AR Receipt — `ApPaymentPostingService` wajib alokasi ke
`pur_invoices` outstanding tertentu, tapi vendor advance belum punya
invoice untuk dilunasi. AP perlu posting sendiri: **Dr Uang Muka Pembelian
(asset) / Cr Kas-Bank** — arah terbalik dari AS, tapi sama alasan "tidak
reuse modul yang assume ada invoice".

**Field:** `fin_ap_payments.bankAccountId` sudah ada sebagai **kolom asli**
(dipakai VP juga) — diisi langsung, bukan metadata. `advanceAccountId`
tidak punya kolom (sama dengan AS) → disimpan di `metadata`. Ditambahkan ke
`CreateVendorAdvanceDto`/`UpdateVendorAdvanceDto`.

**Isolasi namespace ledger dari VP (PENTING):** AP dan VP berbagi **id
space yang sama** (satu tabel `fin_ap_payments`) — kalau `sourceDocType`
posting AP sama dengan VP (`'fin_ap_payments'`), REOPEN salah satu bisa
menghapus ledger milik yang lain pada id yang sama secara tidak sengaja.
**Sengaja dipakai `sourceDocType` berbeda**: AP = `fin_ap_payments_vendor_advance`,
VP tetap `fin_ap_payments`. Diverifikasi eksplisit di test (ledger AP tidak
muncul saat query dengan `sourceDocType: 'fin_ap_payments'`).

**Bug fix sekalian:** `transition()` sebelumnya **tidak pakai
`$transaction`** sama sekali (update langsung tanpa wrap) — diperbaiki
jadi pola standar (`$transaction` + `posting.reverseLedger` sebelum
`postToLedger` + update status, sama seperti semua modul lain) + `NEXT`
map ditambah `POSTED: { REOPEN: 'DRAFT' }`.

**Diverifikasi end-to-end terhadap database nyata:** AP dibuat (amount
3000000) → SUBMIT→APPROVE→POST → `postingStatus` benar jadi `POSTED`
(bukan stuck `UNPOSTED` seperti sebelumnya) → 2 baris ledger balanced (Dr
3000000 Uang Muka Pembelian/Cr 3000000 Bank) → **dikonfirmasi ledger AP
tidak bocor ke namespace VP** → REOPEN dari POSTED berhasil (bug fix
teruji end-to-end, bukan cuma guard check) → ledger terhapus bersih.

**Dua belas unit kerja total selesai sesi ini.** Fase 2 (Purchasing) kini
juga **lengkap** untuk semua transaksi non-gated PRD (PR, RFQ/BS belum
disentuh tapi PRD bilang Jurnal=Tidak/Stok=Tidak untuk keduanya — cek
ulang sebelum declare selesai total).

### § Fase 0 pilot pertama — outstanding-qty DO-dari-SO (FR-SLS-01) (2026-10-01)

Unit kerja baru (bukan isi NO-OP) — fondasi "create dari dokumen sumber"
yang sebelumnya tidak ada sama sekali untuk pasangan DO←SO. Dipilih sebagai
pilot karena paling sering dirujuk PRD dan field FK-nya sudah ada di
schema (`sls_delivery_order_lines.sourceLineId` — ada sejak awal, 0 match
di grep sebelum ini).

**Perubahan:**
- `SlsDeliveryOrderLineDto.sourceLineId` (opsional) — baris DO yang menarik
  SO wajib isi ini menunjuk `sls_order_lines.id`.
- `mapDeliveryOrderLine` (helpers) meneruskan `sourceLineId` ke create data
  (sebelumnya field itu di-drop diam-diam meski sudah ada di schema).
- `sls-delivery-order-outstanding.helpers.ts` (baru):
  - `validateSourceOrderOutstanding` — dipanggil di `create()` saat
    `dto.orderId` diisi. Cek (a) SO exists & `status === 'POSTED'` (SO
    belum final kalau belum POSTED — walau SO sendiri no-op GL/stok), (b)
    tiap `sourceLineId` benar milik SO itu, (c) **qty yang diminta tidak
    melebihi sisa outstanding** = `soLine.quantity − SUM(qty semua DO lain
    yang sudah menarik baris SO itu)`, dihitung on-the-fly (tidak ada
    kolom `remainingQty` tersimpan, pola sama dengan AR Receipt/AP
    Payment).
  - `maybeCloseSourceOrder` — dipanggil setelah create sukses. Kalau SEMUA
    baris SO sudah fully-taken, tandai closed.
- **Gap skema ditemukan saat implementasi:** `ErpDocumentStatus` **tidak
  punya nilai `CLOSED`** (cuma DRAFT/NEED_APPROVE/APPROVE_1-4/APPROVED/
  REJECTED/POSTED/VOID/CANCELLED) — menambah value butuh migrasi `ALTER
  TYPE ADD VALUE`, di luar scope pilot ini. **Solusi tanpa migrasi:** pakai
  kolom `closedDate` (nullable, sudah ada di schema sejak awal, persis
  untuk tujuan ini) sebagai sinyal "closed" — `status` tetap `POSTED`,
  `closedDate` terisi = outstanding sudah nol. **Siapa pun yang bikin
  laporan/UI "SO yang masih open"**: cek `closedDate IS NULL`, BUKAN
  `status != 'CLOSED'` (status itu tidak akan pernah jadi CLOSED).

**Sengaja belum disentuh (scope pilot dijaga kecil):**
- `update()`/`remove()` DO tidak re-sync `closedDate` SO (kalau DO yang
  menarik SO di-edit/dihapus, SO bisa salah ke-skip "closed" status).
  Perlu panggil `maybeCloseSourceOrder` juga di sana — follow-up.
- Pasangan SO→SI langsung (tanpa lewat DO) belum dicek — pola sama persis
  tapi di `erp-sls-invoices`, giliran berikutnya kalau mau extend pilot ini.
- Guard anti-double-posting lintas dokumen (SR dari RNR vs tanpa RNR, dst)
  masih terpisah, belum disentuh.

**Diverifikasi end-to-end terhadap database nyata:** SO qty=10 POSTED → DO1
tarik 6 (sukses, SO belum closed) → DO2 coba tarik 5 lagi (total 11>10,
**ditolak** dengan pesan sisa outstanding yang jelas) → DO3 tarik sisa 4
pas (sukses) → SO otomatis `closedDate` terisi begitu outstanding = 0.

**Tiga belas unit kerja total selesai sesi ini.** Fase 0 dimulai (pilot
pertama), bukan selesai — masih banyak pasangan dokumen lain yang perlu
pola serupa (GRN←PO, SI←DO, PI←GRN, dst — semua sudah punya field FK tapi
belum ada validasi outstanding).

### § FR-SLS-02 — SI stock posting anti-double (dari DO vs mandiri) (2026-10-01)

`src/erp-sls-invoices/sls-invoice-posting.service.ts` — tambahkan stock
posting yang sebelumnya 100% absen (SI dari awal sesi ini hanya posting
GL, tidak pernah menyentuh stok sama sekali). Sesuai FR-SLS-02 persis:
"SI yang dibuat dari DO tidak boleh memotong stok lagi. SI tanpa DO
memotong stok sendiri."

- **Jika `invoice.deliveryOrderId` terisi:** TIDAK ada stock movement baru
  — barang sudah keluar lewat DO. Cukup posting GL (perilaku yang sudah
  ada sebelumnya, tidak diubah).
- **Jika `deliveryOrderId` null (SI mandiri):** buat `ErpInvStockMovement`
  sendiri (`movementType: ISSUE`), 1 baris per baris SI, delegasi penuh ke
  `InvStockMovementPostingService.postMovement` — sama seperti DO (arah
  ISSUE untuk penjualan itu benar secara akuntansi, beda dari kasus
  GRN/PI/DNR yang butuh GL manual).
- Doc number movement: kode baru **`SII`** (SI Issue).
- Module wiring: `ErpSlsInvoicesModule` sekarang import
  `ErpInvStockMovementsModule`.

**Sengaja belum disentuh (scope dijaga kecil, follow-up terpisah):**
outstanding-qty validasi SI←DO (mirror `validateSourceOrderOutstanding` DO←
SO dari pilot sebelumnya) — SI line DTO belum punya `sourceLineId`, dan SI
header belum divalidasi terhadap qty DO yang belum ditagih. Saat ini kalau
`deliveryOrderId` diisi, SI percaya begitu saja tanpa cek qty — bisa
nagih lebih dari yang dikirim DO. Itu langkah berikutnya untuk menutup
FR-SLS-02 sepenuhnya.

**Diverifikasi end-to-end terhadap database nyata:** PATH A (SI dari DO,
qty=5) → posting sukses, **nol** stock movement tercipta (dikonfirmasi).
PATH B (SI mandiri, qty=4) → posting sukses, 1 stock movement ISSUE qty=4
tercipta, reverse membersihkan movement-nya.

**Empat belas unit kerja total selesai sesi ini.**

### § FR-SLS-02 outstanding-qty SI-dari-DO + bug kritis `settlementStatus` (2026-10-01)

Menutup gap yang dicatat di section sebelumnya: `sourceLineId` ditambah ke
`SlsInvoiceLineDto`, diteruskan `mapInvoiceLine`, dan
`sls-invoice-outstanding.helpers.ts` (baru) —
`validateSourceDeliveryOrderOutstanding`, **mirror persis**
`validateSourceOrderOutstanding` (DO←SO) tapi untuk SI←DO: SI line dengan
`sourceLineId` tidak boleh menagih melebihi sisa qty DO line
(`doLine.quantity − SUM(qty SI lain yang sudah menagih baris itu)`), DO
harus `status === 'POSTED'`. Dipanggil di `create()` saat `dto.
deliveryOrderId` diisi.

**Bug kritis ditemukan & diperbaiki saat testing (di luar scope langsung,
tapi memblokir verifikasi dan jelas-jelas bug nyata):**
`sls-invoice-persistence.mapper.ts` set `settlementStatus: 'UNSETTLED' as
never` — nilai itu **tidak ada** di enum `ErpSettlementStatus`
(`UNPAID`/`PARTIAL`/`PAID` saja). `as never` membungkam TypeScript tapi
Prisma tetap menolaknya di runtime dengan `Invalid value for argument
settlementStatus`. **Akibatnya: `ErpSlsInvoicesService.create()` SELALU
GAGAL sejak awal** — setiap SI yang dibuat sesi ini sebelumnya (SI pilot GL
posting, SR, VP test, dll) dibuat lewat **raw Prisma langsung** di script
test (bypass service), bukan lewat `service.create()` — jadi bug ini tidak
pernah ketahuan sampai sekarang karena ini test pertama yang benar-benar
memanggil `ErpSlsInvoicesService.create()`. Diperbaiki jadi
`'UNPAID'` (status awal yang benar, sejalan field lain seperti AS/SR/PI).
**Implikasi:** service SI kemungkinan tidak pernah dipanggil end-to-end
lewat endpoint API sejak dibangun — worth flagging ke user/QA kalau ada
ekspektasi SI sudah "berfungsi" di frontend.

**Diverifikasi end-to-end terhadap database nyata, kali ini LEWAT SERVICE
LAYER (bukan raw Prisma):** DO qty=10 POSTED → SI1 (via `service.create()`)
tagih 6 (sukses) → SI2 coba tagih 5 lagi (total 11>10, **ditolak**
dengan pesan sisa outstanding jelas) → SI3 tagih sisa 4 pas (sukses).

**Lima belas unit kerja total selesai sesi ini.** FR-SLS-01 (DO←SO) dan
FR-SLS-02 (SI←DO) qty-side kini sama-sama lengkap dengan pola konsisten.
Kandidat berikutnya untuk pola serupa: GRN←PO, PI←GRN, RNR←SI, SR←RNR,
dan pasangan Purchasing (DNR/PRT←PI).

### § Dua bug kritis lagi ditemukan: SR & RNR `create()` juga selalu gagal (2026-10-01)

Setelah fix SI `settlementStatus: 'UNSETTLED'`, audit cepat ke seluruh
`*-persistence.mapper.ts` dan `*-enrich.ts` untuk pola serupa menemukan
**dua bug lagi di modul yang sudah dicatat "selesai" sesi ini (SR, RNR)**:

1. **`settlementStatus: 'OPEN' as never`** — nilai yang juga **tidak ada**
   di enum `ErpSettlementStatus` (sama pola persis dengan bug SI), di
   `sls-return-persistence.mapper.ts` (SR) dan
   `sls-return-receipt-persistence.mapper.ts` (RNR). Diperbaiki → `'UNPAID'`.
2. **`select: SELECT` ({id,code,**name**}) dipakai untuk reference ke model
   transaksi** (`erpSlsInvoice`, `erpSlsReturn`) di `sls-return-enrich.ts`
   dan `sls-return-receipt-enrich.ts` — **tidak ada model transaksi
   manapun di skema ini yang punya kolom `name`** (semua pakai `code`/
   `docNumber`). Prisma menolak select itu di runtime dengan "Unknown
   field `name`". Diperbaiki: query terpisah `select: {id, code,
   docNumber}` lalu map manual `name: docNumber` (pola yang SI dan SIE
   sudah pakai dengan benar sejak awal — developer tahu soal ini di 2 file
   itu tapi lupa terapkan konsisten di SR/RNR).

**Audit lanjutan membuktikan ini TIDAK meluas ke file lain:** di-grep
seluruh `*enrich*.ts` yang pakai `select: SELECT` — semua sisanya memang
model master data yang benar punya `name` (`erpAccount`, `erpItem`,
`erpBranch`, dst). Hanya SR dan RNR yang kena kombinasi dua bug ini.

**Implikasi gabungan dengan temuan SI:** ketiga modul (SI, SR, RNR) yang
sesi ini saya "selesaikan" posting-nya (GL/stok) kemungkinan **tidak
pernah benar-benar dites lewat service layer sebelumnya** — semua test
end-to-end saya sepanjang sesi untuk ketiganya memakai raw Prisma langsung
(bypass `.create()`/`.findOne()`), jadi bug-bug ini baru ketahuan sekarang
setelah saya kebetulan menguji `service.create()` asli untuk kasus SI←DO.

**Diverifikasi end-to-end lewat service layer (bukan raw Prisma) untuk
pertama kali:** SR dan RNR masing-masing `service.create()` sukses tanpa
crash, `settlementStatus` benar `UNPAID`.

**Enam belas unit kerja total selesai sesi ini.** Rekomendasi untuk sesi
berikutnya: audit serupa (test `service.create()`/`service.findOne()`
nyata, bukan raw Prisma) untuk PI, PRT/DNR, AR Receipt, AP Payment, AS, AP
yang belum pernah dipanggil lewat service layer penuh dalam sesi ini —
risiko bug tersembunyi serupa belum sepenuhnya dikesampingkan untuk modul
itu.

### § Audit service-layer lanjutan: PI, PRT, AS, AR Receipt, AP Payment, AP — bersih (2026-10-01)

Menjalankan rekomendasi follow-up di atas. `service.create()` +
`service.findOne()` (bukan raw Prisma) dites nyata terhadap database untuk
keenam modul yang belum pernah diverifikasi lewat jalur ini sesi ini:

- **PI** — bersih. Tidak ada reference lintas-transaksi di
  `pur-invoice-enrich.ts`, `settlementStatus` sudah `'UNPAID'` sejak awal.
- **PRT** — bersih, sama alasan dengan PI.
- **AS, AR Receipt, AP Payment, AP (vendor advance)** — bersih. Dicek
  `erp-fin-ar-receipts.service.ts`/`erp-fin-ap-payments.service.ts`
  (keduanya **tidak punya enrich logic sama sekali** — `findRaw`/`one`
  polos, jadi tidak ada vektor bug `select: SELECT` di situ) dan
  `sls-customer-advance-enrich.ts` (semua target `select: SELECT` adalah
  master data murni — partner/branch/currency/paymentTerm/costCenter/
  division/project — tidak ada reference ke model transaksi lain).

**Hasil: tidak ada bug baru ditemukan.** Kombinasi bug `settlementStatus`
salah + `enrich select` salah yang ditemukan di SI/SR/RNR **tidak meluas**
ke enam modul ini. Audit lengkap `service.create()`+`findOne()` lewat
service layer asli (bukan raw Prisma) sekarang mencakup **seluruh 15
transaksi** yang disentuh sesi ini: SI, DO, GRN, RNR, SR, PI, PRT/DNR, AR
Receipt, AP Payment, AS, AP — semua terverifikasi bersih atau sudah
diperbaiki.

### § GRN←PO outstanding-qty (FR-PUR-01) (2026-10-01)

Mirror persis DO←SO/SI←DO, sekarang untuk GRN←PO. **Beda kecil dari dua
pendahulunya:** GRN sudah punya field line **`orderLineId`** sejak awal
(bukan `sourceLineId` generik) — sudah tersimpan tapi tanpa validasi
outstanding sama sekali (pola gap yang sama).

`pur-goods-receipt-outstanding.helpers.ts` (baru):
- `validateSourcePurchaseOrderOutstanding` — dipanggil di `create()` saat
  `dto.orderId` diisi. PO harus `status === 'POSTED'`, setiap `orderLineId`
  harus milik PO itu, dan **outstanding diukur dari `acceptedQty`** (bukan
  `quantity` yang diminta) — qty ditolak QC tidak mengurangi outstanding PO
  karena vendor tetap berutang qty yang diterima, bukan yang diminta.
- `maybeCloseSourcePurchaseOrder` — set `closedDate` (bukan `status`, sama
  alasan `CLOSED` tidak ada di `ErpDocumentStatus`) begitu semua baris PO
  fully-received.

**Diverifikasi end-to-end terhadap database nyata:** PO qty=10 POSTED →
GRN1 terima 6 (sukses) → GRN2 coba terima 5 lagi (11>10, **ditolak**) →
GRN3 terima sisa 4 pas (sukses) → PO `closedDate` otomatis terisi.

**Tujuh belas unit kerja total selesai sesi ini.**

### § PI←GRN outstanding-qty (FR-PUR-03 billed-side) (2026-10-01)

Mirror SI←DO, sekarang PI←GRN. `goodsReceiptLineId` ditambah ke
`PurInvoiceLineDto` (belum ada di DTO walau sudah ada di schema line),
diteruskan `mapInvoiceLine` via `connect` (field punya `@relation`, bukan
scalar langsung — beda dari `sourceLineId` di SI/DO yang scalar polos).
`pur-invoice-outstanding.helpers.ts` (baru):
`validateSourceGoodsReceiptOutstanding` — GRN harus `POSTED`, outstanding
diukur dari `acceptedQty` GRN line dikurangi total qty PI lain yang sudah
menagih baris itu.

**Diverifikasi end-to-end terhadap database nyata:** GRN acceptedQty=10
POSTED → PI1 tagih 6 (sukses) → PI2 coba tagih 5 lagi (11>10, **ditolak**)
→ PI3 tagih sisa 4 pas (sukses).

**Delapan belas unit kerja total selesai sesi ini.** Outstanding-qty kini
lengkap untuk 4 pasangan: DO←SO, SI←DO, GRN←PO, PI←GRN. Sisa pasangan:
RNR←SI, SR←RNR, DNR/PRT←PI — levelnya sama, kandidat berikutnya.

### § RNR←SI (qty) + SR (nominal) outstanding — koreksi arah rantai (2026-10-01)

**Temuan penting sebelum implementasi:** rencana awal "SR←RNR" **salah
arah**. Cek skema: `ErpSlsReturn` (SR) tidak punya FK ke RNR sama sekali
— yang ada **RNR punya FK opsional `returnId` ke SR** (`ErpSlsReturnReceipt.
returnId`). Arah rantai sebenarnya sesuai PRD tabel Sales: **RNR dibuat
dari SI** (`Manual, SI`), **SR dibuat dari RNR atau SI** (`Manual, RNR,
SI`). Jadi pasangan qty-side yang benar adalah **RNR←SI**, bukan SR←RNR.

**RNR←SI** (`sls-return-receipt-outstanding.helpers.ts`, baru): mirror
persis SI←DO. `sourceLineId` ditambah ke `SlsReturnReceiptLineDto`
(scalar polos, sudah ada di schema line), `validateSourceInvoiceOutstanding`
— SI harus `POSTED`, outstanding diukur dari `SI line.quantity` dikurangi
total qty RNR lain yang sudah menerima retur baris itu.

**SR** (`sls-return-outstanding.helpers.ts`, baru) — **bukan qty-side**,
karena SR sendiri tidak pernah posting stok (`SlsReturnPostingService`
GL-only, RNR sudah pegang stok). Yang relevan FR-SLS-06: **"Nominal SR
tidak boleh melebihi sisa SI ditambah potongan AS/IP yang sudah dipakai."**
`validateSourceInvoiceRemainingBalance` — validasi **nominal** terhadap
`invoiceId` SR: outstanding AR SI = `grandTotal − SUM(alokasi AR Receipt
untuk invoice itu) − SUM(grandTotal SR lain yang sudah dibuat terhadap
invoice itu, status aktif)`. **Catatan: "potongan AS/IP" di FR-SLS-06
belum ikut dihitung** — itu perlu nilai `appliedAmount` dari AS yang sudah
dipotong ke SI (field ada tapi belum ada logic pemotongan AS→SI sama
sekali di manapun, infra terpisah, di luar scope pass ini).

**Diverifikasi end-to-end terhadap database nyata:**
- RNR←SI: SI qty=10 POSTED → RNR1 retur 6 (sukses) → RNR2 coba retur 5
  lagi (11>10, **ditolak**) → RNR3 retur sisa 4 pas (sukses).
- SR: SI grandTotal=200000 POSTED → SR1 retur 120000 (sukses) → SR2 coba
  retur 100000 lagi (220000>200000, **ditolak** dengan pesan sisa
  piutang jelas) → SR3 retur sisa 80000 pas (sukses).

**Dua puluh unit kerja total selesai sesi ini.** Outstanding-tracking kini
mencakup 6 pasangan/validasi: DO←SO, SI←DO, GRN←PO, PI←GRN, RNR←SI, SR
(nominal). Sisa: DNR/PRT←PI (Purchasing return chain, cek dulu arahnya
sebelum implementasi — jangan ulangi kesalahan arah SR←RNR).

### § DNR/PRT outstanding — qty-side dari GRN + nominal-side dari PI (2026-10-01)

Arah rantai dicek dulu (pelajaran dari kesalahan SR←RNR): `ErpPurReturn`
punya FK **langsung** ke `orderId`, `goodsReceiptId`, **dan** `invoiceId`
— arah benar, tidak perlu dibalik. DNR/PRT = satu model dibedakan
`returnType` (lihat § PRT/DNR posting sebelumnya), jadi tidak ada "DNR
terpisah" untuk dirujuk PRT — guard qty-side relevan justru terhadap GRN
langsung (saat `returnType === 'RETURN_TO_VENDOR'`), bukan "PRT dari DNR".

**Qty-side** (`validateSourceGoodsReceiptReturnOutstanding`, dalam
`pur-return-outstanding.helpers.ts` baru) — mirror RNR←SI: dipanggil hanya
saat `returnType === 'RETURN_TO_VENDOR'` dan `goodsReceiptId` diisi. GRN
harus `POSTED`, outstanding diukur dari `acceptedQty` GRN line dikurangi
total qty retur lain. `goodsReceiptLineId` ditambah ke `PurReturnLineDto`
(punya `@relation`, pakai `connect` sama seperti PI).

**Nominal-side** (`validateSourceInvoiceRemainingPayable`) — mirror SR vs
SI (FR-SLS-06), sekarang FR-PUR-05 direct mode: dipanggil saat `invoiceId`
diisi (independen dari `returnType` — baik DEBIT_NOTE maupun
RETURN_TO_VENDOR bisa punya `invoiceId` direct). Outstanding AP PI =
`grandTotal − SUM(alokasi AP Payment) − SUM(grandTotal PRT lain aktif
untuk invoice itu)`. **Undirect mode (`invoiceId` kosong) sengaja
dilewati** — jadi saldo umum ditarik VPP, tidak ada PI spesifik untuk
dicek.

**Diverifikasi end-to-end terhadap database nyata, kedua jalur:**
- Qty-side: GRN acceptedQty=10 POSTED → DNR1 retur 6 (sukses) → DNR2 coba
  retur 5 lagi (11>10, **ditolak**).
- Nominal-side: PI grandTotal=150000 POSTED → PRT1 retur 80000 (sukses) →
  PRT2 coba retur 80000 lagi (160000>150000, **ditolak** dengan pesan sisa
  utang jelas: 70000).

**Dua puluh dua unit kerja total selesai sesi ini.** Semua enam pasangan/
validasi outstanding-tracking yang direncanakan (DO←SO, SI←DO, GRN←PO,
PI←GRN, RNR←SI, SR, DNR/PRT) **selesai**. Sisa plan: guard anti-double-
posting generik lintas dokumen, dan transaksi ter-gate PRD (RP, PP, SIE
GL, RF, DC, RW, BOM/WO).

### § Guard anti-double-posting generik — "Aturan pembatalan" PRD (2026-10-01)

Infra Fase 0 terakhir yang direncanakan dari awal sesi: "Dokumen yang
sudah dipakai sebagai sumber dokumen lain tidak bisa dibatalkan sebelum
dokumen turunannya dibatalkan" (§Aturan pembatalan, PRD).

`src/erp-common/guards/source-document-lock.helper.ts` (baru) —
`assertNoActiveDerivedDocuments(checks[])`: dipanggil di awal `$transaction`
REOPEN sebelum `reverseLedger` dipanggil. Tiap `check` = satu kemungkinan
dokumen turunan (label untuk pesan error + Prisma `findFirst` delegate +
where clause "menunjuk ke dokumen ini, status masih aktif"). **Sengaja
tidak introspeksi relasi Prisma otomatis** — field FK "ini sumberku"
berbeda nama per pasangan (`orderId`/`deliveryOrderId`/`goodsReceiptId`/
`invoiceId`), jadi tiap call site menyatakan check-nya sendiri daripada
helper menebak. `INACTIVE_STATUSES = ['VOID', 'CANCELLED', 'REJECTED']` —
dokumen turunan dengan status itu tidak menghalangi (sudah dianggap batal).

**Dipasang di 6 modul (REOPEN, sebelum reverseLedger):**
- SO: cek DO + AS aktif.
- DO: cek SI aktif.
- SI: cek RNR + SR aktif.
- PO: cek GRN + PI aktif.
- GRN: cek PI + PRT aktif.
- PI: cek PRT aktif.

**Diverifikasi end-to-end terhadap database nyata, 2 pasangan (lintas
domain Sales & Purchasing untuk pastikan konsisten):**
- SO→DO: SO POSTED dengan DO aktif → REOPEN SO **ditolak** dengan pesan
  jelas "sudah ditarik oleh Delivery Order ... masih aktif" → DO di-VOID
  → REOPEN SO berhasil.
- GRN→PI: GRN POSTED dengan PI aktif → REOPEN GRN **ditolak** → PI di-
  CANCELLED → REOPEN GRN berhasil.

**Belum dipasang (di luar scope pass ini, kandidat lanjutan):** RNR, SR,
PRT/DNR sendiri belum punya guard REOPEN terhadap turunannya (RNR/SR
sendiri biasanya adalah ujung rantai, jarang punya turunan lagi — kecuali
SIE yang di-gate). VP/IP/AS/AP (fin) juga belum dipasang guard ini (saat
di-REOPEN, allocation row dihapus otomatis sudah oleh posting service
masing-masing, jadi risiko lebih kecil — tapi belum diverifikasi formal).

**Dua puluh tiga unit kerja total selesai sesi ini.** Seluruh scope Fase 0
yang direncanakan (outstanding-tracking 6 pasangan + guard anti-double-
posting generik) **selesai**. Sisa plan: transaksi ter-gate PRD (RP, PP,
SIE GL, RF, DC, RW, BOM/WO) — semuanya butuh klarifikasi proses bisnis
dari user sebelum bisa dikerjakan, sesuai gate yang sudah ditetapkan sejak
plan awal.

### § SIE (Invoice Swap) — gate dibuka, klarifikasi user diterima (2026-10-01)

Klarifikasi user: **SIE = realokasi saldo antar invoice** (sesuai asumsi
yang sudah tertanam di komentar kode lama), bukan "ganti faktur salah
terbit". Gate dibuka, posting GL diimplementasikan.

`sls-invoice-swap-posting.service.ts` — per baris swap: **Cr**
`fromInvoice.receivableAccountId` (fallback customer) **/ Dr**
`toInvoice.receivableAccountId` (fallback customer) — murni reklasifikasi
AR, nominal sama (tidak ada perubahan total, sesuai sifat "swap"). Tiap
leg bawa `partnerId` invoice masing-masing, bukan satu partner — penting
karena `fromInvoice`/`toInvoice` **bisa beda customer** (field header
`swap.customerId` hanya kontak utama dokumen, bukan pembatas).

**`toInvoiceId` nullable di schema tapi WAJIB diisi di validasi ini** —
baris tanpa tujuan berarti write-off, yang butuh akun kontra tersendiri
(di luar scope, berbeda dari "swap" yang murni reklasifikasi tanpa ubah
nilai). Ditolak eksplisit dengan pesan jelas, bukan di-skip diam-diam.

**Diverifikasi end-to-end lewat service layer penuh (SUBMIT→APPROVE→
POST→REOPEN):** SI A grandTotal=100000 (customer A) + SI B grandTotal=
50000 (customer B, beda customer) → SIE realokasi 30000 dari A ke B →
2 baris ledger balanced (Cr 30000 akun AR customer A / Dr 30000 akun AR
customer B, partnerId berbeda per baris) → REOPEN → ledger terhapus bersih.

**Dua puluh empat unit kerja total selesai sesi ini.**

### § PP (Freight Payable) — gate dibuka, klarifikasi user diterima (2026-10-01)

Klarifikasi user: ongkos kirim **terpisah dari HPP** — dicatat sebagai
beban operasional, tidak menambah nilai persediaan. Gate dibuka.

Sama pola dengan AP (Vendor Advance): `erp-pur-freight-payables` **tidak
punya model Prisma sendiri** — menumpang `fin_ap_payments`, `source='PP'`.
Levelnya sama dengan AP sebelum dikerjakan: `transition()` ada tapi
`postingStatus` stuck `UNPOSTED`, tanpa `$transaction`, `NEXT` map tanpa
`POSTED: { REOPEN }`.

**GL (arah dikonfirmasi user):** **Dr** Beban Angkut Pembelian (metadata
`expenseAccountId`, tidak ada kolom khusus) **/ Cr** Kas-Bank
(`bankAccountId`, kolom asli di `fin_ap_payments`, dipakai bersama VP/AP).
**sourceDocType terpisah** (`fin_ap_payments_freight_payable`) dari VP
(`fin_ap_payments`) dan AP (`fin_ap_payments_vendor_advance`) — tiga
"source" berbagi satu tabel+id space, isolasi namespace wajib supaya
REOPEN salah satu tidak menyentuh ledger yang lain.

**Bug fix sekalian** (sama seperti AP): `transition()` dibungkus
`$transaction` + `NEXT['POSTED']` ditambahkan.

**Diverifikasi end-to-end lewat service layer penuh:** PP dibuat (amount
750000) → SUBMIT→APPROVE→POST → `postingStatus` benar `POSTED` → 2 baris
ledger balanced (Dr 750000 Beban Angkut/Cr 750000 Bank) → **dikonfirmasi
ledger PP tidak bocor ke namespace VP maupun AP** → REOPEN berhasil,
ledger terhapus bersih.

**Dua puluh lima unit kerja total selesai sesi ini.**

### § RP (Freight Receivable) — modul baru dari nol + migrasi skema (2026-10-01)

**Satu-satunya unit kerja sesi ini yang butuh migrasi skema.** Berbeda
dari PP (sudah ada skeleton), RP **tidak punya model/tabel/controller/
service apapun** di seluruh codebase — grep `'RP'` nihil total. User
diberi pilihan eksplisit (migrasi baru vs tumpangi `fin_ar_receipts` vs
skip) dan **menyetujui migrasi baru**.

**Model baru:** `ErpSlsFreightReceivable` (`sls_freight_receivables`,
domain `sls` karena ini tagihan ke customer, bukan entri finance generik —
konsisten dengan `sls_customer_advances`). **Sengaja TIDAK** mirror
`fin_ar_receipts` (instrument+allocation) — RP murni dokumen tagihan
sebelum ada kas sama sekali (beda dari AR Receipt yang representasi
penerimaan kas); mirror `ErpSlsCustomerAdvance` (standalone, tanpa baris
item, tanpa instrument).

**Migrasi additive** (`20261001_001_erp_sls_freight_receivables`): `CREATE
TABLE` baru, nol `DROP`/`ALTER` ke tabel existing. **Catatan proses migrasi
penting:** `prisma migrate dev` gagal karena shadow database mereplay
histori migration lama yang sudah tidak valid (`clinic_client` tidak ada —
masalah pre-existing, bukan dari perubahan ini). **Solusi**: tulis SQL
manual, `npx prisma db execute --file ... --schema prisma/schema` untuk
apply langsung ke DB (skip shadow DB), lalu `npx prisma migrate resolve
--applied <nama>` untuk mencatat status di `_prisma_migrations`, lalu
`npm run db:generate`. Pola ini disebut CLAUDE.md root ("Migrasi ERP =
hand-written SQL + prisma migrate deploy") — dipakai pertama kali sesi ini.

**`npx prisma format` punya efek samping**: mengubah whitespace alignment
di `erp-md.prisma` (file lain yang tidak disentuh) — **di-revert** sebelum
commit supaya diff tetap minimal dan fokus hanya ke perubahan yang
dimaksud. **Catatan untuk sesi depan:** jalankan `git diff --stat` setelah
`prisma format`/`generate` sebelum `git add`, jangan asumsikan command itu
hanya menyentuh file yang baru diedit.

**GL (arah dikonfirmasi user, sama seperti PP):** **Dr** Piutang Usaha
(`receivableAccountId`, fallback customer) **/ Cr** Pendapatan Jasa Angkut
(`incomeAccountId`) — ongkos kirim terpisah dari penjualan barang/HPP.

Modul lengkap: DTO (create/update/query/transition), service (CRUD +
transition dengan `$transaction` + `NEXT['POSTED']` sejak awal — tidak
perlu ditemukan ulang bug-nya), controller, module, posting service.
Didaftarkan di `app.module.ts`.

**Diverifikasi end-to-end lewat service layer penuh (query database nyata
terhadap tabel baru):** RP dibuat (amount 600000) → SUBMIT→APPROVE→POST →
2 baris ledger balanced (Dr 600000 Piutang/Cr 600000 Pendapatan Jasa
Angkut) → REOPEN → ledger terhapus bersih → `findOne` dengan customer
enrich berhasil.

**Dua puluh enam unit kerja total selesai sesi ini.** RP dan PP (Freight
Receivable/Payable) kini lengkap — kedua gate PRD soal ongkos kirim
terbuka dan terselesaikan.

### § Status akhir gate PRD (checkpoint, 2026-10-01)

Dari 7 transaksi/isu yang di-gate PRD sejak plan awal:
- **RP, PP** — **selesai** (sesi ini, setelah klarifikasi user: terpisah
  dari HPP).
- **SIE** — **selesai** (sesi ini, setelah klarifikasi user: realokasi
  saldo antar invoice).
- **RF, DC, RW** — **tetap gated** sesuai keputusan eksplisit user
  (proses bisnis belum dikonfirmasi, jangan ditebak).
- **BOM/WO** — **tetap gated** sesuai plan awal (PRD sendiri bilang
  "usulan awal, belum ada di diagram kerja").

Fase 0 (outstanding-tracking 6 pasangan + guard anti-double-posting) dan
Fase 1/2 (semua transaksi non-gated Sales+Purchasing) **selesai total**.
Sisa scope PRD yang genuinely belum bisa dikerjakan: RF/DC/RW/BOM/WO,
semuanya menunggu klarifikasi proses bisnis dari user/tim operasional —
bukan lagi keputusan yang bisa diambil sendiri tanpa menebak.


### § FR-FIN-04 — periode CLOSED memblokir posting & pembatalan (2026-10-01)

Sebelumnya hanya GRN yang cek periode. Sekarang guard terpusat
`erp-common/utils/ledger-period-guard.ts`: `assertLedgerRowsPeriodOpen(tx, rows)`
dipanggil sebelum **setiap** `erpFinLedgerEntry.createMany*` (17 posting service)
dan `assertSourceLedgerPeriodOpen(tx, where)` sebelum setiap `deleteMany`
(reverse; 3 service langsung + `reverseInvLedger`). Status yang diblok = `CLOSED`
saja; `SOFT_CLOSED` tetap boleh (sengaja — soft close = peringatan, bukan kunci).
**Catatan:** reversal masih pola hard-delete (bukan jurnal balik bertanggal
pembatalan seperti PRD) — gap terpisah, belum dikerjakan.

### § FR-SLS-04 — SI memotong saldo Customer Advance (AS) (2026-10-01)

`sls-invoice-advance.helpers.ts`: saat SI POST dengan `advanceId`+`advanceAmount`,
validasi AS (POSTED, pelanggan sama, potongan ≤ sisa saldo & ≤ grandTotal), leg
`Dr Uang Muka Penjualan` ditambahkan dan Piutang berkurang sebesar potongan
(jurnal tetap balance); `AS.appliedAmount` naik + `settlementStatus` AS diupdate.
Reopen/re-post → `releaseInvoiceAdvance` membalik. Outstanding AR Receipt
(`ar-receipt-posting`) kini = grandTotal − potongan AS − alokasi. Akun uang muka:
`invoice.advanceAccountId` → fallback `AS.metadata.advanceAccountId`.
**Belum:** potongan dari IP (Payment Receipt) — IP tidak punya field saldo yang
bisa dipotong SI; perlu keputusan bila dibutuhkan. Hanya 1 AS per SI (skema).

### § Laporan kontrol — rekonsiliasi AR/AP vs akun kontrol GL (2026-10-01)

`GET /erp/fin/reports/control-reconciliation?asOf=&branchId=`
(`erp-fin-reports/control-reconciliation.service.ts`). Subledger AR = Σ SI POSTED
− potongan AS − Σ SR POSTED − Σ alokasi AR Receipt; AP = Σ PI − Σ PRT − Σ alokasi
AP Payment. GL = saldo ledger (≤ asOf) pada akun piutang/utang yang dipakai
dokumen & master partner. `isBalanced` toleransi 0,01. **Belum:** rekonsiliasi
stok (Σ nilai saldo stok vs akun persediaan) dan halaman frontend; endpoint
**belum diuji terhadap data live** (hanya typecheck) — jalankan dulu sebelum
dipercaya untuk migrasi data lama.

### § FR-FIN-06 — Opening Balance (CoA) seimbang dengan opening subledger (2026-10-01)

`erp-fin-journal-entries/journal-opening-control.helpers.ts`, dipanggil dari
`JournalPostingService.postToLedger` untuk `journalType=OPENING_BALANCE`. Bila
jurnal menyentuh akun kontrol yang punya saldo awal subledger (SI/PI
`isOpeningBalance` POSTED, baris Opening Stock POSTED), saldo akun itu (jurnal
ini + jurnal opening POSTED lain) wajib = Σ subledger (toleransi 0,01), kalau
tidak POST ditolak. Akun non-kontrol tidak diperiksa.
**Peringatan desain:** dokumen opening subledger (SI/PI/IB) sudah memposting GL
sendiri; jika operator juga memasukkan akun kontrol yang sama di jurnal CoA, saldo
GL akan dobel. Pilih satu jalur per akun kontrol (subledger *atau* jurnal CoA).

### § Penomoran dokumen — increment atomik (2026-10-01)

Audit PRD "tanpa nomor lompat saat simpan bersamaan": semua pembangkit nomor
(`getNextNumber` + 43 `genDocNumber` per modul) membaca `nextNumber` lalu
`update { nextNumber: seq + 1 }` → dua simpan bersamaan bisa dapat nomor sama
(tabrakan unique `doc_number`). Sekarang `update { nextNumber: { increment: 1 } }`
dan nomor = nilai hasil update − 1: row lock UPDATE menserialkan pemanggil, dan
rollback transaksi ikut membatalkan increment (tidak ada nomor lompat). Tetap
per `documentCode` — penomoran per cabang/periode (PRD) belum ada di skema.
Belum diuji dengan beban konkuren nyata.

### § FX Revaluation — FR-FIN-05 (2026-10-02)

Modul `apps/api-gateway/src/erp-fin-fx-revaluations/` (`POST erp/fin/fx-revaluations/run`,
`GET erp/fin/fx-revaluations`). Per periode fiskal: saldo akun bermata uang asing
(`md_accounts.currencyId` ≠ base; kas/bank, kontrol piutang/utang) dari ledger POSTED
s/d akhir periode (Σ debitFx−creditFx vs Σ debit−credit) dinilai ulang ke kurs
`md_currency_rates` terakhir ≤ tanggal akhir periode. Selisih belum terealisasi
diposting ke akun laba/rugi dari `sys_settings` (module=`finance`, group=`accounts`,
key `fxUnrealizedGainAccountId` / `fxUnrealizedLossAccountId`) — ditolak bila belum diset
atau kurs hilang. Hasil disimpan di `fin_fx_revaluation_runs/lines`; reversal otomatis
dengan tanggal hari pertama periode berikutnya (sourceDocType `fin_fx_revaluation_reversals`)
bila periode itu ada. Satu run COMPLETED per periode. Guard periode CLOSED (FR-FIN-04) berlaku.
Logika murni di `fx-revaluation.helpers.ts` (+spec). Saldo debit-signed: utang naik = rugi.
**Belum:** dua key setting belum di-seed/UI Setting; halaman FE `fin-revaluations-page`
masih wrapper jurnal (belum tombol "Jalankan revaluasi"); service belum diuji terhadap DB nyata.

### § Jurnal pembalik bertanggal — helper `reverseWithOffset` (2026-10-02)

`erp-inv-gl/ledger-offset.helpers.ts`: salin baris ledger sumber dengan debit/kredit
(dan Fx) ditukar, bertanggal & berperiode baru, `sourceDocType = <tipe>:void`, docNumber
`<no>-V`; baris asli tidak disentuh; menolak pembalikan ganda dan periode CLOSED.
**Temuan:** SI dan GRN **tidak punya aksi VOID/cancel** — hanya REOPEN (POSTED→DRAFT) yang
hard-delete ledger (tetap dipakai untuk koreksi di periode sama). Helper ini belum
di-wire ke dokumen mana pun; menambah aksi VOID butuh keputusan (lihat di bawah).
**Keputusan tertunda:** VOID SI juga harus membalik pergerakan stok (HPP), uang muka (AS) &
alokasi receipt; VOID GRN harus membalik stok & memblokir bila sudah ada PI/Return aktif.

### § VOID Sales Invoice — jurnal pembalik bertanggal (2026-10-02)

Aksi baru `VOID` (POSTED → VOID, `reason` wajib) di `erp-sls-invoices`. Dipakai
`SlsInvoicePostingService.voidLedger` + `reverseWithOffset`: jurnal AR/pendapatan/pajak SI
dan jurnal HPP movement turunan dibalik dengan baris offset bertanggal **hari ini** di
periode yang memuatnya (baris asli tak berubah → periode lampau/CLOSED aman); movement
stok turunan di-set `VOID` (keluar dari on-hand & moving average, yang memfilter
`status='POSTED'`); potongan uang muka (AS) dikembalikan. SI diset `status=VOID`,
`postingStatus=UNPOSTED` (laporan AR & rekonsiliasi kontrol memfilter `POSTED`, jadi SI
VOID otomatis keluar dari subledger sementara GL net nol). Diblokir bila sudah ada
alokasi AR Receipt aktif atau Return/Return Receipt aktif (`sls-invoice-void.helpers.ts`).
REOPEN tetap hard-delete (koreksi sebelum final). Frontend: item kebab **Void** (danger, hanya status POSTED) di `sls-invoices-page.tsx` →
`confirmAction` + prompt alasan → `transition VOID`. **Belum:** VOID GRN (blokir bila ada PI/
Return aktif, balik stok) & dokumen lain; belum diuji ke DB nyata.

---

## § Scope produk MVP CV Bahtera Madani (2026-10-04)

Sumber: `temp/PRD-ERP-CV-Bahtera-Madani-A4.pdf`. Untuk deployment CV Bahtera
Madani, **MVP produk = Fase 1 “Fondasi & pengadaan sekolah”**: A1 CRM Sekolah,
A2 Order Hub, A3 Dokumen pengadaan, A4 Pajak pengadaan, D1 Katalog,
D2 Pembelian, D3 Persediaan multi-gudang, dan E1 Keuangan & akuntansi. Scope
otoritatif, gap terhadap kode saat ini, dan urutan MVP-0–5 ada di
`docs/bahtera-madani-mvp-scope.md`.

**Keputusan 2026-10-04 — Yayasan ditunda, sekolah jadi entitas utama.** PRD
menggambarkan hierarki yayasan → unit sekolah, tapi **Yayasan bukan syarat
gerbang MVP**. Transaksi (order, invoice, BAST, pengiriman, sumber dana BOS,
piutang) selalu melekat pada **sekolah**, bukan yayasan. Yayasan hanya
relasi opsional satu-ke-banyak untuk negosiasi grup, kontrak pusat, dan
laporan konsolidasi. Biaya integrasi foundation (tabel yayasan, hierarki,
perubahan CRM/pipeline/filter/piutang) tinggi dan manfaatnya baru terasa
saat ada pelanggan grup → yayasan **dipindahkan ke Fase 3/B1**. MVP-1 =
sekolah saja, tanpa yayasan. Lihat §7 di `bahtera-madani-mvp-scope.md`.

Keputusan batas:

- P2P/O2C/giro dan baseline konfigurasi yang sudah dikerjakan adalah **fondasi
  teknis**, bukan keseluruhan MVP produk.
- Pada MVP, kanal aktif adalah input admin, input sales, dan SIPLah melalui API
  merchant bila tersedia atau impor file bila tidak. Portal sekolah dan portal
  orang tua tetap Fase 3; Order Hub harus channel-ready agar portal kelak menjadi
  producer baru tanpa mengganti engine transaksi.
- Kapabilitas vertikal sekolah wajib mereuse domain `sls`/`pur`/`inv`/`fin`,
  Report Studio, attachment, dan workflow existing; dilarang membuat fork atau
  engine transaksi kedua.
- Gerbang MVP adalah **satu periode BOS berjalan penuh tanpa rekap operasional
  Excel paralel**, dibuktikan dengan alur end-to-end dan rekonsiliasi order,
  stok, AR/AP, pajak, bank/pencairan, serta GL.
- Percetakan penuh = Fase 2; portal dan mesin harga kontrak penuh = Fase 3;
  SDM, fixed assets, advanced analytics, dan AI = Fase 4.

Open decisions PRD dan kebutuhan pilot-specific (API SIPLah, volume/cohort,
rabat, konsinyasi, barcode, alokasi eksklusif, PPh 22/23) wajib dikonfirmasi
sebelum estimasi final atau penguncian workstream terkait.
