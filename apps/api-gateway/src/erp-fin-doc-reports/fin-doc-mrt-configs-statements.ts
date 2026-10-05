/**
 * G3 — Posisi keuangan & laba rugi (staging legacy m2r_posisi_keuangan, kolom pk*).
 *
 * Semantik mengikuti endpoint erp-fin-reports yang sudah live:
 * - Mode balance (neraca): saldo kumulatif s.d. period_end, sisi normal akun,
 *   opening_balance md_accounts terlipat di sisi normalnya, header = rollup
 *   seluruh turunan postable; akun tanpa aktivitas & tanpa saldo awal diskip.
 *   Baris sintetis "Laba/(Rugi) Tahun Berjalan" ditambahkan di grup ekuitas
 *   agar neraca tercetak balance (sama seperti balance-sheet endpoint).
 * - Mode movement (laba rugi): mutasi jendela [period_start, period_end]
 *   sisi normal; kolom *lalu = jendela bulan sebelumnya.
 * Placeholder `?` hanya di CTE prm (bind: period_end, period_start, [dimensi]).
 */
import { FinDatasetConfig } from './fin-doc-mrt-configs';
import {
  ANCESTOR_JOINS,
  ANCESTOR_SELECT,
  DIM_PARAM,
  DIM_TABLE,
  LedgerDim,
  PRM_DIM,
  PRM_MONTH,
  SUB_CTE,
  pkLevelCols,
} from './fin-doc-mrt-configs-g3';

export interface PkOpts {
  types: string[];
  mode: 'balance' | 'movement';
  dim?: LedgerDim;
  dimCols?: Record<string, string>;
  niRow?: boolean;
  maxLevel?: number;
}

/** CTE bersama pk: ledger agregat + rollup turunan + baris per akun. */
export const pkCtes = (o: PkOpts): string => {
  const dimCond = o.dim
    ? ` AND (prm.dimv IS NULL OR l.${o.dim} = prm.dimv)`
    : '';
  const rollD =
    o.mode === 'balance'
      ? `SUM(p.d_cur + CASE WHEN p.nb = 'DEBIT' THEN p.ob ELSE 0 END)`
      : `SUM(p.d_w)`;
  const rollC =
    o.mode === 'balance'
      ? `SUM(p.c_cur + CASE WHEN p.nb = 'CREDIT' THEN p.ob ELSE 0 END)`
      : `SUM(p.c_w)`;
  const rollDp =
    o.mode === 'balance'
      ? `SUM(p.d_pv + CASE WHEN p.nb = 'DEBIT' THEN p.ob ELSE 0 END)`
      : `SUM(p.d_wp)`;
  const rollCp =
    o.mode === 'balance'
      ? `SUM(p.c_pv + CASE WHEN p.nb = 'CREDIT' THEN p.ob ELSE 0 END)`
      : `SUM(p.c_wp)`;
  const act =
    o.mode === 'balance'
      ? `SUM(p.d_cur + p.c_cur + ABS(p.ob))`
      : `SUM(p.d_w + p.c_w + p.d_wp + p.c_wp)`;
  const niCte = o.niRow
    ? `,
ni AS (
  SELECT
    SUM(CASE WHEN a.type = 'REVENUE' THEN COALESCE(x.c_cur, 0) - COALESCE(x.d_cur, 0) ELSE 0 END)
      - SUM(CASE WHEN a.type = 'EXPENSE' THEN COALESCE(x.d_cur, 0) - COALESCE(x.c_cur, 0) ELSE 0 END) AS ni_cur,
    SUM(CASE WHEN a.type = 'REVENUE' THEN COALESCE(x.c_pv, 0) - COALESCE(x.d_pv, 0) ELSE 0 END)
      - SUM(CASE WHEN a.type = 'EXPENSE' THEN COALESCE(x.d_pv, 0) - COALESCE(x.c_pv, 0) ELSE 0 END) AS ni_pv
  FROM md_accounts a LEFT JOIN led x ON x.aid = a.id
  WHERE a.deleted_at IS NULL AND a.kind = 'POSTABLE' AND a.type IN ('REVENUE', 'EXPENSE'))`
    : '';
  const niUnion = o.niRow
    ? `
  UNION ALL
  SELECT eq.id, eq.code, 'Laba/(Rugi) Tahun Berjalan', 'EQUITY', 'POSTABLE', 2, 'CREDIT',
    eq.code, NULL, NULL, NULL, NULL,
    0::numeric, (SELECT COALESCE(ni_cur, 0) FROM ni), 0::numeric, (SELECT COALESCE(ni_pv, 0) FROM ni), 1::numeric
  FROM (SELECT id, code FROM md_accounts
        WHERE type = 'EQUITY' AND level = 1 AND deleted_at IS NULL
        ORDER BY code LIMIT 1) eq`
    : '';
  return `WITH RECURSIVE ${SUB_CTE},
${o.dim ? PRM_DIM : PRM_MONTH},
led AS (
  SELECT l.account_id AS aid,
    SUM(CASE WHEN l.entry_date <= prm.pend THEN l.debit ELSE 0 END) AS d_cur,
    SUM(CASE WHEN l.entry_date <= prm.pend THEN l.credit ELSE 0 END) AS c_cur,
    SUM(CASE WHEN l.entry_date <= prm.pendprev THEN l.debit ELSE 0 END) AS d_pv,
    SUM(CASE WHEN l.entry_date <= prm.pendprev THEN l.credit ELSE 0 END) AS c_pv,
    SUM(CASE WHEN l.entry_date >= prm.pstart AND l.entry_date <= prm.pend THEN l.debit ELSE 0 END) AS d_w,
    SUM(CASE WHEN l.entry_date >= prm.pstart AND l.entry_date <= prm.pend THEN l.credit ELSE 0 END) AS c_w,
    SUM(CASE WHEN l.entry_date >= prm.pstartprev AND l.entry_date <= prm.pendprevwin THEN l.debit ELSE 0 END) AS d_wp,
    SUM(CASE WHEN l.entry_date >= prm.pstartprev AND l.entry_date <= prm.pendprevwin THEN l.credit ELSE 0 END) AS c_wp
  FROM fin_ledger_entries l, prm
  WHERE l.deleted_at IS NULL${dimCond}
  GROUP BY l.account_id),
per AS (
  SELECT a.id, a.normal_balance AS nb, a.opening_balance AS ob,
    COALESCE(x.d_cur, 0) AS d_cur, COALESCE(x.c_cur, 0) AS c_cur,
    COALESCE(x.d_pv, 0) AS d_pv, COALESCE(x.c_pv, 0) AS c_pv,
    COALESCE(x.d_w, 0) AS d_w, COALESCE(x.c_w, 0) AS c_w,
    COALESCE(x.d_wp, 0) AS d_wp, COALESCE(x.c_wp, 0) AS c_wp
  FROM md_accounts a LEFT JOIN led x ON x.aid = a.id
  WHERE a.deleted_at IS NULL AND a.kind = 'POSTABLE'),
roll AS (
  SELECT s.root_id, ${rollD} AS rd, ${rollC} AS rc,
    ${rollDp} AS rdp, ${rollCp} AS rcp, ${act} AS act
  FROM sub s JOIN per p ON p.id = s.desc_id GROUP BY s.root_id)${niCte},
rows0 AS (
  SELECT a.id, a.code, a.name, a.type, a.kind, a.level, a.normal_balance AS nb,
    ${ANCESTOR_SELECT},
    COALESCE(r.rd, 0) AS rd, COALESCE(r.rc, 0) AS rc,
    COALESCE(r.rdp, 0) AS rdp, COALESCE(r.rcp, 0) AS rcp, COALESCE(r.act, 0) AS act
  FROM md_accounts a ${ANCESTOR_JOINS}
  LEFT JOIN roll r ON r.root_id = a.id
  WHERE a.deleted_at IS NULL AND a.type IN (${o.types.map((t) => `'${t}'`).join(', ')})
    AND COALESCE(r.act, 0) > 0${o.maxLevel ? ` AND a.level <= ${o.maxLevel}` : ''}${niUnion})`;
};

/** SELECT akhir derived pk: kolom netral + bulan/tahun + dimensi opsional. */
export const pkDerived = (o: PkOpts): string => {
  const dimSel = o.dim
    ? `, prm.dimv AS dimid, dt.name AS dimnama FROM rows0 s, prm LEFT JOIN ${DIM_TABLE[o.dim]} dt ON dt.id = prm.dimv`
    : ` FROM rows0 s, prm`;
  return `(
${pkCtes(o)}
SELECT s.code AS norek, s.name AS noreknama, s.type AS tipe, s.type AS jenis,
  s.kind, s.level, s.nb, s.lv1, s.lv2, s.lv3, s.lv4, s.lv5,
  s.rd, s.rc, s.rdp, s.rcp,
  (CASE WHEN s.nb = 'DEBIT' THEN s.rd - s.rc ELSE s.rc - s.rd END) AS saldo,
  (CASE WHEN s.nb = 'DEBIT' THEN s.rdp - s.rcp ELSE s.rcp - s.rdp END) AS saldolalu,
  EXTRACT(MONTH FROM prm.pend)::int AS bulan, EXTRACT(YEAR FROM prm.pend)::int AS tahun${dimSel}) s`;
};

const pkBinds = (o: PkOpts) =>
  o.dim
    ? [
        { name: 'period_end', kind: 'date' as const },
        { name: 'period_start', kind: 'date' as const },
        { name: DIM_PARAM[o.dim], kind: 'number' as const },
      ]
    : [
        { name: 'period_end', kind: 'date' as const },
        { name: 'period_start', kind: 'date' as const },
      ];

export const pkConfig = (o: PkOpts): FinDatasetConfig => ({
  from: pkDerived(o),
  select: {
    pknorek: 's.norek',
    pknoreknama: 's.noreknama',
    pktipe: 's.tipe',
    pkjenis: 's.jenis',
    pkgd: `(CASE WHEN s.kind = 'POSTABLE' THEN 'D' ELSE 'G' END)::text`,
    pkgddata: `(CASE WHEN s.kind = 'POSTABLE' THEN 'D' ELSE 'G' END)::text`,
    pklevel: 's.level',
    pkleveldata: '5',
    ...pkLevelCols,
    pkdebit: 's.rd',
    pkkredit: 's.rc',
    pkdebitlalu: 's.rdp',
    pkkreditlalu: 's.rcp',
    pksaldo: 's.saldo',
    pksaldo2: 's.saldo',
    pksaldolalu: 's.saldolalu',
    pkbulan: 's.bulan',
    pktahun: 's.tahun',
    ...(o.dimCols ?? {}),
  },
  bindParams: pkBinds(o),
  orderBy: 's.norek',
});

const dimColPair = (prefix: string): Record<string, string> => ({
  [`pk${prefix}`]: 's.dimid',
  [`pk${prefix}nama`]: 's.dimnama',
});

/** T-form: dua daftar sejajar (aktiva | pasiva) dipasangkan ROW_NUMBER. */
const pkTConfig = (o: PkOpts): FinDatasetConfig => ({
  from: `(
${pkCtes(o)}
SELECT a.norek AS anorek, a.noreknama AS anoreknama, a.level AS alevel,
  a.saldo AS asaldo, a.saldo AS anilaisaldo,
  p.norek AS pnorek, p.noreknama AS pnoreknama, p.level AS plevel,
  p.saldo AS psaldo, p.saldo AS pnilaisaldo,
  COALESCE(a.rn, p.rn) AS rn
FROM (SELECT s.code AS norek, s.name AS noreknama, s.level, (CASE WHEN s.nb = 'DEBIT' THEN s.rd - s.rc ELSE s.rc - s.rd END) AS saldo, ROW_NUMBER() OVER (ORDER BY s.code) AS rn FROM rows0 s WHERE s.type = 'ASSET') a
FULL OUTER JOIN (SELECT s.code AS norek, s.name AS noreknama, s.level, (CASE WHEN s.nb = 'DEBIT' THEN s.rd - s.rc ELSE s.rc - s.rd END) AS saldo, ROW_NUMBER() OVER (ORDER BY s.code) AS rn FROM rows0 s WHERE s.type IN ('LIABILITY', 'EQUITY')) p
  ON p.rn = a.rn) s`,
  select: {
    anorek: 's.anorek',
    anoreknama: 's.anoreknama',
    alevel: 's.alevel',
    aleveldata: '5',
    agd: `(CASE WHEN s.anorek IS NULL THEN NULL ELSE 'D' END)::text`,
    asal: 's.asaldo',
    asaldo: 's.asaldo',
    anilaisaldo: 's.anilaisaldo',
    pnorek: 's.pnorek',
    pnoreknama: 's.pnoreknama',
    plevel: 's.plevel',
    pleveldata: '5',
    pgd: `(CASE WHEN s.pnorek IS NULL THEN NULL ELSE 'D' END)::text`,
    psaldo: 's.psaldo',
    pnilaisaldo: 's.pnilaisaldo',
  },
  bindParams: pkBinds(o),
  orderBy: 's.rn',
});

const BS: PkOpts = { types: ['ASSET', 'LIABILITY', 'EQUITY'], mode: 'balance', niRow: true };
const IS: PkOpts = { types: ['REVENUE', 'EXPENSE'], mode: 'movement' };

const dimPk = (base: PkOpts, dim: LedgerDim, prefix: string): PkOpts => ({
  ...base,
  dim,
  dimCols: dimColPair(prefix),
});

/** Laba rugi tahunan: pksaldo1..12 = mutasi sisi normal per bulan tahun berjalan. */
const yearConfig = (): FinDatasetConfig => ({
  from: `(
WITH RECURSIVE ${SUB_CTE},
yprm AS (SELECT COALESCE(EXTRACT(YEAR FROM ?::date), EXTRACT(YEAR FROM CURRENT_DATE))::int AS yr),
ledm AS (
  SELECT l.account_id AS aid, EXTRACT(MONTH FROM l.entry_date)::int AS mo,
    SUM(l.debit) AS d, SUM(l.credit) AS c
  FROM fin_ledger_entries l, yprm
  WHERE l.deleted_at IS NULL AND EXTRACT(YEAR FROM l.entry_date) = yprm.yr
  GROUP BY l.account_id, EXTRACT(MONTH FROM l.entry_date)),
per AS (
  SELECT a.id, ${Array.from({ length: 12 }, (_, i) => `SUM(CASE WHEN x.mo = ${i + 1} THEN (CASE WHEN a.normal_balance = 'DEBIT' THEN x.d - x.c ELSE x.c - x.d END) ELSE 0 END) AS m${i + 1}`).join(', ')},
    SUM(ABS(COALESCE(x.d, 0)) + ABS(COALESCE(x.c, 0))) AS act
  FROM md_accounts a LEFT JOIN ledm x ON x.aid = a.id
  WHERE a.deleted_at IS NULL AND a.kind = 'POSTABLE'
  GROUP BY a.id, a.normal_balance),
roll AS (
  SELECT s.root_id, ${Array.from({ length: 12 }, (_, i) => `SUM(p.m${i + 1}) AS m${i + 1}`).join(', ')}, SUM(p.act) AS act
  FROM sub s JOIN per p ON p.id = s.desc_id GROUP BY s.root_id)
SELECT a.code AS norek, a.name AS noreknama, a.kind, a.level,
  ${Array.from({ length: 12 }, (_, i) => `COALESCE(r.m${i + 1}, 0) AS m${i + 1}`).join(', ')},
  yprm.yr AS tahun
FROM md_accounts a LEFT JOIN roll r ON r.root_id = a.id, yprm
WHERE a.deleted_at IS NULL AND a.type IN ('REVENUE', 'EXPENSE') AND COALESCE(r.act, 0) > 0) s`,
  select: {
    pknorek: 's.norek',
    pknoreknama: 's.noreknama',
    pkgd: `(CASE WHEN s.kind = 'POSTABLE' THEN 'D' ELSE 'G' END)::text`,
    pklevel: 's.level',
    pktahun: 's.tahun',
    ...Object.fromEntries(
      Array.from({ length: 12 }, (_, i) => [`pksaldo${i + 1}`, `s.m${i + 1}`]),
    ),
  },
  bindParams: [{ name: 'period_start', kind: 'date' }],
  orderBy: 's.norek',
});

/** Laba rugi per tahun: pksaldo1 = tahun berjalan, pksaldo2 = tahun lalu. */
const perYearConfig = (): FinDatasetConfig => ({
  from: `(
WITH RECURSIVE ${SUB_CTE},
yprm AS (SELECT COALESCE(EXTRACT(YEAR FROM ?::date), EXTRACT(YEAR FROM CURRENT_DATE))::int AS yr),
ledy AS (
  SELECT l.account_id AS aid, EXTRACT(YEAR FROM l.entry_date)::int AS yy,
    SUM(l.debit) AS d, SUM(l.credit) AS c
  FROM fin_ledger_entries l, yprm
  WHERE l.deleted_at IS NULL AND EXTRACT(YEAR FROM l.entry_date) IN (yprm.yr, yprm.yr - 1)
  GROUP BY l.account_id, EXTRACT(YEAR FROM l.entry_date)),
per AS (
  SELECT a.id,
    SUM(CASE WHEN x.yy = (SELECT yr FROM yprm) THEN (CASE WHEN a.normal_balance = 'DEBIT' THEN x.d - x.c ELSE x.c - x.d END) ELSE 0 END) AS y1,
    SUM(CASE WHEN x.yy = (SELECT yr FROM yprm) - 1 THEN (CASE WHEN a.normal_balance = 'DEBIT' THEN x.d - x.c ELSE x.c - x.d END) ELSE 0 END) AS y2,
    SUM(ABS(COALESCE(x.d, 0)) + ABS(COALESCE(x.c, 0))) AS act
  FROM md_accounts a LEFT JOIN ledy x ON x.aid = a.id
  WHERE a.deleted_at IS NULL AND a.kind = 'POSTABLE'
  GROUP BY a.id, a.normal_balance),
roll AS (
  SELECT s.root_id, SUM(p.y1) AS y1, SUM(p.y2) AS y2, SUM(p.act) AS act
  FROM sub s JOIN per p ON p.id = s.desc_id GROUP BY s.root_id)
SELECT a.code AS norek, a.name AS noreknama, COALESCE(r.y1, 0) AS s1, COALESCE(r.y2, 0) AS s2
FROM md_accounts a LEFT JOIN roll r ON r.root_id = a.id
WHERE a.deleted_at IS NULL AND a.type IN ('REVENUE', 'EXPENSE') AND COALESCE(r.act, 0) > 0) s`,
  select: { pknorek: 's.norek', pknoreknama: 's.noreknama', pksaldo1: 's.s1', pksaldo2: 's.s2' },
  bindParams: [{ name: 'period_start', kind: 'date' }],
  orderBy: 's.norek',
});

export const FIN_G3_STATEMENT_CONFIGS: Record<string, Record<string, FinDatasetConfig>> = {
  'fin.posisikeuangan': { DS1: pkConfig(BS) },
  'fin.posisikeuangantanpaakun': { DS1: pkConfig(BS) },
  'fin.labarugi': { DS1: pkConfig(IS) },
  'fin.labarugianggaran': { DS1: pkConfig(IS) },
  'fin.labarugitanpaakun': { DS1: pkConfig(IS) },
  'fin.posisikeuangancabang': { DS1: pkConfig(dimPk(BS, 'branch_id', 'cabang')) },
  'fin.posisikeuanganlokasi': { DS1: pkConfig(dimPk(BS, 'location_id', 'lokasi')) },
  'fin.posisikeuangandivisi': { DS1: pkConfig(dimPk(BS, 'division_id', 'divisi')) },
  'fin.posisikeuanganproyek': { DS1: pkConfig(dimPk(BS, 'project_id', 'proyek')) },
  'fin.posisikeuangancostcenter': { DS1: pkConfig(dimPk(BS, 'cost_center_id', 'costcenter')) },
  'fin.labarugicabang': { DS1: pkConfig(dimPk(IS, 'branch_id', 'cabang')) },
  'fin.labarugilokasi': { DS1: pkConfig(dimPk(IS, 'location_id', 'lokasi')) },
  'fin.labarugidivisi': { DS1: pkConfig(dimPk(IS, 'division_id', 'divisi')) },
  'fin.labarugiproyek': { DS1: pkConfig(dimPk(IS, 'project_id', 'proyek')) },
  'fin.labarugicostcenter': { DS1: pkConfig(dimPk(IS, 'cost_center_id', 'costcenter')) },
  'fin.posisikeuangant': { DS1: pkTConfig(BS) },
  'fin.posisikeuanganttanpaakun': { DS1: pkTConfig(BS) },
  'fin.neracat': { DS1: pkTConfig(BS) },
  'fin.posisikeuangancabangt': { DS1: pkTConfig(dimPk(BS, 'branch_id', 'cabang')) },
  'fin.posisikeuanganlokasit': { DS1: pkTConfig(dimPk(BS, 'location_id', 'lokasi')) },
  'fin.posisikeuangandivisit': { DS1: pkTConfig(dimPk(BS, 'division_id', 'divisi')) },
  'fin.posisikeuanganproyekt': { DS1: pkTConfig(dimPk(BS, 'project_id', 'proyek')) },
  'fin.posisikeuangancostcentert': { DS1: pkTConfig(dimPk(BS, 'cost_center_id', 'costcenter')) },
  'fin.labarugitahun': { DS1: yearConfig() },
  'fin.labarugimultiperiode': { DS1: yearConfig() },
  'fin.labarugipertahun': { DS1: perYearConfig() },
};
