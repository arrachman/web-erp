-- ClinicPsikologDateOverride: per-date override yang mengganti weeklyAvailability
-- untuk tanggal spesifik (cuti, makeup session, dll).

CREATE TABLE "clinic_psikolog_date_override" (
    "id" SERIAL NOT NULL,
    "psikolog_user_id" INTEGER NOT NULL,
    "date" DATE NOT NULL,
    "is_open" BOOLEAN NOT NULL,
    "slot_indices" JSONB,
    "reason" TEXT,
    "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" INTEGER,
    "updated_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_by" INTEGER,

    CONSTRAINT "clinic_psikolog_date_override_pkey" PRIMARY KEY ("id")
);

CREATE UNIQUE INDEX "clinic_psikolog_date_override_psikolog_user_id_date_key"
  ON "clinic_psikolog_date_override"("psikolog_user_id", "date");

CREATE INDEX "clinic_psikolog_date_override_date_idx"
  ON "clinic_psikolog_date_override"("date");

ALTER TABLE "clinic_psikolog_date_override"
  ADD CONSTRAINT "clinic_psikolog_date_override_psikolog_user_id_fkey"
  FOREIGN KEY ("psikolog_user_id")
  REFERENCES "clinic_psikolog_profile"("user_id")
  ON DELETE CASCADE
  ON UPDATE CASCADE;
