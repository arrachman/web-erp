-- A4 Pajak pengadaan: PPH_22 in the tax entry type enum + Pajak Pengadaan menu.
--
-- fin_tax_entries / fin_withholding_tax_certificates already exist (unused
-- until now); this migration only adds the missing PPH_22 enum value used
-- for bendahara withholding on BOS purchases, and the Finance menu.

ALTER TYPE "ErpTaxEntryType" ADD VALUE IF NOT EXISTS 'PPH_22';

-- Menu: Finance & Accounting -> Transactions -> Pajak Pengadaan.
INSERT INTO "sys_menus"
  ("code", "title", "path", "icon", "type", "parent_id", "sort_order",
   "is_active", "created_at", "updated_at")
SELECT 'TAX-SUBLEDGER', 'Pajak Pengadaan', '/finance/tax-subledger', NULL, 'ITEM',
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'FIN.TX'),
       17, true, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'TAX-SUBLEDGER');

-- Role grants: copy every grant on the Cash Receipt menu (same FIN.TX group).
INSERT INTO "adm_role_menus"
  ("role_id", "menu_id", "can_view", "can_create", "can_edit", "can_delete",
   "can_approve", "can_print", "can_export", "can_import", "is_favorite")
SELECT rm."role_id",
       (SELECT "id" FROM "sys_menus" WHERE "code" = 'TAX-SUBLEDGER'),
       rm."can_view", rm."can_create", rm."can_edit", rm."can_delete",
       rm."can_approve", rm."can_print", rm."can_export", rm."can_import",
       rm."is_favorite"
FROM "adm_role_menus" rm
WHERE rm."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'CASH-RECEIPT')
  AND NOT EXISTS (
    SELECT 1 FROM "adm_role_menus" x
    WHERE x."role_id" = rm."role_id"
      AND x."menu_id" = (SELECT "id" FROM "sys_menus" WHERE "code" = 'TAX-SUBLEDGER')
  );
