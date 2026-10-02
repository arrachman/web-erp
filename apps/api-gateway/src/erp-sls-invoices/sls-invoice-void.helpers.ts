import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  assertNoActiveDerivedDocuments,
  INACTIVE_STATUSES,
} from '../erp-common/guards/source-document-lock.helper';

/** SI hanya boleh di-VOID bila belum dilunasi dan tidak punya turunan aktif. */
export async function assertInvoiceVoidable(tx: Prisma.TransactionClient, invoiceId: bigint): Promise<void> {
  const paid = await tx.erpFinSettlementAllocation.count({
    where: {
      invoiceRef: invoiceId.toString(),
      arReceipt: { deletedAt: null, status: { notIn: [...INACTIVE_STATUSES] as never } },
    },
  });
  if (paid > 0) {
    throw new BadRequestException('SI sudah dilunasi/dialokasikan di AR Receipt — batalkan receipt dulu.');
  }
  await assertNoActiveDerivedDocuments([
    {
      label: 'Return Receipt',
      findFirst: (args) => tx.erpSlsReturnReceipt.findFirst(args as never),
      where: { invoiceId, deletedAt: null, status: { notIn: INACTIVE_STATUSES } },
    },
    {
      label: 'Sales Return',
      findFirst: (args) => tx.erpSlsReturn.findFirst(args as never),
      where: { invoiceId, deletedAt: null, status: { notIn: INACTIVE_STATUSES } },
    },
  ]);
}

/** Periode fiskal yang memuat tanggal pembalikan (hari ini). */
export async function resolveVoidPeriod(tx: Prisma.TransactionClient, date: Date): Promise<bigint> {
  const period = await tx.erpFiscalPeriod.findFirst({
    where: { deletedAt: null, startDate: { lte: date }, endDate: { gte: date } },
    select: { id: true },
  });
  if (!period) throw new BadRequestException('Tidak ada periode fiskal untuk tanggal hari ini.');
  return period.id;
}
