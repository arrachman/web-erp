-- Add Dashboard > Delivery menu.
-- Idempotent upsert and role-menu grant for roles that can already view dashboard.

INSERT INTO "m0_menu" (
  "key", "title", "path", "icon", "type", "parent_id", "sort_order", "is_visible", "is_active"
)
VALUES (
  'dashboard-delivery',
  'Delivery',
  '/app',
  'Truck',
  'ITEM',
  (SELECT "id" FROM "m0_menu" WHERE "key" = 'dashboard' AND "deleted_at" IS NULL),
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

WITH dashboard_roles AS (
  SELECT DISTINCT rm."role_id"
  FROM "m0_role_menu" rm
  JOIN "m0_menu" m ON m."id" = rm."menu_id"
  WHERE m."key" = 'dashboard'
    AND rm."deleted_at" IS NULL
    AND rm."can_view" = TRUE
),
menu_target AS (
  SELECT "id"
  FROM "m0_menu"
  WHERE "key" = 'dashboard-delivery'
    AND "deleted_at" IS NULL
)
INSERT INTO "m0_role_menu" ("role_id", "menu_id", "can_view")
SELECT r."role_id", t."id", TRUE
FROM dashboard_roles r
CROSS JOIN menu_target t
ON CONFLICT ("role_id", "menu_id") DO UPDATE
SET
  "can_view" = TRUE,
  "deleted_at" = NULL,
  "deleted_by" = NULL,
  "updated_at" = NOW();
