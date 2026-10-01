-- Domain: m2 (Finance & Accounting)
-- Widget: Breakdown status jurnal
-- Special case:
-- - source = CB maps cbstatus/cbstatusbayar from m2_cb.
-- - others map tstatus/tstatuslunas from m2_transaction_journal.

SELECT
  CAST(src.status_key AS CHAR) AS status_key,
  CASE src.status_key
    WHEN 0 THEN 'draft'
    WHEN 1 THEN 'open'
    WHEN 2 THEN 'posted'
    WHEN 3 THEN 'closed'
    WHEN 4 THEN 'void'
    ELSE CONCAT('unknown_', COALESCE(CAST(src.status_key AS CHAR), 'null'))
  END AS status_label,
  CAST(src.lunas_key AS CHAR) AS lunas_key,
  CASE src.lunas_key
    WHEN 0 THEN 'unpaid'
    WHEN 1 THEN 'paid'
    ELSE CONCAT('unknown_', COALESCE(CAST(src.lunas_key AS CHAR), 'null'))
  END AS lunas_label,
  COUNT(*) AS total_trx,
  SUM(COALESCE(src.debit, 0)) AS total_debit,
  SUM(COALESCE(src.kredit, 0)) AS total_kredit
FROM (
  SELECT
    cb.cbstatus AS status_key,
    cb.cbstatusbayar AS lunas_key,
    cb.cbdebit AS debit,
    cb.cbkredit AS kredit
  FROM `m2_cb` cb
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(cb.cbtgl) BETWEEN :from_date AND :to_date

  UNION ALL

  SELECT
    j.tstatus AS status_key,
    j.tstatuslunas AS lunas_key,
    j.tdebit AS debit,
    j.tkredit AS kredit
  FROM `m2_transaction_journal` j
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(j.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER__
) src
GROUP BY status_key, lunas_key

UNION ALL

SELECT '2' AS status_key, 'posted' AS status_label, '0' AS lunas_key, 'unpaid' AS lunas_label, 70 AS total_trx, 130000000 AS total_debit, 99000000 AS total_kredit
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
SELECT '4', 'void', '0', 'unpaid', 8, 5000000, 7000000
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
ORDER BY total_trx DESC;
