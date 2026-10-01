-- Replace operating_hours with slots_of_day + closed_day_of_week
-- Sumber default seed: apps/psychology-design/althea-data.jsx (mockup yang
-- sudah disetujui klien per JAWABAN-PERTANYAAN-KLIEN #6).

-- AlterTable: add new columns
ALTER TABLE "clinic_settings" ADD COLUMN "slots_of_day" JSONB NOT NULL DEFAULT '[]'::jsonb;
ALTER TABLE "clinic_settings" ADD COLUMN "closed_day_of_week" JSONB NOT NULL DEFAULT '[0]'::jsonb;

-- Seed default 6 slot mockup ke row settings (id=1) kalau belum ada
UPDATE "clinic_settings" SET "slots_of_day" = '[
  {"start": "08:30", "end": "10:00", "label": "Pagi 1"},
  {"start": "10:00", "end": "11:30", "label": "Pagi 2"},
  {"start": "12:00", "end": "13:30", "label": "Siang 1"},
  {"start": "13:30", "end": "15:00", "label": "Siang 2"},
  {"start": "15:15", "end": "16:45", "label": "Sore 1"},
  {"start": "16:45", "end": "18:15", "label": "Sore 2"}
]'::jsonb
WHERE "id" = 1;

-- Drop old operating_hours column
ALTER TABLE "clinic_settings" DROP COLUMN "operating_hours";
