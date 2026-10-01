-- Repurpose md_items.field_unit_id as "Satuan Jual Default" (default selling unit).
-- Add conversion factor: 1 selling unit = N base units (e.g. 1 kwintal = 100 kg).

ALTER TABLE "md_items" ADD COLUMN "field_unit_factor" DECIMAL(19,4) NOT NULL DEFAULT 1;
