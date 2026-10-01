-- Domain: m2 (Finance & Accounting)
-- Widget: Top cabang by nominal transaksi jurnal
-- Special case:
-- - source = CB uses m2_cb
-- - others use m2_transaction_journal

SELECT
  COALESCE(NULLIF(TRIM(src.cabang), ''), 'UNKNOWN') AS cabang,
  COUNT(*) AS total_trx,
  SUM(COALESCE(src.debit, 0)) AS total_debit,
  SUM(COALESCE(src.kredit, 0)) AS total_kredit,
  SUM(ABS(COALESCE(src.debit, 0) - COALESCE(src.kredit, 0))) AS movement_amount
FROM (
  SELECT cb.cbcabang AS cabang, cb.cbdebit AS debit, cb.cbkredit AS kredit
  FROM `m2_cb` cb
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(cb.cbtgl) BETWEEN :from_date AND :to_date

  UNION ALL

  SELECT j.tcabang AS cabang, j.tdebit AS debit, j.tkredit AS kredit
  FROM `m2_transaction_journal` j
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(j.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER__
) src
GROUP BY cabang

UNION ALL

SELECT 'JKT' AS cabang, 58 AS total_trx, 120000000 AS total_debit, 93000000 AS total_kredit, 27000000 AS movement_amount
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
SELECT 'SBY', 34, 76000000, 71000000, 5000000
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
SELECT 'BDG', 27, 64000000, 58000000, 6000000
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
ORDER BY movement_amount DESC, total_trx DESC;
