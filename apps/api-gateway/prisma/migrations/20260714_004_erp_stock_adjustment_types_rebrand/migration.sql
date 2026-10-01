-- Rebrand Item Transaction Types → Stock Adjustment Types.
-- 1) Rename physical table + indexes
-- 2) Add optional postable GL account (No Akun)
-- 3) Update menu title/path for live UI

ALTER TABLE "md_item_transaction_types" RENAME TO "md_stock_adjustment_types";

ALTER TABLE "md_stock_adjustment_types"
  RENAME CONSTRAINT "md_item_transaction_types_pkey" TO "md_stock_adjustment_types_pkey";

ALTER INDEX "md_item_transaction_types_code_key"
  RENAME TO "md_stock_adjustment_types_code_key";

ALTER INDEX "md_item_transaction_types_legacy_code_idx"
  RENAME TO "md_stock_adjustment_types_legacy_code_idx";

ALTER TABLE "md_stock_adjustment_types"
  ADD COLUMN "account_id" BIGINT;

CREATE INDEX "md_stock_adjustment_types_account_id_idx"
  ON "md_stock_adjustment_types"("account_id");

ALTER TABLE "md_stock_adjustment_types"
  ADD CONSTRAINT "md_stock_adjustment_types_account_id_fkey"
  FOREIGN KEY ("account_id") REFERENCES "md_accounts"("id")
  ON DELETE SET NULL ON UPDATE CASCADE;

-- Live menu label + route (code kept for role_menu stability)
UPDATE "sys_menus"
SET
  "title" = 'Stock Adjustment Types',
  "path" = '/master/stock-adjustment-types',
  "updated_at" = CURRENT_TIMESTAMP
WHERE "code" = 'M1.REF.ITEM-TXN-TYPE';
