-- AddColumn sub_area_id to md_partner_addresses
ALTER TABLE "md_partner_addresses"
  ADD COLUMN IF NOT EXISTS "sub_area_id" BIGINT;

ALTER TABLE "md_partner_addresses"
  ADD CONSTRAINT "md_partner_addresses_sub_area_id_fkey"
  FOREIGN KEY ("sub_area_id") REFERENCES "md_sub_areas"("id")
  ON DELETE SET NULL ON UPDATE CASCADE
  DEFERRABLE INITIALLY DEFERRED;

CREATE INDEX IF NOT EXISTS "md_partner_addresses_sub_area_id_idx"
  ON "md_partner_addresses"("sub_area_id");
