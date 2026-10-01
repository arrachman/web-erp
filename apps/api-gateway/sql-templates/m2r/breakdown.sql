-- Domain: m2r
-- Purpose: grouped breakdown chart
-- Suggested filter source: m2r_ap_card.apstatuslunas

SELECT
  COALESCE(CAST(`__GROUP_BY__` AS CHAR), 'UNKNOWN') AS group_key,
  COUNT(*) AS total_rows,
  SUM(COALESCE(`apsaldoawal`, 0)) AS total_metric
FROM `m2r_ap_card`
WHERE 1=1
AND DATE(__DATE_EXPR__) BETWEEN :from_date AND :to_date
GROUP BY group_key
ORDER BY total_metric DESC, total_rows DESC;
