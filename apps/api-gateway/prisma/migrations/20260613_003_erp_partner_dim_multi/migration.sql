-- Multi-select dimensions (Cabang / Gudang / Lokasi) on md_partners.
-- New junction tables md_partner_dim_branches / md_partner_dim_warehouses / md_partner_dim_locations.
-- The existing single column md_partners.branch_id stays as the primary/default (= first branch).
-- Warehouse & location have no single column on md_partners (pivot-only).

CREATE TABLE "md_partner_dim_branches" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "branch_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_partner_dim_branches_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_partner_dim_branches_partner_id_branch_id_key" ON "md_partner_dim_branches"("partner_id", "branch_id");
CREATE INDEX "md_partner_dim_branches_branch_id_idx" ON "md_partner_dim_branches"("branch_id");

ALTER TABLE "md_partner_dim_branches"
    ADD CONSTRAINT "md_partner_dim_branches_partner_id_fkey" FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_partner_dim_branches"
    ADD CONSTRAINT "md_partner_dim_branches_branch_id_fkey" FOREIGN KEY ("branch_id") REFERENCES "md_branches"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

CREATE TABLE "md_partner_dim_warehouses" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_partner_dim_warehouses_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_partner_dim_warehouses_partner_id_warehouse_id_key" ON "md_partner_dim_warehouses"("partner_id", "warehouse_id");
CREATE INDEX "md_partner_dim_warehouses_warehouse_id_idx" ON "md_partner_dim_warehouses"("warehouse_id");

ALTER TABLE "md_partner_dim_warehouses"
    ADD CONSTRAINT "md_partner_dim_warehouses_partner_id_fkey" FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_partner_dim_warehouses"
    ADD CONSTRAINT "md_partner_dim_warehouses_warehouse_id_fkey" FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

CREATE TABLE "md_partner_dim_locations" (
    "id" BIGSERIAL NOT NULL,
    "partner_id" BIGINT NOT NULL,
    "location_id" BIGINT NOT NULL,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "md_partner_dim_locations_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_partner_dim_locations_partner_id_location_id_key" ON "md_partner_dim_locations"("partner_id", "location_id");
CREATE INDEX "md_partner_dim_locations_location_id_idx" ON "md_partner_dim_locations"("location_id");

ALTER TABLE "md_partner_dim_locations"
    ADD CONSTRAINT "md_partner_dim_locations_partner_id_fkey" FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE "md_partner_dim_locations"
    ADD CONSTRAINT "md_partner_dim_locations_location_id_fkey" FOREIGN KEY ("location_id") REFERENCES "md_locations"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- Backfill: existing single branch becomes the first (only) dim row.
INSERT INTO "md_partner_dim_branches" ("partner_id", "branch_id")
SELECT "id", "branch_id" FROM "md_partners" WHERE "branch_id" IS NOT NULL
ON CONFLICT DO NOTHING;
