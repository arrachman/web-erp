-- Corrective: md_item_placements.location_id must reference the storage-spot master
-- md_item_locations (legacy "Lokasi"), not the site-level md_locations. An earlier
-- draft of migration 003 pointed it at md_locations; repoint it here. Idempotent.
-- The table is freshly created and empty, so the FK swap is safe.

DO $$
BEGIN
  -- Drop whichever target the existing FK has (md_locations from the draft, or
  -- md_item_locations if 003 already shipped correct — drop+re-add is harmless).
  IF EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'md_item_placements_location_id_fkey'
  ) THEN
    ALTER TABLE "md_item_placements" DROP CONSTRAINT "md_item_placements_location_id_fkey";
  END IF;

  ALTER TABLE "md_item_placements"
    ADD CONSTRAINT "md_item_placements_location_id_fkey"
      FOREIGN KEY ("location_id") REFERENCES "md_item_locations"("id")
      ON DELETE RESTRICT ON UPDATE CASCADE;
END $$;
