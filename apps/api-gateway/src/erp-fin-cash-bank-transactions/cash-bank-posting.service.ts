import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

const SOURCE = 'CASH_BANK';
const SOURCE_DOC_TYPE = 'fin_cash_bank_transactions';

type TxnWithLines = Prisma.ErpFinCashBankTransactionGetPayload<{
  include: { lines: true };
}>;

/**
 * Double-entry posting for cash/bank transactions → fin_ledger_entries.
 *
 * RECEIPT (Kas/Bank Masuk):  Dr bank account (header) ; Cr each contra line.
 * DISBURSEMENT (Kas/Bank Keluar): Cr bank account (header) ; Dr each contra line.
 *
 * Append-on-post; reversal (REOPEN) hard-deletes this document's own entries
 * — non-posting corrections go through a fresh post. Mirrors the posting matrix
 * in apps/web-erp/db-design/entities-m2-finance.md.
 */
@Injectable()
export class CashBankPostingService {
  /** Build + insert the balanced ledger rows for a posted transaction. */
  async postToLedger(
    tx: Prisma.TransactionClient,
    txn: TxnWithLines,
    actorId: bigint | null,
  ) {
    if (!txn.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris akun.');
    }

    const lineSum = txn.lines.reduce(
      (s, l) => s.add(new Prisma.Decimal(l.amount)),
      new Prisma.Decimal(0),
    );
    if (!lineSum.equals(new Prisma.Decimal(txn.amount))) {
      throw new BadRequestException(
        `Total baris (${lineSum}) tidak sama dengan total header (${txn.amount}).`,
      );
    }
    if (lineSum.lessThanOrEqualTo(0)) {
      throw new BadRequestException('Total transaksi harus lebih dari nol.');
    }

    const isReceipt = txn.direction === 'RECEIPT';
    const base = {
      branchId: txn.branchId,
      locationId: txn.locationId,
      source: SOURCE,
      sourceDocType: SOURCE_DOC_TYPE,
      sourceId: txn.id,
      docNumber: txn.docNumber,
      entryDate: txn.transactionDate,
      fiscalPeriodId: txn.fiscalPeriodId,
      currencyId: txn.currencyId,
      exchangeRate: txn.exchangeRate,
      reconciliationStatus: 'UNRECONCILED' as const,
      status: 'POSTED' as const,
      postingStatus: 'POSTED' as const,
      postedAt: new Date(),
      createdById: actorId,
      updatedById: actorId,
    };

    const zero = new Prisma.Decimal(0);

    // Header cash/bank account row.
    const rows: Prisma.ErpFinLedgerEntryCreateManyInput[] = [
      {
        ...base,
        accountId: txn.bankAccountId,
        partnerId: txn.partnerId,
        description: txn.description,
        debit: isReceipt ? new Prisma.Decimal(txn.amount) : zero,
        credit: isReceipt ? zero : new Prisma.Decimal(txn.amount),
        lineNo: 1,
      },
    ];

    // Contra account rows (opposite side of the cash account).
    txn.lines.forEach((l, i) => {
      const amt = new Prisma.Decimal(l.amount);
      rows.push({
        ...base,
        accountId: l.accountId,
        partnerId: txn.partnerId,
        description: l.notes ?? txn.description,
        notes: l.notes,
        debit: isReceipt ? zero : amt,
        credit: isReceipt ? amt : zero,
        costCenterId: l.costCenterId,
        divisionId: l.divisionId,
        subdivisionId: l.subdivisionId,
        projectId: l.projectId,
        lineNo: i + 2,
      });
    });

    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  /** Remove this document's posted ledger rows (used by REOPEN / re-post). */
  async reverseLedger(tx: Prisma.TransactionClient, txnId: bigint) {
    await tx.erpFinLedgerEntry.deleteMany({
      where: { sourceDocType: SOURCE_DOC_TYPE, sourceId: txnId },
    });
  }
}
