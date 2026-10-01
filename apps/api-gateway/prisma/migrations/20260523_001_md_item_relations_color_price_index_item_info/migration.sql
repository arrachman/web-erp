-- Migration: md_item_relations_color_price_index_item_info
-- Add FK attribute columns to md_items, fix relations on md_item_locations & md_item_permissions,
-- create md_colors, md_price_indices, md_item_informations

-- 1. Add attribute FK columns to md_items
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "kind_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "brand_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "material_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "item_model_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "size_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "color_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "product_class_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "section_id" BIGINT;

-- 2. Create md_colors table
CREATE TABLE IF NOT EXISTS "md_colors" (
  "id"           BIGSERIAL PRIMARY KEY,
  "code"         TEXT NOT NULL,
  "name"         TEXT NOT NULL,
  "hex_code"     TEXT,
  "is_active"    BOOLEAN NOT NULL DEFAULT true,
  "legacy_code"  TEXT,
  "metadata"     JSONB,
  "created_at"   TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"   TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at"   TIMESTAMPTZ(6),
  CONSTRAINT "md_colors_code_key" UNIQUE ("code")
);
CREATE INDEX IF NOT EXISTS "md_colors_legacy_code_idx" ON "md_colors"("legacy_code");

-- 3. Create md_price_indices table
CREATE TABLE IF NOT EXISTS "md_price_indices" (
  "id"           BIGSERIAL PRIMARY KEY,
  "code"         TEXT NOT NULL,
  "name"         TEXT NOT NULL,
  "margin"       DECIMAL(9,4) NOT NULL DEFAULT 0,
  "notes"        TEXT,
  "is_active"    BOOLEAN NOT NULL DEFAULT true,
  "legacy_code"  TEXT,
  "metadata"     JSONB,
  "created_at"   TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"   TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  "deleted_at"   TIMESTAMPTZ(6),
  CONSTRAINT "md_price_indices_code_key" UNIQUE ("code")
);
CREATE INDEX IF NOT EXISTS "md_price_indices_legacy_code_idx" ON "md_price_indices"("legacy_code");

-- 4. Create md_item_informations table (1:1 with md_items)
CREATE TABLE IF NOT EXISTS "md_item_informations" (
  "id"                    BIGSERIAL PRIMARY KEY,
  "item_id"               BIGINT NOT NULL,
  "long_description"      TEXT,
  "specifications"        TEXT,
  "manufacturer"          TEXT,
  "country_of_origin"     TEXT,
  "warranty_period_months" INTEGER,
  "tags"                  TEXT,
  "notes"                 TEXT,
  "legacy_code"           TEXT,
  "metadata"              JSONB,
  "created_at"            TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"            TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id"         BIGINT,
  "updated_by_id"         BIGINT,
  "deleted_at"            TIMESTAMPTZ(6),
  CONSTRAINT "md_item_informations_item_id_key" UNIQUE ("item_id")
);
CREATE INDEX IF NOT EXISTS "md_item_informations_item_id_idx" ON "md_item_informations"("item_id");
CREATE INDEX IF NOT EXISTS "md_item_informations_legacy_code_idx" ON "md_item_informations"("legacy_code");

-- 5. FK constraints on md_items -> attribute masters
ALTER TABLE "md_items"
  ADD CONSTRAINT "md_items_kind_id_fkey"
    FOREIGN KEY ("kind_id") REFERENCES "md_item_types"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_brand_id_fkey"
    FOREIGN KEY ("brand_id") REFERENCES "md_brands"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_material_id_fkey"
    FOREIGN KEY ("material_id") REFERENCES "md_materials"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_item_model_id_fkey"
    FOREIGN KEY ("item_model_id") REFERENCES "md_item_models"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_size_id_fkey"
    FOREIGN KEY ("size_id") REFERENCES "md_sizes"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_color_id_fkey"
    FOREIGN KEY ("color_id") REFERENCES "md_colors"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_product_class_id_fkey"
    FOREIGN KEY ("product_class_id") REFERENCES "md_product_classes"("id") ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_items_section_id_fkey"
    FOREIGN KEY ("section_id") REFERENCES "md_sections"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- 6. Indexes on md_items new FK columns
CREATE INDEX IF NOT EXISTS "md_items_kind_id_idx" ON "md_items"("kind_id");
CREATE INDEX IF NOT EXISTS "md_items_brand_id_idx" ON "md_items"("brand_id");
CREATE INDEX IF NOT EXISTS "md_items_material_id_idx" ON "md_items"("material_id");
CREATE INDEX IF NOT EXISTS "md_items_item_model_id_idx" ON "md_items"("item_model_id");
CREATE INDEX IF NOT EXISTS "md_items_size_id_idx" ON "md_items"("size_id");
CREATE INDEX IF NOT EXISTS "md_items_color_id_idx" ON "md_items"("color_id");
CREATE INDEX IF NOT EXISTS "md_items_product_class_id_idx" ON "md_items"("product_class_id");
CREATE INDEX IF NOT EXISTS "md_items_section_id_idx" ON "md_items"("section_id");

-- 7. Fix md_item_locations -> md_warehouses FK (column already exists as scalar)
DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.table_constraints
    WHERE constraint_name = 'md_item_locations_warehouse_id_fkey'
      AND table_name = 'md_item_locations'
  ) THEN
    ALTER TABLE "md_item_locations"
      ADD CONSTRAINT "md_item_locations_warehouse_id_fkey"
        FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END $$;

-- 8. Fix md_item_permissions -> md_items and adm_roles FK
DO $$ BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.table_constraints
    WHERE constraint_name = 'md_item_permissions_item_id_fkey'
      AND table_name = 'md_item_permissions'
  ) THEN
    ALTER TABLE "md_item_permissions"
      ADD CONSTRAINT "md_item_permissions_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.table_constraints
    WHERE constraint_name = 'md_item_permissions_role_id_fkey'
      AND table_name = 'md_item_permissions'
  ) THEN
    ALTER TABLE "md_item_permissions"
      ADD CONSTRAINT "md_item_permissions_role_id_fkey"
        FOREIGN KEY ("role_id") REFERENCES "adm_roles"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;
END $$;

-- 9. FK on md_item_informations -> md_items
ALTER TABLE "md_item_informations"
  ADD CONSTRAINT "md_item_informations_item_id_fkey"
    FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
