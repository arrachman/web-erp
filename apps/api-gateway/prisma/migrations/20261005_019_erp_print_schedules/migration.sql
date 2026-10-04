-- Fase 2 P3 — Penjadwalan produksi: master mesin cetak + jadwal job per mesin.
-- Dua mesin contoh bertanda provisional (CLAUDE.md §6 + asumsi rencana §2 #5),
-- diganti data asli klien bila sudah tersedia.

CREATE TYPE "ErpMachineType" AS ENUM ('OFFSET', 'DIGITAL', 'FINISHING', 'LAIN');
CREATE TYPE "ErpMachineStatus" AS ENUM ('ACTIVE', 'MAINTENANCE', 'INACTIVE');
CREATE TYPE "ErpJobScheduleStatus" AS ENUM ('TERJADWAL', 'BERJALAN', 'SELESAI', 'BATAL');

CREATE TABLE "mfg_machines" (
  "id" BIGSERIAL PRIMARY KEY,
  "code" TEXT NOT NULL UNIQUE,
  "name" TEXT NOT NULL,
  "machine_type" "ErpMachineType" NOT NULL,
  "capacity_per_hour" DECIMAL(19,4) NOT NULL DEFAULT 0,
  "capacity_unit" TEXT,
  "work_start" TEXT NOT NULL DEFAULT '08:00',
  "work_end" TEXT NOT NULL DEFAULT '17:00',
  "branch_id" BIGINT,
  "status" "ErpMachineStatus" NOT NULL DEFAULT 'ACTIVE',
  "legacy_code" TEXT,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);

CREATE TABLE "mfg_job_schedules" (
  "id" BIGSERIAL PRIMARY KEY,
  "job_id" BIGINT NOT NULL REFERENCES "mfg_print_jobs" ("id"),
  "machine_id" BIGINT NOT NULL REFERENCES "mfg_machines" ("id"),
  "stage" TEXT,
  "planned_start" TIMESTAMPTZ NOT NULL,
  "planned_end" TIMESTAMPTZ NOT NULL,
  "actual_start" TIMESTAMPTZ,
  "actual_end" TIMESTAMPTZ,
  "status" "ErpJobScheduleStatus" NOT NULL DEFAULT 'TERJADWAL',
  "sequence_no" INTEGER NOT NULL DEFAULT 0,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE INDEX "mfg_job_schedules_machine_time_idx" ON "mfg_job_schedules" ("machine_id", "planned_start");
CREATE INDEX "mfg_job_schedules_job_idx" ON "mfg_job_schedules" ("job_id");

-- Mesin contoh provisional (menunggu daftar mesin asli dari klien).
INSERT INTO "mfg_machines" ("code", "name", "machine_type", "capacity_per_hour", "capacity_unit", "legacy_code")
VALUES
  ('MCN-OFFSET-01', '[PROVISIONAL] Mesin Offset 4 Warna', 'OFFSET', 8000, 'lembar/jam', 'provisional'),
  ('MCN-DIGITAL-01', '[PROVISIONAL] Mesin Digital Printing', 'DIGITAL', 1200, 'lembar/jam', 'provisional')
ON CONFLICT ("code") DO NOTHING;

-- Menu Jadwal Produksi di bawah M6.TX (meniru baris Work Order).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M6.TX.SCHED', 'Jadwal Produksi', '/manufacturing/schedules', "icon", "type", "parent_id", 6, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M6.TX.WO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M6.TX.SCHED');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.SCHED'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.WO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M6.TX.SCHED')
  );
