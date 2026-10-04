-- G3 Finance document reports: reactivate the existing Cash Receipt report item
-- only after its backend endpoint and frontend renderer exist. Finance's report
-- catalog is the discovery surface for the remaining key-driven document reports.
-- GROUP paths remain NULL by invariant.

UPDATE "sys_menus"
SET "path" = '/finance/reports/cash-receipts',
    "is_active" = true,
    "updated_at" = CURRENT_TIMESTAMP
WHERE "id" = 7437
  AND "code" = 'FIN.RPT.CASH-RECEIPT';

-- Clone all parent-group roles and name the permission booleans explicitly:
-- adm_role_menus defaults every permission to false.
INSERT INTO "adm_role_menus" ("menu_id", "role_id", "can_view", "can_print", "can_export")
SELECT child."id", parentGrant."role_id", true, true, true
FROM "sys_menus" child
JOIN "adm_role_menus" parentGrant ON parentGrant."menu_id" = child."parent_id"
WHERE child."code" = 'FIN.RPT.CASH-RECEIPT'
ON CONFLICT DO NOTHING;

UPDATE "adm_role_menus" roleMenu
SET "can_view" = true,
    "can_print" = true,
    "can_export" = true
FROM "sys_menus" menu
WHERE menu."id" = roleMenu."menu_id"
  AND menu."code" = 'FIN.RPT.CASH-RECEIPT';
