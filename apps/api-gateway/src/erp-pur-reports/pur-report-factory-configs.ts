/**
 * Additional Purchasing report views backed by verified document sources.
 * Aliased legacy menu labels are intentionally excluded when they share an
 * indistinguishable source model with another report.
 */

import { PrismaService } from '../prisma/prisma.service';
import { makeOutstandingReport } from './report-factory';
import { ReportDef } from './report-types';

type RecapConfig = {
  key: string;
  title: string;
  model: string;
  partnerField: string;
  amountField: string;
  where?: Record<string, unknown>;
};

const PURCHASE_RECAPS: RecapConfig[] = [
  {
    key: 'purchase-requisitions-by-status',
    title: 'Rekap Purchase Requisition per Status',
    model: 'erpPurRequisition',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
  },
  {
    key: 'rfqs-by-status',
    title: 'Rekap Request for Quotation per Status',
    model: 'erpPurRfq',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
  },
  {
    key: 'bid-comparisons-by-status',
    title: 'Rekap Bid Comparison per Status',
    model: 'erpPurBidSelection',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
  },
  {
    key: 'purchase-orders-by-status',
    title: 'Rekap Purchase Order per Status',
    model: 'erpPurOrder',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
  },
  {
    key: 'goods-receipts-by-status',
    title: 'Rekap Goods Receipt per Status',
    model: 'erpPurGoodsReceipt',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
  },
  {
    key: 'purchase-invoices-by-status',
    title: 'Rekap Purchase Invoice per Status',
    model: 'erpPurInvoice',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
  },
  {
    key: 'return-shipments-by-status',
    title: 'Rekap Return Shipment per Status',
    model: 'erpPurReturn',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
    where: { returnType: 'DEBIT_NOTE' },
  },
  {
    key: 'purchase-returns-by-status',
    title: 'Rekap Purchase Return per Status',
    model: 'erpPurReturn',
    partnerField: 'supplierId',
    amountField: 'grandTotal',
    where: { returnType: 'RETURN_TO_VENDOR' },
  },
  {
    key: 'vendor-payments-by-status',
    title: 'Rekap Pembayaran Vendor per Status',
    model: 'erpFinApPayment',
    partnerField: 'partnerId',
    amountField: 'amount',
  },
];

export function buildPurFactoryReports(prisma: PrismaService): ReportDef[] {
  return PURCHASE_RECAPS.map((config) =>
    makeOutstandingReport(prisma, {
      ...config,
      group: 'transaction',
      basis: 'recap',
      dateField: config.model === 'erpFinApPayment' ? 'transactionDate' : 'docDate',
      groupBy: 'status',
      openWhere: config.where,
    }),
  );
}
