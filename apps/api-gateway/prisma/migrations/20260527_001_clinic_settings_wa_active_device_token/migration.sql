-- Add waActiveDeviceToken column to ClinicSettings.
-- Holds the per-device Fonnte send token that was last activated via
-- /clinic/settings/wa-devices/activate. Null = fallback to env FONNTE_API_TOKEN.

ALTER TABLE "clinic_settings"
ADD COLUMN IF NOT EXISTS "wa_active_device_token" TEXT;
