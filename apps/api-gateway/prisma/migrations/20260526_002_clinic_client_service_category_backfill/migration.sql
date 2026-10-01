-- Fallback backfill untuk klien yang `preferred_service_type`-nya bukan
-- `clinic_service.name` melainkan **enum kategori** lowercase ('konseling',
-- 'terapi', 'tes'). Itu pola lama dari seed dev sebelum keputusan 26 Mei 2026
-- ("Layanan = dropdown dari catalog"). Untuk klien-klien tsb, link ke
-- service pertama (urut by id asc) di kategori yang match.
--
-- Idempotent: ON CONFLICT DO NOTHING. Aman dijalankan ulang.
-- Skip klien yang sudah punya entry di clinic_client_service.

INSERT INTO "clinic_client_service" ("client_id", "service_id")
SELECT c.id, s.id
FROM "clinic_client" c
JOIN LATERAL (
    SELECT id
    FROM "clinic_service"
    WHERE "category" = LOWER(c."preferred_service_type")
      AND "deleted_at" IS NULL
    ORDER BY id ASC
    LIMIT 1
) s ON TRUE
WHERE c."preferred_service_type" IS NOT NULL
  AND LOWER(c."preferred_service_type") IN ('konseling', 'terapi', 'tes')
  AND c."deleted_at" IS NULL
  AND NOT EXISTS (
      SELECT 1 FROM "clinic_client_service" ccs WHERE ccs."client_id" = c.id
  )
ON CONFLICT ("client_id", "service_id") DO NOTHING;

-- Sync preferred_service_type ke nama service hasil link, supaya kolom legacy
-- juga konsisten (UI yang masih baca kolom ini tidak lagi tampilkan
-- "konseling"/"terapi"/"tes" generic).
UPDATE "clinic_client" c
SET "preferred_service_type" = s.name
FROM "clinic_client_service" ccs
JOIN "clinic_service" s ON s.id = ccs.service_id
WHERE ccs.client_id = c.id
  AND LOWER(c."preferred_service_type") IN ('konseling', 'terapi', 'tes')
  AND c."deleted_at" IS NULL;
