-- Per-layanan slot time-range override.
-- Identitas slot (jumlah/label/urutan/index) tetap dari clinic_settings.slots_of_day;
-- kolom ini hanya menyimpan slot yang start/end-nya digeser untuk layanan ini.
-- Bentuk: [{ "index": 0, "start": "08:00", "end": "10:00" }, ...]
ALTER TABLE "clinic_service"
  ADD COLUMN "slot_overrides" JSONB NOT NULL DEFAULT '[]';
