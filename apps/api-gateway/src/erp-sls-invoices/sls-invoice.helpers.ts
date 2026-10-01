import { Prisma } from '@prisma/client';
import { SlsInvoiceLineDto } from './dto/create-sls-invoice.dto';
import { QuerySlsInvoicesDto } from './dto/query-sls-invoices.dto';
import { SlsInvoiceTransitionAction as A } from './dto/transition-sls-invoice.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where header/lines may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** valid (status, action) → next status. POST/REOPEN handled separately. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
};

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

export function buildSlsInvoiceWhere(query: QuerySlsInvoicesDto): Prisma.ErpSlsInvoiceWhereInput {
  const where: Prisma.ErpSlsInvoiceWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.customerId) where.customerId = BigInt(query.customerId);
  if (query.locationId) where.locationId = BigInt(query.locationId);
  if (query.orderId) where.orderId = BigInt(query.orderId);
  if (query.deliveryOrderId) where.deliveryOrderId = BigInt(query.deliveryOrderId);
  if (query.isOpeningBalance !== undefined) where.isOpeningBalance = query.isOpeningBalance;
  if (query.settlementStatus) where.settlementStatus = query.settlementStatus as never;
  if (query.createdById) where.createdById = BigInt(query.createdById);
  if (query.dateFrom || query.dateTo) {
    where.docDate = {
      ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
      ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
    };
  }
  if (query.docNumberFrom || query.docNumberTo) {
    where.docNumber = {
      ...(query.docNumberFrom ? { gte: query.docNumberFrom } : {}),
      ...(query.docNumberTo ? { lte: query.docNumberTo } : {}),
    };
  }
  if (query.description?.trim()) {
    where.description = { contains: query.description.trim(), mode: 'insensitive' };
  }
  if (query.search?.trim()) {
    const q = query.search.trim();
    where.OR = [
      { docNumber: { contains: q, mode: 'insensitive' } },
      { code: { contains: q, mode: 'insensitive' } },
      { description: { contains: q, mode: 'insensitive' } },
    ];
  }
  return where;
}

/**
 * Map a line DTO → Prisma create input for ErpSlsInvoiceLine.
 */
export function mapInvoiceLine(
  line: SlsInvoiceLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpSlsInvoiceLineCreateWithoutInvoiceInput {
  return {
    itemId: BigInt(line.itemId),
    quantity: dec(line.quantity),
    unitId: BigInt(line.unitId),
    unitValue: new Prisma.Decimal(1),
    baseQuantity: dec(line.quantity),
    baseUnitId: BigInt(line.unitId),
    currencyId: BigInt(header.currencyId),
    exchangeRate: new Prisma.Decimal(header.exchangeRate),
    unitPrice: dec(line.unitPrice),
    discountPercent: line.discountPercent != null ? new Prisma.Decimal(line.discountPercent) : null,
    discountAmount: line.discountAmount != null ? new Prisma.Decimal(line.discountAmount) : null,
    tax1Id: toBigInt(line.tax1Id),
    tax1Amount: line.tax1Amount != null ? new Prisma.Decimal(line.tax1Amount) : null,
    tax2Id: toBigInt(line.tax2Id),
    tax2Amount: line.tax2Amount != null ? new Prisma.Decimal(line.tax2Amount) : null,
    warehouseId: toBigInt(line.warehouseId),
    inventoryAccountId: toBigInt(line.inventoryAccountId),
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}

const HUNDRED = new Prisma.Decimal(100);
const ZERO = new Prisma.Decimal(0);
const r4 = (d: Prisma.Decimal) => d.toDecimalPlaces(4);

export function lineGross(line: SlsInvoiceLineDto): Prisma.Decimal {
  return dec(line.quantity).mul(dec(line.unitPrice));
}

export function lineDiscount(line: SlsInvoiceLineDto): Prisma.Decimal {
  if (line.discountAmount != null) return dec(line.discountAmount);
  if (line.discountPercent != null) return lineGross(line).mul(dec(line.discountPercent)).div(HUNDRED);
  return ZERO;
}

export function lineNet(line: SlsInvoiceLineDto): Prisma.Decimal {
  return lineGross(line).sub(lineDiscount(line));
}

type PriceMode = 'TAX_INCLUSIVE' | 'TAX_EXCLUSIVE';

function lineTaxAmounts(
  line: SlsInvoiceLineDto,
  rateById: Map<string, Prisma.Decimal>,
  priceMode: PriceMode,
): { tax1: Prisma.Decimal | null; tax2: Prisma.Decimal | null; base: Prisma.Decimal } {
  const net = lineNet(line);
  const r1 = line.tax1Id ? rateById.get(line.tax1Id) ?? ZERO : ZERO;
  const r2 = line.tax2Id ? rateById.get(line.tax2Id) ?? ZERO : ZERO;

  if (priceMode === 'TAX_INCLUSIVE') {
    const totalRate = r1.add(r2);
    if (totalRate.lte(0)) return { tax1: null, tax2: null, base: net };
    const base = net.div(totalRate.div(HUNDRED).add(1));
    const taxTotal = net.sub(base);
    const tax1 = line.tax1Id ? r4(taxTotal.mul(r1).div(totalRate)) : null;
    const tax2 = line.tax2Id ? (tax1 != null ? r4(taxTotal.sub(tax1)) : r4(taxTotal)) : null;
    return { tax1, tax2, base: r4(base) };
  }

  const tax1 = line.tax1Id ? r4(net.mul(r1).div(HUNDRED)) : null;
  const tax2 = line.tax2Id ? r4(net.mul(r2).div(HUNDRED)) : null;
  return { tax1, tax2, base: net };
}

export interface InvoiceTotalsResult {
  subtotal: Prisma.Decimal;
  grandTotal: Prisma.Decimal;
  lines: SlsInvoiceLineDto[];
  discountAmount: Prisma.Decimal | null;
  otherCostAmount: Prisma.Decimal | null;
}

export function computeInvoiceTotals(
  rawLines: SlsInvoiceLineDto[],
  header: {
    discountPercent?: string;
    discountAmount?: string;
    tax1Amount?: string;
    tax2Amount?: string;
    otherCostPercent?: string;
    otherCostAmount?: string;
  },
  rateById: Map<string, Prisma.Decimal>,
  priceMode: PriceMode,
): InvoiceTotalsResult {
  let subtotal = ZERO;
  let lineTaxTotal = ZERO;
  const lines = (rawLines ?? []).map((l) => {
    const { tax1, tax2, base } = lineTaxAmounts(l, rateById, priceMode);
    subtotal = subtotal.add(base);
    if (tax1) lineTaxTotal = lineTaxTotal.add(tax1);
    if (tax2) lineTaxTotal = lineTaxTotal.add(tax2);
    return { ...l, tax1Amount: tax1?.toString(), tax2Amount: tax2?.toString() };
  });
  subtotal = r4(subtotal);

  const discountAmount =
    header.discountAmount != null
      ? dec(header.discountAmount)
      : header.discountPercent != null
        ? r4(subtotal.mul(dec(header.discountPercent)).div(HUNDRED))
        : null;
  const otherCostAmount =
    header.otherCostAmount != null
      ? dec(header.otherCostAmount)
      : header.otherCostPercent != null
        ? r4(subtotal.mul(dec(header.otherCostPercent)).div(HUNDRED))
        : null;

  const grandTotal = r4(
    subtotal
      .add(lineTaxTotal)
      .add(dec(header.tax1Amount))
      .add(dec(header.tax2Amount))
      .add(otherCostAmount ?? ZERO)
      .sub(discountAmount ?? ZERO),
  );

  return { subtotal, grandTotal, lines, discountAmount, otherCostAmount };
}
