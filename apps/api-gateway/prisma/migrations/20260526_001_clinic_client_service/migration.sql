-- Junction table klien ↔ service (multi-select layanan per klien).
-- Field `clinic_client.preferred_service_type` (single-name varchar) tetap ada untuk
-- backward compat & kode legacy. Backfill di bawah salin nilai existing ke junction.

CREATE TABLE "clinic_client_service" (
    "id"         SERIAL PRIMARY KEY,
    "client_id"  INTEGER NOT NULL,
    "service_id" INTEGER NOT NULL,
    "created_at" TIMESTAMPTZ(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "created_by" INTEGER
);

CREATE UNIQUE INDEX "clinic_client_service_client_id_service_id_key"
    ON "clinic_client_service"("client_id", "service_id");

CREATE INDEX "clinic_client_service_client_id_idx"
    ON "clinic_client_service"("client_id");

CREATE INDEX "clinic_client_service_service_id_idx"
    ON "clinic_client_service"("service_id");

ALTER TABLE "clinic_client_service"
    ADD CONSTRAINT "clinic_client_service_client_id_fkey"
    FOREIGN KEY ("client_id") REFERENCES "clinic_client"("id")
    ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE "clinic_client_service"
    ADD CONSTRAINT "clinic_client_service_service_id_fkey"
    FOREIGN KEY ("service_id") REFERENCES "clinic_service"("id")
    ON DELETE CASCADE ON UPDATE CASCADE;

-- Backfill: untuk tiap klien yg punya preferred_service_type non-null & non-empty,
-- match by name ke clinic_service (aktif maupun nonaktif, biar data lama tetap masuk),
-- insert 1 row ke junction. Klien tanpa nilai dilewati. ON CONFLICT DO NOTHING
-- mengamankan jalan ulang/idempotency.
INSERT INTO "clinic_client_service" ("client_id", "service_id")
SELECT c.id, s.id
FROM "clinic_client" c
JOIN "clinic_service" s ON s.name = c."preferred_service_type"
WHERE c."preferred_service_type" IS NOT NULL
  AND c."preferred_service_type" <> ''
  AND c."deleted_at" IS NULL
ON CONFLICT ("client_id", "service_id") DO NOTHING;
