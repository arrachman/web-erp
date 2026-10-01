/**
 * Purchasing report resolvers — Part 2: PP, DNR, PRT, VPP, VP, Summary.
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportDef, ReportFilters, ReportSummaryItem } from './report-types';

type Deps = { prisma: PrismaService };

/** Resolve partner display names from a set of IDs. */
async function resolvePartnerMap(
  prisma: PrismaService,
  ids: (bigint | null | undefined)[],
): Promise<Map<string, string>> {
  const unique = [...new Set(ids.filter(Boolean).map(String))];
  if (unique.length === 0) return new Map();
  const partners = await prisma.erpPartner.findMany({
    where: { id: { in: unique.map(BigInt) } },
    select: { id: true, code: true, name: true },
  });
  return new Map(partners.map((p) => [String(p.id), `${p.code} — ${p.name}`]));
}

/** Build a standard supplier where clause for purchasing docs. */
function buildSupplierWhere(filters: ReportFilters): Record<string, unknown> {
  return {
    deletedAt: null,
    ...(filters.dateFrom || filters.dateTo
      ? {
          docDate: {
            ...(filters.dateFrom ? { gte: new Date(filters.dateFrom) } : {}),
            ...(filters.dateTo ? { lte: new Date(filters.dateTo) } : {}),
          },
        }
      : {}),
    ...(filters.partnerId ? { supplierId: BigInt(filters.partnerId) } : {}),
    ...(filters.status ? { status: filters.status as never } : {}),
    ...(filters.search
      ? {
          OR: [
            { docNumber: { contains: filters.search, mode: 'insensitive' } },
            { description: { contains: filters.search, mode: 'insensitive' } },
          ],
        }
      : {}),
  };
}

/** Build a where clause for erpFinApPayment (uses transactionDate + partnerId). */
function buildApPaymentWhere(filters: ReportFilters): Record<string, unknown> {
  return {
    deletedAt: null,
    ...(filters.dateFrom || filters.dateTo
      ? {
          transactionDate: {
            ...(filters.dateFrom ? { gte: new Date(filters.dateFrom) } : {}),
            ...(filters.dateTo ? { lte: new Date(filters.dateTo) } : {}),
          },
        }
      : {}),
    ...(filters.partnerId ? { partnerId: BigInt(filters.partnerId) } : {}),
    ...(filters.status ? { status: filters.status as never } : {}),
    ...(filters.search
      ? {
          OR: [
            { docNumber: { contains: filters.search, mode: 'insensitive' } },
            { description: { contains: filters.search, mode: 'insensitive' } },
          ],
        }
      : {}),
  };
}

export function buildPurReportsPart2(deps: Deps): ReportDef[] {
  const { prisma } = deps;

  const freightPayableResolver: ReportDef = {
    key: 'freight-payables',
    title: 'Freight Payable (PP)',
    group: 'transaction',
    columns: [
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'docDate', header: 'Tanggal', type: 'date' },
      { key: 'supplier', header: 'Supplier', type: 'text' },
      { key: 'dueDate', header: 'Jatuh Tempo', type: 'date' },
      { key: 'grandTotal', header: 'Total', type: 'money' },
      { key: 'settlementStatus', header: 'Status Bayar', type: 'status' },
      { key: 'status', header: 'Status', type: 'status' },
    ],
    resolve: async (filters: ReportFilters) => {
      const where = buildSupplierWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurInvoice.count({ where }),
        prisma.erpPurInvoice.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
      ]);
      const partnerMap = await resolvePartnerMap(prisma, records.map((r) => r.supplierId));
      const rows = records.map((r) => ({
        docNumber: r.docNumber,
        docDate: r.docDate.toISOString().slice(0, 10),
        supplier: r.supplierId ? (partnerMap.get(String(r.supplierId)) ?? '—') : '—',
        dueDate: r.dueDate ? r.dueDate.toISOString().slice(0, 10) : '',
        grandTotal: Number(r.grandTotal),
        settlementStatus: r.settlementStatus,
        status: r.status,
      }));
      const totalAmount = records.reduce((s, r) => s + Number(r.grandTotal), 0);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai', value: totalAmount, type: 'money' },
      ];
      return { rows, summary, total };
    },
  };

  const returnShipmentResolver: ReportDef = {
    key: 'return-shipments',
    title: 'Return Shipment (DNR)',
    group: 'transaction',
    columns: [
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'docDate', header: 'Tanggal', type: 'date' },
      { key: 'supplier', header: 'Supplier', type: 'text' },
      { key: 'description', header: 'Keterangan', type: 'text' },
      { key: 'grandTotal', header: 'Total', type: 'money' },
      { key: 'status', header: 'Status', type: 'status' },
    ],
    resolve: async (filters: ReportFilters) => {
      const where = { ...buildSupplierWhere(filters), returnType: 'DEBIT_NOTE' as never };
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurReturn.count({ where }),
        prisma.erpPurReturn.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
      ]);
      const partnerMap = await resolvePartnerMap(prisma, records.map((r) => r.supplierId));
      const rows = records.map((r) => ({
        docNumber: r.docNumber,
        docDate: r.docDate.toISOString().slice(0, 10),
        supplier: r.supplierId ? (partnerMap.get(String(r.supplierId)) ?? '—') : '—',
        description: r.description ?? '',
        grandTotal: Number(r.grandTotal),
        status: r.status,
      }));
      const totalAmount = records.reduce((s, r) => s + Number(r.grandTotal), 0);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai', value: totalAmount, type: 'money' },
      ];
      return { rows, summary, total };
    },
  };

  const purchaseReturnResolver: ReportDef = {
    key: 'purchase-returns',
    title: 'Purchase Return (PRT)',
    group: 'transaction',
    columns: [
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'docDate', header: 'Tanggal', type: 'date' },
      { key: 'supplier', header: 'Supplier', type: 'text' },
      { key: 'description', header: 'Keterangan', type: 'text' },
      { key: 'grandTotal', header: 'Total', type: 'money' },
      { key: 'status', header: 'Status', type: 'status' },
    ],
    resolve: async (filters: ReportFilters) => {
      const where = { ...buildSupplierWhere(filters), returnType: 'RETURN_TO_VENDOR' as never };
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurReturn.count({ where }),
        prisma.erpPurReturn.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
      ]);
      const partnerMap = await resolvePartnerMap(prisma, records.map((r) => r.supplierId));
      const rows = records.map((r) => ({
        docNumber: r.docNumber,
        docDate: r.docDate.toISOString().slice(0, 10),
        supplier: r.supplierId ? (partnerMap.get(String(r.supplierId)) ?? '—') : '—',
        description: r.description ?? '',
        grandTotal: Number(r.grandTotal),
        status: r.status,
      }));
      const totalAmount = records.reduce((s, r) => s + Number(r.grandTotal), 0);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai', value: totalAmount, type: 'money' },
      ];
      return { rows, summary, total };
    },
  };

  const paymentScheduleResolver: ReportDef = {
    key: 'payment-schedules',
    title: 'Payment Schedule (VPP)',
    group: 'transaction',
    columns: [
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'transactionDate', header: 'Tanggal', type: 'date' },
      { key: 'vendor', header: 'Vendor', type: 'text' },
      { key: 'description', header: 'Keterangan', type: 'text' },
      { key: 'amount', header: 'Jumlah', type: 'money' },
      { key: 'paymentStatus', header: 'Status Bayar', type: 'status' },
      { key: 'status', header: 'Status', type: 'status' },
    ],
    resolve: async (filters: ReportFilters) => {
      const where = buildApPaymentWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpFinApPayment.count({ where }),
        prisma.erpFinApPayment.findMany({ where, skip, take: limit, orderBy: { transactionDate: 'desc' } }),
      ]);
      const partnerMap = await resolvePartnerMap(prisma, records.map((r) => r.partnerId));
      const rows = records.map((r) => ({
        docNumber: r.docNumber,
        transactionDate: r.transactionDate.toISOString().slice(0, 10),
        vendor: r.partnerId ? (partnerMap.get(String(r.partnerId)) ?? '—') : '—',
        description: r.description ?? '',
        amount: Number(r.amount),
        paymentStatus: r.paymentStatus,
        status: r.status,
      }));
      const totalAmount = records.reduce((s, r) => s + Number(r.amount), 0);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai', value: totalAmount, type: 'money' },
      ];
      return { rows, summary, total };
    },
  };

  const vendorPaymentResolver: ReportDef = {
    key: 'vendor-payments',
    title: 'Vendor Payment (VP)',
    group: 'transaction',
    columns: [
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'transactionDate', header: 'Tanggal', type: 'date' },
      { key: 'vendor', header: 'Vendor', type: 'text' },
      { key: 'description', header: 'Keterangan', type: 'text' },
      { key: 'amount', header: 'Jumlah', type: 'money' },
      { key: 'allocatedAmount', header: 'Dialokasikan', type: 'money' },
      { key: 'paymentStatus', header: 'Status Bayar', type: 'status' },
      { key: 'status', header: 'Status', type: 'status' },
    ],
    resolve: async (filters: ReportFilters) => {
      const where = buildApPaymentWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpFinApPayment.count({ where }),
        prisma.erpFinApPayment.findMany({ where, skip, take: limit, orderBy: { transactionDate: 'desc' } }),
      ]);
      const partnerMap = await resolvePartnerMap(prisma, records.map((r) => r.partnerId));
      const rows = records.map((r) => ({
        docNumber: r.docNumber,
        transactionDate: r.transactionDate.toISOString().slice(0, 10),
        vendor: r.partnerId ? (partnerMap.get(String(r.partnerId)) ?? '—') : '—',
        description: r.description ?? '',
        amount: Number(r.amount),
        allocatedAmount: Number(r.allocatedAmount),
        paymentStatus: r.paymentStatus,
        status: r.status,
      }));
      const totalAmount = records.reduce((s, r) => s + Number(r.amount), 0);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai', value: totalAmount, type: 'money' },
      ];
      return { rows, summary, total };
    },
  };

  const summaryResolver: ReportDef = {
    key: 'summary',
    title: 'Purchases Summary',
    group: 'transaction',
    columns: [
      { key: 'category', header: 'Kategori', type: 'text' },
      { key: 'count', header: 'Jumlah Dok.', type: 'number' },
      { key: 'totalAmount', header: 'Total Nilai', type: 'money' },
    ],
    resolve: async (filters: ReportFilters) => {
      const dateFilter =
        filters.dateFrom || filters.dateTo
          ? {
              docDate: {
                ...(filters.dateFrom ? { gte: new Date(filters.dateFrom) } : {}),
                ...(filters.dateTo ? { lte: new Date(filters.dateTo) } : {}),
              },
            }
          : {};
      const apDateFilter =
        filters.dateFrom || filters.dateTo
          ? {
              transactionDate: {
                ...(filters.dateFrom ? { gte: new Date(filters.dateFrom) } : {}),
                ...(filters.dateTo ? { lte: new Date(filters.dateTo) } : {}),
              },
            }
          : {};

      const [prCount, rfqCount, poCount, grnCount, piCount, vpCount] = await Promise.all([
        prisma.erpPurRequisition.count({ where: { deletedAt: null, ...dateFilter } }),
        prisma.erpPurRfq.count({ where: { deletedAt: null, ...dateFilter } }),
        prisma.erpPurOrder.count({ where: { deletedAt: null, ...dateFilter } }),
        prisma.erpPurGoodsReceipt.count({ where: { deletedAt: null, ...dateFilter } }),
        prisma.erpPurInvoice.count({ where: { deletedAt: null, ...dateFilter } }),
        prisma.erpFinApPayment.count({ where: { deletedAt: null, ...apDateFilter } }),
      ]);

      const [poTotal, piTotal, vpTotal] = await Promise.all([
        prisma.erpPurOrder.aggregate({ _sum: { grandTotal: true }, where: { deletedAt: null, ...dateFilter } }),
        prisma.erpPurInvoice.aggregate({ _sum: { grandTotal: true }, where: { deletedAt: null, ...dateFilter } }),
        prisma.erpFinApPayment.aggregate({ _sum: { amount: true }, where: { deletedAt: null, ...apDateFilter } }),
      ]);

      const rows = [
        { category: 'Purchase Requisition (PR)', count: prCount, totalAmount: null },
        { category: 'Request for Quotation (RFQ)', count: rfqCount, totalAmount: null },
        { category: 'Purchase Order (PO)', count: poCount, totalAmount: Number(poTotal._sum.grandTotal ?? 0) },
        { category: 'Goods Receipt (GRN)', count: grnCount, totalAmount: null },
        { category: 'Purchase Invoice (PI)', count: piCount, totalAmount: Number(piTotal._sum.grandTotal ?? 0) },
        { category: 'Vendor Payment (VP)', count: vpCount, totalAmount: Number(vpTotal._sum.amount ?? 0) },
      ];

      return { rows, summary: [], total: rows.length };
    },
  };

  return [
    freightPayableResolver,
    returnShipmentResolver,
    purchaseReturnResolver,
    paymentScheduleResolver,
    vendorPaymentResolver,
    summaryResolver,
  ];
}
