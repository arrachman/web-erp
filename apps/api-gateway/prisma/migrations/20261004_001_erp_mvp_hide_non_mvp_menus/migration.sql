-- MVP navigation scope: hide modules outside MVP (Fase 2+) from the live ERP
-- sidebar. Production (M6), Fixed Assets (M7), and Point of Sale (M12) are
-- deactivated, not deleted, so they can be re-enabled when their phase starts.
-- Data, endpoints, and role mappings are untouched; only `is_active` flips,
-- which the `getMyMenus` tree builder already filters out.
-- See docs/bahtera-madani-mvp-scope.md § "Navigasi deployment MVP".

UPDATE "sys_menus"
SET "is_active" = false,
    "updated_at" = NOW()
WHERE "code" IN ('M6', 'M7', 'M12')
  AND "deleted_at" IS NULL
  AND "is_active" = true;