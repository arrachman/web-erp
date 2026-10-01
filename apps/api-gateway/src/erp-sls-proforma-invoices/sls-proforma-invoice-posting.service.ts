import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

type ProformaInvoiceWithLines = Prisma.ErpSlsProformaInvoiceGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Proforma Invoices → fin_ledger_entries.
 *
 * IMPORTANT: a Proforma Invoice is a *pre-billing* document (invoice preview
 * sent before final invoice), NOT yet a financial event. In standard accrual
 * accounting nothing is recognised at PI time — no AR, no revenue. Recognition
 * happens at the Sales Invoice stage.
 *
 * Therefore `postToLedger` here is a deliberate NO-OP: it only validates that
 * the document has at least one line, then marks the workflow status POSTED in
 * the caller (no ledger rows are written). The method signature is kept parallel
 * to CashBankPostingService so a future Sales Invoice posting service can drop
 * in real double-entry posting without changing the call sites.
 */
@Injectable()
export class SlsProformaInvoicePostingService {
  /**
   * NO-OP for Proforma Invoices (see class docs). Validates that lines exist so a
   * POST of an empty proforma invoice is rejected, then returns without writing ledger rows.
   */
  async postToLedger(
    _tx: Prisma.TransactionClient,
    proformaInvoice: ProformaInvoiceWithLines,
    _actorId: bigint | null,
  ): Promise<void> {
    if (!proformaInvoice.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Intentionally no fin_ledger_entries written — PI posts no GL entries.
  }

  /**
   * NO-OP reversal. A Proforma Invoice never created ledger rows, so REOPEN / re-post
   * has nothing to remove. Kept for signature parity with CashBankPostingService.
   */
  async reverseLedger(_tx: Prisma.TransactionClient, _proformaInvoiceId: bigint): Promise<void> {
    // No entries to remove — PI posts no GL entries.
  }
}
