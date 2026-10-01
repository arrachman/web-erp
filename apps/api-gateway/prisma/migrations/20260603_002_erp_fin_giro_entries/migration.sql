-- Giro register/clearing header (fin_giro_entries) + link existing fin_giros
-- instruments to their REGISTER entry (giro_entry_id) and CLEAR entry
-- (cleared_by_entry_id). Additive — existing fin_giros rows keep both columns
-- NULL. See web-erp DECISIONS.md "§ Giro (RG/SG/RGC/SGC) — header + instrumen".

-- 1. Discriminator enum (REGISTER vs CLEAR).
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'ErpGiroEntryKind') THEN
    CREATE TYPE "ErpGiroEntryKind" AS ENUM ('REGISTER', 'CLEAR');
  END IF;
END
$$;

-- 2. Header table.
CREATE TABLE IF NOT EXISTS "fin_giro_entries" (
    "id" BIGSERIAL NOT NULL,
    "doc_number" TEXT NOT NULL,
    "auto_number" TEXT,
    "kind" "ErpGiroEntryKind" NOT NULL,
    "type" "ErpGiroType" NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "partner_id" BIGINT,
    "entry_date" DATE NOT NULL,
    "fiscal_period_id" BIGINT NOT NULL,
    "bank_account_id" BIGINT,
    "currency_id" BIGINT NOT NULL,
    "exchange_rate" DECIMAL(19,6) NOT NULL,
    "description" TEXT,
    "notes" TEXT,
    "status" "ErpDocumentStatus" NOT NULL,
    "previous_status" "ErpDocumentStatus",
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

    CONSTRAINT "fin_giro_entries_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "fin_giro_entries_doc_number_key" ON "fin_giro_entries"("doc_number");
CREATE INDEX IF NOT EXISTS "fin_giro_entries_kind_type_status_idx" ON "fin_giro_entries"("kind", "type", "status");
CREATE INDEX IF NOT EXISTS "fin_giro_entries_fiscal_period_id_idx" ON "fin_giro_entries"("fiscal_period_id");
CREATE INDEX IF NOT EXISTS "fin_giro_entries_entry_date_idx" ON "fin_giro_entries"("entry_date");
CREATE INDEX IF NOT EXISTS "fin_giro_entries_partner_id_idx" ON "fin_giro_entries"("partner_id");

-- 3. Link instruments to register + clearing entries.
ALTER TABLE "fin_giros" ADD COLUMN IF NOT EXISTS "giro_entry_id" BIGINT;
ALTER TABLE "fin_giros" ADD COLUMN IF NOT EXISTS "cleared_by_entry_id" BIGINT;

CREATE INDEX IF NOT EXISTS "fin_giros_giro_entry_id_idx" ON "fin_giros"("giro_entry_id");
CREATE INDEX IF NOT EXISTS "fin_giros_cleared_by_entry_id_idx" ON "fin_giros"("cleared_by_entry_id");

-- 4. FKs. Registering entry cascades (delete entry → delete its instruments);
--    clearing entry sets null (un-clearing keeps the instrument).
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fin_giros_giro_entry_id_fkey') THEN
    ALTER TABLE "fin_giros"
      ADD CONSTRAINT "fin_giros_giro_entry_id_fkey"
      FOREIGN KEY ("giro_entry_id") REFERENCES "fin_giro_entries"("id")
      ON DELETE CASCADE ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fin_giros_cleared_by_entry_id_fkey') THEN
    ALTER TABLE "fin_giros"
      ADD CONSTRAINT "fin_giros_cleared_by_entry_id_fkey"
      FOREIGN KEY ("cleared_by_entry_id") REFERENCES "fin_giro_entries"("id")
      ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END
$$;
