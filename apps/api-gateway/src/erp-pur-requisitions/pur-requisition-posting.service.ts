import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type RequisitionWithLines = Prisma.ErpPurRequisitionGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Purchase Requisitions → fin_ledger_entries.
 *
 * IMPORTANT: a Purchase Requisition is an *internal request* to procure (the
 * start of the procure-to-pay chain), NOT a financial event. In standard accrual
 * accounting nothing is recognised at PR time — no AP, no expense, no inventory
 * movement. Recognition happens far downstream: Goods Receipt (inventory + GR/IR
 * accrual) and Purchase Invoice (AP / 3-way match).
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it only validates that the
 * document has at least one line, then marks the workflow status POSTED in the
 * caller (no ledger rows are written). The method signature is kept parallel to
 * PurOrderPostingService so a future posting service can drop in real double-entry
 * posting without changing the call sites.
 */
@Injectable()
export class PurRequisitionPostingService {
  /**
   * NO-OP for Purchase Requisitions (see class docs). Validates that lines exist
   * so a POST of an empty requisition is rejected, then returns without writing
   * ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    requisition: RequisitionWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!requisition.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — PR posts no GL entries.
  }

  /**
   * NO-OP reversal. A Purchase Requisition never created ledger rows, so REOPEN /
   * re-post has nothing to remove. Kept for signature parity with
   * PurOrderPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _requisitionId: bigint): Promise<void> {
    // No entries to remove — PR posts no GL entries.
  }
}
