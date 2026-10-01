import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type DeliveryOrderWithLines = Prisma.ErpSlsDeliveryOrderGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Delivery Orders → fin_ledger_entries.
 *
 * A Delivery Order records the physical dispatch of goods to the customer.
 * Full COGS/inventory movement recognition will be wired here once the inventory
 * valuation layer is ready. Until then this service is a deliberate NO-OP: it
 * validates that the document has at least one line, then marks the workflow
 * status POSTED in the caller (no ledger rows are written).
 *
 * The method signature is kept parallel to other posting services so real
 * double-entry posting can be dropped in without changing call sites.
 */
@Injectable()
export class SlsDeliveryOrderPostingService {
  /**
   * NO-OP for Delivery Orders (see class docs). Validates that lines exist so a
   * POST of an empty delivery order is rejected, then returns without writing
   * ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    deliveryOrder: DeliveryOrderWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!deliveryOrder.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — DO posts no GL entries yet.
  }

  /**
   * NO-OP reversal. Until inventory posting is implemented there are no ledger
   * rows to remove. Kept for signature parity.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _deliveryOrderId: bigint): Promise<void> {
    // No entries to remove — DO posts no GL entries yet.
  }
}
