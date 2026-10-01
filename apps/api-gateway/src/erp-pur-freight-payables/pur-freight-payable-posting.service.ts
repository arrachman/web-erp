import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const PUR_GL_SOURCE = 'PURCHASING';
const PUR_GL_DOCTYPE = 'fin_ap_payments_freight_payable';

type FreightPayableRow = Prisma.ErpFinApPaymentGetPayload<Record<string, never>>;

/**
 * GL posting for Freight Payable (PP) → fin_ledger_entries.
 *
 * Stored in the SAME table as VP/AP (`fin_ap_payments`), discriminated by
 * `source = 'PP'` (see erp-pur-freight-payables.service.ts). Confirmed with
 * user: freight cost is recorded SEPARATE from HPP/inventory value — it's
 * an operating expense, not a landed-cost component.
 *
 * §2 posting matrix (DECISIONS.md "Pola jurnal usulan" — PP row, user-
 * confirmed direction):
 *   Dr  Beban Angkut Pembelian / Freight Expense   amount
 *   Cr  Kas atau Bank (header bankAccountId)         amount
 *
 * `fin_ap_payments.bankAccountId` is a real column (shared with VP/AP) and
 * is used directly; the expense account has no dedicated column on this
 * table, so it is kept in `metadata.expenseAccountId` — same pattern as
 * AS/AP's missing account columns.
 *
 * sourceDocType is namespaced separately from VP ('fin_ap_payments') and AP
 * ('fin_ap_payments_vendor_advance') so REOPEN on one never touches another
 * source's ledger rows despite sharing a table and id space.
 */
@Injectable()
export class PurFreightPayablePostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    freightPayable: FreightPayableRow,
    actorId: bigint | null,
  ): Promise<void> {
    const amount = new Prisma.Decimal(freightPayable.amount?.toString() ?? 0);
    if (amount.lte(0)) {
      throw new BadRequestException('Tidak bisa posting: amount harus lebih besar dari 0.');
    }
    if (!freightPayable.bankAccountId) {
      throw new BadRequestException('Tidak bisa posting: akun kas/bank (bankAccountId) belum diisi.');
    }
    const metadata = (freightPayable.metadata as Record<string, unknown> | null) ?? {};
    const expenseAccountId = toBigIntOrNull(metadata.expenseAccountId);
    if (!expenseAccountId) {
      throw new BadRequestException(
        'Tidak bisa posting: akun Beban Angkut Pembelian (expenseAccountId) belum diisi untuk dokumen ini.',
      );
    }

    const legs: LedgerLeg[] = [
      {
        accountId: expenseAccountId,
        debit: amount,
        credit: new Prisma.Decimal(0),
        description: freightPayable.description,
        partnerId: freightPayable.partnerId,
      },
      {
        accountId: freightPayable.bankAccountId,
        debit: new Prisma.Decimal(0),
        credit: amount,
        description: 'Pembayaran ongkos kirim',
        partnerId: freightPayable.partnerId,
      },
    ];

    const base: LedgerBase = {
      branchId: freightPayable.branchId,
      locationId: freightPayable.locationId,
      sourceDocType: PUR_GL_DOCTYPE,
      sourceId: freightPayable.id,
      docNumber: freightPayable.docNumber,
      entryDate: freightPayable.transactionDate,
      fiscalPeriodId: freightPayable.fiscalPeriodId,
      currencyId: freightPayable.currencyId,
      exchangeRate: freightPayable.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: PUR_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, freightPayableId: bigint): Promise<void> {
    await reverseInvLedger(tx, PUR_GL_DOCTYPE, freightPayableId);
  }
}

function toBigIntOrNull(v: unknown): bigint | null {
  if (v === undefined || v === null || v === '') return null;
  try {
    return BigInt(v as string);
  } catch {
    return null;
  }
}
