-- Tambah kolom `disabled_slot_indices` di clinic_service.
-- Default '[]' = layanan pakai semua slot global apa adanya.
-- Berisi index slot global (0-based) yang dinonaktifkan untuk layanan ini.
-- Booking yang nyentuh slot di list ini akan ditolak oleh assertSlotMatch.
ALTER TABLE "clinic_service"
  ADD COLUMN "disabled_slot_indices" JSONB NOT NULL DEFAULT '[]';
