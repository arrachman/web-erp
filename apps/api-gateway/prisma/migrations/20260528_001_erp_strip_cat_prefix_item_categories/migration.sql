-- Strip redundant 'CAT-' prefix from md_item_categories.code.
-- Entity scope (item category) already implied by table — prefix added noise.
-- Safe because: (a) code @unique constraint will not collide (every stripped
-- code matches the existing legacy_code which is already free in the column),
-- (b) all FKs to md_item_categories use id (BigInt), not code string.
UPDATE md_item_categories
SET code = SUBSTRING(code FROM 5)
WHERE code LIKE 'CAT-%';
