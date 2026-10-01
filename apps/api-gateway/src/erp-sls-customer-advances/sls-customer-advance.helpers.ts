import { Prisma } from '@prisma/client';
import { QuerySlsCustomerAdvancesDto } from './dto/query-sls-customer-advances.dto';
import { SlsCustomerAdvanceTransitionAction as A } from './dto/transition-sls-customer-advance.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where the document may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** valid (status, action) → next status. POST/REOPEN handled separately. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
};

export function buildCustomerAdvanceWhere(
  query: QuerySlsCustomerAdvancesDto,
): Prisma.ErpSlsCustomerAdvanceWhereInput {
  const where: Prisma.ErpSlsCustomerAdvanceWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.settlementStatus) where.settlementStatus = query.settlementStatus as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.customerId) where.customerId = BigInt(query.customerId);
  if (query.createdById) where.createdById = BigInt(query.createdById);
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
      { code: { contains: q, mode: 'insensitive' } },
      { description: { contains: q, mode: 'insensitive' } },
    ];
  }
  return where;
}
