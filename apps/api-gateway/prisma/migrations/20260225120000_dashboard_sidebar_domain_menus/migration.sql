-- Configure dashboard sidebar as parent + domain children.

INSERT INTO "m0_menu" (
  "key", "title", "path", "icon", "type", "parent_id", "sort_order", "is_visible", "is_active"
)
VALUES (
  'dashboard', 'Dashboard', NULL, 'LayoutGrid', 'COLLAPSE', NULL, 1, TRUE, TRUE
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
  'dashboard-overview',
  'Overview',
  '/app/overview',
  'LayoutGrid',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'dashboard'),
  1,
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
  'dashboard-m1',
  'Dashboard M1',
  '/app/overview?domain=m1',
  'BarChart3',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'dashboard'),
  2,
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
  'dashboard-m',
  'Dashboard M',
  '/app/overview?domain=m',
  'LineChart',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'dashboard'),
  3,
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
  'dashboard-m2r',
  'Dashboard M2R',
  '/app/overview?domain=m2r',
  'Activity',
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

WITH dashboard_roles AS (
  SELECT DISTINCT rm."role_id"
  FROM "m0_role_menu" rm
  JOIN "m0_menu" m ON m."id" = rm."menu_id"
  WHERE m."key" = 'dashboard'
),
dashboard_menu_targets AS (
  SELECT "id"
  FROM "m0_menu"
  WHERE "key" IN ('dashboard', 'dashboard-overview', 'dashboard-m1', 'dashboard-m', 'dashboard-m2r')
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT r."role_id", t."id", TRUE
FROM dashboard_roles r
CROSS JOIN dashboard_menu_targets t
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();
