import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type DailyCheckWithLines = Prisma.ErpInvDailyCheckGetPayload<{
  include: { lines: true };
}>;

/**
 * Posting seam for Daily Check (DC).
 *
 * DC is an operational / time-sheet record — it does not generate GL journal
 * entries. Posting simply validates that lines exist and flips the document
 * status to POSTED. Future GL integration can drop in here without touching
 * call sites.
 */
@Injectable()
export class InvDailyCheckPostingService {
  /**
   * Validates the daily check has at least one line. No ledger rows are written.
   * DC is an operational record — no GL posting required.
   */
  async postDailyCheck(
    _tx: Prisma.TransactionClient,
    dailyCheck: DailyCheckWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!dailyCheck.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no ledger rows — DC is an operational record, no GL.
  }

  /**
   * NO-OP reversal. Reopening a daily check flips it out of POSTED.
   * Kept for signature parity with other posting services.
   */
  async reverseDailyCheck(
    _tx: Prisma.TransactionClient,
    _dailyCheckId: bigint,
  ): Promise<void> {
    // No entries to remove — DC has no GL impact.
  }
}
