# Legacy MyERP+ → Modern Web-ERP Mapping & Deferred Backlog

Even though the new product uses **modern English naming** (not a 1:1 port), this
appendix preserves the legacy lineage so a future CDC/ETL from MyERP+
(`apps/myerpplus-db-mapping`, `myerpplus_serenity.sql`) has a documented trail.

Authoritative legacy reference: `apps/myerpplus-db-mapping/db/semantic-schema.json`.

---

## 1. MVP table mapping (in scope)

| Modern entity | Legacy table | Notes on transform |
| --- | --- | --- |
| ErpUser | `m0_user` | `upassword` plaintext → **hash**; `ulevel` int → `UserLevel` enum; audit cols → standard |
| ErpRole | *(none)* | new — legacy access via `m0_setting['hakakses']` + `m0_user.ugrup` |
| ErpPermission | *(none)* | new — explicit permission catalog |
| ErpUserRole | `m0_user.ugrup` | exploded to a join table |
| ErpRolePermission | `m0_setting['hakakses']` | parsed from setting blob → explicit rows |
| ErpMenu | `m0_menu` (+ `m0_module`) | module folds into a top-level `MODULE` menu node |
| ErpRoleMenu | `m0_menu` `c1..c13` bits | opaque bitfield → 8 named booleans + favorite |
| UserBranchAccess | `m0_user_branch` | direct |
| UserLocationAccess | `m0_user_location` | direct |
| UserWarehouseAccess | `m0_user_warehouse` | direct |
| ErpSetting | `m0_setting` | access-control rows removed (→ Role/Permission) |
| DocumentNumbering | `m0_nomor` | add explicit `nextNumber`, `resetPolicy` |
| FiscalPeriod | `m0_setting` (fiscal keys) | promoted to a first-class table; global scope |
| ErpAuditLog | `m0_userlog` | promoted into MVP (resolved §8 #3); append-only, `AuditAction` enum |
| Branch | `m1_branch` | direct |
| Location | `m1_location` | direct; `lkategoripos` → metadata |
| Warehouse | `m1_warehouse` | `wdivisi` dropped (org hierarchy deferred) |
| Unit | `m1_unit` | direct |
| ItemCategory | `m1_item_category` | add optional `parentId`; keeps 3 default GL FKs |
| Item | `m1_item` | **128 → ~24 cols**; tiers/dims/custom → deferred or `metadata` |
| Partner | `m1_contact` | **128 → core**; addresses/contacts/banks extracted |
| PartnerAddress | `m1_contact` k1..k4 | 4 inline blocks → 1:N rows |
| PartnerContact | `m1_contact_attention` | direct |
| PartnerBankAccount | `m1_contact.kbank/knorekening` | extracted to 1:N |
| PartnerCategory | `m1_contact_category` (+ customer/supplier/salesman category) | merged, add `kind` discriminator |
| Currency | `m1_currency` | snapshot `ckurs` dropped |
| CurrencyRate | `m1_currency.ckurs` | new — dated FX rows (resolved §8 #8); seed one row from `ckurs` on backfill |
| Account | `m1_coa` | `cjenis A/H/M/P/B` → `kind`+`type`+`normalBalance`; multi-dim deferred |
| Tax | `m1_tax` | direct |
| PaymentTerm | `m1_terms` | direct |

## 1b. m2 Finance → `fin` mapping (catalogued — see entities-m2-finance.md)

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpFinJournalEntry | `m2_gj` `m2_jm` `m2_aj` `m2_cb` | 4 near-identical headers unified via `journalType` enum |
| ErpFinJournalLine | `m2_*_detail` | normalized; `costcenter/divisi/subdivisi/proyek` → dimension FKs |
| ErpFinLedgerEntry | `m2_transaction_journal` | the posted GL; append-only/immutable; AR-AP open-item markers kept |
| ErpFinCashBankTransaction | `m2_cr` `m2_bd` `m2_cd` | `direction` enum (RECEIPT/DISBURSEMENT); `anggaran*` → line dimensions |
| ErpFinCashBankLine | `m2_cr_detail` `m2_bd_detail` | normalized lines |
| ErpFinArReceipt | `m2_rm` | customer receipt |
| ErpFinApPayment | `m2_sm` | supplier payment (symmetric to AR) |
| ErpFinPaymentInstrument | `m2_rm_pay` `m2_sm_pay` | unified tender lines; `carabayar` → `PaymentMethod` |
| ErpFinSettlementAllocation | *(none — legacy via `transaction_journal` open-items)* | explicit receipt/payment → invoice allocation |
| ErpFinGiro | `m2_rg` `m2_sg` `m2_giro_list`; clr `m2_rgc`/`m2_sgc` | giro register; clearing = status transition, not a table |
| ErpFinBudgetRealization | `m2_realization` + `m2_realization_*` | per-dimension split collapsed to dimension FKs |
| *(reuse `sys_fiscal_periods`)* | `m2_accounting_period` | no new table — period gate is fiscal period |
| CostCenter/Division/Subdivision/Project | `m1_cost_center`/`m1_division`/`m1_subdivision`/`m1_project` | un-deferred GL dimension masters → `md_*` (§8 #16) |

`riwayat_*` snapshot tables (none in m2 explicitly) and `*_gagal` staging are not modeled
(change history → `sys_audit_logs`; failed-process staging is an app concern).

## 1c. m3 Inventory → `inv` mapping (catalogued — see entities-m3-inventory.md)

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpInvStockMovement(+Line) | `m3_mr` `m3_rf` `m3_ts` `m3_rs` (+`_detail`) | 4 near-identical headers → `movementType` enum; MR→TS→RS chain via self-ref |
| ErpInvOpeningStock(+Line) | `m3_ib` `m3_ib_detail` | per-warehouse opening qty + cost + inventory account |
| ErpInvStockCount(+Line) | `m3_sp` `m3_sp_detail` | physical count: system/physical/good/damaged/variance |
| ErpInvStockAdjustment(+Line) | `m3_sa` `m3_sa_detail` | GL-posting adj (`rekpersediaan`/`reklawan`); links to count |
| ErpInvWeighbridgeTicket | `m3_rw` | gross/tare/net weighing ticket |
| inv_stock_balances *(view)* | *(none — legacy `bstok` cache)* | derived on-hand; never written |
| *(flagged out)* | `m3_dc` (+`_check`/`_detail`) | equipment hour-meter/check log → study with m6 `mfg` |
| *(deferred)* | `m3_pa` (+`_detail`) | 10-tier price/discount revision → pricing phase (§8 #3) |

Denormalized `namabarang`/`tipebarang`/`satuanbarang` echoes dropped (→ `md_items`/
`md_units` relations); `hpplama` prior-cost not stored (derived).

## 1d. m4 Purchasing → `pur` mapping (catalogued — see entities-m4-purchasing.md)

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpPurRequisition(+Line) | `m4_pr` (+`_detail`) | internal requisition; `pridsq` = sales demand link |
| ErpPurRfq(+Supplier) | `m4_rfq` (+`_detail`) | rfq_detail = invited suppliers, not items |
| ErpPurQuotation(+Line) | `m4_rq` (+`_detail`) | supplier priced offer |
| ErpPurBidSelection(+Line/+Quotations) | `m4_bs` (+`_detail`) | RQ comparison/award (`bsidrq1..5`, `terpilih`, `hargake`) |
| ErpPurOrder(+Line) | `m4_po` (+`_detail`) | purchase order |
| ErpPurGoodsReceipt(+Line) | `m4_grn` (+`_detail`) | goods receipt; accrues GR/IR (`rekhutangsementara`), moves `inv` |
| ErpPurInvoice(+Line) | `m4_ri` (+`_detail`); `m4_ipc` folded | supplier bill = AP open item |
| ErpPurReturn(+Line) | `m4_dnr` + `m4_prt` (+`_detail`) | unified via `returnType` enum |
| *(reuse `fin_ap_payments`)* | `m4_ap` `m4_pp` `m4_vp` `m4_vpp` (+`_pay`/`_detail`) | one AP-payment concept; +4 FX/term cols flagged onto `fin_ap_payments` |
| *(not modeled)* | `m4_pie` | generic invoice-entry aggregator → superseded by chain links |

Per-step fulfilment counters (`jml*`/`status*`) **derived** (downstream sums), not stored;
partner address blocks (`*1alamat*`/`*2alamat*`) resolved via `md_partner_addresses`.

## 1e. m5 Sales → `sls` mapping (catalogued — see entities-m5-sales.md)

Mirror of m4 (order-to-cash). ~40 `riwayat_*` shadow tables → `sys_audit_logs`.

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpSlsQuotation(+Line/+Materials) | `m5_sq` (+`_detail`/`_out_bahan`) | `sqidpr` = buyer RFP link |
| ErpSlsOrder(+Line) | `m5_so` (+`_detail`) | sales order |
| ErpSlsProformaInvoice(+Line) | `m5_pi` (+`_detail`) | proforma |
| ErpSlsPackingList(+Line/+Packs) | `m5_pl` (+`_detail`/`_pack`) | pick/pack |
| ErpSlsDeliveryOrder(+Line) | `m5_do` (+`_detail`) | surat jalan |
| ErpSlsDeliveryReport(+Line) | `m5_dr` (+`_detail`) | delivery confirmation |
| ErpSlsInvoice(+Line) | `m5_si` (+`_detail`) | AR open item; POS tender→`fin`, points→m12 |
| ErpSlsReturn(+Line) | `m5_sr` (+`_detail`) | sales return / credit note |
| ErpSlsReturnReceipt(+Line) | `m5_rnr` (+`_detail`) | goods-back receipt; moves `inv` in |
| ErpSlsInvoiceInstallment/Material/Cost | `m5_si_installment`/`_material`/`_cost` | invoice satellites |
| ErpSlsCustomerAdvance | `m5_as` (+`_pay`→`fin`) | customer advance/down-payment |
| ErpSlsInvoiceSwap | `m5_sie` | tukar faktur |
| ErpSlsForecast | `m5_sf` | lean sales forecast |
| *(reuse `fin_ar_receipts`)* | `m5_ip` `m5_pv` `m5_ic` (+`_pay`/alloc) | one AR-receipt concept; +4 FX/term cols flagged onto `fin_ar_receipts` |
| *(flagged out)* | `m5_cl` (sales closing), `m5_spa` (loyalty→m12), `m5_rp` (freight AR) | secondary/deferred |

## 1f. m6 Manufacturing → `mfg` mapping (catalogued — see entities-m6-manufacturing.md)

> **Source note:** m6+ is **not** in `semantic-schema.json` (m0–m5 only). m6 was
> read from the legacy schema in `/home/rania/apps/myerpplus_serenity.sql`
> (read-only) + m6 Flex screens. Same applies to later non-m0–m5 modules.

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpMfgBom(+Inputs/Outputs) | `m6_bom` `m6_bom_in/out` (`m6_itembom_in/out`) | versioned recipe; standing per-item BOM = same line tables |
| ErpMfgWorkOrder(+In/Out/Activities/RouteCards) | `m6_wo` `m6_wo_in/out/activity/route_card` | production order vs BOM |
| ErpMfgMaterialIssue(+In/Out) | `m6_mrs` `m6_mrs_in/out` | issue materials to floor (store→WIP) |
| ErpMfgMaterialReturn(+In/Out) | `m6_mrn` `m6_mrn_in/out` | return materials from floor (WIP→store) |
| ErpMfgProductionEntry(+In/Out/Boms) | `m6_pd` `m6_pd_in/out/bom` | execute production; COGM/COGS + stock |
| ErpMfgProductionRework(+In/Out) | `m6_pdr` `m6_pdr_in/out` | rework/disassembly |
| *(not modeled)* | `m6_files` `m6_notes`; all `*_history` | attachments/notes app concern; history→`sys_audit_logs` |

`*custom{text,int,dbl,date}*` → `metadata Json?`; machine master (`m1_machine`+)
stays **deferred** — `work_order_activities.machineCode` is a string for now.

## 1g. m7 Fixed Assets → `fa` mapping (catalogued — see entities-m7-fixed-assets.md)

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpFaAssetCategory / …Tax / DepreciationCategory / AssetDepartment | `m7_asset_category` `_tax` / `m7_depreciation_category` / `m7_master_asset_department` | masters |
| ErpFaAsset | `m7_asset` | the register; running accumDep/bookValue maintained by posting |
| ErpFaAssetMovement | `m7_asset_transaction` | append-only per-asset ledger (`atjenismutasi`) |
| ErpFaAssetRequisition/Quotation/Order/Acquisition(+lines) | `m7_ar`/`m7_aq`/`m7_ao`/`m7_ae` (+`_detail`) | reuse «PurchaseDocHeader»+«AssetLine» |
| ErpFaAssetRegistration | `m7_ag` (+`_detail`) | capitalize acquired items into assets |
| ErpFaDepreciationRun(+lines) | `m7_da` `m7_da_detail` | periodic depreciation; posts to `fin` + movements |
| ErpFaTransfer / ErpFaDisposal | asset transfer/disposal doc family + `m7_master_asset_disposal` | move / derecognize asset |
| *(reuse `fin_ap_payments`)* | `m7_at` (+`_detail`/`_pay`) | asset payable payment; term-disc cols already flagged |
| *(flagged/deferred)* | `m7_ab/ac/asl/asr/dsa/dsr/ia/ir/irt/ra/ta/te/tr/ua/ur/urt` | asset-ops long tail not surfaced by Flex; folded into movements/transfer/disposal |

## 1h. m12 POS / Retail → `pos` mapping (catalogued — see entities-m12-pos.md)

> **Source note:** m12 is in **neither** `semantic-schema.json` **nor**
> `myerpplus_serenity.sql`. Derived from backend VB ws (`app_code/ws/m12/*.vb`) +
> m12 Flex screens — **inferred field-level**, verify vs live m12 schema first.

| Modern entity | Legacy table(s) | Notes on transform |
| --- | --- | --- |
| ErpPosArea(+Category) | `m12_area` `m12_area_category` | sales territory master |
| ErpPosTerminal / TransactionType | `m12_pos_hardware` / `m12_pos_type`+`_category`(`_setting`) | station + POS doc-type; scalar config → `sys_settings` |
| ErpPosItemPrice(+Tiers) | `m12_pos_item` | **realizes deferred tiered pricing** (§8 #3): `hargajual1..5`/`diskonjual1..5` → tier rows |
| ErpPosPriceAgreement(+lines) | `m12_cpa` | per-customer price + loyalty point roll |
| ErpPosPromotion + Bonus/Substitution/Additional/Discount rules | `m12_pos_promo` `_bonus_item`/`_bonus_trans` `_substitution_item` `_additional_item` `_discount_item`/`_discount_category_item`/`_discount_category_customer` | discount matrices unified via `scope` enum |
| ErpPosPointRule / PointTransaction / Voucher | `m12_pos_point_item`/`_point_category_item` / `_point_transaction` / `_voucher` | loyalty engine |
| *(reuse `sls_invoices` channel=POS)* | `m12_si` (+`_history`) | POS sale = m5 invoice; tender→`fin`, points→`pos`; +4 cols flagged onto `sls_invoices` |
| *(reuse `inv`)* | `m12_st` | POS stock transfer → `inv_stock_movements` |
| *(not modeled)* | `m12_ppa/ppv/sbi/ai/bi/di/lp/dataPenjualan` (reports), `m12_upload/getValueMember/item/contact` (shims) | report views / utility over `md_*` |

## 2. Cross-cutting transforms (every table)

| Legacy pattern | Modern handling |
| --- | --- |
| `*aktif = 0/1` | `deletedAt` (delete) + `isActive` (business toggle) |
| `*inputuser/*inputtgl/*modifikasiuser/*modifikasitgl` | `createdById/createdAt/updatedById/updatedAt` |
| `*custom1..15`, `*customtext1..5` | `metadata Json?` |
| denormalized `*nama` echo columns | dropped — resolved via relations/API |
| naive local datetime | `timestamptz` UTC; business TZ in app layer (ADR 008) |
| `latin1` encoding | UTF-8 on import (db-mapping convention) |
| FKs not enforced | Prisma relations + `onDelete` rules; validate on ETL import |
| string code as PK | surrogate `id BigInt` + unique `code` + `legacyCode String?` (original MyERP+ code, indexed — keeps the backfill trail; resolved §8 #2/#7) |

## 3. Deferred legacy tables (catalogued, NOT in m0+m1 MVP)

Scoped out of the **m0+m1 MVP** (Core subset). The legacy *transactional modules*
m2–m12 are no longer merely deferred — they are mapped at roadmap depth in
**[module-roadmap.md](module-roadmap.md)** (resolved README §8 #14–17). The list
below is the remaining **m1 master** lineage still outside the first slice.

**Org hierarchy:** `m1_department`, `m1_subdepartment`, `m1_section` *(still
deferred)*. `m1_division`, `m1_subdivision`, `m1_cost_center`, `m1_project`
**un-deferred** → `md_divisions`/`md_subdivisions`/`md_cost_centers`/`md_projects`
as GL dimension masters (resolved §8 #16; see module-roadmap.md m2 `fin`).

**Product attributes:** `m1_merk` (brand), `m1_warna`/color, `m1_size`,
`m1_model`, `m1_material`, `m1_class_product`, `m1_oem`, `m1_designer`.

**Pricing depth:** `m1_item_price` (multi-currency), `m1_contact_price`,
`m1_price_category` + `_detail`, `m1_index_price`, item tiers `bhargajual1..10`
/ `bdiskonjual1..10`.

**Item extras:** `m1_item_type` (kept as enum), `m1_item_location`,
`m1_item_location_warehouse`, `m1_item_stock_warehouse`, `m1_item_supplier`,
`m1_item_permission` (**dropped 2026-06-12** — fitur Izin Item/`md_item_permissions`
dihapus penuh atas permintaan user: halaman, endpoint, dan tabel),
`m1_item_transaction` (→ inventory module),
`m1_item_hauling`.

**Geography:** `m1_country`, `m1_province`, `m1_city`.

**Other masters:** `m1_bank` (standalone bank master),
`m1_expedition`, `m1_machine` + `_kapasitas`, `m1_kategori_mesin`,
`m1_production_activity`, `m1_route_card` (BOM), `m1_production_category`,
`m1_working`, `m1_other_cost`, `m1_selling_point`, `m1_vendor`,
`m1_checking_category`, `m1_transaction_note` + `_detail`,
`m1_contact_terms` (special per-contact term overrides).

**Admin extras:** approval workflow (`m0_setting` approval),
file/attachment manager (`m0_files`),
data import/export/purge tooling, language/localization tables
(`m0_language*`), serial/batch registries, journal/COGS recompute utilities,
statistics/report-definition manager.

## 4. ETL / CDC guidance (future, from `myerpplus-db-mapping/CLAUDE.md`)

- MyERP+ source is **read-only**; never UPDATE/DELETE the production MySQL.
- Document each mapping (source table/col → target) in a `plan.md` before implementing.
- Filter soft-deleted rows on import (`*aktif`/`deleted_at`).
- Convert `latin1` → UTF-8 on insert to Postgres.
- Validate referential integrity on import (legacy FKs not enforced).
- Treat legacy timestamps as Asia/Jakarta unless proven otherwise; store UTC.

---

*This appendix is documentation only — no ETL is built in this phase.*
