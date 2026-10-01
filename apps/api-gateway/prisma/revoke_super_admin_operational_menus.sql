-- Revoke Master Data, Finance, and Logistic menus for the roles currently
-- assigned to super_admin@fr-labs.my.id.
--
-- Safe to rerun: it only flips can_view=false for matching role-menu rows.

WITH target_user AS (
    SELECT id
    FROM m0_users
    WHERE email = 'super_admin@fr-labs.my.id'
      AND deleted_at IS NULL
),
target_roles AS (
    SELECT DISTINCT ur.role_id
    FROM m0_user_role ur
    JOIN target_user tu ON tu.id = ur.user_id
    WHERE ur.deleted_at IS NULL
),
target_menu_roots AS (
    SELECT id
    FROM m0_menu
    WHERE key IN ('master-data', 'finance', 'logistic')
      AND deleted_at IS NULL
),
target_menus AS (
    WITH RECURSIVE menu_tree AS (
        SELECT id
        FROM target_menu_roots
        UNION ALL
        SELECT child.id
        FROM m0_menu child
        JOIN menu_tree parent ON child.parent_id = parent.id
        WHERE child.deleted_at IS NULL
    )
    SELECT DISTINCT id
    FROM menu_tree
)
UPDATE m0_role_menu rm
SET
    can_view = FALSE,
    updated_at = NOW()
WHERE rm.deleted_at IS NULL
  AND rm.role_id IN (SELECT role_id FROM target_roles)
  AND rm.menu_id IN (SELECT id FROM target_menus);

-- Verification query:
-- SELECT r.name AS role_name, m.key, m.title, rm.can_view
-- FROM m0_users u
-- JOIN m0_user_role ur ON ur.user_id = u.id AND ur.deleted_at IS NULL
-- JOIN m0_role r ON r.id = ur.role_id AND r.deleted_at IS NULL
-- JOIN m0_role_menu rm ON rm.role_id = r.id AND rm.deleted_at IS NULL
-- JOIN m0_menu m ON m.id = rm.menu_id AND m.deleted_at IS NULL
-- WHERE u.email = 'super_admin@fr-labs.my.id'
--   AND (m.key IN ('master-data', 'finance', 'logistic')
--        OR m.parent_id IN (
--             SELECT id FROM m0_menu WHERE key IN ('master-data', 'finance', 'logistic')
--           ))
-- ORDER BY r.name, m.sort_order, m.id;
