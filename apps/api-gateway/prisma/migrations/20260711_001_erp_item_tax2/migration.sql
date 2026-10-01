ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "purchase_tax2_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "sale_tax2_id" BIGINT;

DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_purchase_tax2_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_purchase_tax2_id_fkey"
      FOREIGN KEY ("purchase_tax2_id") REFERENCES "md_taxes"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;

  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'md_items_sale_tax2_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_sale_tax2_id_fkey"
      FOREIGN KEY ("sale_tax2_id") REFERENCES "md_taxes"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END $$;

CREATE INDEX IF NOT EXISTS "md_items_purchase_tax2_id_idx" ON "md_items"("purchase_tax2_id");
CREATE INDEX IF NOT EXISTS "md_items_sale_tax2_id_idx" ON "md_items"("sale_tax2_id");
