import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

const SOURCE = 'JOURNAL';
const SOURCE_DOC_TYPE = 'fin_journal_entries';

type EntryWithLines = Prisma.ErpFinJournalEntryGetPayload<{
  include: { lines: true };
}>;

/**
 * Double-entry posting for journal entries → fin_ledger_entries.
 *
 * General/Adjustment/Memorial/Opening-Balance journals post the user-entered
 * lines verbatim: each line's Dr/Cr is copied to the ledger, keyed by
 * sourceDocType='fin_journal_entries'/sourceId. No special GL accounts are
 * injected — the operator enters a balanced set of lines (validated here:
 * Σdebit = Σcredit, total > 0, ≥ 2 lines). `isOpeningBalance` / `isAdjustment`
 * flags are derived from journalType for downstream reports.
 *
 * Append-on-post; REOPEN hard-deletes this document's own entries (re-post is
 * idempotent). Mirrors CashBankPostingService + the matrix in
 * apps/web-erp/db-design/entities-m2-finance.md.
 */
@Injectable()
export class JournalPostingService {
  /** Build + insert the balanced ledger rows for a posted journal entry. */
  async postToLedger(
    tx: Prisma.TransactionClient,
    entry: EntryWithLines,
    actorId: bigint | null,
  ) {
    if (entry.lines.length < 2) {
      throw new BadRequestException('Jurnal harus punya minimal 2 baris untuk diposting.');
    }
    const zero = new Prisma.Decimal(0);
    const totalDebit = entry.lines.reduce((s, l) => s.add(new Prisma.Decimal(l.debit)), zero);
    const totalCredit = entry.lines.reduce((s, l) => s.add(new Prisma.Decimal(l.credit)), zero);
    if (!totalDebit.equals(totalCredit)) {
      throw new BadRequestException(
        `Jurnal tidak balance: total debit (${totalDebit}) ≠ total kredit (${totalCredit}).`,
      );
    }
    if (totalDebit.lessThanOrEqualTo(0)) {
      throw new BadRequestException('Total jurnal harus lebih dari nol.');
    }

    const base = {
      branchId: entry.branchId,
      locationId: entry.locationId,
      source: SOURCE,
      sourceDocType: SOURCE_DOC_TYPE,
      sourceId: entry.id,
      docNumber: entry.docNumber,
      entryDate: entry.entryDate,
      fiscalPeriodId: entry.fiscalPeriodId,
      partnerId: entry.partnerId,
      currencyId: entry.currencyId,
      exchangeRate: entry.exchangeRate,
      reconciliationStatus: 'UNRECONCILED' as const,
      isOpeningBalance: entry.journalType === 'OPENING_BALANCE',
      isAdjustment: entry.journalType === 'ADJUSTMENT',
      status: 'POSTED' as const,
      postingStatus: 'POSTED' as const,
      postedAt: new Date(),
      createdById: actorId,
      updatedById: actorId,
    };

    const rows: Prisma.ErpFinLedgerEntryCreateManyInput[] = entry.lines.map((l, i) => ({
      ...base,
      accountId: l.accountId,
      description: l.notes ?? entry.description,
      notes: l.notes,
      debit: new Prisma.Decimal(l.debit),
      credit: new Prisma.Decimal(l.credit),
      debitFx: l.debitFx ?? null,
      creditFx: l.creditFx ?? null,
      costCenterId: l.costCenterId,
      divisionId: l.divisionId,
      subdivisionId: l.subdivisionId,
      projectId: l.projectId,
      lineNo: l.lineNo ?? i + 1,
    }));

    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  /** Remove this document's posted ledger rows (used by REOPEN / re-post). */
  async reverseLedger(tx: Prisma.TransactionClient, entryId: bigint) {
    await tx.erpFinLedgerEntry.deleteMany({
      where: { sourceDocType: SOURCE_DOC_TYPE, sourceId: entryId },
    });
  }
}
