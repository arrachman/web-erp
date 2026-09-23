# Fixed Assets: Entity Catalog (m7 → `fa`)

> Legacy "Fixed Assets" (m7) maps to the **`fa_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15). **Source note:** m7 is **not** in `semantic-schema.json` (m0–m5 only) — this
> catalog is derived from `/home/rania/apps/myerpplus_serenity.sql` (read-only) +
> the m7 Flex screens (which surface AR/AQ/AO/AE/AG/DA/AT + Master Asset/Category).

Field-level model. Types Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
Global **audit + soft-delete** ([README §3](README.md#3-global-conventions)) and
`legacyCode String?` on masters — omitted per-row. `*custom{text,int,dbl,date}*`
→ `metadata Json?`. Money/qty `Decimal(19,4)`, rate `Decimal(19,6)`.

> **Asset lifecycle:** Requisition → Quotation → Order → Acquisition →
> Registration (capitalize) → … periodic Depreciation … → Transfer / Disposal.
> The acquisition chain (AR→AQ→AO→AE) **reuses the m4
> [«PurchaseDocHeader»](entities-m4-purchasing.md#common-shapes-defined-once-referenced-per-entity)**
> shape with «AssetLine» instead of item lines; AT payment **reuses
> `fin_ap_payments`** (no `fa_payment*` table). Period FK =
> `fiscalPeriodId → sys_fiscal_periods`; dimensions reuse m2 `md_*`.

---

## Masters (`fa_*`)

### ErpFaAssetCategory → `fa_asset_categories`  (legacy `m7_asset_category`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `ackode` |
| name | String | `acnama` |
| taxCategoryId ○ ➜ | BigInt → ErpFaAssetCategoryTax | `ackategoripajak` |
| assetAccountId ○ ➜ | BigInt → Account | `acrekasset` |
| accumDepreciationAccountId ○ ➜ | BigInt → Account | `acrekakumdepresiasi` |
| depreciationExpenseAccountId ○ ➜ | BigInt → Account | `acrekdepresiasi` |
| metadata ○ | Json | `accustom*` |

### ErpFaAssetCategoryTax → `fa_asset_category_taxes`  (legacy `m7_asset_category_tax`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | `actkode` |
| name | String | `actnama` |
| method ◆ | `DepreciationMethod` | `actmetode` |
| usefulLifeMonths | Int | `actumur` |
| depreciationTable ○ | String | `actpenyusutan` (fiscal table ref) |
| metadata ○ | Json | `actcustom*` |

### ErpFaDepreciationCategory → `fa_depreciation_categories`  (legacy `m7_depreciation_category`)

`id`, `code 🔑` (`kode`), `name` (`nama`), `isActive` (`aktif`). Lookup only.

### ErpFaAssetDepartment → `fa_asset_departments`  (legacy `m7_master_asset_department`)

`id`, `code 🔑` (`dpkode`), `name` (`dpnama`), `locationId ○ ➜ Location`
(`dplokasi`), `description ○` (`dpketerangan`). Asset-specific department
(distinct from GL dimensions; not folded into `md_*`).

### ErpFaAsset → `fa_assets`  (legacy `m7_asset`) — the register

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | `aid` |
| code 🔑 | String unique | `akode` |
| name | String | `anama` |
| categoryId ➜ | BigInt → ErpFaAssetCategory | `akategori` |
| tagNumber ○ | String | `anomor` (physical tag) |
| linkedItemId ○ ➜ | BigInt → Item | `aidbarang` |
| branchId ➜ | BigInt → Branch | `acabang` |
| locationId ○ ➜ | BigInt → Location | `alokasi` |
| warehouseId ○ ➜ | BigInt → Warehouse | `agudang` |
| departmentId ○ ➜ | BigInt → ErpFaAssetDepartment | (asset dept) |
| costCenterId / divisionId / subdivisionId / projectId ○ ➜ | BigInt → `md_*` | `acostcenter`/`adivisi`/`asubdivisi`/`aproyek` |
| purchaseDate ○ | Date | `atglbeli` |
| inServiceDate ○ | Date | `atglpakai` |
| quantity | Decimal(19,4) | `ajml` |
| unitId ○ ➜ | BigInt → Unit | `asatuan` |
| currencyId ➜ | BigInt → Currency | `amatauang` |
| exchangeRate | Decimal(19,6) | `akurs` |
| acquisitionCost | Decimal(19,4) | `ahargabeli` (`aharga`−disc+tax) |
| residualValue | Decimal(19,4) | `anilairesidu` |
| usefulLifeMonths | Int | `aumurekonomis` |
| monthlyDepreciation | Decimal(19,4) | `abebanperbln` |
| accumulatedDepreciation | Decimal(19,4) | `aakumulasibeban` |
| bookValue | Decimal(19,4) | `anilaibuku` |
| depreciationCount | Int | `apenyusutanke` |
| method ◆ | `DepreciationMethod` | `ametode` |
| depreciationTable ○ | String | `atabelpenyusutan` |
| isIntangible | Boolean | `aintangible` |
| isFiscal | Boolean | `afiskal` (fiscal-book asset) |
| halfMonthConvention | Boolean | `aatastengahbulan` |
| isDecliningValue | Boolean | `anilaimenurun` |
| assetAccountId ○ ➜ | BigInt → Account | `arekasset` |
| accumDepreciationAccountId ○ ➜ | BigInt → Account | `arekakumdepresiasi` |
| depreciationExpenseAccountId ○ ➜ | BigInt → Account | `arekdepresiasi` |
| disposalAccountId ○ ➜ | BigInt → Account | `arekpenghapusan` |
| manufacturer ○ | String | `aprodusen` |
| retirementDate ○ | Date | `atglpensiun` |
| isDisposed | Boolean | `adispose` |
| isLocked | Boolean | `alocked` (period-locked) |
| status ◆ | `DocumentStatus` | `astatus` |
| previousStatus ○ ◆ | `DocumentStatus` | `astatussebelumnya` |
| metadata ○ | Json | `acustom*`, `apembelian`/`apenjualan`/`a*sebelumnya` |

Relations: `category`, `movements ErpFaAssetMovement[]`. Running
`accumulatedDepreciation`/`bookValue` are maintained by posting depreciation
runs (the movement ledger is the audit trail).

### ErpFaAssetMovement → `fa_asset_movements`  (legacy `m7_asset_transaction`)

Per-asset movement ledger — append-on-post, immutable (mirrors
`fin_ledger_entries` discipline).

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | `atid` |
| assetId ➜ | BigInt → ErpFaAsset | `atasetid` |
| movementType ◆ | `AssetMovementType` | `atjenismutasi` |
| sourceDocType ○ | String | `atsumber` |
| sourceId ○ | BigInt | `atidutama` |
| docNumber ○ | String | `atnotransaksi` |
| movementDate | Date | `attgl` |
| amount ○ | Decimal(19,4) | snapshot of value moved |
| accumulatedAfter ○ | Decimal(19,4) | `atakumulasibeban` |
| bookValueAfter ○ | Decimal(19,4) | `atnilaibuku` |
| dimensions ○ | (FKs) | `atcostcenter`/`atdivisi`/… → `md_*` |
| status ◆ | `DocumentStatus` | `atstatus` |
| metadata ○ | Json | `atcustom*` + snapshot fields |

`@@index([assetId, movementDate])`, `@@index([sourceDocType, sourceId])`.

---

## Acquisition chain (`fa_*`) — reuses «PurchaseDocHeader» + «AssetLine»

«AssetLine» = «PurchaseDocLine» with `assetId ➜ ErpFaAsset` / `assetName`
(`idasset`/`namaasset`) instead of `itemId`, and `assetAccountId ➜ Account`
(`rekasset`), `purchaseDiscountAccountId` (`rekdiskonpembelian`),
`acquisitionPayableAccountId` (`rekhutangpembelian`). Each = «PurchaseDocHeader»
(supplier-side; `supplierId → Partner`) + `lines: «AssetLine»[]`.

### ErpFaAssetRequisition → `fa_asset_requisitions`  (legacy `m7_ar`)
- Request to acquire an asset. `requestedById ➜ ErpUser` (`ardimintaoleh`),
  `neededDate ○` (`artgldipakai`). `faDocType = REQUISITION`.

### ErpFaAssetQuotation → `fa_asset_quotations`  (legacy `m7_aq`)
- Supplier quote. `groupNo ○` (`aqnogrup`). Upstream: `requisitionId` (`aqidar`).

### ErpFaAssetOrder → `fa_asset_orders`  (legacy `m7_ao`)
- Asset purchase order. Upstream: `requisitionId`/`quotationId`.

### ErpFaAcquisition → `fa_acquisitions`  (legacy `m7_ae`)
- Asset purchase invoice = **AP open item**. Adds `taxInvoiceNo ○`
  (`aenofakturpajak`), `settlementStatus ◆` (`aestatuslunas`), `settledDate ○`.
  Upstream: `requisitionId`/`quotationId`/`orderId` (`aeidar`/`aeidaq`/`aeidao`).
  Posting → `fin_ledger_entries` (asset clearing / AP).

### ErpFaAssetRegistration → `fa_asset_registrations`  (legacy `m7_ag`)
- Capitalize acquired items into `fa_assets` (set cost basis, accounts, life).
  Lines: `assetId ➜`, `acquisitionCost` (`hargabeli`), `assetAccountId ➜`, dims.

---

## Depreciation, transfer, disposal (`fa_*`)

### ErpFaDepreciationRun → `fa_depreciation_runs` (+ `fa_depreciation_run_lines`)  (legacy `m7_da` + `m7_da_detail`)

| Field (header) | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `danotransaksi` |
| autoNumber ○ | String | `daautonotransaksi` |
| branchId ➜ | BigInt → Branch | `dacabang` |
| runDate | Date | `datgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `dakodepa` |
| currencyId ➜ | BigInt → Currency | `damatauang` |
| exchangeRate | Decimal(19,6) | `dakurs` |
| description ○ | String | `dauraian` |
| status ◆ | `DocumentStatus` | `dastatus` |
| postingStatus ◆ | `PostingStatus` | `daposting` |
| postedAt ○ | DateTime | `dapostingtgl` |

Line (`m7_da_detail`): `depreciationRunId ➜`, `assetId ➜` (`idaset`),
`depreciationNo Int` (`penyusutanke`), `depreciationAmount` (`nilaipenyusutan`),
`bookValueBefore` (`nilaibukusebelumnya`), dims, `lineNo`, `metadata`. Posting →
`fin_ledger_entries` (Dr depreciation expense / Cr accumulated depreciation) and
a `fa_asset_movements` row per asset.

### ErpFaTransfer → `fa_transfers`  (legacy asset-transfer doc family)
- Move an asset's branch/location/department/custodian/dimensions. Lean header
  + lines (`assetId ➜`, from/to location/department, effective date). Writes a
  `fa_asset_movements` (`TRANSFER`) row; no GL effect (or branch reclass only).

### ErpFaDisposal → `fa_disposals`  (legacy asset-disposal doc family + `m7_master_asset_disposal`)
- Sale / scrap / write-off. Lean header + lines (`assetId ➜`, `disposalType`,
  `proceeds ○`, `gainLossAccountId ○ ➜`). Posting → derecognize asset:
  `fin_ledger_entries` (reverse cost & accumulated dep, book gain/loss) +
  `fa_asset_movements` (`DISPOSAL`); sets `fa_assets.isDisposed`.

---

## Payment — reuses the finance domain

`m7_at` (+ `m7_at_detail` / `m7_at_pay`) = asset-acquisition payment. Maps to
**`fin_ap_payments` / `fin_payment_instruments` / `fin_settlement_allocations`**
(the `fa_acquisitions` invoice is the AP open item) — **no `fa_payment*` table**.
`at*diskontermin`/`atrekdiskontermin` → the **already-flagged** `fin_ap_payments`
term-discount columns (see entities-m4-purchasing.md follow-up).

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `DepreciationMethod` | `STRAIGHT_LINE`, `DECLINING_BALANCE`, `DOUBLE_DECLINING`, `SUM_OF_YEARS`, `UNITS_OF_PRODUCTION`, `NONE` | `ametode`/`actmetode` |
| `FaDocType` | `REQUISITION`, `QUOTATION`, `ORDER`, `ACQUISITION`, `REGISTRATION`, `DEPRECIATION`, `TRANSFER`, `DISPOSAL` | the m7 doc set |
| `AssetMovementType` | `ACQUISITION`, `DEPRECIATION`, `REVALUATION`, `TRANSFER`, `DISPOSAL`, `ADJUSTMENT` | `m7_asset_transaction.atjenismutasi` |

Reused: `DocumentStatus`, `PostingStatus`, `PriceMode`.

### Flagged / secondary (not modeled in core)
- Legacy asset-ops long tail (`m7_ab`, `m7_ac`, `m7_asl`, `m7_asr`, `m7_dsa`,
  `m7_dsr`, `m7_ia`, `m7_ir`, `m7_irt`, `m7_ra`, `m7_ta`, `m7_te`, `m7_tr`,
  `m7_ua`, `m7_ur`, `m7_urt`) — additional revaluation/impairment/usage/transfer
  variants **not surfaced** by the m7 Flex screens. Folded conceptually into
  `fa_asset_movements` / `fa_transfers` / `fa_disposals`; **exact per-doc
  mapping deferred** until a finance/fa deep phase needs them.
- `m7_files` / `m7_notes` — attachments/notes (app concern), not modeled.
- All `*_history` → `sys_audit_logs`.

---

**Count:** ~13 core Fixed-Assets (`fa_*`) entities — 4 masters (AssetCategory,
AssetCategoryTax, DepreciationCategory, AssetDepartment) + Asset + AssetMovement
+ acquisition chain (Requisition/Quotation/Order/Acquisition + Registration) +
DepreciationRun(+lines) + Transfer + Disposal ≈ **~17 tables**. Payment reuses
`fin_*`; period reuses `sys_fiscal_periods`; dimensions reuse `md_*`. Asset-ops
long tail flagged/deferred.

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
