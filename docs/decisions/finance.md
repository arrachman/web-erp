# DECISIONS — Finance (M2) — kas/bank masuk-keluar & memo

> Bagian dari decision log Web-ERP. Dipindahkan dari `DECISIONS.md`
> (2026-10-04) agar file indeks ramping; **isi entri tidak diubah** dan
> nomor `§` dipertahankan sebagai anchor stabil. Indeks semua entri:
> [`DECISIONS.md`](../../DECISIONS.md).

---

## § Finance (M2) menu parity — koreksi label + folder Laporan (2026-05-31)

Audit menu **Keuangan ▸ Transaksi** legacy MyERP+ vs seed kita (`prisma/seed-erp.ts`
grup `M2`). Legacy "Transaksi" = 11 item: Kas Masuk (CR), Kas Keluar (CD), Bank
Masuk (RM), Bank Keluar (SM), Jurnal Umum (GJ), Giro Masuk/Keluar (RG/SG), Giro
Masuk/Keluar Batal (RGC/SGC), Saldo Awal Coa (CB), Buku Besar (GL = laporan).

**Koreksi label seed (kontradiksi legacy + dok desain `entities-m2-finance.md`):**
- **CB** legacy = **Saldo Awal Coa / Opening Balance** (→ `JournalType.OPENING_BALANCE`),
  bukan "Cash/Bank Transfer". Seed lama salah label → diperbaiki jadi
  `M2.TX.OPENING-BALANCE` "Opening Balance (CoA)" `/finance/opening-balances`.
- **RM/SM** legacy = **Bank Masuk/Bank Keluar**; label "Receipt Memo/Send Memo"
  membingungkan → diganti **Bank Receipt** (`RM`) / **Bank Payment** (`SM`),
  path `/finance/bank-receipts` & `/finance/bank-payments`.
  > **⚠️ Koreksi model RM (2026-05-31, lihat § Bank Masuk di bawah):** rencana
  > awal memetakan RM → `fin_ar_receipts` (AR settlement). Setelah lihat layar
  > legacy "Bank Masuk (RM)" (header + baris kontra CoA + Total, identik Kas
  > Masuk), keputusan dengan user: **RM = twin Kas Masuk** di
  > `fin_cash_bank_transactions` (`kind=BANK`, `direction=RECEIPT`), **bukan**
  > `fin_ar_receipts`. `fin_ar_receipts` dicadangkan untuk settlement AR murni
  > bila dibutuhkan, bukan untuk path `/finance/bank-receipts`.
- **BD** (Bank Disbursement) & **AJ** (Adjustment Journal) = tambahan modern,
  **tak ada** di menu Transaksi legacy ini — dipertahankan (didukung skema).

**Folder Laporan (M2.RPT) ditambah.** Legacy MODULEID=2 (`Report.vb`) punya ~150
varian laporan; di seed kita di-kanonkan jadi parent report yang dipenuhi
`fin_ledger_entries`/`fin_budget_realizations`/`fin_giros`: General Ledger,
Trial Balance, Balance Sheet, Income Statement, Cash Flow, Daily Cash & Bank,
AR Card, AR Aging, AP Card, AP Aging, Giro Maturity, Budget vs Realization
(legacyCode = `2-<MENUID>`). Sebelumnya M2.RPT cuma punya General Ledger.

**Tambahan 2 laporan finance (2026-06-07).** Audit ulang `m0_report` legacy modul
Keuangan (1.301 report; mayoritas cetakan transaksi) → jenis laporan keuangan
sejati yang belum live tinggal **dua**: **Neraca Mutasi** (`M2.RPT.MOVEMENT-BALANCE`,
`/finance/movement-balance`, legacy `neracamutasi`) = trial-balance-with-movement
(Saldo Awal · Debit · Kredit · Saldo Akhir per akun, konvensi debit-positif) dan
**Perubahan Modal** (`M2.RPT.EQUITY-CHANGES`, `/finance/equity-changes`) = statement
of changes in equity (Saldo Awal · Mutasi · Saldo Akhir per akun EQUITY + baris
Laba/(Rugi) Tahun Berjalan). Dibangun lewat subsistem **live** `erp-fin-reports`
(builder `statement-builders.ts` → `service.buildMovementBalance/buildEquityChanges`
→ controller `GET /erp/fin/reports/{movement-balance,equity-changes}` → `ReportDocument`
→ `ReportPage` wrapper tipis + route `ERP_PAGES`/`ERP_ROUTE_META` + `ReportKey`).
**Catatan arsitektur penting:** menu Laporan finance yang LIVE = subsistem
`erp-fin-reports` (`ReportPage`→`getReport`→backend SQL nyata), bukan
`financial-report.tsx`/`REPORTS`/`NAV` (itu **fallback statis basi** dengan data
mock — jangan dipakai untuk report baru). Seed `seed-erp.ts` grant semua menu ke
SUPERADMIN → 2 entri langsung tampil. Smoke-test authenticated OK (2026-06-07).

**Cakupan DB:** seluruh 11 item legacy ter-cover oleh 31 tabel `fin_*` (jauh di
atas legacy). Yang belum: **frontend M2 belum dibangun** (path `/finance/*`
ter-seed tapi belum ada entry di `ERP_PAGES`/`ERP_ROUTE_META`).

**Drift live DB (perlu prune).** `sys_menus` di DB akumulasi entri M2 usang dari
iterasi seed lama (seed tak punya prune): `M2.TX.CASHBANK-TRANSFER`,
`M2.TX.RECEIPT-MEMO`, `M2.TX.SEND-MEMO` (digantikan koreksi di atas) + duplikat
prefix `/keuangan/` lama: `M2.TX.AP-PAYMENT`, `M2.TX.AR-RECEIPT`, `M2.TX.GIRO`,
`M2.TX.JOURNAL`. FK `adm_role_menus.menu_id` = `ON DELETE CASCADE`, jadi prune
baris `sys_menus` aman (role-map ikut terhapus). Re-seed menambah kode baru tapi
**tidak** menghapus yang usang — prune manual diperlukan.

## § Kas Masuk / Cash Receipt (CR) — transaksi fin pertama (2026-05-31)

Fitur transaksi master-detail penuh pertama di M2 Finance — dibangun dari UI legacy
MyERP+ "Kas Masuk (CR)" tapi pakai design system + standar Senti (§2.7/§2.9).
Keputusan dengan user: **posting GL sekarang**, **state machine Senti**, **backend
cash-bank shared (wire CR dulu)**.

**Status enum diperluas (additive).** `ErpDocumentStatus` ditambah `NEED_APPROVE`,
`APPROVED`, `REJECTED` (migrasi `20260531_004_erp_document_status_workflow`,
`ALTER TYPE ADD VALUE`) agar DB sejalan dengan `lib/status.ts` (5-status canonical)
dan mendukung state machine §2.7. Dipakai semua dokumen fin/inv/pur/sls.

**Backend = 1 modul shared `erp-fin-cash-bank-transactions`** (melayani
RECEIPT/DISBURSEMENT via enum `direction`; CD/BD nyusul gratis). Endpoint
`/erp/fin/cash-bank-transactions` (`ErpJwtAuthGuard`). Pola:
- `create`: `docNumber` auto via `sys_document_numberings` (code `CASH_RECEIPT`,
  prefix `CR`) saat `auto=true`; `fiscalPeriodId` **diturunkan dari
  transactionDate** (cari periode yang memuat tanggal — tidak dipilih manual);
  `amount` header = Σ baris (server-side, tak percaya klien).
- **Workflow** `transition` (state machine): DRAFT→NEED_APPROVE→APPROVED→POSTED
  (+REJECTED, +REOPEN). Edit hanya saat DRAFT/NEED_APPROVE/REJECTED; POSTED tak
  bisa dihapus (reopen dulu).
- **Posting GL** (`cash-bank-posting.service.ts`): saat POST → generate
  `fin_ledger_entries` balanced — RECEIPT = **Dr Akun Kas (header)** + **Cr tiap
  baris**; DISBURSEMENT kebalikannya. Periode `CLOSED` ditolak. REOPEN
  hard-delete ledger milik dokumen (re-post idempoten). Validasi Σbaris=header.
- Cross-domain FK (partner/account/branch/currency) = **scalar tanpa @relation**
  → di-enrich code+name server-side (`cash-bank-enrich.ts`) agar list bawa nama.
- **E2E terverifikasi (2026-05-31):** create→submit→approve→post menghasilkan
  3 ledger entries balanced (Dr 455.000 = Cr 300.000+155.000).

**Frontend.** Editor baris kas/bank = organism reusable
[`components/organisms/cash-bank-lines.tsx`](components/organisms/cash-bank-lines.tsx)
— **satu kolom Total per baris** (No Akun · Nama · Total · Total Valas · Catatan ·
Cost Center), bukan debit/kredit (beda dari `JournalLinesEditor` jurnal umum).
SearchSelect CoA "code - name" (`loadAccountOptionsCoded`) + cost center, `NumInput`,
total footer. Form [`fin-cash-receipts-form.tsx`](components/pages/fin-cash-receipts-form.tsx):
header SearchSelect (Terima Dari/Akun Kas/Cabang/Lokasi) + tab Detail/Info + Total;
**Status read-only (badge), transisi via aksi** (§2.7). List
[`fin-cash-receipts-page.tsx`](components/pages/fin-cash-receipts-page.tsx) §2.7:
kolom legacy (No Transaksi link, Tanggal, Terima Dari, Total, Uang, Kurs, Status),
filter status + rentang tanggal, kebab + context menu workflow actions, bulk hapus,
keyboard nav, list↔form mode (back-arrow). `lib/api/fin-cash-receipts.ts` diselaraskan
ke endpoint shared (`direction=RECEIPT`) + `transitionCashReceipt`.

**Dihapus:** prototype `kas-masuk-list.tsx` + `kas-masuk-list-parts.tsx` (mock
client-side, orphaned, langgar §2.12) + route legacy `'kas-masuk'`.

**Filter list = paritas legacy (2026-05-31).** Panel filter
[`fin-cash-receipts-filters.tsx`](components/pages/fin-cash-receipts-filters.tsx)
(`CashReceiptFilters`): No Transaksi (range), Status, Tanggal (range), Terima Dari,
Lokasi, Cabang, Uraian, Catatan, User — semua **server-driven** (debounce 350ms) +
reset filter, plus search global di header. Backend query DTO menambah
`docNumberFrom/To`, `description`, `notes`, `createdById` (partner/branch/location
sudah ada). Pola filter kaya transaksi: panel terpisah di atas tabel (bukan
`FilterConfig` dropdown ErpListLayout yang cuma cocok untuk master sederhana).

**Header form = label inline 1 baris (2026-05-31).** `Field` di
[`fin-cash-receipts-form.tsx`](components/pages/fin-cash-receipts-form.tsx) diubah dari
label-di-atas (`flex flex-col`) jadi label-kiri-input-kanan (`flex items-center`, label
`w-24 shrink-0 text-left`) atas permintaan user — tiap field header jadi satu baris.

**Akun Kas [D] = picker akun kas saja (2026-05-31).** Picker `bankAccountId`
dibatasi ke akun kas/bank (sebelumnya semua akun). Paritas filter legacy MyERP+
`cgd='D' and caktif=1 and ctipe=0` → Senti `normalBalance=DEBIT` + `isActive=true`
+ `type=ASSET` (`ctipe` legacy = `AccountType`, nilai 0 = ASSET) + tambahan
`kind=POSTABLE` (header spt "Aset Lancar" tak bisa di-posting GL). Loader baru
`loadCashAccountOptionsCoded` ([items-form-lookups.ts](components/pages/items-form-lookups.ts));
DTO query account `apps/api-gateway` ditambah filter `normalBalance` (enum
`ErpNormalBalance`, additive — tanpa migrasi). Catatan: ini ikut legacy = **semua
aset debit** (termasuk piutang/persediaan), bukan murni kas/bank; belum ada flag
`isCashAccount` di `md_accounts`.

**Detail grid = spreadsheet cell-selection (2026-05-31).** Atas permintaan user,
grid Detail bukan lagi deret input aktif (search "Pencarian CoA", tombol "+ Tambah",
dan kolom trash dibuang). Default tiap cell = **terpilih (highlighted), bukan input**;
edit muncul on-demand. Dipecah jadi 4 file (<400 baris, §3): `cash-bank-line-model.ts`
(tipe + `newCashLine` + `cellColumns`), `cash-bank-line-cell.tsx` (display↔edit per
cell), `use-cash-grid-nav.ts` (state machine keyboard), `cash-bank-lines.tsx`
(organism komposit; re-export model untuk form).
- **Masuk edit (Excel-style):** klik pilih cell; **ketik / Enter / F2 / dobel-klik**
  masuk edit. Mengetik karakter langsung menyemai nilai (num/notes di-`patch`,
  akun/cost-center lewat `initialQuery` SearchSelect).
- **Navigasi:** `↑↓←→` pindah cell (saat tidak edit). Saat edit: **Enter** =
  commit & tetap di cell (keluar edit), **Tab** = commit & pindah kanan/kiri,
  **Esc** = batal (revert snapshot). Panah saat edit = gerak caret di input.
- **Tambah baris:** **Tab** di cell terakhir baris terakhir, atau **↓** di baris
  terakhir → append baris baru. **Hapus baris:** **Ctrl/Cmd+Delete** (sisakan ≥1).
- **Fokus:** root `div` `tabIndex=0` menangkap keydown; setelah nav/exit-edit di-
  refocus via `wantRoot` ref + `useLayoutEffect` (tidak mencuri fokus saat blur).
- **SearchSelect** ditambah prop reusable: `autoFocus`, `initialQuery` (semai
  pencarian saat type-to-edit), dan `onPick(value,label)` (cell butuh label untuk
  render display — `onValueChange` hanya kasih value). Additive; caller lama aman.
Berlaku ke **semua** form yang reuse organism ini (CR/CD/BD).

**Kas Keluar / CD = adopter kedua (2026-05-31).** CD disamakan penuh dengan CR:
endpoint shared `direction=DISBURSEMENT`, URL sub-route (§2.3.1), workflow actions,
slim filter bar + drawer (§2.40), keyboard nav. Karena form & filter CR/CD beda
**hanya label + arah**, keduanya diekstrak jadi organism reusable berparameter
(keputusan user — generik & share, bukan duplikat ~380 baris):
- Form: [`cash-bank-transaction-form.tsx`](components/pages/cash-bank-transaction-form.tsx)
  (`CashBankTransactionForm`, prop `labels:{partner,account}`) + model
  [`cash-bank-form-model.ts`](components/pages/cash-bank-form-model.ts)
  (`toCashBankPayload(d, direction)`). CR/CD form = wrapper tipis.
- Filter: [`cash-bank-filters.tsx`](components/pages/cash-bank-filters.tsx)
  (`CashBankFiltersBar`, prop `entityName`+`partnerLabel`) +
  [`cash-bank-filter-fields.tsx`](components/pages/cash-bank-filter-fields.tsx).
  CR/CD filter = wrapper tipis.
- Label arah: CR = "Terima Dari" / "Akun Kas [D]"; CD = "Bayar Ke" / "Akun Kas [K]"
  (paritas legacy: disbursement = Cr Akun Kas, Dr tiap baris).
- `lib/api/fin-cash-disbursements.ts` ditulis ulang ke endpoint shared
  (`direction=DISBURSEMENT`, reuse tipe CR) + `transitionCashDisbursement`. Modal
  CRUD skeleton lama (entryDate/cashAccountId/ID-input) dibuang. Registrasi pindah
  `ERP_PAGES` → `TRX_FORM_PAGES` (shell-route-renderer). File CR filter-fields lama
  dihapus (digantikan shared).

**Config FIN.CD = mirror FIN.CR (2026-06-02).** Setup data config `FIN.CD` (live
DB, bukan seed) disamakan dengan kurasi `FIN.CR` agar `cash-disbursements/new`
tampil identik dgn Kas Masuk:
- **Grid** (`sys_transaction_grid_columns`, primary grid): 15 kolom default seed →
  9 kolom kurasi CR — kolom `No.` (ROWNUM), `No. Akun` (elastis width 0),
  `Total`, `Total Valas` (hidden), `Catatan`, `Cost Center`, **Divisi / Sub Divisi /
  Proyek visible**; slot kustom (customText/Double/Date) dibuang.
- **Form** (`sys_form_fields`): tambah default record-baru CR yg belum ada di CD —
  `transactionDate.defaultValue=@today` + `currencyId.defaultValue=1` (IDR).
- Field struktural CD sudah lengkap & berlabel arah benar ("Bayar Ke"/"Akun Kas [K]")
  sejak adopter kedua; custom field uji CR ("Field Baru") **tidak** disalin.
- Kurasi grid/form = live-DB only (sama spt CR, lewat UI), **bukan** lewat seed —
  re-run `seed-erp-transaction-grids.ts` me-recreate slot kustom default (perilaku
  existing, berlaku CR & CD).

**Code kanonik Bank = FIN.RM / FIN.SM (2026-06-02).** Ditemukan mismatch: page
Bank pakai `transactionCode` **FIN.RM** (Bank Masuk) / **FIN.SM** (Bank Keluar),
tapi seed lama men-seed **FIN.BR / FIN.BP** → backend `typeByCode` lempar 404 →
config grid/form bank **tak pernah ketemu/tersimpan** (selalu fallback default).
Keputusan user: **FIN.RM/FIN.SM kanonik** (selaras frontend + abbreviation legacy
RM/SM). Tindakan:
- DB: `sys_transaction_types.code` FIN.BR→**FIN.RM**, FIN.BP→**FIN.SM** (grid ikut
  via FK, tak ter-orphan).
- Seed `seed-erp-transaction-grids.ts`: TXNS code disesuaikan ke FIN.RM/FIN.SM.
- Grid FIN.RM & FIN.SM dikurasi mirror CR (9 kolom, sama spt CD).
- Form fields FIN.RM/FIN.SM dibuat (copy 8 struktural CD + default @today/IDR),
  label arah: RM = "Terima Dari" / "Akun Bank [D]"; SM = "Bayar Ke" / "Akun Bank [K]".
- Catatan: label form sekarang dari **config DB** (prop `labels` form hanya feed
  fallback DEFAULT_FORM_FIELDS) — makanya label arah wajib benar saat seed config.
- Empat anggota kas/bank (CR/CD/RM/SM) kini paritas penuh: 8–9 form fields + 9
  kolom grid (8 visible).

**Belum (follow-up):** edit dokumen POSTED auto reverse+repost (sekarang diblok —
reopen dulu); kolom User Input di tabel (filter User sudah ada); FE BD/transfer
belum pakai backend baru ini (CR + CD sudah).


---

## § Bank Masuk (RM) — twin Kas Masuk + Cara Bayar + Giro (2026-05-31)

Build halaman **Bank Masuk / Bank Receipt** (`/finance/bank-receipts`, legacyCode
`RM`), meniru layar legacy MyERP+ (amati-tiru-modifikasi).

**Keputusan dengan user:**
- **Model = twin Kas Masuk**, BUKAN `fin_ar_receipts`. Bank Masuk dibangun di
  atas modul **shared** `erp-fin-cash-bank-transactions` (sama dgn CR/CD), dengan
  diskriminator baru `kind=BANK` + `direction=RECEIPT`. Layar legacy "Bank Masuk
  (RM)" = header + baris kontra CoA + Total, identik Kas Masuk (hanya "Akun Bank
  [D]" + Cara Bayar + tab Giro) — jadi reuse pola Kas Masuk, bukan layar alokasi
  AR. Mengoreksi catatan menu-parity yang sempat memetakan RM → `fin_ar_receipts`.
- **Full parity**: sertakan **Cara Bayar** (enum `ErpPaymentMethod`, default
  `TRANSFER`) + **tab Giro** yang berfungsi.

**Skema (additive, migrasi `20260531_006_erp_cash_bank_kind_payment_method`, 0 DROP):**
- Enum baru `ErpCashBankKind { CASH, BANK }`.
- `fin_cash_bank_transactions` + kolom `kind` (`ErpCashBankKind` NOT NULL default
  `CASH` → baris CR/CD lama otomatis `CASH`) & `payment_method`
  (`ErpPaymentMethod` nullable). Index `(kind, direction, status)`.
- **Doc numbering** di-key per (kind, direction): `BANK_RECEIPT` prefix **RM**
  (seed `seed-erp.ts` + insert idempotent ke DB live). CR/CD tetap.
- **Giro tab** = baris giro disimpan sebagai rekor **`fin_giros`** (type
  `INCOMING`, `source='CASH_BANK_TXN'`, `sourceTransactionId`=id transaksi,
  status `OUTSTANDING`). Sinkron seperti baris kontra: hard delete + recreate saat
  update (dokumen masih editable/pre-post → belum ada clearing), soft-delete saat
  transaksi dihapus. Field per giro: No Giro/Cek, Bank Penerbit, Jatuh Tempo,
  Nominal, Catatan.

**Posting GL:** tidak berubah — RECEIPT tetap Dr akun bank (header) / Cr baris
kontra, apa pun Cara Bayar. **Asumsi/ditunda:** nuansa akuntansi giro-belum-cair
(Dr Giro/Notes Receivable lalu pindah saat clearing) belum dimodelkan; giro di tab
ini = pencatatan instrumen + dasar untuk modul Receipt Giro Clearing (RGC) ke
depan. Eskalasi bila perlu posting giro yang berbeda.

**Backend (shared module `erp-fin-cash-bank-transactions`):**
- DTO create + `kind`/`paymentMethod`/`giros[]` (`CashBankGiroDto`); query DTO
  + `kind`. `genDocNumber(tx, kind, direction)`. Helper `syncGiros`/`loadGiros`;
  `one()` melampirkan `giros`. Guard tetap `ErpJwtAuthGuard` (§2.5).
- Regresi Kas Masuk aman: default `kind=CASH`, jalur CR tak berubah.

**Frontend (adopter §2.3.1 — reuse, jangan fork):**
- `lib/api/fin-bank-receipts.ts` (reuse tipe shared dari `fin-cash-receipts`,
  `kind:'BANK'`, + `paymentMethod`/`giros`).
- `fin-bank-receipts-form.tsx` = **wrapper tipis** atas form shared
  `cash-bank-transaction-form.tsx` (header/Detail/Info dari sana). Dua hal khas
  bank di-inject via slot **baru** form shared: **`headerExtra`** (Cara Bayar
  Select §2.6 → 6 opsi) + **`extraTabs`** (tab Giro = organism reusable
  `components/organisms/cash-bank-giros.tsx`, dipakai bareng Bank Keluar). Slot
  additive → CR/CD tak terpengaruh. Model `fin-bank-receipts-form-model.ts`
  extend `CashBankFormData` + `paymentMethod`/`giros`, reuse mapper shared
  (`toCashBankPayload`). (Editor giro standalone yang sempat dibuat → dihapus,
  diganti organism shared agar tak duplikat.)
- `fin-bank-receipts-page.tsx` (list + router sub-route), reuse filter
  `CashReceiptFilters` + workflow `cashBankWorkflowActions`. Kolom list +
  **Cara Bayar**.
- Routing: daftar `/finance/bank-receipts` di `TRX_FORM_PAGES`
  (shell-route-renderer) + `ERP_ROUTE_META` (`lib/nav.ts`).

## § Bank Keluar (SM) — twin Kas Keluar + Cara Bayar + Giro (2026-05-31)

Build halaman **Bank Keluar / Bank Disbursement** (`/finance/bank-disbursements`,
legacyCode `SM`), meniru layar legacy MyERP+ "Bank Keluar (SM)". Sibling Bank Masuk
(RM, § atas) — arah keluar. Reuse fondasi shared yang sama (jangan fork).

**Keputusan dengan user (2026-05-31):**
- `/finance/bank-disbursements` = **Bank Keluar (SM)** di atas modul **shared**
  `erp-fin-cash-bank-transactions`, `kind=BANK` + `direction=DISBURSEMENT`. Judul
  page + sidebar = **"Bank Keluar"**, code-tag **SM** (override konvensi English
  untuk item ini, atas pilihan user eksplisit).
- **Rapikan duplikat**: entri menu lama `/finance/bank-payments` ("Bank Payment",
  SM→`fin_ap_payments`) **dihapus** dari seed + di-prune dari DB live (idempotent),
  karena duplikat konsep dengan halaman ini.

**Skema:** tak ada perubahan baru — reuse migrasi `20260531_006` (kind +
payment_method + index) dari Bank Masuk. **Doc numbering** key
`BANK_DISBURSEMENT` prefix **SM** (seed `seed-erp.ts` + insert idempotent ke DB
live). **Giro tab** = `fin_giros` type **`OUTGOING`** (derive dari direction),
`source='CASH_BANK_TXN'`, sinkron hard delete+recreate (sama pola Bank Masuk;
dasar modul Send Giro Clearing/SGC ke depan).

**Backend (shared module):** tambah filter **`paymentMethod`** di query DTO +
`where` service (melengkapi `kind` dari Bank Masuk). `genDocNumber`/`syncGiros`/
`loadGiros` sudah generik (dipakai RM & SM). Regresi CR/CD/RM aman.

**Frontend (adopter §2.3.1 — reuse, jangan fork):**
- `lib/api/fin-bank-disbursements.ts` (reuse tipe shared dari `fin-cash-receipts`,
  `direction:'DISBURSEMENT'` + `kind:'BANK'`).
- `fin-bank-disbursements-form.tsx` = **wrapper tipis** atas `cash-bank-transaction-form.tsx`,
  inject **Cara Bayar** (Select §2.6, 6 opsi, default `TRANSFER`) via `headerExtra`
  + **tab Giro** via `extraTabs` (organism shared `cash-bank-giros.tsx`). Label
  bank: "Bayar Ke" + "Akun Bank [K]". Export `paymentMethodLabel` untuk kolom list.
- **Model**: SM pakai **shared `cash-bank-form-model.ts`** langsung —
  `CashBankFormData` diperluas `kind`/`paymentMethod`/`giros`; `defaultCashBankForm('BANK')`
  set `paymentMethod='TRANSFER'`; `toCashBankPayload` kirim `giros` hanya saat
  `kind='BANK'` (cash → `undefined`, backend skip sync). Types di `fin-cash-receipts.ts`
  + `ErpCashBankGiro`/`CashBankGiroPayload`/`ErpPaymentMethod`/`ErpCashBankKind`.
- `fin-bank-disbursements-page.tsx` (list + router sub-route §2.3.1), reuse filter
  bar shared (wrapper `fin-bank-disbursements-filters.tsx`, "Bank Keluar"/"Bayar Ke")
  + `cashBankWorkflowActions`. Kolom list + **Cara Bayar**.
- Routing: `/finance/bank-disbursements` dipindah dari `ERP_PAGES` ke
  `TRX_FORM_PAGES` (shell-route-renderer) + `ERP_ROUTE_META` (`lib/nav.ts`).
- **Ditunda**: filter Cara Bayar di drawer (butuh ubah `cash-bank-filter-fields`
  shared); pass ini cukup **kolom** Cara Bayar di list. Backend filter
  `paymentMethod` sudah siap → tinggal wire field saat melanjutkan.

## Data dummy transaksi finance — wajib POSTED + "terima dari" terisi (2026-06-07)

Keputusan (dengan user): semua data transaksi DUMMY_SEED di menu TRANSAKSI
finance harus **terposting** dan punya **"terima dari" (`partner_id`)**.

- Partner diisi **acak per arah**: RECEIPT / giro INCOMING → partner `is_customer`;
  DISBURSEMENT / giro OUTGOING → partner `is_supplier`; journal (tak berarah) →
  acak dari semua partner.
- Posting lewat **API state machine** (`/transition` SUBMIT→APPROVE→POST), bukan
  flip kolom langsung, supaya `fin_ledger_entries` (GL) ikut ter-generate dgn
  partner. Untuk doc yang sudah posted, `partner_id` dipropagasi ke ledger existing.
- Script repair: `scripts/fix-transactions-posted-and-partner.mjs` (idempotent,
  hanya menyentuh `source LIKE 'DUMMY%'`; giro tanpa kolom source = semua seed).
  Punya retry backoff utk throttle 429.
- Hasil: cash/bank 2219, giro 307, journal 636 → semua POSTED. Tanpa "terima dari"
  tersisa hanya OPENING_BALANCE (1, memang tanpa lawan transaksi) + 1 journal
  manual non-dummy. GL ledger 4693 → 6121.

## Receipt Memo / Send Memo = AR Receipt / AP Payment (2026-06-07)

Menu TRANSAKSI **Receipt Memo** → route `/finance/receipt-memos` → `ErpArReceiptsPage`
→ tabel **`fin_ar_receipts`** (endpoint `/fin/ar-receipts`). **Send Memo** →
`/finance/send-memos` → `ErpApPaymentsPage` → **`fin_ap_payments`**. (Komentar di
`scripts/seed-bank-out-1000.mjs` yg menyebut memo = cash-bank-txn sudah usang.)

AR Receipt = **skeleton CRUD**: `create` insert apa adanya, **tanpa** auto-number,
**tanpa** GL ledger, **tanpa** state machine `/transition`. Jadi "terposting" =
set `status='POSTED'`+`posting_status='POSTED'` langsung di payload/insert; `partner_id`
(customer) wajib = "terima dari".

Seed 2026-06-07: 1000 Receipt Memo POSTED, partner customer acak, tersebar merata
per bulan Jan 2025→Jun 2026 (56×10 + 55×8), doc `RM0000001..RM0001000`,
`source='DUMMY_SEED_RECEIPT_MEMO'`, bank acak 185–189, currency IDR. Insert via SQL
langsung (setara API krn create tak punya side-effect GL). Hapus: delete where
source='DUMMY_SEED_RECEIPT_MEMO'.

---

