-- Fase 2 P2 — Job cetak & pre-press: profil cetak di atas Work Order generik
-- (tahap PRE_PRESS -> CETAK -> FINISHING -> QC -> SELESAI + checklist pre-press).

CREATE TYPE "ErpPrintJobStage" AS ENUM ('PRE_PRESS', 'CETAK', 'FINISHING', 'QC', 'SELESAI', 'CANCELLED');

CREATE TABLE "mfg_print_jobs" (
  "id" BIGSERIAL PRIMARY KEY,
  "work_order_id" BIGINT NOT NULL,
  "estimate_id" BIGINT,
  "sales_order_id" BIGINT,
  "stage" "ErpPrintJobStage" NOT NULL DEFAULT 'PRE_PRESS',
  "paper_size" TEXT,
  "page_count" INTEGER,
  "print_quantity" DECIMAL(19,4) NOT NULL,
  "color_spec" TEXT,
  "finishing" TEXT,
  "master_file_name" TEXT,
  "checklist" JSONB NOT NULL DEFAULT '[]',
  "stage_log" JSONB NOT NULL DEFAULT '[]',
  "notes" TEXT,
  "legacy_code" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE UNIQUE INDEX "mfg_print_jobs_work_order_key" ON "mfg_print_jobs" ("work_order_id");
CREATE INDEX "mfg_print_jobs_stage_idx" ON "mfg_print_jobs" ("stage");
CREATE INDEX "mfg_print_jobs_estimate_idx" ON "mfg_print_jobs" ("estimate_id");

-- Menu Job Cetak di bawah M6.TX (meniru baris Work Order).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M6.TX.JOB', 'Job Cetak', '/manufacturing/print-jobs', "icon", "type", "parent_id", 4, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M6.TX.WO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M6.TX.JOB');

-- Role grants: salin dari menu Work Order.
INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.JOB'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.WO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.JOB')
  );
