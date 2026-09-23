/**
 * Transaction form pages registry.
 * Keyed by canonical sys_menus.path. Each entry supports three URL shapes
 * (list / <base>/new / <base>/:id) via lib/trx-route.
 * Extracted from shell-route-renderer to keep both files under 400 lines.
 */

import { ErpCashReceiptsPage } from '@/components/pages/fin-cash-receipts-page';
import { ErpBankReceiptsPage } from '@/components/pages/fin-bank-receipts-page';
import { ErpCashDisbursementsPage } from '@/components/pages/fin-cash-disbursements-page';
import { ErpBankDisbursementsPage } from '@/components/pages/fin-bank-disbursements-page';
import { ErpGeneralJournalsPage } from '@/components/pages/fin-general-journals-page';
import { ErpReceiptGirosPage } from '@/components/pages/fin-receipt-giros-page';
import { ErpSendGirosPage } from '@/components/pages/fin-send-giros-page';
import { ErpReceiptGiroClearingsPage } from '@/components/pages/fin-receipt-giro-clearings-page';
import { ErpSendGiroClearingsPage } from '@/components/pages/fin-send-giro-clearings-page';
import { ErpAdjustmentJournalsPage } from '@/components/pages/fin-adjustment-journals-page';
import { ErpMemorialJournalsPage } from '@/components/pages/fin-memorial-journals-page';
import { ErpOpeningBalancesPage } from '@/components/pages/fin-opening-balances-page';
import { ErpRevaluationsPage } from '@/components/pages/fin-revaluations-page';
import { ErpSlsOrdersPage } from '@/components/pages/sls-orders-page';
import { ErpSlsQuotationsPage } from '@/components/pages/sls-quotations-page';
import { ErpSlsProformaInvoicesPage } from '@/components/pages/sls-proforma-invoices-page';
import { ErpSlsPackingListsPage } from '@/components/pages/sls-packing-lists-page';
import { ErpSlsDeliveryOrdersPage } from '@/components/pages/sls-delivery-orders-page';
import { ErpSlsDeliveryReportsPage } from '@/components/pages/sls-delivery-reports-page';
import { ErpSlsInvoicesPage } from '@/components/pages/sls-invoices-page';
import { ErpSlsReturnsPage } from '@/components/pages/sls-returns-page';
import { ErpSlsReturnReceiptsPage } from '@/components/pages/sls-return-receipts-page';
import { ErpSlsCustomerAdvancesPage } from '@/components/pages/sls-customer-advances-page';
import { ErpSlsInvoiceSwapsPage } from '@/components/pages/sls-invoice-swaps-page';
import { ErpSlsPaymentReceiptsPage } from '@/components/pages/sls-payment-receipts-page';
import { ErpSlsArCollectionsPage } from '@/components/pages/sls-ar-collections-page';
import { ErpSlsArPaymentsPage } from '@/components/pages/sls-ar-payments-page';
import { ErpSlsOpeningArBalancePage } from '@/components/pages/sls-opening-ar-balance-page';
import { ErpSlsFreightReceivablesPage } from '@/components/pages/sls-freight-receivables-page';
import {
  ErpInvMaterialRequestsPage,
  ErpInvTransfersPage,
  ErpInvTransferReceiptsPage,
  ErpInvFuelRefillsPage,
} from '@/components/pages/inv-stock-movements-page';
import { ErpInvStockAdjustmentsPage } from '@/components/pages/inv-stock-adjustments-page';
import { ErpInvOpeningStocksPage } from '@/components/pages/inv-opening-stocks-page';
import { ErpInvStockCountsPage } from '@/components/pages/inv-stock-counts-page';
import { ErpInvPriceAdjustmentsPage } from '@/components/pages/inv-price-adjustments-page';
import { ErpInvWeighbridgeTicketsPage } from '@/components/pages/inv-weighbridge-tickets-page';
import { ErpInvDailyChecksPage } from '@/components/pages/inv-daily-checks-page';
import { ErpPurOrdersPage } from '@/components/pages/pur-orders-page';
import { ErpPurRequisitionsPage } from '@/components/pages/pur-requisitions-page';
import { ErpPurInvoicesPage } from '@/components/pages/pur-invoices-page';
import { ErpReturnShipmentsPage } from '@/components/pages/pur-return-shipments-page';
import { ErpPurchaseReturnsPage } from '@/components/pages/pur-purchase-returns-page';
import { ErpGoodsReceiptsPage } from '@/components/pages/pur-goods-receipts-page';
import { ErpRfqsPage } from '@/components/pages/pur-rfqs-page';
import { ErpBidSelectionsPage } from '@/components/pages/pur-bid-selections-page';
import { ErpVendorAdvancesPage } from '@/components/pages/pur-vendor-advances-page';
import { ErpFreightPayablesPage } from '@/components/pages/pur-freight-payables-page';
import { ErpPaymentSchedulesPage } from '@/components/pages/pur-payment-schedules-page';
import { ErpVendorPaymentsPage } from '@/components/pages/pur-vendor-payments-page';
import { ErpOpeningApBalancePage } from '@/components/pages/pur-opening-ap-balance-page';
import { ErpMfgBomsPage } from '@/components/pages/mfg-boms-page';
import { MfgWorkOrdersPage } from '@/components/pages/mfg-work-orders-page';
import type { TrxFormPage } from '@/lib/trx-route';

export const TRX_FORM_PAGES: Record<string, TrxFormPage> = {
  '/finance/cash-receipts': ErpCashReceiptsPage,
  '/finance/cash-disbursements': ErpCashDisbursementsPage,
  '/finance/bank-receipts': ErpBankReceiptsPage,
  '/finance/bank-disbursements': ErpBankDisbursementsPage,
  '/finance/general-journals': ErpGeneralJournalsPage,
  '/finance/receipt-giros': ErpReceiptGirosPage,
  '/finance/send-giros': ErpSendGirosPage,
  '/finance/receipt-giro-clearings': ErpReceiptGiroClearingsPage,
  '/finance/send-giro-clearings': ErpSendGiroClearingsPage,
  '/finance/adjustment-journals': ErpAdjustmentJournalsPage,
  '/finance/memorial-journals': ErpMemorialJournalsPage,
  '/finance/opening-balances': ErpOpeningBalancesPage,
  '/finance/revaluations': ErpRevaluationsPage,
  '/sales/orders': ErpSlsOrdersPage,
  '/sales/quotations': ErpSlsQuotationsPage,
  '/sales/proforma-invoices': ErpSlsProformaInvoicesPage,
  '/sales/packing-lists': ErpSlsPackingListsPage,
  '/sales/delivery-orders': ErpSlsDeliveryOrdersPage,
  '/sales/delivery-reports': ErpSlsDeliveryReportsPage,
  '/sales/invoices': ErpSlsInvoicesPage,
  '/sales/returns': ErpSlsReturnsPage,
  '/sales/return-receipts': ErpSlsReturnReceiptsPage,
  '/sales/customer-advances': ErpSlsCustomerAdvancesPage,
  '/sales/invoice-swaps': ErpSlsInvoiceSwapsPage,
  '/sales/payment-receipts': ErpSlsPaymentReceiptsPage,
  '/sales/ar-collections': ErpSlsArCollectionsPage,
  '/sales/ar-payments': ErpSlsArPaymentsPage,
  '/sales/opening-ar-balance': ErpSlsOpeningArBalancePage,
  '/sales/freight-receivables': ErpSlsFreightReceivablesPage,
  '/warehouse/material-requests': ErpInvMaterialRequestsPage,
  '/warehouse/transfers': ErpInvTransfersPage,
  '/warehouse/transfer-receipts': ErpInvTransferReceiptsPage,
  '/warehouse/fuel-refills': ErpInvFuelRefillsPage,
  '/warehouse/stock-adjustments': ErpInvStockAdjustmentsPage,
  '/warehouse/opening-stocks': ErpInvOpeningStocksPage,
  '/warehouse/stock-counts': ErpInvStockCountsPage,
  '/warehouse/price-adjustments': ErpInvPriceAdjustmentsPage,
  '/warehouse/receipt-weighers': ErpInvWeighbridgeTicketsPage,
  '/warehouse/daily-checks': ErpInvDailyChecksPage,
  '/purchasing/purchase-orders': ErpPurOrdersPage,
  '/purchasing/purchase-requisitions': ErpPurRequisitionsPage,
  '/purchasing/purchase-invoices': ErpPurInvoicesPage,
  '/purchasing/return-shipments': ErpReturnShipmentsPage,
  '/purchasing/purchase-returns': ErpPurchaseReturnsPage,
  '/purchasing/goods-receipts': ErpGoodsReceiptsPage,
  '/purchasing/rfqs': ErpRfqsPage,
  '/purchasing/bid-comparisons': ErpBidSelectionsPage,
  '/purchasing/vendor-advances': ErpVendorAdvancesPage,
  '/purchasing/freight-payables': ErpFreightPayablesPage,
  '/purchasing/payment-schedules': ErpPaymentSchedulesPage,
  '/purchasing/vendor-payments': ErpVendorPaymentsPage,
  '/purchasing/opening-ap-balance': ErpOpeningApBalancePage,
  '/manufacturing/boms': ErpMfgBomsPage,
  '/manufacturing/work-orders': MfgWorkOrdersPage,
};
