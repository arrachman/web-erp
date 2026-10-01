-- Tighten partner-type invariants after introducing md_partner_types.
-- Needed for databases where 20260711_002 already ran before the backfill/non-null fixes.

INSERT INTO "md_partner_types" ("code", "name", "kind", "is_active", "created_at", "updated_at") VALUES
  ('CUST', 'Customer', 'CUSTOMER', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
  ('SUP', 'Supplier', 'SUPPLIER', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
  ('SLS', 'Salesman', 'SALESMAN', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
  ('GEN', 'General', 'GENERAL', true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT ("code") DO NOTHING;

UPDATE "md_partners"
SET "partner_type_id" = (SELECT id FROM "md_partner_types" WHERE code = 'GEN')
WHERE "partner_type_id" IS NULL;

ALTER TABLE "md_partners" ALTER COLUMN "partner_type_id" SET NOT NULL;

ALTER TABLE "md_partners" DROP CONSTRAINT IF EXISTS "md_partners_partner_type_id_fkey";
ALTER TABLE "md_partners" ADD CONSTRAINT "md_partners_partner_type_id_fkey"
  FOREIGN KEY ("partner_type_id") REFERENCES "md_partner_types"("id") ON DELETE RESTRICT ON UPDATE CASCADE;

-- Stored dynamic lookup filters used by Form Builder / Grid Customization.
UPDATE "sys_form_fields"
SET "lookup_default_filter" =
  CASE
    WHEN "lookup_default_filter"->>'isCustomer' = 'true' THEN ("lookup_default_filter" - 'isCustomer' - 'isSupplier' - 'isSalesman') || '{"typeKind":"CUSTOMER"}'::jsonb
    WHEN "lookup_default_filter"->>'isSupplier' = 'true' THEN ("lookup_default_filter" - 'isCustomer' - 'isSupplier' - 'isSalesman') || '{"typeKind":"SUPPLIER"}'::jsonb
    WHEN "lookup_default_filter"->>'isSalesman' = 'true' THEN ("lookup_default_filter" - 'isCustomer' - 'isSupplier' - 'isSalesman') || '{"typeKind":"SALESMAN"}'::jsonb
    ELSE "lookup_default_filter"
  END
WHERE "lookup_default_filter" ?| ARRAY['isCustomer', 'isSupplier', 'isSalesman'];

UPDATE "sys_transaction_grid_columns"
SET "lookup_default_filter" =
  CASE
    WHEN "lookup_default_filter"->>'isCustomer' = 'true' THEN ("lookup_default_filter" - 'isCustomer' - 'isSupplier' - 'isSalesman') || '{"typeKind":"CUSTOMER"}'::jsonb
    WHEN "lookup_default_filter"->>'isSupplier' = 'true' THEN ("lookup_default_filter" - 'isCustomer' - 'isSupplier' - 'isSalesman') || '{"typeKind":"SUPPLIER"}'::jsonb
    WHEN "lookup_default_filter"->>'isSalesman' = 'true' THEN ("lookup_default_filter" - 'isCustomer' - 'isSupplier' - 'isSalesman') || '{"typeKind":"SALESMAN"}'::jsonb
    ELSE "lookup_default_filter"
  END
WHERE "lookup_default_filter" ?| ARRAY['isCustomer', 'isSupplier', 'isSalesman'];
