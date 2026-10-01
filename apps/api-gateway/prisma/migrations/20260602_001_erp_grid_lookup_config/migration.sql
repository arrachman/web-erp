-- Add lookup configuration columns to sys_transaction_grid_columns.
-- Mirrors sys_form_fields lookup config so Kustomisasi Grid lookup columns can
-- carry a default sort + filter (in addition to the source).
-- lookup_default_filter: JSONB extra params merged into the loader call (e.g. {"isActive":true}).
-- lookup_default_sort: "field:direction" string, e.g. "name:asc".
-- Nullable so existing rows remain valid without backfill.

ALTER TABLE "sys_transaction_grid_columns"
  ADD COLUMN IF NOT EXISTS "lookup_default_filter" JSONB,
  ADD COLUMN IF NOT EXISTS "lookup_default_sort"   TEXT;
