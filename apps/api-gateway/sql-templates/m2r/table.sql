-- Domain: m2r
-- Purpose: table detail for dashboard
-- Suggested source table: m2r_anggaran

SELECT
  *
FROM `m2r_anggaran`
WHERE 1=1
ORDER BY `__ORDER_BY__` __ORDER_DIR__
LIMIT :limit OFFSET :offset;
