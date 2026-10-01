import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { SlsReturnReceiptLineDto } from './dto/create-sls-return-receipt.dto';

/**
 * FR-SLS-05 enforcement (qty side): when a Return Receipt (RNR) is created
 * against a Sales Invoice (`invoiceId`), every line that references a
 * specific `sourceLineId` (a `sls_invoice_lines.id`) must not receive more
 * than that SI line's remaining un-returned quantity.
 *
 * Mirrors validateSourceDeliveryOrderOutstanding (SI<-DO) exactly — same
 * on-the-fly computation.
 */
export async function validateSourceInvoiceOutstanding(
  tx: Prisma.TransactionClient,
  invoiceId: bigint,
  lines: SlsReturnReceiptLineDto[],
  excludeReturnReceiptId?: bigint,
): Promise<void> {
  const invoice = await tx.erpSlsInvoice.findFirst({
    where: { id: invoiceId, deletedAt: null },
    select: { id: true, status: true },
  });
  if (!invoice) {
    throw new BadRequestException(`Sales Invoice ${invoiceId} tidak ditemukan.`);
  }
  if (invoice.status !== 'POSTED') {
    throw new BadRequestException(
      `Sales Invoice ${invoiceId} berstatus ${invoice.status} — RNR hanya bisa ditarik dari SI yang sudah POSTED.`,
    );
  }

  const sourceLineIds = [
    ...new Set(lines.map((l) => l.sourceLineId).filter((v): v is string => !!v)),
  ];
  if (!sourceLineIds.length) return;

  const siLines = await tx.erpSlsInvoiceLine.findMany({
    where: { id: { in: sourceLineIds.map(BigInt) }, invoiceId },
    select: { id: true, quantity: true, itemId: true },
  });
  const siLineById = new Map(siLines.map((l) => [l.id.toString(), l]));
  for (const sourceLineId of sourceLineIds) {
    if (!siLineById.has(sourceLineId)) {
      throw new BadRequestException(
        `Baris SI ${sourceLineId} tidak ditemukan atau bukan milik Sales Invoice ${invoiceId}.`,
      );
    }
  }

  const returnedRows = await tx.erpSlsReturnReceiptLine.findMany({
    where: {
      sourceLineId: { in: sourceLineIds.map(BigInt) },
      returnReceipt: {
        deletedAt: null,
        ...(excludeReturnReceiptId ? { id: { not: excludeReturnReceiptId } } : {}),
      },
    },
    select: { sourceLineId: true, quantity: true },
  });
  const returnedBySourceLine = new Map<string, Prisma.Decimal>();
  for (const row of returnedRows) {
    const key = row.sourceLineId!.toString();
    returnedBySourceLine.set(key, (returnedBySourceLine.get(key) ?? new Prisma.Decimal(0)).add(row.quantity));
  }

  const requestedBySourceLine = new Map<string, Prisma.Decimal>();
  for (const line of lines) {
    if (!line.sourceLineId) continue;
    const key = line.sourceLineId;
    requestedBySourceLine.set(
      key,
      (requestedBySourceLine.get(key) ?? new Prisma.Decimal(0)).add(new Prisma.Decimal(line.quantity)),
    );
  }

  for (const [sourceLineId, requestedQty] of requestedBySourceLine) {
    const siLine = siLineById.get(sourceLineId)!;
    const alreadyReturned = returnedBySourceLine.get(sourceLineId) ?? new Prisma.Decimal(0);
    const outstanding = new Prisma.Decimal(siLine.quantity).sub(alreadyReturned);
    if (requestedQty.gt(outstanding)) {
      throw new BadRequestException(
        `Baris SI ${sourceLineId} (item ${siLine.itemId}): qty retur (${requestedQty}) melebihi sisa outstanding (${outstanding}).`,
      );
    }
  }
}
