import { Prisma } from '@prisma/client';
import { InvStockMovementLineDto, ErpStockMovementTypeDto } from './dto/create-inv-stock-movement.dto';
import { QueryInvStockMovementsDto } from './dto/query-inv-stock-movements.dto';
import { InvStockMovementTransitionAction as A } from './dto/transition-inv-stock-movement.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

/** Statuses where header/lines may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** valid (status, action) → next status. POST/REOPEN handled separately. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

/** Document-numbering code per movement type (own running number per transaction). */
export const DOC_CODE_BY_TYPE: Record<ErpStockMovementTypeDto, string> = {
  [ErpStockMovementTypeDto.REQUEST]: 'MR',
  [ErpStockMovementTypeDto.TRANSFER]: 'TS',
  [ErpStockMovementTypeDto.TRANSFER_RECEIPT]: 'RS',
  [ErpStockMovementTypeDto.ISSUE]: 'RF',
  [ErpStockMovementTypeDto.RETURN]: 'SR',
};

export function buildInvMovementWhere(
  query: QueryInvStockMovementsDto,
): Prisma.ErpInvStockMovementWhereInput {
  const where: Prisma.ErpInvStockMovementWhereInput = { deletedAt: null };

  if (query.movementType) where.movementType = query.movementType as never;
  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.locationId) where.locationId = BigInt(query.locationId);
  if (query.createdById) where.createdById = BigInt(query.createdById);
  if (query.warehouseId) {
    const wid = BigInt(query.warehouseId);
    where.OR = [
      { sourceWarehouseId: wid },
      { transitWarehouseId: wid },
      { destinationWarehouseId: wid },
    ];
  }
  if (query.dateFrom || query.dateTo) {
    where.movementDate = {
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
    where.AND = [
      {
        OR: [
          { docNumber: { contains: q, mode: 'insensitive' } },
          { description: { contains: q, mode: 'insensitive' } },
        ],
      },
    ];
  }
  return where;
}

/**
 * Map a line DTO → Prisma create input for ErpInvStockMovementLine. Populates ALL
 * NOT NULL columns (quantity, unitId, unitValue, baseQuantity, baseUnitId) —
 * unitValue defaults to 1 and the base unit/qty derive from the transaction unit
 * (no UoM conversion table wired yet; admin enters in base unit).
 */
export function mapMovementLine(
  line: InvStockMovementLineDto,
): Prisma.ErpInvStockMovementLineCreateWithoutStockMovementInput {
  return {
    itemId: BigInt(line.itemId),
    quantity: dec(line.quantity),
    unitId: BigInt(line.unitId),
    unitValue: new Prisma.Decimal(1),
    baseQuantity: dec(line.quantity),
    baseUnitId: BigInt(line.unitId),
    unitCost: line.unitCost != null ? new Prisma.Decimal(line.unitCost) : null,
    salePrice: line.salePrice != null ? new Prisma.Decimal(line.salePrice) : null,
    sourceWarehouseId: toBigInt(line.sourceWarehouseId),
    destinationWarehouseId: toBigInt(line.destinationWarehouseId),
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}
