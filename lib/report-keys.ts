/**
 * Canonical registry of report keys that a Report Designer template can bind to.
 *
 * A template's `reportKey` (`<module>.<report>`) ties it to a specific report; the
 * matching active template drives that report's PDF output (see DECISIONS.md
 * 2026-06-13 "Wiring reports → Report Designer templates"). Keep this list in sync
 * with the backend report definitions — it is the single source for the designer's
 * report-key picker.
 *
 * Finance is authoritative (pilot, Phase 2). Sales/Purchasing/Inventory keys are
 * added as those modules are wired (Phase 3).
 */

export interface ReportKeyOption {
  /** Namespaced binding value stored on the template. */
  value: string;
  /** Human label shown in the designer picker. */
  label: string;
  /** Owning report module domain. */
  module: 'fin' | 'sls' | 'pur' | 'inv';
}

const FINANCE_KEYS: ReportKeyOption[] = [
  { value: 'fin.general-ledger', label: 'General Ledger', module: 'fin' },
  { value: 'fin.trial-balance', label: 'Trial Balance', module: 'fin' },
  { value: 'fin.movement-balance', label: 'Neraca Mutasi', module: 'fin' },
  { value: 'fin.balance-sheet', label: 'Balance Sheet', module: 'fin' },
  { value: 'fin.income-statement', label: 'Income Statement', module: 'fin' },
  { value: 'fin.equity-changes', label: 'Equity Changes', module: 'fin' },
  { value: 'fin.cash-flow', label: 'Cash Flow', module: 'fin' },
  { value: 'fin.daily-cash-bank', label: 'Daily Cash & Bank', module: 'fin' },
  { value: 'fin.ar-card', label: 'AR Card', module: 'fin' },
  { value: 'fin.ar-aging', label: 'AR Aging', module: 'fin' },
  { value: 'fin.ap-card', label: 'AP Card', module: 'fin' },
  { value: 'fin.ap-aging', label: 'AP Aging', module: 'fin' },
  { value: 'fin.giro-maturity', label: 'Giro Maturity', module: 'fin' },
  { value: 'fin.budget-realization', label: 'Budget vs Realization', module: 'fin' },
];

// Curated main document/list reports per module. Reports not listed here still
// render via the module-default template (`<module>.__default`); binding a specific
// template just requires the key to appear in this picker.
const SALES_KEYS: ReportKeyOption[] = [
  { value: 'sls.orders', label: 'Sales Orders', module: 'sls' },
  { value: 'sls.quotations', label: 'Quotations', module: 'sls' },
  { value: 'sls.invoices', label: 'Sales Invoices', module: 'sls' },
  { value: 'sls.delivery-orders', label: 'Delivery Orders', module: 'sls' },
  { value: 'sls.returns', label: 'Sales Returns', module: 'sls' },
  { value: 'sls.proforma-invoices', label: 'Proforma Invoices', module: 'sls' },
  { value: 'sls.packing-lists', label: 'Packing Lists', module: 'sls' },
  { value: 'sls.payment-receipts', label: 'Payment Receipts', module: 'sls' },
  { value: 'sls.ar-collections', label: 'AR Collections', module: 'sls' },
];

const PURCHASING_KEYS: ReportKeyOption[] = [
  { value: 'pur.purchase-orders', label: 'Purchase Orders', module: 'pur' },
  { value: 'pur.purchase-requisitions', label: 'Purchase Requisitions', module: 'pur' },
  { value: 'pur.purchase-invoices', label: 'Purchase Invoices', module: 'pur' },
  { value: 'pur.goods-receipts', label: 'Goods Receipts', module: 'pur' },
  { value: 'pur.purchase-returns', label: 'Purchase Returns', module: 'pur' },
  { value: 'pur.return-shipments', label: 'Return Shipments', module: 'pur' },
  { value: 'pur.rfqs', label: 'RFQs', module: 'pur' },
  { value: 'pur.bid-comparisons', label: 'Bid Comparisons', module: 'pur' },
  { value: 'pur.vendor-payments', label: 'Vendor Payments', module: 'pur' },
];

const INVENTORY_KEYS: ReportKeyOption[] = [
  { value: 'inv.stock-cards', label: 'Stock Cards', module: 'inv' },
  { value: 'inv.stock-mutations', label: 'Stock Mutations', module: 'inv' },
  { value: 'inv.stock-adjustments', label: 'Stock Adjustments', module: 'inv' },
  { value: 'inv.stock-counts', label: 'Stock Counts', module: 'inv' },
  { value: 'inv.opening-stocks', label: 'Opening Stocks', module: 'inv' },
  { value: 'inv.daily-checks', label: 'Daily Checks', module: 'inv' },
  { value: 'inv.price-adjustments', label: 'Price Adjustments', module: 'inv' },
  { value: 'inv.below-minimum', label: 'Below Minimum Stock', module: 'inv' },
  { value: 'inv.consignment', label: 'Consignment', module: 'inv' },
];

export const REPORT_KEY_OPTIONS: ReportKeyOption[] = [
  ...FINANCE_KEYS,
  ...SALES_KEYS,
  ...PURCHASING_KEYS,
  ...INVENTORY_KEYS,
];

/** Options filtered to a module (all if omitted). */
export function reportKeyOptions(module?: string): ReportKeyOption[] {
  if (!module) return REPORT_KEY_OPTIONS;
  return REPORT_KEY_OPTIONS.filter((o) => o.module === module);
}

/** Human label for a stored reportKey (falls back to the raw value). */
export function reportKeyLabel(value: string | null | undefined): string {
  if (!value) return '';
  return REPORT_KEY_OPTIONS.find((o) => o.value === value)?.label ?? value;
}
