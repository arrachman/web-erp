-- Domain: m2r
-- Purpose: time-series trend
-- Suggested source table: m2r_ap_card

SELECT
  DATE(__DATE_EXPR__) AS period_date,
  COUNT(*) AS total_rows,
  SUM(COALESCE(`apdebit`, 0)) AS total_metric
FROM `m2r_ap_card`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date
GROUP BY period_date
ORDER BY period_date ASC;
