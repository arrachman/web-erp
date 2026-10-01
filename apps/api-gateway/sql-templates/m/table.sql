-- Domain: m
-- Purpose: table detail for dashboard
-- Suggested source table: m_10_ad

SELECT
  *
FROM `m_10_ad`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date
ORDER BY `__ORDER_BY__` __ORDER_DIR__
LIMIT :limit OFFSET :offset;
