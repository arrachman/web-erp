/**
 * G3 — Kartu piutang/hutang, rekap saldo, voucher tagihan (staging legacy
 * m2r_ar_card / m2r_ap_card / m2r_ar_voucher).
 *
 * Kartu berbasis dokumen POSTED (faktur penjualan/pembelian + penerimaan/
 * pembayaran + retur), bukan staging jurnal legacy — totalnya rekonsiliasi
 * ke saldo kontrol ledger karena setiap dokumen POSTED terproyeksi ke
 * fin_ledger_entries (dibuktikan di E2E G3). Saldo awal per kontak ditanam
 * di setiap baris; saldo akhir per baris = saldo berjalan setelah baris itu
 * (persis pola template: footer grup kosong, kolom Saldo membaca
 * arsaldoakhir/ap saldoakhir per baris).
 */
import { FinDatasetConfig, partnerFilter } from './fin-doc-mrt-configs';

export type ArApSide = 'AR' | 'AP';
type CardDim = 'branch_id' | 'location_id';

const P = (side: ArApSide) => (side === 'AR' ? 'ar' : 'ap');

/** CTE dokumen mutasi per kontak (debit/kredit mentah + kolom sgn saldo). */
export const docsCte = (side: ArApSide, dim?: CardDim): string => {
  const inv =
    side === 'AR'
      ? `SELECT i.customer_id AS pid, i.doc_date AS tgl, 'SI' AS sumber, i.doc_number AS notrans,
           i.description AS uraian, i.grand_total AS debit, 0::numeric AS kredit,
           i.settled_date AS tgllunas, i.currency_id AS curid, i.branch_id AS brid,
           i.location_id AS locid, 0 AS sk, i.id AS did, i.id AS srcid
         FROM sls_invoices i WHERE i.deleted_at IS NULL AND i.posting_status = 'POSTED'`
      : `SELECT i.supplier_id AS pid, i.doc_date AS tgl, 'PI' AS sumber, i.doc_number AS notrans,
           i.description AS uraian, 0::numeric AS debit, i.grand_total AS kredit,
           i.settled_date AS tgllunas, i.currency_id AS curid, i.branch_id AS brid,
           i.location_id AS locid, 0 AS sk, i.id AS did, i.id AS srcid
         FROM pur_invoices i WHERE i.deleted_at IS NULL AND i.posting_status = 'POSTED'`;
  const pay =
    side === 'AR'
      ? `SELECT r.partner_id AS pid, r.transaction_date AS tgl, 'IP' AS sumber, r.doc_number AS notrans,
           r.description AS uraian, 0::numeric AS debit, r.amount AS kredit,
           r.settled_date AS tgllunas, r.currency_id AS curid, r.branch_id AS brid,
           r.location_id AS locid, 1 AS sk, r.id AS did, r.id AS srcid
         FROM fin_ar_receipts r WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'`
      : `SELECT r.partner_id AS pid, r.transaction_date AS tgl, 'VP' AS sumber, r.doc_number AS notrans,
           r.description AS uraian, r.amount AS debit, 0::numeric AS kredit,
           r.settled_date AS tgllunas, r.currency_id AS curid, r.branch_id AS brid,
           r.location_id AS locid, 1 AS sk, r.id AS did, r.id AS srcid
         FROM fin_ap_payments r WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'`;
  const ret =
    side === 'AR'
      ? `SELECT r.customer_id AS pid, r.doc_date AS tgl, 'SR' AS sumber, r.doc_number AS notrans,
           r.description AS uraian, 0::numeric AS debit, r.grand_total AS kredit,
           NULL::date AS tgllunas, r.currency_id AS curid, r.branch_id AS brid,
           r.location_id AS locid, 1 AS sk, r.id AS did, r.id AS srcid
         FROM sls_returns r WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'`
      : `SELECT r.supplier_id AS pid, r.doc_date AS tgl, 'PR' AS sumber, r.doc_number AS notrans,
           r.description AS uraian, r.grand_total AS debit, 0::numeric AS kredit,
           NULL::date AS tgllunas, r.currency_id AS curid, r.branch_id AS brid,
           r.location_id AS locid, 1 AS sk, r.id AS did, r.id AS srcid
         FROM pur_returns r WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'`;
  const sign = side === 'AR' ? 'd.debit - d.kredit' : 'd.kredit - d.debit';
  const dimCol = dim === 'branch_id' ? 'brid' : 'locid';
  const dimWrap = dim
    ? `docs AS (SELECT * FROM docs0 WHERE (SELECT dimv FROM prm) IS NULL OR ${dimCol} = (SELECT dimv FROM prm)),`
    : '';
  return `docs0 AS (${inv} UNION ALL ${pay} UNION ALL ${ret}),
${dimWrap}docsx AS (SELECT d.*, (${sign}) AS sgn FROM ${dim ? 'docs' : 'docs0'} d)`;
};

const PRM_CARD = `prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
  COALESCE(?::date, CURRENT_DATE) AS pend)`;
const PRM_CARD_DIM = `prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
  COALESCE(?::date, CURRENT_DATE) AS pend, ?::bigint AS dimv)`;

const cardBinds = (dim?: CardDim) =>
  dim
    ? [
        { name: 'period_start', kind: 'date' as const },
        { name: 'period_end', kind: 'date' as const },
        {
          name: dim === 'branch_id' ? 'branch' : 'location',
          kind: 'number' as const,
        },
      ]
    : [
        { name: 'period_start', kind: 'date' as const },
        { name: 'period_end', kind: 'date' as const },
      ];

const partnerCols = `
  JOIN md_partners p ON p.id = r.pid
  LEFT JOIN md_currencies cur ON cur.id = r.curid
  LEFT JOIN LATERAL (SELECT address_line1, phone FROM md_partner_addresses
    WHERE partner_id = p.id AND deleted_at IS NULL
    ORDER BY is_default DESC NULLS LAST, id LIMIT 1) addr ON TRUE`;

export const cardConfig = (side: ArApSide, dim?: CardDim): FinDatasetConfig => {
  const p = P(side);
  return {
    from: `(
WITH ${dim ? PRM_CARD_DIM : PRM_CARD},
${docsCte(side, dim)},
per AS (SELECT pid, SUM(CASE WHEN tgl < (SELECT pstart FROM prm) THEN sgn ELSE 0 END) AS awal
  FROM docsx GROUP BY pid),
rows0 AS (SELECT d.*, per.awal AS saldoawal,
    per.awal + SUM(d.sgn) OVER (PARTITION BY d.pid ORDER BY d.tgl, d.sk, d.did) AS saldoakhir
  FROM docsx d JOIN per ON per.pid = d.pid, prm
  WHERE d.tgl >= prm.pstart AND d.tgl <= prm.pend)
SELECT r.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode,
  addr.address_line1 AS alamat1, addr.phone AS notelp1,
  (CASE WHEN ROW_NUMBER() OVER (PARTITION BY r.pid ORDER BY r.tgl DESC, r.sk DESC, r.did DESC) = 1
        THEN 1 ELSE 0 END) AS lastflag
FROM rows0 r ${partnerCols}) s`,
    select: {
      [`${p}id`]: 's.did',
      [`${p}tgl`]: 's.tgl',
      [`${p}notransaksi`]: 's.notrans',
      [`${p}uraian`]: 's.uraian',
      [`${p}sumber`]: 's.sumber',
      [`${p}debit`]: 's.debit',
      [`${p}kredit`]: 's.kredit',
      [`${p}kredits`]: 's.kredit',
      [`${p}saldoawal`]: 's.saldoawal',
      [`${p}saldoakhir`]: 's.saldoakhir',
      [`${p}kontak`]: 's.id',
      [`${p}kontakkode`]: 's.code',
      [`${p}kontaknama`]: 's.name',
      [`${p}alamat1`]: 's.alamat1',
      [`${p}notelp1`]: 's.notelp1',
      [`${p}matauang`]: 's.curcode',
      [`${p}tgllunas`]: 's.tgllunas',
      [`${p}issaldoakhir`]: 's.lastflag',
    },
    bindParams: cardBinds(dim),
    paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
    orderBy: 's.code, s.tgl, s.sk, s.did',
  };
};

export const rekapConfig = (
  side: ArApSide,
  opts: { dim?: CardDim; split?: boolean } = {},
): FinDatasetConfig => {
  const p = P(side);
  const grpCur = opts.split ? ', curid' : '';
  return {
    from: `(
WITH ${opts.dim ? PRM_CARD_DIM : PRM_CARD},
${docsCte(side, opts.dim)},
per AS (SELECT pid${grpCur},
    SUM(CASE WHEN tgl < (SELECT pstart FROM prm) THEN sgn ELSE 0 END) AS awal,
    SUM(CASE WHEN tgl >= (SELECT pstart FROM prm) AND tgl <= (SELECT pend FROM prm) THEN debit ELSE 0 END) AS d,
    SUM(CASE WHEN tgl >= (SELECT pstart FROM prm) AND tgl <= (SELECT pend FROM prm) THEN kredit ELSE 0 END) AS k,
    SUM(CASE WHEN tgl >= (SELECT pstart FROM prm) AND tgl <= (SELECT pend FROM prm) THEN sgn ELSE 0 END) AS w,
    MAX(curid) AS mcur
  FROM docsx GROUP BY pid${grpCur})
SELECT per.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode,
  ROW_NUMBER() OVER (ORDER BY p.code) AS nourut
FROM per JOIN md_partners p ON p.id = per.pid
LEFT JOIN md_currencies cur ON cur.id = per.mcur
WHERE per.awal <> 0 OR per.d <> 0 OR per.k <> 0) s`,
    select: {
      [`${p}nourut`]: 's.nourut',
      [`${p}kontakkode`]: 's.code',
      [`${p}kontaknama`]: 's.name',
      [`${p}debit`]: 's.d',
      [`${p}kredit`]: 's.k',
      [`${p}saldoawal`]: 's.awal',
      [`${p}saldoakhir`]: '(s.awal + s.w)',
      [`${p}issaldoakhir`]: '(CASE WHEN s.awal + s.w <> 0 THEN 1 ELSE 0 END)',
      [`${p}matauang`]: 's.curcode',
    },
    bindParams: cardBinds(opts.dim),
    paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
    orderBy: 's.code',
  };
};

/** Voucher tagihan terbuka: faktur + alokasi pembayaran s.d. period_end. */
export const openInvoicesFrom = (side: ArApSide, dim?: CardDim): string => {
  const invT = side === 'AR' ? 'sls_invoices' : 'pur_invoices';
  const pidCol = side === 'AR' ? 'customer_id' : 'supplier_id';
  const payJoin =
    side === 'AR'
      ? 'JOIN fin_ar_receipts r ON r.id = al.ar_receipt_id'
      : 'JOIN fin_ap_payments r ON r.id = al.ap_payment_id';
  const dimCond = dim
    ? ` AND ((SELECT dimv FROM prm) IS NULL OR i.${dim} = (SELECT dimv FROM prm))`
    : '';
  return `(
WITH ${dim ? PRM_CARD_DIM : PRM_CARD},
pay AS (
  SELECT al.invoice_ref AS ref, SUM(al.amount) AS amt
  FROM fin_settlement_allocations al
  ${payJoin}, prm
  WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'
    AND r.transaction_date <= prm.pend
  GROUP BY al.invoice_ref),
inv AS (
  SELECT i.id AS iid, i.${pidCol} AS pid, i.doc_number, i.doc_date, i.due_date, i.grand_total,
    i.currency_id AS curid, i.description, i.notes
  FROM ${invT} i, prm
  WHERE i.deleted_at IS NULL AND i.posting_status = 'POSTED'
    AND i.doc_date >= prm.pstart AND i.doc_date <= prm.pend${dimCond})
SELECT v.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode,
  addr.address_line1 AS alamat1, addr.phone AS notelp1, pt.name AS termnama
FROM (SELECT i.*, COALESCE((SELECT SUM(pay.amt) FROM pay
        WHERE pay.ref = i.iid::text OR pay.ref = i.doc_number), 0) AS bayar,
      i.grand_total - COALESCE((SELECT SUM(pay.amt) FROM pay
        WHERE pay.ref = i.iid::text OR pay.ref = i.doc_number), 0) AS sisa
  FROM inv i) v
JOIN md_partners p ON p.id = v.pid
LEFT JOIN md_currencies cur ON cur.id = v.curid
LEFT JOIN md_payment_terms pt ON pt.id = p.sale_term_id
LEFT JOIN LATERAL (SELECT address_line1, phone FROM md_partner_addresses
  WHERE partner_id = p.id AND deleted_at IS NULL
  ORDER BY is_default DESC NULLS LAST, id LIMIT 1) addr ON TRUE) s`;
};

export const voucherConfig = (side: ArApSide, dim?: CardDim): FinDatasetConfig => {
  const p = P(side);
  return {
    from: openInvoicesFrom(side, dim),
    select: {
      [`${p}id`]: 's.iid',
      [`${p}notransaksi`]: 's.doc_number',
      [`${p}tgl`]: 's.doc_date',
      [`${p}tgljatuhtempo`]: 's.due_date',
      [`${p}total`]: 's.grand_total',
      [`${p}bayar`]: 's.bayar',
      [`${p}sisa`]: 's.sisa',
      [`${p}kontak`]: 's.id',
      [`${p}kontakkode`]: 's.code',
      [`${p}kontaknama`]: 's.name',
      [`${p}alamat1`]: 's.alamat1',
      [`${p}notelp1`]: 's.notelp1',
      [`${p}matauang`]: 's.curcode',
      [`${p}uraian`]: 's.description',
      [`${p}catatan`]: 's.notes',
    },
    bindParams: cardBinds(dim),
    paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
    orderBy: 's.code, s.doc_date, s.id',
  };
};

export const FIN_G3_ARAP_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.kartupiutang': { DS1: cardConfig('AR') },
  'fin.kartupiutangcabang': { DS1: cardConfig('AR', 'branch_id') },
  'fin.kartupiutanglokasi': { DS1: cardConfig('AR', 'location_id') },
  'fin.kartuhutang': { DS1: cardConfig('AP') },
  'fin.kartuhutangcabang': { DS1: cardConfig('AP', 'branch_id') },
  'fin.kartuhutanglokasi': { DS1: cardConfig('AP', 'location_id') },
  'fin.rekappiutang': { DS1: rekapConfig('AR') },
  'fin.rekappiutangcabang': { DS1: rekapConfig('AR', { dim: 'branch_id' }) },
  'fin.rekappiutanglokasi': { DS1: rekapConfig('AR', { dim: 'location_id' }) },
  'fin.rekappiutangdetail': { DS1: rekapConfig('AR') },
  'fin.rekappiutangsplit': { DS1: rekapConfig('AR', { split: true }) },
  'fin.rekaphutang': { DS1: rekapConfig('AP') },
  'fin.rekaphutangcabang': { DS1: rekapConfig('AP', { dim: 'branch_id' }) },
  'fin.rekaphutanglokasi': { DS1: rekapConfig('AP', { dim: 'location_id' }) },
  'fin.rekaphutangsplit': { DS1: rekapConfig('AP', { split: true }) },
  'fin.voucherpiutang': { DS1: voucherConfig('AR') },
  'fin.voucherpiutang2': { DS1: voucherConfig('AR') },
  'fin.voucherpiutang22': { DS1: voucherConfig('AR') },
  'fin.voucherpiutangcabang': { DS1: voucherConfig('AR', 'branch_id') },
  'fin.voucherpiutanglokasi': { DS1: voucherConfig('AR', 'location_id') },
  'fin.voucherhutang': { DS1: voucherConfig('AP') },
  'fin.voucherhutangcabang': { DS1: voucherConfig('AP', 'branch_id') },
  'fin.voucherhutanglokasi': { DS1: voucherConfig('AP', 'location_id') },
  'fin.ardailyestimatedcash': { DS1: voucherConfig('AR') },
  'fin.dailyarestimate': { DS1: voucherConfig('AR') },
  'fin.dailyapestimate': { DS1: voucherConfig('AP') },
};
