import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const SLS_GL_SOURCE = 'SALES';
const SLS_GL_DOCTYPE = 'sls_customer_advances';

type AdvanceRow = Prisma.ErpSlsCustomerAdvanceGetPayload<Record<string, never>>;

/**
 * GL posting for Customer Advances (AS) → fin_ledger_entries.
 *
 * §2 posting matrix (DECISIONS.md "Pola jurnal usulan" — AS/IP row):
 *   Dr  Kas atau Bank     amount
 *   Cr  Uang Muka Penjualan (Customer Advance Liability)   amount
 *
 * Posts directly — does NOT go through AR Receipt. AR Receipt's allocations
 * are tied to settling a specific sls_invoices outstanding balance
 * (ErpFinSettlementAllocation.invoiceRef); an advance has no invoice to
 * settle yet, so it is not a valid AR Receipt allocation target. AS is a
 * self-contained posting, same shape as SI/SR.
 *
 * Neither account has a dedicated column on sls_customer_advances (unlike
 * SI, which has receivableAccountId/discountAccountId etc.) — both are kept
 * in `metadata.bankAccountId`/`metadata.advanceAccountId`, set at create
 * time via CreateSlsCustomerAdvanceDto. Required at POST time, not at
 * create time, so a DRAFT advance can be entered before accounts are known.
 */
@Injectable()
export class SlsCustomerAdvancePostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    advance: AdvanceRow,
    actorId: bigint | null,
  ): Promise<void> {
    const amount = new Prisma.Decimal(advance.amount?.toString() ?? 0);
    if (amount.lte(0)) {
      throw new BadRequestException('Tidak bisa posting: amount harus lebih besar dari 0.');
    }

    const metadata = (advance.metadata as Record<string, unknown> | null) ?? {};
    const bankAccountId = toBigIntOrNull(metadata.bankAccountId);
    const advanceAccountId = toBigIntOrNull(metadata.advanceAccountId);
    if (!bankAccountId) {
      throw new BadRequestException(
        'Tidak bisa posting: akun kas/bank (bankAccountId) belum diisi untuk uang muka ini.',
      );
    }
    if (!advanceAccountId) {
      throw new BadRequestException(
        'Tidak bisa posting: akun Uang Muka Penjualan (advanceAccountId) belum diisi untuk uang muka ini.',
      );
    }

    const legs: LedgerLeg[] = [
      {
        accountId: bankAccountId,
        debit: amount,
        credit: new Prisma.Decimal(0),
        description: advance.description,
        partnerId: advance.customerId,
      },
      {
        accountId: advanceAccountId,
        debit: new Prisma.Decimal(0),
        credit: amount,
        description: 'Uang Muka Penjualan',
        partnerId: advance.customerId,
        costCenterId: advance.costCenterId,
        divisionId: advance.divisionId,
        projectId: advance.projectId,
      },
    ];

    const base: LedgerBase = {
      branchId: advance.branchId,
      locationId: null,
      sourceDocType: SLS_GL_DOCTYPE,
      sourceId: advance.id,
      docNumber: advance.docNumber,
      entryDate: advance.docDate,
      fiscalPeriodId: advance.fiscalPeriodId,
      currencyId: advance.currencyId,
      exchangeRate: advance.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: SLS_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, advanceId: bigint): Promise<void> {
    await reverseInvLedger(tx, SLS_GL_DOCTYPE, advanceId);
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
