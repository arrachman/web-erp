-- Master lokasi penyimpanan hierarkis per gudang (zona → rak → bin).
-- Pengganti md_item_locations (flat). Data lama (10 row, semua tanpa gudang,
-- 0 placement) dimigrasi ke gudang pertama, lalu tabel lama di-drop.

CREATE TABLE "md_storage_bins" (
    "id" BIGSERIAL NOT NULL,
    "code" TEXT NOT NULL,
    "name" TEXT NOT NULL,
    "warehouse_id" BIGINT NOT NULL,
    "parent_id" BIGINT,
    "bin_type" TEXT NOT NULL DEFAULT 'BIN',
    "notes" TEXT,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "legacy_code" TEXT,
    "metadata" JSONB,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "md_storage_bins_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "md_storage_bins_warehouse_id_code_key" ON "md_storage_bins"("warehouse_id", "code");
CREATE INDEX "md_storage_bins_warehouse_id_idx" ON "md_storage_bins"("warehouse_id");
CREATE INDEX "md_storage_bins_parent_id_idx" ON "md_storage_bins"("parent_id");
CREATE INDEX "md_storage_bins_legacy_code_idx" ON "md_storage_bins"("legacy_code");

ALTER TABLE "md_storage_bins"
    ADD CONSTRAINT "md_storage_bins_warehouse_id_fkey"
    FOREIGN KEY ("warehouse_id") REFERENCES "md_warehouses"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
ALTER TABLE "md_storage_bins"
    ADD CONSTRAINT "md_storage_bins_parent_id_fkey"
    FOREIGN KEY ("parent_id") REFERENCES "md_storage_bins"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- Migrasi data lama: semua row md_item_locations warehouse_id-nya NULL →
-- assign ke gudang aktif pertama (bisa di-reassign dari UI). Type default BIN.
INSERT INTO "md_storage_bins"
    ("code", "name", "warehouse_id", "bin_type", "is_active", "legacy_code", "metadata",
     "created_at", "updated_at", "created_by_id", "updated_by_id", "deleted_at")
SELECT
    il."code", il."name",
    COALESCE(il."warehouse_id", (SELECT w."id" FROM "md_warehouses" w WHERE w."deleted_at" IS NULL ORDER BY w."id" LIMIT 1)),
    'BIN', il."is_active", il."legacy_code", il."metadata",
    il."created_at", il."updated_at", il."created_by_id", il."updated_by_id", il."deleted_at"
FROM "md_item_locations" il;

-- Repoint placements FK ke md_storage_bins (tabel placements kosong saat migrasi).
ALTER TABLE "md_item_placements" DROP CONSTRAINT IF EXISTS "md_item_placements_location_id_fkey";
DELETE FROM "md_item_placements";
ALTER TABLE "md_item_placements"
    ADD CONSTRAINT "md_item_placements_location_id_fkey"
    FOREIGN KEY ("location_id") REFERENCES "md_storage_bins"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

DROP TABLE "md_item_locations";
