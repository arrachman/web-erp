-- Partner type as its own master data (md_partner_types).
-- Tipe partner sebelumnya diturunkan dari 3 boolean is_customer/is_supplier/is_salesman
-- di md_partners; kini diganti 1 FK partner_type_id -> md_partner_types (1 partner = 1 tipe).

-- 1. Enum peran tipe partner
CREATE TYPE "ErpPartnerTypeKind" AS ENUM ('CUSTOMER', 'SUPPLIER', 'SALESMAN', 'GENERAL');

-- 2. Tabel master tipe partner
CREATE TABLE IF NOT EXISTS "md_partner_types" (
  "id" BIGSERIAL PRIMARY KEY,
  "code" TEXT NOT NULL,
  "name" TEXT NOT NULL,
  "kind" "ErpPartnerTypeKind" NOT NULL,
  "is_active" BOOLEAN NOT NULL DEFAULT true,
  "legacy_code" TEXT,
  "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at" TIMESTAMPTZ(6)
);

CREATE UNIQUE INDEX IF NOT EXISTS "md_partner_types_code_key" ON "md_partner_types"("code");
CREATE INDEX IF NOT EXISTS "md_partner_types_legacy_code_idx" ON "md_partner_types"("legacy_code");

-- 3. Seed canonical role types before mapping existing partners
INSERT INTO "md_partner_types" ("code", "name", "kind", "is_active", "created_at", "updated_at") VALUES
  ('CUST', 'Customer', 'CUSTOMER', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
  ('SUP', 'Supplier', 'SUPPLIER', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
  ('SLS', 'Salesman', 'SALESMAN', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
  ('GEN', 'General', 'GENERAL', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT ("code") DO NOTHING;

-- 4. md_partners: tambah FK tunggal ke tipe, backfill dari boolean lama, baru drop boolean
ALTER TABLE "md_partners" ADD COLUMN IF NOT EXISTS "partner_type_id" BIGINT;

UPDATE "md_partners" p
SET "partner_type_id" = CASE
  WHEN p."is_salesman" = true THEN (SELECT id FROM "md_partner_types" WHERE code = 'SLS')
  WHEN p."is_supplier" = true THEN (SELECT id FROM "md_partner_types" WHERE code = 'SUP')
  WHEN p."is_customer" = true THEN (SELECT id FROM "md_partner_types" WHERE code = 'CUST')
  ELSE (SELECT id FROM "md_partner_types" WHERE code = 'GEN')
END
WHERE p."partner_type_id" IS NULL;

ALTER TABLE "md_partners" ALTER COLUMN "partner_type_id" SET NOT NULL;

ALTER TABLE "md_partners" DROP COLUMN IF EXISTS "is_customer";
ALTER TABLE "md_partners" DROP COLUMN IF EXISTS "is_supplier";
ALTER TABLE "md_partners" DROP COLUMN IF EXISTS "is_salesman";

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_partners_partner_type_id_fkey') THEN
    ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_partner_type_id_fkey"
      FOREIGN KEY ("partner_type_id") REFERENCES "md_partner_types"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;
END $$;

CREATE INDEX IF NOT EXISTS "md_partners_partner_type_id_idx" ON "md_partners"("partner_type_id");
