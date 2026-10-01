-- ERP item price tiers (legacy MyERP+ "Harga"): purchase discount on md_items +
-- normalized sale price tiers (Harga Jual 1..10 + Diskon Jual 1..10) in md_item_prices.
-- Additive, idempotent. 0 DROP.

-- AlterTable: purchase discount (Diskon Pembelian) percent
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "purchase_discount" DECIMAL(9,4) NOT NULL DEFAULT 0;

-- CreateTable: sale price tiers (one row per item + level 1..10)
CREATE TABLE IF NOT EXISTS "md_item_prices" (
  "id"               BIGSERIAL PRIMARY KEY,
  "item_id"          BIGINT NOT NULL,
  "level"            INTEGER NOT NULL,
  "price"            DECIMAL(19,4) NOT NULL DEFAULT 0,
  "discount_percent" DECIMAL(9,4) NOT NULL DEFAULT 0,
  "created_at"       TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "updated_at"       TIMESTAMPTZ(6) NOT NULL DEFAULT NOW(),
  "created_by_id"    BIGINT,
  "updated_by_id"    BIGINT,
  CONSTRAINT "md_item_prices_item_id_level_key" UNIQUE ("item_id", "level")
);

CREATE INDEX IF NOT EXISTS "md_item_prices_item_id_idx" ON "md_item_prices"("item_id");

-- AddForeignKey (intra-domain md — enforced; cascade on item delete)
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_prices_item_id_fkey'
  ) THEN
    ALTER TABLE "md_item_prices"
      ADD CONSTRAINT "md_item_prices_item_id_fkey"
        FOREIGN KEY ("item_id") REFERENCES "md_items"("id") ON DELETE CASCADE ON UPDATE CASCADE;
  END IF;
END $$;
