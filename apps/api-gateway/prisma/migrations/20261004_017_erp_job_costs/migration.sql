-- Fase 2 P5 — HPP per job: entri biaya aktual job cetak + penanda posting
-- jurnal penyelesaian WIP -> Barang Jadi di mfg_print_jobs + menu HPP Job.

CREATE TYPE "ErpJobCostType" AS ENUM ('MATERIAL', 'TENAGA_KERJA', 'OVERHEAD', 'MAKLOON', 'LAIN');

CREATE TABLE "mfg_job_cost_entries" (
  "id" BIGSERIAL PRIMARY KEY,
  "job_id" BIGINT NOT NULL REFERENCES "mfg_print_jobs" ("id"),
  "entry_date" DATE NOT NULL,
  "cost_type" "ErpJobCostType" NOT NULL,
  "stage" TEXT,
  "description" TEXT,
  "item_id" BIGINT,
  "quantity" DECIMAL(19,4) NOT NULL DEFAULT 1,
  "unit_id" BIGINT,
  "unit_cost" DECIMAL(19,4) NOT NULL,
  "amount" DECIMAL(19,4) NOT NULL,
  "source_type" TEXT NOT NULL DEFAULT 'MANUAL',
  "source_id" BIGINT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE INDEX "mfg_job_cost_entries_job_idx" ON "mfg_job_cost_entries" ("job_id");
CREATE INDEX "mfg_job_cost_entries_type_idx" ON "mfg_job_cost_entries" ("cost_type");

ALTER TABLE "mfg_print_jobs" ADD COLUMN "cost_posted_at" TIMESTAMPTZ;
ALTER TABLE "mfg_print_jobs" ADD COLUMN "cost_journal_doc" TEXT;

-- Menu HPP Job di bawah M6.TX (meniru baris Work Order).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M6.TX.JOBCOST', 'HPP Job', '/manufacturing/job-costs', "icon", "type", "parent_id", 5, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M6.TX.WO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M6.TX.JOBCOST');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.JOBCOST'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.WO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.JOBCOST')
  );
