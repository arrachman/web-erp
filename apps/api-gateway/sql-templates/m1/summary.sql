-- Domain: m1
-- Purpose: KPI cards summary
-- Suggested metric source: m1_item_transaction.saldojml

SELECT
  COUNT(*) AS total_rows,
  SUM(COALESCE(`saldojml`, 0)) AS total_metric,
  AVG(COALESCE(`saldojml`, 0)) AS avg_metric,
  MIN(COALESCE(`saldojml`, 0)) AS min_metric,
  MAX(COALESCE(`saldojml`, 0)) AS max_metric
FROM `m1_item_transaction`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date;
