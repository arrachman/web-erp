-- Add columnType field to sys_transaction_grid_columns
-- Stores the semantic column type (text|number|currency|...) that derives the 4 presentation slots.
-- Nullable so existing rows remain valid without backfill.
ALTER TABLE "sys_transaction_grid_columns"
  ADD COLUMN IF NOT EXISTS "column_type" TEXT;
