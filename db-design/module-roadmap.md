# Web-ERP — Module Roadmap (legacy m2–m12 → modern domains)

> Status: **ROADMAP (coarse mapping)** — entity inventory only, **no field catalog**.
> Date: 2026-05-17 · Product: Senti ERP, `apps/web-erp`.
> Per-module field-level catalogs (like `entities-m0-administrator.md` /
> `entities-m1-master-data.md`) are produced **one module at a time, after review**.

This extends the [db-design/](README.md) set beyond the m0+m1 MVP. It maps every
legacy MyERP+ module to a **semantic domain** (per [web-erp/CLAUDE.md §1](../CLAUDE.md))
and its **modern core entities** (core-subset, not legacy 1:1 — resolved
[README §8](README.md#8-resolved-decisions-2026-05-17)).

## Decisions applied to this roadmap (2026-05-17)

- **Scope of this pass:** coarse inventory of *all* modules first; deep field
  catalogs follow per module, in sequence, after review.
- **Depth:** modern **core subset** per module — unify legacy document variants,
  drop `riwayat_*` shadow tables (change history now lives in `sys_audit_logs`),
  drop `*_detail`/`*_pay` denormalization into normalized line tables.
- **GL multi-dimension: IN.** Revises README §8 #9 — journal lines carry full
  analytic dimensions (branch, cost center, division, project, subdivision,
  location). This **pulls org-hierarchy masters into `md`** (previously deferred).
- **Domain naming:** semantic per-function, no numeric `m<n>` (CLAUDE.md §1).

## Legacy module → modern domain map

| Legacy | Module | Legacy tables | Domain | Status |
| --- | --- | --- | --- | --- |
| m0 | Administrator | 10 | `sys` / `adm` | ✅ catalogued (MVP) |
| m1 | Master Data | 49 | `md` | ✅ catalogued (MVP) |
| m2 | Finance / Accounting (GL) | ~38 | **`fin`** | ✅ **catalogued** → `entities-m2-finance.md` |
| m3 | Inventory (stock movement) | ~20 | **`inv`** | ✅ **catalogued** → `entities-m3-inventory.md` |
| m4 | Purchasing | ~35 | **`pur`** | ✅ **catalogued** → `entities-m4-purchasing.md` |
| m5 | Sales / AR | ~81 | **`sls`** | ✅ **catalogued** → `entities-m5-sales.md` |
| m6 | Manufacturing / Production | ~17 | **`mfg`** | ✅ **catalogued** → `entities-m6-manufacturing.md` |
| m7 | Fixed Assets | ~17 | **`fa`** | ✅ **catalogued** → `entities-m7-fixed-assets.md` |
| m8 | Business Intelligence / Dashboards | — | **`bi`** | ⏳ roadmapped (config-heavy) |
| m9 | *(none — no backend/frontend in legacy)* | 0 | — | ❌ skip |
| m10 | *(unclear — frontend-only, no backend ws)* | — | `?` | ⚠ needs study before mapping |
| m11 | Clinic / lab / healthcare billing | — | — | ❌ **out of ERP scope** (Althea vertical → `apps/web-althea`) |
| m12 | POS / Retail & Promotions | ~18 | **`pos`** | ✅ **catalogued** → `entities-m12-pos.md` |
| *(new)* | Planning / MRP-lite | 0 (no legacy) | **`pln`** | ✅ **catalogued** → `entities-pln-planning.md` (resolved §8 #29) |

> Legacy `m2`–`m5` are in `apps/myerpplus-db-mapping/db/semantic-schema.json`
> (authoritative). `m6`–`m12` inferred from the legacy Flex screens
> (`preferensi/Frontned - myerpplus/erp_modN`) + backend `app_code/ws/mN`.

---

## m2 → `fin` — Finance / Accounting ✅ catalogued

> **Field-level catalog done:** [entities-m2-finance.md](entities-m2-finance.md)
> (11 `fin_*` entities + 4 `md_*` GL dimension masters; period reuses
> `sys_fiscal_periods`). The table below is the original coarse plan, kept for context.

Legacy doc codes: GJ=jurnal umum, JM=jurnal memorial, AJ=jurnal penyesuaian,
CR=kas/bank masuk, BD=kas/bank keluar, CD=cash deposit, RM=penerimaan piutang,
SM=pembayaran hutang, RG/SG=giro masuk/keluar, RGC/SGC=giro clearing,
CB=saldo awal COA, `transaction_journal`=buku besar terposting.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| *(reuse `sys_fiscal_periods`)* | `m2_accounting_period` | posting-period open/close gate — already in MVP, **no new table** |
| `fin_journal_entries` | `m2_gj` / `m2_jm` / `m2_aj` | journal header, unified via `journalType` enum (GENERAL/MEMORIAL/ADJUSTMENT) |
| `fin_journal_lines` | `*_detail` | debit/credit line per account **+ analytic dimensions** |
| `fin_ledger_entries` | `m2_transaction_journal` | posted GL movement (the general ledger / buku besar) |
| `fin_opening_balances` | `m2_cb` / `m2_cb_detail` | per-account opening balance per period |
| `fin_cash_bank_transactions` | `m2_cr` / `m2_bd` / `m2_cd` | cash/bank in & out voucher header |
| `fin_cash_bank_lines` | `*_detail` | allocation lines of a cash/bank voucher |
| `fin_ar_receipts` | `m2_rm` / `m2_rm_pay` | customer receipt + allocation to sales invoices |
| `fin_ap_payments` | `m2_sm` / `m2_sm_pay` | supplier payment + allocation to purchase invoices |
| `fin_giros` | `m2_rg` / `m2_sg` / `m2_rgc` / `m2_sgc` / `m2_giro_list` | giro/postdated-cheque register + clearing status |

**Dimension masters added to `md`** (GL dimensions decision — were deferred):
`md_cost_centers`, `md_divisions`, `md_subdivisions`, `md_projects`.
(`md_branches`/`md_locations` already exist in m1.) `fin_journal_lines` FK these.

---

## m3 → `inv` — Inventory (stock movement) ✅ catalogued

> **Field-level catalog done:** [entities-m3-inventory.md](entities-m3-inventory.md)
> (11 `inv_*` + derived `inv_stock_balances` view). `m3_dc` (equipment log) &
> `m3_pa` (tiered price revision) flagged out. Original coarse plan kept below.

Legacy doc codes: DC=delivery/transfer check, MR=material receive, PA=stock
adjustment, RF/RS/RW=receive/return variants, SA=stock opname, SP=stock
issue/pick, TS=transfer stock, IB=saldo awal barang.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `inv_stock_movements` | `m3_dc/mr/pa/rf/rs/rw/sp/ts` | unified stock txn header, `movementType` enum |
| `inv_stock_movement_lines` | `*_detail` | per-item qty/warehouse/cost line |
| `inv_opening_stocks` | `m3_ib` / `m3_ib_detail` | per-warehouse opening stock at period start |
| `inv_stock_counts` | `m3_sa` / `m3_sa_detail` / `m3_dc_check` | physical count / opname + variance |
| `inv_stock_balances` *(derived)* | *(none — legacy `bstok` cache)* | computed on-hand per item/warehouse (view/materialized, not a written master) |

---

## m4 → `pur` — Purchasing ✅ catalogued

> **Field-level catalog done:** [entities-m4-purchasing.md](entities-m4-purchasing.md)
> (~18 `pur_*` chain tables; payment reuses `fin_ap_payments`). `m4_ipc` folded,
> `m4_pie` not modeled. Original coarse plan kept below.

Legacy: RFQ=request for quotation, RQ=supplier quote, PR=purchase requisition,
PO=order pembelian, GRN=penerimaan barang, RI=invoice pembelian, DNR=debit
note/return, AP/PP/VP/VPP=payment variants, IPC=invoice price change, PIE/BS/CS.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `pur_requisitions` | `m4_pr` / `_detail` | internal purchase requisition |
| `pur_rfqs` | `m4_rfq` / `_detail` | request-for-quotation to suppliers |
| `pur_quotations` | `m4_rq` / `_detail` | supplier price quotation received |
| `pur_orders` | `m4_po` / `m4_po_detail` | purchase order to supplier |
| `pur_goods_receipts` | `m4_grn` / `_detail` | goods received against PO |
| `pur_invoices` | `m4_ri` / `_detail` / `m4_ipc` | supplier bill (AP source) + price corrections |
| `pur_returns` | `m4_dnr` / `_detail` | purchase return / debit note |
| `pur_payments` | `m4_ap/pp/vp/vpp` `*_pay` | supplier payment + allocation to invoices |

---

## m5 → `sls` — Sales / Accounts Receivable ✅ catalogued

> **Field-level catalog done:** [entities-m5-sales.md](entities-m5-sales.md)
> (~24 `sls_*`; mirror of `pur`; payment reuses `fin_ar_receipts`). `m5_cl`/`m5_rp`
> secondary, `m5_spa`+loyalty deferred to `pos`. **Pricing reuses `pos` SSOT (§8 #27);
> enterprise extras (credit/commission/target/blanket/CRM) reviewed & deferred (§8 #28).**
> Original coarse plan kept below.

Legacy: SQ=quotation, SO=order, DO=delivery order, DR=delivery report,
PL=packing list, PI=proforma invoice, SI=sales invoice, SR=sales return,
IP/PV=receipt/payment voucher, IC=invoice collection, RNR=return receipt,
AS=uang muka (advance), SIE=tukar faktur, SPA=sales-point adjustment.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `sls_quotations` | `m5` SQ + detail | sales quotation to customer |
| `sls_orders` | `m5` SO + detail | sales order (customer commitment) |
| `sls_deliveries` | `m5` DO/DR + PL | delivery order, packing & delivery report |
| `sls_invoices` | `m5` SI/PI + detail/material | sales invoice (AR source) + proforma |
| `sls_returns` | `m5` SR/RNR + detail | sales return / return receipt |
| `sls_receipts` | `m5` IP/PV + allocation | customer payment receipt + allocation |
| `sls_collections` | `m5` IC + detail | invoice collection / billing run |
| `sls_advances` | `m5` AS + payment | customer advance / down payment |
| `sls_invoice_swaps` | `m5` SIE | invoice exchange/replacement |
| `sls_point_adjustments` | `m5` SPA | loyalty-point adjustment per customer |
| `sls_forecasts` | `m5` sales contract/booking | sales forecast / contract booking |

> All legacy `riwayat_*` snapshot tables → dropped; change history is
> `sys_audit_logs`. `*_gagal` (failed-process staging) → not modeled (app concern).

---

## m6 → `mfg` — Manufacturing / Production ✅ catalogued

> **Field-level catalog done:** [entities-m6-manufacturing.md](entities-m6-manufacturing.md)
> (~17 `mfg_*`; BOM→WO→issue/return→production→rework, input/output line sets).
> Source = SQL dump (m6 not in semantic-schema). Original coarse plan kept below.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `mfg_boms` | `m6_bom` | bill of materials (recipe per produced item) |
| `mfg_work_orders` | `m6_wo` / WOSetBOM | production work order |
| `mfg_production_plans` | `m6` ProductionPlan | planned production schedule |
| `mfg_material_requisitions` | `m6_mrn` / `m6_mrs` | material issue/return to/from production |
| `mfg_production_results` | `m6` PD/PDP/PDR | production output & yield reporting |
| `mfg_machine_plottings` | `m6_machine_plotting` | machine/capacity scheduling |

---

## m7 → `fa` — Fixed Assets ✅ catalogued

> **Field-level catalog done:** [entities-m7-fixed-assets.md](entities-m7-fixed-assets.md)
> (~17 `fa_*`; acquisition chain reuses «PurchaseDocHeader», payment reuses
> `fin_ap_payments`). Asset-ops long tail flagged. Source = SQL dump. Plan below.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `fa_asset_categories` | `m7_asset_category` (+ `_tax`) | asset class + depreciation/tax defaults |
| `fa_assets` | `m7_asset` | fixed-asset register |
| `fa_acquisitions` | `m7_ae` | asset acquisition / capitalization |
| `fa_depreciations` | `m7_ag` | periodic depreciation run |
| `fa_disposals` | `m7_ao` / `m7_ar` | asset disposal / write-off / sale |
| `fa_adjustments` | `m7_aq` / `m7_at` / `m7_ab` | revaluation / adjustment / transfer-of-value |
| `fa_transfers` | `m7_da` | asset location/custodian transfer |

---

## m8 → `bi` — Business Intelligence / Dashboards (config-heavy)

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `bi_charts` | `m8_content_chart` / `m8_chart_finance` | dashboard chart definition |
| `bi_chart_contents` | `m8_content` / `m8_content_detail` | chart datasource/query config |
| `bi_indicators` | `m8_indicator` | KPI indicator definition |
| `bi_chart_roles` | `m8_content_role` | which role sees which dashboard |

> Mostly presentation config — confirm at catalog time whether this belongs in
> DB or app-side config before modeling.

---

## m12 → `pos` — POS / Retail & Promotions ✅ catalogued

> **Field-level catalog done:** [entities-m12-pos.md](entities-m12-pos.md)
> (~18 `pos_*`; resolves deferred tiered pricing; POS sale reuses `sls_invoices`).
> ⚠ Source = VB ws + Flex (not in semantic-schema/SQL) — inferred, verify first.

| Core entity | Maps legacy | One-line purpose |
| --- | --- | --- |
| `pos_areas` | `m12_area` (+ `_category`) | sales area / territory master |
| `pos_contact_prices` | `m12_cpa` | per-area / per-customer price agreement |
| `pos_bonus_items` | `m12` BarangBonus | buy-X-get-Y bonus rules |
| `pos_substitute_items` | `m12` BarangPengganti | substitute/replacement item rules |
| `pos_additional_items` | `m12` BarangTambahan | add-on item rules |
| `pos_installments` | `m12` Angsuran | retail installment / credit plan |
| `pos_category_discounts` | `m12` DiskonKategori (Barang/Customer) | category-level discount matrix |

---

## Excluded / open

- **m9** — no legacy backend or frontend; nothing to map.
- **m10** — frontend-only screens (AD/AL/EMR/PYP/TR), no backend `ws`. Purpose
  unconfirmed; **needs a focused legacy study** before a domain is assigned.
- **m11** — clinic/lab/healthcare billing vertical. This is the **Althea clinic
  domain**, not the ERP product — owned by `apps/web-althea`. **Excluded** from
  web-erp DB design (CLAUDE.md §1: ERP must not absorb the clinic vertical).

## Sequencing (deep field catalogs, after review)

1. ~~**m2 `fin`**~~ ✅ **done** — `entities-m2-finance.md`.
2. ~~**m3 `inv`**~~ ✅ **done** — `entities-m3-inventory.md`.
3. ~~**m4 `pur`**~~ ✅ **done** — `entities-m4-purchasing.md`.
4. ~~**m5 `sls`**~~ ✅ **done** — `entities-m5-sales.md`.
5. ~~**m6 `mfg`**~~ ✅ **done** — `entities-m6-manufacturing.md`.
6. ~~**m7 `fa`**~~ ✅ **done** — `entities-m7-fixed-assets.md`.
7. ~~**m12 `pos`**~~ ✅ **done** — `entities-m12-pos.md`.
8. **m8 `bi`** (last) — dashboards/reporting; config-heavy, confirm DB-vs-app scope.
