-- ERP md_items — expand for GL dimensions, costing method, validity & VAT flags.
-- Additive only. Uses IF NOT EXISTS guards because some columns may already
-- exist from prior partial migrations (e.g. classification columns added in
-- 20260523_001_md_item_relations_color_price_index_item_info).

-- 1. Costing method (enum already exists: ErpCostingMethod = AVG/FIFO/STD)
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "cost_method" "ErpCostingMethod" NOT NULL DEFAULT 'AVG';

-- 2. GL / organizational dimensions (all nullable)
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "division_id"            BIGINT,
  ADD COLUMN IF NOT EXISTS "subdivision_id"         BIGINT,
  ADD COLUMN IF NOT EXISTS "department_id"          BIGINT,
  ADD COLUMN IF NOT EXISTS "sub_department_id"      BIGINT,
  ADD COLUMN IF NOT EXISTS "branch_id"              BIGINT,
  ADD COLUMN IF NOT EXISTS "default_location_id"    BIGINT,
  ADD COLUMN IF NOT EXISTS "default_warehouse_id"   BIGINT,
  ADD COLUMN IF NOT EXISTS "project_id"             BIGINT,
  ADD COLUMN IF NOT EXISTS "cost_center_id"         BIGINT;

-- 3. Validity & flags — legacy parity (BKP, Spesial, Berlaku s.d, Kategori Umur, Min Order)
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "min_order_qty"  DECIMAL(19, 4) NOT NULL DEFAULT 0,
  ADD COLUMN IF NOT EXISTS "age_category"   TEXT,
  ADD COLUMN IF NOT EXISTS "valid_until"    DATE,
  ADD COLUMN IF NOT EXISTS "is_vatable"     BOOLEAN NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "is_special"     BOOLEAN NOT NULL DEFAULT FALSE;

-- 4. Foreign keys (intra-domain md_*). Guarded — re-runnable on partial state.
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_division_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_division_id_fkey"
      FOREIGN KEY ("division_id") REFERENCES "md_divisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_subdivision_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_subdivision_id_fkey"
      FOREIGN KEY ("subdivision_id") REFERENCES "md_subdivisions"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_department_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_department_id_fkey"
      FOREIGN KEY ("department_id") REFERENCES "md_departments"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_sub_department_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_sub_department_id_fkey"
      FOREIGN KEY ("sub_department_id") REFERENCES "md_sub_departments"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_branch_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_branch_id_fkey"
      FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_default_location_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_default_location_id_fkey"
      FOREIGN KEY ("default_location_id") REFERENCES "md_locations"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_default_warehouse_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_default_warehouse_id_fkey"
      FOREIGN KEY ("default_warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_project_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_project_id_fkey"
      FOREIGN KEY ("project_id") REFERENCES "md_projects"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_cost_center_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_cost_center_id_fkey"
      FOREIGN KEY ("cost_center_id") REFERENCES "md_cost_centers"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END $$;

-- 5. Indexes for FK lookups
CREATE INDEX IF NOT EXISTS "md_items_division_id_idx"           ON "md_items"("division_id");
CREATE INDEX IF NOT EXISTS "md_items_department_id_idx"         ON "md_items"("department_id");
CREATE INDEX IF NOT EXISTS "md_items_branch_id_idx"             ON "md_items"("branch_id");
CREATE INDEX IF NOT EXISTS "md_items_default_location_id_idx"   ON "md_items"("default_location_id");
CREATE INDEX IF NOT EXISTS "md_items_default_warehouse_id_idx"  ON "md_items"("default_warehouse_id");
CREATE INDEX IF NOT EXISTS "md_items_project_id_idx"            ON "md_items"("project_id");
CREATE INDEX IF NOT EXISTS "md_items_cost_center_id_idx"        ON "md_items"("cost_center_id");
