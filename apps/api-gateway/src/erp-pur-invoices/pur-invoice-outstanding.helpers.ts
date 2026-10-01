import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PurInvoiceLineDto } from './dto/create-pur-invoice.dto';

/**
 * FR-PUR-01 enforcement (billed side — qty side already done in GRN<-PO):
 * when a PI is created against a Goods Receipt (`goodsReceiptId`), every
 * line that references a specific `goodsReceiptLineId` (a
 * `pur_goods_receipt_lines.id`) must not bill more than that GRN line's
 * remaining un-invoiced `acceptedQty`.
 *
 * Mirrors validateSourceDeliveryOrderOutstanding (SI<-DO) exactly — same
 * on-the-fly computation (no stored remainingQty column exists anywhere).
 */
export async function validateSourceGoodsReceiptOutstanding(
  tx: Prisma.TransactionClient,
  goodsReceiptId: bigint,
  lines: PurInvoiceLineDto[],
  excludeInvoiceId?: bigint,
): Promise<void> {
  const goodsReceipt = await tx.erpPurGoodsReceipt.findFirst({
    where: { id: goodsReceiptId, deletedAt: null },
    select: { id: true, status: true },
  });
  if (!goodsReceipt) {
    throw new BadRequestException(`Goods Receipt ${goodsReceiptId} tidak ditemukan.`);
  }
  if (goodsReceipt.status !== 'POSTED') {
    throw new BadRequestException(
      `Goods Receipt ${goodsReceiptId} berstatus ${goodsReceipt.status} — PI hanya bisa ditarik dari GRN yang sudah POSTED.`,
    );
  }

  const grnLineIds = [
    ...new Set(lines.map((l) => l.goodsReceiptLineId).filter((v): v is string => !!v)),
  ];
  if (!grnLineIds.length) return;

  const grnLines = await tx.erpPurGoodsReceiptLine.findMany({
    where: { id: { in: grnLineIds.map(BigInt) }, goodsReceiptId },
    select: { id: true, acceptedQty: true, itemId: true },
  });
  const grnLineById = new Map(grnLines.map((l) => [l.id.toString(), l]));
  for (const grnLineId of grnLineIds) {
    if (!grnLineById.has(grnLineId)) {
      throw new BadRequestException(
        `Baris GRN ${grnLineId} tidak ditemukan atau bukan milik Goods Receipt ${goodsReceiptId}.`,
      );
    }
  }

  const billedRows = await tx.erpPurInvoiceLine.findMany({
    where: {
      goodsReceiptLineId: { in: grnLineIds.map(BigInt) },
      invoice: { deletedAt: null, ...(excludeInvoiceId ? { id: { not: excludeInvoiceId } } : {}) },
    },
    select: { goodsReceiptLineId: true, quantity: true },
  });
  const billedByGrnLine = new Map<string, Prisma.Decimal>();
  for (const row of billedRows) {
    const key = row.goodsReceiptLineId!.toString();
    billedByGrnLine.set(key, (billedByGrnLine.get(key) ?? new Prisma.Decimal(0)).add(row.quantity));
  }

  const requestedByGrnLine = new Map<string, Prisma.Decimal>();
  for (const line of lines) {
    if (!line.goodsReceiptLineId) continue;
    const key = line.goodsReceiptLineId;
    requestedByGrnLine.set(
      key,
      (requestedByGrnLine.get(key) ?? new Prisma.Decimal(0)).add(new Prisma.Decimal(line.quantity)),
    );
  }

  for (const [grnLineId, requestedQty] of requestedByGrnLine) {
    const grnLine = grnLineById.get(grnLineId)!;
    const alreadyBilled = billedByGrnLine.get(grnLineId) ?? new Prisma.Decimal(0);
    const outstanding = new Prisma.Decimal(grnLine.acceptedQty).sub(alreadyBilled);
    if (requestedQty.gt(outstanding)) {
      throw new BadRequestException(
        `Baris GRN ${grnLineId} (item ${grnLine.itemId}): qty ditagih (${requestedQty}) melebihi sisa outstanding (${outstanding}).`,
      );
    }
  }
}
