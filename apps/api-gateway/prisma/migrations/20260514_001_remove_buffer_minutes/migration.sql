-- AlterTable: drop bufferMinutes (buffer_minutes) from clinic_settings
ALTER TABLE "clinic_settings" DROP COLUMN IF EXISTS "buffer_minutes";
