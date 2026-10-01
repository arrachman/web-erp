-- Add weekly_availability ke ClinicPsikologProfile.
-- Backfill existing psikolog: Senin–Jumat OPEN, Sabtu+Minggu TUTUP (default klinik psikologi).
-- Psikolog baru tanpa availability → empty {} → admin tidak bisa booking.

ALTER TABLE "clinic_psikolog_profile"
  ADD COLUMN "weekly_availability" JSONB NOT NULL DEFAULT '{}'::jsonb;

-- Backfill existing rows
UPDATE "clinic_psikolog_profile" SET "weekly_availability" = '{
  "monday":    {"isOpen": true},
  "tuesday":   {"isOpen": true},
  "wednesday": {"isOpen": true},
  "thursday":  {"isOpen": true},
  "friday":    {"isOpen": true},
  "saturday":  {"isOpen": false},
  "sunday":    {"isOpen": false}
}'::jsonb
WHERE "weekly_availability" = '{}'::jsonb;
