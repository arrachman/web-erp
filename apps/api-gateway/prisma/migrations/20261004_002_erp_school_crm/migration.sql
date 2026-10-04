-- A1 — CRM Sekolah (Bahtera Madani MVP-1)
--
-- A school is a *customer partner* (`md_partners`) that also carries a 1:1
-- school profile (`md_school_profiles`). The partner row keeps every existing
-- sales/AR behaviour untouched; the profile holds the vertical attributes the
-- PRD asks for (NPSN, jenjang, BOS pagu, student headcount, BOS period flags).
--
-- `md_partner_types.code = 'SCHOOL'` is a GENERAL-kind partner type, so a school
-- is a normal customer for sales/AR but is also selectable as a CRM filter.
-- No new enum, no Yayasan table (deferred to Fase 3/B1, see
-- docs/bahtera-madani-mvp-scope.md §1/§7).
--
-- `md_partner_types` already exists (migration 20260711_002_erp_partner_types),
-- so this migration only inserts the SCHOOL row idempotently.

-- School partner type: GENERAL kind (not a sales role), but a customer for
-- transactional purposes. Inserted idempotently so re-running the migration
-- on an already-seeded DB is a no-op for this row.
INSERT INTO "md_partner_types" ("code", "name", "kind", "is_active", "created_at", "updated_at")
VALUES ('SCHOOL', 'Sekolah', 'GENERAL', true, NOW(), NOW())
ON CONFLICT ("code") DO NOTHING;

-- A1 school CRM enums. CREATE TYPE IF NOT EXISTS keeps the migration
-- re-runnable; the values must match enums.prisma (SSOT).
CREATE TYPE "ErpSchoolJenjang" AS ENUM ('PAUD', 'TK', 'SD', 'SMP', 'SMA', 'SMK', 'SLB', 'OTHER');
CREATE TYPE "ErpSchoolNegeriSwasta" AS ENUM ('NEGERI', 'SWASTA');
CREATE TYPE "ErpSchoolBosStage" AS ENUM ('TAHAP_1', 'TAHAP_2');

-- 1:1 school profile. `partner_id` is unique: one school = one profile, and
-- the profile is the single place school-specific attributes live.
CREATE TABLE IF NOT EXISTS "md_school_profiles" (
    "id"                  BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "partner_id"          BIGINT      NOT NULL,
    "npsn"                VARCHAR(20),
    "jenjang"             "ErpSchoolJenjang",
    "negeri_swasta"       "ErpSchoolNegeriSwasta",
    "accreditation"       VARCHAR(20),
    "student_count"       INTEGER,
    "class_count"         INTEGER,
    "bos_pagu"            BIGINT,
    "bos_realisasi"       BIGINT,
    "bos_period_label"    VARCHAR(40),
    "bos_period_stage"    "ErpSchoolBosStage",
    "bos_period_year"     INTEGER,
    "last_visit_at"       TIMESTAMPTZ,
    "visit_notes"         TEXT,
    "negotiation_notes"   TEXT,
    "contract_expiry_at"  TIMESTAMPTZ,
    "is_active"           BOOLEAN     NOT NULL DEFAULT true,
    "metadata"            JSONB,
    "created_at"          TIMESTAMPTZ NOT NULL DEFAULT now(),
    "updated_at"          TIMESTAMPTZ NOT NULL DEFAULT now(),
    "created_by_id"       BIGINT,
    "updated_by_id"       BIGINT,
    "deleted_at"          TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS "md_school_profiles_partner_id_key"
    ON "md_school_profiles"("partner_id");
CREATE INDEX IF NOT EXISTS "md_school_profiles_npsn_idx"
    ON "md_school_profiles"("npsn");
CREATE INDEX IF NOT EXISTS "md_school_profiles_jenjang_idx"
    ON "md_school_profiles"("jenjang");
CREATE INDEX IF NOT EXISTS "md_school_profiles_contract_expiry_idx"
    ON "md_school_profiles"("contract_expiry_at");

-- FK: profile -> partner (RESTRICT: a school profile cannot outlive its partner)
ALTER TABLE "md_school_profiles"
    ADD CONSTRAINT "md_school_profiles_partner_id_fkey"
    FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id")
    ON DELETE RESTRICT ON UPDATE CASCADE;