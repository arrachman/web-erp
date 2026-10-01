-- ERP md_items — legacy "Akun" tab parity: 5 additional GL account FKs.
-- Persediaan/Penjualan/HPP (inventory/sales/cogs) already exist; this adds
-- Retur Penjualan, Diskon Penjualan, Retur Pembelian, Diskon Pembelian,
-- Konsinyasi. All nullable, additive, re-runnable (IF NOT EXISTS guards).

-- 1. Columns (all nullable BIGINT → md_accounts)
ALTER TABLE "md_items"
  ADD COLUMN IF NOT EXISTS "sales_return_account_id"      BIGINT,
  ADD COLUMN IF NOT EXISTS "sales_discount_account_id"    BIGINT,
  ADD COLUMN IF NOT EXISTS "purchase_return_account_id"   BIGINT,
  ADD COLUMN IF NOT EXISTS "purchase_discount_account_id" BIGINT,
  ADD COLUMN IF NOT EXISTS "consignment_account_id"       BIGINT;

-- 2. Foreign keys → md_accounts (ErpAccount). Optional ⇒ ON DELETE SET NULL.
DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_sales_return_account_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_sales_return_account_id_fkey"
      FOREIGN KEY ("sales_return_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_sales_discount_account_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_sales_discount_account_id_fkey"
      FOREIGN KEY ("sales_discount_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_purchase_return_account_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_purchase_return_account_id_fkey"
      FOREIGN KEY ("purchase_return_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_purchase_discount_account_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_purchase_discount_account_id_fkey"
      FOREIGN KEY ("purchase_discount_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
  IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'md_items_consignment_account_id_fkey') THEN
    ALTER TABLE "md_items" ADD CONSTRAINT "md_items_consignment_account_id_fkey"
      FOREIGN KEY ("consignment_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
  END IF;
END $$;
