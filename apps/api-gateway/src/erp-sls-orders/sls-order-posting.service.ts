import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type OrderWithLines = Prisma.ErpSlsOrderGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Sales Orders → fin_ledger_entries.
 *
 * IMPORTANT: a Sales Order is a *commitment* (customer intends to buy), NOT a
 * financial event. In standard accrual accounting nothing is recognised at SO
 * time — no AR, no revenue, no inventory movement. Recognition happens later in
 * the chain: Delivery Order (COGS/inventory) and Sales Invoice (AR/revenue).
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it only validates that the
 * document has at least one line, then marks the workflow status POSTED in the
 * caller (no ledger rows are written). The method signature is kept parallel to
 * CashBankPostingService so a future Sales Invoice / Delivery Order posting
 * service can drop in real double-entry posting without changing the call sites.
 */
@Injectable()
export class SlsOrderPostingService {
  /**
   * NO-OP for Sales Orders (see class docs). Validates that lines exist so a
   * POST of an empty order is rejected, then returns without writing ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    order: OrderWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!order.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — SO posts no GL entries.
  }

  /**
   * NO-OP reversal. A Sales Order never created ledger rows, so REOPEN / re-post
   * has nothing to remove. Kept for signature parity with CashBankPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _orderId: bigint): Promise<void> {
    // No entries to remove — SO posts no GL entries.
  }
}
