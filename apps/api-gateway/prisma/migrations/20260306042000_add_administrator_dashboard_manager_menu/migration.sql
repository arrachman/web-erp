-- Add Administrator > Senti AI menu and grant access to admin role only.
-- Idempotent: upsert menu by key and upsert role-menu relation.

INSERT INTO "m0_menu" (
  "key", "title", "path", "icon", "type", "parent_id", "sort_order", "is_visible", "is_active"
)
VALUES (
  'administrator-dashboard-manager',
  'Senti AI',
  '/app/senti-ai',
  'LayoutDashboard',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'administrator' AND "deleted_at" IS NULL),
  7,
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

WITH admin_role AS (
  SELECT "id" AS role_id
  FROM "m0_role"
  WHERE "name" = 'admin'
    AND "deleted_at" IS NULL
),
menu_target AS (
  SELECT "id" AS menu_id
  FROM "m0_menu"
  WHERE "key" = 'administrator-dashboard-manager'
    AND "deleted_at" IS NULL
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT a.role_id, m.menu_id, TRUE
FROM admin_role a
CROSS JOIN menu_target m
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();
