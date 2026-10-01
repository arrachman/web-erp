import { Prisma } from '@prisma/client';
import { InvStockCountLineDto } from './dto/create-inv-stock-count.dto';
import { QueryInvStockCountsDto } from './dto/query-inv-stock-counts.dto';
import { InvStockCountTransitionAction as A } from './dto/transition-inv-stock-count.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

const dec = (v?: string | null) => new Prisma.Decimal(v ?? 0);

/** Document-numbering code for stock counts (opname). */
export const DOC_CODE = 'SP';

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

export function buildInvCountWhere(
  query: QueryInvStockCountsDto,
): Prisma.ErpInvStockCountWhereInput {
  const where: Prisma.ErpInvStockCountWhereInput = { deletedAt: null };

  if (query.countType) where.countType = query.countType as never;
  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.warehouseId) where.warehouseId = BigInt(query.warehouseId);
  if (query.createdById) where.createdById = BigInt(query.createdById);
  if (query.dateFrom || query.dateTo) {
    where.countDate = {
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
 * Map a line DTO → Prisma create input for ErpInvStockCountLine. Populates ALL
 * NOT NULL columns. Quantities derive from the opname inputs:
 *   systemQty   = line.systemQty ?? 0
 *   physicalQty = required
 *   goodQty     = line.goodQty ?? physicalQty
 *   damagedQty  = line.damagedQty ?? 0
 *   varianceQty = physicalQty - systemQty
 * baseUnitId mirrors unitId (no UoM conversion table wired yet; admin enters in
 * base unit). warehouseId falls back to the header warehouse when omitted.
 */
export function mapCountLine(
  line: InvStockCountLineDto,
  headerWarehouseId: bigint,
): Prisma.ErpInvStockCountLineCreateWithoutStockCountInput {
  const systemQty = dec(line.systemQty);
  const physicalQty = dec(line.physicalQty);
  const goodQty = line.goodQty != null ? new Prisma.Decimal(line.goodQty) : physicalQty;
  const damagedQty = line.damagedQty != null ? new Prisma.Decimal(line.damagedQty) : new Prisma.Decimal(0);
  const varianceQty = physicalQty.minus(systemQty);

  return {
    itemId: BigInt(line.itemId),
    systemQty,
    physicalQty,
    goodQty,
    damagedQty,
    varianceQty,
    unitId: BigInt(line.unitId),
    baseUnitId: BigInt(line.unitId),
    warehouseId: toBigInt(line.warehouseId) ?? headerWarehouseId,
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    notes: line.notes ?? null,
    lineNo: line.lineNo,
  };
}
