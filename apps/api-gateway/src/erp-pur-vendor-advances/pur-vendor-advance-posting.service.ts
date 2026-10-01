import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const PUR_GL_SOURCE = 'PURCHASING';
const PUR_GL_DOCTYPE = 'fin_ap_payments_vendor_advance';

type VendorAdvanceRow = Prisma.ErpFinApPaymentGetPayload<Record<string, never>>;

/**
 * GL posting for Vendor Advances (AP) → fin_ledger_entries.
 *
 * Stored in the SAME table as VP (`fin_ap_payments`), discriminated by
 * `source = 'AP'` (see erp-pur-vendor-advances.service.ts). Must NOT reuse
 * ApPaymentPostingService (VP's posting service) — that one requires
 * allocating against a specific outstanding pur_invoices row via
 * ErpFinSettlementAllocation, but a vendor advance has no invoice yet to
 * settle (same reasoning as AS vs AR Receipt, see DECISIONS.md § AS).
 *
 * §2 posting matrix (DECISIONS.md "Pola jurnal usulan" — AP row):
 *   Dr  Uang Muka Pembelian (Vendor Advance Asset)   amount
 *   Cr  Kas atau Bank (header bankAccountId)          amount
 *
 * `fin_ap_payments.bankAccountId` is a real column (used directly); the
 * advance account has no dedicated column on this table, so it is kept in
 * `metadata.advanceAccountId` — same pattern as AS's bankAccountId/
 * advanceAccountId when sls_customer_advances had no columns for either.
 *
 * sourceDocType is namespaced separately from VP's 'fin_ap_payments' so a
 * REOPEN/reverse on one source never touches the other's ledger rows even
 * though they share a table and an id space with VP.
 */
@Injectable()
export class PurVendorAdvancePostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    advance: VendorAdvanceRow,
    actorId: bigint | null,
  ): Promise<void> {
    const amount = new Prisma.Decimal(advance.amount?.toString() ?? 0);
    if (amount.lte(0)) {
      throw new BadRequestException('Tidak bisa posting: amount harus lebih besar dari 0.');
    }
    if (!advance.bankAccountId) {
      throw new BadRequestException('Tidak bisa posting: akun kas/bank (bankAccountId) belum diisi.');
    }
    const metadata = (advance.metadata as Record<string, unknown> | null) ?? {};
    const advanceAccountId = toBigIntOrNull(metadata.advanceAccountId);
    if (!advanceAccountId) {
      throw new BadRequestException(
        'Tidak bisa posting: akun Uang Muka Pembelian (advanceAccountId) belum diisi untuk uang muka ini.',
      );
    }

    const legs: LedgerLeg[] = [
      {
        accountId: advanceAccountId,
        debit: amount,
        credit: new Prisma.Decimal(0),
        description: 'Uang Muka Pembelian',
        partnerId: advance.partnerId,
      },
      {
        accountId: advance.bankAccountId,
        debit: new Prisma.Decimal(0),
        credit: amount,
        description: advance.description,
        partnerId: advance.partnerId,
      },
    ];

    const base: LedgerBase = {
      branchId: advance.branchId,
      locationId: advance.locationId,
      sourceDocType: PUR_GL_DOCTYPE,
      sourceId: advance.id,
      docNumber: advance.docNumber,
      entryDate: advance.transactionDate,
      fiscalPeriodId: advance.fiscalPeriodId,
      currencyId: advance.currencyId,
      exchangeRate: advance.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: PUR_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, advanceId: bigint): Promise<void> {
    await reverseInvLedger(tx, PUR_GL_DOCTYPE, advanceId);
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
