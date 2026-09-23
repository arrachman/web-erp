/**
 * Sales (M5) "Data" registers — read-only document registers (legacy DATA group).
 * Keyed by canonical `sys_menus.path` (`/sales/data/*`), consumed by
 * `DocumentRegisterPage`. See `inv-registers.ts` for the reference pattern.
 */

import {
  listArCollections,
  type ErpArCollection,
  type ListArCollectionsParams,
} from '@/lib/api/sls-ar-collections';
import {
  listSlsQuotations,
  type ErpSlsQuotation,
  type ListSlsQuotationsParams,
} from '@/lib/api/sls-quotations';
import {
  listSlsOrders,
  type ErpSlsOrder,
  type ListSlsOrdersParams,
} from '@/lib/api/sls-orders';
import {
  listSlsCustomerAdvances,
  type ErpSlsCustomerAdvance,
  type ListSlsCustomerAdvancesParams,
} from '@/lib/api/sls-customer-advances';
import {
  listArReceipts,
  type ErpArReceipt,
  type ListArReceiptsParams,
} from '@/lib/api/fin-ar-receipts';
import {
  listSlsProformaInvoices,
  type ErpSlsProformaInvoice,
  type ListSlsProformaInvoicesParams,
} from '@/lib/api/sls-proforma-invoices';
import {
  listSlsPackingLists,
  type ErpSlsPackingList,
  type ListSlsPackingListsParams,
} from '@/lib/api/sls-packing-lists';
import {
  listSlsDeliveryOrders,
  type ErpSlsDeliveryOrder,
  type ListSlsDeliveryOrdersParams,
} from '@/lib/api/sls-delivery-orders';
import {
  listSlsDeliveryReports,
  type ErpSlsDeliveryReport,
  type ListSlsDeliveryReportsParams,
} from '@/lib/api/sls-delivery-reports';
import {
  listSlsInvoices,
  type ErpSlsInvoice,
  type ListSlsInvoicesParams,
} from '@/lib/api/sls-invoices';
import {
  listSlsReturnReceipts,
  type ErpSlsReturnReceipt,
  type ListSlsReturnReceiptsParams,
} from '@/lib/api/sls-return-receipts';
import {
  listSlsReturns,
  type ErpSlsReturn,
  type ListSlsReturnsParams,
} from '@/lib/api/sls-returns';
import {
  listApPayments,
  type ErpApPayment,
  type ListApPaymentsParams,
} from '@/lib/api/fin-ap-payments';
import {
  listSlsInvoiceSwaps,
  type ErpSlsInvoiceSwap,
  type ListSlsInvoiceSwapsParams,
} from '@/lib/api/sls-invoice-swaps';
import { formatNumber } from '@/lib/format';
import type { DocumentRegisterConfig, AnyDocumentRegisterConfig } from './register-config';

const GROUP = 'Sales · Data';
const dash = (v: string | null | undefined) => v || '—';

/** Right-aligned grand-total column shared by item-based sales documents. */
const grandTotalColumn = <Row extends { grandTotal: string }>() => ({
  header: 'Total',
  align: 'right' as const,
  render: (r: Row) => formatNumber(Number(r.grandTotal ?? 0), 2),
  csv: (r: Row) => r.grandTotal ?? '0',
});

/** Shared columns (Customer · Uraian · Total) for item-based sales documents. */
type ItemDoc = {
  grandTotal: string;
  customer?: { name: string } | null;
  description?: string | null;
};
const itemColumns = <Row extends ItemDoc>() => [
  { header: 'Customer', render: (r: Row) => dash(r.customer?.name), csv: (r: Row) => r.customer?.name ?? '' },
  { header: 'Uraian', render: (r: Row) => dash(r.description), csv: (r: Row) => r.description ?? '' },
  grandTotalColumn<Row>(),
];

const quotations: DocumentRegisterConfig<ErpSlsQuotation> = {
  group: GROUP,
  title: 'Sales Quotation (SQ)',
  code: 'SQ',
  icon: 'database',
  editBase: '/sales/quotations',
  sortBy: 'docDate',
  list: (p) => listSlsQuotations(p as unknown as ListSlsQuotationsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsQuotation>(),
};

const orders: DocumentRegisterConfig<ErpSlsOrder> = {
  group: GROUP,
  title: 'Sales Order (SO)',
  code: 'SO',
  icon: 'database',
  editBase: '/sales/orders',
  sortBy: 'docDate',
  list: (p) => listSlsOrders(p as unknown as ListSlsOrdersParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsOrder>(),
};

const customerAdvances: DocumentRegisterConfig<ErpSlsCustomerAdvance> = {
  group: GROUP,
  title: 'Customer Advance (AS)',
  code: 'AS',
  icon: 'database',
  editBase: '/sales/customer-advances',
  sortBy: 'docDate',
  list: (p) => listSlsCustomerAdvances(p as unknown as ListSlsCustomerAdvancesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Customer', render: (r) => dash(r.customer?.name), csv: (r) => r.customer?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    {
      header: 'Jumlah',
      align: 'right',
      render: (r) => formatNumber(Number(r.amount ?? 0), 2),
      csv: (r) => r.amount ?? '0',
    },
  ],
};

const paymentReceipts: DocumentRegisterConfig<ErpArReceipt> = {
  group: GROUP,
  title: 'Payment Receipt (IP)',
  code: 'IP',
  icon: 'database',
  editBase: '/sales/payment-receipts',
  list: (p) => listArReceipts(p as unknown as ListArReceiptsParams),
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

const proformaInvoices: DocumentRegisterConfig<ErpSlsProformaInvoice> = {
  group: GROUP,
  title: 'Proforma Invoice (PI)',
  code: 'PI',
  icon: 'database',
  editBase: '/sales/proforma-invoices',
  sortBy: 'docDate',
  list: (p) => listSlsProformaInvoices(p as unknown as ListSlsProformaInvoicesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsProformaInvoice>(),
};

const packingLists: DocumentRegisterConfig<ErpSlsPackingList> = {
  group: GROUP,
  title: 'Packing List (PL)',
  code: 'PL',
  icon: 'database',
  editBase: '/sales/packing-lists',
  sortBy: 'docDate',
  list: (p) => listSlsPackingLists(p as unknown as ListSlsPackingListsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsPackingList>(),
};

const deliveryOrders: DocumentRegisterConfig<ErpSlsDeliveryOrder> = {
  group: GROUP,
  title: 'Delivery Order (DO)',
  code: 'DO',
  icon: 'database',
  editBase: '/sales/delivery-orders',
  sortBy: 'docDate',
  list: (p) => listSlsDeliveryOrders(p as unknown as ListSlsDeliveryOrdersParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsDeliveryOrder>(),
};

const deliveryReports: DocumentRegisterConfig<ErpSlsDeliveryReport> = {
  group: GROUP,
  title: 'Delivery Report (DR)',
  code: 'DR',
  icon: 'database',
  editBase: '/sales/delivery-reports',
  sortBy: 'docDate',
  list: (p) => listSlsDeliveryReports(p as unknown as ListSlsDeliveryReportsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsDeliveryReport>(),
};

const invoices: DocumentRegisterConfig<ErpSlsInvoice> = {
  group: GROUP,
  title: 'Sales Invoice (SI)',
  code: 'SI',
  icon: 'database',
  editBase: '/sales/invoices',
  sortBy: 'docDate',
  list: (p) => listSlsInvoices(p as unknown as ListSlsInvoicesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsInvoice>(),
};

const returnReceipts: DocumentRegisterConfig<ErpSlsReturnReceipt> = {
  group: GROUP,
  title: 'Return Receipt (RNR)',
  code: 'RNR',
  icon: 'database',
  editBase: '/sales/return-receipts',
  sortBy: 'docDate',
  list: (p) => listSlsReturnReceipts(p as unknown as ListSlsReturnReceiptsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsReturnReceipt>(),
};

const returns: DocumentRegisterConfig<ErpSlsReturn> = {
  group: GROUP,
  title: 'Sales Return (SR)',
  code: 'SR',
  icon: 'database',
  editBase: '/sales/returns',
  sortBy: 'docDate',
  list: (p) => listSlsReturns(p as unknown as ListSlsReturnsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsReturn>(),
};

const arPayments: DocumentRegisterConfig<ErpApPayment> = {
  group: GROUP,
  title: 'AR Payment (PV)',
  code: 'PV',
  icon: 'database',
  editBase: '/sales/ar-payments',
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

const invoiceSwaps: DocumentRegisterConfig<ErpSlsInvoiceSwap> = {
  group: GROUP,
  title: 'Invoice Swap (SIE)',
  code: 'SIE',
  icon: 'database',
  editBase: '/sales/invoice-swaps',
  sortBy: 'docDate',
  list: (p) => listSlsInvoiceSwaps(p as unknown as ListSlsInvoiceSwapsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Customer', render: (r) => dash(r.customer?.name), csv: (r) => r.customer?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

// Freight Receivable (RP) — same backend as Sales Invoice. NOTE: ListSlsInvoicesParams
// has no code/docCode filter, so this register lists ALL invoices (cannot isolate SLS.RP).
const freightReceivables: DocumentRegisterConfig<ErpSlsInvoice> = {
  group: GROUP,
  title: 'Freight Receivable (RP)',
  code: 'RP',
  icon: 'database',
  editBase: '/sales/freight-receivables',
  sortBy: 'docDate',
  list: (p) => listSlsInvoices(p as unknown as ListSlsInvoicesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsInvoice>(),
};

// Opening AR Balance — Sales Invoices flagged isOpeningBalance.
const openingArBalance: DocumentRegisterConfig<ErpSlsInvoice> = {
  group: GROUP,
  title: 'Opening AR Balance',
  code: 'Opening AR',
  icon: 'database',
  editBase: '/sales/opening-ar-balance',
  sortBy: 'docDate',
  extraParams: { isOpeningBalance: true },
  list: (p) => listSlsInvoices(p as unknown as ListSlsInvoicesParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.docDate,
  getStatus: (r) => r.status,
  columns: itemColumns<ErpSlsInvoice>(),
};

export const SLS_REGISTERS: Record<string, AnyDocumentRegisterConfig> = {
  '/sales/data/quotations': quotations,
  '/sales/data/orders': orders,
  '/sales/data/customer-advances': customerAdvances,
  '/sales/data/payment-receipts': paymentReceipts,
  '/sales/data/proforma-invoices': proformaInvoices,
  '/sales/data/packing-lists': packingLists,
  '/sales/data/delivery-orders': deliveryOrders,
  '/sales/data/delivery-reports': deliveryReports,
  '/sales/data/invoices': invoices,
  '/sales/data/return-receipts': returnReceipts,
  '/sales/data/returns': returns,
  '/sales/data/ar-payments': arPayments,
  '/sales/data/invoice-swaps': invoiceSwaps,
  '/sales/data/freight-receivables': freightReceivables,
  '/sales/data/opening-ar-balance': openingArBalance,
  '/sales/data/ar-collections': {
    group: 'Sales · Data', title: 'AR Collection (IC)', code: 'IC', icon: 'database',
    editBase: '/sales/ar-collections', sortBy: 'transactionDate',
    list: (p) => listArCollections(p as unknown as ListArCollectionsParams),
    getId: (r: ErpArCollection) => r.id,
    getDocNumber: (r: ErpArCollection) => r.docNumber,
    getDocDate: (r: ErpArCollection) => r.transactionDate,
    getStatus: (r: ErpArCollection) => r.status,
    columns: [
      { header: 'Customer', render: (r: ErpArCollection) => dash(r.partner?.name), csv: (r: ErpArCollection) => r.partner?.name ?? '' },
      { header: 'Uraian', render: (r: ErpArCollection) => dash(r.description), csv: (r: ErpArCollection) => r.description },
      { header: 'Jumlah', align: 'right' as const, render: (r: ErpArCollection) => formatNumber(Number(r.amount), 2), csv: (r: ErpArCollection) => r.amount },
    ],
  },
};
