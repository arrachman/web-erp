-- A1 — CRM Sekolah completion (Bahtera Madani MVP-1)
--
-- Completes migration 20261004_002 (school profile) with the remaining A1
-- scope from docs/bahtera-madani-mvp-scope.md §3:
--   * pipeline stage on the school profile (prospek → … → lunas);
--   * students-per-grade breakdown (JSON, keyed by grade label);
--   * standardised contact role on md_partner_contacts
--     (kepala sekolah / bendahara / operator / TU);
--   * school activity log md_school_activities (kunjungan / negosiasi);
--   * "Sekolah" menu under Master Data → Partners (M1.PARTNER.SCHOOLS),
--     with role-menu grants copied from the Partners menu so every role
--     that can open Partners can also open Schools.
--
-- Yayasan stays out of scope (Fase 3/B1, scope doc §7). All statements are
-- idempotent so a re-run is a no-op.

-- ── Enums (must match enums.prisma, SSOT) ───────────────────────────────────
DO $$ BEGIN
  CREATE TYPE "ErpSchoolPipelineStage" AS ENUM ('PROSPEK', 'PENAWARAN', 'PESANAN', 'TERKIRIM', 'LUNAS');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
  CREATE TYPE "ErpSchoolActivityType" AS ENUM ('VISIT', 'NEGOTIATION', 'NOTE');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
  CREATE TYPE "ErpPartnerContactRole" AS ENUM ('KEPALA_SEKOLAH', 'BENDAHARA', 'OPERATOR', 'TU', 'OTHER');
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

-- ── School profile: pipeline stage + students per grade ────────────────────
ALTER TABLE "md_school_profiles"
  ADD COLUMN IF NOT EXISTS "students_per_grade" JSONB,
  ADD COLUMN IF NOT EXISTS "pipeline_stage" "ErpSchoolPipelineStage" NOT NULL DEFAULT 'PROSPEK';

CREATE INDEX IF NOT EXISTS "md_school_profiles_pipeline_stage_idx"
  ON "md_school_profiles"("pipeline_stage");

-- ── Partner contacts: standardised role ────────────────────────────────────
ALTER TABLE "md_partner_contacts"
  ADD COLUMN IF NOT EXISTS "role" "ErpPartnerContactRole";

-- ── School activity log ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "md_school_activities" (
    "id"            BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "partner_id"    BIGINT      NOT NULL,
    "contact_id"    BIGINT,
    "type"          "ErpSchoolActivityType" NOT NULL,
    "activity_at"   TIMESTAMPTZ NOT NULL,
    "notes"         TEXT        NOT NULL,
    "created_at"    TIMESTAMPTZ NOT NULL DEFAULT now(),
    "updated_at"    TIMESTAMPTZ NOT NULL DEFAULT now(),
    "created_by_id" BIGINT,
    "updated_by_id" BIGINT,
    "deleted_at"    TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS "md_school_activities_partner_id_idx"
  ON "md_school_activities"("partner_id");
CREATE INDEX IF NOT EXISTS "md_school_activities_activity_at_idx"
  ON "md_school_activities"("activity_at");

DO $$ BEGIN
  ALTER TABLE "md_school_activities"
    ADD CONSTRAINT "md_school_activities_partner_id_fkey"
    FOREIGN KEY ("partner_id") REFERENCES "md_partners"("id")
    ON DELETE RESTRICT ON UPDATE CASCADE;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

DO $$ BEGIN
  ALTER TABLE "md_school_activities"
    ADD CONSTRAINT "md_school_activities_contact_id_fkey"
    FOREIGN KEY ("contact_id") REFERENCES "md_partner_contacts"("id")
    ON DELETE SET NULL ON UPDATE CASCADE;
EXCEPTION WHEN duplicate_object THEN NULL; END $$;

-- ── Menu: Sekolah under Master Data → Partners ─────────────────────────────
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order",
   "is_active", "created_at", "updated_at")
SELECT 'M1.PARTNER.SCHOOLS', 'Sekolah', '/master/schools', NULL, 'ITEM',
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M1.PARTNER'),
       10, true, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M1.PARTNER.SCHOOLS');

-- Role grants: copy every grant on the Partners menu to the Schools menu.
INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M1.PARTNER.SCHOOLS'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M1.PARTNER.PARTNERS')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M1.PARTNER.SCHOOLS')
  );
