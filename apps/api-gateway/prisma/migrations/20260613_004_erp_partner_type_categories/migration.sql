-- Add per-type category FK columns to md_partners
ALTER TABLE "md_partners"
  ADD COLUMN IF NOT EXISTS "customer_category_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "supplier_category_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "salesman_category_id" BIGINT;

ALTER TABLE "md_partners"
  ADD CONSTRAINT "md_partners_customer_category_id_fkey"
    FOREIGN KEY ("customer_category_id") REFERENCES "md_partner_categories"("id")
    ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_partners_supplier_category_id_fkey"
    FOREIGN KEY ("supplier_category_id") REFERENCES "md_partner_categories"("id")
    ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT "md_partners_salesman_category_id_fkey"
    FOREIGN KEY ("salesman_category_id") REFERENCES "md_partner_categories"("id")
    ON DELETE SET NULL ON UPDATE CASCADE;

CREATE INDEX IF NOT EXISTS "md_partners_customer_category_id_idx" ON "md_partners"("customer_category_id");
CREATE INDEX IF NOT EXISTS "md_partners_supplier_category_id_idx" ON "md_partners"("supplier_category_id");
CREATE INDEX IF NOT EXISTS "md_partners_salesman_category_id_idx" ON "md_partners"("salesman_category_id");
