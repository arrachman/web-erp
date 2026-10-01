import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

export type AccountType = 'ASSET' | 'LIABILITY' | 'EQUITY' | 'REVENUE' | 'EXPENSE';

export interface PostableAccount {
  id: bigint;
  code: string;
  name: string;
  type: AccountType;
  normalBalance: 'DEBIT' | 'CREDIT';
  openingBalance: Prisma.Decimal;
}

export interface Movement {
  debit: Prisma.Decimal;
  credit: Prisma.Decimal;
}

export const ZERO = new Prisma.Decimal(0);

export function num(d: Prisma.Decimal): number {
  return d.toNumber();
}

export async function loadPostableAccounts(prisma: PrismaService): Promise<PostableAccount[]> {
  const accounts = await prisma.erpAccount.findMany({
    where: { kind: 'POSTABLE', deletedAt: null },
    orderBy: { code: 'asc' },
    select: {
      id: true,
      code: true,
      name: true,
      type: true,
      normalBalance: true,
      openingBalance: true,
    },
  });
  return accounts as PostableAccount[];
}

/** Aggregate debit/credit per account id (as string) for a ledger where clause. */
export async function movementsByAccount(
  prisma: PrismaService,
  where: Prisma.ErpFinLedgerEntryWhereInput,
): Promise<Map<string, Movement>> {
  const grouped = await prisma.erpFinLedgerEntry.groupBy({
    by: ['accountId'],
    where,
    _sum: { debit: true, credit: true },
  });
  const map = new Map<string, Movement>();
  for (const g of grouped) {
    map.set(g.accountId.toString(), {
      debit: g._sum.debit ?? ZERO,
      credit: g._sum.credit ?? ZERO,
    });
  }
  return map;
}

export function baseLedgerWhere(branchId?: string): Prisma.ErpFinLedgerEntryWhereInput {
  const where: Prisma.ErpFinLedgerEntryWhereInput = { deletedAt: null };
  if (branchId) where.branchId = BigInt(branchId);
  return where;
}

export function metaCabang(branchId?: string): { label: string; value: string }[] {
  return branchId ? [{ label: 'Cabang', value: branchId }] : [];
}

export function printedMeta(): { label: string; value: string } {
  return { label: 'Dicetak', value: new Date().toISOString().slice(0, 10) };
}
