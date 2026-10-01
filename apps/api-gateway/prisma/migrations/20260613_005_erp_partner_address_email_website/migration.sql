-- Add email and website fields to md_partner_addresses
ALTER TABLE "md_partner_addresses"
  ADD COLUMN IF NOT EXISTS "email" TEXT,
  ADD COLUMN IF NOT EXISTS "website" TEXT;
