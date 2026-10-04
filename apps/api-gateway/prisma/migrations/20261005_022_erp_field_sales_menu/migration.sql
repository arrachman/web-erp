-- Fase 2 P8 — menu Sales Lapangan (PWA di frontend yang sama; tanpa tabel baru).
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.TX.FIELD', 'Sales Lapangan', '/sales/field', "icon", "type", "parent_id", "sort_order" + 1, true, now(), now()
FROM "sys_menus" WHERE "code" = 'M5.TX.SO'
  AND NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.TX.FIELD');

INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.FIELD'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.SO')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'M5.TX.FIELD')
  );
