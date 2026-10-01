import { Prisma } from '@prisma/client';
import { PurGoodsReceiptLineDto } from './dto/create-pur-goods-receipt.dto';
import { QueryPurGoodsReceiptsDto } from './dto/query-pur-goods-receipts.dto';
import { PurGoodsReceiptTransitionAction as A } from './dto/transition-pur-goods-receipt.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
};

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

export function buildPurGrnWhere(q: QueryPurGoodsReceiptsDto): Prisma.ErpPurGoodsReceiptWhereInput {
  const where: Prisma.ErpPurGoodsReceiptWhereInput = { deletedAt: null };
  if (q.status) where.status = q.status as never;
  if (q.branchId) where.branchId = BigInt(q.branchId);
  if (q.supplierId) where.supplierId = BigInt(q.supplierId);
  if (q.locationId) where.locationId = BigInt(q.locationId);
  if (q.createdById) where.createdById = BigInt(q.createdById);
  if (q.dateFrom || q.dateTo) {
    where.docDate = {
      ...(q.dateFrom ? { gte: new Date(q.dateFrom) } : {}),
      ...(q.dateTo ? { lte: new Date(q.dateTo) } : {}),
    };
  }
  if (q.docNumberFrom || q.docNumberTo) {
    where.docNumber = {
      ...(q.docNumberFrom ? { gte: q.docNumberFrom } : {}),
      ...(q.docNumberTo ? { lte: q.docNumberTo } : {}),
    };
  }
  if (q.description?.trim()) where.description = { contains: q.description.trim(), mode: 'insensitive' };
  if (q.search?.trim()) {
    const s = q.search.trim();
    where.OR = [{ docNumber: { contains: s, mode: 'insensitive' } }, { description: { contains: s, mode: 'insensitive' } }];
  }
  return where;
}

/**
 * Map a GRN line DTO → Prisma create. QC fields (acceptedQty/rejectedQty/
 * quarantineQty/qcStatus) have DB defaults so they're optional — if not
 * supplied, the line defaults to acceptedQty=0 / qcStatus=PENDING.
 */
export function mapGrnLine(
  line: PurGoodsReceiptLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpPurGoodsReceiptLineCreateWithoutGoodsReceiptInput {
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
    unitCost: line.unitCost != null ? new Prisma.Decimal(line.unitCost) : null,
    discountPercent: line.discountPercent != null ? new Prisma.Decimal(line.discountPercent) : null,
    discountAmount: line.discountAmount != null ? new Prisma.Decimal(line.discountAmount) : null,
    tax1Id: toBigInt(line.tax1Id),
    tax1Amount: line.tax1Amount != null ? new Prisma.Decimal(line.tax1Amount) : null,
    tax2Id: toBigInt(line.tax2Id),
    tax2Amount: line.tax2Amount != null ? new Prisma.Decimal(line.tax2Amount) : null,
    // QC delta — optional; DB defaults cover the rest.
    ...(line.acceptedQty != null ? { acceptedQty: new Prisma.Decimal(line.acceptedQty) } : {}),
    ...(line.rejectedQty != null ? { rejectedQty: new Prisma.Decimal(line.rejectedQty) } : {}),
    ...(line.quarantineQty != null ? { quarantineQty: new Prisma.Decimal(line.quarantineQty) } : {}),
    ...(line.qcStatus ? { qcStatus: line.qcStatus as never } : {}),
    warehouseId: toBigInt(line.warehouseId),
    inventoryAccountId: toBigInt(line.inventoryAccountId),
    accruedPayableAccountId: toBigInt(line.accruedPayableAccountId),
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    // orderLineId has a @relation — use connect/null rather than raw scalar setter.
    ...(line.orderLineId ? { orderLine: { connect: { id: toBigInt(line.orderLineId)! } } } : {}),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}

/** GRN subtotal = sum of line qty×unitPrice (same formula as PO). */
export function lineNet(line: PurGoodsReceiptLineDto): Prisma.Decimal {
  const gross = dec(line.quantity).mul(dec(line.unitPrice));
  if (line.discountAmount != null) return gross.sub(new Prisma.Decimal(line.discountAmount));
  if (line.discountPercent != null) return gross.sub(gross.mul(new Prisma.Decimal(line.discountPercent)).div(100));
  return gross;
}

export function computeTotals(
  lines: PurGoodsReceiptLineDto[],
  header: { discountAmount?: string; tax1Amount?: string; tax2Amount?: string; otherCostAmount?: string },
): { subtotal: Prisma.Decimal; grandTotal: Prisma.Decimal } {
  const subtotal = (lines ?? []).reduce((s, l) => s.add(lineNet(l)), new Prisma.Decimal(0));
  const lineTax = (lines ?? []).reduce((s, l) => s.add(dec(l.tax1Amount)).add(dec(l.tax2Amount)), new Prisma.Decimal(0));
  const grandTotal = subtotal.add(lineTax).add(dec(header.tax1Amount)).add(dec(header.tax2Amount)).add(dec(header.otherCostAmount)).sub(dec(header.discountAmount));
  return { subtotal, grandTotal };
}
