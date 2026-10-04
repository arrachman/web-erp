-- Fase 2 P4 — Variable Data Printing: dataset data variabel per job cetak +
-- baris-barisnya. Dataset membeku (TERKUNCI) saat job mulai tahap CETAK.
-- Data siswa asli hanya dari sekolah; untuk pengujian dipakai dummy bertanda.

CREATE TYPE "ErpVdpDatasetStatus" AS ENUM ('DRAFT', 'TERKUNCI');

CREATE TABLE "mfg_vdp_datasets" (
  "id" BIGSERIAL PRIMARY KEY,
  "job_id" BIGINT NOT NULL REFERENCES "mfg_print_jobs" ("id"),
  "name" TEXT NOT NULL,
  "source_filename" TEXT,
  "version" INTEGER NOT NULL DEFAULT 1,
  "columns" JSONB NOT NULL DEFAULT '[]',
  "required_columns" JSONB NOT NULL DEFAULT '[]',
  "row_count" INTEGER NOT NULL DEFAULT 0,
  "valid_count" INTEGER NOT NULL DEFAULT 0,
  "status" "ErpVdpDatasetStatus" NOT NULL DEFAULT 'DRAFT',
  "locked_at" TIMESTAMPTZ,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE INDEX "mfg_vdp_datasets_job_idx" ON "mfg_vdp_datasets" ("job_id");

CREATE TABLE "mfg_vdp_rows" (
  "id" BIGSERIAL PRIMARY KEY,
  "dataset_id" BIGINT NOT NULL REFERENCES "mfg_vdp_datasets" ("id") ON DELETE CASCADE,
  "row_no" INTEGER NOT NULL,
  "data" JSONB NOT NULL DEFAULT '{}',
  "is_valid" BOOLEAN NOT NULL DEFAULT true,
  "error_note" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX "mfg_vdp_rows_dataset_idx" ON "mfg_vdp_rows" ("dataset_id", "row_no");

-- Menu Data Variabel (VDP) di bawah M6.TX (meniru baris Work Order).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M6.TX.VDP', 'Data Variabel (VDP)', '/manufacturing/vdp', "icon", "type", "parent_id", 7, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M6.TX.WO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M6.TX.VDP');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.VDP'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.WO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.VDP')
  );
