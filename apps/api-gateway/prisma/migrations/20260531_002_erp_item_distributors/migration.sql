-- ERP item distributors (legacy MyERP+ "Distributor" tab = m1_item_supplier):
-- per-item rows of (partner) = the supplier/distributor partners that supply the
-- item. partner_id -> md_partners (filtered to is_supplier on the UI). Multi-row
-- beyond the single primary_supplier_id on md_items. sort_order keeps display
-- order. Additive, idempotent. 0 DROP.

CREATE TABLE IF NOT EXISTS "md_item_distributors" (
  "id"            BIGSERIAL PRIMARY KEY,
  "item_id"       BIGINT NOT NULL,
  "partner_id"    BIGINT NOT NULL,
  "sort_order"    INTEGER NOT NULL DEFAULT 0,
  "created_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"    TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT,
  CONSTRAINT "md_item_distributors_item_partner_key" UNIQUE ("item_id", "partner_id")
);

CREATE INDEX IF NOT EXISTS "md_item_distributors_item_id_idx"    ON "md_item_distributors"("item_id");
CREATE INDEX IF NOT EXISTS "md_item_distributors_partner_id_idx" ON "md_item_distributors"("partner_id");

-- AddForeignKey. Cascade on item delete; restrict the referenced partner master
-- (partners soft-delete via deleted_at, never hard-drop).
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_distributors_item_id_fkey'
  ) THEN
    ALTER TABLE "md_item_distributors"
      ADD CONSTRAINT "md_item_distributors_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
  END IF;

  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_distributors_partner_id_fkey'
  ) THEN
    ALTER TABLE "md_item_distributors"
      ADD CONSTRAINT "md_item_distributors_partner_id_fkey"
        FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id") ON DELETE RESTRICT ON UPDATE CASCADE;
  END IF;
END $$;
