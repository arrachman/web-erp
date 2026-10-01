import { Prisma } from '@prisma/client';
import { QueryApPaymentDto } from './dto/query-ap-payment.dto';
import { ApPaymentTransitionAction as A } from './dto/transition-ap-payment.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where header/instruments/allocations may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** valid (status, action) → next status. POST/REOPEN handled separately. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

export function buildApPaymentWhere(query: QueryApPaymentDto): Prisma.ErpFinApPaymentWhereInput {
  const where: Prisma.ErpFinApPaymentWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.paymentStatus) where.paymentStatus = query.paymentStatus as never;
  if (query.source) where.source = query.source;
  if (query.partnerId) where.partnerId = BigInt(query.partnerId);
  if (query.dateFrom || query.dateTo) {
    where.transactionDate = {
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
