-- Currency master: decimal places + single base-currency flag
-- Additive only. Base currency is the org home currency; rates are relative to it.

ALTER TABLE "md_currencies"
  ADD COLUMN IF NOT EXISTS "decimal_places" INTEGER NOT NULL DEFAULT 2;

ALTER TABLE "md_currencies"
  ADD COLUMN IF NOT EXISTS "is_base" BOOLEAN NOT NULL DEFAULT false;

CREATE INDEX IF NOT EXISTS "md_currencies_is_base_idx"
  ON "md_currencies"("is_base");

-- Prefer IDR as base when present and nothing is base yet.
UPDATE "md_currencies"
SET "is_base" = true
WHERE "deleted_at" IS NULL
  AND lower("code") = 'idr'
  AND NOT EXISTS (
    SELECT 1 FROM "md_currencies" c2
    WHERE c2."is_base" = true AND c2."deleted_at" IS NULL
  );
