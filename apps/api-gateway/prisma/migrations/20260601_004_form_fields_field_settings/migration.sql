-- Add per-field settings to sys_form_fields (Form Builder).
-- placeholder:   input placeholder text; null = form falls back to its built-in default.
-- default_value: value prefilled on NEW records (lookup types store the id, others a raw string).
-- is_readonly:   field is always non-editable regardless of the document workflow status.

ALTER TABLE "sys_form_fields"
  ADD COLUMN IF NOT EXISTS "placeholder"   TEXT,
  ADD COLUMN IF NOT EXISTS "default_value" TEXT,
  ADD COLUMN IF NOT EXISTS "is_readonly"   BOOLEAN NOT NULL DEFAULT false;
