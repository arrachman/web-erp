ALTER TABLE "m2_inventory_batch"
  DROP CONSTRAINT IF EXISTS "m2_inventory_batch_expiry_vs_mfg_check";

ALTER TABLE "m2_inventory_batch"
  DROP COLUMN IF EXISTS "manufacturing_date",
  DROP COLUMN IF EXISTS "supplier_lot_number",
  DROP COLUMN IF EXISTS "notes",
  DROP COLUMN IF EXISTS "is_active";
