-- MVP navigation scope (Admin): re-enable the Appearance menu
-- (`M0.SYS.APPEARANCE`, /settings/appearance) at the owner's request
-- on 2026-10-04. It was deactivated by migration
-- `20261004_004_erp_mvp_hide_non_mvp_admin_menus`; only `is_active`
-- flips back, which the `getMyMenus` tree builder filters on.
-- See docs/bahtera-madani-mvp-scope.md § "Navigasi deployment MVP".

UPDATE "sys_menus"
SET "is_active" = true,
    "updated_at" = NOW()
WHERE "code" IN (
    'M0.SYS.APPEARANCE'
  )
  AND "deleted_at" IS NULL
  AND "is_active" = false;
