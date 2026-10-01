-- Add structured facilities[] to clinic_room.
-- Sebelumnya admin pakai `description` comma-separated → backfill ke array
-- supaya tampilan di UI konsisten dan future booking wizard bisa filter
-- by facility (mis. "butuh ruangan dengan proyektor").

ALTER TABLE "clinic_room"
  ADD COLUMN IF NOT EXISTS "facilities" TEXT[] NOT NULL DEFAULT ARRAY[]::TEXT[];

-- Backfill: parse description comma-separated → facilities array.
-- Skip rooms yang facilities-nya sudah terisi (idempotent).
UPDATE "clinic_room"
SET "facilities" = (
  SELECT array_agg(trim(item))
  FROM unnest(string_to_array(coalesce("description", ''), ',')) AS item
  WHERE trim(item) <> ''
)
WHERE ("facilities" = ARRAY[]::TEXT[] OR "facilities" IS NULL)
  AND "description" IS NOT NULL
  AND trim("description") <> '';
