-- Fase 2 T1 — Lot/Batch & expiry tracking (implementation-plan Phase 2).
-- Model ErpInvLot + lot_id di movement/opening/count/adjustment lines SUDAH
-- ada sejak skema dasar; migrasi ini menambah: metadata lot di baris GRN
-- (agar penerimaan membentuk lot), FK lot di baris DO (picking), dan menu.

ALTER TABLE "pur_goods_receipt_lines"
  ADD COLUMN IF NOT EXISTS "lot_number" TEXT,
  ADD COLUMN IF NOT EXISTS "supplier_lot_no" TEXT,
  ADD COLUMN IF NOT EXISTS "manufacture_date" DATE,
  ADD COLUMN IF NOT EXISTS "expiry_date" DATE;

ALTER TABLE "sls_delivery_order_lines"
  ADD COLUMN IF NOT EXISTS "lot_id" BIGINT REFERENCES "inv_lots" ("id");
CREATE INDEX IF NOT EXISTS "sls_delivery_order_lines_lot_idx"
  ON "sls_delivery_order_lines" ("lot_id");

-- Menu Lot & Batch di grup transaksi Warehouse (meniru baris Stock Count).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M3.TX.LOT', 'Lot & Batch', '/warehouse/lots', "icon", "type", "parent_id", "sort_order" + 1, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M3.TX.RW'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M3.TX.LOT');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M3.TX.LOT'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M3.TX.SP')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M3.TX.LOT')
  );
