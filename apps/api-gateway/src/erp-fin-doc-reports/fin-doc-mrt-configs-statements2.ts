/**
 * G3 — Neraca mutasi (nm*), anggaran & realisasi, laporan arus kas,
 * mutasi keuangan, daily bank, neraca T level<=2 (neracat2).
 *
 * Neraca mutasi memakai konvensi DEBIT-POSITIF persis endpoint yang live:
 * saldo awal = opening_balance (bertanda debit) + mutasi (debit−kredit)
 * sebelum periode; saldo akhir = awal + debit − kredit periode.
 */
import { FinDatasetConfig } from './fin-doc-mrt-configs';
import {
  ANCESTOR_JOINS,
  BIND_PERIOD,
  DIM_PARAM,
  DIM_PREFIX,
  DIM_TABLE,
  LedgerDim,
  PRM_DIM,
  PRM_MONTH,
  SUB_CTE,
} from './fin-doc-mrt-configs-g3';
import { pkCtes, PkOpts } from './fin-doc-mrt-configs-statements';

/** Set akun kas/bank: dari transaksi + turunan header bernama kas (level<=2). */
const CASHACC_CTE = `cashacc AS (
  SELECT DISTINCT bank_account_id AS aid FROM fin_cash_bank_transactions
  WHERE bank_account_id IS NOT NULL AND deleted_at IS NULL
  UNION
  SELECT s.desc_id FROM sub s
  WHERE s.root_id IN (SELECT id FROM md_accounts
    WHERE deleted_at IS NULL AND level <= 2 AND name ILIKE '%kas%'))`;

interface NmOpts {
  dim?: LedgerDim;
  cashBankOnly?: boolean;
}

const nmConfig = (o: NmOpts): FinDatasetConfig => {
  const dimCond = o.dim ? ` AND (prm.dimv IS NULL OR l.${o.dim} = prm.dimv)` : '';
  const dimSel = o.dim
    ? `, prm.dimv AS dimid, dt.name AS dimnama`
    : '';
  const dimFrom = o.dim
    ? `, prm LEFT JOIN ${DIM_TABLE[o.dim]} dt ON dt.id = prm.dimv`
    : `, prm`;
  const cashFilter = o.cashBankOnly
    ? ` AND a.id IN (SELECT aid FROM cashacc)`
    : '';
  const dimCols: Record<string, string> = {};
  if (o.dim === 'branch_id') {
    dimCols.nmcabang = 's.dimid';
    dimCols.nmcabangnama = 's.dimnama';
  }
  if (o.dim === 'location_id') {
    dimCols.nmlokasi = 's.dimid';
    dimCols.nmlokasinama = 's.dimnama';
  }
  return {
    from: `(
WITH RECURSIVE ${SUB_CTE},
${CASHACC_CTE},
${o.dim ? PRM_DIM : PRM_MONTH},
led AS (
  SELECT l.account_id AS aid,
    SUM(CASE WHEN l.entry_date < prm.pstart THEN l.debit - l.credit ELSE 0 END) AS pre,
    SUM(CASE WHEN l.entry_date >= prm.pstart AND l.entry_date <= prm.pend THEN l.debit ELSE 0 END) AS d,
    SUM(CASE WHEN l.entry_date >= prm.pstart AND l.entry_date <= prm.pend THEN l.credit ELSE 0 END) AS c
  FROM fin_ledger_entries l, prm
  WHERE l.deleted_at IS NULL${dimCond}
  GROUP BY l.account_id)
SELECT a.code AS norek, a.name AS noreknama,
  (CASE WHEN a.level <= 1 THEN a.name WHEN a.level = 2 THEN a1.name
        WHEN a.level = 3 THEN a2.name WHEN a.level = 4 THEN a3.name ELSE a4.name END) AS grop,
  (CASE WHEN a.normal_balance = 'DEBIT' THEN a.opening_balance ELSE -a.opening_balance END)
    + COALESCE(x.pre, 0) AS saldoawal,
  COALESCE(x.d, 0) AS debit, COALESCE(x.c, 0) AS kredit,
  a.level${dimSel}
FROM md_accounts a ${ANCESTOR_JOINS}
LEFT JOIN led x ON x.aid = a.id${dimFrom}
WHERE a.deleted_at IS NULL AND a.kind = 'POSTABLE'
  AND (COALESCE(x.pre, 0) <> 0 OR COALESCE(x.d, 0) <> 0 OR COALESCE(x.c, 0) <> 0
       OR a.opening_balance <> 0)${cashFilter}) s`,
    select: {
      nmnorek: 's.norek',
      nmnoreknama: 's.noreknama',
      grop: 's.grop',
      nmsaldoawal: 's.saldoawal',
      nmdebit: 's.debit',
      nmkredit: 's.kredit',
      nmsaldoakhir: '(s.saldoawal + s.debit - s.kredit)',
      nmf1: 'NULL',
      nmf2: 'NULL',
      nmf3: 'NULL',
      nmf4: 'NULL',
      ...dimCols,
    },
    bindParams: o.dim
      ? [...BIND_PERIOD, { name: DIM_PARAM[o.dim], kind: 'number' as const }]
      : BIND_PERIOD,
    orderBy: 's.norek',
  };
};

interface AggOpts {
  dim?: LedgerDim;
}

const anggaranConfig = (o: AggOpts): FinDatasetConfig => {
  const dimSel = o.dim ? `, prm.dimv AS dimid, dt.name AS dimnama` : '';
  const dimFrom = o.dim
    ? `, prm LEFT JOIN ${DIM_TABLE[o.dim!]} dt ON dt.id = prm.dimv`
    : '';
  const dimCols: Record<string, string> = {};
  if (o.dim) {
    const p = DIM_PREFIX[o.dim];
    dimCols[`nm${p}`] = 's.dimid';
    dimCols[`nm${p}nama`] = 's.dimnama';
  }
  return {
    from: `(
WITH prm AS (SELECT COALESCE(?::date, CURRENT_DATE) AS pend,
    COALESCE(?::date, date_trunc('year', CURRENT_DATE)::date) AS pstart${o.dim ? ', ?::bigint AS dimv' : ''}),
agg AS (
  SELECT r.account_id AS aid, SUM(r.budget_amount) AS bud,
    SUM(r.debit_total) AS dt, SUM(r.credit_total) AS ct
  FROM fin_budget_realizations r
  JOIN sys_fiscal_periods fp ON fp.id = r.fiscal_period_id, prm
  WHERE r.deleted_at IS NULL AND fp.start_date <= prm.pend AND fp.end_date >= prm.pstart${
    o.dim ? ` AND (prm.dimv IS NULL OR r.${o.dim} = prm.dimv)` : ''
  }
  GROUP BY r.account_id)
SELECT a.code AS norek, a.name AS noreknama, g.bud AS anggaran,
  (CASE WHEN a.normal_balance = 'DEBIT' THEN g.dt - g.ct ELSE g.ct - g.dt END) AS saldo${dimSel}
FROM agg g JOIN md_accounts a ON a.id = g.aid${dimFrom}) s`,
    select: {
      nmnorek: 's.norek',
      nmnoreknama: 's.noreknama',
      nmanggaran: 's.anggaran',
      nmsaldo: 's.saldo',
      nmvariasi: '(s.anggaran - s.saldo)',
      nmf1: 'NULL',
      nmf2: 'NULL',
      nmf3: 'NULL',
      nmf4: 'NULL',
      ...dimCols,
    },
    bindParams: o.dim
      ? [...BIND_PERIOD, { name: DIM_PARAM[o.dim], kind: 'number' as const }]
      : BIND_PERIOD,
    orderBy: 's.norek',
  };
};

/** Laporan arus kas metode tidak langsung dari mutasi ledger non-kas. */
const arusKasConfig = (): FinDatasetConfig => ({
  from: `(
WITH RECURSIVE ${SUB_CTE},
${CASHACC_CTE},
${PRM_MONTH},
pre AS (
  SELECT l.account_id AS aid,
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS mv
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date < prm.pstart
    AND l.account_id IN (SELECT aid FROM cashacc)
  GROUP BY l.account_id),
opencash AS (
  SELECT COALESCE(SUM(a.opening_balance + COALESCE(p.mv, 0)), 0) AS v
  FROM md_accounts a LEFT JOIN pre p ON p.aid = a.id
  WHERE a.id IN (SELECT aid FROM cashacc)),
mv AS (
  SELECT a.id, a.code, a.name, a.type, a.cash_flow_category AS cat,
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS bal
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date >= prm.pstart AND l.entry_date <= prm.pend
    AND a.kind = 'POSTABLE' AND a.id NOT IN (SELECT aid FROM cashacc)
  GROUP BY a.id, a.code, a.name, a.type, a.cash_flow_category)
SELECT m.code AS norek, m.name AS noreknama,
  (CASE WHEN m.cat = 'OPERATING' THEN 'Aktivitas Operasi'
        WHEN m.cat = 'INVESTING' THEN 'Aktivitas Investasi'
        WHEN m.cat = 'FINANCING' THEN 'Aktivitas Pendanaan'
        WHEN m.type = 'EQUITY' THEN 'Aktivitas Pendanaan'
        ELSE 'Aktivitas Operasi' END) AS tipe,
  (CASE WHEN m.type IN ('ASSET', 'EXPENSE') THEN -m.bal ELSE m.bal END) AS saldoarus,
  (SELECT v FROM opencash) AS saldoawalkas,
  EXTRACT(MONTH FROM prm.pend)::int AS bulan, EXTRACT(YEAR FROM prm.pend)::int AS tahun
FROM mv m, prm
WHERE m.bal <> 0) s`,
  select: {
    pknorek: 's.norek',
    pknoreknama: 's.noreknama',
    pktipearuskas: 's.tipe',
    pksaldoaruskas: 's.saldoarus',
    pksaldoawalkas: 's.saldoawalkas',
    pkbulan: 's.bulan',
    pktahun: 's.tahun',
  },
  bindParams: BIND_PERIOD,
  orderBy: 's.tipe, s.norek',
});

const mutasiKeuanganConfig = (): FinDatasetConfig => ({
  from: `(
WITH ${PRM_MONTH},
mv AS (
  SELECT l.account_id AS aid,
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS bal
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id, prm
  WHERE l.deleted_at IS NULL AND l.entry_date >= prm.pstart AND l.entry_date <= prm.pend
  GROUP BY l.account_id)
SELECT a.name AS akunnama, COALESCE(m.bal, 0) AS saldo,
  (CASE WHEN a.is_active THEN 1 ELSE 0 END) AS status,
  (CASE WHEN a.is_active THEN 'Aktif' ELSE 'Nonaktif' END) AS statusnama
FROM md_accounts a LEFT JOIN mv m ON m.aid = a.id
WHERE a.deleted_at IS NULL AND a.kind = 'POSTABLE' AND COALESCE(m.bal, 0) <> 0) s`,
  select: {
    mkakunnama: 's.akunnama',
    mksaldo: 's.saldo',
    mkstatus: 's.status',
    mkstatusnama: 's.statusnama',
  },
  bindParams: BIND_PERIOD,
  orderBy: 's.akunnama',
});

const dailyBankConfig = (): FinDatasetConfig => ({
  from: `(
WITH RECURSIVE ${SUB_CTE},
${CASHACC_CTE},
bal AS (
  SELECT l.account_id AS aid,
    SUM(CASE WHEN a.normal_balance = 'DEBIT' THEN l.debit - l.credit ELSE l.credit - l.debit END) AS mv
  FROM fin_ledger_entries l JOIN md_accounts a ON a.id = l.account_id
  WHERE l.deleted_at IS NULL GROUP BY l.account_id)
SELECT a.code || ' - ' || a.name AS uraian,
  a.opening_balance + COALESCE(b.mv, 0) AS saldo,
  COALESCE(cur.code, 'IDR') AS matauang
FROM md_accounts a
LEFT JOIN bal b ON b.aid = a.id
LEFT JOIN md_currencies cur ON cur.id = a.currency_id
WHERE a.deleted_at IS NULL AND a.id IN (SELECT aid FROM cashacc)) s`,
  select: { uraian: 's.uraian', saldo: 's.saldo', matauang: 's.matauang' },
  orderBy: 's.uraian',
});

/** neracat2: pasangan T hanya level<=2, nilai = saldo realisasi kumulatif. */
const neracaT2Config = (): FinDatasetConfig => {
  const o: PkOpts = {
    types: ['ASSET', 'LIABILITY', 'EQUITY'],
    mode: 'balance',
    niRow: true,
    maxLevel: 2,
  };
  return {
    from: `(
${pkCtes(o)}
SELECT a.norek AS anorek, a.noreknama AS anoreknama, a.saldo AS asaldo,
  p.norek AS pnorek, p.noreknama AS pnoreknama, p.saldo AS psaldo,
  COALESCE(a.rn, p.rn) AS rn
FROM (SELECT s.code AS norek, s.name AS noreknama,
        (CASE WHEN s.nb = 'DEBIT' THEN s.rd - s.rc ELSE s.rc - s.rd END) AS saldo,
        ROW_NUMBER() OVER (ORDER BY s.code) AS rn FROM rows0 s WHERE s.type = 'ASSET') a
FULL OUTER JOIN (SELECT s.code AS norek, s.name AS noreknama,
        (CASE WHEN s.nb = 'DEBIT' THEN s.rd - s.rc ELSE s.rc - s.rd END) AS saldo,
        ROW_NUMBER() OVER (ORDER BY s.code) AS rn FROM rows0 s WHERE s.type IN ('LIABILITY', 'EQUITY')) p
  ON p.rn = a.rn) s`,
    select: {
      anorek: 's.anorek',
      anoreknama: 's.anoreknama',
      aleveldata: '5',
      ajmlrealisasi: 's.asaldo',
      pnorek: 's.pnorek',
      pnoreknama: 's.pnoreknama',
      pleveldata: '5',
      pjmlrealisasi: 's.psaldo',
    },
    bindParams: BIND_PERIOD,
    orderBy: 's.rn',
  };
};

export const FIN_G3_STATEMENT2_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.neracamutasi': { DS1: nmConfig({}) },
  'fin.neracamutasicabang': { DS1: nmConfig({ dim: 'branch_id' }) },
  'fin.neracamutasilokasi': { DS1: nmConfig({ dim: 'location_id' }) },
  'fin.kasharianglobal': { DS1: nmConfig({ cashBankOnly: true }) },
  'fin.bankharianglobal': { DS1: nmConfig({ cashBankOnly: true }) },
  'fin.anggarandanrealisasiglobal': { DS1: anggaranConfig({}) },
  'fin.anggarandanrealisasicabang': { DS1: anggaranConfig({ dim: 'branch_id' }) },
  'fin.anggarandanrealisasilokasi': { DS1: anggaranConfig({ dim: 'location_id' }) },
  'fin.anggarandanrealisasidivisi': { DS1: anggaranConfig({ dim: 'division_id' }) },
  'fin.anggarandanrealisasiproyek': { DS1: anggaranConfig({ dim: 'project_id' }) },
  'fin.anggarandanrealisasicostcenter': { DS1: anggaranConfig({ dim: 'cost_center_id' }) },
  'fin.laporanaruskas': { DS1: arusKasConfig() },
  'fin.mutasikeuangan': { DS1: mutasiKeuanganConfig() },
  'fin.dailybank': { DS1: dailyBankConfig() },
  'fin.neracat2': { DS1: neracaT2Config() },
};
