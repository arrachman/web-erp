-- Domain: m
-- Purpose: KPI cards summary
-- Suggested metric source: m_10_ad.adtotalpotongan

SELECT
  COUNT(*) AS total_rows,
  SUM(COALESCE(`adtotalpotongan`, 0)) AS total_metric,
  AVG(COALESCE(`adtotalpotongan`, 0)) AS avg_metric,
  MIN(COALESCE(`adtotalpotongan`, 0)) AS min_metric,
  MAX(COALESCE(`adtotalpotongan`, 0)) AS max_metric
FROM `m_10_ad`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date;
