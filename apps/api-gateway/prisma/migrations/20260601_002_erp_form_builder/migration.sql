-- Form Builder: sys_form_fields table + custom_fields on fin_cash_bank_transactions

CREATE TABLE "sys_form_fields" (
  "id"                    BIGSERIAL PRIMARY KEY,
  "transaction_type_code" TEXT NOT NULL,
  "field_key"             TEXT NOT NULL,
  "kind"                  TEXT NOT NULL DEFAULT 'STRUCTURAL',
  "label"                 TEXT NOT NULL,
  "field_type"            TEXT NOT NULL DEFAULT 'TEXT',
  "lookup_source"         TEXT,
  "is_required"           BOOLEAN NOT NULL DEFAULT FALSE,
  "is_visible"            BOOLEAN NOT NULL DEFAULT TRUE,
  "sort_order"            INTEGER NOT NULL DEFAULT 0,
  "column_slot"           TEXT NOT NULL DEFAULT 'LEFT',
  "created_at"            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "updated_at"            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  "created_by_id"         BIGINT,
  "updated_by_id"         BIGINT,
  "deleted_at"            TIMESTAMPTZ
);

CREATE UNIQUE INDEX "sys_form_fields_transaction_type_code_field_key_key"
  ON "sys_form_fields"("transaction_type_code", "field_key");

CREATE INDEX "sys_form_fields_transaction_type_code_idx"
  ON "sys_form_fields"("transaction_type_code");

-- Add custom_fields JSONB column to header transaction table
ALTER TABLE "fin_cash_bank_transactions"
  ADD COLUMN IF NOT EXISTS "custom_fields" JSONB;
