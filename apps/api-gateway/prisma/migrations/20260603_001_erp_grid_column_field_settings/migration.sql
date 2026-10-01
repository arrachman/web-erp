-- Add per-column field settings to sys_transaction_grid_columns (Kustomisasi Grid).
-- Mirrors sys_form_fields so grid line columns reach parity with the Form Builder:
-- placeholder         : cell placeholder shown when the cell is empty; null = built-in default.
-- default_value       : value prefilled on a NEW line row (lookup columns store the id, others a raw string).
-- default_value_label : resolved "{code} - {name}" label for a lookup default_value, stored at pick time
--                       (grid lookup sources include taxes which have no `code` column, so the label is
--                        stored rather than derived server-side like the Form Builder does).
-- All nullable so existing rows stay valid without backfill.

ALTER TABLE "sys_transaction_grid_columns"
  ADD COLUMN IF NOT EXISTS "placeholder"         TEXT,
  ADD COLUMN IF NOT EXISTS "default_value"       TEXT,
  ADD COLUMN IF NOT EXISTS "default_value_label" TEXT;
