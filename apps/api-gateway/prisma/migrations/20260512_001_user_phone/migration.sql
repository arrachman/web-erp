-- Tambah kolom phone di m0_users untuk No WA psikolog (& user lain).
ALTER TABLE "m0_users" ADD COLUMN IF NOT EXISTS "phone" TEXT;
