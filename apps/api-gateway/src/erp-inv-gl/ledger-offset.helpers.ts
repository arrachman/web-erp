import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { assertLedgerRowsPeriodOpen } from '../erp-common/utils/ledger-period-guard';

export const VOID_DOCTYPE_SUFFIX = ':void';

export interface OffsetOptions {
  entryDate: Date;
  fiscalPeriodId: bigint;
  actorId: bigint | null;
}

/**
 * Pembalikan bertanggal (audit-friendly) sebagai pengganti hard-delete untuk VOID/cancel
 * dokumen POSTED: salin baris ledger sumber dengan debit/kredit ditukar, bertanggal
 * `entryDate` di periode `fiscalPeriodId`, sourceDocType `<tipe>:void`. Baris asli tetap
 * ada, jadi periode lampau tidak berubah. Idempotent: menolak bila sudah pernah dibalik.
 * Hard-delete (reverseInvLedger) tetap untuk REOPEN/re-post di periode yang sama.
 */
export async function reverseWithOffset(
  tx: Prisma.TransactionClient,
  sourceDocType: string,
  sourceId: bigint,
  opts: OffsetOptions,
): Promise<number> {
  const voidType = `${sourceDocType}${VOID_DOCTYPE_SUFFIX}`;
  const already = await tx.erpFinLedgerEntry.count({
    where: { sourceDocType: voidType, sourceId, deletedAt: null },
  });
  if (already > 0) throw new BadRequestException('Dokumen ini sudah dibalik (VOID).');

  const originals = await tx.erpFinLedgerEntry.findMany({
    where: { sourceDocType, sourceId, deletedAt: null },
    orderBy: { lineNo: 'asc' },
  });
  if (!originals.length) throw new BadRequestException('Tidak ada jurnal terposting untuk dibalik.');

  const rows: Prisma.ErpFinLedgerEntryCreateManyInput[] = originals.map((o, i) => ({
    branchId: o.branchId,
    locationId: o.locationId,
    source: o.source,
    sourceDocType: voidType,
    sourceId,
    docNumber: `${o.docNumber}-V`,
    entryDate: opts.entryDate,
    fiscalPeriodId: opts.fiscalPeriodId,
    partnerId: o.partnerId,
    accountId: o.accountId,
    description: `Pembalikan: ${o.description ?? o.docNumber}`,
    currencyId: o.currencyId,
    exchangeRate: o.exchangeRate,
    debit: o.credit,
    credit: o.debit,
    debitFx: o.creditFx,
    creditFx: o.debitFx,
    costCenterId: o.costCenterId,
    divisionId: o.divisionId,
    subdivisionId: o.subdivisionId,
    projectId: o.projectId,
    reconciliationStatus: 'UNRECONCILED',
    isAdjustment: true,
    status: 'POSTED',
    postingStatus: 'POSTED',
    postedAt: new Date(),
    createdById: opts.actorId,
    updatedById: opts.actorId,
    lineNo: i + 1,
  }));
  await assertLedgerRowsPeriodOpen(tx, rows);
  await tx.erpFinLedgerEntry.createMany({ data: rows });
  return rows.length;
}
