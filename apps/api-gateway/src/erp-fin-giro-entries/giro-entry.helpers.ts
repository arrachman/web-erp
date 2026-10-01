import { Prisma } from '@prisma/client';
import { QueryGiroEntryDto } from './dto/query-giro-entry.dto';
import { GiroTransitionAction as A } from './dto/transition-giro-entry.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where header/instruments may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/**
 * valid (status, action) → next status. POST/REOPEN handled separately.
 * Identical to the journal state machine (incl. POSTED → REOPEN → DRAFT).
 */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

/** (kind,type) → transaction-type code (Grid / Form Builder key). */
export const CODE_BY: Record<string, string> = {
  REGISTER_INCOMING: 'FIN.RG',
  REGISTER_OUTGOING: 'FIN.SG',
  CLEAR_INCOMING: 'FIN.RGC',
  CLEAR_OUTGOING: 'FIN.SGC',
};

/** code key (RG/SG/RGC/SGC) → numbering documentCode. */
export const DOC_CODE: Record<string, string> = {
  RG: 'RECEIPT_GIRO',
  SG: 'SEND_GIRO',
  RGC: 'RECEIPT_GIRO_CLEARING',
  SGC: 'SEND_GIRO_CLEARING',
};

/** Fallback prefix when no numbering row exists yet. */
export const FALLBACK_PREFIX: Record<string, string> = {
  RG: 'RG',
  SG: 'SG',
  RGC: 'RGC',
  SGC: 'SGC',
};

/** Derive the short code key from (kind, type). */
export function codeKeyFor(kind: string, type: string): 'RG' | 'SG' | 'RGC' | 'SGC' {
  if (kind === 'REGISTER') return type === 'INCOMING' ? 'RG' : 'SG';
  return type === 'INCOMING' ? 'RGC' : 'SGC';
}

export function buildGiroEntryWhere(
  query: QueryGiroEntryDto,
): Prisma.ErpFinGiroEntryWhereInput {
  const where: Prisma.ErpFinGiroEntryWhereInput = { deletedAt: null };

  if (query.kind) where.kind = query.kind as never;
  if (query.type) where.type = query.type as never;
  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.partnerId) where.partnerId = BigInt(query.partnerId);
  if (query.dateFrom || query.dateTo) {
    where.entryDate = {
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
