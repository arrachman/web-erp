import { BadRequestException } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import type { LedgerLeg } from '../erp-inv-gl/inv-gl-posting.helpers';

type AdvanceInvoice = {
  customerId: bigint | null;
  advanceId: bigint | null;
  advanceAmount: Prisma.Decimal | null;
  advanceAccountId: bigint | null;
  grandTotal: Prisma.Decimal;
  description: string | null;
};

const ZERO = new Prisma.Decimal(0);

export function invoiceAdvanceApplied(invoice: Pick<AdvanceInvoice, 'advanceId' | 'advanceAmount'>): Prisma.Decimal {
  return invoice.advanceId && invoice.advanceAmount ? new Prisma.Decimal(invoice.advanceAmount) : ZERO;
}

function settlementOf(applied: Prisma.Decimal, amount: Prisma.Decimal) {
  if (applied.lte(0)) return 'UNPAID' as const;
  return applied.gte(amount) ? ('PAID' as const) : ('PARTIAL' as const);
}

async function shiftAdvanceApplied(tx: Prisma.TransactionClient, advanceId: bigint, delta: Prisma.Decimal) {
  const adv = await tx.erpSlsCustomerAdvance.findUniqueOrThrow({
    where: { id: advanceId },
    select: { amount: true, appliedAmount: true },
  });
  const next = new Prisma.Decimal(adv.appliedAmount).add(delta);
  await tx.erpSlsCustomerAdvance.update({
    where: { id: advanceId },
    data: { appliedAmount: next, settlementStatus: settlementOf(next, new Prisma.Decimal(adv.amount)) },
  });
}

/**
 * FR-SLS-04: SI memotong saldo Customer Advance (AS) pelanggan yang sama.
 * Validasi AS (POSTED, pelanggan sama, sisa cukup), tambah `appliedAmount` AS,
 * lalu kembalikan leg Dr Uang Muka Penjualan. Null bila SI tidak memakai AS.
 */
export async function applyInvoiceAdvance(
  tx: Prisma.TransactionClient,
  invoice: AdvanceInvoice,
): Promise<LedgerLeg | null> {
  const applied = invoiceAdvanceApplied(invoice);
  if (!invoice.advanceId || applied.lte(0)) return null;

  const adv = await tx.erpSlsCustomerAdvance.findFirst({
    where: { id: invoice.advanceId, deletedAt: null },
    select: { customerId: true, status: true, amount: true, appliedAmount: true, metadata: true },
  });
  if (!adv) throw new BadRequestException('Uang muka (AS) acuan tidak ditemukan.');
  if (adv.status !== 'POSTED') throw new BadRequestException('Uang muka (AS) acuan belum POSTED.');
  if (adv.customerId !== invoice.customerId) {
    throw new BadRequestException('Uang muka (AS) harus milik pelanggan yang sama dengan invoice.');
  }
  const remaining = new Prisma.Decimal(adv.amount).sub(adv.appliedAmount);
  if (applied.gt(remaining)) {
    throw new BadRequestException(`Potongan uang muka melebihi sisa saldo AS (sisa ${remaining.toFixed(2)}).`);
  }
  if (applied.gt(invoice.grandTotal)) {
    throw new BadRequestException('Potongan uang muka tidak boleh melebihi total invoice.');
  }

  const meta = (adv.metadata ?? {}) as { advanceAccountId?: string | number };
  const accountId = invoice.advanceAccountId ?? (meta.advanceAccountId ? BigInt(meta.advanceAccountId) : null);
  if (!accountId) {
    throw new BadRequestException('Tidak bisa posting: akun Uang Muka Penjualan (advanceAccountId) belum diisi.');
  }

  await shiftAdvanceApplied(tx, invoice.advanceId, applied);
  return {
    accountId,
    debit: applied,
    credit: ZERO,
    description: invoice.description ?? 'Pemakaian uang muka',
    partnerId: invoice.customerId,
  };
}

/** Kebalikan `applyInvoiceAdvance` saat SI di-reopen/re-post. */
export async function releaseInvoiceAdvance(tx: Prisma.TransactionClient, invoiceId: bigint): Promise<void> {
  const inv = await tx.erpSlsInvoice.findUnique({
    where: { id: invoiceId },
    select: { advanceId: true, advanceAmount: true, postingStatus: true },
  });
  const applied = inv ? invoiceAdvanceApplied(inv) : ZERO;
  if (!inv?.advanceId || applied.lte(0) || inv.postingStatus !== 'POSTED') return;
  await shiftAdvanceApplied(tx, inv.advanceId, applied.neg());
  const allocations = await tx.erpFinSettlementAllocation.count({ where: { invoiceRef: invoiceId.toString() } });
  if (!allocations) {
    await tx.erpSlsInvoice.update({ where: { id: invoiceId }, data: { settlementStatus: 'UNPAID', settledDate: null } });
  }
}
