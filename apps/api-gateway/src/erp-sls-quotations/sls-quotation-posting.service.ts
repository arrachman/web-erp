import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type QuotationWithLines = Prisma.ErpSlsQuotationGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Sales Quotations → fin_ledger_entries.
 *
 * IMPORTANT: a Sales Quotation is a *pre-commitment* (quote to a customer),
 * NOT a financial event. In standard accrual accounting nothing is recognised
 * at SQ time — no AR, no revenue, no inventory movement. Recognition happens
 * later in the chain: Sales Order → Delivery Order (COGS/inventory) and
 * Sales Invoice (AR/revenue).
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it only validates that
 * the document has at least one line, then marks the workflow status POSTED in
 * the caller (no ledger rows are written). The method signature is kept parallel
 * to CashBankPostingService so a future posting service can drop in real
 * double-entry posting without changing the call sites.
 */
@Injectable()
export class SlsQuotationPostingService {
  /**
   * NO-OP for Sales Quotations (see class docs). Validates that lines exist so a
   * POST of an empty quotation is rejected, then returns without writing ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    quotation: QuotationWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!quotation.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — SQ posts no GL entries.
  }

  /**
   * NO-OP reversal. A Sales Quotation never created ledger rows, so REOPEN / re-post
   * has nothing to remove. Kept for signature parity with CashBankPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _quotationId: bigint): Promise<void> {
    // No entries to remove — SQ posts no GL entries.
  }
}
