import { ErpDocumentStatus, ErpSchoolPipelineStage, ErpSettlementStatus } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

/**
 * A1 CRM insights: everything derived from a school's real sales documents.
 *
 * Sales documents reference the partner through the scalar `customer_id`
 * (no Prisma relation), so these helpers query each document model with
 * `customerId = partnerId` directly. Receipts use `partnerId`.
 */

export const CONTRACT_EXPIRY_ALERT_DAYS = 90;

const DOC_SELECT = {
  id: true,
  docNumber: true,
  docDate: true,
  status: true,
  grandTotal: true,
} as const;

export interface SchoolOrderSummary {
  quotationCount: number;
  orderCount: number;
  deliveryCount: number;
  invoiceCount: number;
  paidInvoiceCount: number;
  lastOrderAt: Date | null;
  lastInvoiceAt: Date | null;
  derivedPipelineStage: ErpSchoolPipelineStage;
}

const STAGE_RANK: Record<ErpSchoolPipelineStage, number> = {
  PROSPEK: 0,
  PENAWARAN: 1,
  PESANAN: 2,
  TERKIRIM: 3,
  LUNAS: 4,
};

export function furthestStage(
  a: ErpSchoolPipelineStage,
  b: ErpSchoolPipelineStage,
): ErpSchoolPipelineStage {
  return STAGE_RANK[a] >= STAGE_RANK[b] ? a : b;
}

export async function getSchoolOrderSummary(
  prisma: PrismaService,
  partnerId: bigint,
  manualStage: ErpSchoolPipelineStage,
): Promise<SchoolOrderSummary> {
  const base = { customerId: partnerId, deletedAt: null };
  const [quotationCount, orderCount, deliveryCount, invoiceCount, paidInvoiceCount, lastOrder, lastInvoice] =
    await Promise.all([
      prisma.erpSlsQuotation.count({ where: base }),
      prisma.erpSlsOrder.count({ where: base }),
      prisma.erpSlsDeliveryOrder.count({ where: base }),
      prisma.erpSlsInvoice.count({ where: base }),
      prisma.erpSlsInvoice.count({ where: { ...base, settlementStatus: ErpSettlementStatus.PAID } }),
      prisma.erpSlsOrder.findFirst({
        where: base,
        orderBy: { docDate: 'desc' },
        select: { docDate: true },
      }),
      prisma.erpSlsInvoice.findFirst({
        where: base,
        orderBy: { docDate: 'desc' },
        select: { docDate: true },
      }),
    ]);

  let derived: ErpSchoolPipelineStage = ErpSchoolPipelineStage.PROSPEK;
  if (quotationCount > 0) derived = ErpSchoolPipelineStage.PENAWARAN;
  if (orderCount > 0) derived = ErpSchoolPipelineStage.PESANAN;
  if (deliveryCount > 0) derived = ErpSchoolPipelineStage.TERKIRIM;
  if (paidInvoiceCount > 0) derived = ErpSchoolPipelineStage.LUNAS;

  return {
    quotationCount,
    orderCount,
    deliveryCount,
    invoiceCount,
    paidInvoiceCount,
    lastOrderAt: lastOrder?.docDate ?? null,
    lastInvoiceAt: lastInvoice?.docDate ?? null,
    derivedPipelineStage: furthestStage(manualStage, derived),
  };
}

/** Unified cross-channel order timeline for one school (newest first). */
export async function buildOrderHistory(prisma: PrismaService, partnerId: bigint) {
  const base = { customerId: partnerId, deletedAt: null };
  const take = 50;
  const [quotations, orders, deliveries, invoices, receipts] = await Promise.all([
    prisma.erpSlsQuotation.findMany({ where: base, orderBy: { docDate: 'desc' }, take, select: DOC_SELECT }),
    prisma.erpSlsOrder.findMany({ where: base, orderBy: { docDate: 'desc' }, take, select: DOC_SELECT }),
    prisma.erpSlsDeliveryOrder.findMany({ where: base, orderBy: { docDate: 'desc' }, take, select: DOC_SELECT }),
    prisma.erpSlsInvoice.findMany({
      where: base,
      orderBy: { docDate: 'desc' },
      take,
      select: { ...DOC_SELECT, channel: true, settlementStatus: true },
    }),
    prisma.erpFinArReceipt.findMany({
      where: { partnerId, deletedAt: null },
      orderBy: { transactionDate: 'desc' },
      take,
      select: { id: true, docNumber: true, transactionDate: true, status: true, amount: true, paymentStatus: true },
    }),
  ]);

  const rows = [
    ...quotations.map((d) => ({ type: 'QUOTATION', ...d, channel: null as string | null, settlementStatus: null as string | null })),
    ...orders.map((d) => ({ type: 'ORDER', ...d, channel: null as string | null, settlementStatus: null as string | null })),
    ...deliveries.map((d) => ({ type: 'DELIVERY', ...d, channel: null as string | null, settlementStatus: null as string | null })),
    ...invoices.map((d) => ({ type: 'INVOICE', ...d })),
    ...receipts.map((d) => ({
      type: 'RECEIPT',
      id: d.id,
      docNumber: d.docNumber,
      docDate: d.transactionDate,
      status: d.status,
      grandTotal: d.amount,
      channel: null as string | null,
      settlementStatus: d.paymentStatus as string | null,
    })),
  ];

  rows.sort((a, b) => new Date(b.docDate).getTime() - new Date(a.docDate).getTime());
  return rows.slice(0, 100);
}

/** Partner ids (from `candidateIds`) that have NO sales order in `year`. */
export async function getNotOrderedIds(
  prisma: PrismaService,
  candidateIds: bigint[],
  year: number,
): Promise<bigint[]> {
  if (candidateIds.length === 0) return [];
  const ordered = await prisma.erpSlsOrder.findMany({
    where: {
      customerId: { in: candidateIds },
      deletedAt: null,
      status: { not: ErpDocumentStatus.REJECTED },
      docDate: { gte: new Date(Date.UTC(year, 0, 1)), lte: new Date(Date.UTC(year, 11, 31)) },
    },
    select: { customerId: true },
    distinct: ['customerId'],
  });
  const orderedSet = new Set(ordered.map((r) => r.customerId?.toString()));
  return candidateIds.filter((id) => !orderedSet.has(id.toString()));
}

export function contractExpiryWindow(now = new Date()) {
  const end = new Date(now);
  end.setDate(end.getDate() + CONTRACT_EXPIRY_ALERT_DAYS);
  return { gte: now, lte: end };
}
