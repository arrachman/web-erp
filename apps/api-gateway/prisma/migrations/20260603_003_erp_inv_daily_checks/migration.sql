-- Migration: 20260603_003_erp_inv_daily_checks
-- Daily Check / Time Sheet (DC) header and lines tables.

CREATE TABLE IF NOT EXISTS "inv_daily_checks" (
    "id"               BIGSERIAL       NOT NULL,
    "doc_number"       TEXT            NOT NULL,
    "auto_number"      TEXT,
    "legacy_code"      TEXT,
    "branch_id"        BIGINT          NOT NULL,
    "location_id"      BIGINT,
    "check_date"       DATE            NOT NULL,
    "fiscal_period_id" BIGINT          NOT NULL,
    "machine_ref"      TEXT,
    "operator_ref"     TEXT,
    "description"      TEXT,
    "notes"            TEXT,
    "status"           TEXT            NOT NULL,
    "posting_status"   TEXT            NOT NULL,
    "posted_at"        TIMESTAMPTZ(6),
    "metadata"         JSONB,
    "deleted_at"       TIMESTAMPTZ(6),
    "created_at"       TIMESTAMPTZ(6)  NOT NULL DEFAULT NOW(),
    "updated_at"       TIMESTAMPTZ(6)  NOT NULL,
    "created_by_id"    BIGINT,
    "updated_by_id"    BIGINT,

    CONSTRAINT "inv_daily_checks_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "inv_daily_checks_doc_number_key"
    ON "inv_daily_checks" ("doc_number");

CREATE INDEX IF NOT EXISTS "inv_daily_checks_fiscal_period_id_idx"
    ON "inv_daily_checks" ("fiscal_period_id");

CREATE INDEX IF NOT EXISTS "inv_daily_checks_legacy_code_idx"
    ON "inv_daily_checks" ("legacy_code");

-- ─────────────────────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS "inv_daily_check_lines" (
    "id"             BIGSERIAL        NOT NULL,
    "daily_check_id" BIGINT           NOT NULL,
    "item_id"        BIGINT           NOT NULL,
    "quantity"       DECIMAL(19, 4)   NOT NULL,
    "unit_id"        BIGINT           NOT NULL,
    "warehouse_id"   BIGINT,
    "cost_center_id" BIGINT,
    "notes"          TEXT,
    "line_no"        INTEGER          NOT NULL,

    CONSTRAINT "inv_daily_check_lines_pkey" PRIMARY KEY ("id"),
    CONSTRAINT "inv_daily_check_lines_daily_check_id_fkey"
        FOREIGN KEY ("daily_check_id")
        REFERENCES "inv_daily_checks" ("id")
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "inv_daily_check_lines_daily_check_id_idx"
    ON "inv_daily_check_lines" ("daily_check_id");

CREATE INDEX IF NOT EXISTS "inv_daily_check_lines_item_id_idx"
    ON "inv_daily_check_lines" ("item_id");
