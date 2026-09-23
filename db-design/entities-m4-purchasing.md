# Purchasing: Entity Catalog (m4 → `pur`)

> Legacy "Purchasing" (m4) maps to the **`pur_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15): the procure-to-pay document chain is kept as distinct entities (different
> business rules) sharing a documented common shape; `riwayat_*` → `sys_audit_logs`;
> **payment/settlement is NOT re-modeled here** — it reuses the finance domain
> (`fin_ap_payments` / `fin_payment_instruments` / `fin_settlement_allocations`).

Field-level model. Types Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
All entities carry global **audit + soft-delete** ([README §3](README.md#3-global-conventions))
and transaction masters carry **`legacyCode String?`** — omitted per-row.

Legend: 🔑 business key · ➜ FK · ◆ enum · ○ nullable. Money/qty `Decimal(19,4)`;
rate `Decimal(19,6)`; percent `Decimal(9,4)`. `*Fx` = transaction-currency amount.

> **Procure-to-pay chain:** Requisition → RFQ → Quotation → BidSelection →
> Order → GoodsReceipt → Invoice → (Return). Each step links upstream via
> `sourceDocId`/line `sourceLineId` (legacy `id{pr,cs,rq,bs,po,ipc,grn,ri}` +
> `id*detail`). Period FK = `fiscalPeriodId → sys_fiscal_periods`. Dimensions
> reuse m2 `md_*` masters. Partner address blocks (`*1alamat*`/`*2alamat*`) are
> **not** snapshotted — resolved via `md_partner_addresses` at print time.

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `PurchaseDocType` | `REQUISITION`, `RFQ`, `QUOTATION`, `BID_SELECTION`, `ORDER`, `GOODS_RECEIPT`, `INVOICE`, `RETURN` | the m4 chain |
| `PurchaseReturnType` | `DEBIT_NOTE`, `RETURN_TO_VENDOR` | `m4_dnr` vs `m4_prt` |
| `PriceMode` | `TAX_INCLUSIVE`, `TAX_EXCLUSIVE` | `*hargatermasukpajak` |
| `QcStatus` | `PENDING`, `PASSED`, `FAILED`, `PARTIAL` | new — goods-receipt line QC state (resolved §8 #25) |
| `MatchStatus` | `PENDING`, `MATCHED`, `MISMATCH`, `WAIVED` | new — 3-way PO↔GR↔Invoice match (resolved §8 #24) |

Reused: `DocumentStatus`, `PostingStatus` (m2); `SettlementStatus`, `PaymentMethod`
(m2 — for the reused finance payment tables).

---

## Common shapes (defined once, referenced per entity)

### «PurchaseDocHeader» — shared header columns

Every `pur_*` document header carries these (legacy `<p>cabang`/`notransaksi`/…):

| Field | Type | Legacy | Notes |
| --- | --- | --- | --- |
| id | BigInt PK | `*id` | |
| docNumber 🔑 | String unique | `*notransaksi` | |
| autoNumber ○ | String | `*autonotransaksi` | via `sys_document_numberings` |
| branchId ➜ | BigInt → Branch | `*cabang` | |
| locationId ○ ➜ | BigInt → Location | `*lokasi` | |
| warehouseId ○ ➜ | BigInt → Warehouse | `*gudang` | |
| docDate | Date | `*tgl` | |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `*kodepa` | |
| supplierId ○ ➜ | BigInt → Partner | `*supplier`/`*kontak` | |
| supplierContactId ○ ➜ | BigInt → PartnerContact | `*supplierkontak`/`*kontakperson` | |
| paymentTermId ○ ➜ | BigInt → PaymentTerm | `*termin` | |
| dueDate ○ | Date | `*tgljatuhtempo` | |
| currencyId ➜ | BigInt → Currency | `*matauang` | |
| exchangeRate | Decimal(19,6) | `*kurs` | |
| priceMode ◆ | `PriceMode` | `*hargatermasukpajak` | tax in/exclusive |
| subtotal | Decimal(19,4) | `*total` | |
| discountPercent ○ | Decimal(9,4) | `*diskonpersen` | |
| discountAmount ○ | Decimal(19,4) | `*jmldiskon` | |
| tax1Amount ○ | Decimal(19,4) | `*totalpajak1detail` | |
| tax2Amount ○ | Decimal(19,4) | `*totalpajak2detail` | |
| otherCostPercent ○ | Decimal(9,4) | `*biayalainpersen` | |
| otherCostAmount ○ | Decimal(19,4) | `*biayalain` | |
| grandTotal | Decimal(19,4) | `*totaltransaksi` | |
| description ○ | String | `*uraian` | |
| notes ○ | String | `*catatan` | |
| referenceNo ○ | String | `*noref` | |
| referenceDate ○ | Date | `*tglnoref` | |
| closedDate ○ | Date | `*tglpenutupan` | |
| discountAccountId ○ ➜ | BigInt → Account | `*rekdiskon` | |
| tax1AccountId ○ ➜ | BigInt → Account | `*rekpajak1` | |
| tax2AccountId ○ ➜ | BigInt → Account | `*rekpajak2` | |
| otherCostAccountId ○ ➜ | BigInt → Account | `*rekbiayalain` | |
| payableAccountId ○ ➜ | BigInt → Account | `*rekbayar` | |
| sourceDocType ○ | String | `*sumber` | originating module |
| status ◆ | `DocumentStatus` | `*status` | |
| previousStatus ○ ◆ | `DocumentStatus` | `*statussebelumnya` | |
| revisionCount | Int | `*jmlrevisi` | |
| printCount | Int | `*cetakanke` | |
| postingStatus ◆ | `PostingStatus` | `*posting` | |
| postedAt ○ | DateTime | `*postingtgl` | |

Plus **upstream links** (nullable FK to the prior chain doc): `requisitionId`,
`quotationId`, `bidSelectionId`, `orderId`, `goodsReceiptId`, `invoiceId` — only
the ones meaningful per entity (legacy `id{pr,rq,bs,po,grn,ri}`).

### «PurchaseDocLine» — shared line columns

| Field | Type | Legacy | Notes |
| --- | --- | --- | --- |
| id | BigInt PK | `id*detail` | |
| «parent»Id ➜ | BigInt → «parent» | `id<doc>` | |
| itemId ➜ | BigInt → Item | `idbarang` | `namabarang`/`tipebarang` echoes dropped |
| quantity | Decimal(19,4) | `jml` | in `unitId` |
| unitId ➜ | BigInt → Unit | `satuan` | |
| unitValue | Decimal(19,4) | `nilaisatuan` | conversion to base |
| baseQuantity | Decimal(19,4) | `jmlbarang` | |
| baseUnitId ➜ | BigInt → Unit | `satuanbarang` | |
| currencyId ➜ | BigInt → Currency | `matauang` | |
| exchangeRate | Decimal(19,6) | `kurs` | |
| unitPrice | Decimal(19,4) | `harga` | |
| fixedPrice ○ | Decimal(19,4) | `hargafix` | |
| discountPercent ○ | Decimal(9,4) | `diskon` | |
| discountAmount ○ | Decimal(19,4) | `jmldiskon` | |
| tax1Id ○ ➜ | BigInt → Tax | `pajak1` | |
| tax1Amount ○ | Decimal(19,4) | `jmlpajak1` | |
| tax2Id ○ ➜ | BigInt → Tax | `pajak2` | |
| tax2Amount ○ | Decimal(19,4) | `jmlpajak2` | |
| unitCost ○ | Decimal(19,4) | `hpp` | valuation (receipt/invoice/return) |
| warehouseId ○ ➜ | BigInt → Warehouse | `gudang`/`gudangtujuan` | |
| inventoryAccountId ○ ➜ | BigInt → Account | `rekpersediaan` | |
| purchaseDiscountAccountId ○ ➜ | BigInt → Account | `rekdiskonpembelian` | |
| accruedPayableAccountId ○ ➜ | BigInt → Account | `rekhutangsementara` | GRN/RI accrual |
| costCenterId / divisionId / subdivisionId / projectId ○ ➜ | BigInt → `md_*` | `costcenter`/`divisi`/`subdivisi`/`proyek` | dims |
| sourceLineId ○ ➜ | BigInt → (prior line) | `id*detail` chain | traceability |
| notes ○ | String | `catatan` | |
| lineNo | Int | `urutan` | |

> Legacy per-step fulfilment counters (`jmlpo`/`statuspo`, `jmlgrn`/`statusgrn`,
> `jmlri`/`statusri`, …) are **derived** (sum of downstream lines), not stored.

---

## Chain documents (`pur_*`)

Each = «PurchaseDocHeader» + `lines: «PurchaseDocLine»[]`, with the deltas below.

### ErpPurRequisition → `pur_requisitions`  (legacy `m4_pr`)
- Adds: `requestedById ➜ ErpUser` (`prdimintaoleh`), `requestedPartnerId ○ ➜ Partner`
  (`prdimintaolehkontak`), `neededDate ○` (`prtgldipakai`), `requestedTo ○`
  (`prmintake`), `validFrom ○`/`validTo ○` (`prtglawal`/`prtglakhir`),
  `salesQuotationId ○ ➜` (`pridsq` — demand from sales),
  `workOrderId ○ ➜ MfgWorkOrder` (`—` — auto-PR from manufacturing shortage; resolved §8 #26).
  No supplier (pre-sourcing).

### ErpPurRfq → `pur_rfqs`  (legacy `m4_rfq` + `m4_rfq_detail`)
- RFQ lines are **suppliers invited** (legacy `rfq_detail` = `idkontak`), not items:
  child `pur_rfq_suppliers (rfqId ➜, supplierId ➜ Partner, notes ○, lineNo)`.
  Item scope inherited from the linked `requisitionId`. `validFrom/validTo`
  (`rfqtglawal/akhir`).

### ErpPurQuotation → `pur_quotations`  (legacy `m4_rq`)
- Supplier's priced offer. Standard header+lines. `groupNo ○` (`rqnogrup`),
  `fulfilDate ○` (`rqtgldipenuhi`).

### ErpPurBidSelection → `pur_bid_selections`  (legacy `m4_bs` + `m4_bs_detail`)
- Compares quotations and picks winners. Header refs up to N quotations
  (`bsidrq1..5` → child `pur_bid_selection_quotations(bidSelectionId ➜,
  quotationId ➜, rank Int)`). Lines (`bs_detail`): `quotationLineId ➜
  ErpPurQuotationLine` (`idrqdetail`), `selected Boolean` (`terpilih`),
  `priceRank Int` (`hargake`), `notes`, `lineNo`. No own pricing.

### ErpPurOrder → `pur_orders`  (legacy `m4_po`)
- The PO. Standard header+lines. `fulfilDate ○` (`potgldipenuhi`).
  Upstream: `requisitionId`/`quotationId`/`bidSelectionId`.

### ErpPurGoodsReceipt → `pur_goods_receipts`  (legacy `m4_grn`)
- Goods received vs PO. Lines carry `unitCost` + `accruedPayableAccountId`
  (`rekhutangsementara`). Posting moves stock (`inv`) + accrues GR/IR.
  Upstream: `orderId`.
- **QC delta on lines (resolved §8 #25):** `acceptedQty Decimal(19,4)` (masuk stok +
  jadi dasar hutang), `rejectedQty Decimal(19,4)` (calon retur ke supplier),
  `quarantineQty Decimal(19,4)` (ditahan, belum diklasifikasi),
  `qcStatus ◆ QcStatus` (`PENDING` saat baris dibuat; `PASSED`/`FAILED`/`PARTIAL`
  setelah QC selesai). Stok hanya naik sebesar `acceptedQty`. `rejectedQty` otomatis
  menjadi seed baris `pur_returns` bila diproses.

### ErpPurInvoice → `pur_invoices`  (legacy `m4_ri`; price-change `m4_ipc` folded)
- Supplier bill = **AP open item**. Adds: `taxInvoiceNo ○` (`rinofakturpajak`),
  `taxPaid Boolean` (`risdhbayarpajak`), `taxPaidDate ○` (`ritglbayarpajak`),
  `settlementStatus ◆` (`ristatuslunas`), `settledDate ○` (`ritgllunas`),
  `advanceAmount ○` (`rijmluangmuka`), `advanceAccountId ○ ➜` (`rirekuangmuka`),
  `isOpeningBalance Boolean` (`risaldoawal`),
  `matchStatus ◆ MatchStatus` (`—` — 3-way match PO↔GR↔Invoice; `PENDING` on creation;
  AP posting blocked until `MATCHED` or explicitly `WAIVED`; resolved §8 #24).
  Upstream: `orderId`/`goodsReceiptId`.
  > `m4_ipc` (purchase price change) is **folded** as an invoice with
  > `documentType` distinction at catalog-build time — not a separate table
  > (core-subset; legacy IPC duplicates the RI shape).

### ErpPurReturn → `pur_returns`  (legacy `m4_dnr` + `m4_prt`)
- Purchase return / debit note, unified via `returnType ◆ PurchaseReturnType`
  (`DEBIT_NOTE`=dnr, `RETURN_TO_VENDOR`=prt). Adds: `taxInvoiceNo ○`,
  `settlementStatus ◆`, `settledDate ○`, `returnPurchaseAccountId ○ ➜`
  (`rekreturpembelian`), `cogsAccountId ○ ➜` (`rekhargapokok`),
  `isOpeningBalance Boolean` (`prtsaldoawal`). Upstream:
  `orderId`/`goodsReceiptId`/`invoiceId`.

---

## Payment & settlement — **reuses the finance domain**

Legacy `m4_ap`, `m4_pp`, `m4_vp`, `m4_vpp` (+ `*_pay`, `*_detail`) are supplier
payables/payments. A fresh design has **one** AP-payment concept, so these map to
the already-cataloged finance entities — **no `pur_payment*` tables**:

| Legacy m4 | Modern target (m2 `fin`) | Notes |
| --- | --- | --- |
| `m4_ap` / `m4_pp` (payable voucher) | the `pur_invoices` AP open item + `fin_ledger_entries` | invoice *is* the payable; `apidpo`/`ppidri` = upstream links |
| `m4_ap_pay` / `m4_pp_pay` | `fin_payment_instruments` | tender lines (cash/giro/transfer) |
| `m4_vp` / `m4_vpp` (vendor payment + plan) | `fin_ap_payments` | multi-invoice payment; `vpp` (plan) = a `fin_ap_payments` in `DRAFT` |
| `m4_vp_detail` / `m4_vpp_detail` | `fin_settlement_allocations` | per-invoice allocation; `rekhutangpiutang` |
| `vp*selisihkurs`/`rekselisihkurs` | **add to `fin_ap_payments`**: `fxGainLossAmount ○`, `fxGainLossAccountId ○ ➜` | realized FX diff on settlement |
| `vp*diskontermin`/`rekdiskontermin` | **add to `fin_ap_payments`**: `termDiscountAmount ○`, `termDiscountAccountId ○ ➜` | early-payment term discount |

> **Follow-up flagged:** `fin_ap_payments` (entities-m2-finance.md) gains 4 optional
> columns — `fxGainLossAmount`, `fxGainLossAccountId`, `termDiscountAmount`,
> `termDiscountAccountId` — to absorb VP/VPP. To be folded into a finance-doc
> revision; recorded here, **not** silently edited into m2.

---

## Approval workflow integration (resolved §8 #27)

Purchasing documents integrate with the **generic approval engine** (`sys_approval_*`,
domain `sys` — designed as a separate cross-module system). No `pur_approval*` tables.

| Doc | Trigger | Effect on status |
| --- | --- | --- |
| `pur_requisitions` | submit for approval | `status` → `PENDING_APPROVAL`; advance to `APPROVED` / `REJECTED` by engine |
| `pur_orders` | submit for approval | same; `APPROVED` = PO can be sent to supplier |

Approval configuration (who approves, amount thresholds, multi-level chain) lives
entirely in `sys_approval_*`. The `pur_*` docs only carry the resulting `status` value.
Design of `sys_approval_*` is a **separate deliverable** (domain `sys`).

---

### Flagged / secondary (not modeled in core)
- **`m4_pie`** (`pie_detail` = bare `sumber`/`idtransaksi` pointers) — a generic
  invoice-entry aggregator. Superseded by explicit chain links; **not modeled**.
- **`m4_ipc`** — folded into `pur_invoices` (see above).
- Legacy address snapshots, `*_realisasi` budget counters → not stored
  (resolve via `md_partner_addresses`; realization is `fin_budget_realizations`).

---

**Count:** 8 Purchasing (`pur_*`) chain entities + their line tables
(Requisition, Rfq[+suppliers], Quotation, BidSelection[+quotations], Order,
GoodsReceipt, Invoice, Return) ≈ **18 tables**. Payment/settlement **reuses
`fin_*`** (no new tables; +4 optional cols flagged onto `fin_ap_payments`).
Period reuses `sys_fiscal_periods`; dimensions reuse `md_*`. Approval engine
reuses `sys_approval_*` (no new `pur_*` tables). Enterprise additions resolved
§8 #24–#27 are **column-level only** — no new tables.

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
