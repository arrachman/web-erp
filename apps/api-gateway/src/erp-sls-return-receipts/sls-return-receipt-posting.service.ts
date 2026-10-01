import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type ReturnReceiptWithLines = Prisma.ErpSlsReturnReceiptGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Return Receipts → fin_ledger_entries.
 *
 * IMPORTANT: a Return Receipt (RNR) acknowledges the physical receipt of
 * returned goods. The financial recognition (inventory reinstatement / AR
 * credit note allocation) is typically a downstream journal. Therefore
 * `postToLedger` here is a deliberate NO-OP: it only validates that the
 * document has at least one line, then marks the workflow status POSTED in
 * the caller (no ledger rows are written). The method signature is kept
 * parallel to CashBankPostingService so a future posting service can drop in
 * real double-entry posting without changing call sites.
 */
@Injectable()
export class SlsReturnReceiptPostingService {
  /**
   * NO-OP for Return Receipts (see class docs). Validates that lines exist so
   * a POST of an empty return receipt is rejected.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    returnReceipt: ReturnReceiptWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!returnReceipt.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — RNR posts no GL entries.
  }

  /**
   * NO-OP reversal. A Return Receipt never created ledger rows, so REOPEN /
   * re-post has nothing to remove. Kept for signature parity with
   * CashBankPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _returnReceiptId: bigint): Promise<void> {
    // No entries to remove — RNR posts no GL entries.
  }
}
