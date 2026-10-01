import { Prisma } from '@prisma/client';
import {
  prismaDateFilter,
  resolveDateRange,
} from '../common/utils/date-range.util';
import { CreateJournalLineDto } from './dto/create-journal-entry.dto';
import { QueryJournalEntryDto } from './dto/query-journal-entry.dto';
import { JournalTransitionAction as A } from './dto/transition-journal-entry.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where header/lines may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/**
 * valid (status, action) → next status. POST/REOPEN handled separately.
 * REOPEN is allowed from APPROVED (un-approve) AND from POSTED (un-post): the
 * REOPEN handler reverses this document's ledger entries, so a posted doc can be
 * corrected by reopen → edit → re-post.
 */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

/** journalType → transaction-type code (Kustomisasi Grid / Form Builder key). */
export const CODE_BY_TYPE: Record<string, string> = {
  GENERAL: 'FIN.GJ',
  ADJUSTMENT: 'FIN.AJ',
  MEMORIAL: 'FIN.JM',
  OPENING_BALANCE: 'FIN.BB',
  REVALUATION: 'FIN.RV',
  CLOSING: 'FIN.CB',
};

/** journalType → numbering documentCode (GENERAL reuses the existing JV row). */
export const DOC_CODE: Record<string, string> = {
  GENERAL: 'JV',
  ADJUSTMENT: 'ADJUSTMENT_JOURNAL',
  MEMORIAL: 'MEMORIAL_JOURNAL',
  OPENING_BALANCE: 'OPENING_BALANCE',
  REVALUATION: 'FX_REVALUATION',
  CLOSING: 'CLOSING_JOURNAL',
};
/** Fallback prefix when no numbering row exists yet. */
export const FALLBACK_PREFIX: Record<string, string> = {
  GENERAL: 'JV',
  ADJUSTMENT: 'AJ',
  MEMORIAL: 'JM',
  OPENING_BALANCE: 'CB',
  REVALUATION: 'RV',
  CLOSING: 'CJ',
};

/** Map a detail-line DTO to a fin_journal_lines create row (per-line currency falls back to header). */
export function mapLine(
  line: CreateJournalLineDto,
  header: { currencyId: string; exchangeRate: string },
) {
  return {
    accountId: BigInt(line.accountId),
    currencyId: BigInt(line.currencyId ?? header.currencyId),
    exchangeRate: new Prisma.Decimal(line.exchangeRate ?? header.exchangeRate),
    debit: new Prisma.Decimal(line.debit ?? 0),
    credit: new Prisma.Decimal(line.credit ?? 0),
    debitFx: line.debitFx ? new Prisma.Decimal(line.debitFx) : null,
    creditFx: line.creditFx ? new Prisma.Decimal(line.creditFx) : null,
    notes: line.notes ?? null,
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    lineNo: line.lineNo,
  };
}

export function buildJournalWhere(
  query: QueryJournalEntryDto,
): Prisma.ErpFinJournalEntryWhereInput {
  const where: Prisma.ErpFinJournalEntryWhereInput = { deletedAt: null };

  if (query.journalType) where.journalType = query.journalType as never;
  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.partnerId) where.partnerId = BigInt(query.partnerId);
  if (query.createdById) where.createdById = BigInt(query.createdById);

  // Bounded date window: default last 31 days when caller omits range;
  // max span 366 days when both ends provided (throws BadRequest).
  const { from, to } = resolveDateRange({
    dateFrom: query.dateFrom,
    dateTo: query.dateTo,
    requireRange: true,
    defaultSpanDays: 31,
    maxSpanDays: 366,
    fieldLabel: 'Journal entryDate',
  });
  const dateFilter = prismaDateFilter(from, to);
  if (dateFilter) where.entryDate = dateFilter;
  if (query.docNumberFrom || query.docNumberTo) {
    where.docNumber = {
      ...(query.docNumberFrom ? { gte: query.docNumberFrom } : {}),
      ...(query.docNumberTo ? { lte: query.docNumberTo } : {}),
    };
  }
  if (query.description?.trim()) {
    where.description = { contains: query.description.trim(), mode: 'insensitive' };
  }
  if (query.notes?.trim()) {
    where.notes = { contains: query.notes.trim(), mode: 'insensitive' };
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
