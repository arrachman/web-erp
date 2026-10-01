/**
 * Purchasing report resolvers — Part 1: PR, RFQ, BS, PO, Vendor Advance, GRN, PI.
 */

import { PrismaService } from '../prisma/prisma.service';
import { ReportDef, ReportFilters, ReportSummaryItem } from './report-types';

type Deps = { prisma: PrismaService };

/** Build a standard supplier-based where clause for purchasing docs. */
function buildSupplierWhere(
  filters: ReportFilters,
  dateField: string = 'docDate',
  supplierField: string = 'supplierId',
): Record<string, unknown> {
  return {
    deletedAt: null,
    ...(filters.dateFrom || filters.dateTo
      ? {
          [dateField]: {
            ...(filters.dateFrom ? { gte: new Date(filters.dateFrom) } : {}),
            ...(filters.dateTo ? { lte: new Date(filters.dateTo) } : {}),
          },
        }
      : {}),
    ...(filters.partnerId ? { [supplierField]: BigInt(filters.partnerId) } : {}),
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

/** Resolve supplier display names from a set of supplier IDs. */
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

export function buildPurReportsPart1(deps: Deps): ReportDef[] {
  const { prisma } = deps;

  const prResolver: ReportDef = {
    key: 'purchase-requisitions',
    title: 'Purchase Requisition (PR)',
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
      const where = buildSupplierWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurRequisition.count({ where }),
        prisma.erpPurRequisition.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
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

  const rfqResolver: ReportDef = {
    key: 'rfqs',
    title: 'Request for Quotation (RFQ)',
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
      const where = buildSupplierWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurRfq.count({ where }),
        prisma.erpPurRfq.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
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

  const bsResolver: ReportDef = {
    key: 'bid-comparisons',
    title: 'Bid Comparison (BS)',
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
      const where = buildSupplierWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurBidSelection.count({ where }),
        prisma.erpPurBidSelection.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
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

  const poResolver: ReportDef = {
    key: 'purchase-orders',
    title: 'Purchase Order (PO)',
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
      const where = buildSupplierWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurOrder.count({ where }),
        prisma.erpPurOrder.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
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

  const vendorAdvanceResolver: ReportDef = {
    key: 'vendor-advances',
    title: 'Vendor Advance (AP)',
    group: 'transaction',
    columns: [
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'docDate', header: 'Tanggal', type: 'date' },
      { key: 'supplier', header: 'Supplier', type: 'text' },
      { key: 'dueDate', header: 'Jatuh Tempo', type: 'date' },
      { key: 'grandTotal', header: 'Total', type: 'money' },
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

  const grnResolver: ReportDef = {
    key: 'goods-receipts',
    title: 'Goods Receipt (GRN)',
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
      const where = buildSupplierWhere(filters);
      const page = filters.page ?? 1;
      const limit = filters.limit ?? 50;
      const skip = (page - 1) * limit;
      const [total, records] = await Promise.all([
        prisma.erpPurGoodsReceipt.count({ where }),
        prisma.erpPurGoodsReceipt.findMany({ where, skip, take: limit, orderBy: { docDate: 'desc' } }),
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

  const piResolver: ReportDef = {
    key: 'purchase-invoices',
    title: 'Purchase Invoice (PI)',
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

  return [prResolver, rfqResolver, bsResolver, poResolver, vendorAdvanceResolver, grnResolver, piResolver];
}
