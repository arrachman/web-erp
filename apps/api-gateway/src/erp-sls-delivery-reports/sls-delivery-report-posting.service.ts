import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type DeliveryReportWithLines = Prisma.ErpSlsDeliveryReportGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Delivery Reports → fin_ledger_entries.
 *
 * A Delivery Report (DR) is a confirmation that goods have been received/accepted
 * by the customer. In standard accrual accounting, COGS recognition and inventory
 * reduction happen at Delivery Order (DO) stage; DR is a logistics confirmation.
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it validates that the
 * document has at least one line, then returns without writing ledger rows.
 * The method signature is kept parallel to CashBankPostingService so a future
 * real double-entry posting service can drop in without changing call sites.
 */
@Injectable()
export class SlsDeliveryReportPostingService {
  /**
   * NO-OP for Delivery Reports (see class docs). Validates that lines exist so a
   * POST of an empty DR is rejected, then returns without writing ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    report: DeliveryReportWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!report.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — DR posts no GL entries.
  }

  /**
   * NO-OP reversal. A Delivery Report never created ledger rows, so REOPEN /
   * re-post has nothing to remove. Kept for signature parity.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _reportId: bigint): Promise<void> {
    // No entries to remove — DR posts no GL entries.
  }
}
