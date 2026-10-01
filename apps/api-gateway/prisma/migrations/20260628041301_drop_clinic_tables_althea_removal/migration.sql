-- Althea Psychology removal: drop all clinic_* tables.
-- Backup taken pre-removal: backups/clinic_tables_pre_removal_*.sql
-- Verified: no non-clinic table references these; CASCADE only removes
-- inter-clinic FKs and clinic->m0_users FK constraints (m0_users untouched).
DROP TABLE IF EXISTS
  "clinic_payment",
  "clinic_session_note",
  "clinic_booking",
  "clinic_client_service",
  "clinic_psikolog_service",
  "clinic_psikolog_date_override",
  "clinic_psikolog_profile",
  "clinic_wa_log",
  "clinic_wa_template",
  "clinic_room",
  "clinic_service",
  "clinic_client",
  "clinic_settings",
  "clinic_idempotency_key"
CASCADE;
