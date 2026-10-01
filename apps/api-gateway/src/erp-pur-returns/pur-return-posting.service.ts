import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type ReturnWithLines = Prisma.ErpPurReturnGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Purchase Returns (DNR / PRT) → fin_ledger_entries.
 *
 * TODO (follow-up): a posted return SHOULD reverse the purchase — Dr Accounts
 * Payable (payableAccountId), Cr inventory / purchase-return account
 * (returnPurchaseAccountId) + adjust input VAT; RETURN_TO_VENDOR also moves stock
 * out. That double-entry posting is deferred to a dedicated pass.
 *
 * For now `postToLedger` is a deliberate NO-OP (parity with PO/PR/PI): validates
 * at least one line, then the caller marks status POSTED. Signature mirrors the
 * other purchasing posting services so real posting can drop in later.
 */
@Injectable()
export class PurReturnPostingService {
  async postToLedger(
    _tx: Prisma.TransactionClient,
    purReturn: ReturnWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!purReturn.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written yet — return posting is a follow-up.
  }

  async reverseLedger(_tx: Prisma.TransactionClient, _returnId: bigint): Promise<void> {
    // No entries to remove — return posts no GL entries yet.
  }
}
