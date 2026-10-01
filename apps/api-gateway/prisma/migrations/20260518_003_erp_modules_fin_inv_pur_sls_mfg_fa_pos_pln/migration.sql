-- CreateEnum
CREATE TYPE "ErpCostingMethod" AS ENUM ('AVG', 'FIFO', 'STD');

-- CreateEnum
CREATE TYPE "ErpJournalType" AS ENUM ('GENERAL', 'MEMORIAL', 'ADJUSTMENT', 'OPENING_BALANCE', 'CLOSING');

-- CreateEnum
CREATE TYPE "ErpDocumentStatus" AS ENUM ('DRAFT', 'POSTED', 'VOID', 'CANCELLED');

-- CreateEnum
CREATE TYPE "ErpPostingStatus" AS ENUM ('UNPOSTED', 'POSTED');

-- CreateEnum
CREATE TYPE "ErpSettlementStatus" AS ENUM ('UNPAID', 'PARTIAL', 'PAID');

-- CreateEnum
CREATE TYPE "ErpCashBankDirection" AS ENUM ('RECEIPT', 'DISBURSEMENT');

-- CreateEnum
CREATE TYPE "ErpPaymentMethod" AS ENUM ('CASH', 'TRANSFER', 'GIRO', 'CHEQUE', 'CARD', 'OTHER');

-- CreateEnum
CREATE TYPE "ErpGiroType" AS ENUM ('INCOMING', 'OUTGOING');

-- CreateEnum
CREATE TYPE "ErpGiroStatus" AS ENUM ('OUTSTANDING', 'CLEARED', 'BOUNCED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "ErpArApType" AS ENUM ('RECEIVABLE', 'PAYABLE');

-- CreateEnum
CREATE TYPE "ErpReconciliationStatus" AS ENUM ('UNRECONCILED', 'RECONCILED');

-- CreateEnum
CREATE TYPE "ErpPeriodCloseStatus" AS ENUM ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'FAILED');

-- CreateEnum
CREATE TYPE "ErpPostingEvent" AS ENUM ('SALE_INVOICE', 'SALE_COGS', 'SALE_RETURN', 'SALE_RETURN_COGS', 'PUR_GOODS_RECEIPT', 'PUR_INVOICE', 'PUR_RETURN', 'INV_OPENING', 'INV_ADJUST_INCREASE', 'INV_ADJUST_DECREASE', 'INV_TRANSFER', 'AR_RECEIPT', 'AP_PAYMENT', 'CASH_RECEIPT', 'CASH_DISBURSEMENT', 'FA_ACQUISITION', 'FA_DEPRECIATION', 'FA_DISPOSAL', 'FA_REVALUATION', 'MFG_MATERIAL_ISSUE', 'MFG_MATERIAL_RETURN', 'MFG_PRODUCTION_OUTPUT', 'MFG_REWORK', 'FX_REVALUATION');

-- CreateEnum
CREATE TYPE "ErpTaxEntryType" AS ENUM ('PPN_KELUARAN', 'PPN_MASUKAN', 'PPH_21', 'PPH_23', 'PPH_4_2', 'PPH_25', 'PPH_26', 'OTHER');

-- CreateEnum
CREATE TYPE "ErpTaxEntryStatus" AS ENUM ('DRAFT', 'CONFIRMED', 'REPORTED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "ErpWhtCertStatus" AS ENUM ('ISSUED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "ErpFxRevaluationStatus" AS ENUM ('PENDING', 'IN_PROGRESS', 'COMPLETED', 'FAILED');

-- CreateEnum
CREATE TYPE "ErpBankStatementStatus" AS ENUM ('IMPORTED', 'IN_REVIEW', 'RECONCILED');

-- CreateEnum
CREATE TYPE "ErpRecurringFrequency" AS ENUM ('DAILY', 'WEEKLY', 'MONTHLY', 'QUARTERLY', 'YEARLY');

-- CreateEnum
CREATE TYPE "ErpRecurringStatus" AS ENUM ('ACTIVE', 'PAUSED', 'COMPLETED', 'CANCELLED');

-- CreateEnum
CREATE TYPE "ErpFinancialReportType" AS ENUM ('BALANCE_SHEET', 'INCOME_STATEMENT', 'CASH_FLOW', 'CUSTOM');

-- CreateEnum
CREATE TYPE "ErpReportLineType" AS ENUM ('ACCOUNTS', 'FORMULA', 'SECTION_TOTAL', 'HEADER', 'SPACER');

-- CreateEnum
CREATE TYPE "ErpCreditLimitAction" AS ENUM ('WARN', 'BLOCK', 'REQUIRE_APPROVAL');

-- CreateEnum
CREATE TYPE "ErpCollectionActivityType" AS ENUM ('PHONE_CALL', 'EMAIL', 'VISIT', 'LETTER', 'LEGAL');

-- CreateEnum
CREATE TYPE "ErpCollectionStatus" AS ENUM ('OPEN', 'IN_PROGRESS', 'RESOLVED', 'ESCALATED');

-- CreateEnum
CREATE TYPE "ErpDunningLevel" AS ENUM ('LEVEL_1', 'LEVEL_2', 'LEVEL_3', 'LEGAL');

-- CreateEnum
CREATE TYPE "ErpIntercompanyStatus" AS ENUM ('PENDING_MATCH', 'MATCHED', 'ELIMINATED');

-- CreateEnum
CREATE TYPE "ErpStockMovementType" AS ENUM ('REQUEST', 'ISSUE', 'TRANSFER', 'TRANSFER_RECEIPT', 'RETURN');

-- CreateEnum
CREATE TYPE "ErpStockCountType" AS ENUM ('FULL', 'CYCLE', 'SPOT');

-- CreateEnum
CREATE TYPE "ErpAdjustmentDirection" AS ENUM ('INCREASE', 'DECREASE');

-- CreateEnum
CREATE TYPE "ErpLotStatus" AS ENUM ('ACTIVE', 'QUARANTINE', 'EXPIRED', 'BLOCKED');

-- CreateEnum
CREATE TYPE "ErpSerialStatus" AS ENUM ('IN_STOCK', 'ISSUED', 'RETURNED', 'SCRAPPED');

-- CreateEnum
CREATE TYPE "ErpReservationStatus" AS ENUM ('ACTIVE', 'FULFILLED', 'RELEASED', 'EXPIRED');

-- CreateEnum
CREATE TYPE "ErpCostRecalcStatus" AS ENUM ('PENDING', 'COMPLETED', 'FAILED');

-- CreateEnum
CREATE TYPE "ErpPurchaseDocType" AS ENUM ('REQUISITION', 'RFQ', 'QUOTATION', 'BID_SELECTION', 'ORDER', 'GOODS_RECEIPT', 'INVOICE', 'RETURN');

-- CreateEnum
CREATE TYPE "ErpPurchaseReturnType" AS ENUM ('DEBIT_NOTE', 'RETURN_TO_VENDOR');

-- CreateEnum
CREATE TYPE "ErpPriceMode" AS ENUM ('TAX_INCLUSIVE', 'TAX_EXCLUSIVE');

-- CreateEnum
CREATE TYPE "ErpQcStatus" AS ENUM ('PENDING', 'PASSED', 'FAILED', 'PARTIAL');

-- CreateEnum
CREATE TYPE "ErpMatchStatus" AS ENUM ('PENDING', 'MATCHED', 'MISMATCH', 'WAIVED');

-- CreateEnum
CREATE TYPE "ErpSalesDocType" AS ENUM ('QUOTATION', 'ORDER', 'PROFORMA_INVOICE', 'PACKING_LIST', 'DELIVERY_ORDER', 'DELIVERY_REPORT', 'INVOICE', 'RETURN', 'RETURN_RECEIPT');

-- CreateEnum
CREATE TYPE "ErpSalesChannel" AS ENUM ('STANDARD', 'POS');

-- CreateEnum
CREATE TYPE "erp_mfg_doc_type" AS ENUM ('BOM', 'WORK_ORDER', 'MATERIAL_ISSUE', 'MATERIAL_RETURN', 'PRODUCTION', 'REWORK');

-- CreateEnum
CREATE TYPE "ErpDepreciationMethod" AS ENUM ('STRAIGHT_LINE', 'DECLINING_BALANCE', 'DOUBLE_DECLINING', 'SUM_OF_YEARS', 'UNITS_OF_PRODUCTION', 'NONE');

-- CreateEnum
CREATE TYPE "ErpFaDocType" AS ENUM ('REQUISITION', 'QUOTATION', 'ORDER', 'ACQUISITION', 'REGISTRATION', 'DEPRECIATION', 'TRANSFER', 'DISPOSAL');

-- CreateEnum
CREATE TYPE "ErpAssetMovementType" AS ENUM ('ACQUISITION', 'DEPRECIATION', 'REVALUATION', 'TRANSFER', 'DISPOSAL', 'ADJUSTMENT');

-- CreateEnum
CREATE TYPE "ErpPromotionType" AS ENUM ('BONUS', 'SUBSTITUTION', 'ADDITIONAL_ITEM', 'DISCOUNT', 'VOUCHER');

-- CreateEnum
CREATE TYPE "ErpDiscountScope" AS ENUM ('ITEM', 'ITEM_CATEGORY', 'CUSTOMER_CATEGORY');

-- CreateEnum
CREATE TYPE "ErpPointTransactionType" AS ENUM ('EARN', 'REDEEM', 'ADJUST');

-- CreateEnum
CREATE TYPE "ErpVoucherStatus" AS ENUM ('ISSUED', 'REDEEMED', 'EXPIRED', 'VOID');

-- CreateEnum
CREATE TYPE "ErpMrpRunStatus" AS ENUM ('DRAFT', 'PROCESSING', 'COMPLETED', 'FAILED');

-- CreateEnum
CREATE TYPE "ErpLotSizeMethod" AS ENUM ('LOT_FOR_LOT', 'FIXED_QTY', 'EOQ', 'MIN_MAX');

-- CreateEnum
CREATE TYPE "ErpReplenishmentSource" AS ENUM ('PURCHASE', 'MANUFACTURE', 'TRANSFER');

-- CreateEnum
CREATE TYPE "ErpSuggestionStatus" AS ENUM ('PENDING', 'APPROVED', 'REJECTED', 'CONVERTED');

-- CreateTable
CREATE TABLE "md_cost_centers" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_cost_centers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_divisions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_divisions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_subdivisions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "division_id" BIGINT NOT NULL,
    "parent_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_subdivisions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "md_projects" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_id" BIGINT,
    "start_date" DATE,
    "end_date" DATE,
    "branch_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_projects_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_journal_entries" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "journal_type" "ErpJournalType" NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source" TEXT,
    "entry_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "partner_id" BIGINT,
    "contact_person" TEXT,
    "description" TEXT NOT NULL,
    "notes" TEXT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "posted_by_id" BIGINT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_journal_entries_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_journal_lines" (
    "id" BIGSERIAL NOT NULL,
    "journal_entry_id" BIGINT NOT NULL,
    "account_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "debit" DECIMAL(19,4) NOT NULL,
    "credit" DECIMAL(19,4) NOT NULL,
    "debit_fx" DECIMAL(19,4),
    "credit_fx" DECIMAL(19,4),
    "notes" TEXT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_journal_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_ledger_entries" (
    "id" BIGSERIAL NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source" TEXT NOT NULL,
    "source_doc_type" TEXT,
    "source_id" BIGINT,
    "doc_number" TEXT NOT NULL,
    "entry_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "partner_id" BIGINT,
    "account_id" BIGINT NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "reference_no" TEXT,
    "debit" DECIMAL(19,4) NOT NULL,
    "credit" DECIMAL(19,4) NOT NULL,
    "debit_fx" DECIMAL(19,4),
    "credit_fx" DECIMAL(19,4),
    "payment_method" "ErpPaymentMethod",
    "ar_ap_type" "ErpArApType",
    "ar_ap_ref" TEXT,
    "due_date" DATE,
    "settled_date" DATE,
    "settlement_status" "ErpSettlementStatus",
    "reconciled_at" DATE,
    "reconciliation_status" "ErpReconciliationStatus" NOT NULL,
    "is_opening_balance" BOOLEAN NOT NULL DEFAULT false,
    "is_adjustment" BOOLEAN NOT NULL DEFAULT false,
    "is_retail" BOOLEAN NOT NULL DEFAULT false,
    "recosted_at" TIMESTAMPTZ(6),
    "recosted_by_id" BIGINT,
    "recosted_by_run_id" BIGINT,
    "group" TEXT,
    "cash_flow_category" "ErpCashFlowCategory",
    "budget_realized_amount" DECIMAL(19,4),
    "realization_status" TEXT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "line_no" INTEGER NOT NULL,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_ledger_entries_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_cash_bank_transactions" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "direction" "ErpCashBankDirection" NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source" TEXT,
    "transaction_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "bank_account_id" BIGINT NOT NULL,
    "partner_id" BIGINT,
    "contact_person" TEXT,
    "description" TEXT NOT NULL,
    "notes" TEXT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "paid_amount" DECIMAL(19,4),
    "payment_status" "ErpSettlementStatus",
    "settled_date" DATE,
    "budget_date" DATE,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_cash_bank_transactions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_cash_bank_lines" (
    "id" BIGSERIAL NOT NULL,
    "cash_bank_transaction_id" BIGINT NOT NULL,
    "account_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "notes" TEXT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_cash_bank_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_ar_receipts" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source" TEXT,
    "transaction_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "contact_person" TEXT,
    "bank_account_id" BIGINT,
    "description" TEXT NOT NULL,
    "notes" TEXT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "allocated_amount" DECIMAL(19,4) NOT NULL,
    "payment_status" "ErpSettlementStatus" NOT NULL,
    "settled_date" DATE,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_ar_receipts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_ap_payments" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source" TEXT,
    "transaction_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "contact_person" TEXT,
    "bank_account_id" BIGINT,
    "description" TEXT NOT NULL,
    "notes" TEXT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "allocated_amount" DECIMAL(19,4) NOT NULL,
    "payment_status" "ErpSettlementStatus" NOT NULL,
    "settled_date" DATE,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_ap_payments_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_payment_instruments" (
    "id" BIGSERIAL NOT NULL,
    "ar_receipt_id" BIGINT,
    "ap_payment_id" BIGINT,
    "method" "ErpPaymentMethod" NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "giro_id" BIGINT,
    "due_date" DATE,
    "bank_name" TEXT,
    "bank_account_no" TEXT,
    "bank_account_id" BIGINT,
    "giro_account_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_payment_instruments_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_settlement_allocations" (
    "id" BIGSERIAL NOT NULL,
    "ar_receipt_id" BIGINT,
    "ap_payment_id" BIGINT,
    "ledger_entry_id" BIGINT NOT NULL,
    "invoice_ref" TEXT,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_settlement_allocations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_giros" (
    "id" BIGSERIAL NOT NULL,
    "giro_number" TEXT NOT NULL,
    "type" "ErpGiroType" NOT NULL,
    "source" TEXT,
    "source_transaction_id" BIGINT,
    "partner_id" BIGINT,
    "branch_id" BIGINT,
    "fiscal_period_id" BIGINT,
    "bank_name" TEXT,
    "bank_account_no" TEXT,
    "bank_account_id" BIGINT,
    "giro_account_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4),
    "due_date" DATE NOT NULL,
    "cleared_date" DATE,
    "status" "ErpGiroStatus" NOT NULL,
    "previous_status" "ErpGiroStatus",
    "description" TEXT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_giros_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_budget_realizations" (
    "id" BIGSERIAL NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "account_id" BIGINT NOT NULL,
    "debit_total" DECIMAL(19,4) NOT NULL,
    "credit_total" DECIMAL(19,4) NOT NULL,
    "budget_amount" DECIMAL(19,4) NOT NULL,
    "branch_id" BIGINT,
    "location_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_budget_realizations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_period_closings" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "status" "ErpPeriodCloseStatus" NOT NULL,
    "retained_earnings_account_id" BIGINT NOT NULL,
    "closing_journal_entry_id" BIGINT,
    "net_amount" DECIMAL(19,4),
    "started_at" TIMESTAMPTZ(6),
    "completed_at" TIMESTAMPTZ(6),
    "failed_at" TIMESTAMPTZ(6),
    "failure_reason" TEXT,
    "reopened_at" TIMESTAMPTZ(6),
    "reopened_by_id" BIGINT,
    "reopen_reason" TEXT,
    "notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_period_closings_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_posting_rules" (
    "id" BIGSERIAL NOT NULL,
    "name" TEXT NOT NULL,
    "module" TEXT NOT NULL,
    "event_type" "ErpPostingEvent" NOT NULL,
    "branch_id" BIGINT,
    "item_category_id" BIGINT,
    "partner_category_id" BIGINT,
    "tax_id" BIGINT,
    "priority" INTEGER NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_posting_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_posting_rule_lines" (
    "id" BIGSERIAL NOT NULL,
    "rule_id" BIGINT NOT NULL,
    "leg_name" TEXT NOT NULL,
    "account_id" BIGINT NOT NULL,
    "is_debit" BOOLEAN NOT NULL,
    "description" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_posting_rule_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_tax_entries" (
    "id" BIGSERIAL NOT NULL,
    "module" TEXT NOT NULL,
    "source_doc_type" TEXT,
    "source_id" BIGINT,
    "doc_number" TEXT NOT NULL,
    "transaction_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "partner_id" BIGINT,
    "partner_npwp" TEXT,
    "partner_name" TEXT,
    "tax_id" BIGINT NOT NULL,
    "tax_entry_type" "ErpTaxEntryType" NOT NULL,
    "dpp" DECIMAL(19,4) NOT NULL,
    "tax_rate" DECIMAL(9,4) NOT NULL,
    "tax_amount" DECIMAL(19,4) NOT NULL,
    "faktur_number" TEXT,
    "faktur_date" DATE,
    "status" "ErpTaxEntryStatus" NOT NULL,
    "reported_period_id" BIGINT,
    "reported_at" TIMESTAMPTZ(6),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "tax_amount_fx" DECIMAL(19,4),
    "ledger_entry_id" BIGINT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_tax_entries_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_withholding_tax_certificates" (
    "id" BIGSERIAL NOT NULL,
    "cert_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "pph_type" "ErpTaxEntryType" NOT NULL,
    "transaction_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "partner_npwp" TEXT,
    "partner_name" TEXT NOT NULL,
    "dpp" DECIMAL(19,4) NOT NULL,
    "rate" DECIMAL(9,4) NOT NULL,
    "amount_withheld" DECIMAL(19,4) NOT NULL,
    "status" "ErpWhtCertStatus" NOT NULL,
    "tax_entry_id" BIGINT,
    "source_doc_type" TEXT,
    "source_id" BIGINT,
    "notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_withholding_tax_certificates_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_fx_revaluation_runs" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "revaluation_date" DATE NOT NULL,
    "status" "ErpFxRevaluationStatus" NOT NULL,
    "total_gain_loss" DECIMAL(19,4),
    "gain_account_id" BIGINT NOT NULL,
    "loss_account_id" BIGINT NOT NULL,
    "closing_journal_entry_id" BIGINT,
    "started_at" TIMESTAMPTZ(6),
    "completed_at" TIMESTAMPTZ(6),
    "failed_at" TIMESTAMPTZ(6),
    "failure_reason" TEXT,
    "notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_fx_revaluation_runs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_fx_revaluation_lines" (
    "id" BIGSERIAL NOT NULL,
    "revaluation_run_id" BIGINT NOT NULL,
    "account_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "book_balance_fx" DECIMAL(19,4) NOT NULL,
    "book_balance_idr" DECIMAL(19,4) NOT NULL,
    "revaluation_rate" DECIMAL(19,6) NOT NULL,
    "revalued_balance_idr" DECIMAL(19,4) NOT NULL,
    "gain_loss_amount" DECIMAL(19,4) NOT NULL,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_fx_revaluation_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_bank_statements" (
    "id" BIGSERIAL NOT NULL,
    "bank_account_id" BIGINT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "statement_number" TEXT,
    "period_start" DATE NOT NULL,
    "period_end" DATE NOT NULL,
    "opening_balance" DECIMAL(19,4) NOT NULL,
    "closing_balance" DECIMAL(19,4) NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "status" "ErpBankStatementStatus" NOT NULL,
    "imported_at" TIMESTAMPTZ(6) NOT NULL,
    "notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_bank_statements_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_bank_statement_lines" (
    "id" BIGSERIAL NOT NULL,
    "statement_id" BIGINT NOT NULL,
    "value_date" DATE NOT NULL,
    "description" TEXT,
    "reference_no" TEXT,
    "debit" DECIMAL(19,4) NOT NULL,
    "credit" DECIMAL(19,4) NOT NULL,
    "running_balance" DECIMAL(19,4),
    "matched_ledger_entry_id" BIGINT,
    "matched_cash_bank_transaction_id" BIGINT,
    "reconciliation_status" "ErpReconciliationStatus" NOT NULL,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_bank_statement_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_recurring_journal_templates" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "journal_type" "ErpJournalType" NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "frequency" "ErpRecurringFrequency" NOT NULL,
    "start_date" DATE NOT NULL,
    "end_date" DATE,
    "next_run_date" DATE NOT NULL,
    "max_occurrences" INTEGER,
    "occurrence_count" INTEGER NOT NULL DEFAULT 0,
    "status" "ErpRecurringStatus" NOT NULL,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_recurring_journal_templates_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_recurring_journal_template_lines" (
    "id" BIGSERIAL NOT NULL,
    "template_id" BIGINT NOT NULL,
    "account_id" BIGINT NOT NULL,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "project_id" BIGINT,
    "debit" DECIMAL(19,4) NOT NULL,
    "credit" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "fin_recurring_journal_template_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_accrual_schedules" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "description" TEXT,
    "total_amount" DECIMAL(19,4) NOT NULL,
    "start_date" DATE NOT NULL,
    "end_date" DATE NOT NULL,
    "frequency" "ErpRecurringFrequency" NOT NULL,
    "prepaid_account_id" BIGINT NOT NULL,
    "expense_account_id" BIGINT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "cost_center_id" BIGINT,
    "amount_per_period" DECIMAL(19,4) NOT NULL,
    "recognized_amount" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "remaining_amount" DECIMAL(19,4) NOT NULL,
    "status" "ErpRecurringStatus" NOT NULL,
    "source_doc_type" TEXT,
    "source_id" BIGINT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_accrual_schedules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_report_definitions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "report_type" "ErpFinancialReportType" NOT NULL,
    "description" TEXT,
    "is_default" BOOLEAN NOT NULL DEFAULT false,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "branch_id" BIGINT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_report_definitions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_report_sections" (
    "id" BIGSERIAL NOT NULL,
    "report_id" BIGINT NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "parent_section_id" BIGINT,
    "sort_order" INTEGER NOT NULL,
    "normal_balance" "ErpNormalBalance",
    "is_total_row" BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT "fin_report_sections_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_report_lines" (
    "id" BIGSERIAL NOT NULL,
    "section_id" BIGINT NOT NULL,
    "code" TEXT,
    "label" TEXT NOT NULL,
    "line_type" "ErpReportLineType" NOT NULL,
    "account_from" TEXT,
    "account_to" TEXT,
    "specific_account_ids" BIGINT[],
    "formula" TEXT,
    "sort_order" INTEGER NOT NULL,
    "indent_level" INTEGER NOT NULL DEFAULT 0,
    "is_negated" BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT "fin_report_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_credit_limits" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "limit_amount" DECIMAL(19,4) NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "action" "ErpCreditLimitAction" NOT NULL,
    "override_role_id" BIGINT,
    "valid_from" DATE,
    "valid_to" DATE,
    "review_date" DATE,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_credit_limits_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_dunning_rules" (
    "id" BIGSERIAL NOT NULL,
    "name" TEXT NOT NULL,
    "overdue_days_from" INTEGER NOT NULL,
    "overdue_days_to" INTEGER,
    "dunning_level" "ErpDunningLevel" NOT NULL,
    "message_template" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "sort_order" INTEGER NOT NULL,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_dunning_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_collection_activities" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "activity_type" "ErpCollectionActivityType" NOT NULL,
    "activity_date" DATE NOT NULL,
    "due_ledger_entry_id" BIGINT,
    "dunning_rule_id" BIGINT,
    "dunning_level" "ErpDunningLevel",
    "status" "ErpCollectionStatus" NOT NULL,
    "assigned_to_id" BIGINT,
    "notes" TEXT NOT NULL,
    "follow_up_date" DATE,
    "resolved_at" TIMESTAMPTZ(6),
    "resolved_by_id" BIGINT,
    "resolved_notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_collection_activities_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_intercompany_rules" (
    "id" BIGSERIAL NOT NULL,
    "from_branch_id" BIGINT NOT NULL,
    "to_branch_id" BIGINT NOT NULL,
    "due_from_account_id" BIGINT NOT NULL,
    "due_to_account_id" BIGINT NOT NULL,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_intercompany_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fin_intercompany_transactions" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "transaction_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "rule_id" BIGINT,
    "from_branch_id" BIGINT NOT NULL,
    "to_branch_id" BIGINT NOT NULL,
    "from_journal_entry_id" BIGINT,
    "to_journal_entry_id" BIGINT,
    "amount" DECIMAL(19,4) NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "description" TEXT NOT NULL,
    "status" "ErpIntercompanyStatus" NOT NULL,
    "eliminated_at" TIMESTAMPTZ(6),
    "eliminated_by_id" BIGINT,
    "consolidation_period_id" BIGINT,
    "notes" TEXT,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "fin_intercompany_transactions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_movements" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "movement_type" "ErpStockMovementType" NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "transit_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "source" TEXT,
    "movement_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "requested_to" TEXT,
    "needed_date" DATE,
    "related_movement_id" BIGINT,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_stock_movements_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_movement_lines" (
    "id" BIGSERIAL NOT NULL,
    "stock_movement_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT,
    "exchange_rate" DECIMAL(19,6),
    "unit_cost" DECIMAL(19,4),
    "sale_price" DECIMAL(19,4),
    "source_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "related_line_id" BIGINT,
    "bin_id" BIGINT,
    "lot_id" BIGINT,
    "serial_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "inv_stock_movement_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_opening_stocks" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT NOT NULL,
    "kind" TEXT,
    "opening_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_opening_stocks_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_opening_stock_lines" (
    "id" BIGSERIAL NOT NULL,
    "opening_stock_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "inventory_account_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "bin_id" BIGINT,
    "lot_id" BIGINT,
    "serial_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "inv_opening_stock_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_counts" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "count_type" "ErpStockCountType" NOT NULL,
    "count_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "step_no" INTEGER,
    "description" TEXT,
    "notes" TEXT,
    "adjustment_status" "ErpDocumentStatus",
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_stock_counts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_count_lines" (
    "id" BIGSERIAL NOT NULL,
    "stock_count_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "system_qty" DECIMAL(19,4) NOT NULL,
    "physical_qty" DECIMAL(19,4) NOT NULL,
    "good_qty" DECIMAL(19,4) NOT NULL,
    "damaged_qty" DECIMAL(19,4) NOT NULL,
    "variance_qty" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "bin_id" BIGINT,
    "lot_id" BIGINT,
    "serial_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "inv_stock_count_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_adjustments" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "adjustment_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "kind" TEXT,
    "stock_count_id" BIGINT,
    "description" TEXT,
    "notes" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_stock_adjustments_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_adjustment_lines" (
    "id" BIGSERIAL NOT NULL,
    "stock_adjustment_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "direction" "ErpAdjustmentDirection" NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "inventory_account_id" BIGINT NOT NULL,
    "contra_account_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "count_line_id" BIGINT,
    "bin_id" BIGINT,
    "lot_id" BIGINT,
    "serial_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "inv_stock_adjustment_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_weighbridge_tickets" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "ticket_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "vehicle_plate" TEXT,
    "driver_name" TEXT,
    "partner_id" BIGINT,
    "item_id" BIGINT,
    "gross_at" TIMESTAMPTZ(6),
    "gross_weight" DECIMAL(19,4) NOT NULL,
    "tare_at" TIMESTAMPTZ(6),
    "tare_weight" DECIMAL(19,4) NOT NULL,
    "net_weight" DECIMAL(19,4) NOT NULL,
    "unit_price" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_weighbridge_tickets_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_cost_recalculations" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "legacy_code" TEXT,
    "costing_method" "ErpCostingMethod" NOT NULL,
    "trigger_type" TEXT NOT NULL,
    "trigger_source_doc_type" TEXT,
    "trigger_source_id" BIGINT,
    "item_id" BIGINT,
    "warehouse_id" BIGINT,
    "from_date" DATE NOT NULL,
    "to_date" DATE,
    "fiscal_period_id" BIGINT NOT NULL,
    "status" "ErpCostRecalcStatus" NOT NULL,
    "total_delta" DECIMAL(19,4),
    "started_at" TIMESTAMPTZ(6),
    "completed_at" TIMESTAMPTZ(6),
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_cost_recalculations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_cost_recalculation_lines" (
    "id" BIGSERIAL NOT NULL,
    "cost_recalculation_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "ledger_entry_id" BIGINT,
    "old_unit_cost" DECIMAL(19,4) NOT NULL,
    "new_unit_cost" DECIMAL(19,4) NOT NULL,
    "affected_qty" DECIMAL(19,4) NOT NULL,
    "old_debit" DECIMAL(19,4),
    "old_credit" DECIMAL(19,4),
    "new_debit" DECIMAL(19,4),
    "new_credit" DECIMAL(19,4),
    "delta_amount" DECIMAL(19,4) NOT NULL,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "inv_cost_recalculation_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_bins" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT,
    "legacy_code" TEXT,
    "warehouse_id" BIGINT NOT NULL,
    "bin_type" TEXT,
    "sequence" INTEGER,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_bins_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_lots" (
    "id" BIGSERIAL NOT NULL,
    "lot_number" TEXT NOT NULL,
    "legacy_code" TEXT,
    "item_id" BIGINT NOT NULL,
    "supplier_lot_no" TEXT,
    "manufacture_date" DATE,
    "expiry_date" DATE,
    "origin_goods_receipt_id" BIGINT,
    "status" "ErpLotStatus" NOT NULL,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_lots_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_serials" (
    "id" BIGSERIAL NOT NULL,
    "serial_number" TEXT NOT NULL,
    "legacy_code" TEXT,
    "item_id" BIGINT NOT NULL,
    "lot_id" BIGINT,
    "current_warehouse_id" BIGINT,
    "current_bin_id" BIGINT,
    "status" "ErpSerialStatus" NOT NULL,
    "origin_goods_receipt_id" BIGINT,
    "last_movement_id" BIGINT,
    "warranty_until" DATE,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_serials_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "inv_stock_reservations" (
    "id" BIGSERIAL NOT NULL,
    "legacy_code" TEXT,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "bin_id" BIGINT,
    "lot_id" BIGINT,
    "quantity" DECIMAL(19,4) NOT NULL,
    "source_doc_type" TEXT NOT NULL,
    "source_doc_id" BIGINT NOT NULL,
    "source_line_id" BIGINT,
    "status" "ErpReservationStatus" NOT NULL,
    "expires_at" TIMESTAMPTZ(6),
    "fulfilled_by_movement_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "inv_stock_reservations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_requisitions" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "requested_by_id" BIGINT NOT NULL,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "requested_to" TEXT,
    "valid_from" DATE,
    "valid_to" DATE,
    "sales_quotation_id" BIGINT,
    "work_order_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_requisitions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_requisition_lines" (
    "id" BIGSERIAL NOT NULL,
    "requisition_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "accrued_payable_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_requisition_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_rfqs" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "requisition_id" BIGINT,
    "valid_from" DATE,
    "valid_to" DATE,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_rfqs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_rfq_suppliers" (
    "id" BIGSERIAL NOT NULL,
    "rfq_id" BIGINT NOT NULL,
    "supplier_id" BIGINT NOT NULL,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_rfq_suppliers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_quotations" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "rfq_id" BIGINT,
    "group_no" TEXT,
    "fulfil_date" DATE,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_quotations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_quotation_lines" (
    "id" BIGSERIAL NOT NULL,
    "quotation_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "accrued_payable_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_quotation_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_bid_selections" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_bid_selections_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_bid_selection_quotations" (
    "id" BIGSERIAL NOT NULL,
    "bid_selection_id" BIGINT NOT NULL,
    "quotation_id" BIGINT NOT NULL,
    "rank" INTEGER NOT NULL,

    CONSTRAINT "pur_bid_selection_quotations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_bid_selection_lines" (
    "id" BIGSERIAL NOT NULL,
    "bid_selection_id" BIGINT NOT NULL,
    "quotation_line_id" BIGINT NOT NULL,
    "selected" BOOLEAN NOT NULL DEFAULT false,
    "price_rank" INTEGER NOT NULL,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_bid_selection_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_orders" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "requisition_id" BIGINT,
    "quotation_id" BIGINT,
    "bid_selection_id" BIGINT,
    "fulfil_date" DATE,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_order_lines" (
    "id" BIGSERIAL NOT NULL,
    "order_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "accrued_payable_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_order_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_goods_receipts" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "order_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_goods_receipts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_goods_receipt_lines" (
    "id" BIGSERIAL NOT NULL,
    "goods_receipt_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "accepted_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "rejected_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "quarantine_qty" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "qc_status" "ErpQcStatus" NOT NULL DEFAULT 'PENDING',
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "accrued_payable_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "order_line_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_goods_receipt_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_invoices" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "tax_invoice_no" TEXT,
    "tax_paid" BOOLEAN NOT NULL DEFAULT false,
    "tax_paid_date" DATE,
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "settled_date" DATE,
    "advance_amount" DECIMAL(19,4),
    "advance_account_id" BIGINT,
    "is_opening_balance" BOOLEAN NOT NULL DEFAULT false,
    "match_status" "ErpMatchStatus" NOT NULL DEFAULT 'PENDING',
    "order_id" BIGINT,
    "goods_receipt_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_invoices_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_invoice_lines" (
    "id" BIGSERIAL NOT NULL,
    "invoice_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "accrued_payable_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "order_line_id" BIGINT,
    "goods_receipt_line_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_invoice_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_returns" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "supplier_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "payable_account_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "return_type" "ErpPurchaseReturnType" NOT NULL,
    "tax_invoice_no" TEXT,
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "settled_date" DATE,
    "return_purchase_account_id" BIGINT,
    "cogs_account_id" BIGINT,
    "is_opening_balance" BOOLEAN NOT NULL DEFAULT false,
    "order_id" BIGINT,
    "goods_receipt_id" BIGINT,
    "invoice_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pur_returns_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pur_return_lines" (
    "id" BIGSERIAL NOT NULL,
    "return_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "accrued_payable_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "order_line_id" BIGINT,
    "goods_receipt_line_id" BIGINT,
    "invoice_line_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pur_return_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_quotations" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "purchase_requisition_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_quotations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_quotation_lines" (
    "id" BIGSERIAL NOT NULL,
    "quotation_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_quotation_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_quotation_materials" (
    "id" BIGSERIAL NOT NULL,
    "quotation_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "cost_price" DECIMAL(19,4) NOT NULL,
    "sale_price" DECIMAL(19,4) NOT NULL,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_quotation_materials_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_orders" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "quotation_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_order_lines" (
    "id" BIGSERIAL NOT NULL,
    "order_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_order_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_proforma_invoices" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "quotation_id" BIGINT,
    "order_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_proforma_invoices_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_proforma_invoice_lines" (
    "id" BIGSERIAL NOT NULL,
    "proforma_invoice_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_proforma_invoice_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_packing_lists" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "packing_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "quotation_id" BIGINT,
    "order_id" BIGINT,
    "proforma_invoice_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_packing_lists_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_packing_list_lines" (
    "id" BIGSERIAL NOT NULL,
    "packing_list_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_packing_list_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_packing_list_packs" (
    "id" BIGSERIAL NOT NULL,
    "packing_list_id" BIGINT NOT NULL,
    "pack_no" TEXT NOT NULL,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_packing_list_packs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_delivery_orders" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "shipping_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "quotation_id" BIGINT,
    "order_id" BIGINT,
    "proforma_invoice_id" BIGINT,
    "packing_list_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_delivery_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_delivery_order_lines" (
    "id" BIGSERIAL NOT NULL,
    "delivery_order_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_delivery_order_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_delivery_reports" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "delivery_order_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_delivery_reports_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_delivery_report_lines" (
    "id" BIGSERIAL NOT NULL,
    "delivery_report_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_delivery_report_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoices" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "channel" "ErpSalesChannel" NOT NULL DEFAULT 'STANDARD',
    "tax_invoice_no" TEXT,
    "tax_paid" BOOLEAN NOT NULL DEFAULT false,
    "tax_paid_date" DATE,
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "settled_date" DATE,
    "advance_amount" DECIMAL(19,4),
    "advance_account_id" BIGINT,
    "is_opening_balance" BOOLEAN NOT NULL DEFAULT false,
    "swap_status" "ErpDocumentStatus",
    "swap_date" DATE,
    "points_earned" DECIMAL(19,4),
    "points_redeemed" DECIMAL(19,4),
    "quotation_id" BIGINT,
    "order_id" BIGINT,
    "advance_id" BIGINT,
    "proforma_invoice_id" BIGINT,
    "delivery_order_id" BIGINT,
    "ar_ledger_entry_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_invoices_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoice_lines" (
    "id" BIGSERIAL NOT NULL,
    "invoice_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_invoice_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoice_installments" (
    "id" BIGSERIAL NOT NULL,
    "invoice_id" BIGINT NOT NULL,
    "installment_no" INTEGER NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "paid_amount" DECIMAL(19,4) NOT NULL,
    "due_date" DATE NOT NULL,
    "settled_date" DATE,
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "receivable_account_id" BIGINT NOT NULL,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_invoice_installments_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoice_materials" (
    "id" BIGSERIAL NOT NULL,
    "invoice_id" BIGINT NOT NULL,
    "invoice_line_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "source_warehouse_id" BIGINT,
    "transit_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,

    CONSTRAINT "sls_invoice_materials_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoice_costs" (
    "id" BIGSERIAL NOT NULL,
    "invoice_id" BIGINT NOT NULL,
    "partner_id" BIGINT,
    "amount" DECIMAL(19,4) NOT NULL,
    "metadata" JSONB,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_invoice_costs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_returns" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "remaining_account_id" BIGINT,
    "is_opening_balance" BOOLEAN NOT NULL DEFAULT false,
    "swap_status" "ErpDocumentStatus",
    "swap_date" DATE,
    "invoice_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_returns_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_return_lines" (
    "id" BIGSERIAL NOT NULL,
    "return_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_return_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_return_receipts" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "customer_contact_id" BIGINT,
    "payment_term_id" BIGINT,
    "due_date" DATE,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "price_mode" "ErpPriceMode" NOT NULL,
    "subtotal" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_amount" DECIMAL(19,4),
    "tax2_amount" DECIMAL(19,4),
    "other_cost_percent" DECIMAL(9,4),
    "other_cost_amount" DECIMAL(19,4),
    "grand_total" DECIMAL(19,4) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "closed_date" DATE,
    "discount_account_id" BIGINT,
    "tax1_account_id" BIGINT,
    "tax2_account_id" BIGINT,
    "other_cost_account_id" BIGINT,
    "receivable_account_id" BIGINT,
    "expedition_id" BIGINT,
    "ship_date" DATE,
    "sales_dept_id" BIGINT,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "tax_invoice_no" TEXT,
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "invoice_id" BIGINT,
    "return_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_return_receipts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_return_receipt_lines" (
    "id" BIGSERIAL NOT NULL,
    "return_receipt_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "fixed_price" DECIMAL(19,4),
    "discount_percent" DECIMAL(9,4),
    "discount_amount" DECIMAL(19,4),
    "tax1_id" BIGINT,
    "tax1_amount" DECIMAL(19,4),
    "tax2_id" BIGINT,
    "tax2_amount" DECIMAL(19,4),
    "unit_cost" DECIMAL(19,4),
    "warehouse_id" BIGINT,
    "inventory_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "source_line_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_return_receipt_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_customer_advances" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "branch_id" BIGINT NOT NULL,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "payment_term_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "amount" DECIMAL(19,4) NOT NULL,
    "amount_fx" DECIMAL(19,4) NOT NULL,
    "applied_amount" DECIMAL(19,4) NOT NULL DEFAULT 0,
    "settlement_status" "ErpSettlementStatus" NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "project_id" BIGINT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "order_id" BIGINT,
    "ar_receipt_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_customer_advances_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoice_swaps" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "customer_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_invoice_swaps_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_invoice_swap_lines" (
    "id" BIGSERIAL NOT NULL,
    "swap_id" BIGINT NOT NULL,
    "from_invoice_id" BIGINT NOT NULL,
    "to_invoice_id" BIGINT,
    "amount" DECIMAL(19,4) NOT NULL,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_invoice_swap_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_forecasts" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "doc_number" TEXT NOT NULL,
    "forecast_date" DATE NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "customer_id" BIGINT NOT NULL,
    "sales_dept_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "description" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "sls_forecasts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "sls_forecast_lines" (
    "id" BIGSERIAL NOT NULL,
    "forecast_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "fiscal_period_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "sls_forecast_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_boms" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "doc_type" "erp_mfg_doc_type" NOT NULL DEFAULT 'BOM',
    "kind" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "work_estimate" DECIMAL(19,4),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "input_total_price" DECIMAL(19,4),
    "output_total_price" DECIMAL(19,4),
    "input_total_cost" DECIMAL(19,4),
    "output_total_cost" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "mfg_boms_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_bom_inputs" (
    "id" BIGSERIAL NOT NULL,
    "bom_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_percent" DECIMAL(9,4),
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_bom_inputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_bom_outputs" (
    "id" BIGSERIAL NOT NULL,
    "bom_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_layer_in_id" BIGINT,
    "cost_layer_fifo_id" BIGINT,
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_bom_outputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_work_orders" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "doc_type" "erp_mfg_doc_type" NOT NULL DEFAULT 'WORK_ORDER',
    "kind" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "work_estimate" DECIMAL(19,4),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "input_total_price" DECIMAL(19,4),
    "output_total_price" DECIMAL(19,4),
    "input_total_cost" DECIMAL(19,4),
    "output_total_cost" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "bom_id" BIGINT,
    "production_rework_id" BIGINT,

    CONSTRAINT "mfg_work_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_work_order_inputs" (
    "id" BIGSERIAL NOT NULL,
    "work_order_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_percent" DECIMAL(9,4),
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "bom_line_id" BIGINT,

    CONSTRAINT "mfg_work_order_inputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_work_order_outputs" (
    "id" BIGSERIAL NOT NULL,
    "work_order_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_layer_in_id" BIGINT,
    "cost_layer_fifo_id" BIGINT,
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "bom_line_id" BIGINT,

    CONSTRAINT "mfg_work_order_outputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_work_order_activities" (
    "id" BIGSERIAL NOT NULL,
    "work_order_id" BIGINT NOT NULL,
    "price_adj_id" BIGINT,
    "activity_name" TEXT NOT NULL,
    "machine_code" TEXT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_work_order_activities_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_work_order_route_cards" (
    "id" BIGSERIAL NOT NULL,
    "work_order_id" BIGINT NOT NULL,
    "doc_number" TEXT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_work_order_route_cards_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_material_issues" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "doc_type" "erp_mfg_doc_type" NOT NULL DEFAULT 'MATERIAL_ISSUE',
    "kind" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "work_estimate" DECIMAL(19,4),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "input_total_price" DECIMAL(19,4),
    "output_total_price" DECIMAL(19,4),
    "input_total_cost" DECIMAL(19,4),
    "output_total_cost" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "bom_id" BIGINT,
    "work_order_id" BIGINT,
    "production_rework_id" BIGINT,
    "inv_stock_movement_id" BIGINT,
    "fin_ledger_entry_id" BIGINT,

    CONSTRAINT "mfg_material_issues_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_material_issue_inputs" (
    "id" BIGSERIAL NOT NULL,
    "material_issue_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_percent" DECIMAL(9,4),
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "work_order_line_id" BIGINT,

    CONSTRAINT "mfg_material_issue_inputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_material_issue_outputs" (
    "id" BIGSERIAL NOT NULL,
    "material_issue_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_layer_in_id" BIGINT,
    "cost_layer_fifo_id" BIGINT,
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "work_order_line_id" BIGINT,

    CONSTRAINT "mfg_material_issue_outputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_material_returns" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "doc_type" "erp_mfg_doc_type" NOT NULL DEFAULT 'MATERIAL_RETURN',
    "kind" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "work_estimate" DECIMAL(19,4),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "input_total_price" DECIMAL(19,4),
    "output_total_price" DECIMAL(19,4),
    "input_total_cost" DECIMAL(19,4),
    "output_total_cost" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "bom_id" BIGINT,
    "work_order_id" BIGINT,
    "material_issue_id" BIGINT,
    "production_rework_id" BIGINT,
    "inv_stock_movement_id" BIGINT,
    "fin_ledger_entry_id" BIGINT,

    CONSTRAINT "mfg_material_returns_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_material_return_inputs" (
    "id" BIGSERIAL NOT NULL,
    "material_return_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_percent" DECIMAL(9,4),
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "material_issue_line_id" BIGINT,

    CONSTRAINT "mfg_material_return_inputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_material_return_outputs" (
    "id" BIGSERIAL NOT NULL,
    "material_return_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_layer_in_id" BIGINT,
    "cost_layer_fifo_id" BIGINT,
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "material_issue_line_id" BIGINT,

    CONSTRAINT "mfg_material_return_outputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_entries" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "doc_type" "erp_mfg_doc_type" NOT NULL DEFAULT 'PRODUCTION',
    "kind" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "work_estimate" DECIMAL(19,4),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "input_total_price" DECIMAL(19,4),
    "output_total_price" DECIMAL(19,4),
    "input_total_cost" DECIMAL(19,4),
    "output_total_cost" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "bom_id" BIGINT,
    "work_order_id" BIGINT,
    "material_issue_id" BIGINT,
    "material_return_id" BIGINT,
    "production_rework_id" BIGINT,
    "inv_stock_movement_id" BIGINT,
    "fin_ledger_entry_id" BIGINT,

    CONSTRAINT "mfg_production_entries_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_entry_inputs" (
    "id" BIGSERIAL NOT NULL,
    "production_entry_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_percent" DECIMAL(9,4),
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "material_issue_line_id" BIGINT,

    CONSTRAINT "mfg_production_entry_inputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_entry_outputs" (
    "id" BIGSERIAL NOT NULL,
    "production_entry_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_layer_in_id" BIGINT,
    "cost_layer_fifo_id" BIGINT,
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,
    "work_order_line_id" BIGINT,

    CONSTRAINT "mfg_production_entry_outputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_boms" (
    "id" BIGSERIAL NOT NULL,
    "production_entry_id" BIGINT NOT NULL,
    "produced_item_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "inventory_account_id" BIGINT,
    "bom_id" BIGINT,
    "bom_output_line_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_production_boms_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_reworks" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "doc_type" "erp_mfg_doc_type" NOT NULL DEFAULT 'REWORK',
    "kind" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "requested_by_id" BIGINT,
    "requested_partner_id" BIGINT,
    "needed_date" DATE,
    "work_estimate" DECIMAL(19,4),
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "input_total_price" DECIMAL(19,4),
    "output_total_price" DECIMAL(19,4),
    "input_total_cost" DECIMAL(19,4),
    "output_total_cost" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "reference_no" TEXT,
    "reference_date" DATE,
    "source_doc_type" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "revision_count" INTEGER NOT NULL DEFAULT 0,
    "print_count" INTEGER NOT NULL DEFAULT 0,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "bom_id" BIGINT,
    "production_entry_id" BIGINT,

    CONSTRAINT "mfg_production_reworks_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_rework_inputs" (
    "id" BIGSERIAL NOT NULL,
    "production_rework_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_percent" DECIMAL(9,4),
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_production_rework_inputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "mfg_production_rework_outputs" (
    "id" BIGSERIAL NOT NULL,
    "production_rework_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT NOT NULL,
    "unit_value" DECIMAL(19,4) NOT NULL,
    "base_quantity" DECIMAL(19,4) NOT NULL,
    "base_unit_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "unit_cost" DECIMAL(19,4) NOT NULL,
    "cost_layer_in_id" BIGINT,
    "cost_layer_fifo_id" BIGINT,
    "inventory_account_id" BIGINT,
    "source_warehouse_id" BIGINT,
    "production_warehouse_id" BIGINT,
    "destination_warehouse_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "notes" TEXT,
    "line_no" INTEGER NOT NULL,
    "metadata" JSONB,

    CONSTRAINT "mfg_production_rework_outputs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "tax_category_id" BIGINT,
    "asset_account_id" BIGINT,
    "accum_depreciation_account_id" BIGINT,
    "depreciation_expense_account_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_category_taxes" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "method" "ErpDepreciationMethod" NOT NULL,
    "useful_life_months" INTEGER NOT NULL,
    "depreciation_table" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_category_taxes_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_depreciation_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_depreciation_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_departments" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "location_id" BIGINT,
    "description" TEXT,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_departments_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_assets" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "category_id" BIGINT NOT NULL,
    "tag_number" TEXT,
    "linked_item_id" BIGINT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "department_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "purchase_date" DATE,
    "in_service_date" DATE,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "acquisition_cost" DECIMAL(19,4) NOT NULL,
    "residual_value" DECIMAL(19,4) NOT NULL,
    "useful_life_months" INTEGER NOT NULL,
    "monthly_depreciation" DECIMAL(19,4) NOT NULL,
    "accumulated_depreciation" DECIMAL(19,4) NOT NULL,
    "book_value" DECIMAL(19,4) NOT NULL,
    "depreciation_count" INTEGER NOT NULL,
    "method" "ErpDepreciationMethod" NOT NULL,
    "depreciation_table" TEXT,
    "is_intangible" BOOLEAN NOT NULL DEFAULT false,
    "is_fiscal" BOOLEAN NOT NULL DEFAULT false,
    "half_month_convention" BOOLEAN NOT NULL DEFAULT false,
    "is_declining_value" BOOLEAN NOT NULL DEFAULT false,
    "asset_account_id" BIGINT,
    "accum_depreciation_account_id" BIGINT,
    "depreciation_expense_account_id" BIGINT,
    "disposal_account_id" BIGINT,
    "manufacturer" TEXT,
    "retirement_date" DATE,
    "is_disposed" BOOLEAN NOT NULL DEFAULT false,
    "is_locked" BOOLEAN NOT NULL DEFAULT false,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "registration_id" BIGINT,

    CONSTRAINT "fa_assets_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_movements" (
    "id" BIGSERIAL NOT NULL,
    "asset_id" BIGINT NOT NULL,
    "movement_type" "ErpAssetMovementType" NOT NULL,
    "source_doc_type" TEXT,
    "source_id" BIGINT,
    "doc_number" TEXT,
    "movement_date" DATE NOT NULL,
    "amount" DECIMAL(19,4),
    "accumulated_after" DECIMAL(19,4),
    "book_value_after" DECIMAL(19,4),
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "status" "ErpDocumentStatus" NOT NULL,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,

    CONSTRAINT "fa_asset_movements_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_requisitions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'REQUISITION',
    "branch_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "requested_by_id" BIGINT,
    "needed_date" DATE,
    "currency_id" BIGINT,
    "exchange_rate" DECIMAL(19,6),
    "price_mode" "ErpPriceMode",
    "description" TEXT,
    "total_amount" DECIMAL(19,4),
    "status" "ErpDocumentStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_requisitions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_requisition_lines" (
    "id" BIGSERIAL NOT NULL,
    "requisition_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "asset_id" BIGINT,
    "asset_name" TEXT,
    "description" TEXT,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "discount_amount" DECIMAL(19,4),
    "tax_amount" DECIMAL(19,4),
    "line_total" DECIMAL(19,4) NOT NULL,
    "asset_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "acquisition_payable_account_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_asset_requisition_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_quotations" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'QUOTATION',
    "branch_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "group_no" TEXT,
    "requisition_id" BIGINT,
    "currency_id" BIGINT,
    "exchange_rate" DECIMAL(19,6),
    "price_mode" "ErpPriceMode",
    "description" TEXT,
    "total_amount" DECIMAL(19,4),
    "status" "ErpDocumentStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_quotations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_quotation_lines" (
    "id" BIGSERIAL NOT NULL,
    "quotation_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "asset_id" BIGINT,
    "asset_name" TEXT,
    "description" TEXT,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "discount_amount" DECIMAL(19,4),
    "tax_amount" DECIMAL(19,4),
    "line_total" DECIMAL(19,4) NOT NULL,
    "asset_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "acquisition_payable_account_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_asset_quotation_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_orders" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'ORDER',
    "branch_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "requisition_id" BIGINT,
    "quotation_id" BIGINT,
    "currency_id" BIGINT,
    "exchange_rate" DECIMAL(19,6),
    "price_mode" "ErpPriceMode",
    "description" TEXT,
    "total_amount" DECIMAL(19,4),
    "status" "ErpDocumentStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_orders_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_order_lines" (
    "id" BIGSERIAL NOT NULL,
    "order_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "asset_id" BIGINT,
    "asset_name" TEXT,
    "description" TEXT,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "discount_amount" DECIMAL(19,4),
    "tax_amount" DECIMAL(19,4),
    "line_total" DECIMAL(19,4) NOT NULL,
    "asset_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "acquisition_payable_account_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_asset_order_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_acquisitions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'ACQUISITION',
    "branch_id" BIGINT NOT NULL,
    "supplier_id" BIGINT,
    "doc_date" DATE NOT NULL,
    "requisition_id" BIGINT,
    "quotation_id" BIGINT,
    "order_id" BIGINT,
    "currency_id" BIGINT,
    "exchange_rate" DECIMAL(19,6),
    "price_mode" "ErpPriceMode",
    "tax_invoice_no" TEXT,
    "description" TEXT,
    "total_amount" DECIMAL(19,4),
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "settlement_status" "ErpSettlementStatus",
    "settled_date" DATE,
    "ap_payment_id" BIGINT,
    "ledger_entry_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_acquisitions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_acquisition_lines" (
    "id" BIGSERIAL NOT NULL,
    "acquisition_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "asset_id" BIGINT,
    "asset_name" TEXT,
    "description" TEXT,
    "quantity" DECIMAL(19,4) NOT NULL,
    "unit_price" DECIMAL(19,4) NOT NULL,
    "discount_amount" DECIMAL(19,4),
    "tax_amount" DECIMAL(19,4),
    "line_total" DECIMAL(19,4) NOT NULL,
    "asset_account_id" BIGINT,
    "purchase_discount_account_id" BIGINT,
    "acquisition_payable_account_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_acquisition_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_registrations" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'REGISTRATION',
    "branch_id" BIGINT NOT NULL,
    "doc_date" DATE NOT NULL,
    "acquisition_id" BIGINT,
    "description" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_asset_registrations_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_asset_registration_lines" (
    "id" BIGSERIAL NOT NULL,
    "registration_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "asset_id" BIGINT,
    "acquisition_cost" DECIMAL(19,4) NOT NULL,
    "residual_value" DECIMAL(19,4),
    "useful_life_months" INTEGER,
    "asset_account_id" BIGINT,
    "accum_depreciation_account_id" BIGINT,
    "depreciation_expense_account_id" BIGINT,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_asset_registration_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_depreciation_runs" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "legacy_code" TEXT,
    "auto_number" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'DEPRECIATION',
    "branch_id" BIGINT NOT NULL,
    "run_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "description" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "ledger_entry_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_depreciation_runs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_depreciation_run_lines" (
    "id" BIGSERIAL NOT NULL,
    "depreciation_run_id" BIGINT NOT NULL,
    "asset_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "depreciation_no" INTEGER NOT NULL,
    "depreciation_amount" DECIMAL(19,4) NOT NULL,
    "book_value_before" DECIMAL(19,4) NOT NULL,
    "cost_center_id" BIGINT,
    "division_id" BIGINT,
    "subdivision_id" BIGINT,
    "project_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_depreciation_run_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_transfers" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'TRANSFER',
    "branch_id" BIGINT NOT NULL,
    "doc_date" DATE NOT NULL,
    "effective_date" DATE,
    "description" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_transfers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_transfer_lines" (
    "id" BIGSERIAL NOT NULL,
    "transfer_id" BIGINT NOT NULL,
    "asset_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "from_location_id" BIGINT,
    "to_location_id" BIGINT,
    "from_department_id" BIGINT,
    "to_department_id" BIGINT,
    "from_branch_id" BIGINT,
    "to_branch_id" BIGINT,
    "custodian" TEXT,
    "effective_date" DATE,
    "metadata" JSONB,

    CONSTRAINT "fa_transfer_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_disposals" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "legacy_code" TEXT,
    "fa_doc_type" "ErpFaDocType" NOT NULL DEFAULT 'DISPOSAL',
    "branch_id" BIGINT NOT NULL,
    "doc_date" DATE NOT NULL,
    "description" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "posting_status" "ErpPostingStatus" NOT NULL,
    "ledger_entry_id" BIGINT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "fa_disposals_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "fa_disposal_lines" (
    "id" BIGSERIAL NOT NULL,
    "disposal_id" BIGINT NOT NULL,
    "asset_id" BIGINT NOT NULL,
    "line_no" INTEGER NOT NULL,
    "disposal_type" TEXT NOT NULL,
    "proceeds" DECIMAL(19,4),
    "gain_loss_account_id" BIGINT,
    "metadata" JSONB,

    CONSTRAINT "fa_disposal_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_area_categories" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_area_categories_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_areas" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "category_id" BIGINT,
    "branch_id" BIGINT,
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_areas_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_terminals" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "warehouse_id" BIGINT,
    "hardware_info" JSONB,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_terminals_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_transaction_types" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "settings" JSONB,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_transaction_types_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_item_prices" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "area_id" BIGINT,
    "category_id" BIGINT,
    "price_is_editable" BOOLEAN NOT NULL DEFAULT false,
    "min_stock" DECIMAL(19,4),
    "max_stock" DECIMAL(19,4),
    "reorder_stock" DECIMAL(19,4),
    "min_order_stock" DECIMAL(19,4),
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_item_prices_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_item_price_tiers" (
    "id" BIGSERIAL NOT NULL,
    "item_price_id" BIGINT NOT NULL,
    "tier_level" INTEGER NOT NULL,
    "price" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "min_qty" DECIMAL(19,4),

    CONSTRAINT "pos_item_price_tiers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_price_agreements" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "legacy_code" TEXT,
    "branch_id" BIGINT NOT NULL,
    "location_id" BIGINT,
    "partner_id" BIGINT NOT NULL,
    "partner_contact_id" BIGINT,
    "fiscal_period_id" BIGINT NOT NULL,
    "agreement_date" DATE NOT NULL,
    "points_before" DECIMAL(19,4),
    "points_in" DECIMAL(19,4),
    "points_out" DECIMAL(19,4),
    "points_after" DECIMAL(19,4),
    "description" TEXT,
    "notes" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
    "posting_status" "ErpPostingStatus" NOT NULL,
    "posted_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_price_agreements_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_price_agreement_lines" (
    "id" BIGSERIAL NOT NULL,
    "agreement_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "agreed_price" DECIMAL(19,4) NOT NULL,
    "discount_percent" DECIMAL(9,4),
    "valid_from" DATE,
    "valid_to" DATE,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pos_price_agreement_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_promotions" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "legacy_code" TEXT,
    "promotion_type" "ErpPromotionType" NOT NULL,
    "valid_from" DATE NOT NULL,
    "valid_to" DATE NOT NULL,
    "area_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_promotions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_bonus_rules" (
    "id" BIGSERIAL NOT NULL,
    "promotion_id" BIGINT,
    "trigger_item_id" BIGINT NOT NULL,
    "trigger_qty" DECIMAL(19,4) NOT NULL,
    "bonus_item_id" BIGINT NOT NULL,
    "bonus_qty" DECIMAL(19,4) NOT NULL,
    "category_id" BIGINT,
    "valid_from" DATE,
    "valid_to" DATE,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_bonus_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_bonus_transactions" (
    "id" BIGSERIAL NOT NULL,
    "bonus_rule_id" BIGINT NOT NULL,
    "sale_invoice_id" BIGINT NOT NULL,
    "qty" DECIMAL(19,4) NOT NULL,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "pos_bonus_transactions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_substitution_rules" (
    "id" BIGSERIAL NOT NULL,
    "promotion_id" BIGINT,
    "from_item_id" BIGINT NOT NULL,
    "to_qty1" DECIMAL(19,4),
    "to_qty2" DECIMAL(19,4),
    "category_id" BIGINT,
    "promo_no" TEXT,
    "valid_from" DATE,
    "valid_to" DATE,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_substitution_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_additional_item_rules" (
    "id" BIGSERIAL NOT NULL,
    "promotion_id" BIGINT,
    "base_item_id" BIGINT NOT NULL,
    "additional_item_id" BIGINT NOT NULL,
    "qty" DECIMAL(19,4) NOT NULL,
    "valid_from" DATE,
    "valid_to" DATE,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_additional_item_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_discount_rules" (
    "id" BIGSERIAL NOT NULL,
    "promotion_id" BIGINT,
    "scope" "ErpDiscountScope" NOT NULL,
    "item_id" BIGINT,
    "item_category_id" BIGINT,
    "partner_category_id" BIGINT,
    "discount_percent" DECIMAL(9,4) NOT NULL,
    "min_qty" DECIMAL(19,4),
    "min_amount" DECIMAL(19,4),
    "valid_from" DATE,
    "valid_to" DATE,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_discount_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_point_rules" (
    "id" BIGSERIAL NOT NULL,
    "scope" "ErpDiscountScope" NOT NULL,
    "item_id" BIGINT,
    "item_category_id" BIGINT,
    "qty_from" DECIMAL(19,4),
    "qty_to" DECIMAL(19,4),
    "points_awarded" DECIMAL(19,4) NOT NULL,
    "valid_from" DATE,
    "valid_to" DATE,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_point_rules_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_point_transactions" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "type" "ErpPointTransactionType" NOT NULL,
    "points" DECIMAL(19,4) NOT NULL,
    "sale_invoice_id" BIGINT,
    "rule_id" BIGINT,
    "transaction_date" DATE NOT NULL,
    "category_id" BIGINT,
    "balance_after" DECIMAL(19,4),
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "pos_point_transactions_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pos_vouchers" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT,
    "legacy_code" TEXT,
    "face_value" DECIMAL(19,4) NOT NULL,
    "status" "ErpVoucherStatus" NOT NULL,
    "issued_to_partner_id" BIGINT,
    "issued_date" DATE,
    "expiry_date" DATE,
    "redeemed_sale_invoice_id" BIGINT,
    "redeemed_date" DATE,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pos_vouchers_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pln_reorder_policies" (
    "id" BIGSERIAL NOT NULL,
    "legacy_code" TEXT,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT,
    "safety_stock" DECIMAL(19,4) NOT NULL,
    "reorder_point" DECIMAL(19,4) NOT NULL,
    "min_order_qty" DECIMAL(19,4) NOT NULL,
    "max_order_qty" DECIMAL(19,4),
    "reorder_qty" DECIMAL(19,4) NOT NULL,
    "lot_size_method" "ErpLotSizeMethod" NOT NULL DEFAULT 'LOT_FOR_LOT',
    "eoq_qty" DECIMAL(19,4),
    "lead_time_days" INTEGER NOT NULL,
    "preferred_source" "ErpReplenishmentSource" NOT NULL DEFAULT 'PURCHASE',
    "preferred_supplier_id" BIGINT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pln_reorder_policies_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pln_demand_forecasts" (
    "id" BIGSERIAL NOT NULL,
    "legacy_code" TEXT,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "forecast_qty" DECIMAL(19,4) NOT NULL,
    "confirmed_qty" DECIMAL(19,4),
    "source" TEXT NOT NULL,
    "source_ref_id" BIGINT,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pln_demand_forecasts_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pln_mrp_runs" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "legacy_code" TEXT,
    "run_type" TEXT NOT NULL,
    "plan_horizon_start" DATE NOT NULL,
    "plan_horizon_end" DATE NOT NULL,
    "item_scope_id" BIGINT,
    "warehouse_scope_id" BIGINT,
    "include_forecast" BOOLEAN NOT NULL DEFAULT true,
    "include_open_orders" BOOLEAN NOT NULL DEFAULT true,
    "status" "ErpMrpRunStatus" NOT NULL DEFAULT 'DRAFT',
    "started_at" TIMESTAMPTZ(6),
    "completed_at" TIMESTAMPTZ(6),
    "suggestion_count" INTEGER,
    "notes" TEXT,
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pln_mrp_runs_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pln_mrp_run_lines" (
    "id" BIGSERIAL NOT NULL,
    "mrp_run_id" BIGINT NOT NULL,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "gross_requirement" DECIMAL(19,4) NOT NULL,
    "scheduled_receipt" DECIMAL(19,4) NOT NULL,
    "projected_on_hand" DECIMAL(19,4) NOT NULL,
    "safety_stock" DECIMAL(19,4) NOT NULL,
    "net_requirement" DECIMAL(19,4) NOT NULL,
    "planned_order_qty" DECIMAL(19,4) NOT NULL,
    "line_no" INTEGER NOT NULL,

    CONSTRAINT "pln_mrp_run_lines_pkey" PRIMARY KEY ("id")
);

-- CreateTable
CREATE TABLE "pln_replenishment_suggestions" (
    "id" BIGSERIAL NOT NULL,
    "legacy_code" TEXT,
    "mrp_run_id" BIGINT,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "source" "ErpReplenishmentSource" NOT NULL,
    "source_warehouse_id" BIGINT,
    "preferred_supplier_id" BIGINT,
    "required_qty" DECIMAL(19,4) NOT NULL,
    "suggested_qty" DECIMAL(19,4) NOT NULL,
    "needed_by_date" DATE NOT NULL,
    "action_due_date" DATE NOT NULL,
    "status" "ErpSuggestionStatus" NOT NULL DEFAULT 'PENDING',
    "converted_to_doc_type" TEXT,
    "converted_to_doc_id" BIGINT,
    "rejection_reason" TEXT,
    "approved_by_id" BIGINT,
    "approved_at" TIMESTAMPTZ(6),
    "metadata" JSONB,
    "deleted_at" TIMESTAMPTZ(6),
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,

    CONSTRAINT "pln_replenishment_suggestions_pkey" PRIMARY KEY ("id")
);

-- CreateIndex
CREATE UNIQUE INDEX "md_cost_centers_code_key" ON "md_cost_centers"("code");

-- CreateIndex
CREATE INDEX "md_cost_centers_parent_id_idx" ON "md_cost_centers"("parent_id");

-- CreateIndex
CREATE INDEX "md_cost_centers_legacy_code_idx" ON "md_cost_centers"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_divisions_code_key" ON "md_divisions"("code");

-- CreateIndex
CREATE INDEX "md_divisions_parent_id_idx" ON "md_divisions"("parent_id");

-- CreateIndex
CREATE INDEX "md_divisions_legacy_code_idx" ON "md_divisions"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_subdivisions_code_key" ON "md_subdivisions"("code");

-- CreateIndex
CREATE INDEX "md_subdivisions_division_id_idx" ON "md_subdivisions"("division_id");

-- CreateIndex
CREATE INDEX "md_subdivisions_parent_id_idx" ON "md_subdivisions"("parent_id");

-- CreateIndex
CREATE INDEX "md_subdivisions_legacy_code_idx" ON "md_subdivisions"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "md_projects_code_key" ON "md_projects"("code");

-- CreateIndex
CREATE INDEX "md_projects_parent_id_idx" ON "md_projects"("parent_id");

-- CreateIndex
CREATE INDEX "md_projects_branch_id_idx" ON "md_projects"("branch_id");

-- CreateIndex
CREATE INDEX "md_projects_legacy_code_idx" ON "md_projects"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_journal_entries_doc_number_key" ON "fin_journal_entries"("doc_number");

-- CreateIndex
CREATE INDEX "fin_journal_entries_fiscal_period_id_idx" ON "fin_journal_entries"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_journal_entries_entry_date_idx" ON "fin_journal_entries"("entry_date");

-- CreateIndex
CREATE INDEX "fin_journal_entries_journal_type_status_idx" ON "fin_journal_entries"("journal_type", "status");

-- CreateIndex
CREATE INDEX "fin_journal_entries_branch_id_idx" ON "fin_journal_entries"("branch_id");

-- CreateIndex
CREATE INDEX "fin_journal_entries_partner_id_idx" ON "fin_journal_entries"("partner_id");

-- CreateIndex
CREATE INDEX "fin_journal_entries_currency_id_idx" ON "fin_journal_entries"("currency_id");

-- CreateIndex
CREATE INDEX "fin_journal_entries_legacy_code_idx" ON "fin_journal_entries"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_journal_lines_journal_entry_id_idx" ON "fin_journal_lines"("journal_entry_id");

-- CreateIndex
CREATE INDEX "fin_journal_lines_account_id_idx" ON "fin_journal_lines"("account_id");

-- CreateIndex
CREATE INDEX "fin_journal_lines_cost_center_id_idx" ON "fin_journal_lines"("cost_center_id");

-- CreateIndex
CREATE INDEX "fin_journal_lines_division_id_idx" ON "fin_journal_lines"("division_id");

-- CreateIndex
CREATE INDEX "fin_journal_lines_subdivision_id_idx" ON "fin_journal_lines"("subdivision_id");

-- CreateIndex
CREATE INDEX "fin_journal_lines_project_id_idx" ON "fin_journal_lines"("project_id");

-- CreateIndex
CREATE INDEX "fin_ledger_entries_account_id_entry_date_idx" ON "fin_ledger_entries"("account_id", "entry_date");

-- CreateIndex
CREATE INDEX "fin_ledger_entries_fiscal_period_id_idx" ON "fin_ledger_entries"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_ledger_entries_partner_id_ar_ap_type_settlement_status_idx" ON "fin_ledger_entries"("partner_id", "ar_ap_type", "settlement_status");

-- CreateIndex
CREATE INDEX "fin_ledger_entries_source_doc_type_source_id_idx" ON "fin_ledger_entries"("source_doc_type", "source_id");

-- CreateIndex
CREATE INDEX "fin_ledger_entries_reconciliation_status_idx" ON "fin_ledger_entries"("reconciliation_status");

-- CreateIndex
CREATE INDEX "fin_ledger_entries_legacy_code_idx" ON "fin_ledger_entries"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_cash_bank_transactions_doc_number_key" ON "fin_cash_bank_transactions"("doc_number");

-- CreateIndex
CREATE INDEX "fin_cash_bank_transactions_fiscal_period_id_idx" ON "fin_cash_bank_transactions"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_transactions_branch_id_idx" ON "fin_cash_bank_transactions"("branch_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_transactions_partner_id_idx" ON "fin_cash_bank_transactions"("partner_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_transactions_legacy_code_idx" ON "fin_cash_bank_transactions"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_cash_bank_lines_cash_bank_transaction_id_idx" ON "fin_cash_bank_lines"("cash_bank_transaction_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_lines_account_id_idx" ON "fin_cash_bank_lines"("account_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_lines_cost_center_id_idx" ON "fin_cash_bank_lines"("cost_center_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_lines_division_id_idx" ON "fin_cash_bank_lines"("division_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_lines_subdivision_id_idx" ON "fin_cash_bank_lines"("subdivision_id");

-- CreateIndex
CREATE INDEX "fin_cash_bank_lines_project_id_idx" ON "fin_cash_bank_lines"("project_id");

-- CreateIndex
CREATE UNIQUE INDEX "fin_ar_receipts_doc_number_key" ON "fin_ar_receipts"("doc_number");

-- CreateIndex
CREATE INDEX "fin_ar_receipts_fiscal_period_id_idx" ON "fin_ar_receipts"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_ar_receipts_partner_id_idx" ON "fin_ar_receipts"("partner_id");

-- CreateIndex
CREATE INDEX "fin_ar_receipts_legacy_code_idx" ON "fin_ar_receipts"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_ap_payments_doc_number_key" ON "fin_ap_payments"("doc_number");

-- CreateIndex
CREATE INDEX "fin_ap_payments_fiscal_period_id_idx" ON "fin_ap_payments"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_ap_payments_partner_id_idx" ON "fin_ap_payments"("partner_id");

-- CreateIndex
CREATE INDEX "fin_ap_payments_legacy_code_idx" ON "fin_ap_payments"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_payment_instruments_ar_receipt_id_idx" ON "fin_payment_instruments"("ar_receipt_id");

-- CreateIndex
CREATE INDEX "fin_payment_instruments_ap_payment_id_idx" ON "fin_payment_instruments"("ap_payment_id");

-- CreateIndex
CREATE INDEX "fin_payment_instruments_giro_id_idx" ON "fin_payment_instruments"("giro_id");

-- CreateIndex
CREATE INDEX "fin_settlement_allocations_ar_receipt_id_idx" ON "fin_settlement_allocations"("ar_receipt_id");

-- CreateIndex
CREATE INDEX "fin_settlement_allocations_ap_payment_id_idx" ON "fin_settlement_allocations"("ap_payment_id");

-- CreateIndex
CREATE INDEX "fin_settlement_allocations_ledger_entry_id_idx" ON "fin_settlement_allocations"("ledger_entry_id");

-- CreateIndex
CREATE UNIQUE INDEX "fin_giros_giro_number_key" ON "fin_giros"("giro_number");

-- CreateIndex
CREATE INDEX "fin_giros_status_due_date_idx" ON "fin_giros"("status", "due_date");

-- CreateIndex
CREATE INDEX "fin_giros_partner_id_idx" ON "fin_giros"("partner_id");

-- CreateIndex
CREATE INDEX "fin_giros_legacy_code_idx" ON "fin_giros"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_budget_realizations_account_id_idx" ON "fin_budget_realizations"("account_id");

-- CreateIndex
CREATE INDEX "fin_budget_realizations_legacy_code_idx" ON "fin_budget_realizations"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_budget_realizations_fiscal_period_id_account_id_branch__key" ON "fin_budget_realizations"("fiscal_period_id", "account_id", "branch_id", "location_id", "cost_center_id", "division_id", "subdivision_id", "project_id");

-- CreateIndex
CREATE UNIQUE INDEX "fin_period_closings_doc_number_key" ON "fin_period_closings"("doc_number");

-- CreateIndex
CREATE INDEX "fin_period_closings_fiscal_period_id_idx" ON "fin_period_closings"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_period_closings_status_idx" ON "fin_period_closings"("status");

-- CreateIndex
CREATE INDEX "fin_period_closings_legacy_code_idx" ON "fin_period_closings"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_period_closings_fiscal_period_id_key" ON "fin_period_closings"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_posting_rules_module_event_type_priority_idx" ON "fin_posting_rules"("module", "event_type", "priority");

-- CreateIndex
CREATE INDEX "fin_posting_rules_legacy_code_idx" ON "fin_posting_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_posting_rule_lines_rule_id_idx" ON "fin_posting_rule_lines"("rule_id");

-- CreateIndex
CREATE INDEX "fin_posting_rule_lines_account_id_idx" ON "fin_posting_rule_lines"("account_id");

-- CreateIndex
CREATE INDEX "fin_tax_entries_tax_entry_type_fiscal_period_id_idx" ON "fin_tax_entries"("tax_entry_type", "fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_tax_entries_source_doc_type_source_id_idx" ON "fin_tax_entries"("source_doc_type", "source_id");

-- CreateIndex
CREATE INDEX "fin_tax_entries_faktur_number_idx" ON "fin_tax_entries"("faktur_number");

-- CreateIndex
CREATE INDEX "fin_tax_entries_ledger_entry_id_idx" ON "fin_tax_entries"("ledger_entry_id");

-- CreateIndex
CREATE INDEX "fin_tax_entries_legacy_code_idx" ON "fin_tax_entries"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_withholding_tax_certificates_cert_number_key" ON "fin_withholding_tax_certificates"("cert_number");

-- CreateIndex
CREATE INDEX "fin_withholding_tax_certificates_tax_entry_id_idx" ON "fin_withholding_tax_certificates"("tax_entry_id");

-- CreateIndex
CREATE INDEX "fin_withholding_tax_certificates_partner_id_idx" ON "fin_withholding_tax_certificates"("partner_id");

-- CreateIndex
CREATE INDEX "fin_withholding_tax_certificates_legacy_code_idx" ON "fin_withholding_tax_certificates"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_fx_revaluation_runs_doc_number_key" ON "fin_fx_revaluation_runs"("doc_number");

-- CreateIndex
CREATE INDEX "fin_fx_revaluation_runs_fiscal_period_id_idx" ON "fin_fx_revaluation_runs"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_fx_revaluation_runs_status_idx" ON "fin_fx_revaluation_runs"("status");

-- CreateIndex
CREATE INDEX "fin_fx_revaluation_runs_legacy_code_idx" ON "fin_fx_revaluation_runs"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_fx_revaluation_lines_revaluation_run_id_idx" ON "fin_fx_revaluation_lines"("revaluation_run_id");

-- CreateIndex
CREATE INDEX "fin_fx_revaluation_lines_account_id_idx" ON "fin_fx_revaluation_lines"("account_id");

-- CreateIndex
CREATE INDEX "fin_bank_statements_bank_account_id_idx" ON "fin_bank_statements"("bank_account_id");

-- CreateIndex
CREATE INDEX "fin_bank_statements_branch_id_idx" ON "fin_bank_statements"("branch_id");

-- CreateIndex
CREATE INDEX "fin_bank_statements_legacy_code_idx" ON "fin_bank_statements"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_bank_statement_lines_statement_id_idx" ON "fin_bank_statement_lines"("statement_id");

-- CreateIndex
CREATE INDEX "fin_bank_statement_lines_matched_ledger_entry_id_idx" ON "fin_bank_statement_lines"("matched_ledger_entry_id");

-- CreateIndex
CREATE INDEX "fin_bank_statement_lines_matched_cash_bank_transaction_id_idx" ON "fin_bank_statement_lines"("matched_cash_bank_transaction_id");

-- CreateIndex
CREATE UNIQUE INDEX "fin_recurring_journal_templates_code_key" ON "fin_recurring_journal_templates"("code");

-- CreateIndex
CREATE INDEX "fin_recurring_journal_templates_legacy_code_idx" ON "fin_recurring_journal_templates"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_recurring_journal_template_lines_template_id_idx" ON "fin_recurring_journal_template_lines"("template_id");

-- CreateIndex
CREATE INDEX "fin_recurring_journal_template_lines_account_id_idx" ON "fin_recurring_journal_template_lines"("account_id");

-- CreateIndex
CREATE INDEX "fin_recurring_journal_template_lines_cost_center_id_idx" ON "fin_recurring_journal_template_lines"("cost_center_id");

-- CreateIndex
CREATE INDEX "fin_recurring_journal_template_lines_division_id_idx" ON "fin_recurring_journal_template_lines"("division_id");

-- CreateIndex
CREATE INDEX "fin_recurring_journal_template_lines_project_id_idx" ON "fin_recurring_journal_template_lines"("project_id");

-- CreateIndex
CREATE UNIQUE INDEX "fin_accrual_schedules_code_key" ON "fin_accrual_schedules"("code");

-- CreateIndex
CREATE INDEX "fin_accrual_schedules_cost_center_id_idx" ON "fin_accrual_schedules"("cost_center_id");

-- CreateIndex
CREATE INDEX "fin_accrual_schedules_legacy_code_idx" ON "fin_accrual_schedules"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_report_definitions_code_key" ON "fin_report_definitions"("code");

-- CreateIndex
CREATE INDEX "fin_report_definitions_legacy_code_idx" ON "fin_report_definitions"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_report_sections_report_id_idx" ON "fin_report_sections"("report_id");

-- CreateIndex
CREATE INDEX "fin_report_sections_parent_section_id_idx" ON "fin_report_sections"("parent_section_id");

-- CreateIndex
CREATE INDEX "fin_report_lines_section_id_idx" ON "fin_report_lines"("section_id");

-- CreateIndex
CREATE INDEX "fin_credit_limits_legacy_code_idx" ON "fin_credit_limits"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_credit_limits_partner_id_key" ON "fin_credit_limits"("partner_id");

-- CreateIndex
CREATE INDEX "fin_dunning_rules_legacy_code_idx" ON "fin_dunning_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_collection_activities_partner_id_idx" ON "fin_collection_activities"("partner_id");

-- CreateIndex
CREATE INDEX "fin_collection_activities_dunning_rule_id_idx" ON "fin_collection_activities"("dunning_rule_id");

-- CreateIndex
CREATE INDEX "fin_collection_activities_due_ledger_entry_id_idx" ON "fin_collection_activities"("due_ledger_entry_id");

-- CreateIndex
CREATE INDEX "fin_collection_activities_legacy_code_idx" ON "fin_collection_activities"("legacy_code");

-- CreateIndex
CREATE INDEX "fin_intercompany_rules_legacy_code_idx" ON "fin_intercompany_rules"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fin_intercompany_rules_from_branch_id_to_branch_id_key" ON "fin_intercompany_rules"("from_branch_id", "to_branch_id");

-- CreateIndex
CREATE UNIQUE INDEX "fin_intercompany_transactions_doc_number_key" ON "fin_intercompany_transactions"("doc_number");

-- CreateIndex
CREATE INDEX "fin_intercompany_transactions_fiscal_period_id_idx" ON "fin_intercompany_transactions"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fin_intercompany_transactions_rule_id_idx" ON "fin_intercompany_transactions"("rule_id");

-- CreateIndex
CREATE INDEX "fin_intercompany_transactions_from_journal_entry_id_idx" ON "fin_intercompany_transactions"("from_journal_entry_id");

-- CreateIndex
CREATE INDEX "fin_intercompany_transactions_to_journal_entry_id_idx" ON "fin_intercompany_transactions"("to_journal_entry_id");

-- CreateIndex
CREATE INDEX "fin_intercompany_transactions_legacy_code_idx" ON "fin_intercompany_transactions"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "inv_stock_movements_doc_number_key" ON "inv_stock_movements"("doc_number");

-- CreateIndex
CREATE INDEX "inv_stock_movements_movement_type_status_idx" ON "inv_stock_movements"("movement_type", "status");

-- CreateIndex
CREATE INDEX "inv_stock_movements_fiscal_period_id_idx" ON "inv_stock_movements"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "inv_stock_movements_related_movement_id_idx" ON "inv_stock_movements"("related_movement_id");

-- CreateIndex
CREATE INDEX "inv_stock_movements_legacy_code_idx" ON "inv_stock_movements"("legacy_code");

-- CreateIndex
CREATE INDEX "inv_stock_movement_lines_stock_movement_id_idx" ON "inv_stock_movement_lines"("stock_movement_id");

-- CreateIndex
CREATE INDEX "inv_stock_movement_lines_item_id_idx" ON "inv_stock_movement_lines"("item_id");

-- CreateIndex
CREATE INDEX "inv_stock_movement_lines_related_line_id_idx" ON "inv_stock_movement_lines"("related_line_id");

-- CreateIndex
CREATE INDEX "inv_stock_movement_lines_bin_id_idx" ON "inv_stock_movement_lines"("bin_id");

-- CreateIndex
CREATE INDEX "inv_stock_movement_lines_lot_id_idx" ON "inv_stock_movement_lines"("lot_id");

-- CreateIndex
CREATE INDEX "inv_stock_movement_lines_serial_id_idx" ON "inv_stock_movement_lines"("serial_id");

-- CreateIndex
CREATE UNIQUE INDEX "inv_opening_stocks_doc_number_key" ON "inv_opening_stocks"("doc_number");

-- CreateIndex
CREATE INDEX "inv_opening_stocks_fiscal_period_id_idx" ON "inv_opening_stocks"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "inv_opening_stocks_legacy_code_idx" ON "inv_opening_stocks"("legacy_code");

-- CreateIndex
CREATE INDEX "inv_opening_stock_lines_opening_stock_id_idx" ON "inv_opening_stock_lines"("opening_stock_id");

-- CreateIndex
CREATE INDEX "inv_opening_stock_lines_item_id_idx" ON "inv_opening_stock_lines"("item_id");

-- CreateIndex
CREATE INDEX "inv_opening_stock_lines_bin_id_idx" ON "inv_opening_stock_lines"("bin_id");

-- CreateIndex
CREATE INDEX "inv_opening_stock_lines_lot_id_idx" ON "inv_opening_stock_lines"("lot_id");

-- CreateIndex
CREATE INDEX "inv_opening_stock_lines_serial_id_idx" ON "inv_opening_stock_lines"("serial_id");

-- CreateIndex
CREATE UNIQUE INDEX "inv_stock_counts_doc_number_key" ON "inv_stock_counts"("doc_number");

-- CreateIndex
CREATE INDEX "inv_stock_counts_fiscal_period_id_idx" ON "inv_stock_counts"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "inv_stock_counts_legacy_code_idx" ON "inv_stock_counts"("legacy_code");

-- CreateIndex
CREATE INDEX "inv_stock_count_lines_stock_count_id_idx" ON "inv_stock_count_lines"("stock_count_id");

-- CreateIndex
CREATE INDEX "inv_stock_count_lines_item_id_idx" ON "inv_stock_count_lines"("item_id");

-- CreateIndex
CREATE INDEX "inv_stock_count_lines_bin_id_idx" ON "inv_stock_count_lines"("bin_id");

-- CreateIndex
CREATE INDEX "inv_stock_count_lines_lot_id_idx" ON "inv_stock_count_lines"("lot_id");

-- CreateIndex
CREATE INDEX "inv_stock_count_lines_serial_id_idx" ON "inv_stock_count_lines"("serial_id");

-- CreateIndex
CREATE UNIQUE INDEX "inv_stock_adjustments_doc_number_key" ON "inv_stock_adjustments"("doc_number");

-- CreateIndex
CREATE INDEX "inv_stock_adjustments_fiscal_period_id_idx" ON "inv_stock_adjustments"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustments_stock_count_id_idx" ON "inv_stock_adjustments"("stock_count_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustments_legacy_code_idx" ON "inv_stock_adjustments"("legacy_code");

-- CreateIndex
CREATE INDEX "inv_stock_adjustment_lines_stock_adjustment_id_idx" ON "inv_stock_adjustment_lines"("stock_adjustment_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustment_lines_item_id_idx" ON "inv_stock_adjustment_lines"("item_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustment_lines_count_line_id_idx" ON "inv_stock_adjustment_lines"("count_line_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustment_lines_bin_id_idx" ON "inv_stock_adjustment_lines"("bin_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustment_lines_lot_id_idx" ON "inv_stock_adjustment_lines"("lot_id");

-- CreateIndex
CREATE INDEX "inv_stock_adjustment_lines_serial_id_idx" ON "inv_stock_adjustment_lines"("serial_id");

-- CreateIndex
CREATE UNIQUE INDEX "inv_weighbridge_tickets_doc_number_key" ON "inv_weighbridge_tickets"("doc_number");

-- CreateIndex
CREATE INDEX "inv_weighbridge_tickets_fiscal_period_id_idx" ON "inv_weighbridge_tickets"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "inv_weighbridge_tickets_legacy_code_idx" ON "inv_weighbridge_tickets"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "inv_cost_recalculations_doc_number_key" ON "inv_cost_recalculations"("doc_number");

-- CreateIndex
CREATE INDEX "inv_cost_recalculations_status_idx" ON "inv_cost_recalculations"("status");

-- CreateIndex
CREATE INDEX "inv_cost_recalculations_item_id_warehouse_id_idx" ON "inv_cost_recalculations"("item_id", "warehouse_id");

-- CreateIndex
CREATE INDEX "inv_cost_recalculations_trigger_source_doc_type_trigger_sou_idx" ON "inv_cost_recalculations"("trigger_source_doc_type", "trigger_source_id");

-- CreateIndex
CREATE INDEX "inv_cost_recalculations_fiscal_period_id_idx" ON "inv_cost_recalculations"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "inv_cost_recalculations_legacy_code_idx" ON "inv_cost_recalculations"("legacy_code");

-- CreateIndex
CREATE INDEX "inv_cost_recalculation_lines_cost_recalculation_id_idx" ON "inv_cost_recalculation_lines"("cost_recalculation_id");

-- CreateIndex
CREATE INDEX "inv_cost_recalculation_lines_item_id_idx" ON "inv_cost_recalculation_lines"("item_id");

-- CreateIndex
CREATE INDEX "inv_cost_recalculation_lines_ledger_entry_id_idx" ON "inv_cost_recalculation_lines"("ledger_entry_id");

-- CreateIndex
CREATE INDEX "inv_bins_warehouse_id_idx" ON "inv_bins"("warehouse_id");

-- CreateIndex
CREATE INDEX "inv_bins_legacy_code_idx" ON "inv_bins"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "inv_bins_warehouse_id_code_key" ON "inv_bins"("warehouse_id", "code");

-- CreateIndex
CREATE INDEX "inv_lots_item_id_status_idx" ON "inv_lots"("item_id", "status");

-- CreateIndex
CREATE INDEX "inv_lots_expiry_date_idx" ON "inv_lots"("expiry_date");

-- CreateIndex
CREATE INDEX "inv_lots_legacy_code_idx" ON "inv_lots"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "inv_lots_item_id_lot_number_key" ON "inv_lots"("item_id", "lot_number");

-- CreateIndex
CREATE INDEX "inv_serials_item_id_status_idx" ON "inv_serials"("item_id", "status");

-- CreateIndex
CREATE INDEX "inv_serials_current_warehouse_id_idx" ON "inv_serials"("current_warehouse_id");

-- CreateIndex
CREATE INDEX "inv_serials_lot_id_idx" ON "inv_serials"("lot_id");

-- CreateIndex
CREATE INDEX "inv_serials_current_bin_id_idx" ON "inv_serials"("current_bin_id");

-- CreateIndex
CREATE INDEX "inv_serials_last_movement_id_idx" ON "inv_serials"("last_movement_id");

-- CreateIndex
CREATE INDEX "inv_serials_legacy_code_idx" ON "inv_serials"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "inv_serials_item_id_serial_number_key" ON "inv_serials"("item_id", "serial_number");

-- CreateIndex
CREATE INDEX "inv_stock_reservations_item_id_warehouse_id_status_idx" ON "inv_stock_reservations"("item_id", "warehouse_id", "status");

-- CreateIndex
CREATE INDEX "inv_stock_reservations_source_doc_type_source_doc_id_idx" ON "inv_stock_reservations"("source_doc_type", "source_doc_id");

-- CreateIndex
CREATE INDEX "inv_stock_reservations_bin_id_idx" ON "inv_stock_reservations"("bin_id");

-- CreateIndex
CREATE INDEX "inv_stock_reservations_lot_id_idx" ON "inv_stock_reservations"("lot_id");

-- CreateIndex
CREATE INDEX "inv_stock_reservations_fulfilled_by_movement_id_idx" ON "inv_stock_reservations"("fulfilled_by_movement_id");

-- CreateIndex
CREATE INDEX "inv_stock_reservations_legacy_code_idx" ON "inv_stock_reservations"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "pur_requisitions_doc_number_key" ON "pur_requisitions"("doc_number");

-- CreateIndex
CREATE INDEX "pur_requisitions_legacy_code_idx" ON "pur_requisitions"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_requisitions_branch_id_idx" ON "pur_requisitions"("branch_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_location_id_idx" ON "pur_requisitions"("location_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_warehouse_id_idx" ON "pur_requisitions"("warehouse_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_fiscal_period_id_idx" ON "pur_requisitions"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_supplier_id_idx" ON "pur_requisitions"("supplier_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_requested_by_id_idx" ON "pur_requisitions"("requested_by_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_sales_quotation_id_idx" ON "pur_requisitions"("sales_quotation_id");

-- CreateIndex
CREATE INDEX "pur_requisitions_work_order_id_idx" ON "pur_requisitions"("work_order_id");

-- CreateIndex
CREATE INDEX "pur_requisition_lines_requisition_id_idx" ON "pur_requisition_lines"("requisition_id");

-- CreateIndex
CREATE INDEX "pur_requisition_lines_item_id_idx" ON "pur_requisition_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_rfqs_doc_number_key" ON "pur_rfqs"("doc_number");

-- CreateIndex
CREATE INDEX "pur_rfqs_legacy_code_idx" ON "pur_rfqs"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_rfqs_branch_id_idx" ON "pur_rfqs"("branch_id");

-- CreateIndex
CREATE INDEX "pur_rfqs_fiscal_period_id_idx" ON "pur_rfqs"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_rfqs_requisition_id_idx" ON "pur_rfqs"("requisition_id");

-- CreateIndex
CREATE INDEX "pur_rfq_suppliers_rfq_id_idx" ON "pur_rfq_suppliers"("rfq_id");

-- CreateIndex
CREATE INDEX "pur_rfq_suppliers_supplier_id_idx" ON "pur_rfq_suppliers"("supplier_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_quotations_doc_number_key" ON "pur_quotations"("doc_number");

-- CreateIndex
CREATE INDEX "pur_quotations_legacy_code_idx" ON "pur_quotations"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_quotations_branch_id_idx" ON "pur_quotations"("branch_id");

-- CreateIndex
CREATE INDEX "pur_quotations_fiscal_period_id_idx" ON "pur_quotations"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_quotations_supplier_id_idx" ON "pur_quotations"("supplier_id");

-- CreateIndex
CREATE INDEX "pur_quotations_rfq_id_idx" ON "pur_quotations"("rfq_id");

-- CreateIndex
CREATE INDEX "pur_quotation_lines_quotation_id_idx" ON "pur_quotation_lines"("quotation_id");

-- CreateIndex
CREATE INDEX "pur_quotation_lines_item_id_idx" ON "pur_quotation_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_bid_selections_doc_number_key" ON "pur_bid_selections"("doc_number");

-- CreateIndex
CREATE INDEX "pur_bid_selections_legacy_code_idx" ON "pur_bid_selections"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_bid_selections_branch_id_idx" ON "pur_bid_selections"("branch_id");

-- CreateIndex
CREATE INDEX "pur_bid_selections_fiscal_period_id_idx" ON "pur_bid_selections"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_bid_selection_quotations_bid_selection_id_idx" ON "pur_bid_selection_quotations"("bid_selection_id");

-- CreateIndex
CREATE INDEX "pur_bid_selection_quotations_quotation_id_idx" ON "pur_bid_selection_quotations"("quotation_id");

-- CreateIndex
CREATE INDEX "pur_bid_selection_lines_bid_selection_id_idx" ON "pur_bid_selection_lines"("bid_selection_id");

-- CreateIndex
CREATE INDEX "pur_bid_selection_lines_quotation_line_id_idx" ON "pur_bid_selection_lines"("quotation_line_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_orders_doc_number_key" ON "pur_orders"("doc_number");

-- CreateIndex
CREATE INDEX "pur_orders_legacy_code_idx" ON "pur_orders"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_orders_branch_id_idx" ON "pur_orders"("branch_id");

-- CreateIndex
CREATE INDEX "pur_orders_fiscal_period_id_idx" ON "pur_orders"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_orders_supplier_id_idx" ON "pur_orders"("supplier_id");

-- CreateIndex
CREATE INDEX "pur_orders_requisition_id_idx" ON "pur_orders"("requisition_id");

-- CreateIndex
CREATE INDEX "pur_orders_quotation_id_idx" ON "pur_orders"("quotation_id");

-- CreateIndex
CREATE INDEX "pur_orders_bid_selection_id_idx" ON "pur_orders"("bid_selection_id");

-- CreateIndex
CREATE INDEX "pur_order_lines_order_id_idx" ON "pur_order_lines"("order_id");

-- CreateIndex
CREATE INDEX "pur_order_lines_item_id_idx" ON "pur_order_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_goods_receipts_doc_number_key" ON "pur_goods_receipts"("doc_number");

-- CreateIndex
CREATE INDEX "pur_goods_receipts_legacy_code_idx" ON "pur_goods_receipts"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_goods_receipts_branch_id_idx" ON "pur_goods_receipts"("branch_id");

-- CreateIndex
CREATE INDEX "pur_goods_receipts_fiscal_period_id_idx" ON "pur_goods_receipts"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_goods_receipts_supplier_id_idx" ON "pur_goods_receipts"("supplier_id");

-- CreateIndex
CREATE INDEX "pur_goods_receipts_order_id_idx" ON "pur_goods_receipts"("order_id");

-- CreateIndex
CREATE INDEX "pur_goods_receipt_lines_goods_receipt_id_idx" ON "pur_goods_receipt_lines"("goods_receipt_id");

-- CreateIndex
CREATE INDEX "pur_goods_receipt_lines_item_id_idx" ON "pur_goods_receipt_lines"("item_id");

-- CreateIndex
CREATE INDEX "pur_goods_receipt_lines_order_line_id_idx" ON "pur_goods_receipt_lines"("order_line_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_invoices_doc_number_key" ON "pur_invoices"("doc_number");

-- CreateIndex
CREATE INDEX "pur_invoices_legacy_code_idx" ON "pur_invoices"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_invoices_branch_id_idx" ON "pur_invoices"("branch_id");

-- CreateIndex
CREATE INDEX "pur_invoices_fiscal_period_id_idx" ON "pur_invoices"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_invoices_supplier_id_idx" ON "pur_invoices"("supplier_id");

-- CreateIndex
CREATE INDEX "pur_invoices_order_id_idx" ON "pur_invoices"("order_id");

-- CreateIndex
CREATE INDEX "pur_invoices_goods_receipt_id_idx" ON "pur_invoices"("goods_receipt_id");

-- CreateIndex
CREATE INDEX "pur_invoice_lines_invoice_id_idx" ON "pur_invoice_lines"("invoice_id");

-- CreateIndex
CREATE INDEX "pur_invoice_lines_item_id_idx" ON "pur_invoice_lines"("item_id");

-- CreateIndex
CREATE INDEX "pur_invoice_lines_order_line_id_idx" ON "pur_invoice_lines"("order_line_id");

-- CreateIndex
CREATE INDEX "pur_invoice_lines_goods_receipt_line_id_idx" ON "pur_invoice_lines"("goods_receipt_line_id");

-- CreateIndex
CREATE UNIQUE INDEX "pur_returns_doc_number_key" ON "pur_returns"("doc_number");

-- CreateIndex
CREATE INDEX "pur_returns_legacy_code_idx" ON "pur_returns"("legacy_code");

-- CreateIndex
CREATE INDEX "pur_returns_branch_id_idx" ON "pur_returns"("branch_id");

-- CreateIndex
CREATE INDEX "pur_returns_fiscal_period_id_idx" ON "pur_returns"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pur_returns_supplier_id_idx" ON "pur_returns"("supplier_id");

-- CreateIndex
CREATE INDEX "pur_returns_order_id_idx" ON "pur_returns"("order_id");

-- CreateIndex
CREATE INDEX "pur_returns_goods_receipt_id_idx" ON "pur_returns"("goods_receipt_id");

-- CreateIndex
CREATE INDEX "pur_returns_invoice_id_idx" ON "pur_returns"("invoice_id");

-- CreateIndex
CREATE INDEX "pur_return_lines_return_id_idx" ON "pur_return_lines"("return_id");

-- CreateIndex
CREATE INDEX "pur_return_lines_item_id_idx" ON "pur_return_lines"("item_id");

-- CreateIndex
CREATE INDEX "pur_return_lines_order_line_id_idx" ON "pur_return_lines"("order_line_id");

-- CreateIndex
CREATE INDEX "pur_return_lines_goods_receipt_line_id_idx" ON "pur_return_lines"("goods_receipt_line_id");

-- CreateIndex
CREATE INDEX "pur_return_lines_invoice_line_id_idx" ON "pur_return_lines"("invoice_line_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_quotations_code_key" ON "sls_quotations"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_quotations_doc_number_key" ON "sls_quotations"("doc_number");

-- CreateIndex
CREATE INDEX "sls_quotations_legacy_code_idx" ON "sls_quotations"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_quotations_branch_id_idx" ON "sls_quotations"("branch_id");

-- CreateIndex
CREATE INDEX "sls_quotations_customer_id_idx" ON "sls_quotations"("customer_id");

-- CreateIndex
CREATE INDEX "sls_quotations_fiscal_period_id_idx" ON "sls_quotations"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_quotations_purchase_requisition_id_idx" ON "sls_quotations"("purchase_requisition_id");

-- CreateIndex
CREATE INDEX "sls_quotation_lines_quotation_id_idx" ON "sls_quotation_lines"("quotation_id");

-- CreateIndex
CREATE INDEX "sls_quotation_lines_item_id_idx" ON "sls_quotation_lines"("item_id");

-- CreateIndex
CREATE INDEX "sls_quotation_materials_quotation_id_idx" ON "sls_quotation_materials"("quotation_id");

-- CreateIndex
CREATE INDEX "sls_quotation_materials_item_id_idx" ON "sls_quotation_materials"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_orders_code_key" ON "sls_orders"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_orders_doc_number_key" ON "sls_orders"("doc_number");

-- CreateIndex
CREATE INDEX "sls_orders_legacy_code_idx" ON "sls_orders"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_orders_branch_id_idx" ON "sls_orders"("branch_id");

-- CreateIndex
CREATE INDEX "sls_orders_customer_id_idx" ON "sls_orders"("customer_id");

-- CreateIndex
CREATE INDEX "sls_orders_fiscal_period_id_idx" ON "sls_orders"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_orders_quotation_id_idx" ON "sls_orders"("quotation_id");

-- CreateIndex
CREATE INDEX "sls_order_lines_order_id_idx" ON "sls_order_lines"("order_id");

-- CreateIndex
CREATE INDEX "sls_order_lines_item_id_idx" ON "sls_order_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_proforma_invoices_code_key" ON "sls_proforma_invoices"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_proforma_invoices_doc_number_key" ON "sls_proforma_invoices"("doc_number");

-- CreateIndex
CREATE INDEX "sls_proforma_invoices_legacy_code_idx" ON "sls_proforma_invoices"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_proforma_invoices_branch_id_idx" ON "sls_proforma_invoices"("branch_id");

-- CreateIndex
CREATE INDEX "sls_proforma_invoices_customer_id_idx" ON "sls_proforma_invoices"("customer_id");

-- CreateIndex
CREATE INDEX "sls_proforma_invoices_fiscal_period_id_idx" ON "sls_proforma_invoices"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_proforma_invoices_quotation_id_idx" ON "sls_proforma_invoices"("quotation_id");

-- CreateIndex
CREATE INDEX "sls_proforma_invoices_order_id_idx" ON "sls_proforma_invoices"("order_id");

-- CreateIndex
CREATE INDEX "sls_proforma_invoice_lines_proforma_invoice_id_idx" ON "sls_proforma_invoice_lines"("proforma_invoice_id");

-- CreateIndex
CREATE INDEX "sls_proforma_invoice_lines_item_id_idx" ON "sls_proforma_invoice_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_packing_lists_code_key" ON "sls_packing_lists"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_packing_lists_doc_number_key" ON "sls_packing_lists"("doc_number");

-- CreateIndex
CREATE INDEX "sls_packing_lists_legacy_code_idx" ON "sls_packing_lists"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_packing_lists_branch_id_idx" ON "sls_packing_lists"("branch_id");

-- CreateIndex
CREATE INDEX "sls_packing_lists_customer_id_idx" ON "sls_packing_lists"("customer_id");

-- CreateIndex
CREATE INDEX "sls_packing_lists_fiscal_period_id_idx" ON "sls_packing_lists"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_packing_lists_order_id_idx" ON "sls_packing_lists"("order_id");

-- CreateIndex
CREATE INDEX "sls_packing_lists_proforma_invoice_id_idx" ON "sls_packing_lists"("proforma_invoice_id");

-- CreateIndex
CREATE INDEX "sls_packing_list_lines_packing_list_id_idx" ON "sls_packing_list_lines"("packing_list_id");

-- CreateIndex
CREATE INDEX "sls_packing_list_lines_item_id_idx" ON "sls_packing_list_lines"("item_id");

-- CreateIndex
CREATE INDEX "sls_packing_list_packs_packing_list_id_idx" ON "sls_packing_list_packs"("packing_list_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_delivery_orders_code_key" ON "sls_delivery_orders"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_delivery_orders_doc_number_key" ON "sls_delivery_orders"("doc_number");

-- CreateIndex
CREATE INDEX "sls_delivery_orders_legacy_code_idx" ON "sls_delivery_orders"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_delivery_orders_branch_id_idx" ON "sls_delivery_orders"("branch_id");

-- CreateIndex
CREATE INDEX "sls_delivery_orders_customer_id_idx" ON "sls_delivery_orders"("customer_id");

-- CreateIndex
CREATE INDEX "sls_delivery_orders_fiscal_period_id_idx" ON "sls_delivery_orders"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_delivery_orders_order_id_idx" ON "sls_delivery_orders"("order_id");

-- CreateIndex
CREATE INDEX "sls_delivery_orders_packing_list_id_idx" ON "sls_delivery_orders"("packing_list_id");

-- CreateIndex
CREATE INDEX "sls_delivery_order_lines_delivery_order_id_idx" ON "sls_delivery_order_lines"("delivery_order_id");

-- CreateIndex
CREATE INDEX "sls_delivery_order_lines_item_id_idx" ON "sls_delivery_order_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_delivery_reports_code_key" ON "sls_delivery_reports"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_delivery_reports_doc_number_key" ON "sls_delivery_reports"("doc_number");

-- CreateIndex
CREATE INDEX "sls_delivery_reports_legacy_code_idx" ON "sls_delivery_reports"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_delivery_reports_branch_id_idx" ON "sls_delivery_reports"("branch_id");

-- CreateIndex
CREATE INDEX "sls_delivery_reports_customer_id_idx" ON "sls_delivery_reports"("customer_id");

-- CreateIndex
CREATE INDEX "sls_delivery_reports_fiscal_period_id_idx" ON "sls_delivery_reports"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_delivery_reports_delivery_order_id_idx" ON "sls_delivery_reports"("delivery_order_id");

-- CreateIndex
CREATE INDEX "sls_delivery_report_lines_delivery_report_id_idx" ON "sls_delivery_report_lines"("delivery_report_id");

-- CreateIndex
CREATE INDEX "sls_delivery_report_lines_item_id_idx" ON "sls_delivery_report_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_invoices_code_key" ON "sls_invoices"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_invoices_doc_number_key" ON "sls_invoices"("doc_number");

-- CreateIndex
CREATE INDEX "sls_invoices_legacy_code_idx" ON "sls_invoices"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_invoices_branch_id_idx" ON "sls_invoices"("branch_id");

-- CreateIndex
CREATE INDEX "sls_invoices_customer_id_idx" ON "sls_invoices"("customer_id");

-- CreateIndex
CREATE INDEX "sls_invoices_fiscal_period_id_idx" ON "sls_invoices"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_invoices_order_id_idx" ON "sls_invoices"("order_id");

-- CreateIndex
CREATE INDEX "sls_invoices_advance_id_idx" ON "sls_invoices"("advance_id");

-- CreateIndex
CREATE INDEX "sls_invoices_delivery_order_id_idx" ON "sls_invoices"("delivery_order_id");

-- CreateIndex
CREATE INDEX "sls_invoices_ar_ledger_entry_id_idx" ON "sls_invoices"("ar_ledger_entry_id");

-- CreateIndex
CREATE INDEX "sls_invoice_lines_invoice_id_idx" ON "sls_invoice_lines"("invoice_id");

-- CreateIndex
CREATE INDEX "sls_invoice_lines_item_id_idx" ON "sls_invoice_lines"("item_id");

-- CreateIndex
CREATE INDEX "sls_invoice_installments_invoice_id_idx" ON "sls_invoice_installments"("invoice_id");

-- CreateIndex
CREATE INDEX "sls_invoice_materials_invoice_id_idx" ON "sls_invoice_materials"("invoice_id");

-- CreateIndex
CREATE INDEX "sls_invoice_materials_invoice_line_id_idx" ON "sls_invoice_materials"("invoice_line_id");

-- CreateIndex
CREATE INDEX "sls_invoice_materials_item_id_idx" ON "sls_invoice_materials"("item_id");

-- CreateIndex
CREATE INDEX "sls_invoice_costs_invoice_id_idx" ON "sls_invoice_costs"("invoice_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_returns_code_key" ON "sls_returns"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_returns_doc_number_key" ON "sls_returns"("doc_number");

-- CreateIndex
CREATE INDEX "sls_returns_legacy_code_idx" ON "sls_returns"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_returns_branch_id_idx" ON "sls_returns"("branch_id");

-- CreateIndex
CREATE INDEX "sls_returns_customer_id_idx" ON "sls_returns"("customer_id");

-- CreateIndex
CREATE INDEX "sls_returns_fiscal_period_id_idx" ON "sls_returns"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_returns_invoice_id_idx" ON "sls_returns"("invoice_id");

-- CreateIndex
CREATE INDEX "sls_return_lines_return_id_idx" ON "sls_return_lines"("return_id");

-- CreateIndex
CREATE INDEX "sls_return_lines_item_id_idx" ON "sls_return_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_return_receipts_code_key" ON "sls_return_receipts"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_return_receipts_doc_number_key" ON "sls_return_receipts"("doc_number");

-- CreateIndex
CREATE INDEX "sls_return_receipts_legacy_code_idx" ON "sls_return_receipts"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_return_receipts_branch_id_idx" ON "sls_return_receipts"("branch_id");

-- CreateIndex
CREATE INDEX "sls_return_receipts_customer_id_idx" ON "sls_return_receipts"("customer_id");

-- CreateIndex
CREATE INDEX "sls_return_receipts_fiscal_period_id_idx" ON "sls_return_receipts"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_return_receipts_invoice_id_idx" ON "sls_return_receipts"("invoice_id");

-- CreateIndex
CREATE INDEX "sls_return_receipts_return_id_idx" ON "sls_return_receipts"("return_id");

-- CreateIndex
CREATE INDEX "sls_return_receipt_lines_return_receipt_id_idx" ON "sls_return_receipt_lines"("return_receipt_id");

-- CreateIndex
CREATE INDEX "sls_return_receipt_lines_item_id_idx" ON "sls_return_receipt_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_customer_advances_code_key" ON "sls_customer_advances"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_customer_advances_doc_number_key" ON "sls_customer_advances"("doc_number");

-- CreateIndex
CREATE INDEX "sls_customer_advances_legacy_code_idx" ON "sls_customer_advances"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_customer_advances_branch_id_idx" ON "sls_customer_advances"("branch_id");

-- CreateIndex
CREATE INDEX "sls_customer_advances_customer_id_idx" ON "sls_customer_advances"("customer_id");

-- CreateIndex
CREATE INDEX "sls_customer_advances_fiscal_period_id_idx" ON "sls_customer_advances"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_customer_advances_order_id_idx" ON "sls_customer_advances"("order_id");

-- CreateIndex
CREATE INDEX "sls_customer_advances_ar_receipt_id_idx" ON "sls_customer_advances"("ar_receipt_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_invoice_swaps_code_key" ON "sls_invoice_swaps"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_invoice_swaps_doc_number_key" ON "sls_invoice_swaps"("doc_number");

-- CreateIndex
CREATE INDEX "sls_invoice_swaps_legacy_code_idx" ON "sls_invoice_swaps"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_invoice_swaps_branch_id_idx" ON "sls_invoice_swaps"("branch_id");

-- CreateIndex
CREATE INDEX "sls_invoice_swaps_customer_id_idx" ON "sls_invoice_swaps"("customer_id");

-- CreateIndex
CREATE INDEX "sls_invoice_swaps_fiscal_period_id_idx" ON "sls_invoice_swaps"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "sls_invoice_swap_lines_swap_id_idx" ON "sls_invoice_swap_lines"("swap_id");

-- CreateIndex
CREATE INDEX "sls_invoice_swap_lines_from_invoice_id_idx" ON "sls_invoice_swap_lines"("from_invoice_id");

-- CreateIndex
CREATE INDEX "sls_invoice_swap_lines_to_invoice_id_idx" ON "sls_invoice_swap_lines"("to_invoice_id");

-- CreateIndex
CREATE UNIQUE INDEX "sls_forecasts_code_key" ON "sls_forecasts"("code");

-- CreateIndex
CREATE UNIQUE INDEX "sls_forecasts_doc_number_key" ON "sls_forecasts"("doc_number");

-- CreateIndex
CREATE INDEX "sls_forecasts_legacy_code_idx" ON "sls_forecasts"("legacy_code");

-- CreateIndex
CREATE INDEX "sls_forecasts_branch_id_idx" ON "sls_forecasts"("branch_id");

-- CreateIndex
CREATE INDEX "sls_forecasts_customer_id_idx" ON "sls_forecasts"("customer_id");

-- CreateIndex
CREATE INDEX "sls_forecast_lines_forecast_id_idx" ON "sls_forecast_lines"("forecast_id");

-- CreateIndex
CREATE INDEX "sls_forecast_lines_item_id_idx" ON "sls_forecast_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "mfg_boms_doc_number_key" ON "mfg_boms"("doc_number");

-- CreateIndex
CREATE INDEX "mfg_boms_branch_id_idx" ON "mfg_boms"("branch_id");

-- CreateIndex
CREATE INDEX "mfg_boms_location_id_idx" ON "mfg_boms"("location_id");

-- CreateIndex
CREATE INDEX "mfg_boms_source_warehouse_id_idx" ON "mfg_boms"("source_warehouse_id");

-- CreateIndex
CREATE INDEX "mfg_boms_production_warehouse_id_idx" ON "mfg_boms"("production_warehouse_id");

-- CreateIndex
CREATE INDEX "mfg_boms_destination_warehouse_id_idx" ON "mfg_boms"("destination_warehouse_id");

-- CreateIndex
CREATE INDEX "mfg_boms_fiscal_period_id_idx" ON "mfg_boms"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "mfg_boms_requested_by_id_idx" ON "mfg_boms"("requested_by_id");

-- CreateIndex
CREATE INDEX "mfg_boms_requested_partner_id_idx" ON "mfg_boms"("requested_partner_id");

-- CreateIndex
CREATE INDEX "mfg_boms_currency_id_idx" ON "mfg_boms"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_boms_legacy_code_idx" ON "mfg_boms"("legacy_code");

-- CreateIndex
CREATE INDEX "mfg_bom_inputs_bom_id_idx" ON "mfg_bom_inputs"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_bom_inputs_item_id_idx" ON "mfg_bom_inputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_bom_inputs_unit_id_idx" ON "mfg_bom_inputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_bom_inputs_currency_id_idx" ON "mfg_bom_inputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_bom_outputs_bom_id_idx" ON "mfg_bom_outputs"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_bom_outputs_item_id_idx" ON "mfg_bom_outputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_bom_outputs_unit_id_idx" ON "mfg_bom_outputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_bom_outputs_currency_id_idx" ON "mfg_bom_outputs"("currency_id");

-- CreateIndex
CREATE UNIQUE INDEX "mfg_work_orders_doc_number_key" ON "mfg_work_orders"("doc_number");

-- CreateIndex
CREATE INDEX "mfg_work_orders_branch_id_idx" ON "mfg_work_orders"("branch_id");

-- CreateIndex
CREATE INDEX "mfg_work_orders_fiscal_period_id_idx" ON "mfg_work_orders"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "mfg_work_orders_currency_id_idx" ON "mfg_work_orders"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_work_orders_bom_id_idx" ON "mfg_work_orders"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_work_orders_production_rework_id_idx" ON "mfg_work_orders"("production_rework_id");

-- CreateIndex
CREATE INDEX "mfg_work_orders_legacy_code_idx" ON "mfg_work_orders"("legacy_code");

-- CreateIndex
CREATE INDEX "mfg_work_order_inputs_work_order_id_idx" ON "mfg_work_order_inputs"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_inputs_item_id_idx" ON "mfg_work_order_inputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_inputs_unit_id_idx" ON "mfg_work_order_inputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_inputs_currency_id_idx" ON "mfg_work_order_inputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_outputs_work_order_id_idx" ON "mfg_work_order_outputs"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_outputs_item_id_idx" ON "mfg_work_order_outputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_outputs_unit_id_idx" ON "mfg_work_order_outputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_outputs_currency_id_idx" ON "mfg_work_order_outputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_activities_work_order_id_idx" ON "mfg_work_order_activities"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_route_cards_work_order_id_idx" ON "mfg_work_order_route_cards"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_work_order_route_cards_unit_id_idx" ON "mfg_work_order_route_cards"("unit_id");

-- CreateIndex
CREATE UNIQUE INDEX "mfg_material_issues_doc_number_key" ON "mfg_material_issues"("doc_number");

-- CreateIndex
CREATE INDEX "mfg_material_issues_branch_id_idx" ON "mfg_material_issues"("branch_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_fiscal_period_id_idx" ON "mfg_material_issues"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_currency_id_idx" ON "mfg_material_issues"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_bom_id_idx" ON "mfg_material_issues"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_work_order_id_idx" ON "mfg_material_issues"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_production_rework_id_idx" ON "mfg_material_issues"("production_rework_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_inv_stock_movement_id_idx" ON "mfg_material_issues"("inv_stock_movement_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_fin_ledger_entry_id_idx" ON "mfg_material_issues"("fin_ledger_entry_id");

-- CreateIndex
CREATE INDEX "mfg_material_issues_legacy_code_idx" ON "mfg_material_issues"("legacy_code");

-- CreateIndex
CREATE INDEX "mfg_material_issue_inputs_material_issue_id_idx" ON "mfg_material_issue_inputs"("material_issue_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_inputs_item_id_idx" ON "mfg_material_issue_inputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_inputs_unit_id_idx" ON "mfg_material_issue_inputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_inputs_currency_id_idx" ON "mfg_material_issue_inputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_outputs_material_issue_id_idx" ON "mfg_material_issue_outputs"("material_issue_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_outputs_item_id_idx" ON "mfg_material_issue_outputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_outputs_unit_id_idx" ON "mfg_material_issue_outputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_material_issue_outputs_currency_id_idx" ON "mfg_material_issue_outputs"("currency_id");

-- CreateIndex
CREATE UNIQUE INDEX "mfg_material_returns_doc_number_key" ON "mfg_material_returns"("doc_number");

-- CreateIndex
CREATE INDEX "mfg_material_returns_branch_id_idx" ON "mfg_material_returns"("branch_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_fiscal_period_id_idx" ON "mfg_material_returns"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_currency_id_idx" ON "mfg_material_returns"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_bom_id_idx" ON "mfg_material_returns"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_work_order_id_idx" ON "mfg_material_returns"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_material_issue_id_idx" ON "mfg_material_returns"("material_issue_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_production_rework_id_idx" ON "mfg_material_returns"("production_rework_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_inv_stock_movement_id_idx" ON "mfg_material_returns"("inv_stock_movement_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_fin_ledger_entry_id_idx" ON "mfg_material_returns"("fin_ledger_entry_id");

-- CreateIndex
CREATE INDEX "mfg_material_returns_legacy_code_idx" ON "mfg_material_returns"("legacy_code");

-- CreateIndex
CREATE INDEX "mfg_material_return_inputs_material_return_id_idx" ON "mfg_material_return_inputs"("material_return_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_inputs_item_id_idx" ON "mfg_material_return_inputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_inputs_unit_id_idx" ON "mfg_material_return_inputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_inputs_currency_id_idx" ON "mfg_material_return_inputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_outputs_material_return_id_idx" ON "mfg_material_return_outputs"("material_return_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_outputs_item_id_idx" ON "mfg_material_return_outputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_outputs_unit_id_idx" ON "mfg_material_return_outputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_material_return_outputs_currency_id_idx" ON "mfg_material_return_outputs"("currency_id");

-- CreateIndex
CREATE UNIQUE INDEX "mfg_production_entries_doc_number_key" ON "mfg_production_entries"("doc_number");

-- CreateIndex
CREATE INDEX "mfg_production_entries_branch_id_idx" ON "mfg_production_entries"("branch_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_fiscal_period_id_idx" ON "mfg_production_entries"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_currency_id_idx" ON "mfg_production_entries"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_bom_id_idx" ON "mfg_production_entries"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_work_order_id_idx" ON "mfg_production_entries"("work_order_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_material_issue_id_idx" ON "mfg_production_entries"("material_issue_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_material_return_id_idx" ON "mfg_production_entries"("material_return_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_production_rework_id_idx" ON "mfg_production_entries"("production_rework_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_inv_stock_movement_id_idx" ON "mfg_production_entries"("inv_stock_movement_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_fin_ledger_entry_id_idx" ON "mfg_production_entries"("fin_ledger_entry_id");

-- CreateIndex
CREATE INDEX "mfg_production_entries_legacy_code_idx" ON "mfg_production_entries"("legacy_code");

-- CreateIndex
CREATE INDEX "mfg_production_entry_inputs_production_entry_id_idx" ON "mfg_production_entry_inputs"("production_entry_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_inputs_item_id_idx" ON "mfg_production_entry_inputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_inputs_unit_id_idx" ON "mfg_production_entry_inputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_inputs_currency_id_idx" ON "mfg_production_entry_inputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_outputs_production_entry_id_idx" ON "mfg_production_entry_outputs"("production_entry_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_outputs_item_id_idx" ON "mfg_production_entry_outputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_outputs_unit_id_idx" ON "mfg_production_entry_outputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_production_entry_outputs_currency_id_idx" ON "mfg_production_entry_outputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_production_boms_production_entry_id_idx" ON "mfg_production_boms"("production_entry_id");

-- CreateIndex
CREATE INDEX "mfg_production_boms_produced_item_id_idx" ON "mfg_production_boms"("produced_item_id");

-- CreateIndex
CREATE INDEX "mfg_production_boms_item_id_idx" ON "mfg_production_boms"("item_id");

-- CreateIndex
CREATE INDEX "mfg_production_boms_unit_id_idx" ON "mfg_production_boms"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_production_boms_bom_id_idx" ON "mfg_production_boms"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_production_boms_bom_output_line_id_idx" ON "mfg_production_boms"("bom_output_line_id");

-- CreateIndex
CREATE UNIQUE INDEX "mfg_production_reworks_doc_number_key" ON "mfg_production_reworks"("doc_number");

-- CreateIndex
CREATE INDEX "mfg_production_reworks_branch_id_idx" ON "mfg_production_reworks"("branch_id");

-- CreateIndex
CREATE INDEX "mfg_production_reworks_fiscal_period_id_idx" ON "mfg_production_reworks"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "mfg_production_reworks_currency_id_idx" ON "mfg_production_reworks"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_production_reworks_bom_id_idx" ON "mfg_production_reworks"("bom_id");

-- CreateIndex
CREATE INDEX "mfg_production_reworks_production_entry_id_idx" ON "mfg_production_reworks"("production_entry_id");

-- CreateIndex
CREATE INDEX "mfg_production_reworks_legacy_code_idx" ON "mfg_production_reworks"("legacy_code");

-- CreateIndex
CREATE INDEX "mfg_production_rework_inputs_production_rework_id_idx" ON "mfg_production_rework_inputs"("production_rework_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_inputs_item_id_idx" ON "mfg_production_rework_inputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_inputs_unit_id_idx" ON "mfg_production_rework_inputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_inputs_currency_id_idx" ON "mfg_production_rework_inputs"("currency_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_outputs_production_rework_id_idx" ON "mfg_production_rework_outputs"("production_rework_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_outputs_item_id_idx" ON "mfg_production_rework_outputs"("item_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_outputs_unit_id_idx" ON "mfg_production_rework_outputs"("unit_id");

-- CreateIndex
CREATE INDEX "mfg_production_rework_outputs_currency_id_idx" ON "mfg_production_rework_outputs"("currency_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_categories_code_key" ON "fa_asset_categories"("code");

-- CreateIndex
CREATE INDEX "fa_asset_categories_legacy_code_idx" ON "fa_asset_categories"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_asset_categories_tax_category_id_idx" ON "fa_asset_categories"("tax_category_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_category_taxes_code_key" ON "fa_asset_category_taxes"("code");

-- CreateIndex
CREATE INDEX "fa_asset_category_taxes_legacy_code_idx" ON "fa_asset_category_taxes"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fa_depreciation_categories_code_key" ON "fa_depreciation_categories"("code");

-- CreateIndex
CREATE INDEX "fa_depreciation_categories_legacy_code_idx" ON "fa_depreciation_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_departments_code_key" ON "fa_asset_departments"("code");

-- CreateIndex
CREATE INDEX "fa_asset_departments_legacy_code_idx" ON "fa_asset_departments"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_asset_departments_location_id_idx" ON "fa_asset_departments"("location_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_assets_code_key" ON "fa_assets"("code");

-- CreateIndex
CREATE INDEX "fa_assets_legacy_code_idx" ON "fa_assets"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_assets_category_id_idx" ON "fa_assets"("category_id");

-- CreateIndex
CREATE INDEX "fa_assets_department_id_idx" ON "fa_assets"("department_id");

-- CreateIndex
CREATE INDEX "fa_assets_registration_id_idx" ON "fa_assets"("registration_id");

-- CreateIndex
CREATE INDEX "fa_assets_branch_id_idx" ON "fa_assets"("branch_id");

-- CreateIndex
CREATE INDEX "fa_asset_movements_asset_id_movement_date_idx" ON "fa_asset_movements"("asset_id", "movement_date");

-- CreateIndex
CREATE INDEX "fa_asset_movements_source_doc_type_source_id_idx" ON "fa_asset_movements"("source_doc_type", "source_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_requisitions_code_key" ON "fa_asset_requisitions"("code");

-- CreateIndex
CREATE INDEX "fa_asset_requisitions_legacy_code_idx" ON "fa_asset_requisitions"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_asset_requisitions_branch_id_idx" ON "fa_asset_requisitions"("branch_id");

-- CreateIndex
CREATE INDEX "fa_asset_requisition_lines_requisition_id_idx" ON "fa_asset_requisition_lines"("requisition_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_quotations_code_key" ON "fa_asset_quotations"("code");

-- CreateIndex
CREATE INDEX "fa_asset_quotations_legacy_code_idx" ON "fa_asset_quotations"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_asset_quotations_requisition_id_idx" ON "fa_asset_quotations"("requisition_id");

-- CreateIndex
CREATE INDEX "fa_asset_quotations_branch_id_idx" ON "fa_asset_quotations"("branch_id");

-- CreateIndex
CREATE INDEX "fa_asset_quotation_lines_quotation_id_idx" ON "fa_asset_quotation_lines"("quotation_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_orders_code_key" ON "fa_asset_orders"("code");

-- CreateIndex
CREATE INDEX "fa_asset_orders_legacy_code_idx" ON "fa_asset_orders"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_asset_orders_requisition_id_idx" ON "fa_asset_orders"("requisition_id");

-- CreateIndex
CREATE INDEX "fa_asset_orders_quotation_id_idx" ON "fa_asset_orders"("quotation_id");

-- CreateIndex
CREATE INDEX "fa_asset_orders_branch_id_idx" ON "fa_asset_orders"("branch_id");

-- CreateIndex
CREATE INDEX "fa_asset_order_lines_order_id_idx" ON "fa_asset_order_lines"("order_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_acquisitions_code_key" ON "fa_acquisitions"("code");

-- CreateIndex
CREATE INDEX "fa_acquisitions_legacy_code_idx" ON "fa_acquisitions"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_acquisitions_requisition_id_idx" ON "fa_acquisitions"("requisition_id");

-- CreateIndex
CREATE INDEX "fa_acquisitions_quotation_id_idx" ON "fa_acquisitions"("quotation_id");

-- CreateIndex
CREATE INDEX "fa_acquisitions_order_id_idx" ON "fa_acquisitions"("order_id");

-- CreateIndex
CREATE INDEX "fa_acquisitions_ap_payment_id_idx" ON "fa_acquisitions"("ap_payment_id");

-- CreateIndex
CREATE INDEX "fa_acquisitions_ledger_entry_id_idx" ON "fa_acquisitions"("ledger_entry_id");

-- CreateIndex
CREATE INDEX "fa_acquisitions_branch_id_idx" ON "fa_acquisitions"("branch_id");

-- CreateIndex
CREATE INDEX "fa_acquisition_lines_acquisition_id_idx" ON "fa_acquisition_lines"("acquisition_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_asset_registrations_code_key" ON "fa_asset_registrations"("code");

-- CreateIndex
CREATE INDEX "fa_asset_registrations_legacy_code_idx" ON "fa_asset_registrations"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_asset_registrations_acquisition_id_idx" ON "fa_asset_registrations"("acquisition_id");

-- CreateIndex
CREATE INDEX "fa_asset_registrations_branch_id_idx" ON "fa_asset_registrations"("branch_id");

-- CreateIndex
CREATE INDEX "fa_asset_registration_lines_registration_id_idx" ON "fa_asset_registration_lines"("registration_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_depreciation_runs_doc_number_key" ON "fa_depreciation_runs"("doc_number");

-- CreateIndex
CREATE INDEX "fa_depreciation_runs_legacy_code_idx" ON "fa_depreciation_runs"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_depreciation_runs_fiscal_period_id_idx" ON "fa_depreciation_runs"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "fa_depreciation_runs_ledger_entry_id_idx" ON "fa_depreciation_runs"("ledger_entry_id");

-- CreateIndex
CREATE INDEX "fa_depreciation_runs_branch_id_idx" ON "fa_depreciation_runs"("branch_id");

-- CreateIndex
CREATE INDEX "fa_depreciation_run_lines_depreciation_run_id_idx" ON "fa_depreciation_run_lines"("depreciation_run_id");

-- CreateIndex
CREATE INDEX "fa_depreciation_run_lines_asset_id_idx" ON "fa_depreciation_run_lines"("asset_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_transfers_code_key" ON "fa_transfers"("code");

-- CreateIndex
CREATE INDEX "fa_transfers_legacy_code_idx" ON "fa_transfers"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_transfers_branch_id_idx" ON "fa_transfers"("branch_id");

-- CreateIndex
CREATE INDEX "fa_transfer_lines_transfer_id_idx" ON "fa_transfer_lines"("transfer_id");

-- CreateIndex
CREATE INDEX "fa_transfer_lines_asset_id_idx" ON "fa_transfer_lines"("asset_id");

-- CreateIndex
CREATE INDEX "fa_transfer_lines_from_department_id_idx" ON "fa_transfer_lines"("from_department_id");

-- CreateIndex
CREATE INDEX "fa_transfer_lines_to_department_id_idx" ON "fa_transfer_lines"("to_department_id");

-- CreateIndex
CREATE UNIQUE INDEX "fa_disposals_code_key" ON "fa_disposals"("code");

-- CreateIndex
CREATE INDEX "fa_disposals_legacy_code_idx" ON "fa_disposals"("legacy_code");

-- CreateIndex
CREATE INDEX "fa_disposals_ledger_entry_id_idx" ON "fa_disposals"("ledger_entry_id");

-- CreateIndex
CREATE INDEX "fa_disposals_branch_id_idx" ON "fa_disposals"("branch_id");

-- CreateIndex
CREATE INDEX "fa_disposal_lines_disposal_id_idx" ON "fa_disposal_lines"("disposal_id");

-- CreateIndex
CREATE INDEX "fa_disposal_lines_asset_id_idx" ON "fa_disposal_lines"("asset_id");

-- CreateIndex
CREATE UNIQUE INDEX "pos_area_categories_code_key" ON "pos_area_categories"("code");

-- CreateIndex
CREATE INDEX "pos_area_categories_legacy_code_idx" ON "pos_area_categories"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "pos_areas_code_key" ON "pos_areas"("code");

-- CreateIndex
CREATE INDEX "pos_areas_legacy_code_idx" ON "pos_areas"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_areas_category_id_idx" ON "pos_areas"("category_id");

-- CreateIndex
CREATE INDEX "pos_areas_branch_id_idx" ON "pos_areas"("branch_id");

-- CreateIndex
CREATE UNIQUE INDEX "pos_terminals_code_key" ON "pos_terminals"("code");

-- CreateIndex
CREATE INDEX "pos_terminals_legacy_code_idx" ON "pos_terminals"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_terminals_branch_id_idx" ON "pos_terminals"("branch_id");

-- CreateIndex
CREATE INDEX "pos_terminals_location_id_idx" ON "pos_terminals"("location_id");

-- CreateIndex
CREATE INDEX "pos_terminals_warehouse_id_idx" ON "pos_terminals"("warehouse_id");

-- CreateIndex
CREATE UNIQUE INDEX "pos_transaction_types_code_key" ON "pos_transaction_types"("code");

-- CreateIndex
CREATE INDEX "pos_transaction_types_legacy_code_idx" ON "pos_transaction_types"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_item_prices_legacy_code_idx" ON "pos_item_prices"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_item_prices_item_id_idx" ON "pos_item_prices"("item_id");

-- CreateIndex
CREATE INDEX "pos_item_prices_area_id_idx" ON "pos_item_prices"("area_id");

-- CreateIndex
CREATE INDEX "pos_item_prices_category_id_idx" ON "pos_item_prices"("category_id");

-- CreateIndex
CREATE INDEX "pos_item_price_tiers_item_price_id_idx" ON "pos_item_price_tiers"("item_price_id");

-- CreateIndex
CREATE UNIQUE INDEX "pos_price_agreements_doc_number_key" ON "pos_price_agreements"("doc_number");

-- CreateIndex
CREATE INDEX "pos_price_agreements_legacy_code_idx" ON "pos_price_agreements"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_price_agreements_branch_id_idx" ON "pos_price_agreements"("branch_id");

-- CreateIndex
CREATE INDEX "pos_price_agreements_location_id_idx" ON "pos_price_agreements"("location_id");

-- CreateIndex
CREATE INDEX "pos_price_agreements_partner_id_idx" ON "pos_price_agreements"("partner_id");

-- CreateIndex
CREATE INDEX "pos_price_agreements_partner_contact_id_idx" ON "pos_price_agreements"("partner_contact_id");

-- CreateIndex
CREATE INDEX "pos_price_agreements_fiscal_period_id_idx" ON "pos_price_agreements"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pos_price_agreement_lines_agreement_id_idx" ON "pos_price_agreement_lines"("agreement_id");

-- CreateIndex
CREATE INDEX "pos_price_agreement_lines_item_id_idx" ON "pos_price_agreement_lines"("item_id");

-- CreateIndex
CREATE UNIQUE INDEX "pos_promotions_code_key" ON "pos_promotions"("code");

-- CreateIndex
CREATE INDEX "pos_promotions_legacy_code_idx" ON "pos_promotions"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_promotions_area_id_idx" ON "pos_promotions"("area_id");

-- CreateIndex
CREATE INDEX "pos_bonus_rules_legacy_code_idx" ON "pos_bonus_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_bonus_rules_promotion_id_idx" ON "pos_bonus_rules"("promotion_id");

-- CreateIndex
CREATE INDEX "pos_bonus_rules_trigger_item_id_idx" ON "pos_bonus_rules"("trigger_item_id");

-- CreateIndex
CREATE INDEX "pos_bonus_rules_bonus_item_id_idx" ON "pos_bonus_rules"("bonus_item_id");

-- CreateIndex
CREATE INDEX "pos_bonus_rules_category_id_idx" ON "pos_bonus_rules"("category_id");

-- CreateIndex
CREATE INDEX "pos_bonus_transactions_bonus_rule_id_idx" ON "pos_bonus_transactions"("bonus_rule_id");

-- CreateIndex
CREATE INDEX "pos_bonus_transactions_sale_invoice_id_idx" ON "pos_bonus_transactions"("sale_invoice_id");

-- CreateIndex
CREATE INDEX "pos_substitution_rules_legacy_code_idx" ON "pos_substitution_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_substitution_rules_promotion_id_idx" ON "pos_substitution_rules"("promotion_id");

-- CreateIndex
CREATE INDEX "pos_substitution_rules_from_item_id_idx" ON "pos_substitution_rules"("from_item_id");

-- CreateIndex
CREATE INDEX "pos_substitution_rules_category_id_idx" ON "pos_substitution_rules"("category_id");

-- CreateIndex
CREATE INDEX "pos_additional_item_rules_legacy_code_idx" ON "pos_additional_item_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_additional_item_rules_promotion_id_idx" ON "pos_additional_item_rules"("promotion_id");

-- CreateIndex
CREATE INDEX "pos_additional_item_rules_base_item_id_idx" ON "pos_additional_item_rules"("base_item_id");

-- CreateIndex
CREATE INDEX "pos_additional_item_rules_additional_item_id_idx" ON "pos_additional_item_rules"("additional_item_id");

-- CreateIndex
CREATE INDEX "pos_discount_rules_legacy_code_idx" ON "pos_discount_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_discount_rules_promotion_id_idx" ON "pos_discount_rules"("promotion_id");

-- CreateIndex
CREATE INDEX "pos_discount_rules_item_id_idx" ON "pos_discount_rules"("item_id");

-- CreateIndex
CREATE INDEX "pos_discount_rules_item_category_id_idx" ON "pos_discount_rules"("item_category_id");

-- CreateIndex
CREATE INDEX "pos_discount_rules_partner_category_id_idx" ON "pos_discount_rules"("partner_category_id");

-- CreateIndex
CREATE INDEX "pos_point_rules_legacy_code_idx" ON "pos_point_rules"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_point_rules_item_id_idx" ON "pos_point_rules"("item_id");

-- CreateIndex
CREATE INDEX "pos_point_rules_item_category_id_idx" ON "pos_point_rules"("item_category_id");

-- CreateIndex
CREATE INDEX "pos_point_transactions_partner_id_idx" ON "pos_point_transactions"("partner_id");

-- CreateIndex
CREATE INDEX "pos_point_transactions_sale_invoice_id_idx" ON "pos_point_transactions"("sale_invoice_id");

-- CreateIndex
CREATE INDEX "pos_point_transactions_rule_id_idx" ON "pos_point_transactions"("rule_id");

-- CreateIndex
CREATE INDEX "pos_point_transactions_category_id_idx" ON "pos_point_transactions"("category_id");

-- CreateIndex
CREATE UNIQUE INDEX "pos_vouchers_code_key" ON "pos_vouchers"("code");

-- CreateIndex
CREATE INDEX "pos_vouchers_legacy_code_idx" ON "pos_vouchers"("legacy_code");

-- CreateIndex
CREATE INDEX "pos_vouchers_issued_to_partner_id_idx" ON "pos_vouchers"("issued_to_partner_id");

-- CreateIndex
CREATE INDEX "pos_vouchers_redeemed_sale_invoice_id_idx" ON "pos_vouchers"("redeemed_sale_invoice_id");

-- CreateIndex
CREATE INDEX "pln_reorder_policies_item_id_idx" ON "pln_reorder_policies"("item_id");

-- CreateIndex
CREATE INDEX "pln_reorder_policies_legacy_code_idx" ON "pln_reorder_policies"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "pln_reorder_policies_item_id_warehouse_id_key" ON "pln_reorder_policies"("item_id", "warehouse_id");

-- CreateIndex
CREATE INDEX "pln_demand_forecasts_fiscal_period_id_idx" ON "pln_demand_forecasts"("fiscal_period_id");

-- CreateIndex
CREATE INDEX "pln_demand_forecasts_item_id_warehouse_id_idx" ON "pln_demand_forecasts"("item_id", "warehouse_id");

-- CreateIndex
CREATE INDEX "pln_demand_forecasts_legacy_code_idx" ON "pln_demand_forecasts"("legacy_code");

-- CreateIndex
CREATE UNIQUE INDEX "pln_demand_forecasts_item_id_warehouse_id_fiscal_period_id_key" ON "pln_demand_forecasts"("item_id", "warehouse_id", "fiscal_period_id");

-- CreateIndex
CREATE UNIQUE INDEX "pln_mrp_runs_doc_number_key" ON "pln_mrp_runs"("doc_number");

-- CreateIndex
CREATE INDEX "pln_mrp_runs_status_idx" ON "pln_mrp_runs"("status");

-- CreateIndex
CREATE INDEX "pln_mrp_runs_plan_horizon_start_plan_horizon_end_idx" ON "pln_mrp_runs"("plan_horizon_start", "plan_horizon_end");

-- CreateIndex
CREATE INDEX "pln_mrp_runs_legacy_code_idx" ON "pln_mrp_runs"("legacy_code");

-- CreateIndex
CREATE INDEX "pln_mrp_run_lines_mrp_run_id_idx" ON "pln_mrp_run_lines"("mrp_run_id");

-- CreateIndex
CREATE INDEX "pln_mrp_run_lines_item_id_warehouse_id_fiscal_period_id_idx" ON "pln_mrp_run_lines"("item_id", "warehouse_id", "fiscal_period_id");

-- CreateIndex
CREATE INDEX "pln_replenishment_suggestions_status_idx" ON "pln_replenishment_suggestions"("status");

-- CreateIndex
CREATE INDEX "pln_replenishment_suggestions_item_id_warehouse_id_status_idx" ON "pln_replenishment_suggestions"("item_id", "warehouse_id", "status");

-- CreateIndex
CREATE INDEX "pln_replenishment_suggestions_action_due_date_idx" ON "pln_replenishment_suggestions"("action_due_date");

-- CreateIndex
CREATE INDEX "pln_replenishment_suggestions_mrp_run_id_idx" ON "pln_replenishment_suggestions"("mrp_run_id");

-- CreateIndex
CREATE INDEX "pln_replenishment_suggestions_converted_to_doc_type_convert_idx" ON "pln_replenishment_suggestions"("converted_to_doc_type", "converted_to_doc_id");

-- CreateIndex
CREATE INDEX "pln_replenishment_suggestions_legacy_code_idx" ON "pln_replenishment_suggestions"("legacy_code");

-- AddForeignKey
ALTER TABLE "md_cost_centers" ADD CONSTRAINT "md_cost_centers_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_divisions" ADD CONSTRAINT "md_divisions_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_subdivisions" ADD CONSTRAINT "md_subdivisions_division_id_fkey" FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_subdivisions" ADD CONSTRAINT "md_subdivisions_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "md_subdivisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "md_projects" ADD CONSTRAINT "md_projects_parent_id_fkey" FOREIGN KEY ("parent_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_journal_lines" ADD CONSTRAINT "fin_journal_lines_journal_entry_id_fkey" FOREIGN KEY ("journal_entry_id") REFERENCES "fin_journal_entries"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_journal_lines" ADD CONSTRAINT "fin_journal_lines_cost_center_id_fkey" FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_journal_lines" ADD CONSTRAINT "fin_journal_lines_division_id_fkey" FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_journal_lines" ADD CONSTRAINT "fin_journal_lines_subdivision_id_fkey" FOREIGN KEY ("subdivision_id") REFERENCES "md_subdivisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_journal_lines" ADD CONSTRAINT "fin_journal_lines_project_id_fkey" FOREIGN KEY ("project_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_ledger_entries" ADD CONSTRAINT "fin_ledger_entries_cost_center_id_fkey" FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_ledger_entries" ADD CONSTRAINT "fin_ledger_entries_division_id_fkey" FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_ledger_entries" ADD CONSTRAINT "fin_ledger_entries_subdivision_id_fkey" FOREIGN KEY ("subdivision_id") REFERENCES "md_subdivisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_ledger_entries" ADD CONSTRAINT "fin_ledger_entries_project_id_fkey" FOREIGN KEY ("project_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_cash_bank_lines" ADD CONSTRAINT "fin_cash_bank_lines_cash_bank_transaction_id_fkey" FOREIGN KEY ("cash_bank_transaction_id") REFERENCES "fin_cash_bank_transactions"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_cash_bank_lines" ADD CONSTRAINT "fin_cash_bank_lines_cost_center_id_fkey" FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_cash_bank_lines" ADD CONSTRAINT "fin_cash_bank_lines_division_id_fkey" FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_cash_bank_lines" ADD CONSTRAINT "fin_cash_bank_lines_subdivision_id_fkey" FOREIGN KEY ("subdivision_id") REFERENCES "md_subdivisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_cash_bank_lines" ADD CONSTRAINT "fin_cash_bank_lines_project_id_fkey" FOREIGN KEY ("project_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_payment_instruments" ADD CONSTRAINT "fin_payment_instruments_ar_receipt_id_fkey" FOREIGN KEY ("ar_receipt_id") REFERENCES "fin_ar_receipts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_payment_instruments" ADD CONSTRAINT "fin_payment_instruments_ap_payment_id_fkey" FOREIGN KEY ("ap_payment_id") REFERENCES "fin_ap_payments"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_payment_instruments" ADD CONSTRAINT "fin_payment_instruments_giro_id_fkey" FOREIGN KEY ("giro_id") REFERENCES "fin_giros"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_settlement_allocations" ADD CONSTRAINT "fin_settlement_allocations_ar_receipt_id_fkey" FOREIGN KEY ("ar_receipt_id") REFERENCES "fin_ar_receipts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_settlement_allocations" ADD CONSTRAINT "fin_settlement_allocations_ap_payment_id_fkey" FOREIGN KEY ("ap_payment_id") REFERENCES "fin_ap_payments"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_settlement_allocations" ADD CONSTRAINT "fin_settlement_allocations_ledger_entry_id_fkey" FOREIGN KEY ("ledger_entry_id") REFERENCES "fin_ledger_entries"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_budget_realizations" ADD CONSTRAINT "fin_budget_realizations_cost_center_id_fkey" FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_budget_realizations" ADD CONSTRAINT "fin_budget_realizations_division_id_fkey" FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_budget_realizations" ADD CONSTRAINT "fin_budget_realizations_subdivision_id_fkey" FOREIGN KEY ("subdivision_id") REFERENCES "md_subdivisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_budget_realizations" ADD CONSTRAINT "fin_budget_realizations_project_id_fkey" FOREIGN KEY ("project_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_period_closings" ADD CONSTRAINT "fin_period_closings_closing_journal_entry_id_fkey" FOREIGN KEY ("closing_journal_entry_id") REFERENCES "fin_journal_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_posting_rule_lines" ADD CONSTRAINT "fin_posting_rule_lines_rule_id_fkey" FOREIGN KEY ("rule_id") REFERENCES "fin_posting_rules"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_tax_entries" ADD CONSTRAINT "fin_tax_entries_ledger_entry_id_fkey" FOREIGN KEY ("ledger_entry_id") REFERENCES "fin_ledger_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_withholding_tax_certificates" ADD CONSTRAINT "fin_withholding_tax_certificates_tax_entry_id_fkey" FOREIGN KEY ("tax_entry_id") REFERENCES "fin_tax_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_fx_revaluation_runs" ADD CONSTRAINT "fin_fx_revaluation_runs_closing_journal_entry_id_fkey" FOREIGN KEY ("closing_journal_entry_id") REFERENCES "fin_journal_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_fx_revaluation_lines" ADD CONSTRAINT "fin_fx_revaluation_lines_revaluation_run_id_fkey" FOREIGN KEY ("revaluation_run_id") REFERENCES "fin_fx_revaluation_runs"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_bank_statement_lines" ADD CONSTRAINT "fin_bank_statement_lines_statement_id_fkey" FOREIGN KEY ("statement_id") REFERENCES "fin_bank_statements"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_bank_statement_lines" ADD CONSTRAINT "fin_bank_statement_lines_matched_ledger_entry_id_fkey" FOREIGN KEY ("matched_ledger_entry_id") REFERENCES "fin_ledger_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_bank_statement_lines" ADD CONSTRAINT "fin_bank_statement_lines_matched_cash_bank_transaction_id_fkey" FOREIGN KEY ("matched_cash_bank_transaction_id") REFERENCES "fin_cash_bank_transactions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_recurring_journal_template_lines" ADD CONSTRAINT "fin_recurring_journal_template_lines_template_id_fkey" FOREIGN KEY ("template_id") REFERENCES "fin_recurring_journal_templates"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_recurring_journal_template_lines" ADD CONSTRAINT "fin_recurring_journal_template_lines_cost_center_id_fkey" FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_recurring_journal_template_lines" ADD CONSTRAINT "fin_recurring_journal_template_lines_division_id_fkey" FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_recurring_journal_template_lines" ADD CONSTRAINT "fin_recurring_journal_template_lines_project_id_fkey" FOREIGN KEY ("project_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_accrual_schedules" ADD CONSTRAINT "fin_accrual_schedules_cost_center_id_fkey" FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_report_sections" ADD CONSTRAINT "fin_report_sections_report_id_fkey" FOREIGN KEY ("report_id") REFERENCES "fin_report_definitions"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_report_sections" ADD CONSTRAINT "fin_report_sections_parent_section_id_fkey" FOREIGN KEY ("parent_section_id") REFERENCES "fin_report_sections"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_report_lines" ADD CONSTRAINT "fin_report_lines_section_id_fkey" FOREIGN KEY ("section_id") REFERENCES "fin_report_sections"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_collection_activities" ADD CONSTRAINT "fin_collection_activities_dunning_rule_id_fkey" FOREIGN KEY ("dunning_rule_id") REFERENCES "fin_dunning_rules"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_collection_activities" ADD CONSTRAINT "fin_collection_activities_due_ledger_entry_id_fkey" FOREIGN KEY ("due_ledger_entry_id") REFERENCES "fin_ledger_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_intercompany_transactions" ADD CONSTRAINT "fin_intercompany_transactions_rule_id_fkey" FOREIGN KEY ("rule_id") REFERENCES "fin_intercompany_rules"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_intercompany_transactions" ADD CONSTRAINT "fin_intercompany_transactions_from_journal_entry_id_fkey" FOREIGN KEY ("from_journal_entry_id") REFERENCES "fin_journal_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fin_intercompany_transactions" ADD CONSTRAINT "fin_intercompany_transactions_to_journal_entry_id_fkey" FOREIGN KEY ("to_journal_entry_id") REFERENCES "fin_journal_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_movements" ADD CONSTRAINT "inv_stock_movements_related_movement_id_fkey" FOREIGN KEY ("related_movement_id") REFERENCES "inv_stock_movements"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_movement_lines" ADD CONSTRAINT "inv_stock_movement_lines_stock_movement_id_fkey" FOREIGN KEY ("stock_movement_id") REFERENCES "inv_stock_movements"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_movement_lines" ADD CONSTRAINT "inv_stock_movement_lines_related_line_id_fkey" FOREIGN KEY ("related_line_id") REFERENCES "inv_stock_movement_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_movement_lines" ADD CONSTRAINT "inv_stock_movement_lines_bin_id_fkey" FOREIGN KEY ("bin_id") REFERENCES "inv_bins"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_movement_lines" ADD CONSTRAINT "inv_stock_movement_lines_lot_id_fkey" FOREIGN KEY ("lot_id") REFERENCES "inv_lots"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_movement_lines" ADD CONSTRAINT "inv_stock_movement_lines_serial_id_fkey" FOREIGN KEY ("serial_id") REFERENCES "inv_serials"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_opening_stock_lines" ADD CONSTRAINT "inv_opening_stock_lines_opening_stock_id_fkey" FOREIGN KEY ("opening_stock_id") REFERENCES "inv_opening_stocks"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_opening_stock_lines" ADD CONSTRAINT "inv_opening_stock_lines_bin_id_fkey" FOREIGN KEY ("bin_id") REFERENCES "inv_bins"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_opening_stock_lines" ADD CONSTRAINT "inv_opening_stock_lines_lot_id_fkey" FOREIGN KEY ("lot_id") REFERENCES "inv_lots"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_opening_stock_lines" ADD CONSTRAINT "inv_opening_stock_lines_serial_id_fkey" FOREIGN KEY ("serial_id") REFERENCES "inv_serials"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_count_lines" ADD CONSTRAINT "inv_stock_count_lines_stock_count_id_fkey" FOREIGN KEY ("stock_count_id") REFERENCES "inv_stock_counts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_count_lines" ADD CONSTRAINT "inv_stock_count_lines_bin_id_fkey" FOREIGN KEY ("bin_id") REFERENCES "inv_bins"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_count_lines" ADD CONSTRAINT "inv_stock_count_lines_lot_id_fkey" FOREIGN KEY ("lot_id") REFERENCES "inv_lots"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_count_lines" ADD CONSTRAINT "inv_stock_count_lines_serial_id_fkey" FOREIGN KEY ("serial_id") REFERENCES "inv_serials"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_adjustments" ADD CONSTRAINT "inv_stock_adjustments_stock_count_id_fkey" FOREIGN KEY ("stock_count_id") REFERENCES "inv_stock_counts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_adjustment_lines" ADD CONSTRAINT "inv_stock_adjustment_lines_stock_adjustment_id_fkey" FOREIGN KEY ("stock_adjustment_id") REFERENCES "inv_stock_adjustments"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_adjustment_lines" ADD CONSTRAINT "inv_stock_adjustment_lines_count_line_id_fkey" FOREIGN KEY ("count_line_id") REFERENCES "inv_stock_count_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_adjustment_lines" ADD CONSTRAINT "inv_stock_adjustment_lines_bin_id_fkey" FOREIGN KEY ("bin_id") REFERENCES "inv_bins"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_adjustment_lines" ADD CONSTRAINT "inv_stock_adjustment_lines_lot_id_fkey" FOREIGN KEY ("lot_id") REFERENCES "inv_lots"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_adjustment_lines" ADD CONSTRAINT "inv_stock_adjustment_lines_serial_id_fkey" FOREIGN KEY ("serial_id") REFERENCES "inv_serials"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_cost_recalculation_lines" ADD CONSTRAINT "inv_cost_recalculation_lines_cost_recalculation_id_fkey" FOREIGN KEY ("cost_recalculation_id") REFERENCES "inv_cost_recalculations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_serials" ADD CONSTRAINT "inv_serials_lot_id_fkey" FOREIGN KEY ("lot_id") REFERENCES "inv_lots"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_serials" ADD CONSTRAINT "inv_serials_current_bin_id_fkey" FOREIGN KEY ("current_bin_id") REFERENCES "inv_bins"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_serials" ADD CONSTRAINT "inv_serials_last_movement_id_fkey" FOREIGN KEY ("last_movement_id") REFERENCES "inv_stock_movements"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_reservations" ADD CONSTRAINT "inv_stock_reservations_bin_id_fkey" FOREIGN KEY ("bin_id") REFERENCES "inv_bins"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_reservations" ADD CONSTRAINT "inv_stock_reservations_lot_id_fkey" FOREIGN KEY ("lot_id") REFERENCES "inv_lots"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "inv_stock_reservations" ADD CONSTRAINT "inv_stock_reservations_fulfilled_by_movement_id_fkey" FOREIGN KEY ("fulfilled_by_movement_id") REFERENCES "inv_stock_movements"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_requisition_lines" ADD CONSTRAINT "pur_requisition_lines_requisition_id_fkey" FOREIGN KEY ("requisition_id") REFERENCES "pur_requisitions"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_rfqs" ADD CONSTRAINT "pur_rfqs_requisition_id_fkey" FOREIGN KEY ("requisition_id") REFERENCES "pur_requisitions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_rfq_suppliers" ADD CONSTRAINT "pur_rfq_suppliers_rfq_id_fkey" FOREIGN KEY ("rfq_id") REFERENCES "pur_rfqs"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_quotations" ADD CONSTRAINT "pur_quotations_rfq_id_fkey" FOREIGN KEY ("rfq_id") REFERENCES "pur_rfqs"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_quotation_lines" ADD CONSTRAINT "pur_quotation_lines_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "pur_quotations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_bid_selection_quotations" ADD CONSTRAINT "pur_bid_selection_quotations_bid_selection_id_fkey" FOREIGN KEY ("bid_selection_id") REFERENCES "pur_bid_selections"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_bid_selection_quotations" ADD CONSTRAINT "pur_bid_selection_quotations_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "pur_quotations"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_bid_selection_lines" ADD CONSTRAINT "pur_bid_selection_lines_bid_selection_id_fkey" FOREIGN KEY ("bid_selection_id") REFERENCES "pur_bid_selections"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_bid_selection_lines" ADD CONSTRAINT "pur_bid_selection_lines_quotation_line_id_fkey" FOREIGN KEY ("quotation_line_id") REFERENCES "pur_quotation_lines"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_orders" ADD CONSTRAINT "pur_orders_bid_selection_id_fkey" FOREIGN KEY ("bid_selection_id") REFERENCES "pur_bid_selections"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_order_lines" ADD CONSTRAINT "pur_order_lines_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "pur_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_goods_receipts" ADD CONSTRAINT "pur_goods_receipts_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "pur_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_goods_receipt_lines" ADD CONSTRAINT "pur_goods_receipt_lines_goods_receipt_id_fkey" FOREIGN KEY ("goods_receipt_id") REFERENCES "pur_goods_receipts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_goods_receipt_lines" ADD CONSTRAINT "pur_goods_receipt_lines_order_line_id_fkey" FOREIGN KEY ("order_line_id") REFERENCES "pur_order_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_invoices" ADD CONSTRAINT "pur_invoices_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "pur_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_invoices" ADD CONSTRAINT "pur_invoices_goods_receipt_id_fkey" FOREIGN KEY ("goods_receipt_id") REFERENCES "pur_goods_receipts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_invoice_lines" ADD CONSTRAINT "pur_invoice_lines_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "pur_invoices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_invoice_lines" ADD CONSTRAINT "pur_invoice_lines_order_line_id_fkey" FOREIGN KEY ("order_line_id") REFERENCES "pur_order_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_invoice_lines" ADD CONSTRAINT "pur_invoice_lines_goods_receipt_line_id_fkey" FOREIGN KEY ("goods_receipt_line_id") REFERENCES "pur_goods_receipt_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_returns" ADD CONSTRAINT "pur_returns_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "pur_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_returns" ADD CONSTRAINT "pur_returns_goods_receipt_id_fkey" FOREIGN KEY ("goods_receipt_id") REFERENCES "pur_goods_receipts"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_returns" ADD CONSTRAINT "pur_returns_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "pur_invoices"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_return_lines" ADD CONSTRAINT "pur_return_lines_return_id_fkey" FOREIGN KEY ("return_id") REFERENCES "pur_returns"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_return_lines" ADD CONSTRAINT "pur_return_lines_order_line_id_fkey" FOREIGN KEY ("order_line_id") REFERENCES "pur_order_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_return_lines" ADD CONSTRAINT "pur_return_lines_goods_receipt_line_id_fkey" FOREIGN KEY ("goods_receipt_line_id") REFERENCES "pur_goods_receipt_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pur_return_lines" ADD CONSTRAINT "pur_return_lines_invoice_line_id_fkey" FOREIGN KEY ("invoice_line_id") REFERENCES "pur_invoice_lines"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_quotation_lines" ADD CONSTRAINT "sls_quotation_lines_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "sls_quotations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_quotation_materials" ADD CONSTRAINT "sls_quotation_materials_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "sls_quotations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_orders" ADD CONSTRAINT "sls_orders_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "sls_quotations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_order_lines" ADD CONSTRAINT "sls_order_lines_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "sls_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_proforma_invoices" ADD CONSTRAINT "sls_proforma_invoices_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "sls_quotations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_proforma_invoices" ADD CONSTRAINT "sls_proforma_invoices_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "sls_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_proforma_invoice_lines" ADD CONSTRAINT "sls_proforma_invoice_lines_proforma_invoice_id_fkey" FOREIGN KEY ("proforma_invoice_id") REFERENCES "sls_proforma_invoices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_packing_lists" ADD CONSTRAINT "sls_packing_lists_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "sls_quotations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_packing_lists" ADD CONSTRAINT "sls_packing_lists_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "sls_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_packing_lists" ADD CONSTRAINT "sls_packing_lists_proforma_invoice_id_fkey" FOREIGN KEY ("proforma_invoice_id") REFERENCES "sls_proforma_invoices"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_packing_list_lines" ADD CONSTRAINT "sls_packing_list_lines_packing_list_id_fkey" FOREIGN KEY ("packing_list_id") REFERENCES "sls_packing_lists"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_packing_list_packs" ADD CONSTRAINT "sls_packing_list_packs_packing_list_id_fkey" FOREIGN KEY ("packing_list_id") REFERENCES "sls_packing_lists"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_delivery_orders" ADD CONSTRAINT "sls_delivery_orders_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "sls_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_delivery_orders" ADD CONSTRAINT "sls_delivery_orders_packing_list_id_fkey" FOREIGN KEY ("packing_list_id") REFERENCES "sls_packing_lists"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_delivery_order_lines" ADD CONSTRAINT "sls_delivery_order_lines_delivery_order_id_fkey" FOREIGN KEY ("delivery_order_id") REFERENCES "sls_delivery_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_delivery_reports" ADD CONSTRAINT "sls_delivery_reports_delivery_order_id_fkey" FOREIGN KEY ("delivery_order_id") REFERENCES "sls_delivery_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_delivery_report_lines" ADD CONSTRAINT "sls_delivery_report_lines_delivery_report_id_fkey" FOREIGN KEY ("delivery_report_id") REFERENCES "sls_delivery_reports"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoices" ADD CONSTRAINT "sls_invoices_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "sls_quotations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoices" ADD CONSTRAINT "sls_invoices_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "sls_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoices" ADD CONSTRAINT "sls_invoices_advance_id_fkey" FOREIGN KEY ("advance_id") REFERENCES "sls_customer_advances"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoices" ADD CONSTRAINT "sls_invoices_proforma_invoice_id_fkey" FOREIGN KEY ("proforma_invoice_id") REFERENCES "sls_proforma_invoices"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoices" ADD CONSTRAINT "sls_invoices_delivery_order_id_fkey" FOREIGN KEY ("delivery_order_id") REFERENCES "sls_delivery_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_lines" ADD CONSTRAINT "sls_invoice_lines_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "sls_invoices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_installments" ADD CONSTRAINT "sls_invoice_installments_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "sls_invoices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_materials" ADD CONSTRAINT "sls_invoice_materials_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "sls_invoices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_materials" ADD CONSTRAINT "sls_invoice_materials_invoice_line_id_fkey" FOREIGN KEY ("invoice_line_id") REFERENCES "sls_invoice_lines"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_costs" ADD CONSTRAINT "sls_invoice_costs_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "sls_invoices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_returns" ADD CONSTRAINT "sls_returns_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "sls_invoices"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_return_lines" ADD CONSTRAINT "sls_return_lines_return_id_fkey" FOREIGN KEY ("return_id") REFERENCES "sls_returns"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_return_receipts" ADD CONSTRAINT "sls_return_receipts_invoice_id_fkey" FOREIGN KEY ("invoice_id") REFERENCES "sls_invoices"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_return_receipts" ADD CONSTRAINT "sls_return_receipts_return_id_fkey" FOREIGN KEY ("return_id") REFERENCES "sls_returns"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_return_receipt_lines" ADD CONSTRAINT "sls_return_receipt_lines_return_receipt_id_fkey" FOREIGN KEY ("return_receipt_id") REFERENCES "sls_return_receipts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_customer_advances" ADD CONSTRAINT "sls_customer_advances_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "sls_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_swap_lines" ADD CONSTRAINT "sls_invoice_swap_lines_swap_id_fkey" FOREIGN KEY ("swap_id") REFERENCES "sls_invoice_swaps"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_swap_lines" ADD CONSTRAINT "sls_invoice_swap_lines_from_invoice_id_fkey" FOREIGN KEY ("from_invoice_id") REFERENCES "sls_invoices"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_invoice_swap_lines" ADD CONSTRAINT "sls_invoice_swap_lines_to_invoice_id_fkey" FOREIGN KEY ("to_invoice_id") REFERENCES "sls_invoices"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "sls_forecast_lines" ADD CONSTRAINT "sls_forecast_lines_forecast_id_fkey" FOREIGN KEY ("forecast_id") REFERENCES "sls_forecasts"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_bom_inputs" ADD CONSTRAINT "mfg_bom_inputs_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_bom_outputs" ADD CONSTRAINT "mfg_bom_outputs_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_work_orders" ADD CONSTRAINT "mfg_work_orders_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_work_orders" ADD CONSTRAINT "mfg_work_orders_production_rework_id_fkey" FOREIGN KEY ("production_rework_id") REFERENCES "mfg_production_reworks"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_work_order_inputs" ADD CONSTRAINT "mfg_work_order_inputs_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_work_order_outputs" ADD CONSTRAINT "mfg_work_order_outputs_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_work_order_activities" ADD CONSTRAINT "mfg_work_order_activities_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_work_order_route_cards" ADD CONSTRAINT "mfg_work_order_route_cards_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_issues" ADD CONSTRAINT "mfg_material_issues_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_issues" ADD CONSTRAINT "mfg_material_issues_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_issues" ADD CONSTRAINT "mfg_material_issues_production_rework_id_fkey" FOREIGN KEY ("production_rework_id") REFERENCES "mfg_production_reworks"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_issue_inputs" ADD CONSTRAINT "mfg_material_issue_inputs_material_issue_id_fkey" FOREIGN KEY ("material_issue_id") REFERENCES "mfg_material_issues"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_issue_outputs" ADD CONSTRAINT "mfg_material_issue_outputs_material_issue_id_fkey" FOREIGN KEY ("material_issue_id") REFERENCES "mfg_material_issues"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_returns" ADD CONSTRAINT "mfg_material_returns_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_returns" ADD CONSTRAINT "mfg_material_returns_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_returns" ADD CONSTRAINT "mfg_material_returns_material_issue_id_fkey" FOREIGN KEY ("material_issue_id") REFERENCES "mfg_material_issues"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_returns" ADD CONSTRAINT "mfg_material_returns_production_rework_id_fkey" FOREIGN KEY ("production_rework_id") REFERENCES "mfg_production_reworks"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_return_inputs" ADD CONSTRAINT "mfg_material_return_inputs_material_return_id_fkey" FOREIGN KEY ("material_return_id") REFERENCES "mfg_material_returns"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_material_return_outputs" ADD CONSTRAINT "mfg_material_return_outputs_material_return_id_fkey" FOREIGN KEY ("material_return_id") REFERENCES "mfg_material_returns"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entries" ADD CONSTRAINT "mfg_production_entries_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entries" ADD CONSTRAINT "mfg_production_entries_work_order_id_fkey" FOREIGN KEY ("work_order_id") REFERENCES "mfg_work_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entries" ADD CONSTRAINT "mfg_production_entries_material_issue_id_fkey" FOREIGN KEY ("material_issue_id") REFERENCES "mfg_material_issues"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entries" ADD CONSTRAINT "mfg_production_entries_material_return_id_fkey" FOREIGN KEY ("material_return_id") REFERENCES "mfg_material_returns"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entries" ADD CONSTRAINT "mfg_production_entries_production_rework_id_fkey" FOREIGN KEY ("production_rework_id") REFERENCES "mfg_production_reworks"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entry_inputs" ADD CONSTRAINT "mfg_production_entry_inputs_production_entry_id_fkey" FOREIGN KEY ("production_entry_id") REFERENCES "mfg_production_entries"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_entry_outputs" ADD CONSTRAINT "mfg_production_entry_outputs_production_entry_id_fkey" FOREIGN KEY ("production_entry_id") REFERENCES "mfg_production_entries"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_boms" ADD CONSTRAINT "mfg_production_boms_production_entry_id_fkey" FOREIGN KEY ("production_entry_id") REFERENCES "mfg_production_entries"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_boms" ADD CONSTRAINT "mfg_production_boms_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_boms" ADD CONSTRAINT "mfg_production_boms_bom_output_line_id_fkey" FOREIGN KEY ("bom_output_line_id") REFERENCES "mfg_bom_outputs"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_reworks" ADD CONSTRAINT "mfg_production_reworks_bom_id_fkey" FOREIGN KEY ("bom_id") REFERENCES "mfg_boms"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_reworks" ADD CONSTRAINT "mfg_production_reworks_production_entry_id_fkey" FOREIGN KEY ("production_entry_id") REFERENCES "mfg_production_entries"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_rework_inputs" ADD CONSTRAINT "mfg_production_rework_inputs_production_rework_id_fkey" FOREIGN KEY ("production_rework_id") REFERENCES "mfg_production_reworks"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "mfg_production_rework_outputs" ADD CONSTRAINT "mfg_production_rework_outputs_production_rework_id_fkey" FOREIGN KEY ("production_rework_id") REFERENCES "mfg_production_reworks"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_categories" ADD CONSTRAINT "fa_asset_categories_tax_category_id_fkey" FOREIGN KEY ("tax_category_id") REFERENCES "fa_asset_category_taxes"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_assets" ADD CONSTRAINT "fa_assets_category_id_fkey" FOREIGN KEY ("category_id") REFERENCES "fa_asset_categories"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_assets" ADD CONSTRAINT "fa_assets_department_id_fkey" FOREIGN KEY ("department_id") REFERENCES "fa_asset_departments"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_assets" ADD CONSTRAINT "fa_assets_registration_id_fkey" FOREIGN KEY ("registration_id") REFERENCES "fa_asset_registrations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_movements" ADD CONSTRAINT "fa_asset_movements_asset_id_fkey" FOREIGN KEY ("asset_id") REFERENCES "fa_assets"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_requisition_lines" ADD CONSTRAINT "fa_asset_requisition_lines_requisition_id_fkey" FOREIGN KEY ("requisition_id") REFERENCES "fa_asset_requisitions"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_quotations" ADD CONSTRAINT "fa_asset_quotations_requisition_id_fkey" FOREIGN KEY ("requisition_id") REFERENCES "fa_asset_requisitions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_quotation_lines" ADD CONSTRAINT "fa_asset_quotation_lines_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "fa_asset_quotations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_orders" ADD CONSTRAINT "fa_asset_orders_requisition_id_fkey" FOREIGN KEY ("requisition_id") REFERENCES "fa_asset_requisitions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_orders" ADD CONSTRAINT "fa_asset_orders_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "fa_asset_quotations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_order_lines" ADD CONSTRAINT "fa_asset_order_lines_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "fa_asset_orders"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_acquisitions" ADD CONSTRAINT "fa_acquisitions_requisition_id_fkey" FOREIGN KEY ("requisition_id") REFERENCES "fa_asset_requisitions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_acquisitions" ADD CONSTRAINT "fa_acquisitions_quotation_id_fkey" FOREIGN KEY ("quotation_id") REFERENCES "fa_asset_quotations"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_acquisitions" ADD CONSTRAINT "fa_acquisitions_order_id_fkey" FOREIGN KEY ("order_id") REFERENCES "fa_asset_orders"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_acquisition_lines" ADD CONSTRAINT "fa_acquisition_lines_acquisition_id_fkey" FOREIGN KEY ("acquisition_id") REFERENCES "fa_acquisitions"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_registrations" ADD CONSTRAINT "fa_asset_registrations_acquisition_id_fkey" FOREIGN KEY ("acquisition_id") REFERENCES "fa_acquisitions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_asset_registration_lines" ADD CONSTRAINT "fa_asset_registration_lines_registration_id_fkey" FOREIGN KEY ("registration_id") REFERENCES "fa_asset_registrations"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_depreciation_run_lines" ADD CONSTRAINT "fa_depreciation_run_lines_depreciation_run_id_fkey" FOREIGN KEY ("depreciation_run_id") REFERENCES "fa_depreciation_runs"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_depreciation_run_lines" ADD CONSTRAINT "fa_depreciation_run_lines_asset_id_fkey" FOREIGN KEY ("asset_id") REFERENCES "fa_assets"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_transfer_lines" ADD CONSTRAINT "fa_transfer_lines_transfer_id_fkey" FOREIGN KEY ("transfer_id") REFERENCES "fa_transfers"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_transfer_lines" ADD CONSTRAINT "fa_transfer_lines_asset_id_fkey" FOREIGN KEY ("asset_id") REFERENCES "fa_assets"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_transfer_lines" ADD CONSTRAINT "fa_transfer_lines_from_department_id_fkey" FOREIGN KEY ("from_department_id") REFERENCES "fa_asset_departments"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_transfer_lines" ADD CONSTRAINT "fa_transfer_lines_to_department_id_fkey" FOREIGN KEY ("to_department_id") REFERENCES "fa_asset_departments"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_disposal_lines" ADD CONSTRAINT "fa_disposal_lines_disposal_id_fkey" FOREIGN KEY ("disposal_id") REFERENCES "fa_disposals"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "fa_disposal_lines" ADD CONSTRAINT "fa_disposal_lines_asset_id_fkey" FOREIGN KEY ("asset_id") REFERENCES "fa_assets"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_areas" ADD CONSTRAINT "pos_areas_category_id_fkey" FOREIGN KEY ("category_id") REFERENCES "pos_area_categories"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_item_prices" ADD CONSTRAINT "pos_item_prices_area_id_fkey" FOREIGN KEY ("area_id") REFERENCES "pos_areas"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_item_price_tiers" ADD CONSTRAINT "pos_item_price_tiers_item_price_id_fkey" FOREIGN KEY ("item_price_id") REFERENCES "pos_item_prices"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_price_agreement_lines" ADD CONSTRAINT "pos_price_agreement_lines_agreement_id_fkey" FOREIGN KEY ("agreement_id") REFERENCES "pos_price_agreements"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_promotions" ADD CONSTRAINT "pos_promotions_area_id_fkey" FOREIGN KEY ("area_id") REFERENCES "pos_areas"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_bonus_rules" ADD CONSTRAINT "pos_bonus_rules_promotion_id_fkey" FOREIGN KEY ("promotion_id") REFERENCES "pos_promotions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_bonus_transactions" ADD CONSTRAINT "pos_bonus_transactions_bonus_rule_id_fkey" FOREIGN KEY ("bonus_rule_id") REFERENCES "pos_bonus_rules"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_substitution_rules" ADD CONSTRAINT "pos_substitution_rules_promotion_id_fkey" FOREIGN KEY ("promotion_id") REFERENCES "pos_promotions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_additional_item_rules" ADD CONSTRAINT "pos_additional_item_rules_promotion_id_fkey" FOREIGN KEY ("promotion_id") REFERENCES "pos_promotions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_discount_rules" ADD CONSTRAINT "pos_discount_rules_promotion_id_fkey" FOREIGN KEY ("promotion_id") REFERENCES "pos_promotions"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pos_point_transactions" ADD CONSTRAINT "pos_point_transactions_rule_id_fkey" FOREIGN KEY ("rule_id") REFERENCES "pos_point_rules"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pln_mrp_run_lines" ADD CONSTRAINT "pln_mrp_run_lines_mrp_run_id_fkey" FOREIGN KEY ("mrp_run_id") REFERENCES "pln_mrp_runs"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- AddForeignKey
ALTER TABLE "pln_replenishment_suggestions" ADD CONSTRAINT "pln_replenishment_suggestions_mrp_run_id_fkey" FOREIGN KEY ("mrp_run_id") REFERENCES "pln_mrp_runs"("id") ON DELETE SET NULL ON UPDATE CASCADE;

