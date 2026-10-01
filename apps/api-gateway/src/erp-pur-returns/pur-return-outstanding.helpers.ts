import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PurReturnLineDto } from './dto/create-pur-return.dto';

/**
 * FR-PUR-06 enforcement (qty side): "PRT dari DNR tidak menggerakkan stok
 * lagi karena barang sudah keluar lewat DNR." In this codebase DNR and PRT
 * are the SAME model (ErpPurReturn, split by returnType — see DECISIONS.md
 * § PRT/DNR posting), so there's no separate "DNR" document to chain from;
 * the qty-side guard that matters is against the GOODS RECEIPT the goods
 * are being sent back from, when `returnType === 'RETURN_TO_VENDOR'`.
 *
 * Mirrors validateSourceInvoiceOutstanding (RNR<-SI) exactly — same
 * on-the-fly computation, now against GRN's acceptedQty.
 */
export async function validateSourceGoodsReceiptReturnOutstanding(
  tx: Prisma.TransactionClient,
  goodsReceiptId: bigint,
  lines: PurReturnLineDto[],
  excludeReturnId?: bigint,
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
      `Goods Receipt ${goodsReceiptId} berstatus ${goodsReceipt.status} — retur hanya bisa ditarik dari GRN yang sudah POSTED.`,
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

  const returnedRows = await tx.erpPurReturnLine.findMany({
    where: {
      goodsReceiptLineId: { in: grnLineIds.map(BigInt) },
      return: { deletedAt: null, ...(excludeReturnId ? { id: { not: excludeReturnId } } : {}) },
    },
    select: { goodsReceiptLineId: true, quantity: true },
  });
  const returnedByGrnLine = new Map<string, Prisma.Decimal>();
  for (const row of returnedRows) {
    const key = row.goodsReceiptLineId!.toString();
    returnedByGrnLine.set(key, (returnedByGrnLine.get(key) ?? new Prisma.Decimal(0)).add(row.quantity));
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
    const alreadyReturned = returnedByGrnLine.get(grnLineId) ?? new Prisma.Decimal(0);
    const outstanding = new Prisma.Decimal(grnLine.acceptedQty).sub(alreadyReturned);
    if (requestedQty.gt(outstanding)) {
      throw new BadRequestException(
        `Baris GRN ${grnLineId} (item ${grnLine.itemId}): qty retur (${requestedQty}) melebihi sisa outstanding (${outstanding}).`,
      );
    }
  }
}

/**
 * FR-PUR-05 enforcement (amount side, direct mode only): a direct PRT
 * (`invoiceId` set) must not reduce a PI's outstanding AP by more than what
 * remains. Undirect PRT (no `invoiceId`) is out of scope here — it becomes
 * a VPP-pooled credit instead, no specific PI to check against.
 *
 * Mirrors validateSourceInvoiceRemainingBalance (SR vs SI) exactly, now for
 * AP: outstanding = PI.grandTotal − SUM(AP Payment allocations for this
 * invoiceRef) − SUM(other active PRT grandTotal already applied to it).
 */
export async function validateSourceInvoiceRemainingPayable(
  tx: Prisma.TransactionClient,
  invoiceId: bigint,
  requestedAmount: Prisma.Decimal,
  excludeReturnId?: bigint,
): Promise<void> {
  const invoice = await tx.erpPurInvoice.findFirst({
    where: { id: invoiceId, deletedAt: null },
    select: { id: true, status: true, grandTotal: true },
  });
  if (!invoice) {
    throw new BadRequestException(`Purchase Invoice ${invoiceId} tidak ditemukan.`);
  }
  if (invoice.status !== 'POSTED') {
    throw new BadRequestException(
      `Purchase Invoice ${invoiceId} berstatus ${invoice.status} — PRT hanya bisa ditarik dari PI yang sudah POSTED.`,
    );
  }

  const [paymentAllocations, otherReturns] = await Promise.all([
    tx.erpFinSettlementAllocation.aggregate({
      where: { invoiceRef: invoiceId.toString() },
      _sum: { amount: true },
    }),
    tx.erpPurReturn.aggregate({
      where: {
        invoiceId,
        deletedAt: null,
        status: { notIn: ['VOID', 'CANCELLED', 'REJECTED'] },
        ...(excludeReturnId ? { id: { not: excludeReturnId } } : {}),
      },
      _sum: { grandTotal: true },
    }),
  ]);

  const alreadyApplied = new Prisma.Decimal(paymentAllocations._sum.amount ?? 0).add(
    new Prisma.Decimal(otherReturns._sum.grandTotal ?? 0),
  );
  const outstanding = new Prisma.Decimal(invoice.grandTotal).sub(alreadyApplied);

  if (requestedAmount.gt(outstanding)) {
    throw new BadRequestException(
      `PRT grandTotal (${requestedAmount}) melebihi sisa utang PI ${invoiceId} (${outstanding}).`,
    );
  }
}
