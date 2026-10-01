-- Reset inventory data because relation key types are changing
TRUNCATE TABLE "m2_inventory_ledger", "m2_inventory_batch" RESTART IDENTITY;

-- Keep view definition consistent with new table shape
DROP VIEW IF EXISTS "v_m2_inventory_balance";

-- Drop FKs first
ALTER TABLE "m2_inventory_batch" DROP CONSTRAINT IF EXISTS "m2_inventory_batch_item_id_fkey";
ALTER TABLE "m2_inventory_ledger" DROP CONSTRAINT IF EXISTS "m2_inventory_ledger_item_id_fkey";
ALTER TABLE "m2_inventory_ledger" DROP CONSTRAINT IF EXISTS "m2_inventory_ledger_warehouse_id_fkey";
ALTER TABLE "m2_inventory_ledger" DROP CONSTRAINT IF EXISTS "m2_inventory_ledger_batch_id_fkey";
ALTER TABLE "m2_inventory_ledger" DROP CONSTRAINT IF EXISTS "m2_inventory_ledger_uom_id_fkey";
ALTER TABLE "m2_inventory_ledger" DROP CONSTRAINT IF EXISTS "m2_inventory_ledger_user_id_fkey";

-- Convert relation columns from TEXT(uuid) to INTEGER(id)
-- Using drop+add because inventory tables are rebuilt by backfill after this migration.
ALTER TABLE "m2_inventory_batch"
  DROP COLUMN "item_id",
  ADD COLUMN "item_id" INTEGER NOT NULL;

ALTER TABLE "m2_inventory_ledger"
  DROP COLUMN "item_id",
  ADD COLUMN "item_id" INTEGER NOT NULL,
  DROP COLUMN "warehouse_id",
  ADD COLUMN "warehouse_id" INTEGER NOT NULL,
  DROP COLUMN "batch_id",
  ADD COLUMN "batch_id" INTEGER NOT NULL,
  DROP COLUMN "uom_id",
  ADD COLUMN "uom_id" INTEGER NOT NULL,
  DROP COLUMN "user_id",
  ADD COLUMN "user_id" INTEGER;

-- Recreate indexes that depend on converted columns
DROP INDEX IF EXISTS "m2_inventory_batch_item_id_batch_number_key";
DROP INDEX IF EXISTS "m2_inventory_batch_item_id_idx";
DROP INDEX IF EXISTS "m2_inventory_ledger_item_id_warehouse_id_batch_id_idx";
DROP INDEX IF EXISTS "m2_inventory_ledger_item_id_warehouse_id_batch_id_transaction_date_idx";
DROP INDEX IF EXISTS "m2_inventory_ledger_batch_id_idx";

CREATE UNIQUE INDEX "m2_inventory_batch_item_id_batch_number_key" ON "m2_inventory_batch"("item_id", "batch_number");
CREATE INDEX "m2_inventory_batch_item_id_idx" ON "m2_inventory_batch"("item_id");
CREATE INDEX "m2_inventory_ledger_item_id_warehouse_id_batch_id_idx" ON "m2_inventory_ledger"("item_id", "warehouse_id", "batch_id");
CREATE INDEX "m2_inventory_ledger_item_id_warehouse_id_batch_id_transaction_date_idx" ON "m2_inventory_ledger"("item_id", "warehouse_id", "batch_id", "transaction_date");
CREATE INDEX "m2_inventory_ledger_batch_id_idx" ON "m2_inventory_ledger"("batch_id");

-- Recreate FKs to numeric PKs
ALTER TABLE "m2_inventory_batch"
  ADD CONSTRAINT "m2_inventory_batch_item_id_fkey"
  FOREIGN KEY ("item_id") REFERENCES "m1_item"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

ALTER TABLE "m2_inventory_ledger"
  ADD CONSTRAINT "m2_inventory_ledger_item_id_fkey"
  FOREIGN KEY ("item_id") REFERENCES "m1_item"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_inventory_ledger_warehouse_id_fkey"
  FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_inventory_ledger_batch_id_fkey"
  FOREIGN KEY ("batch_id") REFERENCES "m2_inventory_batch"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_inventory_ledger_uom_id_fkey"
  FOREIGN KEY ("uom_id") REFERENCES "m1_uom"("id") ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT "m2_inventory_ledger_user_id_fkey"
  FOREIGN KEY ("user_id") REFERENCES "m0_users"("id") ON DELETE SET NULL ON UPDATE CASCADE;

-- Recreate balance view
CREATE OR REPLACE VIEW "v_m2_inventory_balance" AS
SELECT
  l."item_id",
  l."warehouse_id",
  l."batch_id",
  SUM(l."quantity_pcs") AS "balance_pcs",
  SUM(l."quantity_kg") AS "balance_kg"
FROM "m2_inventory_ledger" l
WHERE l."deleted_at" IS NULL
GROUP BY l."item_id", l."warehouse_id", l."batch_id";
