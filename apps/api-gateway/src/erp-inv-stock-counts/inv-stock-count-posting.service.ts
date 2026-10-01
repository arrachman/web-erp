import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type CountWithLines = Prisma.ErpInvStockCountGetPayload<{
  include: { lines: true };
}>;

/**
 * Posting for stock counts (opname).
 *
 * A stock count records the physical-vs-system snapshot; it does NOT itself move
 * stock or write a ledger. Any inventory correction is produced SEPARATELY as a
 * Stock Adjustment (inv_stock_adjustments) that references this count. Therefore
 * POST here is purely a status flip: it finalizes the count. This service only
 * validates the document has lines and leaves a single seam (`postCount` /
 * `reverseCount`) for parity with the movement module — it writes nothing.
 */
@Injectable()
export class InvStockCountPostingService {
  /**
   * Validates the count has at least one line. NO-OP otherwise — adjustments are
   * generated separately from the finalized count.
   */
  async postCount(
    _tx: Prisma.TransactionClient,
    count: CountWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!count.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no ledger rows — a stock count produces adjustments separately.
  }

  /**
   * NO-OP reversal. Reopening a count flips it out of POSTED; nothing was written
   * on post, so nothing to undo. Kept for signature parity with the movement module.
   */
  async reverseCount(_tx: Prisma.TransactionClient, _countId: bigint): Promise<void> {
    // No entries to remove — a stock count writes no ledger rows.
  }
}
