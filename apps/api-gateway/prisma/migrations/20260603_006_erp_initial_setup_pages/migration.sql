-- Initial Setup rich pages: company bank accounts, approval rules, home widgets, import jobs
-- Additive only (0 DROP). Domain: sys_* (system config under M0 Administrator).

-- ── sys_bank_accounts (company bank accounts, legacy 0-31) ──────────────────
CREATE TABLE "sys_bank_accounts" (
  "id"             BIGSERIAL    PRIMARY KEY,
  "code"           TEXT         NOT NULL,
  "name"           TEXT         NOT NULL,
  "bank_name"      TEXT         NOT NULL,
  "account_number" TEXT         NOT NULL,
  "account_holder" TEXT         NOT NULL,
  "branch"         TEXT,
  "currency_id"    BIGINT,
  "gl_account_id"  BIGINT,
  "swift_code"     TEXT,
  "is_primary"     BOOLEAN      NOT NULL DEFAULT false,
  "notes"          TEXT,
  "is_active"      BOOLEAN      NOT NULL DEFAULT true,
  "legacy_code"    TEXT,
  "created_at"     TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at"     TIMESTAMPTZ(6) NOT NULL,
  "created_by_id"  BIGINT,
  "updated_by_id"  BIGINT,
  "deleted_at"     TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX "sys_bank_accounts_code_key" ON "sys_bank_accounts"("code");
CREATE INDEX "sys_bank_accounts_currency_id_idx" ON "sys_bank_accounts"("currency_id");
CREATE INDEX "sys_bank_accounts_gl_account_id_idx" ON "sys_bank_accounts"("gl_account_id");

-- ── sys_approval_rules (approval rule per document type, legacy 0-46) ────────
CREATE TABLE "sys_approval_rules" (
  "id"                BIGSERIAL    PRIMARY KEY,
  "document_type"     TEXT         NOT NULL,
  "name"              TEXT         NOT NULL,
  "level"             INTEGER      NOT NULL DEFAULT 1,
  "requires_approval" BOOLEAN      NOT NULL DEFAULT true,
  "min_amount"        DECIMAL(19,4),
  "approver_role_id"  BIGINT,
  "notes"             TEXT,
  "is_active"         BOOLEAN      NOT NULL DEFAULT true,
  "legacy_code"       TEXT,
  "created_at"        TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at"        TIMESTAMPTZ(6) NOT NULL,
  "created_by_id"     BIGINT,
  "updated_by_id"     BIGINT,
  "deleted_at"        TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX "sys_approval_rules_document_type_level_key" ON "sys_approval_rules"("document_type","level");
CREATE INDEX "sys_approval_rules_approver_role_id_idx" ON "sys_approval_rules"("approver_role_id");

-- ── sys_home_widgets (home/dashboard widget layout, legacy 0-39) ────────────
CREATE TABLE "sys_home_widgets" (
  "id"            BIGSERIAL    PRIMARY KEY,
  "widget_key"    TEXT         NOT NULL,
  "title"         TEXT         NOT NULL,
  "description"   TEXT,
  "enabled"       BOOLEAN      NOT NULL DEFAULT true,
  "sort_order"    INTEGER      NOT NULL DEFAULT 0,
  "col_span"      INTEGER      NOT NULL DEFAULT 1,
  "config"        JSONB,
  "created_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at"    TIMESTAMPTZ(6) NOT NULL,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at"    TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX "sys_home_widgets_widget_key_key" ON "sys_home_widgets"("widget_key");

-- ── sys_import_jobs (data import history, legacy 0-20) ───────────────────────
CREATE TABLE "sys_import_jobs" (
  "id"            BIGSERIAL    PRIMARY KEY,
  "entity"        TEXT         NOT NULL,
  "file_name"     TEXT         NOT NULL,
  "status"        TEXT         NOT NULL DEFAULT 'PENDING',
  "rows_total"    INTEGER      NOT NULL DEFAULT 0,
  "rows_ok"       INTEGER      NOT NULL DEFAULT 0,
  "rows_failed"   INTEGER      NOT NULL DEFAULT 0,
  "errors"        JSONB,
  "created_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT now(),
  "updated_at"    TIMESTAMPTZ(6) NOT NULL,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at"    TIMESTAMPTZ(6)
);
CREATE INDEX "sys_import_jobs_entity_idx" ON "sys_import_jobs"("entity");
