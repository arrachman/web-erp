/**
 * Finance (E1) report catalog — the list that drives the `/finance/reports` hub
 * page. Finance keeps its named-route + `ReportDocument` contract (unlike
 * inv/pur/sls which use a key-driven registry), so the catalog is a static
 * declaration rather than a derived registry listing.
 *
 * `route` is the frontend path for the report. It is NOT uniformly
 * `/finance/reports/<key>`: the statement pages live on flat paths
 * (`/finance/trial-balance`) and `general-ledger` is served at
 * `/finance/ledger`. Keeping the mapping here means the hub never has to guess.
 */

/** Grouping used by the hub page to section the report cards. */
export type FinReportGroup = 'statement' | 'subledger' | 'control';

export interface FinReportCatalogItem {
  /** Endpoint segment under `GET /erp/fin/reports/<key>`. */
  key: string;
  title: string;
  group: FinReportGroup;
  /** Frontend route that renders this report. */
  route: string;
  /** Period mode the report expects — drives the hub card hint. */
  period: 'range' | 'asof';
}

export const FIN_REPORT_CATALOG: FinReportCatalogItem[] = [
  // ── Laporan keuangan pokok ──────────────────────────────────────────────────
  {
    key: 'trial-balance',
    title: 'Neraca Saldo',
    group: 'statement',
    route: '/finance/trial-balance',
    period: 'range',
  },
  {
    key: 'income-statement',
    title: 'Laba Rugi',
    group: 'statement',
    route: '/finance/income-statement',
    period: 'range',
  },
  {
    key: 'balance-sheet',
    title: 'Neraca',
    group: 'statement',
    route: '/finance/balance-sheet',
    period: 'asof',
  },
  {
    key: 'movement-balance',
    title: 'Neraca Mutasi',
    group: 'statement',
    route: '/finance/movement-balance',
    period: 'range',
  },
  {
    key: 'equity-changes',
    title: 'Perubahan Modal',
    group: 'statement',
    route: '/finance/equity-changes',
    period: 'range',
  },
  {
    key: 'cash-flow',
    title: 'Arus Kas',
    group: 'statement',
    route: '/finance/cash-flow',
    period: 'range',
  },
  {
    key: 'general-ledger',
    title: 'Buku Besar',
    group: 'statement',
    route: '/finance/ledger',
    period: 'range',
  },

  // ── Subledger & buku pembantu ───────────────────────────────────────────────
  {
    key: 'daily-cash-bank',
    title: 'Kas & Bank Harian',
    group: 'subledger',
    route: '/finance/daily-cash-bank',
    period: 'range',
  },
  {
    key: 'ar-card',
    title: 'Kartu Piutang',
    group: 'subledger',
    route: '/finance/ar-card',
    period: 'range',
  },
  {
    key: 'ar-aging',
    title: 'Umur Piutang',
    group: 'subledger',
    route: '/finance/ar-aging',
    period: 'asof',
  },
  {
    key: 'ap-card',
    title: 'Kartu Utang',
    group: 'subledger',
    route: '/finance/ap-card',
    period: 'range',
  },
  {
    key: 'ap-aging',
    title: 'Umur Utang',
    group: 'subledger',
    route: '/finance/ap-aging',
    period: 'asof',
  },
  {
    key: 'giro-maturity',
    title: 'Jatuh Tempo Giro',
    group: 'subledger',
    route: '/finance/giro-maturity',
    period: 'range',
  },

  // ── Laporan kontrol & analisis ──────────────────────────────────────────────
  {
    key: 'budget-realization',
    title: 'Realisasi Anggaran',
    group: 'control',
    route: '/finance/budget-realization',
    period: 'range',
  },
  {
    key: 'profit-analysis',
    title: 'Analisis Laba',
    group: 'control',
    route: '/finance/profit-analysis',
    period: 'range',
  },
];

/**
 * Deliberately NOT in the catalog yet: `control-reconciliation`. The endpoint
 * exists (`GET /erp/fin/reports/control-reconciliation`) but returns its own
 * `ControlReconciliationReport` shape rather than `ReportDocument`, so it has
 * no page to render — listing it would produce a card that lands on
 * ComingSoon. Add it here together with its page.
 */

