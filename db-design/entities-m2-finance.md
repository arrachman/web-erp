# Finance / Accounting: Entity Catalog (m2 → `fin`)

> Legacy "Finance" (m2) maps to the **`fin_*`** semantic domain per
> [web-erp/CLAUDE.md §1](../CLAUDE.md). No `erp_` prefix, no numeric `m<n>` segment.
> Depth = modern **core subset** (resolved [README §8](README.md#8-resolved-decisions-2026-05-17)
> #15): legacy GJ/JM/AJ/CB headers (near-identical) unify into one journal; `*_detail`
> normalize into line tables; `riwayat_*` snapshots → `sys_audit_logs`.

Field-level model. Types are Prisma/Postgres (PK/FK = **`BigInt`**, resolved §8 #2).
All entities carry the global **audit + soft-delete** columns from
[README §3](README.md#3-global-conventions) — omitted per-row below. Transaction
masters also carry **`legacyCode String?`** (nullable, `@@index`; original MyERP+
doc number for CDC/ETL backfill — resolved §8 #7) — omitted per-row below.

Legend: 🔑 business key · ➜ FK · ◆ enum · ○ nullable. Money `Decimal(19,4)`;
`*Fx` = the same amount in transaction (foreign) currency; rate `Decimal(19,6)`.

> **Accounting period:** legacy `m2_accounting_period` (aptahun/apbulan/aptutupperiode)
> is **not** a new table — it maps onto **`sys_fiscal_periods`** (already in MVP,
> lifecycle `OPEN`/`SOFT_CLOSED`/`CLOSED`/`REOPENED` — resolved §8 #20).
> Every `fin_*`/`inv_*` document FKs `fiscalPeriodId → sys_fiscal_periods`.
>
> **Invariant (app-enforced, resolved §8 #20):** `fiscalPeriodId` MUST be the period
> whose `[startDate,endDate]` contains the document `entryDate`/`transactionDate` —
> the period is *derived* from the date, never picked independently. Backdated
> documents resolve to the period of their backdated date.

### Posting matrix — `JournalType` × `FiscalPeriodStatus`

| Status | Operational source docs¹ | `GENERAL` JV | `MEMORIAL`/`ADJUSTMENT`/`OPENING_BALANCE` | Recost (direct-update ledger) | `CLOSING` (auto, tutup buku) |
| --- | --- | --- | --- | --- | --- |
| `OPEN` | post ✓ | post ✓ | post ✓ | ✓ update `fin_ledger_entries` langsung | ✗ |
| `SOFT_CLOSED` | DRAFT only — posting ✗ | ✗ | post ✓ (accountant) | ✓ update `fin_ledger_entries` langsung | ✓ — triggered by `fin_period_closings` run |
| `CLOSED` | ✗ | ✗ | ✗ | ✗ — tidak bisa recost periode yang sudah `CLOSED` | ✗ (already closed) |
| `REOPENED` | post ✓ | post ✓ | post ✓ | ✓ update `fin_ledger_entries` langsung | ✓ — re-close run allowed |

¹ Operational = `fin_cash_bank_transactions`, `fin_ar_receipts`, `fin_ap_payments`,
dan posted `inv_*`/`pur_*`/`sls_*` documents.
**Non-disruptive input:** a `SOFT_CLOSED`/`CLOSED` period never blocks users —
they keep creating `DRAFT`s and posting freely into any `OPEN`/`REOPENED` period;
only *posting that would write `fin_ledger_entries` into the locked period* is
rejected. This satisfies "di luar periodic, user tetap bisa input tanpa mengganggu."

**Edit dokumen POSTED (resolved §8 #22):** dokumen yang sudah `POSTED` bisa
diedit. Saat disimpan, sistem otomatis: (1) reverse `fin_ledger_entries` lama
milik dokumen itu, (2) post ulang dengan nilai baru, (3) buat
`inv_cost_recalculations` `status = PENDING`. UI tampilkan notifikasi recost
pending selama ada run `PENDING` untuk periode yang bersangkutan.

---

## Enums (added to [README §4](README.md#4-enum-catalog))

| Enum | Values | Legacy source |
| --- | --- | --- |
| `JournalType` | `GENERAL`, `MEMORIAL`, `ADJUSTMENT`, `OPENING_BALANCE`, `CLOSING` | `m2_gj`/`m2_jm`/`m2_aj`/`m2_cb`; `CLOSING` = system-generated tutup buku (resolved §8 #21) |
| `PeriodCloseStatus` | `PENDING`, `IN_PROGRESS`, `COMPLETED`, `FAILED` | new — `fin_period_closings` run state (resolved §8 #21) |
| `DocumentStatus` | `DRAFT`, `POSTED`, `VOID`, `CANCELLED` | `*status`/`*statussebelumnya` |
| `PostingStatus` | `UNPOSTED`, `POSTED` | `*posting`/`*postingtgl` |
| `SettlementStatus` | `UNPAID`, `PARTIAL`, `PAID` | `*statusbayar`/`*tgllunas` |
| `CashBankDirection` | `RECEIPT`, `DISBURSEMENT` | `m2_cr` vs `m2_bd`/`m2_cd` |
| `PaymentMethod` | `CASH`, `TRANSFER`, `GIRO`, `CHEQUE`, `CARD`, `OTHER` | `*carabayar` |
| `GiroType` | `INCOMING`, `OUTGOING` | `m2_rg` vs `m2_sg` (`gljenis`) |
| `GiroStatus` | `OUTSTANDING`, `CLEARED`, `BOUNCED`, `CANCELLED` | `glstatus`/`rgstatusrgc`/`sgstatussgc` |
| `ArApType` | `RECEIVABLE`, `PAYABLE` | `thutangpiutang` |
| `ReconciliationStatus` | `UNRECONCILED`, `RECONCILED` | `tsudahrekonsiliasi`/`ttglrekonsiliasi` |

`CashFlowCategory` (`OPERATING`/`INVESTING`/`FINANCING`) is **reused** from the m1
enum catalog for `tjenisaruskas`.

---

## GL dimensions (resolved §8 #16 — full analytic dimensions)

Legacy headers carry `cabang`/`lokasi`; legacy `*_detail` carry
`costcenter`/`divisi`/`subdivisi`/`proyek`. These become real FKs on every
journal/ledger/cash line. **Dimension masters live in the `md` domain**
(un-deferred per §8 #16) — catalogued here for completeness; their physical
home + m1 count update belongs to a follow-up `entities-m1` revision.

### CostCenter → `md_cost_centers`  (legacy `m1_cost_center`)
| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | |
| parentId ○ ➜ | BigInt → CostCenter | optional nesting |
| isActive | Boolean | |

### Division → `md_divisions`  ·  Subdivision → `md_subdivisions`  ·  Project → `md_projects`
Same shape as CostCenter (`id`, `code🔑`, `name`, optional `parentId`, `isActive`).
`Subdivision.divisionId ➜ Division`. `Project` adds `startDate○`, `endDate○`,
`branchId ○ ➜ Branch`. (Branch/Location already exist in m1 `md_*`.)

---

## Journals (`fin_*`)

### ErpFinJournalEntry → `fin_journal_entries`  (legacy `m2_gj` + `m2_jm` + `m2_aj` + `m2_cb`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `*notransaksi` (manual/printed no.) |
| autoNumber ○ | String | `*autonotransaksi` (system seq, via `sys_document_numberings`) |
| journalType ◆ | `JournalType` | GENERAL/MEMORIAL/ADJUSTMENT/OPENING_BALANCE — unifies gj/jm/aj/cb |
| branchId ➜ | BigInt → Branch | `*cabang` |
| locationId ○ ➜ | BigInt → Location | `*lokasi` |
| source ○ | String | `*sumber` (originating doc/module) |
| entryDate | Date | `*tgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `*kodepa` → `sys_fiscal_periods` |
| partnerId ○ ➜ | BigInt → Partner | `*kontak` |
| contactPerson ○ | String | `*kontakperson` |
| description | String | `*uraian` |
| notes ○ | String | `*catatan` |
| currencyId ➜ | BigInt → Currency | `*matauang` |
| exchangeRate | Decimal(19,6) | `*kurs` |
| status ◆ | `DocumentStatus` | `*status` |
| previousStatus ○ ◆ | `DocumentStatus` | `*statussebelumnya` |
| revisionCount | Int | `*jmlrevisi` |
| printCount | Int | `*cetakanke` |
| postingStatus ◆ | `PostingStatus` | `*posting` |
| postedAt ○ | DateTime | `*postingtgl` |
| postedById ○ ➜ | BigInt → ErpUser | who posted |

Relations: `lines ErpFinJournalLine[]`, `branch`, `location`, `fiscalPeriod`,
`partner`, `currency`. **Invariant (app-enforced):** Σ`debit` = Σ`credit` per
entry before `postingStatus = POSTED`. Posting creates `fin_ledger_entries` rows.
Indexes: `@@index([fiscalPeriodId])`, `@@index([entryDate])`, `@@index([journalType, status])`.

### ErpFinJournalLine → `fin_journal_lines`  (legacy `m2_*_detail`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| journalEntryId ➜ | BigInt → ErpFinJournalEntry | `idgj`/`idjm`/`idaj`/`idcb` |
| accountId ➜ | BigInt → Account | `norek` → `md_accounts` |
| currencyId ➜ | BigInt → Currency | `matauang` |
| exchangeRate | Decimal(19,6) | `kurs` |
| debit | Decimal(19,4) | `debit` |
| credit | Decimal(19,4) | `kredit` |
| debitFx ○ | Decimal(19,4) | `debitvalas` |
| creditFx ○ | Decimal(19,4) | `kreditvalas` |
| notes ○ | String | `catatan` |
| costCenterId ○ ➜ | BigInt → CostCenter | `costcenter` |
| divisionId ○ ➜ | BigInt → Division | `divisi` |
| subdivisionId ○ ➜ | BigInt → Subdivision | `subdivisi` |
| projectId ○ ➜ | BigInt → Project | `proyek` |
| lineNo | Int | `urutan` |

`@@index([journalEntryId])`, `@@index([accountId])`. One of `debit`/`credit` is 0.

---

## Posted General Ledger (`fin_*`)

### ErpFinLedgerEntry → `fin_ledger_entries`  (legacy `m2_transaction_journal`)

The single posted-movement table — source for trial balance, GL, AR/AP aging,
cash-flow. **Append-on-post; `debit`/`credit` dapat di-update oleh recost run
yang sah** (resolved §8 #23). Koreksi non-recost tetap via reversing entry.

**HPP recalculation → recost direct-update (resolved §8 #18, #23).**
Inventory adalah **perpetual moving-average** (`CostingMethod` setting, §8 #19).
Saat ada edit dokumen POSTED atau posting terlambat yang mengubah cost history,
sistem buat `inv_cost_recalculations` `PENDING`. Akuntan trigger recost →
run recomputes moving-average → **langsung update `debit`/`credit` pada baris
COGS yang ada** di `fin_ledger_entries` (tidak emit jurnal ADJUSTMENT baru).
Neraca + Laba Rugi otomatis fix karena laporan baca langsung dari tabel ini.
Setiap baris yang di-update di-stamp `recostedAt`/`recostedById`/`recostedByRunId`.
Recost hanya bisa jalan di periode `OPEN`/`SOFT_CLOSED`/`REOPENED` — periode
`CLOSED` tidak bisa di-recost (per posting matrix).

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | `tid` |
| branchId ➜ | BigInt → Branch | `tcabang` |
| locationId ○ ➜ | BigInt → Location | `tlokasi` |
| source | String | `tsumber` (module) |
| sourceDocType ○ | String | `tkodetabelangka` (origin table key) |
| sourceId ○ | BigInt | `tidtransaksi` (origin row id) |
| docNumber | String | `tnotransaksi` |
| entryDate | Date | `ttgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `tkodepa` |
| partnerId ○ ➜ | BigInt → Partner | `tkontak` |
| accountId ➜ | BigInt → Account | `tnorek` |
| description ○ | String | `turaian` |
| notes ○ | String | `tcatatan` |
| currencyId ➜ | BigInt → Currency | `tmatauang` |
| exchangeRate | Decimal(19,6) | `tkurs` |
| referenceNo ○ | String | `tnobon` |
| debit | Decimal(19,4) | `tdebit` |
| credit | Decimal(19,4) | `tkredit` |
| debitFx ○ | Decimal(19,4) | `tdebitvalas` |
| creditFx ○ | Decimal(19,4) | `tkreditvalas` |
| paymentMethod ○ ◆ | `PaymentMethod` | `tcarabayar` |
| arApType ○ ◆ | `ArApType` | `thutangpiutang` (open AR/AP marker) |
| arApRef ○ | String | `tnohutangpiutang` (open-item key) |
| dueDate ○ | Date | `ttgljatuhtempo` |
| settledDate ○ | Date | `ttgllunas` |
| settlementStatus ○ ◆ | `SettlementStatus` | `tstatuslunas` |
| reconciledAt ○ | Date | `ttglrekonsiliasi` |
| reconciliationStatus ◆ | `ReconciliationStatus` | `tsudahrekonsiliasi` |
| isOpeningBalance | Boolean | `tsaldoawal` |
| isAdjustment | Boolean | `tadjustment` |
| isRetail | Boolean | `tretail` |
| recostedAt ○ | DateTime | timestamp recost terakhir yang update baris ini |
| recostedById ○ ➜ | BigInt → ErpUser | user yang trigger recost |
| recostedByRunId ○ ➜ | BigInt → ErpInvCostRecalculation | run recost yang mengubah baris ini (audit trail) |
| group ○ | String | `tgrup` |
| cashFlowCategory ○ ◆ | `CashFlowCategory` | `tjenisaruskas` |
| budgetRealizedAmount ○ | Decimal(19,4) | `tjmlrealisasium` |
| realizationStatus ○ | String | `tstatusrealisasi` |
| costCenterId ○ ➜ | BigInt → CostCenter | `tcostcenter` |
| divisionId ○ ➜ | BigInt → Division | `tdivisi` |
| subdivisionId ○ ➜ | BigInt → Subdivision | `tsubdivisi` |
| projectId ○ ➜ | BigInt → Project | `tproyek` |
| lineNo | Int | `turutan` |
| status ◆ | `DocumentStatus` | `tstatus` |
| postingStatus ◆ | `PostingStatus` | `tposting` |
| postedAt ○ | DateTime | `tpostingtgl` |

Indexes: `@@index([accountId, entryDate])`, `@@index([fiscalPeriodId])`,
`@@index([partnerId, arApType, settlementStatus])` (AR/AP aging),
`@@index([sourceDocType, sourceId])` (drill-back to origin),
`@@index([reconciliationStatus])` (bank rec).

---

## Cash & Bank (`fin_*`)

### ErpFinCashBankTransaction → `fin_cash_bank_transactions`  (legacy `m2_cr` + `m2_bd` + `m2_cd`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `*notransaksi` |
| autoNumber ○ | String | `*autonotransaksi` |
| direction ◆ | `CashBankDirection` | RECEIPT (`cr`) / DISBURSEMENT (`bd`/`cd`) |
| branchId ➜ | BigInt → Branch | `*cabang` |
| locationId ○ ➜ | BigInt → Location | `*lokasi` |
| source ○ | String | `*sumber` |
| transactionDate | Date | `*tgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `*kodepa` |
| bankAccountId ➜ | BigInt → Account | `crnorek` — the cash/bank GL account |
| partnerId ○ ➜ | BigInt → Partner | `*kontak` |
| contactPerson ○ | String | `*kontakperson` |
| description | String | `*uraian` |
| notes ○ | String | `*catatan` |
| currencyId ➜ | BigInt → Currency | `*matauang` |
| exchangeRate | Decimal(19,6) | `*kurs` |
| amount | Decimal(19,4) | `crjumlah` |
| amountFx ○ | Decimal(19,4) | `crjumlahvalas` |
| paidAmount ○ | Decimal(19,4) | `*jumlahbayar` |
| paymentStatus ○ ◆ | `SettlementStatus` | `*statusbayar` |
| settledDate ○ | Date | `*tgllunas` |
| budgetDate ○ | Date | `bdtglanggaran` (disbursement budgeting) |
| status ◆ | `DocumentStatus` | `*status` |
| previousStatus ○ ◆ | `DocumentStatus` | `*statussebelumnya` |
| revisionCount | Int | `*jmlrevisi` |
| printCount | Int | `*cetakanke` |
| postingStatus ◆ | `PostingStatus` | `*posting` |
| postedAt ○ | DateTime | `*postingtgl` |

Relations: `lines ErpFinCashBankLine[]`. Legacy disbursement budget tags
(`bdanggaran{kategori,cabang,lokasi,costcenter,divisi,subdivisi,proyek}`) →
captured on the lines' dimension FKs (no parallel "anggaran*" columns).

### ErpFinCashBankLine → `fin_cash_bank_lines`  (legacy `m2_cr_detail` / `m2_bd_detail`)

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| cashBankTransactionId ➜ | BigInt → ErpFinCashBankTransaction | `idcr`/`idbd` |
| accountId ➜ | BigInt → Account | `norek` |
| currencyId ➜ | BigInt → Currency | `matauang` |
| exchangeRate | Decimal(19,6) | `kurs` |
| amount | Decimal(19,4) | `jumlah` |
| amountFx ○ | Decimal(19,4) | `jumlahvalas` |
| notes ○ | String | `catatan` |
| costCenterId ○ ➜ | BigInt → CostCenter | `costcenter` |
| divisionId ○ ➜ | BigInt → Division | `divisi` |
| subdivisionId ○ ➜ | BigInt → Subdivision | `subdivisi` |
| projectId ○ ➜ | BigInt → Project | `proyek` |
| lineNo | Int | `urutan` |

---

## AR Receipt / AP Payment (`fin_*`)

### ErpFinArReceipt → `fin_ar_receipts`  (legacy `m2_rm`)  ·  ErpFinApPayment → `fin_ap_payments`  (legacy `m2_sm`)

Symmetric — same shape; `ArReceipt.partner` = customer, `ApPayment.partner` =
supplier. Fields (prefix `rm*`/`sm*`):

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | `*notransaksi` |
| autoNumber ○ | String | `*autonotransaksi` |
| branchId ➜ | BigInt → Branch | `*cabang` |
| locationId ○ ➜ | BigInt → Location | `*lokasi` |
| source ○ | String | `*sumber` |
| transactionDate | Date | `*tgl` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `*kodepa` |
| partnerId ➜ | BigInt → Partner | `*kontak` (customer / supplier) |
| contactPerson ○ | String | `*kontakperson` |
| bankAccountId ○ ➜ | BigInt → Account | `*norek` (deposit / source account) |
| description | String | `*uraian` |
| notes ○ | String | `*catatan` |
| currencyId ➜ | BigInt → Currency | `*matauang` |
| exchangeRate | Decimal(19,6) | `*kurs` |
| amount | Decimal(19,4) | `*jumlah` |
| amountFx ○ | Decimal(19,4) | `*jumlahvalas` |
| allocatedAmount | Decimal(19,4) | Σ allocations (`*jumlahbayar`) |
| paymentStatus ◆ | `SettlementStatus` | `*statusbayar` |
| settledDate ○ | Date | `*tgllunas` |
| status ◆ | `DocumentStatus` | `*status` |
| previousStatus ○ ◆ | `DocumentStatus` | `*statussebelumnya` |
| revisionCount | Int | `*jmlrevisi` |
| printCount | Int | `*cetakanke` |
| postingStatus ◆ | `PostingStatus` | `*posting` |
| postedAt ○ | DateTime | `*postingtgl` |

Relations: `instruments ErpFinPaymentInstrument[]`, `allocations ErpFinSettlementAllocation[]`.

### ErpFinPaymentInstrument → `fin_payment_instruments`  (legacy `m2_rm_pay` + `m2_sm_pay`)

How a receipt/payment is tendered (cash/giro/transfer). Exactly one of
`arReceiptId`/`apPaymentId` is set.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| arReceiptId ○ ➜ | BigInt → ErpFinArReceipt | `idrm` |
| apPaymentId ○ ➜ | BigInt → ErpFinApPayment | `idsm` |
| method ◆ | `PaymentMethod` | `carabayar` |
| currencyId ➜ | BigInt → Currency | `matauang` |
| exchangeRate | Decimal(19,6) | `kurs` |
| amount | Decimal(19,4) | `jumlah` |
| amountFx ○ | Decimal(19,4) | `jumlahvalas` |
| giroId ○ ➜ | BigInt → ErpFinGiro | `nogiro` → giro register |
| dueDate ○ | Date | `tgljt` |
| bankName ○ | String | `bank` |
| bankAccountNo ○ | String | `noacbank` |
| bankAccountId ○ ➜ | BigInt → Account | `rekbank` |
| giroAccountId ○ ➜ | BigInt → Account | `rekgiro` |
| notes ○ | String | `catatan` |
| lineNo | Int | `urutan` |

### ErpFinSettlementAllocation → `fin_settlement_allocations`  (modern — legacy used `transaction_journal` open-items)

Allocates a receipt/payment to specific open AR/AP ledger items.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| arReceiptId ○ ➜ | BigInt → ErpFinArReceipt | one of receipt/payment |
| apPaymentId ○ ➜ | BigInt → ErpFinApPayment | |
| ledgerEntryId ➜ | BigInt → ErpFinLedgerEntry | the open AR/AP item being settled |
| invoiceRef ○ | String | source invoice doc no. |
| amount | Decimal(19,4) | settled amount |
| amountFx ○ | Decimal(19,4) | |
| lineNo | Int | |

`@@index([ledgerEntryId])`. Σ allocation ≤ open balance (app-enforced).

---

## Giro / Postdated Cheque (`fin_*`)

### ErpFinGiro → `fin_giros`  (legacy `m2_rg` + `m2_sg` + `m2_giro_list`; clearing = `m2_rgc`/`m2_sgc`)

RG/SG document creation registers a giro; RGC/SGC clearing is a **status
transition** here (`OUTSTANDING → CLEARED/BOUNCED`) plus a posted clearing
ledger entry — no separate clearing table.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| giroNumber 🔑 | String unique | `glnogiro` / `*notransaksi` |
| type ◆ | `GiroType` | INCOMING (`rg`) / OUTGOING (`sg`) — `gljenis` |
| source ○ | String | `glsumber` |
| sourceTransactionId ○ | BigInt | `glidtransaksi` (origin doc) |
| partnerId ○ ➜ | BigInt → Partner | `glkontak` / `*kontak` |
| branchId ○ ➜ | BigInt → Branch | `*cabang` |
| fiscalPeriodId ○ ➜ | BigInt → ErpFiscalPeriod | `*kodepa` |
| bankName ○ | String | `glbank` |
| bankAccountNo ○ | String | `glnoacbank` |
| bankAccountId ○ ➜ | BigInt → Account | `glrekbank` |
| giroAccountId ○ ➜ | BigInt → Account | `glrekgiro` |
| currencyId ➜ | BigInt → Currency | `glmatauang` |
| exchangeRate | Decimal(19,6) | `glkurs` |
| amount | Decimal(19,4) | `gljumlah` |
| amountFx ○ | Decimal(19,4) | `gljumlahvalas` |
| dueDate | Date | `gltgljthtempo` |
| clearedDate ○ | Date | `gltglcair` |
| status ◆ | `GiroStatus` | `glstatus` / `rgstatusrgc` / `sgstatussgc` |
| previousStatus ○ ◆ | `GiroStatus` | `glstatussebelumnya` |
| description ○ | String | `*uraian` |
| notes ○ | String | `*catatan` |
| lineNo | Int | `glurutan` |

Indexes: `@@index([status, dueDate])` (giro maturity report),
`@@index([partnerId])`.

---

## Budget vs Realization (`fin_*`)

### ErpFinBudgetRealization → `fin_budget_realizations`  (legacy `m2_realization` + `m2_realization_*`)

Per-account / period / dimension budget vs posted actual. Legacy split realization
across `_branch/_cost_center/_division/_location/_project/_subdivision`; modern
collapses to one row keyed by the dimension FKs (nullable = "all").

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `rtahun`+`rbulan` → period |
| accountId ➜ | BigInt → Account | `rnorek` |
| debitTotal | Decimal(19,4) | `rjmldebit` (posted actual Dr) |
| creditTotal | Decimal(19,4) | `rjmlkredit` (posted actual Cr) |
| budgetAmount | Decimal(19,4) | `ranggaran` |
| branchId ○ ➜ | BigInt → Branch | `m2_realization_branch` |
| locationId ○ ➜ | BigInt → Location | `m2_realization_location` |
| costCenterId ○ ➜ | BigInt → CostCenter | `m2_realization_cost_center` |
| divisionId ○ ➜ | BigInt → Division | `m2_realization_division` |
| subdivisionId ○ ➜ | BigInt → Subdivision | `m2_realization_subdivision` |
| projectId ○ ➜ | BigInt → Project | `m2_realization_project` |

Unique: `@@unique([fiscalPeriodId, accountId, branchId, locationId, costCenterId,
divisionId, subdivisionId, projectId])`. Actuals are recomputable from
`fin_ledger_entries`; budgets are user-entered (kept as a stored snapshot).

---

## Period-close process (`fin_*`)

### ErpFinPeriodClosing → `fin_period_closings`  (modern — no legacy table; resolved §8 #21)

A **tutup-buku run**: one record per period-close execution. Triggered manually by
the accountant after all transactions for the period are finalized. The run:

1. Validates there are no un-posted or in-review documents for the period.
2. Computes net income/loss from `fin_ledger_entries` for the period.
3. Generates one auto `JournalType.CLOSING` entry: rolls net income/loss into the
   `retainedEarningsAccountId` (Dr/Cr Revenue & Expense accounts to zero, Cr/Dr
   Retained Earnings).
4. Sets `sys_fiscal_periods.status = CLOSED` and stamps `closedAt`/`closedById`.
5. Sets own `status = COMPLETED`.

If the period is `REOPENED` later, a new closing run can be initiated (re-close).
`fin_ledger_entries` stays immutable — the closing JV is a normal posted entry.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | run reference (via `sys_document_numberings`) |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | one closing per period per run |
| status ◆ | `PeriodCloseStatus` | `PENDING`/`IN_PROGRESS`/`COMPLETED`/`FAILED` |
| retainedEarningsAccountId ➜ | BigInt → Account | target CoA for laba ditahan roll |
| closingJournalEntryId ○ ➜ | BigInt → ErpFinJournalEntry | the auto CLOSING JV generated |
| netAmount ○ | Decimal(19,4) | net income (positive) or net loss (negative) rolled |
| startedAt ○ | DateTime | |
| completedAt ○ | DateTime | |
| failedAt ○ | DateTime | |
| failureReason ○ | String | error detail if `FAILED` |
| reopenedAt ○ | DateTime | if the period was subsequently reopened |
| reopenedById ○ ➜ | BigInt → ErpUser | who reopened |
| reopenReason ○ | String | mandatory on reopen |
| notes ○ | String | operator notes |

Relations: `fiscalPeriod`, `retainedEarningsAccount`, `closingJournalEntry`,
`reopenedBy`. Indexes: `@@index([fiscalPeriodId])`, `@@index([status])`.

> **Unique constraint:** `@@unique([fiscalPeriodId])` — one *active* close run per
> period. If a period is reopened and re-closed, the same record is updated
> (status transitions `COMPLETED → [period reopened] → IN_PROGRESS → COMPLETED`).
> Historical runs are captured via `sys_audit_logs` on each transition.

---

**Count:** 12 Finance core (`fin_*`) entities — JournalEntry, JournalLine, LedgerEntry,
CashBankTransaction, CashBankLine, ArReceipt, ApPayment, PaymentInstrument,
SettlementAllocation, Giro, BudgetRealization, **PeriodClosing**. Plus **4 GL dimension masters**
moved to `md_*` (CostCenter, Division, Subdivision, Project — to be folded into a
revised `entities-m1-master-data.md` count). Accounting period **reuses**
`sys_fiscal_periods` (no new table).

**Enterprise extensions (19 entities, total `fin` = 31):** Account Determination,
Tax sub-ledger (PPN+PPh), FX Revaluation, Bank Reconciliation, Recurring/Accrual,
Financial Report Definitions, Credit Limit & Collection, Inter-branch/Consolidation —
field catalogs di **[entities-m2-finance-enterprise.md](entities-m2-finance-enterprise.md)**
(decisions README §8 #24–#30, dikonfirmasi 2026-05-18).

Legacy field-mapping appendix: **[legacy-mapping.md](legacy-mapping.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
