# Finance / Accounting: Enterprise Extensions (m2 → `fin`)

> Komplemen dari **[entities-m2-finance.md](entities-m2-finance.md)** (core: GL,
> Cash/Bank, AR/AP, Period Close). File ini menambah 7 kelompok fitur enterprise
> yang dikonfirmasi user 2026-05-18: Account Determination, Tax sub-ledger,
> FX Revaluation, Bank Reconciliation, Recurring/Accrual, Financial Report
> Definitions, Credit Limit & Collection, Inter-branch/Consolidation.
>
> Keputusan: README §8 #24–#30. Field conventions & global enums: README §3–§4.
> PK/FK = **BigInt**. Audit + soft-delete columns (resolved §8 #3/#7) berlaku
> pada semua entitas kecuali detail/lookup table — omitted per-row.

Legend: 🔑 business key · ➜ FK · ◆ enum · ○ nullable.
Money `Decimal(19,4)`; Rate `Decimal(9,4)`; Exchange rate `Decimal(19,6)`.

---

## 1. Account Determination Engine (`fin_posting_rules`)

Tabel aturan posting terpusat — menggantikan akun yang di-hardcode di `md_items`
(inventory/sales/cogs GL) dan `md_partners` (AR/AP GL). Saat sebuah event terjadi
(misal: SALE_INVOICE), posting engine mencari aturan yang paling spesifik match
berdasarkan kombinasi `module × eventType × branchId? × itemCategoryId? × partnerCategoryId?`
dan priority. Hasilnya = daftar leg `(accountId, isDebit)` yang digunakan untuk
generate `fin_journal_entries`.

### ErpFinPostingRule → `fin_posting_rules`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| name | String | label deskriptif |
| module | String | 'sls' / 'pur' / 'inv' / 'mfg' / 'fa' / 'fin' |
| eventType ◆ | `PostingEvent` | jenis event yang memicu aturan ini |
| branchId ○ ➜ | BigInt → Branch | null = semua branch |
| itemCategoryId ○ ➜ | BigInt → ItemCategory | null = semua kategori item |
| partnerCategoryId ○ ➜ | BigInt → PartnerCategory | null = semua kategori partner |
| taxId ○ ➜ | BigInt → Tax | null = non-tax leg; isi untuk aturan akun pajak spesifik |
| priority | Int | makin kecil = makin tinggi prioritas; aturan paling spesifik vince |
| isActive | Boolean | |

Unique index: `@@index([module, eventType, priority])`.
Matching logic (app-enforced): evaluasi kandidat dari priority terendah; ambil
rule pertama yang semua nullable-criteria-nya cocok (null = wildcard).

### ErpFinPostingRuleLine → `fin_posting_rule_lines`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| ruleId ➜ | BigInt → ErpFinPostingRule | |
| legName | String | e.g. RECEIVABLE, REVENUE, TAX, COGS, INVENTORY, CLEARING |
| accountId ➜ | BigInt → Account | target GL account |
| isDebit | Boolean | true = Debit; false = Credit |
| description ○ | String | keterangan leg ini |
| lineNo | Int | urutan dalam rule |

---

## 2. Tax Sub-ledger — PPN & PPh (`fin_tax_entries`, `fin_withholding_tax_certificates`)

Sub-ledger pajak per transaksi — dasar untuk rekap Faktur Pajak (e-Faktur PPN)
dan Bukti Potong PPh. Setiap transaksi kena pajak menghasilkan satu atau lebih
`fin_tax_entries`. PPh yang dipotong masing-masing punya `fin_withholding_tax_certificates`.

### ErpFinTaxEntry → `fin_tax_entries`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| module | String | source module ('sls'/'pur'/etc) |
| sourceDocType ○ | String | e.g. 'sls_invoices', 'pur_invoices' |
| sourceId ○ | BigInt | source doc id |
| docNumber | String | source doc number |
| transactionDate | Date | |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | `sys_fiscal_periods` |
| partnerId ○ ➜ | BigInt → Partner | |
| partnerNpwp ○ | String | NPWP snapshot saat transaksi |
| partnerName ○ | String | nama snapshot saat transaksi |
| taxId ➜ | BigInt → Tax | jenis pajak (`md_taxes`) |
| taxEntryType ◆ | `TaxEntryType` | PPN_KELUARAN / PPN_MASUKAN / PPH_21 / PPH_23 / PPH_4_2 / PPH_25 / PPH_26 / OTHER |
| dpp | Decimal(19,4) | dasar pengenaan pajak |
| taxRate | Decimal(9,4) | tarif yang diaplikasikan |
| taxAmount | Decimal(19,4) | nilai pajak |
| fakturNumber ○ | String | PPN: nomor faktur pajak (e-Faktur) |
| fakturDate ○ | Date | PPN: tanggal faktur |
| status ◆ | `TaxEntryStatus` | DRAFT / CONFIRMED / REPORTED / CANCELLED |
| reportedPeriodId ○ ➜ | BigInt → ErpFiscalPeriod | periode SPT saat dilaporkan ke DJP |
| reportedAt ○ | DateTime | |
| currencyId ➜ | BigInt → Currency | |
| exchangeRate | Decimal(19,6) | |
| taxAmountFx ○ | Decimal(19,4) | nilai pajak dalam mata uang transaksi (jika valas) |
| ledgerEntryId ○ ➜ | BigInt → ErpFinLedgerEntry | link ke posting GL |

Indexes: `@@index([taxEntryType, fiscalPeriodId])` (rekap SPT per periode),
`@@index([sourceDocType, sourceId])` (drill-back ke sumber),
`@@index([fakturNumber])` (cari faktur pajak).

### ErpFinWhtCertificate → `fin_withholding_tax_certificates`

Bukti Potong PPh (e-Bupot) — satu sertifikat per potongan PPh kepada satu pihak.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| certNumber 🔑 | String unique | nomor bukti potong |
| autoNumber ○ | String | system-generated (via `sys_document_numberings`) |
| pphType ◆ | `TaxEntryType` | PPH_21 / PPH_23 / PPH_4_2 / PPH_25 / PPH_26 |
| transactionDate | Date | |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | |
| partnerId ➜ | BigInt → Partner | pihak yang dipotong |
| partnerNpwp ○ | String | NPWP snapshot |
| partnerName | String | nama snapshot |
| dpp | Decimal(19,4) | dasar pengenaan |
| rate | Decimal(9,4) | |
| amountWithheld | Decimal(19,4) | nilai yang dipotong |
| status ◆ | `WhtCertStatus` | ISSUED / CANCELLED |
| taxEntryId ○ ➜ | BigInt → ErpFinTaxEntry | link ke tax entry sumber |
| sourceDocType ○ | String | |
| sourceId ○ | BigInt | |
| notes ○ | String | |

---

## 3. FX Revaluation (`fin_fx_revaluation_runs`, `fin_fx_revaluation_lines`)

Run periodik untuk merevaluasi saldo AR/AP/bank valas ke kurs penutup, lalu
mem-posting selisih kurs unrealized (gain/loss) sebagai jurnal otomatis.
Dilakukan di akhir periode sebelum tutup buku.

### ErpFinFxRevaluationRun → `fin_fx_revaluation_runs`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | via `sys_document_numberings` |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | periode yang direvaluasi |
| revaluationDate | Date | biasanya = tanggal akhir periode |
| status ◆ | `FxRevaluationStatus` | PENDING / IN_PROGRESS / COMPLETED / FAILED |
| totalGainLoss ○ | Decimal(19,4) | net unrealized gain(+) / loss(-) |
| gainAccountId ➜ | BigInt → Account | akun Keuntungan Selisih Kurs |
| lossAccountId ➜ | BigInt → Account | akun Kerugian Selisih Kurs |
| closingJournalEntryId ○ ➜ | BigInt → ErpFinJournalEntry | JV `ADJUSTMENT` yang di-generate |
| startedAt ○ | DateTime | |
| completedAt ○ | DateTime | |
| failedAt ○ | DateTime | |
| failureReason ○ | String | |
| notes ○ | String | |

### ErpFinFxRevaluationLine → `fin_fx_revaluation_lines`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| revaluationRunId ➜ | BigInt → ErpFinFxRevaluationRun | |
| accountId ➜ | BigInt → Account | akun yang direvaluasi |
| currencyId ➜ | BigInt → Currency | mata uang asing |
| bookBalanceFx | Decimal(19,4) | saldo outstanding dalam mata uang asing |
| bookBalanceIdr | Decimal(19,4) | nilai buku saldo (kurs historis) |
| revaluationRate | Decimal(19,6) | kurs penutup di revaluationDate |
| revaluedBalanceIdr | Decimal(19,4) | bookBalanceFx × revaluationRate |
| gainLossAmount | Decimal(19,4) | revaluedBalanceIdr − bookBalanceIdr |
| lineNo | Int | |

---

## 4a. Bank Reconciliation (`fin_bank_statements`, `fin_bank_statement_lines`)

Impor rekening koran bank + workspace matching ke `fin_ledger_entries` /
`fin_cash_bank_transactions`. Menggantikan flag manual `reconciliationStatus`
dengan proses terstruktur.

### ErpFinBankStatement → `fin_bank_statements`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| bankAccountId ➜ | BigInt → Account | akun GL kas/bank |
| branchId ➜ | BigInt → Branch | |
| statementNumber ○ | String | nomor referensi dari bank |
| periodStart | Date | |
| periodEnd | Date | |
| openingBalance | Decimal(19,4) | saldo awal per rekening koran |
| closingBalance | Decimal(19,4) | saldo akhir per rekening koran |
| currencyId ➜ | BigInt → Currency | |
| status ◆ | `BankStatementStatus` | IMPORTED / IN_REVIEW / RECONCILED |
| importedAt | DateTime | |
| notes ○ | String | |

### ErpFinBankStatementLine → `fin_bank_statement_lines`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| statementId ➜ | BigInt → ErpFinBankStatement | |
| valueDate | Date | tanggal efektif per bank |
| description ○ | String | |
| referenceNo ○ | String | nomor referensi dari bank |
| debit | Decimal(19,4) | uang masuk ke rekening |
| credit | Decimal(19,4) | uang keluar dari rekening |
| runningBalance ○ | Decimal(19,4) | saldo berjalan per baris |
| matchedLedgerEntryId ○ ➜ | BigInt → ErpFinLedgerEntry | GL entry yang dicocokkan |
| matchedCashBankTransactionId ○ ➜ | BigInt → ErpFinCashBankTransaction | CBT yang dicocokkan |
| reconciliationStatus ◆ | `ReconciliationStatus` | UNRECONCILED / RECONCILED |
| lineNo | Int | |

---

## 4b. Recurring Journals & Accrual Schedules

### ErpFinRecurringJournalTemplate → `fin_recurring_journal_templates`

Template jurnal berkala (gaji, sewa, bunga, dll). Saat `nextRunDate` tiba,
sistem generate `fin_journal_entries` otomatis dari template ini.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | |
| description ○ | String | |
| journalType ◆ | `JournalType` | biasanya GENERAL atau MEMORIAL |
| branchId ➜ | BigInt → Branch | |
| locationId ○ ➜ | BigInt → Location | |
| currencyId ➜ | BigInt → Currency | |
| frequency ◆ | `RecurringFrequency` | DAILY / WEEKLY / MONTHLY / QUARTERLY / YEARLY |
| startDate | Date | |
| endDate ○ | Date | null = tidak terbatas |
| nextRunDate | Date | tanggal eksekusi berikutnya |
| maxOccurrences ○ | Int | null = tidak terbatas |
| occurrenceCount | Int @default(0) | sudah berapa kali dijalankan |
| status ◆ | `RecurringStatus` | ACTIVE / PAUSED / COMPLETED / CANCELLED |

### ErpFinRecurringJournalTemplateLine → `fin_recurring_journal_template_lines`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| templateId ➜ | BigInt → ErpFinRecurringJournalTemplate | |
| accountId ➜ | BigInt → Account | |
| costCenterId ○ ➜ | BigInt → CostCenter | |
| divisionId ○ ➜ | BigInt → Division | |
| projectId ○ ➜ | BigInt → Project | |
| debit | Decimal(19,4) | |
| credit | Decimal(19,4) | |
| description ○ | String | |
| lineNo | Int | |

### ErpFinAccrualSchedule → `fin_accrual_schedules`

Jadwal amortisasi beban/pendapatan — biasanya untuk biaya dibayar di muka
(prepaid expense) atau pendapatan diterima di muka (deferred revenue).
Sistem generate jurnal pengakuan otomatis tiap periode.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | |
| description ○ | String | |
| totalAmount | Decimal(19,4) | total jumlah yang perlu diamortisasi |
| startDate | Date | awal periode amortisasi |
| endDate | Date | akhir periode amortisasi |
| frequency ◆ | `RecurringFrequency` | biasanya MONTHLY |
| prepaidAccountId ➜ | BigInt → Account | akun aset/liabilitas (Beban Dibayar Dimuka / Pendapatan Diterima Dimuka) |
| expenseAccountId ➜ | BigInt → Account | akun beban/pendapatan yang diakui |
| branchId ➜ | BigInt → Branch | |
| costCenterId ○ ➜ | BigInt → CostCenter | |
| amountPerPeriod | Decimal(19,4) | computed: totalAmount ÷ jumlah periode |
| recognizedAmount | Decimal(19,4) @default(0) | kumulatif yang sudah diakui |
| remainingAmount | Decimal(19,4) | totalAmount − recognizedAmount |
| status ◆ | `RecurringStatus` | |
| sourceDocType ○ | String | dokumen asal (e.g. 'pur_invoices') |
| sourceId ○ | BigInt | id dokumen asal |

---

## 5. Financial Report Definitions (`fin_report_definitions`, sections, lines)

Layout laporan keuangan yang bisa dikonfigurasi: Neraca, Laba/Rugi, Arus Kas.
Memetakan akun CoA ke baris-baris laporan. Akuntan konfigurasi sendiri tanpa
developer. Laporan dibaca dari `fin_ledger_entries` + mapping ini.

### ErpFinReportDefinition → `fin_report_definitions`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| code 🔑 | String unique | |
| name | String | e.g. "Neraca – Standar", "Laba Rugi – Internal" |
| reportType ◆ | `FinancialReportType` | BALANCE_SHEET / INCOME_STATEMENT / CASH_FLOW / CUSTOM |
| description ○ | String | |
| isDefault | Boolean | satu default per reportType (enforced app-side) |
| isActive | Boolean | |
| branchId ○ ➜ | BigInt → Branch | null = global/semua branch |

### ErpFinReportSection → `fin_report_sections`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| reportId ➜ | BigInt → ErpFinReportDefinition | |
| code | String | referensi unik dalam report |
| name | String | label heading seksi |
| parentSectionId ○ ➜ | BigInt → ErpFinReportSection | seksi bertingkat |
| sortOrder | Int | urutan dalam laporan |
| normalBalance ○ ◆ | `NormalBalance` | konvensi tanda (DEBIT/CREDIT) untuk total seksi |
| isTotalRow | Boolean | baris ini menampilkan subtotal anak-anaknya |

### ErpFinReportLine → `fin_report_lines`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| sectionId ➜ | BigInt → ErpFinReportSection | |
| code ○ | String | referensi untuk formula |
| label | String | label tampilan |
| lineType ◆ | `ReportLineType` | ACCOUNTS / FORMULA / SECTION_TOTAL / HEADER / SPACER |
| accountFrom ○ | String | range CoA dari (untuk tipe ACCOUNTS) |
| accountTo ○ | String | range CoA sampai |
| specificAccountIds ○ | BigInt[] | akun spesifik (alternatif range) |
| formula ○ | String | ekspresi formula, e.g. "LABA_KOTOR - BEBAN_USAHA" (tipe FORMULA) |
| sortOrder | Int | |
| indentLevel | Int @default(0) | indentasi visual |
| isNegated | Boolean @default(false) | tampilkan positif meski akun credit-normal (misal Beban di L&R) |

---

## 6. Credit Limit & Collection Management

### ErpFinCreditLimit → `fin_credit_limits`

Batas piutang per customer. Saat input Sales Order, sistem cek saldo AR terbuka
+ nilai SO baru terhadap limit. Action tergantung konfigurasi: WARN (peringatan
saja), BLOCK (blokir), atau REQUIRE_APPROVAL (perlu approval role tertentu).

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| partnerId ➜ | BigInt → Partner | `@@unique` — satu limit aktif per partner |
| limitAmount | Decimal(19,4) | |
| currencyId ➜ | BigInt → Currency | |
| action ◆ | `CreditLimitAction` | WARN / BLOCK / REQUIRE_APPROVAL |
| overrideRoleId ○ ➜ | BigInt → ErpRole | role yang bisa override (untuk REQUIRE_APPROVAL) |
| validFrom ○ | Date | |
| validTo ○ | Date | null = berlaku selamanya |
| reviewDate ○ | Date | reminder jadwal review limit |
| notes ○ | String | |
| isActive | Boolean | |

### ErpFinDunningRule → `fin_dunning_rules`

Konfigurasi level penagihan berdasarkan hari keterlambatan. Saat periode
berjalan, sistem cocokkan saldo overdue tiap partner dengan aturan ini.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| name | String | |
| overdueDaysFrom | Int | ≥ 1 — apply mulai hari ke-N jatuh tempo |
| overdueDaysTo ○ | Int | null = tidak ada batas atas |
| dunningLevel ◆ | `DunningLevel` | LEVEL_1 / LEVEL_2 / LEVEL_3 / LEGAL |
| messageTemplate ○ | String | template pesan reminder/surat |
| isActive | Boolean | |
| sortOrder | Int | |

### ErpFinCollectionActivity → `fin_collection_activities`

Log aktivitas penagihan per customer — telepon, email, kunjungan, surat, jalur
hukum. Terhubung ke open AR item spesifik.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| partnerId ➜ | BigInt → Partner | |
| activityType ◆ | `CollectionActivityType` | PHONE_CALL / EMAIL / VISIT / LETTER / LEGAL |
| activityDate | Date | |
| dueLedgerEntryId ○ ➜ | BigInt → ErpFinLedgerEntry | open AR item yang dikejar |
| dunningLevel ○ ◆ | `DunningLevel` | level dunning saat aktivitas |
| status ◆ | `CollectionStatus` | OPEN / IN_PROGRESS / RESOLVED / ESCALATED |
| assignedToId ○ ➜ | BigInt → ErpUser | staf penagih |
| notes | String | catatan aktivitas |
| followUpDate ○ | Date | jadwal follow up berikutnya |
| resolvedAt ○ | DateTime | |
| resolvedById ○ ➜ | BigInt → ErpUser | |
| resolvedNotes ○ | String | |

---

## 7. Inter-branch / Consolidation (`fin_intercompany_rules`, `fin_intercompany_transactions`)

Untuk transaksi antar cabang atau antar entitas hukum: otomatis generate jurnal
due-from/due-to di kedua sisi, lalu eliminasi saat konsolidasi.

### ErpFinIntercompanyRule → `fin_intercompany_rules`

Konfigurasi pasangan akun antar-branch. Satu baris per kombinasi fromBranch↔toBranch.

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| fromBranchId ➜ | BigInt → Branch | branch sumber |
| toBranchId ➜ | BigInt → Branch | branch tujuan |
| dueFromAccountId ➜ | BigInt → Account | akun "Piutang Antar Cabang" di fromBranch |
| dueToAccountId ➜ | BigInt → Account | akun "Utang Antar Cabang" di toBranch |
| isActive | Boolean | |
| notes ○ | String | |

Unique: `@@unique([fromBranchId, toBranchId])`.

### ErpFinIntercompanyTransaction → `fin_intercompany_transactions`

| Field | Type | Notes |
| --- | --- | --- |
| id | BigInt PK | |
| docNumber 🔑 | String unique | |
| transactionDate | Date | |
| fiscalPeriodId ➜ | BigInt → ErpFiscalPeriod | |
| fromBranchId ➜ | BigInt → Branch | |
| toBranchId ➜ | BigInt → Branch | |
| fromJournalEntryId ○ ➜ | BigInt → ErpFinJournalEntry | JV di buku fromBranch |
| toJournalEntryId ○ ➜ | BigInt → ErpFinJournalEntry | JV di buku toBranch |
| amount | Decimal(19,4) | |
| currencyId ➜ | BigInt → Currency | |
| exchangeRate | Decimal(19,6) | |
| description | String | |
| status ◆ | `IntercompanyStatus` | PENDING_MATCH / MATCHED / ELIMINATED |
| eliminatedAt ○ | DateTime | |
| eliminatedById ○ ➜ | BigInt → ErpUser | |
| consolidationPeriodId ○ ➜ | BigInt → ErpFiscalPeriod | periode konsolidasi |
| notes ○ | String | |

---

**Count: 19 enterprise entities** —
PostingRule + PostingRuleLine (2) ·
TaxEntry + WhtCertificate (2) ·
FxRevaluationRun + FxRevaluationLine (2) ·
BankStatement + BankStatementLine (2) ·
RecurringJournalTemplate + RecurringJournalTemplateLine + AccrualSchedule (3) ·
ReportDefinition + ReportSection + ReportLine (3) ·
CreditLimit + DunningRule + CollectionActivity (3) ·
IntercompanyRule + IntercompanyTransaction (2)

**Total `fin` entities: 12 core + 19 enterprise = 31.**

Catalog core: **[entities-m2-finance.md](entities-m2-finance.md)** ·
Roadmap context: **[module-roadmap.md](module-roadmap.md)**.
