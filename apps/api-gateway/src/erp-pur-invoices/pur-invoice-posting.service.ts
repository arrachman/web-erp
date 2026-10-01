import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type InvoiceWithLines = Prisma.ErpPurInvoiceGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Purchase Invoices → fin_ledger_entries.
 *
 * TODO (follow-up): a Purchase Invoice SHOULD post AP — Dr inventory/expense (per
 * line account) + Dr input VAT, Cr Accounts Payable (payableAccountId) — gated by
 * the 3-way match (`matchStatus` must be MATCHED or WAIVED before AP posts). That
 * double-entry posting is deferred to a dedicated pass.
 *
 * For now `postToLedger` is a deliberate NO-OP (parity with PO/PR): it only
 * validates that the document has at least one line, then marks the workflow
 * status POSTED in the caller. The method signature mirrors the other purchasing
 * posting services so real AP posting can drop in without changing call sites.
 */
@Injectable()
export class PurInvoicePostingService {
  async postToLedger(
    _tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!invoice.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written yet — AP posting is a follow-up.
  }

  async reverseLedger(_tx: Prisma.TransactionClient, _invoiceId: bigint): Promise<void> {
    // No entries to remove — PI posts no GL entries yet.
  }
}
