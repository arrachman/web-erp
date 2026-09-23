# Sales / Accounts Receivable: Entity Catalog (m5 → `sls`)

> Legacy "Sales" (m5) maps to the **`sls_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15). m5 is the **mirror of m4** (order-to-cash ↔ procure-to-pay): the same
> document machinery with `customer` instead of `supplier`. Legacy ~81 tables, but
> ~40 are `riwayat_*` shadow-history (→ `sys_audit_logs`) and many are `*_detail`.

Field-level model. Types Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
Global **audit + soft-delete** ([README §3](README.md#3-global-conventions)) and
`legacyCode String?` on transaction masters — omitted per-row. Money/qty
`Decimal(19,4)`, rate `Decimal(19,6)`, percent `Decimal(9,4)`; `*Fx` =
transaction-currency amount.

> **Order-to-cash chain:** Quotation → Order → ProformaInvoice → PackingList →
> DeliveryOrder → DeliveryReport → Invoice → (Return → ReturnReceipt). Upstream
> links via `sourceDocId`/`sourceLineId` (legacy `id{sq,so,pi,pl,do,dr,si,…}`).
> Period FK = `fiscalPeriodId → sys_fiscal_periods`; dimensions reuse m2 `md_*`;
> partner address blocks (`*1alamat*`/`*2alamat*`) resolved via
> `md_partner_addresses` (not snapshotted).

---

## Common shapes — reuse the m4 definitions

«SalesDocHeader» and «SalesDocLine» are **structurally identical** to
[«PurchaseDocHeader»/«PurchaseDocLine»](entities-m4-purchasing.md#common-shapes-defined-once-referenced-per-entity)
with these substitutions — defined once, not repeated per entity:

| Purchase shape | Sales shape | Legacy |
| --- | --- | --- |
| `supplierId → Partner` | `customerId → Partner` | `*customer` |
| `supplierContactId` | `customerContactId` | `*customerkontak` |
| `payableAccountId` (`rekbayar`) | `receivableAccountId` (`rekbayar`) | AR control |
| — | `expeditionId ○` (`*ekspedisi`), `shipDate ○` (`*tglkirim`) | shipping |
| — | `salesDeptId ○` (`*bagianpenjualan`) | sales unit |

All other «…Header»/«…Line» columns (docNumber, branch, currency, priceMode,
subtotal/discount/tax/otherCost/grandTotal, GL accounts, status/posting, upstream
links, dims, line item/qty/price/tax) carry over **unchanged**.

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `SalesDocType` | `QUOTATION`, `ORDER`, `PROFORMA_INVOICE`, `PACKING_LIST`, `DELIVERY_ORDER`, `DELIVERY_REPORT`, `INVOICE`, `RETURN`, `RETURN_RECEIPT` | the m5 chain |

Reused: `DocumentStatus`, `PostingStatus`, `PriceMode`, `SettlementStatus`,
`PaymentMethod`.

---

## Chain documents (`sls_*`)

Each = «SalesDocHeader» + `lines: «SalesDocLine»[]`, with the deltas below.

### ErpSlsQuotation → `sls_quotations`  (legacy `m5_sq`)
- Mirror of `pur_quotations`/`pur_requisitions`. `purchaseRequisitionId ○ ➜`
  (`sqidpr` — quote raised from a buyer RFP). `material out` lines
  (`m5_sq_out_bahan`) → child `sls_quotation_materials` (lean: item, qty,
  costPrice, salePrice, lineNo).

### ErpSlsOrder → `sls_orders`  (legacy `m5_so`)
- The sales order (customer commitment). Standard. Upstream: `quotationId`
  (`soidsq`).

### ErpSlsProformaInvoice → `sls_proforma_invoices`  (legacy `m5_pi`)
- Pre-invoice. Standard. Upstream: `quotationId`/`orderId`.

### ErpSlsPackingList → `sls_packing_lists`  (legacy `m5_pl`)
- Pick/pack doc. `packingDeptId ○` (`plbagianpengepakan`). Optional pack
  grouping (`m5_pl_pack`) → child `sls_packing_list_packs(packingListId ➜,
  packNo, notes, lineNo)`. Upstream: `quotationId`/`orderId`/`proformaInvoiceId`.

### ErpSlsDeliveryOrder → `sls_delivery_orders`  (legacy `m5_do`)
- Surat jalan. `shippingDeptId ○` (`dobagianpengiriman`). Upstream:
  `…/packingListId`.

### ErpSlsDeliveryReport → `sls_delivery_reports`  (legacy `m5_dr`)
- Delivery confirmation/result. Upstream: `…/deliveryOrderId` (`driddo`).

### ErpSlsInvoice → `sls_invoices`  (legacy `m5_si`)
- Sales invoice = **AR open item**. Adds (mirror of `pur_invoices`):
  `taxInvoiceNo ○` (`sinofakturpajak`), `taxPaid Boolean` (`sisdhbayarpajak`),
  `taxPaidDate ○`, `settlementStatus ◆` (`sistatuslunas`), `settledDate ○`,
  `advanceAmount ○` (`sijmluangmuka`), `advanceAccountId ○ ➜` (`sirekuangmuka`),
  `isOpeningBalance Boolean` (`sisaldoawal`), `swapStatus ○ ◆`/`swapDate ○`
  (`sistatussie`/`sitglsie`). Upstream:
  `quotationId`/`orderId`/`advanceId`/`proformaInvoiceId`/`deliveryOrderId`.
  > **POS tender + loyalty fields** (`sibayartunai/kkredit/kdebit/voucher/poin`,
  > `sicharge*`, `sijmlkembali`, `sipoin*`) are **NOT** core-invoice columns —
  > tender → `fin_payment_instruments`; loyalty points → **flagged for m12 `pos`**
  > (`sls_invoices` keeps only `pointsEarned ○`/`pointsRedeemed ○` placeholders).

### ErpSlsReturn → `sls_returns`  (legacy `m5_sr`)
- Sales return / credit note. Mirror of `pur_returns`. Adds `settlementStatus ◆`,
  `remainingAccountId ○ ➜` (`srreksisa`), `isOpeningBalance Boolean`,
  `swapStatus ○`/`swapDate ○`. Upstream: chain + `invoiceId` (`sridsi`).

### ErpSlsReturnReceipt → `sls_return_receipts`  (legacy `m5_rnr`)
- Goods physically received back from customer (stock-in for a return). Standard
  + `taxInvoiceNo ○`, `settlementStatus ◆`. Upstream:
  `…/invoiceId`/`returnId` (`rnridsi`). Posting moves `inv` stock in.

---

## Invoice satellites (`sls_*`)

### ErpSlsInvoiceInstallment → `sls_invoice_installments`  (legacy `m5_si_installment`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| invoiceId ➜ | BigInt → ErpSlsInvoice | `idsi` |
| installmentNo | Int | `angsuranke` |
| currencyId ➜ | BigInt → Currency | `matauang` |
| exchangeRate | Decimal(19,6) | `kurs` |
| amount | Decimal(19,4) | `jumlah` |
| paidAmount | Decimal(19,4) | `jumlahbayar` |
| dueDate | Date | `tgljt` |
| settledDate ○ | Date | `tgllunas` |
| settlementStatus ◆ | `SettlementStatus` | `statuslunas` |
| receivableAccountId ➜ | BigInt → Account | `rekpiutang` |
| notes ○ | String | `catatan` |
| lineNo | Int | `urutan` |

### ErpSlsInvoiceMaterial → `sls_invoice_materials`  (legacy `m5_si_material`)

Component/material consumption on an invoice line (assembly/service jobs).

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| invoiceId ➜ | BigInt → ErpSlsInvoice | `idsi` |
| invoiceLineId ➜ | BigInt → ErpSlsInvoiceLine | `idsidetail` |
| itemId ➜ | BigInt → Item | `idbarang` (echoes dropped) |
| quantity | Decimal(19,4) | `jml` |
| baseQuantity | Decimal(19,4) | `jmlbarang` |
| unitId ➜ | BigInt → Unit | `satuan` |
| sourceWarehouseId ○ ➜ | BigInt → Warehouse | `gudangasal` |
| transitWarehouseId ○ ➜ | BigInt → Warehouse | `gudangtransit` |
| destinationWarehouseId ○ ➜ | BigInt → Warehouse | `gudangtujuan` |
| costCenterId / divisionId / projectId ○ ➜ | BigInt → `md_*` | dims |
| notes ○ | String | `catatan` |

### ErpSlsInvoiceCost → `sls_invoice_costs`  (legacy `m5_si_cost`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| invoiceId ➜ | BigInt → ErpSlsInvoice | `idsi` |
| partnerId ○ ➜ | BigInt → Partner | `kontak` (cost bearer/3rd party) |
| amount | Decimal(19,4) | `jumlah` |
| metadata ○ | Json | `customdbl1/2` |
| lineNo | Int | `urutan` |

---

## Standalone sales docs (`sls_*`)

### ErpSlsCustomerAdvance → `sls_customer_advances`  (legacy `m5_as` + `m5_as_pay`)
- Customer down-payment/advance (mirror of nothing in `pur` — purchasing handled
  advances inline). «AdvanceHeader»: docNumber🔑, branch, customer, fiscalPeriod,
  paymentTerm, currency/rate, `amount`/`amountFx`, `appliedAmount`,
  `settlementStatus ◆`, `orderId ○ ➜` (`asidso`), dims, status/posting.
  Tender lines (`m5_as_pay`) → **reuse `fin_payment_instruments`**
  (`advanceId` discriminator). Applied to invoices via
  `fin_settlement_allocations` / `sls_invoices.advanceAmount`.

### ErpSlsInvoiceSwap → `sls_invoice_swaps`  (legacy `m5_sie`)
- "Tukar faktur" — replace/exchange an issued invoice. Lean header: docNumber🔑,
  branch, customer (`siekontak`), fiscalPeriod, currency/rate, description/notes,
  reference, status/posting. Lines link source/target invoices:
  `sls_invoice_swap_lines(swapId ➜, fromInvoiceId ➜, toInvoiceId ○ ➜, amount,
  lineNo)`.

### ErpSlsForecast → `sls_forecasts`  (legacy `m5_sf`)
- Lean: `docNumber 🔑` (`sfnotransaksi`), `forecastDate` (`sftgl`),
  `customerId ➜` (`sfcustomer`), `salesDeptId ○` (`sfbagianpenjualan`),
  `currencyId ➜` (`sfmatauang`), `description ○` (`sfuraian`),
  `status ◆` (`sfstatus`). Optional `lines` (item, qty, period) — minimal.

---

## Payment & settlement — **reuses the finance domain**

Mirror of m4: legacy `m5_ip` (receipt), `m5_pv` (payment voucher), `m5_ic`
(invoice collection), `m5_rp` (freight receivable) map to the cataloged finance
AR entities — **no `sls_payment*` tables**:

| Legacy m5 | Modern target (m2 `fin`) | Notes |
| --- | --- | --- |
| `m5_si` AR open item | `sls_invoices` + `fin_ledger_entries` | invoice *is* the receivable |
| `m5_ip` / `m5_ip_pay` (customer receipt) | `fin_ar_receipts` / `fin_payment_instruments` | `ipidso` upstream link |
| `m5_pv` (payment voucher, FX/term-disc) | `fin_ar_receipts` | multi-invoice; `pv*selisihkurs`/`diskontermin` |
| `m5_ic` (invoice collection / billing run) | `fin_ar_receipts` (collection batch) + `fin_settlement_allocations` | `ic*selisihkurs`/`diskontermin` |
| `m5_pv_*`/`m5_ic_*`/`m5_ip` allocations | `fin_settlement_allocations` | per-invoice allocation |
| `m5_rp` / `m5_rp_pay` (freight receivable) | misc AR via `fin_ledger_entries` + `fin_ar_receipts` | `rpidsi` link; **secondary** |

> **Follow-up flagged (extends the m4 note):** the +4 optional columns on
> `fin_ar_receipts` mirror those flagged on `fin_ap_payments` —
> `fxGainLossAmount ○`, `fxGainLossAccountId ○ ➜`, `termDiscountAmount ○`,
> `termDiscountAccountId ○ ➜` (legacy `*selisihkurs`/`*rekselisihkurs`/
> `*diskontermin`/`*rekdiskontermin`). To be folded into a finance-doc revision;
> recorded here, **not** silently edited into m2.

### Pricing — **reuses the `pos` domain** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17) #27)
Sales pricing/discount is **not** modeled in `sls`. Single source of truth =
`pos_contact_prices` + `pos_category_discounts` ([entities-m12-pos.md](entities-m12-pos.md)).
`sls` documents are **consumers**: line `price`/discount is resolved against the
`pos` agreement at entry time and persisted on the line (no `sls` price-list/tier
table). Consistent with §8 #10 (tiered pricing → `pos` phase). This prevents two
SSOTs for price.

### Enterprise scope reviewed & deferred (resolved [README §8](README.md#8-resolved-decisions-2026-05-17) #28)
Reviewed 2026-05-18; **not in core m5** (revisit per real need, not silently):
- **Credit management** — credit-limit/exposure/hold. When added: attribute on
  `md_partners` + a hold *status* on `sls_orders`, **not** a new table.
- **Sales commission** — would be new `sls_commission_rules`/`sls_commission_entries`
  (no legacy source). Deferred.
- **Target / quota vs actual** — would extend `sls_forecasts` to target-vs-realisasi.
  Deferred.
- **Blanket / contract (call-off) order** — would be `orderType` enum + release
  lines on `sls_orders`, **not** a separate table. Deferred.
- **CRM / opportunity pipeline** — **out of ERP scope**; the chain starts at
  Quotation. Pre-quotation funnel belongs to a separate CRM, not `sls`.

### Flagged / secondary / deferred (not modeled in core)
- **`m5_cl`** (penutupan penjualan / sales closing per item) — specialized
  period-close-per-item doc; **not modeled** (superseded by invoice + fiscal
  period close).
- **`m5_spa`** (sales point adjustment) + SI loyalty/point fields — **deferred to
  m12 `pos`** (loyalty/promotions domain).
- **`m5_rp`** (freight receivable) — secondary; treat as misc AR via `fin`.
- `m5_files` / `m5_notes` (attachments/notes), `m5_si_failed` (failed staging),
  `m5_pl_pack` minimal — generic/app concerns; not modeled here.
- Per-step fulfilment counters (`jml*`/`status*`) **derived**, not stored.

---

**Count:** 9 Sales (`sls_*`) chain entities + line tables (Quotation, Order,
ProformaInvoice, PackingList, DeliveryOrder, DeliveryReport, Invoice, Return,
ReturnReceipt) ≈ **18 tables**; + 3 invoice satellites
(Installment/Material/Cost); + 3 standalone (CustomerAdvance, InvoiceSwap,
Forecast) ≈ **~24 tables**. Payment/settlement **reuses `fin_*`** (no new tables;
+4 optional cols flagged onto `fin_ar_receipts`). Period reuses
`sys_fiscal_periods`; dimensions reuse `md_*`. CL/SPA/RP flagged out.

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
