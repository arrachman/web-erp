-- Form Builder / Kustomisasi Grid non-standard fields (JSONB) for Sales Order.
-- Header custom fields live on the order; per-line custom fields on the line.
ALTER TABLE "sls_orders" ADD COLUMN IF NOT EXISTS "custom_fields" JSONB;
ALTER TABLE "sls_order_lines" ADD COLUMN IF NOT EXISTS "custom_fields" JSONB;
