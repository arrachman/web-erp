-- Domain: m
-- Purpose: grouped breakdown chart
-- Suggested filter source: m_10_ab.abstatus

SELECT
  COALESCE(CAST(`__GROUP_BY__` AS CHAR), 'UNKNOWN') AS group_key,
  COUNT(*) AS total_rows,
  SUM(COALESCE(`abid`, 0)) AS total_metric
FROM `m_10_ab`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date
GROUP BY group_key
ORDER BY total_metric DESC, total_rows DESC;
