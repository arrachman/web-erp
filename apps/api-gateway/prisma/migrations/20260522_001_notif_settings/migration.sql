-- Migration: add_notif_settings
-- Adds per-event notification toggles + timing config to clinic_settings.
-- All columns added with DEFAULT so existing row is preserved.

ALTER TABLE "clinic_settings"
  -- WA delivery & retry
  ADD COLUMN IF NOT EXISTS "wa_retry_count"            INTEGER      NOT NULL DEFAULT 3,
  ADD COLUMN IF NOT EXISTS "wa_retry_delay_minutes"    INTEGER      NOT NULL DEFAULT 5,
  ADD COLUMN IF NOT EXISTS "wa_send_window_start"      TEXT         NOT NULL DEFAULT '07:00',
  ADD COLUMN IF NOT EXISTS "wa_send_window_end"        TEXT         NOT NULL DEFAULT '21:00',
  ADD COLUMN IF NOT EXISTS "notif_failed_send_email"   BOOLEAN      NOT NULL DEFAULT TRUE,
  -- Email
  ADD COLUMN IF NOT EXISTS "email_invoice_after_payment" BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "email_weekly_recap"          BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "email_monthly_psikolog"      BOOLEAN    NOT NULL DEFAULT FALSE,
  -- Pengingat sesi otomatis
  ADD COLUMN IF NOT EXISTS "notif_confirm_klien"         BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_confirm_psikolog"      BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_h1_klien"              BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_h1_send_time"          TEXT       NOT NULL DEFAULT '08:00',
  ADD COLUMN IF NOT EXISTS "notif_m30_klien"             BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_followup_klien"        BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_followup_delay_hours"  INTEGER    NOT NULL DEFAULT 3,
  ADD COLUMN IF NOT EXISTS "notif_feedback_klien"        BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_feedback_send_time"    TEXT       NOT NULL DEFAULT '08:00',
  ADD COLUMN IF NOT EXISTS "notif_sesi_lanjutan_klien"   BOOLEAN    NOT NULL DEFAULT FALSE,
  ADD COLUMN IF NOT EXISTS "notif_sesi_lanjutan_days"    INTEGER    NOT NULL DEFAULT 7,
  ADD COLUMN IF NOT EXISTS "notif_paket_habis_klien"     BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_minggu_kosong_psikolog"      BOOLEAN NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_minggu_kosong_days_before"   INTEGER NOT NULL DEFAULT 3,
  ADD COLUMN IF NOT EXISTS "notif_minggu_kosong_threshold"     INTEGER NOT NULL DEFAULT 50,
  -- Perubahan jadwal
  ADD COLUMN IF NOT EXISTS "notif_reschedule_klien"      BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_reschedule_psikolog"   BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_cancel_klien"          BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_cancel_psikolog"       BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_ubah_ruangan_klien"    BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_ubah_ruangan_psikolog" BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_ubah_layanan_klien"    BOOLEAN    NOT NULL DEFAULT FALSE,
  ADD COLUMN IF NOT EXISTS "notif_ubah_layanan_psikolog" BOOLEAN    NOT NULL DEFAULT FALSE,
  -- Onboarding & pembayaran
  ADD COLUMN IF NOT EXISTS "notif_welcome_klien"         BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_welcome_psikolog"      BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_invite_staff"          BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_otp_user"              BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_dp_klien"              BOOLEAN    NOT NULL DEFAULT TRUE,
  ADD COLUMN IF NOT EXISTS "notif_bukti_pembayaran_klien" BOOLEAN   NOT NULL DEFAULT FALSE,
  ADD COLUMN IF NOT EXISTS "notif_pelunasan_klien"       BOOLEAN    NOT NULL DEFAULT TRUE;
