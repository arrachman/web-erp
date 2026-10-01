import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

/**
 * Weighbridge ticket posting seam.
 *
 * Weighbridge tickets record gross/tare/net weight at the point of receipt.
 * They do NOT post GL entries directly — future inventory-valuation logic
 * (linking RW to a GR or purchase receipt) will drop in here without changing
 * call sites.
 */
@Injectable()
export class InvWeighbridgeTicketPostingService {
  /**
   * NO-OP post. Weighbridge ticket posts no GL — future inventory valuation seam.
   */
  async postTicket(
    _tx: Prisma.TransactionClient,
    _ticketId: bigint,
    _actorId: bigint | null,
  ): Promise<void> {
    // Intentionally empty — future valuation logic drops in here.
  }

  /**
   * NO-OP reversal. Kept for signature parity with other posting services.
   */
  async reverseTicket(_tx: Prisma.TransactionClient, _ticketId: bigint): Promise<void> {
    // No GL entries to remove.
  }
}
