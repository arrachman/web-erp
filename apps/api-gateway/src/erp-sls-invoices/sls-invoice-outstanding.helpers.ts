import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { SlsInvoiceLineDto } from './dto/create-sls-invoice.dto';

/**
 * FR-SLS-02 enforcement (qty side — GL/stock anti-double already done in
 * sls-invoice-posting.service.ts): when an SI is created against a Delivery
 * Order (`deliveryOrderId`), every line that references a specific
 * `sourceLineId` (a `sls_delivery_order_lines.id`) must not bill more than
 * that DO line's remaining un-invoiced quantity.
 *
 * Mirrors validateSourceOrderOutstanding (DO<-SO) exactly — same on-the-fly
 * computation (no stored remainingQty column exists anywhere), same
 * excludeInvoiceId parameter shape for a future update() path.
 */
export async function validateSourceDeliveryOrderOutstanding(
  tx: Prisma.TransactionClient,
  deliveryOrderId: bigint,
  lines: SlsInvoiceLineDto[],
  excludeInvoiceId?: bigint,
): Promise<void> {
  const deliveryOrder = await tx.erpSlsDeliveryOrder.findFirst({
    where: { id: deliveryOrderId, deletedAt: null },
    select: { id: true, status: true },
  });
  if (!deliveryOrder) {
    throw new BadRequestException(`Delivery Order ${deliveryOrderId} tidak ditemukan.`);
  }
  if (deliveryOrder.status !== 'POSTED') {
    throw new BadRequestException(
      `Delivery Order ${deliveryOrderId} berstatus ${deliveryOrder.status} — SI hanya bisa ditarik dari DO yang sudah POSTED.`,
    );
  }

  const sourceLineIds = [
    ...new Set(lines.map((l) => l.sourceLineId).filter((v): v is string => !!v)),
  ];
  if (!sourceLineIds.length) return;

  const doLines = await tx.erpSlsDeliveryOrderLine.findMany({
    where: { id: { in: sourceLineIds.map(BigInt) }, deliveryOrderId },
    select: { id: true, quantity: true, itemId: true },
  });
  const doLineById = new Map(doLines.map((l) => [l.id.toString(), l]));
  for (const sourceLineId of sourceLineIds) {
    if (!doLineById.has(sourceLineId)) {
      throw new BadRequestException(
        `Baris DO ${sourceLineId} tidak ditemukan atau bukan milik Delivery Order ${deliveryOrderId}.`,
      );
    }
  }

  const billedRows = await tx.erpSlsInvoiceLine.findMany({
    where: {
      sourceLineId: { in: sourceLineIds.map(BigInt) },
      invoice: { deletedAt: null, ...(excludeInvoiceId ? { id: { not: excludeInvoiceId } } : {}) },
    },
    select: { sourceLineId: true, quantity: true },
  });
  const billedBySourceLine = new Map<string, Prisma.Decimal>();
  for (const row of billedRows) {
    const key = row.sourceLineId!.toString();
    billedBySourceLine.set(key, (billedBySourceLine.get(key) ?? new Prisma.Decimal(0)).add(row.quantity));
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
    const doLine = doLineById.get(sourceLineId)!;
    const alreadyBilled = billedBySourceLine.get(sourceLineId) ?? new Prisma.Decimal(0);
    const outstanding = new Prisma.Decimal(doLine.quantity).sub(alreadyBilled);
    if (requestedQty.gt(outstanding)) {
      throw new BadRequestException(
        `Baris DO ${sourceLineId} (item ${doLine.itemId}): qty ditagih (${requestedQty}) melebihi sisa outstanding (${outstanding}).`,
      );
    }
  }
}
