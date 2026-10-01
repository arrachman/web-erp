import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';

/**
 * FR-SLS-06 enforcement (amount side): "SR mengurangi sisa piutang SI
 * acuannya. Nominal SR tidak boleh melebihi sisa SI ditambah potongan AS/IP
 * yang sudah dipakai."
 *
 * Unlike the qty-side pairs (DO<-SO, SI<-DO, GRN<-PO, PI<-GRN, RNR<-SI),
 * this is a NOMINAL check against the invoice's remaining AR balance, not a
 * line-level qty check — SR has no stock movement of its own to guard
 * (RNR already owns stock posting; SlsReturnPostingService is GL-only, see
 * DECISIONS.md § RNR+SR posting), so there's no qty outstanding to protect
 * here beyond what RNR already enforces against the SI.
 *
 * Outstanding AR for the invoice = grandTotal − SUM(AR Receipt allocations
 * for this invoiceRef) − SUM(other SR grandTotal already applied to this
 * invoice) — same on-the-fly computation style as every other outstanding
 * check this session (no stored remainingAmount column exists anywhere).
 */
export async function validateSourceInvoiceRemainingBalance(
  tx: Prisma.TransactionClient,
  invoiceId: bigint,
  requestedAmount: Prisma.Decimal,
  excludeReturnId?: bigint,
): Promise<void> {
  const invoice = await tx.erpSlsInvoice.findFirst({
    where: { id: invoiceId, deletedAt: null },
    select: { id: true, status: true, grandTotal: true },
  });
  if (!invoice) {
    throw new BadRequestException(`Sales Invoice ${invoiceId} tidak ditemukan.`);
  }
  if (invoice.status !== 'POSTED') {
    throw new BadRequestException(
      `Sales Invoice ${invoiceId} berstatus ${invoice.status} — SR hanya bisa ditarik dari SI yang sudah POSTED.`,
    );
  }

  const [receiptAllocations, otherReturns] = await Promise.all([
    tx.erpFinSettlementAllocation.aggregate({
      where: { invoiceRef: invoiceId.toString() },
      _sum: { amount: true },
    }),
    tx.erpSlsReturn.aggregate({
      where: {
        invoiceId,
        deletedAt: null,
        status: { notIn: ['VOID', 'CANCELLED', 'REJECTED'] },
        ...(excludeReturnId ? { id: { not: excludeReturnId } } : {}),
      },
      _sum: { grandTotal: true },
    }),
  ]);

  const alreadyApplied = new Prisma.Decimal(receiptAllocations._sum.amount ?? 0).add(
    new Prisma.Decimal(otherReturns._sum.grandTotal ?? 0),
  );
  const outstanding = new Prisma.Decimal(invoice.grandTotal).sub(alreadyApplied);

  if (requestedAmount.gt(outstanding)) {
    throw new BadRequestException(
      `SR grandTotal (${requestedAmount}) melebihi sisa piutang SI ${invoiceId} (${outstanding}).`,
    );
  }
}
