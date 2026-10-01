-- Domain: m2 (Finance & Accounting)
-- Widget: detail transaksi
-- Special case:
-- - source = CB uses m2_cb header table
-- - others use m2_transaction_journal

SELECT
  src.tid,
  src.trx_date,
  src.cabang,
  src.sumber,
  src.no_transaksi,
  src.kontak_id,
  src.mata_uang,
  src.debit,
  src.kredit,
  (COALESCE(src.debit, 0) - COALESCE(src.kredit, 0)) AS net_amount,
  src.tstatus,
  src.tstatuslunas,
  src.turaian
FROM (
  SELECT
    cb.cbid AS tid,
    DATE(cb.cbtgl) AS trx_date,
    cb.cbcabang AS cabang,
    cb.cbsumber AS sumber,
    cb.cbnotransaksi AS no_transaksi,
    cb.cbkontak AS kontak_id,
    cb.cbmatauang AS mata_uang,
    COALESCE(cb.cbdebit, 0) AS debit,
    COALESCE(cb.cbkredit, 0) AS kredit,
    cb.cbstatus AS tstatus,
    cb.cbstatusbayar AS tstatuslunas,
    cb.cburaian AS turaian
  FROM `m2_cb` cb
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(cb.cbtgl) BETWEEN :from_date AND :to_date

  UNION ALL

  SELECT
    j.tid,
    DATE(j.ttgl) AS trx_date,
    j.tcabang AS cabang,
    j.tsumber AS sumber,
    j.tnotransaksi AS no_transaksi,
    j.tkontak AS kontak_id,
    j.tmatauang AS mata_uang,
    COALESCE(j.tdebit, 0) AS debit,
    COALESCE(j.tkredit, 0) AS kredit,
    j.tstatus,
    j.tstatuslunas,
    j.turaian
  FROM `m2_transaction_journal` j
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(j.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER__
) src

UNION ALL

SELECT
  0 AS tid,
  DATE('2025-12-31') AS trx_date,
  'DUMMY' AS cabang,
  'CR' AS sumber,
  'DUMMY-TRX-001' AS no_transaksi,
  0 AS kontak_id,
  'IDR' AS mata_uang,
  12500000 AS debit,
  0 AS kredit,
  12500000 AS net_amount,
  2 AS tstatus,
  0 AS tstatuslunas,
  'Dummy row (no data in selected period)' AS turaian
WHERE NOT EXISTS (
  SELECT 1 FROM `m2_cb` x
  WHERE __SOURCE_CODE_LITERAL__ = 'CB'
    AND DATE(x.cbtgl) BETWEEN :from_date AND :to_date
)
AND NOT EXISTS (
  SELECT 1
  FROM `m2_transaction_journal` x
  WHERE COALESCE(__SOURCE_CODE_LITERAL__, '') <> 'CB'
    AND DATE(x.ttgl) BETWEEN :from_date AND :to_date
    __SOURCE_FILTER_X__
)
ORDER BY trx_date DESC, tid DESC
LIMIT :limit OFFSET :offset;
