import { Prisma } from '@prisma/client';
import { PurInvoiceLineDto } from './dto/create-pur-invoice.dto';
import { QueryPurInvoicesDto } from './dto/query-pur-invoices.dto';
import { PurInvoiceTransitionAction as A } from './dto/transition-pur-invoice.dto';

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

export function buildPurInvoiceWhere(query: QueryPurInvoicesDto): Prisma.ErpPurInvoiceWhereInput {
  const where: Prisma.ErpPurInvoiceWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.isOpeningBalance !== undefined) where.isOpeningBalance = query.isOpeningBalance;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.supplierId) where.supplierId = BigInt(query.supplierId);
  if (query.locationId) where.locationId = BigInt(query.locationId);
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
      { description: { contains: q, mode: 'insensitive' } },
    ];
  }
  return where;
}

/**
 * Map a line DTO → Prisma create input for ErpPurInvoiceLine. Populates ALL
 * NOT NULL columns (quantity, unitId, unitValue, baseQuantity, baseUnitId,
 * currencyId, exchangeRate, unitPrice) — defaults derived from the header for
 * the currency/rate and from the transaction unit for the base unit/qty.
 */
export function mapInvoiceLine(
  line: PurInvoiceLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpPurInvoiceLineCreateWithoutInvoiceInput {
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

/** Net of one line: qty*unitPrice minus line discount (explicit amount or %). */
export function lineNet(line: PurInvoiceLineDto): Prisma.Decimal {
  const gross = dec(line.quantity).mul(dec(line.unitPrice));
  let discount: Prisma.Decimal;
  if (line.discountAmount != null) {
    discount = new Prisma.Decimal(line.discountAmount);
  } else if (line.discountPercent != null) {
    discount = gross.mul(new Prisma.Decimal(line.discountPercent)).div(100);
  } else {
    discount = new Prisma.Decimal(0);
  }
  return gross.sub(discount);
}

/**
 * Server-side totals. subtotal = Σ line net; grandTotal = subtotal
 * + Σ(line tax1+tax2) + header tax1+tax2 + otherCost − header discount.
 */
export function computeTotals(
  lines: PurInvoiceLineDto[],
  header: {
    discountAmount?: string;
    tax1Amount?: string;
    tax2Amount?: string;
    otherCostAmount?: string;
  },
): { subtotal: Prisma.Decimal; grandTotal: Prisma.Decimal } {
  const subtotal = (lines ?? []).reduce(
    (s, l) => s.add(lineNet(l)),
    new Prisma.Decimal(0),
  );
  const lineTax = (lines ?? []).reduce(
    (s, l) => s.add(dec(l.tax1Amount)).add(dec(l.tax2Amount)),
    new Prisma.Decimal(0),
  );
  const grandTotal = subtotal
    .add(lineTax)
    .add(dec(header.tax1Amount))
    .add(dec(header.tax2Amount))
    .add(dec(header.otherCostAmount))
    .sub(dec(header.discountAmount));
  return { subtotal, grandTotal };
}
