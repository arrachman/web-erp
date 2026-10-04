-- D2/E1 menus: Rabat Supplier (under the purchasing transaction group,
-- sibling of Purchase Invoice) and Analisis Laba (under the finance
-- reports group, sibling of AR Aging). Role grants cloned from siblings.
INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.TX.REBATES', 'Rabat Supplier', '/purchasing/rebates', 'ITEM', 1462, 11, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.TX.REBATES');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'FIN-PROFIT', 'Analisis Laba', '/finance/profit-analysis', 'ITEM', 367, 11, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'FIN-PROFIT');

INSERT INTO "adm_role_menus" ("menu_id", "role_id")
SELECT m."id", g."role_id"
FROM "sys_menus" m
JOIN "adm_role_menus" g ON g."menu_id" = 1469
WHERE m."code" = 'M4.TX.REBATES'
ON CONFLICT DO NOTHING;

INSERT INTO "adm_role_menus" ("menu_id", "role_id")
SELECT m."id", g."role_id"
FROM "sys_menus" m
JOIN "adm_role_menus" g ON g."menu_id" = 3652
WHERE m."code" = 'FIN-PROFIT'
ON CONFLICT DO NOTHING;
