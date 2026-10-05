/**
 * G3 — Buku besar (gl*), buku besar per cost-center/kontak (bp*),
 * daftar data jurnal (t*), rekap buku besar piutang/hutang per sumber.
 *
 * Saldo berjalan buku besar memakai konvensi sisi normal sama seperti
 * endpoint general-ledger yang live: saldo awal = opening_balance
 * md_accounts + mutasi sisi normal sebelum periode, lalu akumulasi
 * per akun terurut (entry_date, id).
 */
import { FinDatasetConfig } from './fin-doc-mrt-configs';
import { partnerFilter, periodFilters } from './fin-doc-mrt-configs';
import { DIM_PARAM, LedgerDim } from './fin-doc-mrt-configs-g3';

interface GlOpts {
  order: 'date' | 'account';
  dim?: LedgerDim;
  kontak?: boolean;
}

const glConfig = (o: GlOpts): FinDatasetConfig => {
  const dimBind = o.dim ? ', ?::bigint AS dimv' : '';
  const dimCond = o.dim ? ` AND (prm.dimv IS NULL OR l.${o.dim} = prm.dimv)` : '';
  const part = o.kontak ? 'r.aid, r.pid' : 'r.aid';
  const openGrp = o.kontak ? 'l.account_id, l.partner_id' : 'l.account_id';
  const openJoin = o.kontak
    ? 'o.aid = r.aid AND o.pid IS NOT DISTINCT FROM r.pid'
    : 'o.aid = r.aid';
  return {
    from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend${dimBind}),
open AS (
  SELECT ${o.kontak ? 'l.account_id AS aid, l.partner_id AS pid' : 'l.account_id AS aid'},
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS mv
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date < prm.pstart${dimCond}
  GROUP BY ${openGrp}),
rows0 AS (
  SELECT l.id, l.entry_date, l.doc_number, l.description, l.debit, l.credit, l.notes,
    a.id AS aid, a.code, a.name, a.normal_balance AS nb, a.opening_balance AS ob,
    p.id AS pid, p.code AS pcode, p.name AS pname, cur.code AS curcode,
    (CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS delta
  FROM fin_ledger_entries l
  JOIN md_accounts a ON a.id = l.account_id
  LEFT JOIN md_partners p ON p.id = l.partner_id
  LEFT JOIN md_currencies cur ON cur.id = l.currency_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date >= prm.pstart AND l.entry_date <= prm.pend${dimCond})
SELECT r.*, r.ob + COALESCE(o.mv, 0)
  + SUM(r.delta) OVER (PARTITION BY ${part} ORDER BY r.entry_date, r.id) AS saldo
FROM rows0 r LEFT JOIN open o ON ${openJoin}) s`,
    select: {
      glid: 's.id',
      gltgl: 's.entry_date',
      gluraian: 's.description',
      gldebit: 's.debit',
      glkredit: 's.credit',
      glsaldo: 's.saldo',
      glnorek: 's.code',
      glnoreknama: 's.name',
      glkontak: 's.pid',
      glkontakkode: 's.pcode',
      glkontaknama: 's.pname',
      glmatauang: 's.curcode',
      glref: 's.doc_number',
      glcatatan: 's.notes',
      glstatusnama: `('Posted')::text`,
      cdc: `(CASE WHEN s.nb = 'DEBIT' THEN 'D' ELSE 'K' END)::text`,
    },
    bindParams: o.dim
      ? [
          { name: 'period_start', kind: 'date' as const },
          { name: 'period_end', kind: 'date' as const },
          { name: DIM_PARAM[o.dim], kind: 'number' as const },
        ]
      : [
          { name: 'period_start', kind: 'date' as const },
          { name: 'period_end', kind: 'date' as const },
        ],
    orderBy:
      o.order === 'date' ? 's.entry_date, s.code, s.id' : 's.code, s.entry_date, s.id',
  };
};

/** Buku besar per cost center: saldo berjalan per (akun, cost center). */
const bpCostCenterConfig = (): FinDatasetConfig => ({
  from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
open AS (
  SELECT l.account_id AS aid, l.cost_center_id AS ccid,
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS mv
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date < prm.pstart
  GROUP BY l.account_id, l.cost_center_id),
rows0 AS (
  SELECT l.id, l.entry_date, l.description, l.debit, l.credit,
    a.id AS aid, a.code, a.name, cc.id AS ccid, cc.code AS cccode, cc.name AS ccname,
    cur.code AS curcode,
    (CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS delta
  FROM fin_ledger_entries l
  JOIN md_accounts a ON a.id = l.account_id
  LEFT JOIN md_cost_centers cc ON cc.id = l.cost_center_id
  LEFT JOIN md_currencies cur ON cur.id = l.currency_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date >= prm.pstart AND l.entry_date <= prm.pend)
SELECT r.*, COALESCE(o.mv, 0)
  + SUM(r.delta) OVER (PARTITION BY r.aid, r.ccid ORDER BY r.entry_date, r.id) AS saldoakhir
FROM rows0 r LEFT JOIN open o ON o.aid = r.aid AND o.ccid IS NOT DISTINCT FROM r.ccid) s`,
  select: {
    bptgl: 's.entry_date',
    bpuraian: 's.description',
    bpnorek: 's.code',
    bpnoreknama: 's.name',
    bpdebit: 's.debit',
    bpkredit: 's.credit',
    bpsaldoakhir: 's.saldoakhir',
    bpcostcenter: 's.ccid',
    bpcostcenternama: 's.ccname',
    bpmatauang: 's.curcode',
  },
  bindParams: [
    { name: 'period_start', kind: 'date' },
    { name: 'period_end', kind: 'date' },
  ],
  orderBy: 's.code, s.ccname, s.entry_date, s.id',
});

/** Rekap per kontak / per cost center: saldo awal, mutasi, saldo akhir. */
const bpRekapConfig = (group: 'partner' | 'cost_center'): FinDatasetConfig => {
  const gcol = group === 'partner' ? 'partner_id' : 'cost_center_id';
  return {
    from: `(
WITH prm AS (SELECT COALESCE(?::date, DATE '1900-01-01') AS pstart,
    COALESCE(?::date, CURRENT_DATE) AS pend),
pre AS (
  SELECT l.account_id AS aid, l.${gcol} AS gid,
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS mv
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date < prm.pstart
  GROUP BY l.account_id, l.${gcol}),
per AS (
  SELECT l.account_id AS aid, l.${gcol} AS gid,
    SUM(l.debit) AS d, SUM(l.credit) AS c, MAX(cur.code) AS curc
  FROM fin_ledger_entries l
  LEFT JOIN md_currencies cur ON cur.id = l.currency_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date >= prm.pstart AND l.entry_date <= prm.pend
  GROUP BY l.account_id, l.${gcol}),
pairs AS (
  SELECT aid, gid FROM per UNION SELECT aid, gid FROM pre)
SELECT a.code, a.name, a.normal_balance AS nb,
  COALESCE(pr.mv, 0) AS saldoawal, COALESCE(pe.d, 0) AS d, COALESCE(pe.c, 0) AS c,
  pe.curc AS curcode,
  g.id AS gid, g.code AS gcode, g.name AS gname
FROM pairs x
JOIN md_accounts a ON a.id = x.aid
LEFT JOIN pre pr ON pr.aid = x.aid AND pr.gid IS NOT DISTINCT FROM x.gid
LEFT JOIN per pe ON pe.aid = x.aid AND pe.gid IS NOT DISTINCT FROM x.gid
LEFT JOIN ${group === 'partner' ? 'md_partners' : 'md_cost_centers'} g ON g.id = x.gid) s`,
    select: {
      bpnorek: 's.code',
      bpnoreknama: 's.name',
      bpsaldoawal: 's.saldoawal',
      bpdebit: 's.d',
      bpkredit: 's.c',
      bpsaldoakhir:
        '(s.saldoawal + (CASE WHEN s.nb = \'DEBIT\' THEN s.d - s.c ELSE s.c - s.d END))',
      bpissaldoakhir:
        "(CASE WHEN s.saldoawal + (CASE WHEN s.nb = 'DEBIT' THEN s.d - s.c ELSE s.c - s.d END) <> 0 THEN 1 ELSE 0 END)",
      bpmatauang: 's.curcode',
      ...(group === 'partner'
        ? { bpkontak: 's.gid', bpkontakkode: 's.gcode', bpkontaknama: 's.gname' }
        : { bpcostcenter: 's.gid', bpcostcenternama: 's.gname' }),
    },
    bindParams: [
      { name: 'period_start', kind: 'date' },
      { name: 'period_end', kind: 'date' },
    ],
    orderBy: 's.gcode, s.code',
  };
};

const daftarDataJurnalConfig = (): FinDatasetConfig => ({
  from: 'fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id ' +
    'LEFT JOIN md_partners p ON p.id = l.partner_id ' +
    'LEFT JOIN md_currencies cur ON cur.id = l.currency_id',
  deletedAlias: 'l',
  select: {
    tnotransaksi: 'l.doc_number',
    ttgl: 'l.entry_date',
    turaian: 'l.description',
    tnorek: 'a.code',
    namarekening: 'a.name',
    tdebit: 'l.debit',
    tkredit: 'l.credit',
    tmatauang: 'cur.code',
    tstatusnama: `('Posted')::text`,
    thutangpiutang: 'l.ar_ap_type::text',
    tkontak: 'l.partner_id',
    tcatatan: 'l.notes',
    knama: 'p.name',
  },
  paramFilters: {
    ...periodFilters('l.entry_date'),
    ...partnerFilter('p.id', 'p.code', 'p.name'),
    division: { sql: 'l.division_id = ?', kind: 'number' },
    cost_center: { sql: 'l.cost_center_id = ?', kind: 'number' },
    project: { sql: 'l.project_id = ?', kind: 'number' },
  },
  orderBy: 'l.entry_date, l.id',
});

/** Rekap buku besar piutang/hutang: total per sumber dokumen untuk 1 kontak. */
const rekapBukuBesarConfig = (side: 'AR' | 'AP'): FinDatasetConfig => {
  const ctrl =
    side === 'AR'
      ? `(SELECT receivable_account_id FROM md_partners WHERE receivable_account_id IS NOT NULL
          UNION SELECT receivable_account_id FROM sls_invoices WHERE receivable_account_id IS NOT NULL)`
      : `(SELECT payable_account_id FROM md_partners WHERE payable_account_id IS NOT NULL
          UNION SELECT payable_account_id FROM pur_invoices WHERE payable_account_id IS NOT NULL)`;
  return {
    from: `(
SELECT p.id AS id, p.code AS code, p.name AS name,
  l.source_doc_type::text AS src, SUM(l.debit) AS tdebit, SUM(l.credit) AS tkredit
FROM fin_ledger_entries l JOIN md_partners p ON p.id = l.partner_id
WHERE l.deleted_at IS NULL AND l.account_id IN ${ctrl}
GROUP BY p.id, p.code, p.name, l.source_doc_type) s`,
    select: {
      tsumber: 's.src',
      tsumbernama:
        "(CASE s.src WHEN 'sls_invoices' THEN 'Faktur Penjualan' " +
        "WHEN 'fin_ar_receipts' THEN 'Penerimaan Piutang' " +
        "WHEN 'pur_invoices' THEN 'Faktur Pembelian' " +
        "WHEN 'fin_ap_payments' THEN 'Pembayaran Hutang' ELSE s.src END)::text",
      tdebit: 's.tdebit',
      tkredit: 's.tkredit',
    },
    paramFilters: { ...partnerFilter('s.id', 's.code', 's.name') },
    orderBy: 's.src',
  };
};

export const FIN_G3_GL_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.bukubesarglobalpertanggal': { DS1: glConfig({ order: 'date' }) },
  'fin.bukubesarglobaltidakpertanggal': { DS1: glConfig({ order: 'account' }) },
  'fin.bukubesarcabangpertanggal': { DS1: glConfig({ order: 'date', dim: 'branch_id' }) },
  'fin.bukubesarpercabang': { DS1: glConfig({ order: 'account', dim: 'branch_id' }) },
  'fin.bukubesardivisipertanggal': { DS1: glConfig({ order: 'date', dim: 'division_id' }) },
  'fin.bukubesardivisi': { DS1: glConfig({ order: 'account', dim: 'division_id' }) },
  'fin.bukubesarlokasipertanggal': { DS1: glConfig({ order: 'date', dim: 'location_id' }) },
  'fin.bukubesarlokasi': { DS1: glConfig({ order: 'account', dim: 'location_id' }) },
  'fin.bukubesarproyekpertanggal': { DS1: glConfig({ order: 'date', dim: 'project_id' }) },
  'fin.bukubesarproyek': { DS1: glConfig({ order: 'account', dim: 'project_id' }) },
  'fin.bukubesarpekontak': { DS1: glConfig({ order: 'account', kontak: true }) },
  'fin.bukubesarpekontak2': { DS1: glConfig({ order: 'account', kontak: true }) },
  'fin.bukubesarpekontak2kop1': { DS1: glConfig({ order: 'account', kontak: true }) },
  'fin.bukubesarpekontak2kop2': { DS1: glConfig({ order: 'account', kontak: true }) },
  'fin.bukubesarpercostcenter': { DS1: bpCostCenterConfig() },
  'fin.rekappercostcenter': { DS1: bpRekapConfig('cost_center') },
  'fin.rekapperkontak': { DS1: bpRekapConfig('partner') },
  'fin.daftardatajurnal': { DS1: daftarDataJurnalConfig() },
  'fin.rekapbukubesarpiutang': { DS1: rekapBukuBesarConfig('AR') },
  'fin.rekapbukubesarpiutangpercustomer': { DS1: rekapBukuBesarConfig('AR') },
  'fin.rekapbukubesarhutang': { DS1: rekapBukuBesarConfig('AP') },
  'fin.rekapbukubesarhutangpersupplier': { DS1: rekapBukuBesarConfig('AP') },
};
