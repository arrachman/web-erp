import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';
import { assertLedgerRowsPeriodOpen } from '../erp-common/utils/ledger-period-guard';

const FIN_GL_SOURCE = 'FINANCE';
const FIN_GL_DOCTYPE = 'fin_ar_receipts';

export interface DraftAllocation {
  invoiceId: string;
  amount: string;
  lineNo: number;
}

type ArReceiptWithInstruments = Prisma.ErpFinArReceiptGetPayload<{
  include: { instruments: true };
}>;

/**
 * GL posting + invoice allocation for AR Receipts (IP — Payment Receipt).
 *
 * §2 posting matrix (DECISIONS.md "Pola jurnal usulan" — mirror of AS/IP row):
 *   Dr  Kas/Bank per instrument (instrument.bankAccountId)   instrument.amount
 *   Cr  Piutang Usaha (invoice.receivableAccountId, fallback customer) per allocation
 *
 * `ErpFinSettlementAllocation.ledgerEntryId` is NOT NULL — an allocation row
 * can only exist once its ledger entry exists. So allocation *intent*
 * (invoiceId/amount pairs the user entered) is kept as `metadata.
 * draftAllocations` on the receipt header until POST, not as
 * ErpFinSettlementAllocation rows. POST materializes them into real
 * allocation rows pointing at the newly-created AR credit ledger rows; REOPEN
 * deletes those rows (their ledger entry is gone) while the metadata draft
 * survives for the next POST. This mirrors how DO/GRN keep source-document
 * traceability in `metadata` when no FK column exists for it (see DECISIONS.md
 * § DO stock posting).
 *
 * Outstanding is NOT a stored column anywhere (no FR-SLS-01 remainingAmount
 * tracking exists yet) — computed on the fly from existing
 * ErpFinSettlementAllocation rows at POST time.
 */
@Injectable()
export class ArReceiptPostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    receipt: ArReceiptWithInstruments,
    actorId: bigint | null,
  ): Promise<void> {
    if (!receipt.instruments.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada rincian cara bayar.');
    }
    const draftAllocations = this.readDraftAllocations(receipt.metadata);
    if (!draftAllocations.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada alokasi ke invoice.');
    }

    const instrumentTotal = receipt.instruments.reduce(
      (s, i) => s.add(new Prisma.Decimal(i.amount)),
      new Prisma.Decimal(0),
    );
    const allocationTotal = draftAllocations.reduce(
      (s, a) => s.add(new Prisma.Decimal(a.amount)),
      new Prisma.Decimal(0),
    );
    if (!instrumentTotal.equals(allocationTotal)) {
      throw new BadRequestException(
        `Total cara bayar (${instrumentTotal}) tidak sama dengan total alokasi invoice (${allocationTotal}).`,
      );
    }
    if (!instrumentTotal.equals(new Prisma.Decimal(receipt.amount))) {
      throw new BadRequestException(
        `Total cara bayar (${instrumentTotal}) tidak sama dengan amount header (${receipt.amount}).`,
      );
    }

    const invoiceIds = [...new Set(draftAllocations.map((a) => BigInt(a.invoiceId)))];
    const invoices = await tx.erpSlsInvoice.findMany({
      where: { id: { in: invoiceIds }, deletedAt: null },
      select: { id: true, grandTotal: true, receivableAccountId: true, customerId: true },
    });
    const invoiceById = new Map(invoices.map((i) => [i.id.toString(), i]));
    if (invoices.length !== invoiceIds.length) {
      throw new BadRequestException('Satu atau lebih invoice pada alokasi tidak ditemukan.');
    }

    for (const invoiceId of invoiceIds) {
      const invoice = invoiceById.get(invoiceId.toString())!;
      const allocatedHere = draftAllocations
        .filter((a) => a.invoiceId === invoiceId.toString())
        .reduce((s, a) => s.add(new Prisma.Decimal(a.amount)), new Prisma.Decimal(0));
      const priorAllocated = await tx.erpFinSettlementAllocation.aggregate({
        where: { invoiceRef: invoiceId.toString(), arReceiptId: { not: receipt.id } },
        _sum: { amount: true },
      });
      const alreadyPaid = new Prisma.Decimal(priorAllocated._sum.amount ?? 0);
      const outstanding = new Prisma.Decimal(invoice.grandTotal).sub(alreadyPaid);
      if (allocatedHere.gt(outstanding)) {
        throw new BadRequestException(
          `Alokasi ke invoice ${invoiceId} (${allocatedHere}) melebihi sisa outstanding (${outstanding}).`,
        );
      }
    }

    const legs: LedgerLeg[] = receipt.instruments.map((instrument) => ({
      accountId: instrument.bankAccountId!,
      debit: new Prisma.Decimal(instrument.amount),
      credit: new Prisma.Decimal(0),
      description: instrument.notes ?? receipt.description,
      partnerId: receipt.partnerId,
    }));

    for (const allocation of draftAllocations) {
      const invoice = invoiceById.get(allocation.invoiceId)!;
      const receivableAccountId =
        invoice.receivableAccountId ?? (await this.customerReceivableAccount(tx, invoice.customerId));
      if (!receivableAccountId) {
        throw new BadRequestException(
          `Invoice ${allocation.invoiceId} tidak punya akun piutang (receivable account) di dokumen maupun master pelanggan.`,
        );
      }
      legs.push({
        accountId: receivableAccountId,
        debit: new Prisma.Decimal(0),
        credit: new Prisma.Decimal(allocation.amount),
        description: `Pelunasan invoice ${allocation.invoiceId}`,
        partnerId: receipt.partnerId,
      });
    }

    const base: LedgerBase = {
      branchId: receipt.branchId,
      locationId: receipt.locationId,
      sourceDocType: FIN_GL_DOCTYPE,
      sourceId: receipt.id,
      docNumber: receipt.docNumber,
      entryDate: receipt.transactionDate,
      fiscalPeriodId: receipt.fiscalPeriodId,
      currencyId: receipt.currencyId,
      exchangeRate: receipt.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: FIN_GL_SOURCE }));
    await assertLedgerRowsPeriodOpen(tx, rows);
    const created = await tx.erpFinLedgerEntry.createManyAndReturn({ data: rows });

    // Materialize allocation rows now that their ledger entries exist
    // (ledgerEntryId is NOT NULL — can't be created any earlier).
    const creditRows = created.filter((r) => new Prisma.Decimal(r.credit).gt(0));
    await tx.erpFinSettlementAllocation.createMany({
      data: draftAllocations.map((a, i) => ({
        arReceiptId: receipt.id,
        ledgerEntryId: creditRows[i].id,
        invoiceRef: a.invoiceId,
        amount: new Prisma.Decimal(a.amount),
        lineNo: a.lineNo,
      })),
    });

    await this.resettleInvoices(tx, invoiceIds);
  }

  async reverseLedger(tx: Prisma.TransactionClient, receiptId: bigint): Promise<void> {
    const allocations = await tx.erpFinSettlementAllocation.findMany({
      where: { arReceiptId: receiptId },
      select: { invoiceRef: true },
    });
    const invoiceIds = [...new Set(allocations.map((a) => BigInt(a.invoiceRef!)))];

    // Allocation rows must go before their ledger rows (FK), then the
    // metadata draft re-materializes them on the next POST.
    await tx.erpFinSettlementAllocation.deleteMany({ where: { arReceiptId: receiptId } });
    await reverseInvLedger(tx, FIN_GL_DOCTYPE, receiptId);

    await this.resettleInvoices(tx, invoiceIds);
  }

  readDraftAllocations(metadata: Prisma.JsonValue): DraftAllocation[] {
    if (!metadata || typeof metadata !== 'object' || Array.isArray(metadata)) return [];
    const raw = (metadata as Record<string, unknown>).draftAllocations;
    if (!Array.isArray(raw)) return [];
    return raw as DraftAllocation[];
  }

  private async resettleInvoices(tx: Prisma.TransactionClient, invoiceIds: bigint[]): Promise<void> {
    for (const invoiceId of invoiceIds) {
      const invoice = await tx.erpSlsInvoice.findUnique({
        where: { id: invoiceId },
        select: { grandTotal: true },
      });
      if (!invoice) continue;
      const sum = await tx.erpFinSettlementAllocation.aggregate({
        where: { invoiceRef: invoiceId.toString() },
        _sum: { amount: true },
      });
      const paid = new Prisma.Decimal(sum._sum.amount ?? 0);
      const total = new Prisma.Decimal(invoice.grandTotal);
      const settlementStatus = paid.lte(0) ? 'UNPAID' : paid.gte(total) ? 'PAID' : 'PARTIAL';
      await tx.erpSlsInvoice.update({
        where: { id: invoiceId },
        data: {
          settlementStatus,
          settledDate: settlementStatus === 'PAID' ? new Date() : null,
        },
      });
    }
  }

  private async customerReceivableAccount(
    tx: Prisma.TransactionClient,
    customerId: bigint | null,
  ): Promise<bigint | null> {
    if (!customerId) return null;
    const customer = await tx.erpPartner.findUnique({
      where: { id: customerId },
      select: { receivableAccountId: true },
    });
    return customer?.receivableAccountId ?? null;
  }
}
