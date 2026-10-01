import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type SwapWithLines = Prisma.ErpSlsInvoiceSwapGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Invoice Swaps (SIE) → fin_ledger_entries.
 *
 * An Invoice Swap redistributes outstanding balances between invoices
 * (e.g. consolidating or splitting AR balances). In standard accounting the
 * swap itself does not create new value — it is a reallocation journal. Full
 * double-entry (DR/CR AR sub-ledger reclassification) is defined by the
 * finance team per business rule.
 *
 * For now this is a lightweight NO-OP: validates that lines exist and amounts
 * are non-zero, then marks POSTED. Real GL entries can be wired in when the
 * accounting spec is confirmed, without changing the call site in the service.
 */
@Injectable()
export class SlsInvoiceSwapPostingService {
  async postToLedger(
    _tx: Prisma.TransactionClient,
    swap: SwapWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!swap.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris invoice swap.');
    }
    const hasZero = swap.lines.some((l) => new Prisma.Decimal(l.amount.toString()).lte(0));
    if (hasZero) {
      throw new BadRequestException('Semua baris harus memiliki amount > 0.');
    }
    // Intentionally no fin_ledger_entries written — to be implemented when GL spec confirmed.
  }

  async reverseLedger(_tx: Prisma.TransactionClient, _swapId: bigint): Promise<void> {
    // No entries to remove at this level.
  }
}
