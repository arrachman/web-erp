import { Prisma } from '@prisma/client';
import { QueryInvWeighbridgeTicketsDto } from './dto/query-inv-weighbridge-tickets.dto';
import { InvWeighbridgeTicketTransitionAction as A } from './dto/transition-inv-weighbridge-ticket.dto';

export function toBigInt(v?: string | null): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  return BigInt(v);
}

/** Statuses where the header may still be edited (§2.7 state machine). */
export const EDITABLE = new Set(['DRAFT', 'NEED_APPROVE', 'REJECTED']);

/** valid (status, action) → next status. POST/REOPEN handled separately. */
export const NEXT: Record<string, Partial<Record<A, string>>> = {
  DRAFT: { [A.SUBMIT]: 'NEED_APPROVE' },
  REJECTED: { [A.SUBMIT]: 'NEED_APPROVE' },
  NEED_APPROVE: { [A.APPROVE]: 'APPROVED', [A.REJECT]: 'REJECTED' },
  APPROVED: { [A.POST]: 'POSTED', [A.REOPEN]: 'DRAFT' },
  POSTED: { [A.REOPEN]: 'DRAFT' },
};

export function buildWeighbridgeWhere(
  query: QueryInvWeighbridgeTicketsDto,
): Prisma.ErpInvWeighbridgeTicketWhereInput {
  const where: Prisma.ErpInvWeighbridgeTicketWhereInput = { deletedAt: null };

  if (query.status) where.status = query.status as never;
  if (query.branchId) where.branchId = BigInt(query.branchId);
  if (query.partnerId) where.partnerId = BigInt(query.partnerId);
  if (query.createdById) where.createdById = BigInt(query.createdById);

  if (query.dateFrom || query.dateTo) {
    where.ticketDate = {
      ...(query.dateFrom ? { gte: new Date(query.dateFrom) } : {}),
      ...(query.dateTo ? { lte: new Date(query.dateTo) } : {}),
    };
  }

  if (query.search?.trim()) {
    const q = query.search.trim();
    where.AND = [
      {
        OR: [
          { docNumber: { contains: q, mode: 'insensitive' } },
          { vehiclePlate: { contains: q, mode: 'insensitive' } },
          { driverName: { contains: q, mode: 'insensitive' } },
        ],
      },
    ];
  }

  return where;
}

export async function genDocNumber(
  tx: Prisma.TransactionClient,
  docCode: string,
): Promise<string> {
  const numbering = await tx.erpDocumentNumbering.findFirst({
    where: { documentCode: docCode, deletedAt: null },
  });
  if (numbering) {
    const seq = numbering.nextNumber;
    await tx.erpDocumentNumbering.update({
      where: { id: numbering.id },
      data: { nextNumber: seq + 1 },
    });
    return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
  }
  const count = await tx.erpInvWeighbridgeTicket.count();
  return `${docCode}${String(count + 1).padStart(6, '0')}`;
}
