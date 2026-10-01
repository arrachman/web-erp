import { Prisma } from '@prisma/client';
import { MfgBomLineDto } from './dto/create-mfg-bom.dto';
import { QueryMfgBomsDto } from './dto/query-mfg-boms.dto';
import { MfgBomTransitionAction as A } from './dto/transition-mfg-bom.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

/** Statuses where header and lines may still be edited. */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** Valid (currentStatus, action) → nextStatus transitions. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.REOPEN]: 'DRAFT' },
};

export function buildBomWhere(query: QueryMfgBomsDto): Prisma.ErpMfgBomWhereInput {
  const where: Prisma.ErpMfgBomWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.dateFrom || query.dateTo) {
    where.docDate = {
      ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
      ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
    };
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
 * Map a BOM line DTO → Prisma create input for ErpMfgBomInput.
 * All NOT NULL columns (unitValue, baseQuantity, baseUnitId, currencyId,
 * exchangeRate) are defaulted from the header or sensible defaults.
 */
export function mapBomInputLine(
  l: MfgBomLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpMfgBomInputCreateWithoutBomInput {
  return {
    itemId: BigInt(l.itemId),
    quantity: dec(l.quantity),
    unitId: BigInt(l.unitId),
    unitValue: new Prisma.Decimal(1),
    baseQuantity: dec(l.quantity),
    baseUnitId: BigInt(l.unitId),
    currencyId: BigInt(header.currencyId),
    exchangeRate: new Prisma.Decimal(header.exchangeRate),
    unitPrice: dec(l.unitPrice),
    unitCost: dec(l.unitCost),
    costPercent: l.costPercent != null ? new Prisma.Decimal(l.costPercent) : null,
    sourceWarehouseId: toBigInt(l.sourceWarehouseId),
    productionWarehouseId: toBigInt(l.productionWarehouseId),
    destinationWarehouseId: toBigInt(l.destinationWarehouseId),
    inventoryAccountId: toBigInt(l.inventoryAccountId),
    costCenterId: toBigInt(l.costCenterId),
    divisionId: toBigInt(l.divisionId),
    subdivisionId: toBigInt(l.subdivisionId),
    projectId: toBigInt(l.projectId),
    notes: l.notes ?? null,
    lineNo: l.lineNo,
  };
}

/**
 * Map a BOM line DTO → Prisma create input for ErpMfgBomOutput.
 */
export function mapBomOutputLine(
  l: MfgBomLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpMfgBomOutputCreateWithoutBomInput {
  return {
    itemId: BigInt(l.itemId),
    quantity: dec(l.quantity),
    unitId: BigInt(l.unitId),
    unitValue: new Prisma.Decimal(1),
    baseQuantity: dec(l.quantity),
    baseUnitId: BigInt(l.unitId),
    currencyId: BigInt(header.currencyId),
    exchangeRate: new Prisma.Decimal(header.exchangeRate),
    unitPrice: dec(l.unitPrice),
    unitCost: dec(l.unitCost),
    sourceWarehouseId: toBigInt(l.sourceWarehouseId),
    productionWarehouseId: toBigInt(l.productionWarehouseId),
    destinationWarehouseId: toBigInt(l.destinationWarehouseId),
    inventoryAccountId: toBigInt(l.inventoryAccountId),
    costCenterId: toBigInt(l.costCenterId),
    divisionId: toBigInt(l.divisionId),
    subdivisionId: toBigInt(l.subdivisionId),
    projectId: toBigInt(l.projectId),
    notes: l.notes ?? null,
    lineNo: l.lineNo,
  };
}

/**
 * Compute aggregate totals from input and output lines.
 * inputTotal / outputTotal = Σ (qty * unitPrice) and Σ (qty * unitCost).
 */
export function computeBomTotals(
  inputs: MfgBomLineDto[],
  outputs: MfgBomLineDto[],
): {
  inputTotalPrice: Prisma.Decimal;
  inputTotalCost: Prisma.Decimal;
  outputTotalPrice: Prisma.Decimal;
  outputTotalCost: Prisma.Decimal;
} {
  const sumLines = (lines: MfgBomLineDto[]) =>
    (lines ?? []).reduce(
      (acc, l) => ({
        price: acc.price.add(dec(l.quantity).mul(dec(l.unitPrice))),
        cost: acc.cost.add(dec(l.quantity).mul(dec(l.unitCost))),
      }),
      { price: new Prisma.Decimal(0), cost: new Prisma.Decimal(0) },
    );

  const inp = sumLines(inputs);
  const out = sumLines(outputs);
  return {
    inputTotalPrice: inp.price,
    inputTotalCost: inp.cost,
    outputTotalPrice: out.price,
    outputTotalCost: out.cost,
  };
}
