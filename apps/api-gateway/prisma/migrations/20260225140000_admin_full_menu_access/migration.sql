-- Enforce full menu access for admin role.
-- Idempotent: upserts all existing menus to m0_role_menu for admin.

WITH admin_role AS (
  SELECT "id" AS role_id
  FROM "m0_role"
  WHERE "name" = 'admin'
    AND "deleted_at" IS NULL
  LIMIT 1
),
menu_targets AS (
  SELECT "id" AS menu_id
  FROM "m0_menu"
  WHERE "deleted_at" IS NULL
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT a.role_id, m.menu_id, TRUE
FROM admin_role a
CROSS JOIN menu_targets m
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();
