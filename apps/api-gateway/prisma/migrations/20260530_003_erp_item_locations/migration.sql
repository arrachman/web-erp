-- ERP item placements (legacy MyERP+ "Lokasi" tab = m1_item_location_warehouse):
-- per-item rows of (Gudang, Lokasi). warehouse_id -> md_warehouses (Gudang),
-- location_id -> md_item_locations (Lokasi = named storage spot master). Both FK to
-- md masters (intra-domain enforced). Multi placement beyond the single
-- defaultWarehouseId/defaultLocationId on md_items. Additive, idempotent. 0 DROP.

CREATE TABLE IF NOT EXISTS "md_item_placements" (
  "id"            BIGSERIAL PRIMARY KEY,
  "item_id"       BIGINT NOT NULL,
  "warehouse_id"  BIGINT NOT NULL,
  "location_id"   BIGINT NOT NULL,
  "created_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  CONSTRAINT "md_item_placements_item_warehouse_location_key" UNIQUE ("item_id", "warehouse_id", "location_id")
);

CREATE INDEX IF NOT EXISTS "md_item_placements_item_id_idx"      ON "md_item_placements"("item_id");
CREATE INDEX IF NOT EXISTS "md_item_placements_warehouse_id_idx" ON "md_item_placements"("warehouse_id");
CREATE INDEX IF NOT EXISTS "md_item_placements_location_id_idx"  ON "md_item_placements"("location_id");

-- AddForeignKey (intra-domain md — enforced). Cascade on item delete; restrict the
-- referenced gudang/lokasi masters (they soft-delete via deleted_at, never hard-drop).
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_placements_item_id_fkey'
  ) THEN
    ALTER TABLE "md_item_placements"
      ADD CONSTRAINT "md_item_placements_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_placements_warehouse_id_fkey'
  ) THEN
    ALTER TABLE "md_item_placements"
      ADD CONSTRAINT "md_item_placements_warehouse_id_fkey"
        FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_placements_location_id_fkey'
  ) THEN
    ALTER TABLE "md_item_placements"
      ADD CONSTRAINT "md_item_placements_location_id_fkey"
        FOREIGN KEY ("location_id") REFERENCES "md_item_locations"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;
END $$;
