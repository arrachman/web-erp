-- Report hub menu entries for the four report modules (G1).
--
-- The "Reports" node of each module is a GROUP row whose `path` is NULL — an
-- invariant we keep (GROUPs are containers, never routes). So the hub route
-- gets its own ITEM row as the first child (`sort_order = 0`), pointing at the
-- catalog page that the frontend now renders at `/<module>/reports`.
--
-- Parent GROUPs: 367 FIN.RPT, 1254 M3.RPT, 1490 M4.RPT, 1538 M5.RPT.
--
-- NOTE on grants: every permission column in `adm_role_menus` is
-- `boolean NOT NULL DEFAULT false`, and `getMyMenus()` filters on
-- `canView = true`. An INSERT that omits the permission columns therefore
-- creates a row that hides the menu (the defect in migration
-- 20261004_012_erp_d2_e1_menus). All grants below name
-- can_view/can_print/can_export explicitly.

-- ── 1. Hub ITEM rows ────────────────────────────────────────────────────────
INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'FIN.RPT.HUB', 'Semua Laporan', '/finance/reports', 'ITEM', 367, 0, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'FIN.RPT.HUB');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M3.RPT.HUB', 'Semua Laporan', '/warehouse/reports', 'ITEM', 1254, 0, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M3.RPT.HUB');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M4.RPT.HUB', 'Semua Laporan', '/purchasing/reports', 'ITEM', 1490, 0, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M4.RPT.HUB');

INSERT INTO "sys_menus" ("code", "title", "path", "type", "parent_id", "sort_order", "is_active", "created_at", "updated_at")
SELECT 'M5.RPT.HUB', 'Semua Laporan', '/sales/reports', 'ITEM', 1538, 0, true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
WHERE NOT EXISTS (SELECT 1 FROM "sys_menus" WHERE "code" = 'M5.RPT.HUB');

-- ── 2. Grants: clone the roles that can already see the parent GROUP ─────────
INSERT INTO "adm_role_menus" ("menu_id", "role_id", "can_view", "can_print", "can_export")
SELECT m."id", g."role_id", true, true, true
FROM "sys_menus" m
JOIN "adm_role_menus" g ON g."menu_id" = m."parent_id"
WHERE m."code" IN ('FIN.RPT.HUB', 'M3.RPT.HUB', 'M4.RPT.HUB', 'M5.RPT.HUB')
ON CONFLICT DO NOTHING;

-- ── 3. Park the orphan finance report menu (id 7437) ────────────────────────
-- `/finance/reports/cash-receipt` is active but has ZERO role grants, so it
-- is invisible today — and its route has no renderer, because finance reports
-- do not use a `/finance/reports/<key>` prefix (the statements live on flat
-- paths like `/finance/ledger`). Granting it now would surface a menu that
-- lands on ComingSoon — the exact defect this migration exists to remove.
-- So: align the path to the plural convention and mark it inactive. G3
-- (`erp-fin-doc-reports`) builds the real cash-receipts document report and
-- re-activates this row together with an explicit grant.
UPDATE "sys_menus"
SET "path" = '/finance/reports/cash-receipts',
    "is_active" = false,
    "updated_at" = CURRENT_TIMESTAMP
WHERE "id" = 7437 AND "path" = '/finance/reports/cash-receipt';

-- ── 4. Repair grants created without permission columns ─────────────────────
-- Migration 20261004_012 inserted grants for 'M4.TX.REBATES' and 'FIN-PROFIT'
-- without naming the permission columns, so they defaulted to false and the
-- menus never appeared. Grant view/print/export on exactly those two rows.
UPDATE "adm_role_menus" rm
SET "can_view" = true, "can_print" = true, "can_export" = true
FROM "sys_menus" m
WHERE m."id" = rm."menu_id"
  AND m."code" IN ('M4.TX.REBATES', 'FIN-PROFIT')
  AND rm."can_view" = false;
