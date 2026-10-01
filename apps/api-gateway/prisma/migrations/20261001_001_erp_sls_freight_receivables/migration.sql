-- Freight Receivable (RP): billing the customer for freight/shipping cost,
-- kept separate from HPP/inventory value (confirmed with user). Standalone
-- Dr AR / Cr Freight Income document — no instrument/allocation, no stock.
-- Additive only.

CREATE TABLE "sls_freight_receivables" (
    "id"                    BIGSERIAL PRIMARY KEY,
    "doc_number"            TEXT NOT NULL,
    "auto_number"           TEXT,
    "branch_id"             BIGINT NOT NULL,
    "location_id"           BIGINT,
    "transaction_date"      DATE NOT NULL,
    "fiscal_period_id"      BIGINT NOT NULL,
    "customer_id"           BIGINT NOT NULL,
    "description"           TEXT NOT NULL,
    "notes"                 TEXT,
    "currency_id"           BIGINT NOT NULL,
    "exchange_rate"         DECIMAL(19,6) NOT NULL,
    "amount"                DECIMAL(19,4) NOT NULL,
    "receivable_account_id" BIGINT,
    "income_account_id"     BIGINT,
    "settlement_status"     "ErpSettlementStatus" NOT NULL,
    "settled_date"          DATE,
    "status"                "ErpDocumentStatus" NOT NULL,
    "previous_status"       "ErpDocumentStatus",
    "posting_status"        "ErpPostingStatus" NOT NULL,
    "posted_at"             TIMESTAMPTZ,
    "legacy_code"           TEXT,
    "metadata"              JSONB,
    "deleted_at"            TIMESTAMPTZ,
    "created_at"            TIMESTAMPTZ NOT NULL DEFAULT now(),
    "updated_at"            TIMESTAMPTZ NOT NULL DEFAULT now(),
    "created_by_id"         BIGINT,
    "updated_by_id"         BIGINT,

    CONSTRAINT "sls_freight_receivables_doc_number_key" UNIQUE ("doc_number")
);

CREATE INDEX "sls_freight_receivables_branch_id_idx" ON "sls_freight_receivables" ("branch_id");
CREATE INDEX "sls_freight_receivables_customer_id_idx" ON "sls_freight_receivables" ("customer_id");
CREATE INDEX "sls_freight_receivables_fiscal_period_id_idx" ON "sls_freight_receivables" ("fiscal_period_id");
