import { Prisma } from '@prisma/client';
import { CashBankLineDto } from './dto/create-cash-bank-transaction.dto';
import { QueryCashBankTransactionDto } from './dto/query-cash-bank-transaction.dto';
import { CashBankTransitionAction as A } from './dto/transition-cash-bank-transaction.dto';

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
 * safely returned to DRAFT. Mirrors the journal/giro state machine.
 */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

/** (kind, direction) → numbering documentCode / fallback prefix. Kas=CR/CD, Bank=RM/SM. */
export const docKey = (kind: string, direction: string) => `${kind}_${direction}`;
export const FALLBACK_PREFIX: Record<string, string> = {
  CASH_RECEIPT: 'CR',
  CASH_DISBURSEMENT: 'CD',
  BANK_RECEIPT: 'RM',
  BANK_DISBURSEMENT: 'SM',
};
export const DOC_CODE: Record<string, string> = {
  CASH_RECEIPT: 'CASH_RECEIPT',
  CASH_DISBURSEMENT: 'CASH_DISBURSEMENT',
  BANK_RECEIPT: 'BANK_RECEIPT',
  BANK_DISBURSEMENT: 'BANK_DISBURSEMENT',
};

/** Marks giros owned by a cash/bank transaction (Giro tab) for sync/cleanup. */
export const GIRO_SOURCE = 'CASH_BANK_TXN';

/** RECEIPT → INCOMING giro, DISBURSEMENT → OUTGOING. */
export function giroType(direction: string) {
  return direction === 'RECEIPT' ? 'INCOMING' : 'OUTGOING';
}

export function sumAmount(lines: CashBankLineDto[], fallback?: string) {
  if (!lines?.length) return new Prisma.Decimal(fallback ?? 0);
  return lines.reduce((s, l) => s.add(new Prisma.Decimal(l.amount)), new Prisma.Decimal(0));
}

export function buildCashBankWhere(
  query: QueryCashBankTransactionDto,
): Prisma.ErpFinCashBankTransactionWhereInput {
  const where: Prisma.ErpFinCashBankTransactionWhereInput = { deletedAt: null };

  if (query.direction) where.direction = query.direction as never;
  if (query.kind) where.kind = query.kind as never;
  if (query.paymentMethod) where.paymentMethod = query.paymentMethod as never;
  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.locationId) where.locationId = BigInt(query.locationId);
  if (query.partnerId) where.partnerId = BigInt(query.partnerId);
  if (query.createdById) where.createdById = BigInt(query.createdById);
  if (query.dateFrom || query.dateTo) {
    where.transactionDate = {
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
  if (query.notes?.trim()) {
    where.notes = { contains: query.notes.trim(), mode: 'insensitive' };
  }
  if (query.search?.trim()) {
    const q = query.search.trim();
    where.OR = [
      { docNumber: { contains: q, mode: 'insensitive' } },
      { description: { contains: q, mode: 'insensitive' } },
      { contactPerson: { contains: q, mode: 'insensitive' } },
    ];
  }
  return where;
}

export function mapLine(
  line: CashBankLineDto,
  header: { currencyId: string; exchangeRate: string },
) {
  return {
    accountId: BigInt(line.accountId),
    currencyId: BigInt(line.currencyId ?? header.currencyId),
    exchangeRate: new Prisma.Decimal(line.exchangeRate ?? header.exchangeRate),
    amount: new Prisma.Decimal(line.amount),
    amountFx: line.amountFx ? new Prisma.Decimal(line.amountFx) : null,
    notes: line.notes ?? null,
    costCenterId: toBigInt(line.costCenterId),
    divisionId: toBigInt(line.divisionId),
    subdivisionId: toBigInt(line.subdivisionId),
    projectId: toBigInt(line.projectId),
    customFields: line.customFields
      ? (line.customFields as Prisma.InputJsonValue)
      : Prisma.JsonNull,
    lineNo: line.lineNo,
  };
}
