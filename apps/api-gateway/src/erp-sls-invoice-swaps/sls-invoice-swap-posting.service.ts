import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const SLS_GL_SOURCE = 'SALES';
const SLS_GL_DOCTYPE = 'sls_invoice_swaps';

type SwapWithLines = Prisma.ErpSlsInvoiceSwapGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Invoice Swaps (SIE) → fin_ledger_entries.
 *
 * Confirmed with user: SIE reallocates outstanding AR balance between
 * invoices (possibly different customers) without changing any total — a
 * pure reclassification journal, per the comment this service already had.
 * Per line: Cr fromInvoice's receivableAccountId (fallback its customer) /
 * Dr toInvoice's receivableAccountId (fallback its customer), each leg
 * carrying its own invoice's partnerId so the AR sub-ledger stays correct
 * even when the two invoices belong to different customers.
 *
 * `toInvoiceId` is nullable on the schema but is REQUIRED here — a swap
 * line with no destination has nothing to reallocate to (a write-off would
 * need its own contra account, out of scope; reject explicitly instead of
 * guessing).
 */
@Injectable()
export class SlsInvoiceSwapPostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    swap: SwapWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!swap.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris invoice swap.');
    }
    const hasZero = swap.lines.some((l) => new Prisma.Decimal(l.amount.toString()).lte(0));
    if (hasZero) {
      throw new BadRequestException('Semua baris harus memiliki amount > 0.');
    }
    const missingTo = swap.lines.find((l) => !l.toInvoiceId);
    if (missingTo) {
      throw new BadRequestException(
        `Baris ${missingTo.lineNo} tidak punya toInvoiceId — SIE wajib realokasi ke invoice tujuan, bukan write-off.`,
      );
    }

    const invoiceIds = [
      ...new Set(swap.lines.flatMap((l) => [l.fromInvoiceId, l.toInvoiceId!])),
    ];
    const invoices = await tx.erpSlsInvoice.findMany({
      where: { id: { in: invoiceIds }, deletedAt: null },
      select: { id: true, receivableAccountId: true, customerId: true },
    });
    const invoiceById = new Map(invoices.map((i) => [i.id.toString(), i]));
    if (invoices.length !== invoiceIds.length) {
      throw new BadRequestException('Satu atau lebih invoice pada baris swap tidak ditemukan.');
    }

    const legs: LedgerLeg[] = [];
    for (const line of swap.lines) {
      const fromInvoice = invoiceById.get(line.fromInvoiceId.toString())!;
      const toInvoice = invoiceById.get(line.toInvoiceId!.toString())!;
      const fromAccountId = await this.resolveReceivableAccount(tx, fromInvoice);
      const toAccountId = await this.resolveReceivableAccount(tx, toInvoice);
      const amount = new Prisma.Decimal(line.amount);

      legs.push({
        accountId: fromAccountId,
        debit: new Prisma.Decimal(0),
        credit: amount,
        description: `Swap ke invoice ${line.toInvoiceId}`,
        partnerId: fromInvoice.customerId,
      });
      legs.push({
        accountId: toAccountId,
        debit: amount,
        credit: new Prisma.Decimal(0),
        description: `Swap dari invoice ${line.fromInvoiceId}`,
        partnerId: toInvoice.customerId,
      });
    }

    const base: LedgerBase = {
      branchId: swap.branchId,
      locationId: null,
      sourceDocType: SLS_GL_DOCTYPE,
      sourceId: swap.id,
      docNumber: swap.docNumber,
      entryDate: swap.docDate,
      fiscalPeriodId: swap.fiscalPeriodId,
      currencyId: swap.currencyId,
      exchangeRate: swap.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: SLS_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, swapId: bigint): Promise<void> {
    await reverseInvLedger(tx, SLS_GL_DOCTYPE, swapId);
  }

  private async resolveReceivableAccount(
    tx: Prisma.TransactionClient,
    invoice: { id: bigint; receivableAccountId: bigint | null; customerId: bigint | null },
  ): Promise<bigint> {
    if (invoice.receivableAccountId) return invoice.receivableAccountId;
    if (invoice.customerId) {
      const customer = await tx.erpPartner.findUnique({
        where: { id: invoice.customerId },
        select: { receivableAccountId: true },
      });
      if (customer?.receivableAccountId) return customer.receivableAccountId;
    }
    throw new BadRequestException(
      `Tidak bisa posting: invoice ${invoice.id} tidak punya akun piutang (receivable account) di dokumen maupun master pelanggan.`,
    );
  }
}
