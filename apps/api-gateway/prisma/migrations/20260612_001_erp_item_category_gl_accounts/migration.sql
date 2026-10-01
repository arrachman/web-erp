-- Add 5 GL account mappings to md_item_categories (parity with legacy MyERP+ 8-account set:
-- existing inventory/cogs/sales + new sales return, sales discount, purchase return,
-- purchase discount, consignment). All nullable FK to md_accounts.

ALTER TABLE "md_item_categories" ADD COLUMN "sales_return_account_id" BIGINT;
ALTER TABLE "md_item_categories" ADD COLUMN "sales_discount_account_id" BIGINT;
ALTER TABLE "md_item_categories" ADD COLUMN "purchase_return_account_id" BIGINT;
ALTER TABLE "md_item_categories" ADD COLUMN "purchase_discount_account_id" BIGINT;
ALTER TABLE "md_item_categories" ADD COLUMN "consignment_account_id" BIGINT;

ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_sales_return_account_id_fkey"
  FOREIGN KEY ("sales_return_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_sales_discount_account_id_fkey"
  FOREIGN KEY ("sales_discount_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_purchase_return_account_id_fkey"
  FOREIGN KEY ("purchase_return_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_purchase_discount_account_id_fkey"
  FOREIGN KEY ("purchase_discount_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
ALTER TABLE "md_item_categories" ADD CONSTRAINT "md_item_categories_consignment_account_id_fkey"
  FOREIGN KEY ("consignment_account_id") REFERENCES "md_accounts"("id") ON DELETE SET NULL ON UPDATE CASCADE;
