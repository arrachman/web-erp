/**
 * Additional Sales report views backed by verified document header fields.
 * Settlement aging is omitted because invoice headers do not store cumulative
 * receipts; a status recap remains source-verifiable for partial payments.
 */

import { PrismaService } from '../prisma/prisma.service';
import { makeOutstandingReport } from './report-factory';
import { baseWhere } from './report-helpers';
import { ReportDef } from './report-types';

type RecapConfig = {
  key: string;
  title: string;
  model: string;
  dateField: string;
  partnerField: string;
  amountField: string;
  where?: Record<string, unknown>;
};

const SALES_RECAPS: RecapConfig[] = [
  {
    key: 'quotations-by-status',
    title: 'Rekap Penawaran Harga per Status',
    model: 'erpSlsQuotation',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'orders-by-status',
    title: 'Rekap Pesanan Penjualan per Status',
    model: 'erpSlsOrder',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'proforma-invoices-by-status',
    title: 'Rekap Faktur Proforma per Status',
    model: 'erpSlsProformaInvoice',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'packing-lists-by-status',
    title: 'Rekap Daftar Kemasan per Status',
    model: 'erpSlsPackingList',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'delivery-orders-by-status',
    title: 'Rekap Surat Jalan per Status',
    model: 'erpSlsDeliveryOrder',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'delivery-reports-by-status',
    title: 'Rekap Laporan Pengiriman per Status',
    model: 'erpSlsDeliveryReport',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'invoices-by-status',
    title: 'Rekap Faktur Penjualan per Status',
    model: 'erpSlsInvoice',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'returns-by-status',
    title: 'Rekap Retur Penjualan per Status',
    model: 'erpSlsReturn',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'return-receipts-by-status',
    title: 'Rekap Tanda Terima Retur per Status',
    model: 'erpSlsReturnReceipt',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
  },
  {
    key: 'customer-advances-by-status',
    title: 'Rekap Uang Muka Pelanggan per Status',
    model: 'erpSlsCustomerAdvance',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'amount',
  },
  {
    key: 'freight-receivables-by-status',
    title: 'Rekap Piutang Angkutan per Status',
    model: 'erpSlsInvoice',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'otherCostAmount',
    where: { otherCostAmount: { not: null } },
  },
  {
    key: 'opening-ar-balance-by-status',
    title: 'Rekap Saldo Awal Piutang per Status',
    model: 'erpSlsInvoice',
    dateField: 'docDate',
    partnerField: 'customerId',
    amountField: 'grandTotal',
    where: { isOpeningBalance: true },
  },
  {
    key: 'ar-receipts-by-status',
    title: 'Rekap Penerimaan AR per Status',
    model: 'erpFinArReceipt',
    dateField: 'transactionDate',
    partnerField: 'partnerId',
    amountField: 'amount',
  },
];

function buildInvoiceSwapStatusReport(prisma: PrismaService): ReportDef {
  return {
    key: 'invoice-swaps-by-status',
    title: 'Rekap Tukar Faktur per Status',
    group: 'document',
    columns: [
      { key: 'status', header: 'Status', type: 'status' },
      { key: 'documentCount', header: 'Jumlah Dokumen', type: 'number' },
    ],
    resolve: async (filters) => {
      const groups = await prisma.erpSlsInvoiceSwap.groupBy({
        by: ['status'],
        where: baseWhere('docDate', filters),
        _count: { _all: true },
      });
      const rows = groups.map((group) => ({
        status: group.status,
        documentCount: group._count._all,
      }));
      return {
        rows,
        total: rows.length,
        summary: [{ label: 'Jumlah Dokumen', value: rows.reduce((total, row) => total + row.documentCount, 0), type: 'number' }],
        charts: [{
          kind: 'donut',
          title: 'Dokumen per Status',
          labels: rows.map((row) => row.status),
          values: rows.map((row) => row.documentCount),
        }],
      };
    },
  };
}

export function buildSalesFactoryReports(prisma: PrismaService): ReportDef[] {
  const recaps = SALES_RECAPS.map((config) =>
    makeOutstandingReport(prisma, {
      ...config,
      group: 'document',
      basis: 'recap',
      groupBy: 'status',
      openWhere: config.where,
    }),
  );
  return [...recaps, buildInvoiceSwapStatusReport(prisma)];
}
