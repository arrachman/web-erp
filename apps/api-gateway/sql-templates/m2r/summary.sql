-- Domain: m2r
-- Purpose: KPI cards summary
-- Suggested metric source: m2r_ap_card.apsaldoakhir

SELECT
  COUNT(*) AS total_rows,
  SUM(COALESCE(`apsaldoakhir`, 0)) AS total_metric,
  AVG(COALESCE(`apsaldoakhir`, 0)) AS avg_metric,
  MIN(COALESCE(`apsaldoakhir`, 0)) AS min_metric,
  MAX(COALESCE(`apsaldoakhir`, 0)) AS max_metric
FROM `m2r_ap_card`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date;
