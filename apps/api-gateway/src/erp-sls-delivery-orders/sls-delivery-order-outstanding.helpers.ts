import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { SlsDeliveryOrderLineDto } from './dto/create-sls-delivery-order.dto';

/**
 * FR-SLS-01 enforcement: when a DO is created against a Sales Order
 * (`orderId`), every line that references a specific `sourceLineId` (a
 * `sls_order_lines.id`) must not take more than that SO line's remaining
 * outstanding quantity.
 *
 * Outstanding is NOT a stored column anywhere (DECISIONS.md § Plan — Fase 0:
 * no remainingQty tracking exists in this schema) — computed on the fly as
 * `soLine.quantity - SUM(other non-deleted DO lines' quantity with the same
 * sourceLineId)`. This mirrors the same on-the-fly outstanding pattern used
 * by ArReceiptPostingService/ApPaymentPostingService for invoice allocation.
 *
 * `excludeDeliveryOrderId` lets an UPDATE recompute outstanding without
 * double-counting the DO being edited (its own prior lines are excluded from
 * the "already taken" sum before the new lines are checked).
 */
export async function validateSourceOrderOutstanding(
  tx: Prisma.TransactionClient,
  orderId: bigint,
  lines: SlsDeliveryOrderLineDto[],
  excludeDeliveryOrderId?: bigint,
): Promise<void> {
  const order = await tx.erpSlsOrder.findFirst({
    where: { id: orderId, deletedAt: null },
    select: { id: true, status: true },
  });
  if (!order) {
    throw new BadRequestException(`Sales Order ${orderId} tidak ditemukan.`);
  }
  if (order.status !== 'POSTED') {
    throw new BadRequestException(
      `Sales Order ${orderId} berstatus ${order.status} — DO hanya bisa ditarik dari SO yang sudah POSTED.`,
    );
  }

  const sourceLineIds = [
    ...new Set(lines.map((l) => l.sourceLineId).filter((v): v is string => !!v)),
  ];
  if (!sourceLineIds.length) return;

  const soLines = await tx.erpSlsOrderLine.findMany({
    where: { id: { in: sourceLineIds.map(BigInt) }, orderId },
    select: { id: true, quantity: true, itemId: true },
  });
  const soLineById = new Map(soLines.map((l) => [l.id.toString(), l]));
  for (const sourceLineId of sourceLineIds) {
    if (!soLineById.has(sourceLineId)) {
      throw new BadRequestException(
        `Baris SO ${sourceLineId} tidak ditemukan atau bukan milik Sales Order ${orderId}.`,
      );
    }
  }

  const takenRows = await tx.erpSlsDeliveryOrderLine.findMany({
    where: {
      sourceLineId: { in: sourceLineIds.map(BigInt) },
      deliveryOrder: { deletedAt: null, ...(excludeDeliveryOrderId ? { id: { not: excludeDeliveryOrderId } } : {}) },
    },
    select: { sourceLineId: true, quantity: true },
  });
  const takenBySourceLine = new Map<string, Prisma.Decimal>();
  for (const row of takenRows) {
    const key = row.sourceLineId!.toString();
    takenBySourceLine.set(key, (takenBySourceLine.get(key) ?? new Prisma.Decimal(0)).add(row.quantity));
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
    const soLine = soLineById.get(sourceLineId)!;
    const alreadyTaken = takenBySourceLine.get(sourceLineId) ?? new Prisma.Decimal(0);
    const outstanding = new Prisma.Decimal(soLine.quantity).sub(alreadyTaken);
    if (requestedQty.gt(outstanding)) {
      throw new BadRequestException(
        `Baris SO ${sourceLineId} (item ${soLine.itemId}): qty diminta (${requestedQty}) melebihi sisa outstanding (${outstanding}).`,
      );
    }
  }
}

/**
 * Auto-close an SO once every line's outstanding qty reaches zero
 * (FR-SLS-01: "SO berstatus Closed otomatis saat keduanya nol").
 *
 * `ErpDocumentStatus` has NO 'CLOSED' value (checked: DRAFT/NEED_APPROVE/
 * APPROVE_1-4/APPROVED/REJECTED/POSTED/VOID/CANCELLED only — adding one
 * needs its own migration, out of scope for this pass). `closedDate`
 * (nullable, already on the schema) is used as the "closed" signal instead
 * — `status` stays POSTED, `closedDate` set marks it fulfilled. Any report/
 * UI treating SO as "open" should check `closedDate IS NULL`, not `status`.
 */
export async function maybeCloseSourceOrder(
  tx: Prisma.TransactionClient,
  orderId: bigint,
): Promise<void> {
  const soLines = await tx.erpSlsOrderLine.findMany({
    where: { orderId },
    select: { id: true, quantity: true },
  });
  if (!soLines.length) return;

  const takenRows = await tx.erpSlsDeliveryOrderLine.findMany({
    where: {
      sourceLineId: { in: soLines.map((l) => l.id) },
      deliveryOrder: { deletedAt: null },
    },
    select: { sourceLineId: true, quantity: true },
  });
  const takenBySourceLine = new Map<string, Prisma.Decimal>();
  for (const row of takenRows) {
    const key = row.sourceLineId!.toString();
    takenBySourceLine.set(key, (takenBySourceLine.get(key) ?? new Prisma.Decimal(0)).add(row.quantity));
  }

  const allFulfilled = soLines.every((l) => {
    const taken = takenBySourceLine.get(l.id.toString()) ?? new Prisma.Decimal(0);
    return taken.gte(l.quantity);
  });

  await tx.erpSlsOrder.update({
    where: { id: orderId },
    data: { closedDate: allFulfilled ? new Date() : null },
  });
}
