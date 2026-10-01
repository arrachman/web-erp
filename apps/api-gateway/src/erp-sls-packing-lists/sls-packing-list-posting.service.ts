import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type PackingListWithLines = Prisma.ErpSlsPackingListGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Packing Lists → fin_ledger_entries.
 *
 * A Packing List is a physical goods-packing document (goods prepared for
 * shipment), NOT a financial event by itself. No AR or revenue recognition
 * happens at PL time — that occurs at Delivery Order (COGS/inventory) and
 * Sales Invoice (AR/revenue).
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it only validates that
 * the document has at least one line, then marks the workflow status POSTED in
 * the caller (no ledger rows are written). The method signature is kept parallel
 * to other posting services so a future upgrade can drop in real double-entry
 * posting without changing call sites.
 */
@Injectable()
export class SlsPackingListPostingService {
  /**
   * NO-OP for Packing Lists (see class docs). Validates that lines exist so a
   * POST of an empty packing list is rejected, then returns without writing
   * ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    packingList: PackingListWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!packingList.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — PL posts no GL entries.
  }

  /**
   * NO-OP reversal. A Packing List never created ledger rows, so REOPEN /
   * re-post has nothing to remove. Kept for signature parity.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _packingListId: bigint): Promise<void> {
    // No entries to remove — PL posts no GL entries.
  }
}
