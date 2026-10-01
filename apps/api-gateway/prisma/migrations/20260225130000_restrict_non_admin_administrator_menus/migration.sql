-- Restrict administrator menus to admin role only.
-- Keep dashboard parent+domain menus available for manager/user.

UPDATE "m0_role_menu" rm
SET
  "can_view" = FALSE,
  "deleted_at" = NOW(),
  "deleted_by" = NULL,
  "updated_at" = NOW()
FROM "m0_role" r, "m0_menu" m
WHERE rm."role_id" = r."id"
  AND rm."menu_id" = m."id"
  AND r."name" IN ('manager', 'user')
  AND m."key" LIKE 'administrator%'
  AND rm."deleted_at" IS NULL;

WITH role_targets AS (
  SELECT "id" AS role_id
  FROM "m0_role"
  WHERE "name" IN ('manager', 'user')
    AND "deleted_at" IS NULL
),
menu_targets AS (
  SELECT "id" AS menu_id
  FROM "m0_menu"
  WHERE "key" IN ('dashboard', 'dashboard-overview', 'dashboard-m1', 'dashboard-m', 'dashboard-m2r')
    AND "deleted_at" IS NULL
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT r.role_id, m.menu_id, TRUE
FROM role_targets r
CROSS JOIN menu_targets m
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();
