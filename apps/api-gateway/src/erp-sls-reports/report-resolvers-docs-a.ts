/**
 * Sales document-list resolvers A: quotations, orders, customer-advances,
 * payment-receipts, proforma-invoices, packing-lists, delivery-orders, delivery-reports.
 */

import { PrismaService } from '../prisma/prisma.service';
import { dateRange, display, isoDate, num, paginate, resolvePartners, baseWhere } from './report-helpers';
import { ReportDef } from './report-types';
import {
  DOC_COLS_ADVANCE, DOC_COLS_AR, DOC_COLS_MONEY, DOC_COLS_SIMPLE, sumMoney,
} from './report-resolvers-docs-cols';

export function buildDocReportsA(prisma: PrismaService): ReportDef[] {
  return [
    {
      key: 'quotations',
      title: 'Penawaran Harga',
      group: 'document',
      columns: DOC_COLS_MONEY,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsQuotation.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, subtotal: true, grandTotal: true, status: true } }),
          prisma.erpSlsQuotation.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), subtotal: num(d.subtotal), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
    {
      key: 'orders',
      title: 'Pesanan Penjualan',
      group: 'document',
      columns: DOC_COLS_MONEY,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsOrder.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, subtotal: true, grandTotal: true, status: true } }),
          prisma.erpSlsOrder.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), subtotal: num(d.subtotal), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
    {
      key: 'customer-advances',
      title: 'Uang Muka Pelanggan',
      group: 'document',
      columns: DOC_COLS_ADVANCE,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const dateFilter = dateRange('docDate', filters);
        const baseCondition: any = {
          deletedAt: null,
          ...(dateFilter.docDate ? { docDate: dateFilter.docDate } : {}),
          ...(filters.status ? { status: filters.status } : {}),
          ...(filters.search ? { docNumber: { contains: filters.search, mode: 'insensitive' } } : {}),
        };
        const [docs, total] = await Promise.all([
          prisma.erpSlsCustomerAdvance.findMany({ where: baseCondition, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, amount: true, status: true } }),
          prisma.erpSlsCustomerAdvance.count({ where: baseCondition }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), amount: num(d.amount), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Amount', value: sumMoney(rows, 'amount'), type: 'money' as const }] };
      },
    },
    {
      key: 'payment-receipts',
      title: 'Penerimaan Pembayaran',
      group: 'document',
      columns: DOC_COLS_AR,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const dateFilter = dateRange('transactionDate', filters);
        const baseCondition: any = {
          deletedAt: null,
          ...(dateFilter.transactionDate ? { transactionDate: dateFilter.transactionDate } : {}),
          ...(filters.status ? { status: filters.status } : {}),
          ...(filters.search ? { docNumber: { contains: filters.search, mode: 'insensitive' } } : {}),
        };
        const [docs, total] = await Promise.all([
          prisma.erpFinArReceipt.findMany({ where: baseCondition, orderBy: { transactionDate: 'desc' }, skip, take, select: { id: true, docNumber: true, transactionDate: true, partnerId: true, amount: true, status: true } }),
          prisma.erpFinArReceipt.count({ where: baseCondition }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.partnerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.transactionDate), partner: display(partners.get(d.partnerId?.toString() ?? '') ?? null), amount: num(d.amount), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Amount', value: sumMoney(rows, 'amount'), type: 'money' as const }] };
      },
    },
    {
      key: 'proforma-invoices',
      title: 'Faktur Proforma',
      group: 'document',
      columns: [
        { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
        { key: 'docDate', header: 'Tanggal', type: 'date' },
        { key: 'customer', header: 'Pelanggan', type: 'text' },
        { key: 'grandTotal', header: 'Grand Total', type: 'money' },
        { key: 'status', header: 'Status', type: 'status' },
      ],
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsProformaInvoice.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, grandTotal: true, status: true } }),
          prisma.erpSlsProformaInvoice.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
    {
      key: 'packing-lists',
      title: 'Daftar Kemasan',
      group: 'document',
      columns: DOC_COLS_SIMPLE,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsPackingList.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, status: true } }),
          prisma.erpSlsPackingList.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), status: d.status }));
        return { rows, total };
      },
    },
    {
      key: 'delivery-orders',
      title: 'Surat Jalan',
      group: 'document',
      columns: DOC_COLS_SIMPLE,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsDeliveryOrder.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, status: true } }),
          prisma.erpSlsDeliveryOrder.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), status: d.status }));
        return { rows, total };
      },
    },
    {
      key: 'delivery-reports',
      title: 'Laporan Pengiriman',
      group: 'document',
      columns: DOC_COLS_SIMPLE,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsDeliveryReport.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, status: true } }),
          prisma.erpSlsDeliveryReport.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), status: d.status }));
        return { rows, total };
      },
    },
  ];
}
