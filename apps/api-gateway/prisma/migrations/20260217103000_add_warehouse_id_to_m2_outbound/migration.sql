ALTER TABLE "m2_outbound"
ADD COLUMN IF NOT EXISTS "warehouse_id" INTEGER;

CREATE INDEX IF NOT EXISTS "m2_outbound_warehouse_id_idx"
ON "m2_outbound"("warehouse_id");

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1
    FROM pg_constraint
    WHERE conname = 'm2_outbound_warehouse_id_fkey'
  ) THEN
    ALTER TABLE "m2_outbound"
    ADD CONSTRAINT "m2_outbound_warehouse_id_fkey"
    FOREIGN KEY ("warehouse_id") REFERENCES "m1_warehouse"("id")
    ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END $$;
