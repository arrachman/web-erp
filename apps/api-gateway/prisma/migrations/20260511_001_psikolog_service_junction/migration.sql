-- Junction table psikolog ↔ service untuk filter booking wizard.
-- Default: kosong = handle semua service (backward compat untuk existing 7 psikolog).
-- Filled: hanya handle service yang di-list.

CREATE TABLE "clinic_psikolog_service" (
  "id" SERIAL PRIMARY KEY,
  "psikolog_user_id" INTEGER NOT NULL,
  "service_id" INTEGER NOT NULL,
  "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
  "created_by" INTEGER,

  CONSTRAINT "clinic_psikolog_service_psikolog_fk"
    FOREIGN KEY ("psikolog_user_id") REFERENCES "m0_users"("id") ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT "clinic_psikolog_service_service_fk"
    FOREIGN KEY ("service_id") REFERENCES "clinic_service"("id") ON DELETE CASCADE ON UPDATE CASCADE
);

CREATE UNIQUE INDEX "clinic_psikolog_service_psikolog_user_id_service_id_key"
  ON "clinic_psikolog_service"("psikolog_user_id", "service_id");

CREATE INDEX "clinic_psikolog_service_service_id_idx"
  ON "clinic_psikolog_service"("service_id");

CREATE INDEX "clinic_psikolog_service_psikolog_user_id_idx"
  ON "clinic_psikolog_service"("psikolog_user_id");
