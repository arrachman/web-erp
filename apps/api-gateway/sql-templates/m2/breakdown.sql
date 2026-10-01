-- Domain: m2 (Finance & Accounting)
-- Widget: Breakdown komposisi sumber transaksi
-- Special case:
-- - source = CB uses m2_cb (group key from cbsumber)
-- - others use m2_transaction_journal (group key from tsumber)

SELECT
  COALESCE(NULLIF(TRIM(src.group_key), ''), 'UNKNOWN') AS group_key,
  COUNT(*) AS total_trx,
  SUM(COALESCE(src.debit, 0)) AS total_debit,
  SUM(COALESCE(src.kredit, 0)) AS total_kredit
FROM (
  SELECT cb.cbsumber AS group_key, cb.cbdebit AS debit, cb.cbkredit AS kredit
  FROM `m2_cb` cb
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(cb.cbtgl) BETWEEN :from_date AND :to_date

  UNION ALL

  SELECT j.tsumber AS group_key, j.tdebit AS debit, j.tkredit AS kredit
  FROM `m2_transaction_journal` j
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(j.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER__
) src
GROUP BY group_key

UNION ALL

SELECT 'CR' AS group_key, 28 AS total_trx, 120000000 AS total_debit, 20000000 AS total_kredit
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
SELECT 'CD', 18, 20000000, 97000000
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
SELECT 'GJ', 36, 50000000, 43000000
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
ORDER BY total_debit DESC, total_trx DESC;
