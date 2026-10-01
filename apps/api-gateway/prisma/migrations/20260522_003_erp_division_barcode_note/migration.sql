-- Add barcode and note to md_divisions
ALTER TABLE "md_divisions" ADD COLUMN IF NOT EXISTS "barcode" TEXT;
ALTER TABLE "md_divisions" ADD COLUMN IF NOT EXISTS "note" TEXT;
