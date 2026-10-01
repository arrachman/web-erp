import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PurGoodsReceiptLineDto } from './dto/create-pur-goods-receipt.dto';

/**
 * FR-PUR-01 enforcement (qty side): when a GRN is created against a
 * Purchase Order (`orderId`), every line that references a specific
 * `orderLineId` (a `pur_order_lines.id`) must not receive more than that PO
 * line's remaining un-received quantity.
 *
 * Outstanding is measured by `acceptedQty` (QC-gated, same field the
 * posting service already uses to decide what enters stock) — rejected/
 * quarantine qty does not reduce the PO's outstanding, since the vendor
 * still owes the accepted amount.
 *
 * Mirrors validateSourceOrderOutstanding (DO<-SO) and
 * validateSourceDeliveryOrderOutstanding (SI<-DO) exactly — same on-the-fly
 * computation (no stored remainingQty column exists anywhere).
 */
export async function validateSourcePurchaseOrderOutstanding(
  tx: Prisma.TransactionClient,
  orderId: bigint,
  lines: PurGoodsReceiptLineDto[],
  excludeGrnId?: bigint,
): Promise<void> {
  const order = await tx.erpPurOrder.findFirst({
    where: { id: orderId, deletedAt: null },
    select: { id: true, status: true },
  });
  if (!order) {
    throw new BadRequestException(`Purchase Order ${orderId} tidak ditemukan.`);
  }
  if (order.status !== 'POSTED') {
    throw new BadRequestException(
      `Purchase Order ${orderId} berstatus ${order.status} — GRN hanya bisa ditarik dari PO yang sudah POSTED.`,
    );
  }

  const orderLineIds = [
    ...new Set(lines.map((l) => l.orderLineId).filter((v): v is string => !!v)),
  ];
  if (!orderLineIds.length) return;

  const poLines = await tx.erpPurOrderLine.findMany({
    where: { id: { in: orderLineIds.map(BigInt) }, orderId },
    select: { id: true, quantity: true, itemId: true },
  });
  const poLineById = new Map(poLines.map((l) => [l.id.toString(), l]));
  for (const orderLineId of orderLineIds) {
    if (!poLineById.has(orderLineId)) {
      throw new BadRequestException(
        `Baris PO ${orderLineId} tidak ditemukan atau bukan milik Purchase Order ${orderId}.`,
      );
    }
  }

  const receivedRows = await tx.erpPurGoodsReceiptLine.findMany({
    where: {
      orderLineId: { in: orderLineIds.map(BigInt) },
      goodsReceipt: { deletedAt: null, ...(excludeGrnId ? { id: { not: excludeGrnId } } : {}) },
    },
    select: { orderLineId: true, acceptedQty: true },
  });
  const receivedByOrderLine = new Map<string, Prisma.Decimal>();
  for (const row of receivedRows) {
    const key = row.orderLineId!.toString();
    receivedByOrderLine.set(key, (receivedByOrderLine.get(key) ?? new Prisma.Decimal(0)).add(row.acceptedQty));
  }

  const requestedByOrderLine = new Map<string, Prisma.Decimal>();
  for (const line of lines) {
    if (!line.orderLineId) continue;
    const key = line.orderLineId;
    const acceptedQty = new Prisma.Decimal(line.acceptedQty ?? line.quantity ?? 0);
    requestedByOrderLine.set(key, (requestedByOrderLine.get(key) ?? new Prisma.Decimal(0)).add(acceptedQty));
  }

  for (const [orderLineId, requestedQty] of requestedByOrderLine) {
    const poLine = poLineById.get(orderLineId)!;
    const alreadyReceived = receivedByOrderLine.get(orderLineId) ?? new Prisma.Decimal(0);
    const outstanding = new Prisma.Decimal(poLine.quantity).sub(alreadyReceived);
    if (requestedQty.gt(outstanding)) {
      throw new BadRequestException(
        `Baris PO ${orderLineId} (item ${poLine.itemId}): qty diterima (${requestedQty}) melebihi sisa outstanding (${outstanding}).`,
      );
    }
  }
}

/**
 * Auto-close a PO once every line's outstanding qty reaches zero
 * (FR-PUR-01: "PO berstatus Closed otomatis saat keduanya nol" — qty side
 * only here; the "belum ditagih" (billed) side is PI's job, separate pass).
 *
 * Same closedDate-not-status convention as maybeCloseSourceOrder (DO<-SO)
 * — ErpDocumentStatus has no CLOSED value.
 */
export async function maybeCloseSourcePurchaseOrder(
  tx: Prisma.TransactionClient,
  orderId: bigint,
): Promise<void> {
  const poLines = await tx.erpPurOrderLine.findMany({
    where: { orderId },
    select: { id: true, quantity: true },
  });
  if (!poLines.length) return;

  const receivedRows = await tx.erpPurGoodsReceiptLine.findMany({
    where: {
      orderLineId: { in: poLines.map((l) => l.id) },
      goodsReceipt: { deletedAt: null },
    },
    select: { orderLineId: true, acceptedQty: true },
  });
  const receivedByOrderLine = new Map<string, Prisma.Decimal>();
  for (const row of receivedRows) {
    const key = row.orderLineId!.toString();
    receivedByOrderLine.set(key, (receivedByOrderLine.get(key) ?? new Prisma.Decimal(0)).add(row.acceptedQty));
  }

  const allFulfilled = poLines.every((l) => {
    const received = receivedByOrderLine.get(l.id.toString()) ?? new Prisma.Decimal(0);
    return received.gte(l.quantity);
  });

  await tx.erpPurOrder.update({
    where: { id: orderId },
    data: { closedDate: allFulfilled ? new Date() : null },
  });
}
