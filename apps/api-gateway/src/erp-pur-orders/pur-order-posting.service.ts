import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type OrderWithLines = Prisma.ErpPurOrderGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Purchase Orders → fin_ledger_entries.
 *
 * IMPORTANT: a Purchase Order is a *commitment* (we intend to buy from a
 * supplier), NOT a financial event. In standard accrual accounting nothing is
 * recognised at PO time — no AP, no expense, no inventory movement. Recognition
 * happens later in the chain: Goods Receipt (inventory + GR/IR accrual) and
 * Purchase Invoice (AP / 3-way match).
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it only validates that the
 * document has at least one line, then marks the workflow status POSTED in the
 * caller (no ledger rows are written). The method signature is kept parallel to
 * SlsOrderPostingService so a future Goods Receipt / Purchase Invoice posting
 * service can drop in real double-entry posting without changing the call sites.
 */
@Injectable()
export class PurOrderPostingService {
  /**
   * NO-OP for Purchase Orders (see class docs). Validates that lines exist so a
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
    // Intentionally no fin_ledger_entries written — PO posts no GL entries.
  }

  /**
   * NO-OP reversal. A Purchase Order never created ledger rows, so REOPEN / re-post
   * has nothing to remove. Kept for signature parity with SlsOrderPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _orderId: bigint): Promise<void> {
    // No entries to remove — PO posts no GL entries.
  }
}
