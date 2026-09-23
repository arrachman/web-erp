# POS / Retail & Promotions: Entity Catalog (m12 → `pos`)

> Legacy "POS / Retail" (m12) maps to the **`pos_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15).
>
> ⚠ **Lower-fidelity source.** m12 is **not** in `semantic-schema.json` (m0–m5) and
> **not** in `myerpplus_serenity.sql` (the seed dump excludes m12). This catalog is
> derived from the backend VB data-access classes
> (`preferensi/Backened - myerpplus/app_code/ws/m12/*.vb`) + the m12 Flex screens.
> Field lists are **best-effort inferred** (prefix tokens `pi*`/`cpa*`/`si*`/…),
> not column-exact — to be **re-verified against a live m12 schema** before Prisma.

Field-level model. Types Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
Global **audit + soft-delete** ([README §3](README.md#3-global-conventions)) and
`legacyCode String?` on masters — omitted per-row. `*custom{text,int,dbl,date}*`
→ `metadata Json?`. Money/qty `Decimal(19,4)`, percent `Decimal(9,4)`.

> **Resolves a deferred decision:** the legacy **tiered pricing** (`bhargajual1..10`
> / `bdiskonjual1..10`, `m1_contact_price`, `m1_price_category`) deferred in
> [README §8 #3 / §10](README.md#8-resolved-decisions-2026-05-17) **lands here** as
> `pos_item_prices`(+tiers) and `pos_price_agreements`. Cross-referenced, **not**
> silently merged into m1 — flag for an m1/§8 revision.

---

## Area & POS config (`pos_*`)

### ErpPosAreaCategory → `pos_area_categories`  (legacy `m12_area_category`)
`id`, `code 🔑` (`ackode`), `name` (`acnama`), `isActive` (`acaktif`),
`notes ○` (`accatatan`), `metadata ○` (`accustom*`).

### ErpPosArea → `pos_areas`  (legacy `m12_area`)
`id`, `code 🔑`, `name` (`acnama`), `categoryId ○ ➜ ErpPosAreaCategory`,
`branchId ○ ➜ Branch`, `notes ○` (`acatatan`), `isActive`, `metadata ○`.
Sales territory; referenced by `sls_invoices.customArea` (POS sale).

### ErpPosTerminal → `pos_terminals`  (legacy `m12_pos_hardware`)
`id`, `code 🔑`, `name`, `branchId ➜`, `locationId ○ ➜`, `warehouseId ○ ➜`,
`hardwareInfo ○ Json`, `isActive`. Registered POS station.

### ErpPosTransactionType → `pos_transaction_types`  (legacy `m12_pos_type` + `m12_pos_category`/`_setting`)
`id`, `code 🔑`, `name`, `settings ○ Json`, `isActive`. POS document/category
type config. **Global POS settings reuse `sys_settings`** (module=`pos`) — no
separate `pos_settings` table for scalar config (`m12_pos_setting`).

---

## Pricing (`pos_*`) — realizes deferred tiered pricing

### ErpPosItemPrice → `pos_item_prices`  (legacy `m12_pos_item`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| itemId ➜ | BigInt → Item | `piidbarang` |
| areaId ○ ➜ | BigInt → ErpPosArea | area-scoped price (null = global) |
| categoryId ○ ➜ | BigInt → ItemCategory | `pikategori`/`pikategoribarang` |
| priceIsEditable | Boolean | `pihargaedited` (cashier may override) |
| minStock / maxStock ○ | Decimal(19,4) | `pistokminimal`/`pistokmaksimal` |
| reorderStock / minOrderStock ○ | Decimal(19,4) | `pistokreorder`/`pistokminorder` |
| isActive | Boolean | |
| metadata ○ | Json | `picustom*` |

Child **ErpPosItemPriceTier → `pos_item_price_tiers`** (legacy `pihargajual1..5`
+ `pidiskonjual1..5`): `id`, `itemPriceId ➜`, `tierLevel Int` (1–N),
`price Decimal(19,4)`, `discountPercent ○ Decimal(9,4)`, `minQty ○`. Replaces
the 10 flat tier columns with rows (extensible beyond 5/10).

### ErpPosPriceAgreement → `pos_price_agreements` (+ lines)  (legacy `m12_cpa`)

Per-customer/area negotiated price agreement document (+ that customer's loyalty
point balance roll).

| Field (header) | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | `cpaid` |
| docNumber 🔑 | String unique | `cpanotransaksi` |
| autoNumber ○ | String | `cpaautonotransaksi` |
| branchId ➜ | BigInt → Branch | `cpacabang` |
| locationId ○ ➜ | BigInt → Location | `cpalokasi` |
| partnerId ➜ | BigInt → Partner | `cpakontak` |
| partnerContactId ○ ➜ | BigInt → PartnerContact | `cpakontakperson` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `cpakodepa` |
| agreementDate | Date | `cpatgl` |
| pointsBefore / pointsIn / pointsOut / pointsAfter ○ | Decimal(19,4) | `poinlama`/`poinmasuk`/`poinkeluar`/`poinbaru` |
| description ○ | String | `cpauraian` |
| notes ○ | String | `cpacatatan` |
| status ◆ | `DocumentStatus` | `cpastatus` |
| previousStatus ○ ◆ | `DocumentStatus` | `cpastatussebelumnya` |
| postingStatus ◆ | `PostingStatus` | `cpaposting` |
| postedAt ○ | DateTime | `cpapostingtgl` |
| metadata ○ | Json | `cpacustom*` |

Line `pos_price_agreement_lines`: `agreementId ➜`, `itemId ➜`,
`agreedPrice Decimal(19,4)`, `discountPercent ○`, `validFrom ○`/`validTo ○`,
`lineNo`.

---

## Promotions (`pos_*`)

### ErpPosPromotion → `pos_promotions`  (legacy `m12_pos_promo`)
Promotion header: `id`, `code 🔑` (`nopromo`), `name`, `promotionType ◆`
`PromotionType`, `validFrom`/`validTo` (`tgl1`/`tgl2`), `areaId ○ ➜`,
`isActive`, `metadata ○`. Rules attach via the children below.

### ErpPosBonusRule → `pos_bonus_rules` (+ `pos_bonus_transactions`)  (legacy `m12_pos_bonus_item` / `_bonus_trans`)
Buy-X-get-Y: `promotionId ○ ➜`, `triggerItemId ➜ Item`, `triggerQty`
(`jml1`), `bonusItemId ➜ Item`, `bonusQty` (`jml2`), `categoryId ○`,
`validFrom`/`validTo`. `pos_bonus_transactions` = applied-bonus log per sale
(`saleInvoiceId ➜ ErpSlsInvoice`, rule, qty).

### ErpPosSubstitutionRule → `pos_substitution_rules`  (legacy `m12_pos_substitution_item`)
`promotionId ○ ➜`, `fromItemId ➜` (`siidbarang`), `toQty1`/`toQty2`
(`sijml1`/`sijml2`), `categoryId ○` (`sikategori`), `promoNo ○` (`sinopromo`),
`validFrom`/`validTo` (`sitgl1`/`sitgl2`), `metadata ○` (`sicustom*`).

### ErpPosAdditionalItemRule → `pos_additional_item_rules`  (legacy `m12_pos_additional_item`)
Add-on/upsell item suggestions per item/category. Lean: `promotionId ○ ➜`,
`baseItemId ➜`, `additionalItemId ➜`, `qty`, `validFrom`/`validTo`.

### ErpPosDiscountRule → `pos_discount_rules`  (legacy `m12_pos_discount_item` + `_discount_category_item` + `_discount_category_customer`)
Unified discount matrix via `scope ◆ DiscountScope`:

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| promotionId ○ ➜ | BigInt → ErpPosPromotion | |
| scope ◆ | `DiscountScope` | ITEM / ITEM_CATEGORY / CUSTOMER_CATEGORY |
| itemId ○ ➜ | BigInt → Item | scope=ITEM |
| itemCategoryId ○ ➜ | BigInt → ItemCategory | scope=ITEM_CATEGORY |
| partnerCategoryId ○ ➜ | BigInt → PartnerCategory | scope=CUSTOMER_CATEGORY |
| discountPercent | Decimal(9,4) | |
| minQty ○ / minAmount ○ | Decimal(19,4) | threshold |
| validFrom ○ / validTo ○ | Date | |

---

## Loyalty points (`pos_*`)

### ErpPosPointRule → `pos_point_rules`  (legacy `m12_pos_point_item` + `_point_category_item`)
Point-earn rule: `id`, `scope ◆ DiscountScope` (ITEM / ITEM_CATEGORY),
`itemId ○ ➜`, `itemCategoryId ○ ➜` (`pikategori`), `qtyFrom`/`qtyTo`
(`pijml1`/`pijml2`), `pointsAwarded Decimal(19,4)` (`pijmlpoint`),
`validFrom`/`validTo` (`pitgl1`/`pitgl2`), `metadata ○`.

### ErpPosPointTransaction → `pos_point_transactions`  (legacy `m12_pos_point_transaction`)
Point ledger (append-only): `id`, `partnerId ➜ Partner`,
`type ◆ PointTransactionType` (EARN/REDEEM/ADJUST), `points Decimal(19,4)`
(`ptjmlpoint`), `saleInvoiceId ○ ➜ ErpSlsInvoice`, `ruleId ○ ➜ ErpPosPointRule`,
`transactionDate` (`pttgl1`), `categoryId ○` (`ptkategori`),
`balanceAfter ○ Decimal(19,4)`, `metadata ○`. Customer balance is the running
sum (the CPA `poin*` roll snapshots it per agreement).

### ErpPosVoucher → `pos_vouchers`  (legacy `m12_pos_voucher`)
`id`, `code 🔑` (voucher no.), `name ○`, `faceValue Decimal(19,4)`,
`status ◆ VoucherStatus` (ISSUED/REDEEMED/EXPIRED/VOID), `issuedToPartnerId ○ ➜`,
`issuedDate ○`, `expiryDate ○`, `redeemedSaleInvoiceId ○ ➜ ErpSlsInvoice`
(`sinotransaksi`), `redeemedDate ○`, `metadata ○`.

---

## POS sale — **reuses the sales domain**

Legacy `m12_si` (+ `_history`) is the **POS sales invoice** — structurally
`m5_si` plus retail tender (`sibayartunai`/`kkredit`/`kdebit`/`voucher`/`poin`,
`sicharge*`, `sijmlkembali`) and loyalty (`sipoindidapat`/`sipoinsebelumnya`).
Per the m5 decision, this is **not** a new table:

| Legacy m12 | Modern target | Notes |
| --- | --- | --- |
| `m12_si` (+`_history`) | **`sls_invoices`** with `channel = POS` | the m5 invoice; history → `sys_audit_logs` |
| `sibayartunai/kkredit/kdebit/voucher/poin`, `sicharge` | `fin_payment_instruments` (+ `serviceChargeAmount ○` flagged onto `sls_invoices`) | retail tender split |
| `sibayarvoucher` | `pos_vouchers.redeemedSaleInvoiceId` | voucher redemption |
| `sipoindidapat`/`sipoinsebelumnya` | `pos_point_transactions` (EARN) | loyalty earn on the sale |
| `sicustomarea` | `sls_invoices` → `ErpPosArea` (`posAreaId ○`) | sale area tag |

> **Follow-up flagged:** `sls_invoices` gains `channel ◆ SalesChannel`
> (`STANDARD`/`POS`), `posAreaId ○ ➜ ErpPosArea`, `serviceChargeAmount ○`,
> `serviceChargeAccountId ○ ➜`. Recorded here; folded into an `sls`/finance-doc
> revision, **not** silently edited into m5.

### Flagged / secondary (not modeled in core)
- `m12_st` (POS stock transfer between outlets) → reuse `inv_stock_movements`
  (`movementType = TRANSFER`); no `pos` table.
- `m12_ppa` / `m12_ppv` / `m12_sbi` / `m12_ai` / `m12_bi` / `m12_di` /
  `m12_lp` / `m12_dataPenjualan` — **report/screen views**, not master/transaction
  tables; not modeled.
- `m12_upload` / `m12_getValueMember` / `m12_item` / `m12_contact` — utility /
  lookup shims over `md_items`/`md_partners`; not modeled (reuse `md_*`).
- All `*_history` → `sys_audit_logs`.

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `PromotionType` | `BONUS`, `SUBSTITUTION`, `ADDITIONAL_ITEM`, `DISCOUNT`, `VOUCHER` | m12 promo family |
| `DiscountScope` | `ITEM`, `ITEM_CATEGORY`, `CUSTOMER_CATEGORY` | m12 discount/point matrices |
| `PointTransactionType` | `EARN`, `REDEEM`, `ADJUST` | `m12_pos_point_transaction` |
| `VoucherStatus` | `ISSUED`, `REDEEMED`, `EXPIRED`, `VOID` | `m12_pos_voucher` |
| `SalesChannel` *(flagged onto `sls_invoices`)* | `STANDARD`, `POS` | `m12_si` vs `m5_si` |

Reused: `DocumentStatus`, `PostingStatus`.

---

**Count:** ~15 POS (`pos_*`) entities — Area(+Category), Terminal,
TransactionType, ItemPrice(+Tiers), PriceAgreement(+lines), Promotion +
Bonus/Substitution/AdditionalItem/Discount rules (+BonusTxn), PointRule,
PointTransaction, Voucher ≈ **~18 tables**. POS sale **reuses `sls_invoices`**
(channel=POS; +4 cols flagged); POS config reuses `sys_settings`; stock transfer
reuses `inv`. Resolves deferred tiered pricing (§8 #3) here. Period reuses
`sys_fiscal_periods`; dimensions reuse `md_*`. **Fidelity: inferred from VB/Flex
— verify vs live m12 schema before Prisma.**

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
