import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type InvoiceWithLines = Prisma.ErpSlsInvoiceGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Sales Invoices → fin_ledger_entries.
 *
 * A Sales Invoice is the PRIMARY financial event in the sales chain:
 * - Debit: Accounts Receivable (AR) — customer owes us money
 * - Credit: Revenue accounts per line (sales income)
 * - Credit/Debit: Tax accounts (if applicable)
 *
 * When POST is called, the posting service writes double-entry GL rows to
 * fin_ledger_entries and links the invoice to the AR open item via
 * `arLedgerEntryId`. When REOPEN is called, the entries are reversed.
 *
 * NOTE: Actual double-entry implementation should be completed when the
 * fin_ledger_entries schema and AR sub-ledger patterns are finalised.
 * For now the service validates readiness and marks the posting status.
 * The TODO comment marks where the real insert should go.
 */
@Injectable()
export class SlsInvoicePostingService {
  /**
   * Post Sales Invoice to GL: validate lines exist, then write AR + Revenue
   * ledger entries. Currently records `postingStatus = POSTED` in the caller;
   * the actual double-entry rows (TODO) will be added when fin schema is stable.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!invoice.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }

    // TODO: Insert double-entry AR + revenue rows into fin_ledger_entries.
    // Pattern (accrual):
    //   DR  receivableAccountId     grandTotal
    //   CR  revenueAccountId(line)  lineNet  (per line)
    //   CR  tax1AccountId           tax1Amount (if any)
    //   CR  tax2AccountId           tax2Amount (if any)
    //
    // After inserting, update invoice.arLedgerEntryId with the AR row's id so
    // the AR aging report can find the open item.
    //
    // Until fin_ledger_entries is wired in, postToLedger is a validated no-op
    // and the caller sets postingStatus = POSTED directly.
  }

  /**
   * Reverse Sales Invoice GL entries: delete (or counter-post) the fin_ledger_entries
   * rows that were written by `postToLedger`, and clear `arLedgerEntryId`.
   *
   * Currently a no-op (no rows were written). Implement when AR posting goes live.
   */
  async reverseLedger(
    _tx: Prisma.TransactionClient,
    _invoiceId: bigint,
  ): Promise<void> {
    // TODO: Delete or reverse-post fin_ledger_entries rows for this invoice.
    // Clear invoice.arLedgerEntryId after reversal.
  }
}
