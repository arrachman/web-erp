# Inventory: Entity Catalog (m3 → `inv`)

> Legacy "Inventory" (m3) maps to the **`inv_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15): legacy MR/RF/TS/RS movement headers (near-identical) unify into one stock
> movement; `*_detail` normalize into line tables; `riwayat_*` → `sys_audit_logs`.

Field-level model. Types are Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
All entities carry the global **audit + soft-delete** columns from
[README §3](README.md#3-global-conventions) — omitted per-row. Transaction masters
also carry **`legacyCode String?`** (nullable, `@@index`; original MyERP+ doc number
for CDC/ETL backfill — resolved §8 #7) — omitted per-row.

Legend: 🔑 business key · ➜ FK · ◆ enum · ○ nullable. Qty/cost `Decimal(19,4)`;
rate `Decimal(19,6)`. Dimension FKs (`costCenterId`/`divisionId`/`subdivisionId`/
`projectId` → `md_*`, plus `branchId`/`locationId`) follow the m2 convention; period
FK = `fiscalPeriodId → sys_fiscal_periods` (no new period table).

> **Stock on hand is NOT stored.** `bstok`-style caches are dropped. On-hand per
> item/warehouse is **derived** from opening stock + posted movements/adjustments
> (`inv_stock_balances`, a view/materialized projection — not a written master).
> Legacy denormalized `namabarang`/`tipebarang`/`satuanbarang` echoes are dropped
> (resolved via the `md_items`/`md_units` relations).
>
> **Costing model (resolved §8 #18–19, #22–23).** Inventory adalah **perpetual**;
> valuation method = `CostingMethod` setting (`sys_settings` key
> `inventory.costing_method`, default `AVG` = moving-average; `FIFO`/`STD`
> selectable). Line `unitCost` adalah **frozen as-posted snapshot** — tidak pernah
> ditimpa. Saat dokumen POSTED diedit atau ada posting terlambat yang mengubah
> cost history, sistem otomatis buat `inv_cost_recalculations` `PENDING`. Akuntan
> trigger recost → run **langsung update `debit`/`credit` pada baris COGS di
> `fin_ledger_entries`** (tidak emit ADJUSTMENT journal baru — resolved §8 #23).
> Neraca + Laba Rugi otomatis fix. Audit trail: baris yang di-update di-stamp
> `recostedAt`/`recostedByRunId`; detail before→after di
> `inv_cost_recalculation_lines`.

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `StockMovementType` | `REQUEST`, `ISSUE`, `TRANSFER`, `TRANSFER_RECEIPT`, `RETURN` | `m3_mr`/`rf`/`ts`/`rs` |
| `StockCountType` | `FULL`, `CYCLE`, `SPOT` | `m3_sp` `spjenis`/`sajenis` |
| `AdjustmentDirection` | `INCREASE`, `DECREASE` | `m3_sa` `jmlmasuk`/`jmlkeluar` |
| `LotStatus` | `ACTIVE`, `QUARANTINE`, `EXPIRED`, `BLOCKED` | new — batch/expiry control (resolved §8 #24) |
| `SerialStatus` | `IN_STOCK`, `ISSUED`, `RETURNED`, `SCRAPPED` | new — serial lifecycle (resolved §8 #24) |
| `ReservationStatus` | `ACTIVE`, `FULFILLED`, `RELEASED`, `EXPIRED` | new — soft allocation / ATP (resolved §8 #25) |

Reused from m2: `DocumentStatus` (`DRAFT`/`POSTED`/`VOID`/`CANCELLED`),
`PostingStatus` (`UNPOSTED`/`POSTED`).

---

## Stock movements (`inv_*`)

### ErpInvStockMovement → `inv_stock_movements`  (legacy `m3_mr` + `m3_rf` + `m3_ts` + `m3_rs`)

Unifies the near-identical material-request / transfer / receipt headers. The
legacy chain MR → TS → RS (request → transfer → receipt) is preserved via
`relatedMovementId` self-reference (so a TS points to its MR, an RS to its TS).

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `*notransaksi` |
| autoNumber ○ | String | `*autonotransaksi` (via `sys_document_numberings`) |
| movementType ◆ | `StockMovementType` | REQUEST(`mr`)/RETURN(`rf`)/TRANSFER(`ts`)/TRANSFER_RECEIPT(`rs`) |
| branchId ➜ | BigInt → Branch | `*cabang` |
| locationId ○ ➜ | BigInt → Location | `*lokasi` |
| sourceWarehouseId ○ ➜ | BigInt → Warehouse | `*gudangasal` |
| transitWarehouseId ○ ➜ | BigInt → Warehouse | `*gudangtransit` (TS/RS) |
| destinationWarehouseId ○ ➜ | BigInt → Warehouse | `*gudangtujuan` |
| source ○ | String | `*sumber` (originating module) |
| movementDate | Date | `*tgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `*kodepa` |
| requestedById ○ ➜ | BigInt → ErpUser | `*dimintaoleh` / `*bagian*` |
| requestedPartnerId ○ ➜ | BigInt → Partner | `*dimintaolehkontak` / `*kontak` |
| requestedTo ○ | String | `*mintake` |
| neededDate ○ | Date | `*tgldipakai` |
| relatedMovementId ○ ➜ | BigInt → ErpInvStockMovement | chain link (`*idmr`/`*idts`) |
| description ○ | String | `*uraian` |
| notes ○ | String | `*catatan` |
| referenceNo ○ | String | `*noref` |
| referenceDate ○ | Date | `*tglnoref` |
| status ◆ | `DocumentStatus` | `*status` |
| previousStatus ○ ◆ | `DocumentStatus` | `*statussebelumnya` |
| revisionCount | Int | `*jmlrevisi` |
| printCount | Int | `*cetakanke` |
| postingStatus ◆ | `PostingStatus` | `*posting` |
| postedAt ○ | DateTime | `*postingtgl` |

Relations: `lines ErpInvStockMovementLine[]`, warehouses, `relatedMovement`
(self), `branch`, `fiscalPeriod`. Indexes: `@@index([movementType, status])`,
`@@index([fiscalPeriodId])`, `@@index([relatedMovementId])`.

### ErpInvStockMovementLine → `inv_stock_movement_lines`  (legacy `m3_*_detail`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| stockMovementId ➜ | BigInt → ErpInvStockMovement | `idmr`/`idrf`/`idts`/`idrs` |
| itemId ➜ | BigInt → Item | `idbarang` (`namabarang`/`tipebarang` echoes dropped) |
| quantity | Decimal(19,4) | `jml` (in `unitId`) |
| unitId ➜ | BigInt → Unit | `satuan` |
| unitValue | Decimal(19,4) | `nilaisatuan` (conversion to base) |
| baseQuantity | Decimal(19,4) | `jmlbarang` (base-unit qty) |
| baseUnitId ➜ | BigInt → Unit | `satuanbarang` |
| currencyId ○ ➜ | BigInt → Currency | `matauang` |
| exchangeRate ○ | Decimal(19,6) | `kurs` |
| unitCost ○ | Decimal(19,4) | `hargabeli`/`hpp` (valuation snapshot) |
| salePrice ○ | Decimal(19,4) | `hargajual` (reference) |
| sourceWarehouseId ○ ➜ | BigInt → Warehouse | `gudangasal` (line override) |
| destinationWarehouseId ○ ➜ | BigInt → Warehouse | `gudangtujuan` |
| relatedLineId ○ ➜ | BigInt → ErpInvStockMovementLine | `idmrdetail`/`idtsdetail`/`idgrndetail` traceability |
| costCenterId ○ ➜ | BigInt → CostCenter | `costcenter` |
| divisionId ○ ➜ | BigInt → Division | `divisi` |
| subdivisionId ○ ➜ | BigInt → Subdivision | `subdivisi` |
| projectId ○ ➜ | BigInt → Project | `proyek` |
| notes ○ | String | `catatan` |
| lineNo | Int | `urutan` |

`@@index([stockMovementId])`, `@@index([itemId])`.

---

## Opening stock (`inv_*`)

### ErpInvOpeningStock → `inv_opening_stocks`  (legacy `m3_ib`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `ibnotransaksi` |
| autoNumber ○ | String | `ibautonotransaksi` |
| branchId ➜ | BigInt → Branch | `ibcabang` |
| locationId ○ ➜ | BigInt → Location | `iblokasi` |
| warehouseId ➜ | BigInt → Warehouse | `ibgudang` |
| kind ○ | String | `ibjenis` |
| openingDate | Date | `ibtgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `ibkodepa` |
| currencyId ➜ | BigInt → Currency | `ibmatauang` |
| exchangeRate | Decimal(19,6) | `ibkurs` |
| description ○ | String | `iburaian` |
| notes ○ | String | `ibcatatan` |
| status ◆ | `DocumentStatus` | `ibstatus` |
| postingStatus ◆ | `PostingStatus` | `ibposting` |
| postedAt ○ | DateTime | `ibpostingtgl` |

### ErpInvOpeningStockLine → `inv_opening_stock_lines`  (legacy `m3_ib_detail`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| openingStockId ➜ | BigInt → ErpInvOpeningStock | `idib` |
| itemId ➜ | BigInt → Item | `idbarang` |
| quantity | Decimal(19,4) | `jml`/`jmlbarang` |
| unitId ➜ | BigInt → Unit | `satuan` |
| baseUnitId ➜ | BigInt → Unit | `satuanbarang` |
| unitCost | Decimal(19,4) | `hpp` (`hpplama` is prior cost — not stored) |
| inventoryAccountId ➜ | BigInt → Account | `rekpersediaan` |
| warehouseId ➜ | BigInt → Warehouse | `gudang` |
| costCenterId ○ ➜ | BigInt → CostCenter | `costcenter` |
| divisionId ○ ➜ | BigInt → Division | `divisi` |
| subdivisionId ○ ➜ | BigInt → Subdivision | `subdivisi` |
| projectId ○ ➜ | BigInt → Project | `proyek` |
| notes ○ | String | `catatan` |
| lineNo | Int | `urutan` |

---

## Stock count & adjustment (`inv_*`)

### ErpInvStockCount → `inv_stock_counts`  (legacy `m3_sp`)

Physical count sheet (opname): system vs physical, good/damaged, variance.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `spnotransaksi` |
| autoNumber ○ | String | `spautonotransaksi` |
| branchId ➜ | BigInt → Branch | `spcabang` |
| warehouseId ➜ | BigInt → Warehouse | `spgudang` |
| countDate | Date | `sptgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `spkodepa` |
| stepNo ○ | Int | `spstepke` |
| description ○ | String | `spuraian` |
| notes ○ | String | `spcatatan` |
| adjustmentStatus ○ ◆ | `DocumentStatus` | `spstatussa` (linked SA state) |
| status ◆ | `DocumentStatus` | `spstatus` |
| postingStatus ◆ | `PostingStatus` | `spposting` |

### ErpInvStockCountLine → `inv_stock_count_lines`  (legacy `m3_sp_detail`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| stockCountId ➜ | BigInt → ErpInvStockCount | `idsp` |
| itemId ➜ | BigInt → Item | `idbarang` |
| systemQty | Decimal(19,4) | `jmlsistem`/`jmlbarangsistem` |
| physicalQty | Decimal(19,4) | `jmlfisik` |
| goodQty | Decimal(19,4) | `jmlbagus` |
| damagedQty | Decimal(19,4) | `jmlrusak` |
| varianceQty | Decimal(19,4) | `selisih` (physical − system) |
| unitId ➜ | BigInt → Unit | `satuan` |
| baseUnitId ➜ | BigInt → Unit | `satuanbarang` |
| warehouseId ➜ | BigInt → Warehouse | `gudang` |
| binLocation ○ | String | `lokasibarang` |
| dimensions ○ | (FKs) | `costcenter`/`divisi`/`subdivisi`/`proyek` (as m2) |
| notes ○ | String | `catatan` |
| lineNo | Int | `urutan` |

### ErpInvStockAdjustment → `inv_stock_adjustments`  (legacy `m3_sa`)

GL-affecting valuation/quantity adjustment, optionally derived from a count.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `sanotransaksi` |
| autoNumber ○ | String | `saautonotransaksi` |
| branchId ➜ | BigInt → Branch | `sacabang` |
| warehouseId ➜ | BigInt → Warehouse | `sagudang` |
| adjustmentDate | Date | `satgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `sakodepa` |
| kind ○ | String | `sajenis` |
| stockCountId ○ ➜ | BigInt → ErpInvStockCount | `saidsp` (source count) |
| description ○ | String | `sauraian` |
| notes ○ | String | `sacatatan` |
| status ◆ | `DocumentStatus` | `sastatus` |
| postingStatus ◆ | `PostingStatus` | `saposting` |
| postedAt ○ | DateTime | `sapostingtgl` |

### ErpInvStockAdjustmentLine → `inv_stock_adjustment_lines`  (legacy `m3_sa_detail`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| stockAdjustmentId ➜ | BigInt → ErpInvStockAdjustment | `idsa` |
| itemId ➜ | BigInt → Item | `idbarang` |
| direction ◆ | `AdjustmentDirection` | INCREASE(`jmlmasuk`)/DECREASE(`jmlkeluar`) |
| quantity | Decimal(19,4) | `jmlmasuk`/`jmlkeluar` (abs) |
| baseQuantity | Decimal(19,4) | `jmlbarangmasuk`/`jmlbarangkeluar` |
| unitId ➜ | BigInt → Unit | `satuan` |
| baseUnitId ➜ | BigInt → Unit | `satuanbarang` |
| unitCost | Decimal(19,4) | `hpp` (`hpplama` not stored) |
| inventoryAccountId ➜ | BigInt → Account | `rekpersediaan` |
| contraAccountId ➜ | BigInt → Account | `reklawan` (GL contra) |
| warehouseId ➜ | BigInt → Warehouse | `gudang` |
| countLineId ○ ➜ | BigInt → ErpInvStockCountLine | `idspdetail` |
| dimensions ○ | (FKs) | `costcenter`/`divisi`/`subdivisi`/`proyek` |
| notes ○ | String | `catatan` |
| lineNo | Int | `urutan` |

Posting an adjustment writes `fin_ledger_entries` (inventory vs contra account).

---

## Weighbridge (`inv_*`)

### ErpInvWeighbridgeTicket → `inv_weighbridge_tickets`  (legacy `m3_rw`)

Gross/tare/net weighing ticket (commodity/bulk inventory).

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `rwnotransaksi` |
| branchId ➜ | BigInt → Branch | `rwcabang` |
| locationId ○ ➜ | BigInt → Location | `rwlokasi` |
| ticketDate | Date | `rwtgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `rwkodepa` |
| vehiclePlate ○ | String | `rwnopol` |
| driverName ○ | String | `rwsopir` |
| partnerId ○ ➜ | BigInt → Partner | `rwkid` (counterparty) |
| itemId ○ ➜ | BigInt → Item | `rwbid` |
| grossAt ○ | DateTime | `rwtglbruto` |
| grossWeight | Decimal(19,4) | `rwbruto` |
| tareAt ○ | DateTime | `rwtgltara` |
| tareWeight | Decimal(19,4) | `rwtara` |
| netWeight | Decimal(19,4) | `rwneto` (gross − tare) |
| unitPrice ○ | Decimal(19,4) | `rwharga` |
| description ○ | String | `rwuraian` |
| notes ○ | String | `rwcatatan` |
| status ◆ | `DocumentStatus` | `rwstatus` |
| postingStatus ◆ | `PostingStatus` | `rwposting` |

---

## HPP recalculation (`inv_*`)

### ErpInvCostRecalculation → `inv_cost_recalculations`  (modern — no legacy table)

A **recost run**: records when/why the cost layer was recomputed and which
`fin_ledger_entries` rows were updated. Triggered when (a) dokumen POSTED
diedit (`EDITED_DOC`), (b) ada posting terlambat/backdated, atau (c) manual
oleh akuntan. Recost **langsung update `debit`/`credit` pada baris COGS
di `fin_ledger_entries`** — tidak emit jurnal ADJUSTMENT baru (resolved §8 #23).
Neraca + Laba Rugi otomatis fix setelah run `COMPLETED`.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | run reference (via `sys_document_numberings`) |
| costingMethod ◆ | `CostingMethod` | method applied (snapshot of the setting at run time) |
| triggerType | String | `EDITED_DOC` / `BACKDATED_POST` / `MANUAL` / `ADJUSTMENT` / `RETURN` |
| triggerSourceDocType ○ | String | originating doc table key |
| triggerSourceId ○ | BigInt | originating doc row id |
| itemId ○ ➜ | BigInt → Item | scope: single item (null = all in scope) |
| warehouseId ○ ➜ | BigInt → Warehouse | scope: single warehouse (null = all) |
| fromDate | Date | recompute window start (earliest affected movement) |
| toDate ○ | Date | window end (null = up to latest) |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | periode yang di-recost (harus `OPEN`/`SOFT_CLOSED`/`REOPENED`) |
| status ◆ | `CostRecalcStatus` | `PENDING`/`COMPLETED`/`FAILED` |
| totalDelta ○ | Decimal(19,4) | total net COGS delta (sum of all line deltas) — informasi saja |
| startedAt ○ | DateTime | |
| completedAt ○ | DateTime | |
| notes ○ | String | failure reason / operator note |

Relations: `lines ErpInvCostRecalculationLine[]`, `item`, `warehouse`,
`fiscalPeriod`. Indexes:
`@@index([status])`, `@@index([itemId, warehouseId])`,
`@@index([triggerSourceDocType, triggerSourceId])`.

### ErpInvCostRecalculationLine → `inv_cost_recalculation_lines`

Per item/warehouse before→after cost + per ledger entry yang di-update,
untuk audit trail dan report drill-down.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| costRecalculationId ➜ | BigInt → ErpInvCostRecalculation | |
| itemId ➜ | BigInt → Item | |
| warehouseId ➜ | BigInt → Warehouse | |
| ledgerEntryId ○ ➜ | BigInt → ErpFinLedgerEntry | baris `fin_ledger_entries` yang di-update (null = item-level summary row) |
| oldUnitCost | Decimal(19,4) | moving-average before recost |
| newUnitCost | Decimal(19,4) | recomputed moving-average |
| affectedQty | Decimal(19,4) | qty re-valued |
| oldDebit ○ | Decimal(19,4) | nilai `debit` sebelum recost (snapshot) |
| oldCredit ○ | Decimal(19,4) | nilai `credit` sebelum recost (snapshot) |
| newDebit ○ | Decimal(19,4) | nilai `debit` setelah recost |
| newCredit ○ | Decimal(19,4) | nilai `credit` setelah recost |
| deltaAmount | Decimal(19,4) | `(new − old) × affectedQty` |
| lineNo | Int | |

`@@index([costRecalculationId])`, `@@index([itemId])`,
`@@index([ledgerEntryId])`.

---

## Derived (not a written table)

### inv_stock_balances *(view / materialized projection)*

On-hand qty + moving-average cost per `(itemId, warehouseId, binId, lotId)`
(bin/lot NULL for non-tracked items) computed from `inv_opening_stocks` +
posted `inv_stock_movements` + `inv_stock_adjustments`. Available-to-promise =
this qty − Σ `ACTIVE` `inv_stock_reservations`.
Modeled as a read projection — **never written directly** (replaces legacy
`bstok`/`m1_item_stock_warehouse` caches). Recomputation is *the source of truth*
for cost; `inv_cost_recalculations` records the GL-side reconciliation of that
recompute.

---

## Lot / Serial / Bin / Reservation — enterprise traceability (`inv_*`)

> Added by **resolved §8 #24–26 (2026-05-18)**. These promote `inv` from an
> accounting-inventory to an enterprise WMS-capable model: batch/expiry,
> per-unit serials, structured bins, and available-to-promise reservation.
> **Stock granularity changes:** `inv_stock_balances` is now keyed by
> `(itemId, warehouseId, binId, lotId)` (bin/lot nullable for non-tracked
> items). Serial on-hand is derived from `inv_serials.status = IN_STOCK`.

### ErpInvBin → `inv_bins`  (modern — no legacy table; was string `lokasibarang`)

Structured sub-location inside a warehouse (rack/shelf/slot) for putaway &
picking. Replaces the free-text `binLocation` string. Per-item tracking flags
live on `md_items` (`tracksBatch`/`tracksSerial`/`tracksBin`, see
[entities-m1-master-data.md](entities-m1-master-data.md)); non-tracked items keep
`binId = null`.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | bin code (unique per warehouse) |
| name ○ | String | display label |
| warehouseId ➜ | BigInt → Warehouse | owning warehouse |
| binType ○ | String | `STORAGE` / `PICKING` / `STAGING` / `QUARANTINE` |
| sequence ○ | Int | pick-path ordering |
| isActive | Boolean | business toggle (default true) |

Relations: `warehouse`, `movementLines`, `balances`. `@@unique([warehouseId, code])`,
`@@index([warehouseId])`.

### ErpInvLot → `inv_lots`  (modern — no legacy table)

Batch/lot master per item — manufacturing/expiry/recall traceability.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| lotNumber 🔑 | String | batch number (unique per item) |
| itemId ➜ | BigInt → Item | tracked item |
| supplierLotNo ○ | String | vendor batch ref |
| manufactureDate ○ | Date | |
| expiryDate ○ | Date | drives FEFO + expiry alerts |
| originGoodsReceiptId ○ ➜ | BigInt | source `pur_goods_receipts` (lineage) |
| status ◆ | `LotStatus` | `ACTIVE`/`QUARANTINE`/`EXPIRED`/`BLOCKED` |
| notes ○ | String | |

Relations: `item`, `movementLines`, `balances`. `@@unique([itemId, lotNumber])`,
`@@index([itemId, status])`, `@@index([expiryDate])`.

### ErpInvSerial → `inv_serials`  (modern — no legacy table)

Per-unit serial record; one row per physical unit, current location & state.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| serialNumber 🔑 | String | unique per item |
| itemId ➜ | BigInt → Item | tracked item |
| lotId ○ ➜ | BigInt → ErpInvLot | optional lot membership |
| currentWarehouseId ○ ➜ | BigInt → Warehouse | null once `ISSUED`/`SCRAPPED` |
| currentBinId ○ ➜ | BigInt → ErpInvBin | current bin |
| status ◆ | `SerialStatus` | `IN_STOCK`/`ISSUED`/`RETURNED`/`SCRAPPED` |
| originGoodsReceiptId ○ ➜ | BigInt | source `pur_goods_receipts` |
| lastMovementId ○ ➜ | BigInt → ErpInvStockMovement | last movement touching this unit |
| warrantyUntil ○ | Date | |
| notes ○ | String | |

Relations: `item`, `lot`, `currentWarehouse`, `currentBin`.
`@@unique([itemId, serialNumber])`, `@@index([itemId, status])`,
`@@index([currentWarehouseId])`.

### ErpInvStockReservation → `inv_stock_reservations`  (modern — no legacy table)

Soft allocation of on-hand stock against a demand doc (SO line / WO material
requirement). **Available-to-promise** = `inv_stock_balances.qty` − Σ `ACTIVE`
reservation qty. Reservation does **not** move stock or post GL; fulfilment
(delivery / material issue) consumes it and flips it `FULFILLED`.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| itemId ➜ | BigInt → Item | reserved item |
| warehouseId ➜ | BigInt → Warehouse | reserved from |
| binId ○ ➜ | BigInt → ErpInvBin | optional bin pin |
| lotId ○ ➜ | BigInt → ErpInvLot | optional lot pin |
| quantity | Decimal(19,4) | reserved qty (base unit) |
| sourceDocType | String | `SLS_ORDER` / `MFG_WORK_ORDER` / `MANUAL` |
| sourceDocId | BigInt | demand doc row id |
| sourceLineId ○ | BigInt | demand doc line id |
| status ◆ | `ReservationStatus` | `ACTIVE`/`FULFILLED`/`RELEASED`/`EXPIRED` |
| expiresAt ○ | DateTime | auto-release window |
| fulfilledByMovementId ○ ➜ | BigInt → ErpInvStockMovement | the issue/delivery that consumed it |

Relations: `item`, `warehouse`, `bin`, `lot`, `fulfilledByMovement`.
`@@index([itemId, warehouseId, status])`,
`@@index([sourceDocType, sourceDocId])`.

### FK additions to existing line tables

`inv_stock_movement_lines`, `inv_stock_count_lines`,
`inv_stock_adjustment_lines`, and `inv_opening_stock_lines` each gain optional
**`binId ➜ ErpInvBin`**, **`lotId ➜ ErpInvLot`**, **`serialId ➜ ErpInvSerial`**
(nullable; populated only for items with `tracksBin`/`tracksBatch`/`tracksSerial`
on `md_items`). The
`relatedLineId` traceability already present is unchanged.

---

## Flagged / deferred (not in this catalog)

- **`m3_dc` (+ `dc_check`, `dc_detail`)** — carries hour-meter readings
  (`dchmstart/stop/total`), shift, and op/sb/sp/rf/bd counters. This is an
  **equipment-usage / daily-check log**, not a stock movement. **Flagged for
  study** with the m6 `mfg` / equipment scope — *not* force-fit into `inv`.
- **`m3_pa` (+ `pa_detail`)** — selling-price/discount revision with 10 price
  tiers + 10 discount tiers. Tiered pricing is **deferred** (resolved §8 #3); a
  lean `inv_price_adjustments` is **not** modeled now — it belongs to the future
  `ItemPrice`/pricing phase, kept out of the core to avoid pulling in tiers.

---

**Count:** 17 Inventory (`inv_*`) entities — StockMovement(+Line),
OpeningStock(+Line), StockCount(+Line), StockAdjustment(+Line),
WeighbridgeTicket, **CostRecalculation(+Line)**, **Bin**, **Lot**, **Serial**,
**StockReservation**, + derived `inv_stock_balances` (view, now keyed by
`(itemId, warehouseId, binId, lotId)`). Period reuses `sys_fiscal_periods`;
dimensions reuse the m2 `md_*` masters. `m3_dc`/`m3_pa` flagged out (see above).

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
