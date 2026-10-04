# DECISIONS — Sales, Purchasing & Warehouse (M3/M4)

> Bagian dari decision log Web-ERP. Dipindahkan dari `DECISIONS.md`
> (2026-10-04) agar file indeks ramping; **isi entri tidak diubah** dan
> nomor `§` dipertahankan sebagai anchor stabil. Indeks semua entri:
> [`DECISIONS.md`](../../DECISIONS.md).

---

## § Sales Order (SO) — transaksi sales pertama + grid engine generik (2026-06-02)

Build transaksi **item-based** pertama di m5 Sales — pola **persis cash/bank**
(config-driven header via Form Builder + grid baris via Kustomisasi Grid + §2.36
layout + state machine §2.7 + sub-route URL §2.3.1), tapi baris = **item**
(Item·Qty·Satuan·Harga·Disc·Pajak·Total), bukan kontra-akun + Total tunggal.
Keputusan user: pilot **Sales Order**, **full backend incl. posting**, dan **semua
16 transaksi sales terdaftar** di Form Builder + Kustomisasi Grid.

**Grid engine digeneralkan (bukan fork).** Mesin grid spreadsheet (navigasi cell,
edit-state, render kolom config-driven) diekstrak jadi **generik model-agnostik**:
- [`grid-line-core.ts`](components/organisms/grid-line-core.ts) — `GridCol`/`GridDataType`/
  `GridRowBase` + interface adapter **`GridModel<Row>`** (`newRow`/`getCellRaw`/
  `buildCellPatch`) + helper generik (`isLookupCol`/`isCellFilled`/`rowRequiredMissing`/
  `linesRequiredMissing`).
- [`use-grid-nav.ts`](components/organisms/use-grid-nav.ts) — `useGridNav<Row>` generik
  (logika dipindah dari `use-cash-grid-nav`). `LineCell` ([cash-bank-line-cell.tsx](components/organisms/cash-bank-line-cell.tsx))
  sudah row-agnostic → dipakai apa adanya (semua editor: ROWNUM/LOOKUP/NUMBER/
  DISCOUNT/STEPPER/DATE/TEXTAREA/CHECKBOX/…).
- Cash/bank **tidak berubah perilaku**: `cash-bank-line-model.ts` kini mendefinisikan
  `cashBankGridModel` + delegasi helper ke core; `use-cash-grid-nav.ts` = wrapper tipis
  (`useGridNav` + `cashBankGridModel`). `cash-bank-lines.tsx` tak disentuh. **Jangan
  fork mesin grid** — bikin `GridModel<Row>` baru + organism konsumen.
- Sales: [`sls-item-line-model.ts`](components/organisms/sls-item-line-model.ts)
  (`SlsItemLineRow` + `slsItemGridModel`; `lineTotal` = derived qty×harga−disc, read-only/skip)
  + organism [`sls-item-lines.tsx`](components/organisms/sls-item-lines.tsx).

**Backend = modul baru `erp-sls-orders`** (`/erp/sls/orders`, `ErpJwtAuthGuard`),
mirror cash/bank: create/list/get/update/remove + `transition` (state machine
DRAFT→NEED_APPROVE→APPROVED→POSTED + REJECT/REOPEN), `docNumber` auto via
`sys_document_numberings` code **`SO`** (tabel `sls_orders` punya **dua** kolom unik
NOT NULL `code` + `doc_number` → di-set sama), `fiscalPeriodId` diturunkan dari
`docDate`, `subtotal`/`grandTotal` dihitung server-side (`lineNet = qty×harga −
disc`; `grandTotal = subtotal + Σpajak baris + pajak/biaya header − disc header`).
Enrich cross-domain (customer/branch/location/warehouse/currency/paymentTerm/
salesDept + per-baris item/unit/tax/warehouse) = scalar FK tanpa `@relation`,
di-resolve code+name server-side.

**SO TIDAK posting GL (penting).** Sales Order = dokumen komitmen, bukan peristiwa
finansial → `SlsOrderPostingService.postToLedger` = **no-op terdokumentasi** (POST
hanya cek periode tak CLOSED + set POSTED/postedAt; **0 baris `fin_ledger_entries`**).
Signature paralel `CashBankPostingService` agar **Sales Invoice (SI)** nanti isi
posting AR/revenue sungguhan. **E2E terverifikasi (2026-06-02):** create (subtotal=
grandTotal 202.500 dari 2 baris) → submit → approve → post (POSTED, 0 ledger) → list.

**Frontend (adopter §2.3.1).** API client [`lib/api/sls-orders.ts`](lib/api/sls-orders.ts);
form shared [`sales-transaction-form.tsx`](components/pages/sales-transaction-form.tsx)
(header 100% dari Form Builder via `useFormFields('SLS.SO')`, dispatch struktural
→ [`sls-structural-field.tsx`](components/molecules/sls-structural-field.tsx), custom →
reuse `CashBankCustomField`; Detail = `sls-item-lines`; fallback `DEFAULT_SLS_FORM_FIELDS`)
+ wrapper tipis [`sls-order-form.tsx`](components/pages/sls-order-form.tsx) +
model [`sls-order-form-model.ts`](components/pages/sls-order-form-model.ts). List/router
[`sls-orders-page.tsx`](components/pages/sls-orders-page.tsx) (reuse `cashBankWorkflowActions`
= mesin §2.7 yang sama) + filter slim [`sls-orders-filters.tsx`](components/pages/sls-orders-filters.tsx).
Daftar `/sales/orders` di `TRX_FORM_PAGES` (shell-route-renderer) + `ERP_ROUTE_META`.

**Lookup source baru (registry).** [`lookup-source-registry.ts`](lib/lookup-source-registry.ts)
ditambah `items`/`units`/`taxes`/`payment-terms` (selain 10 lama) → dipakai grid item
(Item/Satuan/Pajak) & header (Termin). `items` dikecualikan dari eager label-fetch di
`LineCell` (katalog besar — pakai label tersimpan/enriched, sama spt `accounts`).

**Katalog + config 16 transaksi sales (`seed-erp-transaction-grids.ts` +
`seed-erp-sales-forms.ts`).** Famili grid baru **`salesItem`** (kolom item-based default,
`lineTable` per-txn karena tiap dokumen sales punya tabel baris sendiri). 16 type
`SLS.*` di `sys_transaction_types` (kode lama `SLS.INV`/`SLS.RET` **di-prune** →
diganti `SLS.SI`/`SLS.SR`). 9 dokumen item-based (SQ/SO/PI/PL/DO/DR/SI/RNR/SR) dapat
grid + 12 field header (`sys_form_fields`); 7 dokumen pembayaran/alokasi (AS/IP/RP/IC/
PV/SIE/BB) = katalog saja (belum item-based). **Form kerja penuh baru SLS.SO.**

**Kontrak dataField/fieldKey (jangan rename).** Grid SLS.SO: `rowNo`,`itemId`,`quantity`,
`unitId`,`unitPrice`,`discountPercent`,`discountAmount`(hidden),`tax1Id`,`lineTotal`
(skip/derived),`warehouseId`(hidden),`notes`,`costCenterId`/`divisionId`/`subdivisionId`/
`projectId`(hidden). Form SLS.SO: `customerId`/`description`/`referenceNo` (LEFT) ·
`branchId`/`locationId`/`warehouseId`/`salesDeptId` (CENTER) · `docDate`(@today)/`docNumber`/
`currencyId`(default 1)/`paymentTermId`/`dueDate` (RIGHT).

**Follow-up:**
- Kolom **custom** baris sales belum persist (`sls_*_lines` tak punya `custom_fields`
  JSONB; cash/bank punya). Tambah kolom JSONB additif bila perlu custom line column.
- SI/DO/dst belum punya backend/form (hanya katalog + grid/form config). SI = isi
  posting AR/revenue di `SlsOrderPostingService` pola.
- **REOPEN dari POSTED → 400** (mesin `NEXT` tak punya transisi POSTED→REOPEN; **sama
  persis** dgn cash/bank — gap warisan, bukan regresi). UI kebab menawarkan "Reopen"
  di POSTED tapi backend menolak. Perbaiki serempak dgn cash/bank di pass terpisah.

---

## M3 Warehouse & Inventory — 10 Transaksi (2026-06-03)

Semua 10 transaksi M3 dibangun end-to-end: config layer (Form Builder + Kustomisasi Grid)
untuk semua 10, backend + frontend untuk 7 transaksi utama; PA/RW/DC punya bentuk
khusus (diuraikan di bawah).

### Pola umum (MR/TS/RS/RF/SA/IB/SP)

- **Backend**: satu module per tabel (`erp-inv-stock-movements` shared untuk 4 dokumen
  via `movementType` discriminator; module terpisah untuk SA/IB/SP). Semua: guard
  `ErpJwtAuthGuard`, state machine §2.7, penomoran via `sys_document_numberings`,
  fiscal period dari tanggal transaksi, posting NO-OP seam (stock balance = derived
  view `inv_stock_balances` dari status POSTED), enrich FK batched tanpa @relation.
- **Frontend**: 1 page per kode transaksi (thin wrapper di atas shared core atau
  standalone); form header 100% dari `useFormFields(transactionCode)`; grid detail
  dari `getGridColumns(transactionCode)`; generic grid engine reuse
  (`grid-line-core`/`use-grid-nav`/`LineCell`). URL sub-route `<base>/new` + `<base>/:id`
  via `TRX_FORM_PAGES` (§2.3.1).

### Keputusan desain per transaksi

**MR/TS/RS/RF** → `inv_stock_movements` shared, `movementType` enum discriminator.
Penomoran per-kode (MR/TS/RS/RF). Source/dest warehouse per-baris (TS/RS) atau
header-only (MR). RF = fuel refill, kolom Harga/Liter tampil default.

**SA (Stock Adjustment)** → `inv_stock_adjustments`. Arah INCREASE/DECREASE per-baris.
**Akun GL server-side:** `inventoryAccountId = line ?? item.inventoryAccountId ??
Setting(inventory.accounts.defaultInventoryAccountId)` — error eksplisit bila tidak
diset. `contraAccountId = line ?? Setting(inventory.accounts.defaultAdjustmentContraAccountId)`.
Frontend grid TIDAK tampilkan kolom akun (diturunkan otomatis).

**IB (Opening Stock)** → `inv_opening_stocks`. Header wajib `currencyId`+`exchangeRate`.
Akun persediaan server-side sama dengan SA. Header warehouse = default untuk baris;
baris boleh override per baris.

**SP (Stock Count)** → `inv_stock_counts`. Qty sistem/fisik/baik/rusak + varianceQty
= fisik − sistem (dihitung server-side). Tanpa akun GL (penyesuaian terpisah via SA).
Model TIDAK punya `postedAt` — POST hanya flip `status`+`postingStatus`.

**PA (Price Adjustment)** → `inv_cost_recalculations`. Bukan dokumen hand-keyed;
ini trigger proses recalc. Create = scope (item?/gudang?, dateRange, costingMethod)
→ status PENDING; kalkulasi async out-of-scope. Tanpa workflow approval.
Frontend: form header-only, create-only (tanpa edit), status badge job.

**RW (Receipt Weigher)** → `inv_weighbridge_tickets`. Header-only (tanpa grid baris).
`netWeight = grossWeight − tareWeight` dihitung server-side, tampil read-only di form.
Workflow §2.7 penuh.

**DC (Daily Check / Time Sheet)** → `inv_daily_checks` + `inv_daily_check_lines` (**tabel
BARU**, migrasi `20260603_003`). Header: branchId/checkDate/machineRef/operatorRef.
Baris: item+qty+unit+gudang. Workflow §2.7 penuh.

### Kode transaksi ↔ route ↔ backend ↔ tabel

| Kode    | Route FE                         | Backend endpoint                | Tabel header                  |
|---------|----------------------------------|---------------------------------|-------------------------------|
| INV.MR  | /warehouse/material-requests     | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.TS  | /warehouse/transfers             | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.RS  | /warehouse/transfer-receipts     | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.RF  | /warehouse/fuel-refills          | /erp/inv/stock-movements        | inv_stock_movements           |
| INV.SA  | /warehouse/stock-adjustments     | /erp/inv/stock-adjustments      | inv_stock_adjustments         |
| INV.IB  | /warehouse/opening-stocks        | /erp/inv/opening-stocks         | inv_opening_stocks            |
| INV.SP  | /warehouse/stock-counts          | /erp/inv/stock-counts           | inv_stock_counts              |
| INV.PA  | /warehouse/price-adjustments     | /erp/inv/price-adjustments      | inv_cost_recalculations       |
| INV.RW  | /warehouse/receipt-weighers      | /erp/inv/weighbridge-tickets    | inv_weighbridge_tickets       |
| INV.DC  | /warehouse/daily-checks          | /erp/inv/daily-checks           | inv_daily_checks              |

### Deploy & verifikasi (2026-06-03)
- Migrasi `20260603_003_erp_inv_daily_checks` → `prisma migrate deploy` (Postgres :3208). 79 migrasi sinkron.
- **Fix gap grid-custom DC:** INV.DC awalnya terdaftar **tanpa** `grid:` family di
  `seed-erp-transaction-grids.ts` → `sys_transaction_grids` 0 baris, sehingga DC selalu
  jatuh ke `defaultInvDailyCheckCols()` dan **tak bisa** dikustomisasi via Kustomisasi Grid
  (beda dengan 8 transaksi lain). Ditambah famili `invDailyCheck` →
  `inv_daily_check_lines` + `INV_DAILY_CHECK_COLUMNS` (mirror default cols). Re-seed →
  INV.DC kini 1 grid + 11 kolom (parity). RW tetap 0/0 (header-only, benar).
- Container `sentient-infra-api-gateway` (`nest --watch`) tidak otomatis recompile modul
  PA/RW/DC yang di-commit belakangan (route 404). Prosedur §2.34: `prisma generate` di
  dalam container (DC model baru) → `docker restart`. Semua 7 route inv kini 401
  (terdaftar + guarded): stock-movements, stock-adjustments, opening-stocks, stock-counts,
  price-adjustments, weighbridge-tickets, daily-checks.

### Follow-up — RESOLVED (2026-06-03)

Keempat follow-up M3 dikerjakan atas permintaan user ("ke-4 follow-up sekarang"):

1. **DC field line mesin/jam** ✅ — `inv_daily_check_lines` dapat kolom `machineRef`
   (TEXT) + `workHours` Decimal(19,4). Migrasi additif `20260603_004`. Distinct dari
   `machineRef`/`operatorRef` yang sudah ada di HEADER. DTO/mapper/FE row model/default
   cols/grid seed sinkron. Commit `c457e073`.

2. **Kolom akun GL override (SA/IB)** ✅ — kolom `inventoryAccountId` (SA: + `contraAccountId`)
   di-set `visible` di Kustomisasi Grid. FE row model bawa field typed (masuk
   `STANDARD_FIELDS`+`LABEL_KEYS`) + serializer kirim/hydrate. Backend sudah resolve
   line→item→Setting; sebelumnya nilai jatuh ke `customFields` & di-drop. Live DB:
   3 kolom di-flip `is_visible=true` via SQL (seed `update:{}` tak meng-update baris
   existing — baseline seed sudah benar untuk env baru). Commit `c457e073`.

3. **PA processor** ✅ — `process()` kini **generate baris server-side** dari moving-average
   (PA form header-only/trigger, tak ada line grid). Hitung `oldUnitCost` (item.averageCost
   ?? standardCost) vs `newUnitCost` (moving-avg) × `affectedQty` (qty on-hand) →
   `deltaAmount` + `totalDelta`, update `item.averageCost`, status `COMPLETED`/`FAILED`.
   Endpoint `POST /erp/inv/price-adjustments/:id/process` + aksi "Proses" (status
   PENDING/FAILED). **Catatan:** `inv_cost_recalculation_lines.warehouseId` NOT NULL →
   PA wajib `warehouseId` di header (PA company-wide → BadRequest jelas). Commit `595b555f`.

4. **GL valuation persediaan** ✅ (gated) — modul `erp-inv-gl`: `InvMovingAverageCostService`
   (moving-average on-the-fly dari POSTED stock-movement lines UNION opening-stock lines,
   sign per `movementType`; REQUEST/TRANSFER internal di-skip; `averageCost` field lama
   tak dipakai/unmaintained) + helpers (`buildLedgerRows` assert debit==kredit,
   `reverseInvLedger`) posting ke **`fin_ledger_entries`** (target nyata, mirror
   `CashBankPostingService` — BUKAN `fin_journal_entries`). Wire ke 3 seam NO-OP:
   - **SA**: Dr/Cr per arah `INCREASE`/`DECREASE` dari akun line.
   - **IB**: N Dr persediaan + 1 Cr ekuitas pembukaan (Setting `defaultOpeningEquityAccountId`).
   - **Movement valued**: `ISSUE` Dr COGS/Cr persediaan; `RETURN` kebalikan; TRANSFER/
     TRANSFER_RECEIPT/REQUEST tanpa GL. Cost = line.unitCost ?? moving-avg ?? item cost.
   - `reverseX` simetris (REOPEN/re-post unwind), assert balance tiap jurnal.

   **AMAN BY DEFAULT:** seluruh posting **gated** di belakang Setting
   `inventory/accounts/glPostingEnabled` (default **false** → seam tetap NO-OP persis
   perilaku lama; status POST tetap flip, 0 baris ledger). Aktifkan via:
   ```sql
   INSERT INTO sys_settings(module,"group",key,value) VALUES
     ('inventory','accounts','glPostingEnabled','true'),
     ('inventory','accounts','defaultOpeningEquityAccountId','<id>'),
     ('inventory','accounts','defaultCogsAccountId','<id>'),
     ('inventory','accounts','defaultInventoryAccountId','<id>');
   ```
   Saat enabled tapi akun kurang → `BadRequestException` jelas (tidak silent).

### Follow-up (sisa, butuh keputusan/di luar inventory)
- **Sales COGS (DO/SI)**: posting COGS sisi penjualan dimiliki modul `erp-sls-*` yang
  sedang dibangun sesi lain (seam `SlsDeliveryReportPostingService` NO-OP; modul SI/DO
  masih stub). Ditunda agar tak bentrok; pola sama (`buildLedgerRows` reusable).
- **IB exchangeRate** di posting GL: header `currencyId`/`exchangeRate` IB sudah dipakai;
  SA/Movement tak punya kolom currency → default `currencyId=1`, `exchangeRate=1`.
- **Snapshot `item.averageCost`**: kini diupdate hanya oleh PA process; movement POST belum
  menyetel ulang average (moving-avg dihitung on-the-fly). Bila perlu snapshot konsisten,
  tambah update saat movement POST.

---

## § Purchasing (M4) — config baseline + forms

**Pola = persis Sales (M5).** Purchasing dibangun meniru Sales Order item-based
(header config-driven Form Builder + grid baris config-driven Kustomisasi Grid +
state machine §2.7 + sub-route URL §2.3.1). Tiap dokumen `pur_*` punya line table
sendiri (per-txn `lineTable` override, bukan satu tabel bersama).

**13 tipe transaksi PUR** (paritas menu M4.TX) terdaftar di `sys_transaction_types`
(`seed-erp-transaction-grids.ts`). Kode stub lama (`PUR.GR`/`PUR.INV`/`PUR.RET`)
**di-prune** → diganti kode kanonik per menu:

| Kode | Dokumen | Grid family | Line table |
| --- | --- | --- | --- |
| `PUR.PR` | Purchase Requisition | `purchaseItem` | `pur_requisition_lines` |
| `PUR.RFQ` | Request for Quotation | `purchaseRfq` | `pur_rfq_suppliers` (baris = supplier diundang) |
| `PUR.BS` | Bid Comparison | `purchaseBid` | `pur_bid_selection_lines` |
| `PUR.PO` | Purchase Order | `purchaseItem` | `pur_order_lines` |
| `PUR.GRN` | Goods Receipt | `purchaseReceipt` | `pur_goods_receipt_lines` (+ QC: accepted/rejected/quarantine) |
| `PUR.PI` | Purchase Invoice | `purchaseItem` | `pur_invoice_lines` |
| `PUR.DNR` | Return Shipment | `purchaseItem` | `pur_return_lines` (returnType=DEBIT_NOTE) |
| `PUR.PRT` | Purchase Return | `purchaseItem` | `pur_return_lines` (returnType=RETURN_TO_VENDOR) |
| `PUR.AP` · `PUR.PP` · `PUR.VPP` · `PUR.VP` · `PUR.OB` | Vendor Advance / Freight Payable / Payment Schedule / Vendor Payment / Opening AP | — (reuse finance domain) | — (katalog + header saja) |

8 dokumen item-based dapat grid kolom default; 5 dokumen pembayaran/saldo-awal
(`AP/PP/VPP/VP/OB` reuse `fin_ap_payments`/`fin_settlement_allocations`) = katalog +
header form saja (grid menyusul saat desain reuse finance difinalisasi). Header field
default per kode di `seed-erp-purchasing-forms.ts` (`seedPurchasingForms`): supplier
**required** untuk PO/GRN/PI/DNR/PRT, **optional** untuk PR/RFQ/BS (pre-sourcing).

**Kontrak dataField/fieldKey (jangan rename).** Grid `purchaseItem`: `rowNo`,`itemId`,
`quantity`,`unitId`,`unitPrice`,`discountPercent`,`discountAmount`(hidden),`tax1Id`,
`lineTotal`(skip/derived),`warehouseId`(hidden),`notes`,dims(hidden). Grid
`purchaseReceipt` tambah `acceptedQty`/`rejectedQty`(hidden)/`quarantineQty`(hidden)/
`unitCost`(hidden). Header item-doc: `supplierId`/`description`/`referenceNo` (LEFT) ·
`branchId`/`locationId`/`warehouseId`/`payableAccountId` (CENTER) · `docDate`(@today)/
`docNumber`/`currencyId`(default 1)/`paymentTermId`/`dueDate` (RIGHT).

**Lookup registry** sudah punya `items`/`units`/`taxes`/`payment-terms` (ditambah saat
Sales). Reuse — jangan bikin slug baru.

**Form kerja penuh pertama = Purchase Order** (`/purchasing/purchase-orders`, code
`PUR.PO`) — mirror SO 1:1. Backend `erp-pur-orders` (CRUD + numbering `PO` + fiscal
period dari docDate + totals server-side + enrich cross-domain + workflow). **PO TIDAK
posting GL** (dokumen komitmen; GRN posting inventory+GR/IR, PI posting AP) — posting
service = no-op terdokumentasi. E2E verified: create→submit→approve→post (POSTED, 0
ledger entries; subtotal/grandTotal benar). FE: `lib/api/pur-orders`,
`purchase-transaction-form` (shared) + `pur-order-form`, `pur-item-lines`,
`pur-structural-field`, `pur-orders-page` + filters; route di `TRX_FORM_PAGES` +
`ERP_ROUTE_META`. **Beda dari SO:** `pur_orders` **tidak punya kolom `code`** (SO punya);
`customerId`→`supplierId`, `receivableAccountId`→`payableAccountId`, `salesDeptId` di-drop.
Reuse `cashBankWorkflowActions` + grid engine generik (sama spt SO).

Semua 13 transaksi Purchasing selesai. Replikasi selesai (PR/PI/GRN/DNR/PRT/RFQ/BS).
Payment docs (AP/PP/VPP/VP/OB): reuse pur_invoices / fin_ap_payments — lihat §§ payment docs di bawah.

**Follow-up (sama persis SO):** REOPEN dari POSTED → 400 (gap warisan); kolom custom
baris belum persist (`pur_*_lines` tak punya `custom_fields` JSONB).

**Payment docs (AP/PP/VPP/VP/OB)**: reuse Finance domain + `fin_ap_payments` +4 kolom
(migrasi `20260603_001`): `fx_gain_loss_amount/account_id` + `term_discount_amount/account_id`.
AP/PP/OB = list pur_invoices dari sisi pembelian; VPP/VP = fin_ap_payments DRAFT/ALL.
Form semua coming-soon — menunggu integrasi Finance AP payment form ke purchasing UI.
Routes: /purchasing/vendor-advances · /purchasing/freight-payables ·
/purchasing/payment-schedules · /purchasing/vendor-payments · /purchasing/opening-ap-balance.


---

## § Warehouse (M3) Reports — 23 laporan + export server-side (2026-06-03)

**Konteks:** menu REPORTS (`/warehouse/reports/*`, 23 path seeded) sebelumnya
render blank (ComingSoon — tak ada komponen terdaftar). Dibuat full: view +
export ke Excel/PDF/Word. Pilihan user: **semua report (A+B) sekaligus**,
**export server-side** (api-gateway).

**Pola = framework laporan uniform (1 kontrak).** Tiap report = `ReportDef`
(`{ key, title, group, columns, resolve(filters) }`) → `ReportDataset`
(`{ columns, rows, summary, total, generatedAt }`) yang **sama** dipakai view
JSON dan ketiga exporter, jadi tampilan & file selalu konsisten. Tambah report =
tambah satu `ReportDef`; tak ada plumbing per-report. Backend: `apps/api-gateway/
src/erp-inv-reports/` (registry `inv-reports.service.ts` compose `buildTxnReports`
+ `buildStockReports`). Endpoint (guard `ErpJwtAuthGuard`):
- `GET /erp/inv/reports` → katalog (key/title/group).
- `GET /erp/inv/reports/:key` → ReportDataset JSON (tabel layar).
- `GET /erp/inv/reports/:key/export?format=xlsx|pdf|docx&<filters>` → unduh file.
Filter query: dateFrom/dateTo/asOfDate/warehouseId/itemId/status/search/page/limit.

**23 report:**
- **11 transaksi** (`group:'transaction'`): MR/TS/RS/RF/Return (ErpInvStockMovement
  per `movementType`), SP/SA/PA/IB/DC/RW (header-level per modul). Filter status +
  tanggal + gudang + search.
- **4 item** (`group:'item'`): batch-items/batch-cards (ErpInvLot), serial-items/
  serial-cards (ErpInvSerial). (Lot tak punya qty on-hand per-lot → kolom qty 0;
  follow-up.)
- **8 agregasi stok** (`group:'stock'`, reuse `InvMovingAverageCostService` dari
  `erp-inv-gl` — saldo DERIVED dari POSTED movements ∪ opening, tak ada tabel
  stock-ledger): stock (saldo+nilai), stock-cards (kartu stok running balance,
  butuh itemId), stock-mutations (opening/in/out/closing), below-minimum
  (`md_items.minStock`), daily-stock (saldo harian), cogs-balance (cost recalc),
  stock-minus (saldo negatif), consignment (**kosong + note** — belum ada model
  konsinyasi; hanya `md_items.consignmentAccountId`).

**Export server-side** (`report-export.service.ts` + per-format): **exceljs**
(xlsx), **pdfkit** (pdf — font Helvetica built-in, tanpa Chromium; pdfmake
**tidak** dipakai, di-skip karena setup font 0.3.x ribet — pdfkit sudah jadi dep
& dipakai modul `erp-fin-reports`), **docx** (word). Format sel per `column.type`
(money/qty/number/percent/date/status) identik view & file. Filename
`${key}-YYYYMMDD.ext`.

**Frontend:** satu `InvReportPage` generik (`components/pages/inv-report-page.tsx`)
+ `ReportToolbar` (filter + tombol Excel/PDF/Word) + `ReportTable`, reuse
`ErpListLayout`/`Table`/format helpers + `lib/api/client.downloadFile` (cookie
`erp_token` + nama file dari `Content-Disposition`). Routing: `renderRoute`
dispatch `/warehouse/reports/:key` → `InvReportPage`; opsi per-key di
`lib/inv-report-options.ts` (status filter utk transaksi, item picker utk
stock-cards, as-of utk stock).

**Bug pre-eksis ditemukan & diperbaiki:** `inv_weighbridge_tickets.posted_at`
ada di schema (sejak build RW) tapi tak pernah dimigrasi → semua Prisma read RW
500 (P2022), termasuk list RW. Migrasi `20260603_005` menambah kolomnya.

**Verifikasi E2E:** login admin → katalog 23 report → 23/23 data http 200 →
export xlsx/pdf/docx magic bytes valid (PK/%PDF/PK). Typecheck api-gateway 0
error; file FE report clean.

**Follow-up:** (1) consignment butuh model transaksi konsinyasi. (2) batch/lot
on-hand qty per gudang (skema lot tak simpan qty). (3) Statistics group
(`/warehouse/stats/*`, 6 dashboard) masih ComingSoon — di luar scope ini.
(4) Finance reports (`/finance/*`: cash-flow, AR/AP card/aging, giro-maturity,
budget-realization) dibangun **paralel sesi lain** (modul `erp-fin-reports`,
pola berbeda) — jangan dobel.

---

