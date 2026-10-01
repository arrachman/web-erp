import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type ReturnWithLines = Prisma.ErpSlsReturnGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Sales Returns → fin_ledger_entries.
 *
 * IMPORTANT: a Sales Return is a *reversal commitment* — it acknowledges that
 * goods are being returned by the customer. The financial recognition (AR
 * credit note / inventory revaluation) happens in a downstream document (e.g.
 * Return Receipt). Therefore `postToLedger` here is a deliberate NO-OP: it
 * only validates that the document has at least one line, then marks the
 * workflow status POSTED in the caller (no ledger rows are written). The
 * method signature is kept parallel to CashBankPostingService so a future
 * posting service can drop in real double-entry posting without changing call sites.
 */
@Injectable()
export class SlsReturnPostingService {
  /**
   * NO-OP for Sales Returns (see class docs). Validates that lines exist so a
   * POST of an empty return is rejected, then returns without writing ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    slsReturn: ReturnWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!slsReturn.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — SR posts no GL entries.
  }

  /**
   * NO-OP reversal. A Sales Return never created ledger rows, so REOPEN / re-post
   * has nothing to remove. Kept for signature parity with CashBankPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _returnId: bigint): Promise<void> {
    // No entries to remove — SR posts no GL entries.
  }
}
