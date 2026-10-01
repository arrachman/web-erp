-- Add isActive flag to ClinicClient. Aligns with ClinicPsikologProfile / ClinicService / ClinicRoom.
-- Soft-delete blocked when bookings exist; admin uses isActive=false to retire a client safely.
ALTER TABLE "clinic_client"
  ADD COLUMN "is_active" BOOLEAN NOT NULL DEFAULT true;

CREATE INDEX "clinic_client_is_active_idx" ON "clinic_client"("is_active");
