/**
 * Per-report UI affordances for the generic Purchasing (M4) report page.
 * The report KEY = the seeded `/purchasing/reports/:key` path segment.
 * Anything not listed falls back to sensible defaults (date-range filter,
 * no vendor/item picker). The real title/columns come from the backend
 * dataset; `title` here is only a pre-load fallback for the page header.
 */

export interface PurReportOption {
  title?: string;
  /** Show an "as of date" filter instead of a date range. */
  asOfMode?: boolean;
  /** Show the vendor picker. */
  showVendor?: boolean;
  /** Show the item picker (for per-item reports). */
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

const OPTIONS: Record<string, PurReportOption> = {
  'purchase-requisitions': { title: 'Purchase Requisition (PR)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'rfqs': { title: 'Request for Quotation (RFQ)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'bid-comparisons': { title: 'Bid Comparison (BS)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'purchase-orders': { title: 'Purchase Order (PO)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'vendor-advances': { title: 'Vendor Advance (AP)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'goods-receipts': { title: 'Goods Receipt (GRN)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'purchase-invoices': { title: 'Purchase Invoice (PI)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'freight-payables': { title: 'Freight Payable (PP)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'return-shipments': { title: 'Return Shipment (DNR)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'purchase-returns': { title: 'Purchase Return (PRT)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'payment-schedules': { title: 'Payment Schedule (VPP)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'vendor-payments': { title: 'Vendor Payment (VP)', showVendor: true, statusOptions: WORKFLOW_STATUS },
  'summary': { title: 'Purchases Summary' },
};

/** Resolve the UI options for a purchasing report key (defaults when unlisted). */
export function purReportOptions(key: string): PurReportOption {
  return OPTIONS[key] ?? {};
}
