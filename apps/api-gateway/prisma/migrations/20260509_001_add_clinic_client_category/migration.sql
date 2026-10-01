-- AlterTable
ALTER TABLE "clinic_client" ADD COLUMN "category" TEXT;

-- Backfill heuristic: derive default from age (matches mockup categorization)
UPDATE "clinic_client" SET "category" = CASE
  WHEN "age" IS NULL THEN NULL
  WHEN "age" < 12 THEN 'anak'
  WHEN "age" < 18 THEN 'remaja'
  ELSE 'dewasa'
END WHERE "category" IS NULL;
