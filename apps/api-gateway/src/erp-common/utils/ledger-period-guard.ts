import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';

const BLOCKED_STATUSES = ['CLOSED'];
const CLOSED_MESSAGE = 'Periode fiskal sudah ditutup — tidak bisa posting atau membatalkan jurnal di periode ini.';

async function assertPeriodIdsOpen(tx: Prisma.TransactionClient, periodIds: bigint[]): Promise<void> {
  const ids = [...new Set(periodIds)];
  if (!ids.length) return;
  const closed = await tx.erpFiscalPeriod.findFirst({
    where: { id: { in: ids }, status: { in: BLOCKED_STATUSES as never } },
    select: { id: true },
  });
  if (closed) throw new BadRequestException(CLOSED_MESSAGE);
}

/** FR-FIN-04: blok posting ledger baru ke periode CLOSED. Panggil sebelum `createMany`. */
export async function assertLedgerRowsPeriodOpen(
  tx: Prisma.TransactionClient,
  rows: ReadonlyArray<{ fiscalPeriodId: bigint | number }>,
): Promise<void> {
  await assertPeriodIdsOpen(tx, rows.map((r) => BigInt(r.fiscalPeriodId)));
}

/** FR-FIN-04: blok pembatalan (reverse) jurnal yang sudah ada di periode CLOSED. Panggil sebelum `deleteMany`. */
export async function assertSourceLedgerPeriodOpen(
  tx: Prisma.TransactionClient,
  where: Prisma.ErpFinLedgerEntryWhereInput,
): Promise<void> {
  const existing = await tx.erpFinLedgerEntry.findMany({
    where,
    select: { fiscalPeriodId: true },
    distinct: ['fiscalPeriodId'],
  });
  await assertPeriodIdsOpen(tx, existing.map((e) => e.fiscalPeriodId));
}
