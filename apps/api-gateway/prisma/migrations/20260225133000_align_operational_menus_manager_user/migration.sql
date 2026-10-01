-- Align manager/user menu access to operational scope only:
-- Dashboard + active Master Data + active Logistic menus.

WITH role_targets AS (
  SELECT "id" AS role_id
  FROM "m0_role"
  WHERE "name" IN ('manager', 'user')
    AND "deleted_at" IS NULL
),
parent_targets AS (
  SELECT "id", "key"
  FROM "m0_menu"
  WHERE "key" IN ('master-data', 'logistic')
    AND "deleted_at" IS NULL
),
menu_targets AS (
  SELECT m."id" AS menu_id
  FROM "m0_menu" m
  WHERE m."deleted_at" IS NULL
    AND (
      m."key" LIKE 'dashboard%'
      OR m."key" IN ('master-data', 'logistic')
      OR (
        m."parent_id" IN (SELECT "id" FROM parent_targets)
        AND m."is_visible" = TRUE
        AND m."is_active" = TRUE
      )
    )
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT r.role_id, t.menu_id, TRUE
FROM role_targets r
CROSS JOIN menu_targets t
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();

WITH role_targets AS (
  SELECT "id" AS role_id
  FROM "m0_role"
  WHERE "name" IN ('manager', 'user')
    AND "deleted_at" IS NULL
),
parent_targets AS (
  SELECT "id"
  FROM "m0_menu"
  WHERE "key" IN ('master-data', 'logistic')
    AND "deleted_at" IS NULL
),
menu_targets AS (
  SELECT m."id" AS menu_id
  FROM "m0_menu" m
  WHERE m."deleted_at" IS NULL
    AND (
      m."key" LIKE 'dashboard%'
      OR m."key" IN ('master-data', 'logistic')
      OR (
        m."parent_id" IN (SELECT "id" FROM parent_targets)
        AND m."is_visible" = TRUE
        AND m."is_active" = TRUE
      )
    )
)
UPDATE "m0_role_menu" rm
SET
  "can_view" = FALSE,
  "deleted_at" = NOW(),
  "deleted_by" = NULL,
  "updated_at" = NOW()
FROM role_targets r
WHERE rm."role_id" = r.role_id
  AND rm."deleted_at" IS NULL
  AND rm."menu_id" NOT IN (SELECT menu_id FROM menu_targets);
