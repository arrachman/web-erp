import { Prisma } from '@prisma/client';
import { MfgWorkOrderLineDto } from './dto/create-mfg-work-order.dto';
import { QueryMfgWorkOrdersDto } from './dto/query-mfg-work-orders.dto';
import { MfgWoTransitionAction as A } from './dto/transition-mfg-work-order.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where header/lines may still be edited. */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/**
 * Valid (status, action) → next status.
 * Work Order is a planning doc — no POSTED state.
 */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE', [A.REOPEN]: 'DRAFT' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.REOPEN]: 'DRAFT' },
};

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

export function buildWorkOrderWhere(
  query: QueryMfgWorkOrdersDto,
): Prisma.ErpMfgWorkOrderWhereInput {
  const where: Prisma.ErpMfgWorkOrderWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.locationId) where.locationId = BigInt(query.locationId);
  if (query.bomId) where.bomId = BigInt(query.bomId);
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
 * Map a line DTO → Prisma create input for ErpMfgWorkOrderInput.
 * Populates all NOT NULL columns with sensible defaults.
 */
export function mapWorkOrderInputLine(
  line: MfgWorkOrderLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpMfgWorkOrderInputCreateWithoutWorkOrderInput {
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
    unitCost: dec(line.unitCost),
    inventoryAccountId: toBigInt(line.inventoryAccountId),
    sourceWarehouseId: toBigInt(line.sourceWarehouseId),
    productionWarehouseId: toBigInt(line.productionWarehouseId),
    destinationWarehouseId: toBigInt(line.destinationWarehouseId),
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}

/**
 * Map a line DTO → Prisma create input for ErpMfgWorkOrderOutput.
 */
export function mapWorkOrderOutputLine(
  line: MfgWorkOrderLineDto,
  header: { currencyId: string; exchangeRate: string },
): Prisma.ErpMfgWorkOrderOutputCreateWithoutWorkOrderInput {
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
    unitCost: dec(line.unitCost),
    inventoryAccountId: toBigInt(line.inventoryAccountId),
    sourceWarehouseId: toBigInt(line.sourceWarehouseId),
    productionWarehouseId: toBigInt(line.productionWarehouseId),
    destinationWarehouseId: toBigInt(line.destinationWarehouseId),
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}

/** Sum of input line totals (qty * unitPrice). */
export function computeInputTotal(lines: MfgWorkOrderLineDto[]): Prisma.Decimal {
  return (lines ?? []).reduce(
    (s, l) => s.add(dec(l.quantity).mul(dec(l.unitPrice))),
    new Prisma.Decimal(0),
  );
}

/** Sum of output line totals (qty * unitPrice). */
export function computeOutputTotal(lines: MfgWorkOrderLineDto[]): Prisma.Decimal {
  return (lines ?? []).reduce(
    (s, l) => s.add(dec(l.quantity).mul(dec(l.unitPrice))),
    new Prisma.Decimal(0),
  );
}
