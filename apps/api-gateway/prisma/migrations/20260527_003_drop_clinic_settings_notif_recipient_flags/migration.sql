-- Drop kolom ClinicSettings.notif<event><recipient> setelah migrasi ke
-- ClinicWaTemplate.recipients sebagai single source of truth dispatch.
--
-- State pengiriman per-template sudah di-backfill ke template.recipients
-- oleh migration 20260527_002. Setelah migration ini, routing klien/psikolog
-- hanya dari ClinicWaTemplate.recipients + master switch waSendEnabled.
--
-- Yang TETAP di ClinicSettings: waSendEnabled, *SendTime, *DelayHours, notifFailedSendEmail.

ALTER TABLE clinic_settings
  DROP COLUMN IF EXISTS notif_confirm_klien,
  DROP COLUMN IF EXISTS notif_confirm_psikolog,
  DROP COLUMN IF EXISTS notif_h1_klien,
  DROP COLUMN IF EXISTS notif_m30_klien,
  DROP COLUMN IF EXISTS notif_followup_klien,
  DROP COLUMN IF EXISTS notif_feedback_klien,
  DROP COLUMN IF EXISTS notif_sesi_lanjutan_klien,
  DROP COLUMN IF EXISTS notif_sesi_lanjutan_days,
  DROP COLUMN IF EXISTS notif_paket_habis_klien,
  DROP COLUMN IF EXISTS notif_minggu_kosong_psikolog,
  DROP COLUMN IF EXISTS notif_minggu_kosong_days_before,
  DROP COLUMN IF EXISTS notif_minggu_kosong_threshold,
  DROP COLUMN IF EXISTS notif_reschedule_klien,
  DROP COLUMN IF EXISTS notif_reschedule_psikolog,
  DROP COLUMN IF EXISTS notif_cancel_klien,
  DROP COLUMN IF EXISTS notif_cancel_psikolog,
  DROP COLUMN IF EXISTS notif_ubah_ruangan_klien,
  DROP COLUMN IF EXISTS notif_ubah_ruangan_psikolog,
  DROP COLUMN IF EXISTS notif_ubah_layanan_klien,
  DROP COLUMN IF EXISTS notif_ubah_layanan_psikolog,
  DROP COLUMN IF EXISTS notif_welcome_klien,
  DROP COLUMN IF EXISTS notif_welcome_psikolog,
  DROP COLUMN IF EXISTS notif_invite_staff,
  DROP COLUMN IF EXISTS notif_otp_user,
  DROP COLUMN IF EXISTS notif_dp_klien,
  DROP COLUMN IF EXISTS notif_bukti_pembayaran_klien,
  DROP COLUMN IF EXISTS notif_pelunasan_klien;
