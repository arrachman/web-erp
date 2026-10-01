-- Domain: m
-- Purpose: time-series trend
-- Suggested source table: m_10_ad

SELECT
  DATE(__DATE_EXPR__) AS period_date,
  COUNT(*) AS total_rows,
  SUM(COALESCE(`adtotalpotongan`, 0)) AS total_metric
FROM `m_10_ad`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date
GROUP BY period_date
ORDER BY period_date ASC;
