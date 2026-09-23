/**
 * Purchasing (M4) "Data" registers — read-only document registers (legacy DATA group).
 * Keyed by canonical `sys_menus.path` (`/purchasing/data/*`), consumed by
 * `DocumentRegisterPage`. See `inv-registers.ts` for the reference pattern.
 *
 * Return Shipment (DNR) and Purchase Return (PRT) share one backend table
 * (`pur_returns`), discriminated by `returnType` via `extraParams`:
 *   DEBIT_NOTE       → Return Shipment (DNR)
 *   RETURN_TO_VENDOR → Purchase Return (PRT)
 * Vendor Payment (VP) reuses the Finance AP-payments endpoint.
 */

import {
  listPurRequisitions,
  type ErpPurRequisition,
  type ListPurRequisitionsParams,
} from '@/lib/api/pur-requisitions';
import {
  listPurRfqs,
  type ErpPurRfq,
  type ListPurRfqsParams,
} from '@/lib/api/pur-rfqs';
import {
  listPurBidSelections,
  type ErpPurBidSelection,
  type ListPurBidSelectionsParams,
} from '@/lib/api/pur-bid-selections';
import {
  listPurOrders,
  type ErpPurOrder,
  type ListPurOrdersParams,
} from '@/lib/api/pur-orders';
import {
  listPurGoodsReceipts,
  type ErpPurGoodsReceipt,
  type ListPurGoodsReceiptsParams,
} from '@/lib/api/pur-goods-receipts';
import {
  listPurInvoices,
  type ErpPurInvoice,
  type ListPurInvoicesParams,
} from '@/lib/api/pur-invoices';
import {
  listPurReturns,
  type ErpPurReturn,
  type ListPurReturnsParams,
} from '@/lib/api/pur-returns';
import {
  listVendorAdvances,
  type ErpVendorAdvance,
  type ListVendorAdvancesParams,
} from '@/lib/api/pur-vendor-advances';
import {
  listFreightPayables,
  type ErpFreightPayable,
  type ListFreightPayablesParams,
} from '@/lib/api/pur-freight-payables';
import {
  listPaymentSchedules,
  type ErpPaymentSchedule,
  type ListPaymentSchedulesParams,
} from '@/lib/api/pur-payment-schedules';
import {
  listApPayments,
  type ErpApPayment,
  type ListApPaymentsParams,
} from '@/lib/api/fin-ap-payments';
import { formatNumber } from '@/lib/format';
import type { DocumentRegisterConfig, AnyDocumentRegisterConfig } from './register-config';

const GROUP = 'Purchasing · Data';
const dash = (v: string | null | undefined) => v || '—';

/** Right-aligned grand-total column shared by supplier documents. */
const grandTotalColumn = {
  header: 'Grand Total',
  align: 'right' as const,
  render: (r: { grandTotal?: string | null }) => formatNumber(Number(r.grandTotal ?? 0), 2),
  csv: (r: { grandTotal?: string | null }) => r.grandTotal ?? '0',
};

const purchaseRequisitions: DocumentRegisterConfig<ErpPurRequisition> = {
  group: GROUP,
  title: 'Purchase Requisition (PR)',
  code: 'PR',
  icon: 'database',
  editBase: '/purchasing/purchase-requisitions',
  sortBy: 'docDate',
  list: (p) => listPurRequisitions(p as unknown as ListPurRequisitionsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Supplier', render: (r) => dash(r.supplier?.name), csv: (r) => r.supplier?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    grandTotalColumn,
  ],
};

const rfqs: DocumentRegisterConfig<ErpPurRfq> = {
  group: GROUP,
  title: 'Request for Quotation (RFQ)',
  code: 'RFQ',
  icon: 'database',
  editBase: '/purchasing/rfqs',
  sortBy: 'docDate',
  list: (p) => listPurRfqs(p as unknown as ListPurRfqsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Cabang', render: (r) => dash(r.branch?.name), csv: (r) => r.branch?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

const bidComparisons: DocumentRegisterConfig<ErpPurBidSelection> = {
  group: GROUP,
  title: 'Bid Comparison (BS)',
  code: 'BS',
  icon: 'database',
  editBase: '/purchasing/bid-comparisons',
  sortBy: 'docDate',
  list: (p) => listPurBidSelections(p as unknown as ListPurBidSelectionsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Cabang', render: (r) => dash(r.branch?.name), csv: (r) => r.branch?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

const purchaseOrders: DocumentRegisterConfig<ErpPurOrder> = {
  group: GROUP,
  title: 'Purchase Order (PO)',
  code: 'PO',
  icon: 'database',
  editBase: '/purchasing/purchase-orders',
  sortBy: 'docDate',
  list: (p) => listPurOrders(p as unknown as ListPurOrdersParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Supplier', render: (r) => dash(r.supplier?.name), csv: (r) => r.supplier?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    grandTotalColumn,
  ],
};

const goodsReceipts: DocumentRegisterConfig<ErpPurGoodsReceipt> = {
  group: GROUP,
  title: 'Goods Receipt (GRN)',
  code: 'GRN',
  icon: 'database',
  editBase: '/purchasing/goods-receipts',
  sortBy: 'docDate',
  list: (p) => listPurGoodsReceipts(p as unknown as ListPurGoodsReceiptsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Supplier', render: (r) => dash(r.supplier?.name), csv: (r) => r.supplier?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    grandTotalColumn,
  ],
};

const purchaseInvoices: DocumentRegisterConfig<ErpPurInvoice> = {
  group: GROUP,
  title: 'Purchase Invoice (PI)',
  code: 'PI',
  icon: 'database',
  editBase: '/purchasing/purchase-invoices',
  sortBy: 'docDate',
  list: (p) => listPurInvoices(p as unknown as ListPurInvoicesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Supplier', render: (r) => dash(r.supplier?.name), csv: (r) => r.supplier?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    grandTotalColumn,
  ],
};

/** Opening AP Balance = pur_invoices flagged isOpeningBalance (read-only register). */
const openingApBalance: DocumentRegisterConfig<ErpPurInvoice> = {
  group: GROUP,
  title: 'Opening AP Balance',
  code: 'OB-AP',
  icon: 'database',
  editBase: '/purchasing/purchase-invoices',
  sortBy: 'docDate',
  extraParams: { isOpeningBalance: true },
  list: (p) => listPurInvoices(p as unknown as ListPurInvoicesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Supplier', render: (r) => dash(r.supplier?.name), csv: (r) => r.supplier?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    grandTotalColumn,
  ],
};

/** Purchase-returns register factory (DNR/PRT share one endpoint via returnType). */
function returnRegister(
  returnType: ListPurReturnsParams['returnType'],
  title: string,
  code: string,
  editBase: string,
): DocumentRegisterConfig<ErpPurReturn> {
  return {
    group: GROUP,
    title,
    code,
    icon: 'database',
    editBase,
    extraParams: { returnType },
    sortBy: 'docDate',
    list: (p) => listPurReturns(p as unknown as ListPurReturnsParams),
    getId: (r) => r.id,
    getDocNumber: (r) => r.docNumber,
    getDocDate: (r) => r.docDate,
    getStatus: (r) => r.status,
    columns: [
      { header: 'Supplier', render: (r) => dash(r.supplier?.name), csv: (r) => r.supplier?.name ?? '' },
      { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
      grandTotalColumn,
    ],
  };
}

const vendorPayments: DocumentRegisterConfig<ErpApPayment> = {
  group: GROUP,
  title: 'Vendor Payment (VP)',
  code: 'VP',
  icon: 'database',
  editBase: '/purchasing/vendor-payments',
  sortBy: 'transactionDate',
  list: (p) => listApPayments(p as unknown as ListApPaymentsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.transactionDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    {
      header: 'Jumlah',
      align: 'right',
      render: (r) => formatNumber(Number(r.amount ?? 0), 2),
      csv: (r) => r.amount ?? '0',
    },
  ],
};

export const PUR_REGISTERS: Record<string, AnyDocumentRegisterConfig> = {
  '/purchasing/data/purchase-requisitions': purchaseRequisitions,
  '/purchasing/data/rfqs': rfqs,
  '/purchasing/data/bid-comparisons': bidComparisons,
  '/purchasing/data/purchase-orders': purchaseOrders,
  '/purchasing/data/goods-receipts': goodsReceipts,
  '/purchasing/data/purchase-invoices': purchaseInvoices,
  '/purchasing/data/return-shipments': returnRegister('DEBIT_NOTE', 'Return Shipment (DNR)', 'DNR', '/purchasing/return-shipments'),
  '/purchasing/data/purchase-returns': returnRegister('RETURN_TO_VENDOR', 'Purchase Return (PRT)', 'PRT', '/purchasing/purchase-returns'),
  '/purchasing/data/vendor-payments': vendorPayments,
  '/purchasing/data/opening-ap-balance': openingApBalance,
  '/purchasing/data/vendor-advances': {
    group: GROUP, title: 'Vendor Advance (AP)', code: 'AP', icon: 'database',
    editBase: '/purchasing/vendor-advances', sortBy: 'transactionDate',
    list: (p) => listVendorAdvances(p as unknown as ListVendorAdvancesParams),
    getId: (r: ErpVendorAdvance) => r.id,
    getDocNumber: (r: ErpVendorAdvance) => r.docNumber,
    getDocDate: (r: ErpVendorAdvance) => r.transactionDate,
    getStatus: (r: ErpVendorAdvance) => r.status,
    columns: [
      { header: 'Supplier', render: (r: ErpVendorAdvance) => dash(r.partner?.name), csv: (r: ErpVendorAdvance) => r.partner?.name ?? '' },
      { header: 'Uraian', render: (r: ErpVendorAdvance) => dash(r.description), csv: (r: ErpVendorAdvance) => r.description },
      { header: 'Jumlah', align: 'right' as const, render: (r: ErpVendorAdvance) => formatNumber(Number(r.amount), 2), csv: (r: ErpVendorAdvance) => r.amount },
    ],
  },
  '/purchasing/data/freight-payables': {
    group: GROUP, title: 'Freight Payable (PP)', code: 'PP', icon: 'database',
    editBase: '/purchasing/freight-payables', sortBy: 'transactionDate',
    list: (p) => listFreightPayables(p as unknown as ListFreightPayablesParams),
    getId: (r: ErpFreightPayable) => r.id,
    getDocNumber: (r: ErpFreightPayable) => r.docNumber,
    getDocDate: (r: ErpFreightPayable) => r.transactionDate,
    getStatus: (r: ErpFreightPayable) => r.status,
    columns: [
      { header: 'Partner', render: (r: ErpFreightPayable) => dash(r.partner?.name), csv: (r: ErpFreightPayable) => r.partner?.name ?? '' },
      { header: 'Uraian', render: (r: ErpFreightPayable) => dash(r.description), csv: (r: ErpFreightPayable) => r.description },
      { header: 'Jumlah', align: 'right' as const, render: (r: ErpFreightPayable) => formatNumber(Number(r.amount), 2), csv: (r: ErpFreightPayable) => r.amount },
    ],
  },
  '/purchasing/data/payment-schedules': {
    group: GROUP, title: 'Payment Schedule (VPP)', code: 'VPP', icon: 'database',
    editBase: '/purchasing/payment-schedules', sortBy: 'transactionDate',
    list: (p) => listPaymentSchedules(p as unknown as ListPaymentSchedulesParams),
    getId: (r: ErpPaymentSchedule) => r.id,
    getDocNumber: (r: ErpPaymentSchedule) => r.docNumber,
    getDocDate: (r: ErpPaymentSchedule) => r.transactionDate,
    getStatus: (r: ErpPaymentSchedule) => r.status,
    columns: [
      { header: 'Supplier', render: (r: ErpPaymentSchedule) => dash(r.partner?.name), csv: (r: ErpPaymentSchedule) => r.partner?.name ?? '' },
      { header: 'Uraian', render: (r: ErpPaymentSchedule) => dash(r.description), csv: (r: ErpPaymentSchedule) => r.description },
      { header: 'Jumlah', align: 'right' as const, render: (r: ErpPaymentSchedule) => formatNumber(Number(r.amount), 2), csv: (r: ErpPaymentSchedule) => r.amount },
    ],
  },
};
