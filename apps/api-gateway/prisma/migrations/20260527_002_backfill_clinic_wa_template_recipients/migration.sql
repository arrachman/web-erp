-- Backfill clinic_wa_template.recipients dari ClinicSettings notif<event><recipient> flags.
--
-- Setelah migration ini + drop kolom (lihat 20260527_003), template.recipients menjadi
-- single source of truth untuk keputusan dispatch (siapa terima apa). Dispatcher
-- (booking-notification.service.ts + booking-reminder.scheduler.ts) hanya akan baca
-- recipients + waSendEnabled master switch.
--
-- Idempotent: re-run akan menulis ulang recipients berdasarkan nilai settings yang sama.

UPDATE clinic_wa_template t SET recipients = (
  CASE t.name
    WHEN 'Konfirmasi Booking' THEN
      ARRAY[]::text[]
      || CASE WHEN s.notif_confirm_klien    THEN ARRAY['klien']    ELSE ARRAY[]::text[] END
      || CASE WHEN s.notif_confirm_psikolog THEN ARRAY['psikolog'] ELSE ARRAY[]::text[] END
    WHEN 'Pengingat H-1 Booking' THEN
      CASE WHEN s.notif_h1_klien THEN ARRAY['klien'] ELSE ARRAY[]::text[] END
    WHEN 'Pengingat 30 Menit Sebelum Sesi' THEN
      CASE WHEN s.notif_m30_klien THEN ARRAY['klien'] ELSE ARRAY[]::text[] END
    WHEN 'Follow-up Post Session' THEN
      CASE WHEN s.notif_followup_klien THEN ARRAY['klien'] ELSE ARRAY[]::text[] END
    WHEN 'Form Feedback' THEN
      CASE WHEN s.notif_feedback_klien THEN ARRAY['klien'] ELSE ARRAY[]::text[] END
    WHEN 'Reschedule Booking' THEN
      ARRAY[]::text[]
      || CASE WHEN s.notif_reschedule_klien    THEN ARRAY['klien']    ELSE ARRAY[]::text[] END
      || CASE WHEN s.notif_reschedule_psikolog THEN ARRAY['psikolog'] ELSE ARRAY[]::text[] END
    WHEN 'Cancel Booking' THEN
      ARRAY[]::text[]
      || CASE WHEN s.notif_cancel_klien    THEN ARRAY['klien']    ELSE ARRAY[]::text[] END
      || CASE WHEN s.notif_cancel_psikolog THEN ARRAY['psikolog'] ELSE ARRAY[]::text[] END
    WHEN 'Welcome New Client' THEN
      CASE WHEN s.notif_welcome_klien THEN ARRAY['klien'] ELSE ARRAY[]::text[] END
    WHEN 'Welcome Psikolog Baru' THEN
      CASE WHEN s.notif_welcome_psikolog THEN ARRAY['psikolog'] ELSE ARRAY[]::text[] END
    WHEN 'OTP Login' THEN
      CASE WHEN s.notif_otp_user THEN ARRAY['user'] ELSE ARRAY[]::text[] END
    WHEN 'Bukti Pembayaran' THEN
      CASE WHEN s.notif_bukti_pembayaran_klien THEN ARRAY['klien'] ELSE ARRAY[]::text[] END
    ELSE t.recipients
  END
)
FROM clinic_settings s
WHERE s.id = 1
  AND t.name IN (
    'Konfirmasi Booking',
    'Pengingat H-1 Booking',
    'Pengingat 30 Menit Sebelum Sesi',
    'Follow-up Post Session',
    'Form Feedback',
    'Reschedule Booking',
    'Cancel Booking',
    'Welcome New Client',
    'Welcome Psikolog Baru',
    'OTP Login',
    'Bukti Pembayaran'
  );
