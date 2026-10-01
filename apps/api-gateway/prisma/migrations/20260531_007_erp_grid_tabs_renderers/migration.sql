-- Kustomisasi Grid — tabbing (banyak grid/tab per jenis transaksi) + slot
-- renderer/formatter per kolom + flag skip-fokus. Kolom yang tadinya menempel
-- langsung ke transaction_type dipindah menjadi anak dari grid (tab).
-- Data-preserving: tiap jenis transaksi existing dapat 1 grid "main" (primary),
-- semua kolomnya di-repoint ke situ. Tidak ada DROP tabel; clinic/m0_*/m1_* aman.

-- 1. Grid (tab) di bawah satu jenis transaksi.
CREATE TABLE IF NOT EXISTS "sys_transaction_grids" (
    "id" BIGSERIAL NOT NULL,
    "transaction_type_id" BIGINT NOT NULL,
    "key" TEXT NOT NULL,
    "label" TEXT NOT NULL,
    "sort_order" INTEGER NOT NULL DEFAULT 0,
    "line_table" TEXT,
    "is_primary" BOOLEAN NOT NULL DEFAULT false,
    "is_active" BOOLEAN NOT NULL DEFAULT true,
    "created_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" TIMESTAMPTZ(6) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at" TIMESTAMPTZ(6),

    CONSTRAINT "sys_transaction_grids_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "sys_transaction_grids_type_key_key" ON "sys_transaction_grids"("transaction_type_id", "key");
CREATE INDEX IF NOT EXISTS "sys_transaction_grids_transaction_type_id_idx" ON "sys_transaction_grids"("transaction_type_id");

ALTER TABLE "sys_transaction_grids"
    ADD CONSTRAINT "sys_transaction_grids_transaction_type_id_fkey"
    FOREIGN KEY ("transaction_type_id") REFERENCES "sys_transaction_types"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- 2. Backfill: grid "main" (primary) untuk setiap jenis transaksi yang ada.
INSERT INTO "sys_transaction_grids" ("transaction_type_id", "key", "label", "sort_order", "line_table", "is_primary")
SELECT t."id", 'main', 'Utama', 0, t."line_table", true
FROM "sys_transaction_types" t
ON CONFLICT ("transaction_type_id", "key") DO NOTHING;

-- 3. Re-parent kolom: dari transaction_type_id → grid_id (grid "main").
ALTER TABLE "sys_transaction_grid_columns" ADD COLUMN IF NOT EXISTS "grid_id" BIGINT;

UPDATE "sys_transaction_grid_columns" c
SET "grid_id" = g."id"
FROM "sys_transaction_grids" g
WHERE g."transaction_type_id" = c."transaction_type_id" AND g."key" = 'main' AND c."grid_id" IS NULL;

-- Lepas constraint/kolom lama yang ber-scope transaction_type.
ALTER TABLE "sys_transaction_grid_columns" DROP CONSTRAINT IF EXISTS "sys_transaction_grid_columns_transaction_type_id_fkey";
DROP INDEX IF EXISTS "sys_transaction_grid_columns_type_field_key";
DROP INDEX IF EXISTS "sys_transaction_grid_columns_transaction_type_id_idx";
ALTER TABLE "sys_transaction_grid_columns" DROP COLUMN IF EXISTS "transaction_type_id";

ALTER TABLE "sys_transaction_grid_columns" ALTER COLUMN "grid_id" SET NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "sys_transaction_grid_columns_grid_field_key" ON "sys_transaction_grid_columns"("grid_id", "data_field");
CREATE INDEX IF NOT EXISTS "sys_transaction_grid_columns_grid_id_idx" ON "sys_transaction_grid_columns"("grid_id");

ALTER TABLE "sys_transaction_grid_columns"
    ADD CONSTRAINT "sys_transaction_grid_columns_grid_id_fkey"
    FOREIGN KEY ("grid_id") REFERENCES "sys_transaction_grids"("id") ON DELETE CASCADE ON UPDATE CASCADE;

-- 4. Slot baru per kolom: skip-fokus + formatter/renderer/editor (TEXT, app-level allowlist).
ALTER TABLE "sys_transaction_grid_columns"
    ADD COLUMN IF NOT EXISTS "is_skippable" BOOLEAN NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "label_formatter" TEXT,
    ADD COLUMN IF NOT EXISTS "header_renderer" TEXT,
    ADD COLUMN IF NOT EXISTS "cell_renderer" TEXT,
    ADD COLUMN IF NOT EXISTS "cell_editor" TEXT;
