import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

const SOURCE = 'GIRO';
const SOURCE_DOC_TYPE = 'fin_giro_entries';

type GiroEntry = Prisma.ErpFinGiroEntryGetPayload<object>;
type Giro = Prisma.ErpFinGiroGetPayload<object>;

/**
 * GL posting for CLEAR (RGC/SGC) giro entries → fin_ledger_entries.
 *
 * Each cleared giro produces a balanced 2-row pair:
 *   - settlement bank account = entry.bankAccountId (REQUIRED)
 *   - control account = giro.giroAccountId (REQUIRED)
 *   INCOMING (RGC): Dr settlementBank, Cr controlAccount.
 *   OUTGOING (SGC): Dr controlAccount, Cr settlementBank.
 * Keyed by sourceDocType='fin_giro_entries'/sourceId. Append-on-post; REOPEN
 * hard-deletes this document's own rows. Mirrors JournalPostingService.
 */
@Injectable()
export class GiroPostingService {
  /** Build + insert the balanced ledger rows for a posted CLEAR entry. */
  async postClearing(
    tx: Prisma.TransactionClient,
    entry: GiroEntry,
    clearedGiros: Giro[],
    actorId: bigint | null,
  ) {
    if (entry.bankAccountId === null || entry.bankAccountId === undefined) {
      throw new BadRequestException('Bank settlement (bankAccountId) wajib untuk posting kliring giro.');
    }
    const settlementBankId = entry.bankAccountId;

    const base = {
      branchId: entry.branchId,
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
      status: 'POSTED' as const,
      postingStatus: 'POSTED' as const,
      postedAt: new Date(),
      createdById: actorId,
      updatedById: actorId,
    };

    const zero = new Prisma.Decimal(0);
    const rows: Prisma.ErpFinLedgerEntryCreateManyInput[] = [];
    let lineNo = 1;

    for (const giro of clearedGiros) {
      if (giro.giroAccountId === null || giro.giroAccountId === undefined) {
        throw new BadRequestException(
          `Giro ${giro.giroNumber} belum punya Akun Giro untuk diposting.`,
        );
      }
      const amount = new Prisma.Decimal(giro.amount);
      const desc = entry.description ?? giro.giroNumber;
      const isIncoming = entry.type === 'INCOMING';

      // Settlement bank leg
      rows.push({
        ...base,
        accountId: settlementBankId,
        description: desc,
        debit: isIncoming ? amount : zero,
        credit: isIncoming ? zero : amount,
        lineNo: lineNo++,
      });
      // Control (giro) account leg
      rows.push({
        ...base,
        accountId: giro.giroAccountId,
        description: desc,
        debit: isIncoming ? zero : amount,
        credit: isIncoming ? amount : zero,
        lineNo: lineNo++,
      });
    }

    if (rows.length) await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  /** Remove this document's posted ledger rows (used by REOPEN / re-post). */
  async reverseLedger(tx: Prisma.TransactionClient, entryId: bigint) {
    await tx.erpFinLedgerEntry.deleteMany({
      where: { sourceDocType: SOURCE_DOC_TYPE, sourceId: entryId },
    });
  }
}
