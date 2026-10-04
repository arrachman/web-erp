-- Fase 2 P6 — Packing per siswa: unit kemas per siswa/kelas di atas Packing
-- List yang sudah ada + menu Packing per Siswa.

CREATE TYPE "ErpPackingUnitStatus" AS ENUM ('PENDING', 'PACKED');

CREATE TABLE "sls_packing_units" (
  "id" BIGSERIAL PRIMARY KEY,
  "packing_list_id" BIGINT NOT NULL REFERENCES "sls_packing_lists" ("id"),
  "sequence_no" INTEGER NOT NULL,
  "student_name" TEXT NOT NULL,
  "class_name" TEXT,
  "contents" JSONB NOT NULL DEFAULT '[]',
  "status" "ErpPackingUnitStatus" NOT NULL DEFAULT 'PENDING',
  "packed_at" TIMESTAMPTZ,
  "packed_by_id" BIGINT,
  "notes" TEXT,
  "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
  "deleted_at" TIMESTAMPTZ,
  "created_by_id" BIGINT,
  "updated_by_id" BIGINT
);
CREATE INDEX "sls_packing_units_list_idx" ON "sls_packing_units" ("packing_list_id");
CREATE INDEX "sls_packing_units_status_idx" ON "sls_packing_units" ("status");

-- Menu Packing per Siswa di bawah M5.TX (meniru baris Packing List).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.TX.PACKUNITS', 'Packing per Siswa', '/sales/packing-units', "icon", "type", "parent_id", "sort_order" + 1, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M5.TX.PL'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.TX.PACKUNITS');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.PACKUNITS'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.PL')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.PACKUNITS')
  );
