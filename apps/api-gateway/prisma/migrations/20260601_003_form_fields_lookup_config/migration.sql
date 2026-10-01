-- Add lookup configuration columns to sys_form_fields.
-- lookup_default_filter: JSONB extra params merged into the loader call (e.g. {"isCustomer":true}).
-- lookup_default_sort: "field:direction" string, e.g. "name:asc".

ALTER TABLE "sys_form_fields"
  ADD COLUMN IF NOT EXISTS "lookup_default_filter" JSONB,
  ADD COLUMN IF NOT EXISTS "lookup_default_sort"   TEXT;
