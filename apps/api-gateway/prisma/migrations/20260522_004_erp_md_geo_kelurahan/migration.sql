-- Migration: 20260522_004_erp_md_geo_kelurahan
-- Additive: postalCode on md_areas + new md_sub_areas (kelurahan)
-- 0 DROP, 0 breaking changes

-- 1. Add postal_code to md_areas (nullable, already idempotent via IF NOT EXISTS)
ALTER TABLE "md_areas"
  ADD COLUMN IF NOT EXISTS "postal_code" TEXT;

-- 2. Create md_sub_areas (kelurahan)
CREATE TABLE IF NOT EXISTS "md_sub_areas" (
  "id"              BIGSERIAL       PRIMARY KEY,
  "code"            TEXT            NOT NULL,
  "name"            TEXT            NOT NULL,
  "area_id"         BIGINT          NOT NULL,
  "postal_code"     TEXT,
  "is_active"       BOOLEAN         NOT NULL DEFAULT TRUE,
  "legacy_code"     TEXT,
  "created_at"      TIMESTAMPTZ(6)  NOT NULL DEFAULT NOW(),
  "updated_at"      TIMESTAMPTZ(6)  NOT NULL DEFAULT NOW(),
  "created_by_id"   BIGINT,
  "updated_by_id"   BIGINT,
  "deleted_at"      TIMESTAMPTZ(6),

  CONSTRAINT "md_sub_areas_code_key" UNIQUE ("code"),
  CONSTRAINT "md_sub_areas_area_id_fkey" FOREIGN KEY ("area_id") REFERENCES "md_areas"("id")
);

CREATE INDEX IF NOT EXISTS "md_sub_areas_area_id_idx"   ON "md_sub_areas"("area_id");
CREATE INDEX IF NOT EXISTS "md_sub_areas_postal_code_idx" ON "md_sub_areas"("postal_code");
CREATE INDEX IF NOT EXISTS "md_sub_areas_legacy_code_idx" ON "md_sub_areas"("legacy_code");
