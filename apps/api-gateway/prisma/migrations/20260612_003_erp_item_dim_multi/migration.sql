-- Multi-select GL dimensions (Cabang / Gudang Default / Lokasi Default) on md_items.
-- New junction tables md_item_dim_branches / md_item_dim_warehouses / md_item_dim_locations.
-- Existing single columns on md_items stay as the primary/default (= first selection).

CREATE TABLE "md_item_dim_branches" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_item_dim_branches_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_item_dim_branches_item_id_branch_id_key" ON "md_item_dim_branches"("item_id", "branch_id");
CREATE INDEX "md_item_dim_branches_branch_id_idx" ON "md_item_dim_branches"("branch_id");

ALTER TABLE "md_item_dim_branches"
    ADD CONSTRAINT "md_item_dim_branches_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_item_dim_branches"
    ADD CONSTRAINT "md_item_dim_branches_branch_id_fkey" FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

CREATE TABLE "md_item_dim_warehouses" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_item_dim_warehouses_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_item_dim_warehouses_item_id_warehouse_id_key" ON "md_item_dim_warehouses"("item_id", "warehouse_id");
CREATE INDEX "md_item_dim_warehouses_warehouse_id_idx" ON "md_item_dim_warehouses"("warehouse_id");

ALTER TABLE "md_item_dim_warehouses"
    ADD CONSTRAINT "md_item_dim_warehouses_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_item_dim_warehouses"
    ADD CONSTRAINT "md_item_dim_warehouses_warehouse_id_fkey" FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

CREATE TABLE "md_item_dim_locations" (
    "id" BIGSERIAL NOT NULL,
    "item_id" BIGINT NOT NULL,
    "location_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_item_dim_locations_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_item_dim_locations_item_id_location_id_key" ON "md_item_dim_locations"("item_id", "location_id");
CREATE INDEX "md_item_dim_locations_location_id_idx" ON "md_item_dim_locations"("location_id");

ALTER TABLE "md_item_dim_locations"
    ADD CONSTRAINT "md_item_dim_locations_item_id_fkey" FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_item_dim_locations"
    ADD CONSTRAINT "md_item_dim_locations_location_id_fkey" FOREIGN KEY ("location_id") REFERENCES "md_locations"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- Backfill: existing single-value dimensions become the first (only) row.
INSERT INTO "md_item_dim_branches" ("item_id", "branch_id")
SELECT "id", "branch_id" FROM "md_items" WHERE "branch_id" IS NOT NULL
ON CONFLICT DO NOTHING;

INSERT INTO "md_item_dim_warehouses" ("item_id", "warehouse_id")
SELECT "id", "default_warehouse_id" FROM "md_items" WHERE "default_warehouse_id" IS NOT NULL
ON CONFLICT DO NOTHING;

INSERT INTO "md_item_dim_locations" ("item_id", "location_id")
SELECT "id", "default_location_id" FROM "md_items" WHERE "default_location_id" IS NOT NULL
ON CONFLICT DO NOTHING;
