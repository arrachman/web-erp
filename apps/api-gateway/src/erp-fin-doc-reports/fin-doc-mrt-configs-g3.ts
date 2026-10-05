/**
 * G3 — fragmen SQL bersama untuk builder laporan FIN statements/cards/lists.
 *
 * Konvensi (lihat juga fin-doc-mrt-configs.ts):
 * - Placeholder `?` HANYA di from/where/groupBy/orderBy (bindParams, urutan
 *   jalan = urutan teks SQL akhir). Kolom select tidak boleh memuat `?`
 *   karena renderer mengunjungi kolom sesuai urutan kamus template.
 * - Periode default: period_start = awal bulan period_end; period_end =
 *   CURRENT_DATE, kecuali dinyatakan lain per keluarga laporan.
 */
import { FinBindParam } from './fin-doc-mrt-configs';

/** Param tanggal standar: period_end lalu period_start (urutan bind). */
export const BIND_PERIOD: FinBindParam[] = [
  { name: 'period_end', kind: 'date' },
  { name: 'period_start', kind: 'date' },
];

/**
 * CTE standar periode bulanan untuk laporan posisi keuangan / laba rugi:
 * bind: period_end, period_start. pstart default = awal bulan pend;
 * pendprev = akhir bulan sebelum pend; pstartprev = pstart − 1 bulan;
 * pendprevwin = pend − 1 bulan (jendela bulan lalu untuk laba rugi).
 */
export const PRM_MONTH = `prm AS (
  SELECT pend, COALESCE(ps0, date_trunc('month', pend)::date) AS pstart,
    (date_trunc('month', pend) - INTERVAL '1 day')::date AS pendprev,
    ((COALESCE(ps0, date_trunc('month', pend)::date)) - INTERVAL '1 month')::date AS pstartprev,
    (pend - INTERVAL '1 month')::date AS pendprevwin
  FROM (SELECT COALESCE(?::date, CURRENT_DATE) AS pend, ?::date AS ps0) z)`;

/** Varian PRM_MONTH dengan bind dimensi ketiga (?::bigint AS dimv). */
export const PRM_DIM = `prm AS (
  SELECT pend, COALESCE(ps0, date_trunc('month', pend)::date) AS pstart,
    (date_trunc('month', pend) - INTERVAL '1 day')::date AS pendprev,
    ((COALESCE(ps0, date_trunc('month', pend)::date)) - INTERVAL '1 month')::date AS pstartprev,
    (pend - INTERVAL '1 month')::date AS pendprevwin, dimv
  FROM (SELECT COALESCE(?::date, CURRENT_DATE) AS pend, ?::date AS ps0, ?::bigint AS dimv) z)`;

/** CTE akar→turunan untuk rollup akun (semua akun, termasuk dirinya). */
export const SUB_CTE = `sub(root_id, desc_id) AS (
  SELECT id, id FROM md_accounts WHERE deleted_at IS NULL
  UNION ALL
  SELECT s.root_id, a.id FROM sub s
  JOIN md_accounts a ON a.parent_id = s.desc_id AND a.deleted_at IS NULL)`;

/** Level 1..5: kode leluhur per kedalaman (parent join berantai). */
export const ANCESTOR_SELECT = `
  CASE WHEN a.level <= 1 THEN a.code ELSE a1.code END AS lv1,
  CASE WHEN a.level <= 2 THEN a.code ELSE a2.code END AS lv2,
  CASE WHEN a.level <= 3 THEN a.code ELSE a3.code END AS lv3,
  CASE WHEN a.level <= 4 THEN a.code ELSE a4.code END AS lv4,
  CASE WHEN a.level <= 5 THEN a.code ELSE NULL END AS lv5`;

export const ANCESTOR_JOINS = `
  LEFT JOIN md_accounts a1 ON a1.id = a.parent_id
  LEFT JOIN md_accounts a2 ON a2.id = a1.parent_id
  LEFT JOIN md_accounts a3 ON a3.id = a2.parent_id
  LEFT JOIN md_accounts a4 ON a4.id = a3.parent_id`;

/** Kolom level kamus pk*: levelN = kode leluhur bila level akun >= N. */
export const pkLevelCols = {
  pklevel1: `(CASE WHEN s.level >= 1 THEN s.lv1 END)::text`,
  pklevel2: `(CASE WHEN s.level >= 2 THEN s.lv2 END)::text`,
  pklevel3: `(CASE WHEN s.level >= 3 THEN s.lv3 END)::text`,
  pklevel4: `(CASE WHEN s.level >= 4 THEN s.lv4 END)::text`,
  pklevel5: `(CASE WHEN s.level >= 5 THEN s.lv5 END)::text`,
};

/** Dimensi ledger yang didukung varian laporan (kolom fin_ledger_entries). */
export type LedgerDim =
  | 'branch_id'
  | 'location_id'
  | 'division_id'
  | 'project_id'
  | 'cost_center_id';

export const DIM_TABLE: Record<LedgerDim, string> = {
  branch_id: 'md_branches',
  location_id: 'md_locations',
  division_id: 'md_divisions',
  project_id: 'md_projects',
  cost_center_id: 'md_cost_centers',
};

export const DIM_PARAM: Record<LedgerDim, string> = {
  branch_id: 'branch',
  location_id: 'location',
  division_id: 'division',
  project_id: 'project',
  cost_center_id: 'cost_center',
};

/** Awalan kolom dimensi pada staging legacy (keluarga pk dan nm). */
export const DIM_PREFIX: Record<LedgerDim, string> = {
  branch_id: 'cabang',
  location_id: 'lokasi',
  division_id: 'divisi',
  project_id: 'proyek',
  cost_center_id: 'costcenter',
};
