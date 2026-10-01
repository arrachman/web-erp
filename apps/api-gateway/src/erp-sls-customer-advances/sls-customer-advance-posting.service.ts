import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type AdvanceRow = Prisma.ErpSlsCustomerAdvanceGetPayload<Record<string, never>>;

/**
 * GL posting for Customer Advances → fin_ledger_entries.
 *
 * A Customer Advance (AS) is a receipt of cash from a customer BEFORE goods/
 * services are delivered. In accrual accounting the posting is:
 *   DR  Cash / Bank (ar_receipt_id links to the actual receipt)
 *   CR  Deferred Revenue / Customer Advance Liability
 *
 * The full double-entry is handled by the linked AR Receipt document.
 * Here we perform a lightweight NO-OP similar to SlsOrderPostingService:
 * we validate the document is ready to post, then mark it POSTED. Actual
 * ledger entries are created when the linked AR Receipt is posted or when
 * the advance is applied to a Sales Invoice.
 */
@Injectable()
export class SlsCustomerAdvancePostingService {
  /**
   * Validate and mark ready for POSTED status. Writes no ledger rows — the
   * linked AR Receipt (arReceiptId) owns the cash entry; invoice application
   * owns the offset entry. Kept signature-parallel to other posting services.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    advance: AdvanceRow,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!advance.amount || new Prisma.Decimal(advance.amount.toString()).lte(0)) {
      throw new BadRequestException('Tidak bisa posting: amount harus lebih besar dari 0.');
    }
    // Intentionally no fin_ledger_entries written — posting is handled by AR Receipt.
  }

  /**
   * NO-OP reversal. Ledger entries (if any) are reversed by the AR Receipt
   * or invoice application reversal. Kept for signature parity.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _advanceId: bigint): Promise<void> {
    // No entries to remove at this level.
  }
}
