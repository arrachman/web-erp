# DB-Design Clarity Audit — 2026-05-17

> Working artifact (NOT part of the authoritative `db-design/` set). Review-only.
> Scope: usability of the design docs for a business/non-DBA reader.
> Method: 3 parallel reviewers across all 11 entity/roadmap docs + README §4.
> Status: findings only — no doc edits applied (user chose "laporan saja dulu").

## Verdict matrix

| Doc | Understandable to business user | Enum values explained | Flow clarity |
| --- | --- | --- | --- |
| entities-m0-administrator | GOOD | **WEAK** | GOOD |
| entities-m1-master-data | GOOD | **WEAK** | GOOD |
| entities-m2-finance | NEEDS WORK | **WEAK** | NEEDS WORK |
| entities-m3-inventory | GOOD | **WEAK** | GOOD |
| entities-m4-purchasing | NEEDS WORK | **WEAK** | NEEDS WORK |
| entities-m5-sales | GOOD | **WEAK** | GOOD |
| entities-m6-manufacturing | GOOD | **WEAK** | GOOD |
| entities-m7-fixed-assets | GOOD | **WEAK** | GOOD |
| entities-m12-pos | WEAK | **WEAK** | WEAK |
| module-roadmap | GOOD | n/a | GOOD |

## Finding 1 — Enums have NO per-value meaning anywhere (universal, highest impact)

`README.md §4` (lines 84–137) lists only enum **value names + legacy source**.
No per-module doc adds a plain-language gloss. A reader cannot tell what a value
*means* or *when* it applies.

Concrete instances:
- `README.md:88` `UserLevel` POS/CENTRAL/BI… — no meaning (legacy `ulevel 0–4`).
- `entities-m1-master-data.md:110` `ItemType` — **values not even listed**, only legacy `bjenis`/`btipe`.
- `entities-m1-master-data.md:177` `AddressType` — OFFICE vs OTHER not differentiated.
- m2 `DocumentStatus` DRAFT/POSTED/VOID/CANCELLED — VOID = reversed? CANCELLED editable? unstated.
- m2 `PaymentMethod` GIRO — Indonesian term, no gloss for non-ID readers.
- m2 `GiroStatus` OUTSTANDING→CLEARED/BOUNCED/CANCELLED — no transition rules.
- m4 `PurchaseReturnType` DEBIT_NOTE vs RETURN_TO_VENDOR — difference unexplained.
- m4 `PriceMode` TAX_INCLUSIVE/EXCLUSIVE — assumes invoicing knowledge.
- m6 `MfgDocType` — PRODUCTION vs REWORK indistinguishable to a PO.
- m7 `DepreciationMethod` (6 values) / `AssetMovementType` (6 values) — no computation/meaning gloss.
- m12 `PromotionType` SUBSTITUTION, `PointTransactionType` ADJUST, `VoucherStatus` ISSUED — zero context.

**Recommended fix:** add an "Arti bisnis / contoh" column to the README §4 enum
table (1 line per value); fill the missing `ItemType` values; mirror briefly in
each module's enum section. ~1 work session, removes the single biggest
comprehension blocker repo-wide.

## Finding 2 — m2 finance: jargon + missing end-to-end flow

- Posting matrix (m2:30–35) assumes reader knows `SOFT_CLOSED` operationally.
- `arApType` (m2:193), `sourceDocType`, `cashFlowCategory` — accounting jargon, no gloss.
- AR/AP split across Receipt + PaymentInstrument + SettlementAllocation
  (m2:284–360) with **no narrated journey** (invoice → receipt → instrument → allocation → settled).
- Giro lifecycle uses legacy acronyms RG/SG/RGC/SGC (m2:365–399), no flow.
- No decision rule for CashBankTransaction vs ArReceipt (when to use which).
- Period-close story split across m0 (lifecycle) + m2 (posting matrix), never narrated end-to-end.

**Recommended fix:** add a "Business flows" section (AR/AP, giro, period-close)
+ a short "For business users" intro paragraph.

## Finding 3 — m4 purchasing: structure-heavy, no procure-to-pay narrative

- «PurchaseDocHeader/Line» common shapes are field tables with no story.
- `accruedPayableAccountId` (GR/IR accrual), `priceMode`, dimensions
  (`costCenterId`/`divisionId`/…) listed without business explanation.
- Chain (requisition→RFQ→quotation→bid→order→GR→invoice→return) is a header
  line only; no narrated lifecycle with the linking FKs / status progression.

**Recommended fix:** add a "Document flow (procure-to-pay)" subsection +
a dimensions/GL-account explainer paragraph.

## Finding 4 — m12 POS: weakest doc

- Opens with a fidelity caveat (inferred from VB/Flex, not in semantic-schema) —
  undermines confidence; should be a single top callout, not scattered.
- POS sale reuses `sls_invoices` → module narrative split across m5 + m12.
- Promotion/point/voucher interaction within one sale never narrated
  (how points are earned, when a voucher redeems, what `SUBSTITUTION` does).

**Recommended fix:** consolidate the fidelity warning to one callout; add a
customer-perspective promo/point/voucher flow; gloss all 4 m12 enums.

## What is already good (no action)

- m0/m1/m3/m5/m6/m7 entity comprehension and flow narration are solid.
- `module-roadmap.md` legacy→modern mapping is clear and needs nothing.
- ERDs (README §5–6.7) are accurate; the gap is prose, not structure.

## Suggested remediation order (when go-ahead given)

1. Enum gloss across README §4 + module enum sections (universal, cheapest).
2. m2 finance flow + intro; m12 POS flow + enum gloss + caveat consolidation.
3. m4 purchasing procure-to-pay narrative + dimensions explainer.
