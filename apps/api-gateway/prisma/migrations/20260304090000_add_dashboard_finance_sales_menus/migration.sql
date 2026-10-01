-- Add Dashboard domain menus for Finance & Accounting and Sales.
-- Idempotent upsert and role-menu grant for roles that can already view dashboard.

INSERT INTO "m0_menu" (
  "key", "title", "path", "icon", "type", "parent_id", "sort_order", "is_visible", "is_active"
)
VALUES (
  'dashboard-m2',
  'Finance & Accounting',
  '/app/dashboard/finance-accounting',
  'Wallet',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'dashboard'),
  4,
  TRUE,
  TRUE
)
ON CONFLICT ("key") DO UPDATE
SET
  "title" = EXCLUDED."title",
  "path" = EXCLUDED."path",
  "icon" = EXCLUDED."icon",
  "type" = EXCLUDED."type",
  "parent_id" = EXCLUDED."parent_id",
  "sort_order" = EXCLUDED."sort_order",
  "is_visible" = EXCLUDED."is_visible",
  "is_active" = EXCLUDED."is_active",
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();

INSERT INTO "m0_menu" (
  "key", "title", "path", "icon", "type", "parent_id", "sort_order", "is_visible", "is_active"
)
VALUES (
  'dashboard-so',
  'Sales',
  '/app/dashboard/sales',
  'TrendingUp',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'dashboard'),
  5,
  TRUE,
  TRUE
)
ON CONFLICT ("key") DO UPDATE
SET
  "title" = EXCLUDED."title",
  "path" = EXCLUDED."path",
  "icon" = EXCLUDED."icon",
  "type" = EXCLUDED."type",
  "parent_id" = EXCLUDED."parent_id",
  "sort_order" = EXCLUDED."sort_order",
  "is_visible" = EXCLUDED."is_visible",
  "is_active" = EXCLUDED."is_active",
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();

UPDATE "m0_menu"
SET
  "sort_order" = 6,
  "updated_at" = NOW()
WHERE "key" = 'dashboard-m2r'
  AND "sort_order" <> 6;

WITH dashboard_roles AS (
  SELECT DISTINCT rm."role_id"
  FROM "m0_role_menu" rm
  JOIN "m0_menu" m ON m."id" = rm."menu_id"
  WHERE m."key" = 'dashboard'
    AND rm."deleted_at" IS NULL
    AND rm."can_view" = TRUE
),
menu_targets AS (
  SELECT "id"
  FROM "m0_menu"
  WHERE "key" IN ('dashboard-m2', 'dashboard-so')
    AND "deleted_at" IS NULL
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT r."role_id", t."id", TRUE
FROM dashboard_roles r
CROSS JOIN menu_targets t
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();
