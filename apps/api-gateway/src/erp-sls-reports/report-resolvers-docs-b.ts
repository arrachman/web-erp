/**
 * Sales document-list resolvers B: invoices, freight-receivables, return-receipts,
 * returns, ar-collections, ar-payments, invoice-swaps, opening-ar-balance.
 */

import { PrismaService } from '../prisma/prisma.service';
import { dateRange, display, isoDate, num, paginate, resolvePartners, baseWhere } from './report-helpers';
import { ReportDef } from './report-types';
import {
  DOC_COLS_AR, DOC_COLS_FREIGHT, DOC_COLS_GRAND, DOC_COLS_INVOICE, DOC_COLS_SIMPLE, sumMoney,
} from './report-resolvers-docs-cols';

export function buildDocReportsB(prisma: PrismaService): ReportDef[] {
  return [
    {
      key: 'invoices',
      title: 'Faktur Penjualan',
      group: 'document',
      columns: DOC_COLS_INVOICE,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsInvoice.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, subtotal: true, tax1Amount: true, grandTotal: true, status: true } }),
          prisma.erpSlsInvoice.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), subtotal: num(d.subtotal), tax1Amount: num(d.tax1Amount), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Subtotal', value: sumMoney(rows, 'subtotal'), type: 'money' as const }, { label: 'Total PPN', value: sumMoney(rows, 'tax1Amount'), type: 'money' as const }, { label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
    {
      key: 'freight-receivables',
      title: 'Piutang Angkutan',
      group: 'document',
      columns: DOC_COLS_FREIGHT,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = { ...baseWhere('docDate', filters), otherCostAmount: { not: null } };
        const [docs, total] = await Promise.all([
          prisma.erpSlsInvoice.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, otherCostAmount: true, status: true } }),
          prisma.erpSlsInvoice.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), otherCostAmount: num(d.otherCostAmount), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Biaya', value: sumMoney(rows, 'otherCostAmount'), type: 'money' as const }] };
      },
    },
    {
      key: 'return-receipts',
      title: 'Tanda Terima Retur',
      group: 'document',
      columns: DOC_COLS_GRAND,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsReturnReceipt.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, grandTotal: true, status: true } }),
          prisma.erpSlsReturnReceipt.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
    {
      key: 'returns',
      title: 'Retur Penjualan',
      group: 'document',
      columns: DOC_COLS_GRAND,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const where = baseWhere('docDate', filters);
        const [docs, total] = await Promise.all([
          prisma.erpSlsReturn.findMany({ where, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, grandTotal: true, status: true } }),
          prisma.erpSlsReturn.count({ where }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
    {
      key: 'ar-collections',
      title: 'Koleksi AR',
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
      key: 'ar-payments',
      title: 'Pembayaran AR',
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
      key: 'invoice-swaps',
      title: 'Tukar Faktur',
      group: 'document',
      columns: DOC_COLS_SIMPLE,
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
          prisma.erpSlsInvoiceSwap.findMany({ where: baseCondition, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, status: true } }),
          prisma.erpSlsInvoiceSwap.count({ where: baseCondition }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), status: d.status }));
        return { rows, total };
      },
    },
    {
      key: 'opening-ar-balance',
      title: 'Saldo Awal Piutang',
      group: 'document',
      columns: DOC_COLS_GRAND,
      resolve: async (filters) => {
        const { skip, take } = paginate(filters);
        const dateFilter = dateRange('docDate', filters);
        const baseCondition: any = {
          deletedAt: null,
          isOpeningBalance: true,
          ...(dateFilter.docDate ? { docDate: dateFilter.docDate } : {}),
          ...(filters.status ? { status: filters.status } : {}),
          ...(filters.search ? { docNumber: { contains: filters.search, mode: 'insensitive' } } : {}),
        };
        const [docs, total] = await Promise.all([
          prisma.erpSlsInvoice.findMany({ where: baseCondition, orderBy: { docDate: 'desc' }, skip, take, select: { id: true, docNumber: true, docDate: true, customerId: true, grandTotal: true, status: true } }),
          prisma.erpSlsInvoice.count({ where: baseCondition }),
        ]);
        const partners = await resolvePartners(prisma, docs.map((d) => d.customerId));
        const rows = docs.map((d) => ({ docNumber: d.docNumber, docDate: isoDate(d.docDate), customer: display(partners.get(d.customerId?.toString() ?? '') ?? null), grandTotal: num(d.grandTotal), status: d.status }));
        return { rows, total, summary: [{ label: 'Total Grand Total', value: sumMoney(rows, 'grandTotal'), type: 'money' as const }] };
      },
    },
  ];
}
