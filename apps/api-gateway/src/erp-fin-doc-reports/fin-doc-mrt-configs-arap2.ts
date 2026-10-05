/**
 * G3 — Analisa umur piutang/hutang, keluarga IP (terima pembayaran),
 * uang muka penjualan, piutang ongkos kirim.
 *
 * Bucket umur diambil dari LABEL HEADER template legacy (analisa umur):
 * umur1 = jatuh tempo >60 hari lagi; lalu pita 15 hari (60-46, 45-31,
 * 30-16, 15-1 hari sebelum jatuh tempo); umur6 = 0-30 hari lewat;
 * umur7 = 31-60; umur8 = >60 hari lewat. Setting legacy m0_setting
 * UmurPiutang1..10 tidak ikut termigrasi ke ERP, jadi batas pita kini
 * dibaca dari template-nya sendiri (dicatat di DECISIONS G3).
 * Umur diukur dari due_date; faktur tanpa due_date memakai doc_date.
 */
import {
  FinDatasetConfig,
  partnerFilter,
  periodFilters,
} from './fin-doc-mrt-configs';
import { ArApSide } from './fin-doc-mrt-configs-arap';

const P = (side: ArApSide) => (side === 'AR' ? 'ar' : 'ap');
type CardDim = 'branch_id' | 'location_id';

/** [kolom umur, kondisi atas umur hari d] sesuai label header template. */
const BUCKETS: Array<[string, string]> = [
  ['1', 'd < -60'],
  ['2', 'd >= -60 AND d <= -46'],
  ['3', 'd >= -45 AND d <= -31'],
  ['4', 'd >= -30 AND d <= -16'],
  ['5', 'd >= -15 AND d <= -1'],
  ['6', 'd >= 0 AND d <= 30'],
  ['7', 'd >= 31 AND d <= 60'],
  ['8', 'd >= 61'],
];

/** Faktur terbuka (sisa > 0) per tanggal period_end + umur hari. */
const openAgingFrom = (side: ArApSide, dim?: CardDim): string => {
  const invT = side === 'AR' ? 'sls_invoices' : 'pur_invoices';
  const pidCol = side === 'AR' ? 'customer_id' : 'supplier_id';
  const payJoin =
    side === 'AR'
      ? 'JOIN fin_ar_receipts r ON r.id = al.ar_receipt_id'
      : 'JOIN fin_ap_payments r ON r.id = al.ap_payment_id';
  const dimCond = dim
    ? ` AND ((SELECT dimv FROM prm) IS NULL OR v.${dim} = (SELECT dimv FROM prm))`
    : '';
  return `(
WITH prm AS (SELECT COALESCE(?::date, CURRENT_DATE) AS pend${dim ? ', ?::bigint AS dimv' : ''}),
pay AS (
  SELECT al.invoice_ref AS ref, SUM(al.amount) AS amt
  FROM fin_settlement_allocations al ${payJoin}, prm
  WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'
    AND r.transaction_date <= prm.pend
  GROUP BY al.invoice_ref),
open0 AS (
  SELECT i.id AS iid, i.${pidCol} AS pid, i.doc_number, i.doc_date, i.due_date,
    i.grand_total, i.currency_id AS curid, i.branch_id, i.location_id,
    i.grand_total - COALESCE((SELECT SUM(pay.amt) FROM pay
      WHERE pay.ref = i.id::text OR pay.ref = i.doc_number), 0) AS sisa,
    COALESCE((SELECT SUM(pay.amt) FROM pay
      WHERE pay.ref = i.id::text OR pay.ref = i.doc_number), 0) AS bayar
  FROM ${invT} i, prm
  WHERE i.deleted_at IS NULL AND i.posting_status = 'POSTED' AND i.doc_date <= prm.pend)
SELECT v.*, (SELECT pend FROM prm) - COALESCE(v.due_date, v.doc_date) AS age,
  p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode, pt.name AS termnama
FROM open0 v
JOIN md_partners p ON p.id = v.pid
LEFT JOIN md_currencies cur ON cur.id = v.curid
LEFT JOIN md_payment_terms pt ON pt.id = p.sale_term_id
WHERE v.sisa > 0${dimCond}) s`;
};

const agingBinds = (dim?: CardDim) =>
  dim
    ? [
        { name: 'period_end', kind: 'date' as const },
        {
          name: dim === 'branch_id' ? 'branch' : 'location',
          kind: 'number' as const,
        },
      ]
    : [{ name: 'period_end', kind: 'date' as const }];

const agingSummaryConfig = (side: ArApSide, dim?: CardDim): FinDatasetConfig => {
  const p = P(side);
  const buckets = BUCKETS.map(
    ([n, cond]) => `SUM(CASE WHEN ${cond.split('d ').join('s.age ')} THEN s.sisa ELSE 0 END) AS u${n}`,
  ).join(', ');
  return {
    from: `(
SELECT s.id, s.code, s.name, MAX(s.curcode) AS curcode, MAX(s.termnama) AS termnama,
  SUM(s.sisa) AS sisa, ${buckets}
FROM ${openAgingFrom(side, dim)}
GROUP BY s.id, s.code, s.name) s`,
    select: {
      [`${p}kontakkode`]: 's.code',
      [`${p}kontaknama`]: 's.name',
      [`${p}sisa`]: 's.sisa',
      [`${p}matauang`]: 's.curcode',
      [`${p}issaldoakhir`]: '1',
      kterminjual: 's.termnama',
      kterminbeli: 's.termnama',
      ...Object.fromEntries(BUCKETS.map(([n]) => [`${p}umur${n}`, `s.u${n}`])),
      [`${p}umur_1`]: '0',
      [`${p}umur9`]: '0',
      [`${p}umur10`]: '0',
      [`${p}umur_10`]: '0',
    },
    bindParams: agingBinds(dim),
    paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
    orderBy: 's.code',
  };
};

const agingDetailConfig = (side: ArApSide, dim?: CardDim): FinDatasetConfig => {
  const p = P(side);
  return {
    from: openAgingFrom(side, dim),
    select: {
      [`${p}id`]: 's.iid',
      [`${p}notransaksi`]: 's.doc_number',
      [`${p}tgl`]: 's.doc_date',
      [`${p}tgljatuhtempo`]: 's.due_date',
      [`${p}total`]: 's.grand_total',
      [`${p}bayar`]: 's.bayar',
      [`${p}sisa`]: 's.sisa',
      [`${p}kontakkode`]: 's.code',
      [`${p}kontaknama`]: 's.name',
      [`${p}matauang`]: 's.curcode',
      ...Object.fromEntries(
        BUCKETS.map(([n, cond]) => [
          `${p}umur${n}`,
          `(CASE WHEN ${cond.split('d ').join('s.age ')} THEN s.sisa ELSE 0 END)`,
        ]),
      ),
      [`${p}umur_1`]: '0',
      [`${p}umur9`]: '0',
      [`${p}umur10`]: '0',
      [`${p}umur_10`]: '0',
    },
    bindParams: agingBinds(dim),
    paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
    orderBy: 's.code, s.doc_date, s.iid',
  };
};

/* ---------------- IP: daftar/kartu/rekap/voucher terima pembayaran ------ */

const ipListFrom =
  "(SELECT * FROM fin_ar_receipts WHERE posting_status = 'POSTED') r " +
  'JOIN md_partners p ON p.id = r.partner_id ' +
  'LEFT JOIN md_currencies cur ON cur.id = r.currency_id';

const ipDaftarConfig = (): FinDatasetConfig => ({
  from: ipListFrom,
  deletedAlias: 'r',
  select: {
    ipid: 'r.id',
    ipnotransaksi: 'r.doc_number',
    iptgl: 'r.transaction_date',
    ipuraian: 'r.description',
    ipjumlah: 'r.amount',
    ipjumlahbayar: 'r.allocated_amount',
    ipsisa: '(r.amount - r.allocated_amount)',
    ipkontak: 'r.partner_id',
    ipkontakkode: 'p.code',
    ipkontaknama: 'p.name',
    ipkurs: 'r.exchange_rate',
    ipmatauang: 'cur.code',
    ipcatatan: 'r.notes',
  },
  paramFilters: { ...periodFilters('r.transaction_date'), ...partnerFilter('r.partner_id', 'p.code', 'p.name') },
  orderBy: 'r.transaction_date, r.id',
});

const ipDetailConfig = (): FinDatasetConfig => ({
  from:
    "(SELECT * FROM fin_ar_receipts WHERE posting_status = 'POSTED') r " +
    'JOIN md_partners p ON p.id = r.partner_id ' +
    'LEFT JOIN md_currencies cur ON cur.id = r.currency_id ' +
    'LEFT JOIN fin_settlement_allocations al ON al.ar_receipt_id = r.id ' +
    'LEFT JOIN sls_invoices i ON i.id::text = al.invoice_ref OR i.doc_number = al.invoice_ref ' +
    'LEFT JOIN md_partners cp ON cp.id = i.customer_id ' +
    'LEFT JOIN md_currencies icur ON icur.id = i.currency_id',
  deletedAlias: 'r',
  select: {
    ipid: 'r.id',
    ipnotransaksi: 'r.doc_number',
    iptgl: 'r.transaction_date',
    ipjumlah: 'r.amount',
    ipjumlahbayar: 'r.allocated_amount',
    ipsisa: '(r.amount - r.allocated_amount)',
    ipkontakkode: 'p.code',
    ipkontaknama: 'p.name',
    ipkurs: 'r.exchange_rate',
    ipmatauang: 'cur.code',
    ipcatatan: 'r.notes',
    pvid: 'i.id',
    pvnotransaksi: 'i.doc_number',
    pvtgl: 'i.doc_date',
    pvjumlahbayar: 'al.amount',
    jmlbayar: 'al.amount',
    pvcustomernama: 'cp.name',
    pvkurs: 'i.exchange_rate',
    pvmatauang: 'icur.code',
    pvcatatan: 'i.notes',
  },
  paramFilters: { ...periodFilters('r.transaction_date'), ...partnerFilter('r.partner_id', 'p.code', 'p.name') },
  orderBy: 'r.transaction_date, r.id, i.doc_date',
});

const ipCardConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
docs AS (SELECT r.partner_id AS pid, r.transaction_date AS tgl, r.doc_number AS notrans,
    r.description AS uraian, r.amount AS kredit, r.id AS did, r.currency_id AS curid
  FROM fin_ar_receipts r WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'),
per AS (SELECT pid, SUM(CASE WHEN tgl < (SELECT pstart FROM prm) THEN kredit ELSE 0 END) AS awal
  FROM docs GROUP BY pid),
rows0 AS (SELECT d.*, per.awal AS saldoawal,
    per.awal + SUM(d.kredit) OVER (PARTITION BY d.pid ORDER BY d.tgl, d.did) AS saldoakhir
  FROM docs d JOIN per ON per.pid = d.pid, prm
  WHERE d.tgl >= prm.pstart AND d.tgl <= prm.pend)
SELECT r.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode
FROM rows0 r JOIN md_partners p ON p.id = r.pid
LEFT JOIN md_currencies cur ON cur.id = r.curid) s`,
  select: {
    arid: 's.did',
    artgl: 's.tgl',
    arnotransaksi: 's.notrans',
    aruraian: 's.uraian',
    arsumber: `('IP')::text`,
    ardebit: '0',
    arkredit: 's.kredit',
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

const ipRekapConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
docs AS (SELECT r.partner_id AS pid, r.transaction_date AS tgl, r.amount AS kredit,
    r.currency_id AS curid
  FROM fin_ar_receipts r WHERE r.deleted_at IS NULL AND r.posting_status = 'POSTED'),
per AS (SELECT pid,
    SUM(CASE WHEN tgl < (SELECT pstart FROM prm) THEN kredit ELSE 0 END) AS awal,
    SUM(CASE WHEN tgl >= (SELECT pstart FROM prm) AND tgl <= (SELECT pend FROM prm) THEN kredit ELSE 0 END) AS k,
    MAX(curid) AS mcur
  FROM docs GROUP BY pid)
SELECT per.*, p.id AS id, p.code AS code, p.name AS name, cur.code AS curcode,
  ROW_NUMBER() OVER (ORDER BY p.code) AS nourut
FROM per JOIN md_partners p ON p.id = per.pid
LEFT JOIN md_currencies cur ON cur.id = per.mcur
WHERE per.awal <> 0 OR per.k <> 0) s`,
  select: {
    arnourut: 's.nourut',
    arkontakkode: 's.code',
    arkontaknama: 's.name',
    ardebit: '0',
    arkredit: 's.k',
    arsaldoawal: 's.awal',
    arsaldoakhir: '(s.awal + s.k)',
    arissaldoakhir: '(CASE WHEN s.awal + s.k <> 0 THEN 1 ELSE 0 END)',
    armatauang: 's.curcode',
  },
  bindParams: [
    { name: 'period_start', kind: 'date' },
    { name: 'period_end', kind: 'date' },
  ],
  paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
  orderBy: 's.code',
});

const ipVoucherConfig = (): FinDatasetConfig => ({
  from: ipListFrom,
  deletedAlias: 'r',
  select: {
    arid: 'r.id',
    arnotransaksi: 'r.doc_number',
    artgl: 'r.transaction_date',
    artotal: 'r.amount',
    arbayar: 'r.allocated_amount',
    arsisa: '(r.amount - r.allocated_amount)',
    arkontakkode: 'p.code',
    arkontaknama: 'p.name',
    armatauang: 'cur.code',
    aruraian: 'r.description',
    arcatatan: 'r.notes',
  },
  paramFilters: { ...periodFilters('r.transaction_date'), ...partnerFilter('r.partner_id', 'p.code', 'p.name') },
  orderBy: 'r.transaction_date, r.id',
});

/* ---------------- Uang muka penjualan (sls_customer_advances) ----------- */

const umFrom =
  "(SELECT * FROM sls_customer_advances WHERE posting_status = 'POSTED') a " +
  'JOIN md_partners p ON p.id = a.customer_id ' +
  'LEFT JOIN md_currencies cur ON cur.id = a.currency_id';

const umDaftarConfig = (): FinDatasetConfig => ({
  from: umFrom,
  deletedAlias: 'a',
  select: {
    asid: 'a.id',
    asnotransaksi: 'a.doc_number',
    astgl: 'a.doc_date',
    asjumlah: 'a.amount',
    asjumlahbayar: 'a.applied_amount',
    assisa: '(a.amount - a.applied_amount)',
    askontak: 'a.customer_id',
    askontakkode: 'p.code',
    askontaknama: 'p.name',
    askurs: 'a.exchange_rate',
    asmatauang: 'cur.code',
    ascatatan: 'a.notes',
  },
  paramFilters: { ...periodFilters('a.doc_date'), ...partnerFilter('a.customer_id', 'p.code', 'p.name') },
  orderBy: 'a.doc_date, a.id',
});

const umDetailConfig = (): FinDatasetConfig => ({
  from:
    umFrom +
    ' LEFT JOIN fin_ar_receipts r ON r.id = a.ar_receipt_id' +
    ' LEFT JOIN md_currencies rcur ON rcur.id = r.currency_id',
  deletedAlias: 'a',
  select: {
    asid: 'a.id',
    asnotransaksi: 'a.doc_number',
    astgl: 'a.doc_date',
    asjumlah: 'a.amount',
    asjumlahbayar: 'a.applied_amount',
    assisa: '(a.amount - a.applied_amount)',
    askontakkode: 'p.code',
    askontaknama: 'p.name',
    askurs: 'a.exchange_rate',
    asmatauang: 'cur.code',
    ascatatan: 'a.notes',
    pvid: 'r.id',
    pvnotransaksi: 'r.doc_number',
    pvtgl: 'r.transaction_date',
    pvjumlahbayar: 'a.applied_amount',
    jmlbayar: 'a.applied_amount',
    pvcustomernama: 'p.name',
    pvkurs: 'r.exchange_rate',
    pvmatauang: 'rcur.code',
    pvcatatan: 'r.notes',
  },
  paramFilters: { ...periodFilters('a.doc_date'), ...partnerFilter('a.customer_id', 'p.code', 'p.name') },
  orderBy: 'a.doc_date, a.id',
});

export const FIN_G3_ARAP2_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.analisaumurpiutang': { DS1: agingSummaryConfig('AR') },
  'fin.analisaumurpiutangcabang': { DS1: agingSummaryConfig('AR', 'branch_id') },
  'fin.analisaumurpiutanglokasi': { DS1: agingSummaryConfig('AR', 'location_id') },
  'fin.analisaumurpiutangdetail': { DS1: agingDetailConfig('AR') },
  'fin.analisaumurpiutangcabangdetail': { DS1: agingDetailConfig('AR', 'branch_id') },
  'fin.analisaumurpiutanglokasidetail': { DS1: agingDetailConfig('AR', 'location_id') },
  'fin.analisaumurhutang': { DS1: agingSummaryConfig('AP') },
  'fin.analisaumurhutangcabang': { DS1: agingSummaryConfig('AP', 'branch_id') },
  'fin.analisaumurhutanglokasi': { DS1: agingSummaryConfig('AP', 'location_id') },
  'fin.analisaumurhutangdetail': { DS1: agingDetailConfig('AP') },
  'fin.analisaumurhutangcabangdetail': { DS1: agingDetailConfig('AP', 'branch_id') },
  'fin.analisaumurhutanglokasidetail': { DS1: agingDetailConfig('AP', 'location_id') },
  'fin.daftarterimapembayaran': { DS1: ipDaftarConfig() },
  'fin.daftarterimapembayarandetail': { DS1: ipDetailConfig() },
  'fin.kartuterimapembayaran': { DS1: ipCardConfig() },
  'fin.rekapterimapembayaran': { DS1: ipRekapConfig() },
  'fin.voucherterimapembayaran': { DS1: ipVoucherConfig() },
  'fin.daftarumpenjualan': { DS1: umDaftarConfig() },
  'fin.daftarumpenjualandetail': { DS1: umDetailConfig() },
};
