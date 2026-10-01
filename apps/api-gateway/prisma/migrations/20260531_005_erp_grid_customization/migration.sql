-- Kustomisasi Grid — system-wide transaction grid column definitions (§Kustomisasi Grid).
-- Additive only: 2 new sys_* tables + 1 nullable JSONB column on fin_cash_bank_lines.
-- No DROP, no data change. clinic/m0_*/m1_* untouched.

-- Catalog of transaction types (drives the left module→transaction tree).
CREATE TABLE IF NOT EXISTS "sys_transaction_types" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "module_key" TEXT NOT NULL,
    "module_label" TEXT NOT NULL,
    "group_label" TEXT,
    "line_table" TEXT,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_transaction_types_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "sys_transaction_types_code_key" ON "sys_transaction_types"("code");
CREATE INDEX IF NOT EXISTS "sys_transaction_types_module_key_idx" ON "sys_transaction_types"("module_key");

-- Per-transaction grid column definitions.
CREATE TABLE IF NOT EXISTS "sys_transaction_grid_columns" (
    "id" BIGSERIAL NOT NULL,
    "transaction_type_id" BIGINT NOT NULL,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "header_text" TEXT NOT NULL,
    "data_field" TEXT NOT NULL,
    "width" INTEGER NOT NULL DEFAULT 120,
    "is_visible" BOOLEAN NOT NULL DEFAULT true,
    "is_required" BOOLEAN NOT NULL DEFAULT false,
    "is_editable" BOOLEAN NOT NULL DEFAULT true,
    "kind" TEXT NOT NULL DEFAULT 'STANDARD',
    "data_type" TEXT NOT NULL DEFAULT 'TEXT',
    "lookup_source" TEXT,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_transaction_grid_columns_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "sys_transaction_grid_columns_type_field_key" ON "sys_transaction_grid_columns"("transaction_type_id", "data_field");
CREATE INDEX IF NOT EXISTS "sys_transaction_grid_columns_transaction_type_id_idx" ON "sys_transaction_grid_columns"("transaction_type_id");

ALTER TABLE "sys_transaction_grid_columns"
    ADD CONSTRAINT "sys_transaction_grid_columns_transaction_type_id_fkey"
    FOREIGN KEY ("transaction_type_id") REFERENCES "sys_transaction_types"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- Custom-column values store on cash/bank lines (keyed by data_field).
ALTER TABLE "fin_cash_bank_lines" ADD COLUMN IF NOT EXISTS "custom_fields" JSONB;
