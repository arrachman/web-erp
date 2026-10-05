/**
 * G3 — Kartu/rekap/voucher uang muka penjualan + piutang ongkos kirim.
 *
 * UM penjualan dari sls_customer_advances (POSTED): baris kartu = dokumen
 * UM (kredit) + baris aplikasi (debit = applied_amount, tanggal dokumen UM
 * karena ERP tidak menyimpan tanggal aplikasi terpisah — dicatat di
 * DECISIONS G3). UM pembelian tidak dibangun: ERP tidak punya dokumen
 * uang muka pembelian (lihat daftar honest-empty di configs-lists).
 * Ongkos kirim dari sls_freight_receivables; pelunasan hanya dikenal lewat
 * settlement_status (SETTLED = lunas penuh), tidak ada dokumen pembayaran
 * terpisah sehingga kolom pv* pada template detail dibiarkan NULL.
 */
import {
  FinDatasetConfig,
  partnerFilter,
  periodFilters,
} from './fin-doc-mrt-configs';

const umOpenFrom =
  "(SELECT * FROM sls_customer_advances WHERE posting_status = 'POSTED' AND amount > applied_amount) a " +
  'JOIN md_partners p ON p.id = a.customer_id ' +
  'LEFT JOIN md_currencies cur ON cur.id = a.currency_id';

const umCardConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
docs AS (
  SELECT a.customer_id AS pid, a.doc_date AS tgl, a.doc_number AS notrans,
    a.description AS uraian, 0::numeric AS debit, a.amount AS kredit, 0 AS sk,
    a.id AS did, a.currency_id AS curid
  FROM sls_customer_advances a
  WHERE a.deleted_at IS NULL AND a.posting_status = 'POSTED'
  UNION ALL
  SELECT a.customer_id, a.doc_date, a.doc_number,
    'Aplikasi uang muka', a.applied_amount, 0, 1, a.id, a.currency_id
  FROM sls_customer_advances a
  WHERE a.deleted_at IS NULL AND a.posting_status = 'POSTED' AND a.applied_amount > 0),
per AS (SELECT pid, SUM(CASE WHEN tgl < (SELECT pstart FROM prm) THEN kredit - debit ELSE 0 END) AS awal
  FROM docs GROUP BY pid),
rows0 AS (SELECT d.*, per.awal AS saldoawal,
    per.awal + SUM(d.kredit - d.debit) OVER (PARTITION BY d.pid ORDER BY d.tgl, d.sk, d.did) AS saldoakhir
  FROM docs d JOIN per ON per.pid = d.pid, prm
  WHERE d.tgl >= prm.pstart AND d.tgl <= prm.pend)
SELECT r.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode
FROM rows0 r JOIN md_partners p ON p.id = r.pid
LEFT JOIN md_currencies cur ON cur.id = r.curid) s`,
  select: {
    asid: 's.did',
    astgl: 's.tgl',
    asnotransaksi: 's.notrans',
    asuraian: 's.uraian',
    asdebit: 's.debit',
    askredit: 's.kredit',
    assaldoawal: 's.saldoawal',
    assaldoakhir: 's.saldoakhir',
    askontakkode: 's.code',
    askontaknama: 's.name',
    asmatauang: 's.curcode',
  },
  bindParams: [
    { name: 'period_start', kind: 'date' },
    { name: 'period_end', kind: 'date' },
  ],
  paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
  orderBy: 's.code, s.tgl, s.sk, s.did',
});

const umRekapConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
per AS (SELECT a.customer_id AS pid,
    SUM(CASE WHEN a.doc_date < prm.pstart THEN a.amount - a.applied_amount ELSE 0 END) AS awal,
    SUM(CASE WHEN a.doc_date >= prm.pstart AND a.doc_date <= prm.pend THEN a.applied_amount ELSE 0 END) AS d,
    SUM(CASE WHEN a.doc_date >= prm.pstart AND a.doc_date <= prm.pend THEN a.amount ELSE 0 END) AS k,
    MAX(a.currency_id) AS mcur
  FROM sls_customer_advances a, prm
  WHERE a.deleted_at IS NULL AND a.posting_status = 'POSTED'
  GROUP BY a.customer_id)
SELECT per.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode,
  ROW_NUMBER() OVER (ORDER BY p.code) AS nourut
FROM per JOIN md_partners p ON p.id = per.pid
LEFT JOIN md_currencies cur ON cur.id = per.mcur
WHERE per.awal <> 0 OR per.d <> 0 OR per.k <> 0) s`,
  select: {
    asnourut: 's.nourut',
    askontakkode: 's.code',
    askontaknama: 's.name',
    asdebit: 's.d',
    askredit: 's.k',
    assaldoawal: 's.awal',
    assaldoakhir: '(s.awal + s.k - s.d)',
    asissaldoakhir: '(CASE WHEN s.awal + s.k - s.d <> 0 THEN 1 ELSE 0 END)',
    asmatauang: 's.curcode',
  },
  bindParams: [
    { name: 'period_start', kind: 'date' },
    { name: 'period_end', kind: 'date' },
  ],
  paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
  orderBy: 's.code',
});

const umVoucherConfig = (): FinDatasetConfig => ({
  from: umOpenFrom,
  deletedAlias: 'a',
  select: {
    asid: 'a.id',
    asnotransaksi: 'a.doc_number',
    astgl: 'a.doc_date',
    astotal: 'a.amount',
    asbayar: 'a.applied_amount',
    assisa: '(a.amount - a.applied_amount)',
    askontakkode: 'p.code',
    askontaknama: 'p.name',
    asmatauang: 'cur.code',
    ascatatan: 'a.notes',
  },
  paramFilters: { ...periodFilters('a.doc_date'), ...partnerFilter('a.customer_id', 'p.code', 'p.name') },
  orderBy: 'a.doc_date, a.id',
});

/* ---------------- Piutang ongkos kirim (sls_freight_receivables) ----------- */

const ongkirFrom =
  "(SELECT * FROM sls_freight_receivables WHERE posting_status = 'POSTED') f " +
  'JOIN md_partners p ON p.id = f.customer_id ' +
  'LEFT JOIN md_currencies cur ON cur.id = f.currency_id';

const ongkirBayar = "(CASE WHEN f.settlement_status = 'PAID' THEN f.amount ELSE 0 END)";

const ongkirDaftarConfig = (): FinDatasetConfig => ({
  from: ongkirFrom,
  deletedAlias: 'f',
  select: {
    rpid: 'f.id',
    rpnotransaksi: 'f.doc_number',
    rptgl: 'f.transaction_date',
    rpjumlah: 'f.amount',
    rpjumlahbayar: ongkirBayar,
    rpsisa: `(f.amount - ${ongkirBayar})`,
    rpkontak: 'f.customer_id',
    rpkontakkode: 'p.code',
    rpkontaknama: 'p.name',
    rpcatatan: 'f.notes',
    rpmatauang: 'cur.code',
  },
  paramFilters: { ...periodFilters('f.transaction_date'), ...partnerFilter('f.customer_id', 'p.code', 'p.name') },
  orderBy: 'f.transaction_date, f.id',
});

const ongkirCardConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
docs AS (SELECT f.customer_id AS pid, f.transaction_date AS tgl, f.doc_number AS notrans,
    f.amount AS debit, f.id AS did, f.currency_id AS curid
  FROM sls_freight_receivables f WHERE f.deleted_at IS NULL AND f.posting_status = 'POSTED'),
per AS (SELECT pid, SUM(CASE WHEN tgl < (SELECT pstart FROM prm) THEN debit ELSE 0 END) AS awal
  FROM docs GROUP BY pid),
rows0 AS (SELECT d.*, per.awal AS saldoawal,
    per.awal + SUM(d.debit) OVER (PARTITION BY d.pid ORDER BY d.tgl, d.did) AS saldoakhir
  FROM docs d JOIN per ON per.pid = d.pid, prm
  WHERE d.tgl >= prm.pstart AND d.tgl <= prm.pend)
SELECT r.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode
FROM rows0 r JOIN md_partners p ON p.id = r.pid
LEFT JOIN md_currencies cur ON cur.id = r.curid) s`,
  select: {
    arid: 's.did',
    artgl: 's.tgl',
    arnotransaksi: 's.notrans',
    ardebit: 's.debit',
    arkredit: '0',
    arsaldoawal: 's.saldoawal',
    arsaldoakhir: 's.saldoakhir',
    arkontakkode: 's.code',
    arkontaknama: 's.name',
    armatauang: 's.curcode',
  },
  bindParams: [
    { name: 'period_start', kind: 'date' },
    { name: 'period_end', kind: 'date' },
  ],
  paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
  orderBy: 's.code, s.tgl, s.did',
});

const ongkirRekapConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
per AS (SELECT f.customer_id AS pid,
    SUM(CASE WHEN f.transaction_date < prm.pstart THEN f.amount ELSE 0 END) AS awal,
    SUM(CASE WHEN f.transaction_date >= prm.pstart AND f.transaction_date <= prm.pend THEN f.amount ELSE 0 END) AS d,
    MAX(f.currency_id) AS mcur
  FROM sls_freight_receivables f, prm
  WHERE f.deleted_at IS NULL AND f.posting_status = 'POSTED'
  GROUP BY f.customer_id)
SELECT per.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode,
  ROW_NUMBER() OVER (ORDER BY p.code) AS nourut
FROM per JOIN md_partners p ON p.id = per.pid
LEFT JOIN md_currencies cur ON cur.id = per.mcur
WHERE per.awal <> 0 OR per.d <> 0) s`,
  select: {
    arnourut: 's.nourut',
    arkontakkode: 's.code',
    arkontaknama: 's.name',
    ardebit: 's.d',
    arkredit: '0',
    arsaldoawal: 's.awal',
    arsaldoakhir: '(s.awal + s.d)',
    arissaldoakhir: '(CASE WHEN s.awal + s.d <> 0 THEN 1 ELSE 0 END)',
    armatauang: 's.curcode',
  },
  bindParams: [
    { name: 'period_start', kind: 'date' },
    { name: 'period_end', kind: 'date' },
  ],
  paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
  orderBy: 's.code',
});

const ongkirVoucherConfig = (): FinDatasetConfig => ({
  from:
    "(SELECT * FROM sls_freight_receivables WHERE posting_status = 'POSTED' AND settlement_status <> 'PAID') f " +
    'JOIN md_partners p ON p.id = f.customer_id ' +
    'LEFT JOIN md_currencies cur ON cur.id = f.currency_id',
  deletedAlias: 'f',
  select: {
    arid: 'f.id',
    arnotransaksi: 'f.doc_number',
    artgl: 'f.transaction_date',
    artotal: 'f.amount',
    arbayar: '0',
    arsisa: 'f.amount',
    arkontakkode: 'p.code',
    arkontaknama: 'p.name',
    armatauang: 'cur.code',
    arcatatan: 'f.notes',
  },
  paramFilters: { ...periodFilters('f.transaction_date'), ...partnerFilter('f.customer_id', 'p.code', 'p.name') },
  orderBy: 'f.transaction_date, f.id',
});

export const FIN_G3_ARAP3_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.kartuumpenjualan': { DS1: umCardConfig() },
  'fin.rekapumpenjualan': { DS1: umRekapConfig() },
  'fin.voucherumpenjualan': { DS1: umVoucherConfig() },
  'fin.daftarpiutangongkoskirim': { DS1: ongkirDaftarConfig() },
  'fin.daftarpiutangongkoskirimdetail': { DS1: ongkirDaftarConfig() },
  'fin.kartupiutangongkoskirim': { DS1: ongkirCardConfig() },
  'fin.rekappiutangongkoskirim': { DS1: ongkirRekapConfig() },
  'fin.voucherpiutangongkoskirim': { DS1: ongkirVoucherConfig() },
};
