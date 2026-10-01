import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const FIN_GL_SOURCE = 'FINANCE';
const FIN_GL_DOCTYPE = 'fin_ap_payments';

export interface DraftAllocation {
  invoiceId: string;
  amount: string;
  fxGainLossAmount?: string;
  termDiscountAmount?: string;
  lineNo: number;
}

type ApPaymentWithInstruments = Prisma.ErpFinApPaymentGetPayload<{
  include: { instruments: true };
}>;

/**
 * GL posting + invoice allocation for AP Payments (VP — Vendor Payment).
 *
 * Mirror image of ArReceiptPostingService (§2 posting matrix,
 * DECISIONS.md "Pola jurnal usulan" — VP row, FR-PUR-08):
 *   Dr  Utang Usaha (invoice.payableAccountId, fallback supplier) per allocation
 *   Dr  Selisih Kurs — rugi (header/allocation fxGainLossAccountId) when fxGainLossAmount < 0
 *   Cr  Selisih Kurs — laba when fxGainLossAmount > 0
 *   Cr  Potongan Termin (header/allocation termDiscountAccountId) per allocation, if any
 *   Cr  Kas/Bank per instrument (instrument.bankAccountId)
 *
 * Same NOT-NULL-FK constraint as AR Receipt: ErpFinSettlementAllocation.
 * ledgerEntryId can't exist before its ledger row does, so allocation intent
 * lives in `metadata.draftAllocations` until POST and is deleted (not just
 * unlinked) on REOPEN — see ArReceiptPostingService for the full rationale,
 * identical here.
 *
 * Balance identity enforced: instrumentTotal == allocationTotal - fxGainLossNet
 * - termDiscountNet (the AP amount actually extinguished, minus the parts
 * settled by FX/discount legs instead of cash).
 */
@Injectable()
export class ApPaymentPostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    payment: ApPaymentWithInstruments,
    actorId: bigint | null,
  ): Promise<void> {
    if (!payment.instruments.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada rincian cara bayar.');
    }
    const draftAllocations = this.readDraftAllocations(payment.metadata);
    if (!draftAllocations.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada alokasi ke invoice.');
    }

    const instrumentTotal = payment.instruments.reduce(
      (s, i) => s.add(new Prisma.Decimal(i.amount)),
      new Prisma.Decimal(0),
    );
    const allocationTotal = draftAllocations.reduce(
      (s, a) => s.add(new Prisma.Decimal(a.amount)),
      new Prisma.Decimal(0),
    );
    const fxNet = draftAllocations.reduce(
      (s, a) => s.add(new Prisma.Decimal(a.fxGainLossAmount ?? 0)),
      new Prisma.Decimal(0),
    );
    const termDiscountNet = draftAllocations.reduce(
      (s, a) => s.add(new Prisma.Decimal(a.termDiscountAmount ?? 0)),
      new Prisma.Decimal(0),
    );
    // Dr AP(allocationTotal) [+ Dr FX rugi] = [Cr FX laba +] Cr Discount + Cr Bank(instrumentTotal)
    // => instrumentTotal = allocationTotal - fxNet - termDiscountNet
    // (fxGainLossAmount > 0 = laba kurs → kas dibayar lebih kecil; < 0 = rugi → kas dibayar lebih besar.
    //  termDiscountAmount > 0 = potongan → kas dibayar lebih kecil.)
    const expectedInstrumentTotal = allocationTotal.sub(fxNet).sub(termDiscountNet);
    if (!instrumentTotal.equals(expectedInstrumentTotal)) {
      throw new BadRequestException(
        `Total cara bayar (${instrumentTotal}) tidak sama dengan alokasi (${allocationTotal}) dikurangi selisih kurs (${fxNet}) dan potongan termin (${termDiscountNet}) = ${expectedInstrumentTotal}.`,
      );
    }
    if (!instrumentTotal.equals(new Prisma.Decimal(payment.amount))) {
      throw new BadRequestException(
        `Total cara bayar (${instrumentTotal}) tidak sama dengan amount header (${payment.amount}).`,
      );
    }

    const invoiceIds = [...new Set(draftAllocations.map((a) => BigInt(a.invoiceId)))];
    const invoices = await tx.erpPurInvoice.findMany({
      where: { id: { in: invoiceIds }, deletedAt: null },
      select: { id: true, grandTotal: true, payableAccountId: true, supplierId: true },
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
        where: { invoiceRef: invoiceId.toString(), apPaymentId: { not: payment.id } },
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

    const legs: LedgerLeg[] = [];
    for (const allocation of draftAllocations) {
      const invoice = invoiceById.get(allocation.invoiceId)!;
      const payableAccountId =
        invoice.payableAccountId ?? (await this.supplierPayableAccount(tx, invoice.supplierId));
      if (!payableAccountId) {
        throw new BadRequestException(
          `Invoice ${allocation.invoiceId} tidak punya akun hutang (payable account) di dokumen maupun master vendor.`,
        );
      }
      legs.push({
        accountId: payableAccountId,
        debit: new Prisma.Decimal(allocation.amount),
        credit: new Prisma.Decimal(0),
        description: `Pelunasan invoice ${allocation.invoiceId}`,
        partnerId: payment.partnerId,
      });

      const fx = new Prisma.Decimal(allocation.fxGainLossAmount ?? 0);
      if (!fx.isZero()) {
        if (!payment.fxGainLossAccountId) {
          throw new BadRequestException(
            `Alokasi invoice ${allocation.invoiceId} punya selisih kurs tapi fxGainLossAccountId belum diset di header.`,
          );
        }
        legs.push({
          accountId: payment.fxGainLossAccountId,
          debit: fx.lt(0) ? fx.abs() : new Prisma.Decimal(0),
          credit: fx.gt(0) ? fx : new Prisma.Decimal(0),
          description: 'Selisih kurs',
        });
      }

      const discount = new Prisma.Decimal(allocation.termDiscountAmount ?? 0);
      if (discount.gt(0)) {
        if (!payment.termDiscountAccountId) {
          throw new BadRequestException(
            `Alokasi invoice ${allocation.invoiceId} punya potongan termin tapi termDiscountAccountId belum diset di header.`,
          );
        }
        legs.push({
          accountId: payment.termDiscountAccountId,
          debit: new Prisma.Decimal(0),
          credit: discount,
          description: 'Potongan termin',
        });
      }
    }

    for (const instrument of payment.instruments) {
      legs.push({
        accountId: instrument.bankAccountId!,
        debit: new Prisma.Decimal(0),
        credit: new Prisma.Decimal(instrument.amount),
        description: instrument.notes ?? payment.description,
        partnerId: payment.partnerId,
      });
    }

    const base: LedgerBase = {
      branchId: payment.branchId,
      locationId: payment.locationId,
      sourceDocType: FIN_GL_DOCTYPE,
      sourceId: payment.id,
      docNumber: payment.docNumber,
      entryDate: payment.transactionDate,
      fiscalPeriodId: payment.fiscalPeriodId,
      currencyId: payment.currencyId,
      exchangeRate: payment.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: FIN_GL_SOURCE }));
    const created = await tx.erpFinLedgerEntry.createManyAndReturn({ data: rows });

    // Materialize allocation rows now that their ledger entries exist — link
    // to each allocation's AP debit row specifically (first N debit rows in
    // leg-build order correspond 1:1 to draftAllocations).
    const apDebitRows = created.slice(0, draftAllocations.length);
    await tx.erpFinSettlementAllocation.createMany({
      data: draftAllocations.map((a, i) => ({
        apPaymentId: payment.id,
        ledgerEntryId: apDebitRows[i].id,
        invoiceRef: a.invoiceId,
        amount: new Prisma.Decimal(a.amount),
        lineNo: a.lineNo,
      })),
    });

    await this.resettleInvoices(tx, invoiceIds);
  }

  async reverseLedger(tx: Prisma.TransactionClient, paymentId: bigint): Promise<void> {
    const allocations = await tx.erpFinSettlementAllocation.findMany({
      where: { apPaymentId: paymentId },
      select: { invoiceRef: true },
    });
    const invoiceIds = [...new Set(allocations.map((a) => BigInt(a.invoiceRef!)))];

    await tx.erpFinSettlementAllocation.deleteMany({ where: { apPaymentId: paymentId } });
    await reverseInvLedger(tx, FIN_GL_DOCTYPE, paymentId);

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
      const invoice = await tx.erpPurInvoice.findUnique({
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
      await tx.erpPurInvoice.update({
        where: { id: invoiceId },
        data: {
          settlementStatus,
          settledDate: settlementStatus === 'PAID' ? new Date() : null,
        },
      });
    }
  }

  private async supplierPayableAccount(
    tx: Prisma.TransactionClient,
    supplierId: bigint | null,
  ): Promise<bigint | null> {
    if (!supplierId) return null;
    const supplier = await tx.erpPartner.findUnique({
      where: { id: supplierId },
      select: { payableAccountId: true },
    });
    return supplier?.payableAccountId ?? null;
  }
}
