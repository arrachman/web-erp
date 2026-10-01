-- ERP md_items — legacy MyERP+ "Atribut" tab parity.
-- Adds physical dimensions, handling/regulatory flags, the field unit (Satuan
-- Lapangan), and the attribute lookups (Desainer, Nozzle, Oem, Vendor) plus the
-- two genuinely-new lookup masters md_nozzles / md_oems. Also enforces FK
-- constraints for the pre-existing attribute columns (brand/material/size/color/
-- section) that had columns but no relation/constraint until now.
-- Additive only, idempotent, 0 DROP (repo norm). Vendor reuses md_partners;
-- Warna/Merk/Ukuran/Material/Section reuse existing masters.

-- 1. New lookup masters (mirror md_colors shape, minus hex_code)
CREATE TABLE IF NOT EXISTS "md_nozzles" (
  "id"            BIGSERIAL PRIMARY KEY,
  "code"          TEXT NOT NULL,
  "name"          TEXT NOT NULL,
  "is_active"     BOOLEAN NOT NULL DEFAULT TRUE,
  "legacy_code"   TEXT,
  "metadata"      JSONB,
  "created_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at"    TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX IF NOT EXISTS "md_nozzles_code_key"        ON "md_nozzles"("code");
CREATE INDEX        IF NOT EXISTS "md_nozzles_legacy_code_idx" ON "md_nozzles"("legacy_code");

CREATE TABLE IF NOT EXISTS "md_oems" (
  "id"            BIGSERIAL PRIMARY KEY,
  "code"          TEXT NOT NULL,
  "name"          TEXT NOT NULL,
  "is_active"     BOOLEAN NOT NULL DEFAULT TRUE,
  "legacy_code"   TEXT,
  "metadata"      JSONB,
  "created_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at"    TIMESTAMPTZ(6)
);
CREATE UNIQUE INDEX IF NOT EXISTS "md_oems_code_key"        ON "md_oems"("code");
CREATE INDEX        IF NOT EXISTS "md_oems_legacy_code_idx" ON "md_oems"("legacy_code");

-- 2. New md_items columns (all nullable except defaulted flags)
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "designer_id"       BIGINT,
  ADD COLUMN IF NOT EXISTS "nozzle_id"         BIGINT,
  ADD COLUMN IF NOT EXISTS "oem_id"            BIGINT,
  ADD COLUMN IF NOT EXISTS "vendor_id"         BIGINT,
  ADD COLUMN IF NOT EXISTS "field_unit_id"     BIGINT,
  ADD COLUMN IF NOT EXISTS "length"            DECIMAL(19, 4),
  ADD COLUMN IF NOT EXISTS "width"             DECIMAL(19, 4),
  ADD COLUMN IF NOT EXISTS "height"            DECIMAL(19, 4),
  ADD COLUMN IF NOT EXISTS "volume"            DECIMAL(19, 4),
  ADD COLUMN IF NOT EXISTS "conversion_kg_pcs" DECIMAL(19, 4) NOT NULL DEFAULT 1,
  ADD COLUMN IF NOT EXISTS "registration_no"  TEXT,
  ADD COLUMN IF NOT EXISTS "is_returnable"    BOOLEAN NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "is_mobile"        BOOLEAN NOT NULL DEFAULT FALSE;

-- 3. Foreign keys (intra-domain md_*). Guarded — re-runnable on partial state.
--    Includes the pre-existing attribute columns that lacked a constraint.
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_brand_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_brand_id_fkey"
      FOREIGN KEY ("brand_id") REFERENCES "md_brands"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_material_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_material_id_fkey"
      FOREIGN KEY ("material_id") REFERENCES "md_materials"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_size_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_size_id_fkey"
      FOREIGN KEY ("size_id") REFERENCES "md_sizes"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_color_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_color_id_fkey"
      FOREIGN KEY ("color_id") REFERENCES "md_colors"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_section_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_section_id_fkey"
      FOREIGN KEY ("section_id") REFERENCES "md_sections"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_designer_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_designer_id_fkey"
      FOREIGN KEY ("designer_id") REFERENCES "md_designers"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_nozzle_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_nozzle_id_fkey"
      FOREIGN KEY ("nozzle_id") REFERENCES "md_nozzles"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_oem_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_oem_id_fkey"
      FOREIGN KEY ("oem_id") REFERENCES "md_oems"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_vendor_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_vendor_id_fkey"
      FOREIGN KEY ("vendor_id") REFERENCES "md_partners"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_field_unit_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_field_unit_id_fkey"
      FOREIGN KEY ("field_unit_id") REFERENCES "md_units"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END $$;

-- 4. Indexes for new FK lookups (existing attribute-column indexes guarded too)
CREATE INDEX IF NOT EXISTS "md_items_brand_id_idx"      ON "md_items"("brand_id");
CREATE INDEX IF NOT EXISTS "md_items_material_id_idx"   ON "md_items"("material_id");
CREATE INDEX IF NOT EXISTS "md_items_size_id_idx"       ON "md_items"("size_id");
CREATE INDEX IF NOT EXISTS "md_items_color_id_idx"      ON "md_items"("color_id");
CREATE INDEX IF NOT EXISTS "md_items_section_id_idx"    ON "md_items"("section_id");
CREATE INDEX IF NOT EXISTS "md_items_designer_id_idx"   ON "md_items"("designer_id");
CREATE INDEX IF NOT EXISTS "md_items_nozzle_id_idx"     ON "md_items"("nozzle_id");
CREATE INDEX IF NOT EXISTS "md_items_oem_id_idx"        ON "md_items"("oem_id");
CREATE INDEX IF NOT EXISTS "md_items_vendor_id_idx"     ON "md_items"("vendor_id");
CREATE INDEX IF NOT EXISTS "md_items_field_unit_id_idx" ON "md_items"("field_unit_id");
