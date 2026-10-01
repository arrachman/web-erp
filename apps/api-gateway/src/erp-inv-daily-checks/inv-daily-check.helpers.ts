import { Prisma } from '@prisma/client';
import { InvDailyCheckLineDto } from './dto/create-inv-daily-check.dto';
import { QueryInvDailyChecksDto } from './dto/query-inv-daily-checks.dto';
import { InvDailyCheckTransitionAction as A } from './dto/transition-inv-daily-check.dto';

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
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

/** Document-numbering code for daily checks. */
export const DC_DOC_CODE = 'DC';

export function buildDailyCheckWhere(
  query: QueryInvDailyChecksDto,
): Prisma.ErpInvDailyCheckWhereInput {
  const where: Prisma.ErpInvDailyCheckWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.locationId) where.locationId = BigInt(query.locationId);
  if (query.createdById) where.createdById = BigInt(query.createdById);
  if (query.dateFrom || query.dateTo) {
    where.checkDate = {
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
 * Map a line DTO → Prisma create input for ErpInvDailyCheckLine.
 */
export function mapDailyCheckLine(
  line: InvDailyCheckLineDto,
): Prisma.ErpInvDailyCheckLineCreateWithoutDailyCheckInput {
  return {
    itemId: BigInt(line.itemId),
    quantity: new Prisma.Decimal(line.quantity),
    unitId: BigInt(line.unitId),
    warehouseId: toBigInt(line.warehouseId),
    costCenterId: toBigInt(line.costCenterId),
    notes: line.notes ?? null,
    machineRef: line.machineRef ?? null,
    workHours: line.workHours ? new Prisma.Decimal(line.workHours) : null,
    lineNo: line.lineNo,
  };
}
