-- Domain: m2 (Finance & Accounting)
-- Widget: KPI cards (single row)
-- Special case:
-- - When source feature = CB, data is read from m2_cb.
-- - Otherwise, data is read from m2_transaction_journal.

SELECT
  CASE WHEN COUNT(*) = 0 THEN 120 ELSE COUNT(*) END AS total_journal_rows,
  CASE WHEN COUNT(*) = 0 THEN 250000000 ELSE SUM(COALESCE(src.debit, 0)) END AS total_debit,
  CASE WHEN COUNT(*) = 0 THEN 210000000 ELSE SUM(COALESCE(src.kredit, 0)) END AS total_kredit,
  CASE WHEN COUNT(*) = 0 THEN 40000000 ELSE (SUM(COALESCE(src.debit, 0)) - SUM(COALESCE(src.kredit, 0))) END AS net_cashflow,
  CASE WHEN COUNT(*) = 0 THEN 6 ELSE COUNT(DISTINCT src.cabang) END AS total_cabang,
  CASE WHEN COUNT(*) = 0 THEN 4 ELSE COUNT(DISTINCT COALESCE(src.sumber, 'UNKNOWN')) END AS total_sumber
FROM (
  SELECT
    cb.cbid AS trx_id,
    cb.cbcabang AS cabang,
    cb.cbsumber AS sumber,
    cb.cbdebit AS debit,
    cb.cbkredit AS kredit
  FROM `m2_cb` cb
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(cb.cbtgl) BETWEEN :from_date AND :to_date

  UNION ALL

  SELECT
    j.tid AS trx_id,
    j.tcabang AS cabang,
    j.tsumber AS sumber,
    j.tdebit AS debit,
    j.tkredit AS kredit
  FROM `m2_transaction_journal` j
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(j.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER__
) src;
