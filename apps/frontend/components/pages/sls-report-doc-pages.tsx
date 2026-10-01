'use client';

import { SlsReportPage } from './sls-report-page';

export function SlsRptQuotationsPage() {
  return <SlsReportPage reportKey="quotations" title="Sales Quotation (SQ)" />;
}

export function SlsRptOrdersPage() {
  return <SlsReportPage reportKey="orders" title="Sales Order (SO)" />;
}

export function SlsRptCustomerAdvancesPage() {
  return <SlsReportPage reportKey="customer-advances" title="Customer Advance (AS)" />;
}

export function SlsRptPaymentReceiptsPage() {
  return <SlsReportPage reportKey="payment-receipts" title="Payment Receipt (IP)" />;
}

export function SlsRptProformaInvoicesPage() {
  return <SlsReportPage reportKey="proforma-invoices" title="Proforma Invoice (PI)" />;
}

export function SlsRptPackingListsPage() {
  return <SlsReportPage reportKey="packing-lists" title="Packing List (PL)" />;
}

export function SlsRptDeliveryOrdersPage() {
  return <SlsReportPage reportKey="delivery-orders" title="Delivery Order (DO)" />;
}

export function SlsRptDeliveryReportsPage() {
  return <SlsReportPage reportKey="delivery-reports" title="Delivery Report (DR)" />;
}

export function SlsRptInvoicesPage() {
  return <SlsReportPage reportKey="invoices" title="Sales Invoice (SI)" />;
}

export function SlsRptFreightReceivablesPage() {
  return <SlsReportPage reportKey="freight-receivables" title="Freight Receivable (RP)" />;
}

export function SlsRptReturnReceiptsPage() {
  return <SlsReportPage reportKey="return-receipts" title="Return Receipt (RNR)" />;
}

export function SlsRptReturnsPage() {
  return <SlsReportPage reportKey="returns" title="Sales Return (SR)" />;
}

export function SlsRptArCollectionsPage() {
  return <SlsReportPage reportKey="ar-collections" title="AR Collection (IC)" />;
}

export function SlsRptArPaymentsPage() {
  return <SlsReportPage reportKey="ar-payments" title="AR Payment (PV)" />;
}

export function SlsRptInvoiceSwapsPage() {
  return <SlsReportPage reportKey="invoice-swaps" title="Invoice Swap (SIE)" />;
}

export function SlsRptOpeningArBalancePage() {
  return <SlsReportPage reportKey="opening-ar-balance" title="Opening AR Balance" />;
}
