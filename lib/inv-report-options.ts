/**
 * Per-report UI affordances for the generic Warehouse (M3) report page. The
 * report KEY = the seeded `/warehouse/reports/:key` path segment. Anything not
 * listed falls back to sensible defaults (date-range filter, no item picker).
 * The real title/columns come from the backend dataset; `title` here is only a
 * pre-load fallback for the page header.
 */

export interface InvReportOption {
  title?: string;
  /** Show an "as of date" filter instead of a date range. */
  asOfMode?: boolean;
  /** Show the item picker (required by per-item reports like the stock card). */
  showItem?: boolean;
  /** Workflow status filter options (transaction reports only). */
  statusOptions?: { label: string; value: string }[];
}

const WORKFLOW_STATUS: { label: string; value: string }[] = [
  { label: 'Draft', value: 'DRAFT' },
  { label: 'Need Approve', value: 'NEED_APPROVE' },
  { label: 'Approved', value: 'APPROVED' },
  { label: 'Rejected', value: 'REJECTED' },
  { label: 'Posted', value: 'POSTED' },
];

/** Transaction reports share the workflow status filter. */
const TXN_KEYS = [
  'material-requests',
  'transfers',
  'transfer-receipts',
  'fuel-refills',
  'returns',
  'stock-counts',
  'stock-adjustments',
  'price-adjustments',
  'opening-stocks',
  'daily-checks',
  'receipt-weighers',
];

const OPTIONS: Record<string, InvReportOption> = {
  // Stock aggregations
  stock: { title: 'Stock (Saldo)', asOfMode: true },
  'stock-cards': { title: 'Stock Card (Kartu Stok)', showItem: true },
  'stock-mutations': { title: 'Stock Mutation (Mutasi Stok)' },
  'below-minimum': { title: 'Below Minimum Stock' },
  'daily-stock': { title: 'Daily Available Stock', showItem: true },
  'cogs-balance': { title: 'COGS Balance' },
  'stock-minus': { title: 'Stock Minus' },
  consignment: { title: 'Consignment Summary' },
  // Item tracking
  'batch-items': { title: 'Batch Items' },
  'batch-cards': { title: 'Batch Item Cards' },
  'serial-items': { title: 'Serial Items' },
  'serial-cards': { title: 'Serial Item Cards' },
};

for (const key of TXN_KEYS) {
  OPTIONS[key] = { ...(OPTIONS[key] ?? {}), statusOptions: WORKFLOW_STATUS };
}

/** Resolve the UI options for a report key (defaults when unlisted). */
export function invReportOptions(key: string): InvReportOption {
  return OPTIONS[key] ?? {};
}
