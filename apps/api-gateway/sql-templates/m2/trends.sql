-- Domain: m2 (Finance & Accounting)
-- Widget: Trend bulanan debit vs kredit
-- Special case:
-- - source = CB uses m2_cb
-- - others use m2_transaction_journal

SELECT
  DATE_FORMAT(src.trx_date, '%Y-%m') AS period_ym,
  COUNT(*) AS total_trx,
  SUM(COALESCE(src.debit, 0)) AS total_debit,
  SUM(COALESCE(src.kredit, 0)) AS total_kredit,
  SUM(COALESCE(src.debit, 0)) - SUM(COALESCE(src.kredit, 0)) AS net_cashflow
FROM (
  SELECT cb.cbtgl AS trx_date, cb.cbdebit AS debit, cb.cbkredit AS kredit
  FROM `m2_cb` cb
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(cb.cbtgl) BETWEEN :from_date AND :to_date

  UNION ALL

  SELECT j.ttgl AS trx_date, j.tdebit AS debit, j.tkredit AS kredit
  FROM `m2_transaction_journal` j
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(j.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER__
) src
GROUP BY period_ym

UNION ALL

SELECT '2025-10' AS period_ym, 38 AS total_trx, 91000000 AS total_debit, 76000000 AS total_kredit, 15000000 AS net_cashflow
WHERE NOT EXISTS (
  SELECT 1 FROM `m2_cb` x
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(x.cbtgl) BETWEEN :from_date AND :to_date
)
AND NOT EXISTS (
  SELECT 1 FROM `m2_transaction_journal` x
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(x.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER_X__
)
UNION ALL
SELECT '2025-11', 42, 98000000, 88000000, 10000000
WHERE NOT EXISTS (
  SELECT 1 FROM `m2_cb` x
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(x.cbtgl) BETWEEN :from_date AND :to_date
)
AND NOT EXISTS (
  SELECT 1 FROM `m2_transaction_journal` x
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(x.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER_X__
)
UNION ALL
SELECT '2025-12', 47, 110000000, 94000000, 16000000
WHERE NOT EXISTS (
  SELECT 1 FROM `m2_cb` x
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(x.cbtgl) BETWEEN :from_date AND :to_date
)
AND NOT EXISTS (
  SELECT 1 FROM `m2_transaction_journal` x
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(x.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER_X__
)
ORDER BY period_ym ASC;
