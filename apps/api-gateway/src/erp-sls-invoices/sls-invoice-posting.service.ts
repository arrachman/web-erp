import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const SLS_GL_SOURCE = 'SALES';
const SLS_GL_DOCTYPE = 'sls_invoices';

type InvoiceWithLines = Prisma.ErpSlsInvoiceGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Sales Invoices → fin_ledger_entries.
 *
 * SI is the primary AR event in the sales chain (§2 posting matrix,
 * DECISIONS.md "Aturan posting jurnal dan stok"):
 *   Dr  Piutang Usaha (header receivableAccountId, fallback customer)   grandTotal
 *   Cr  Penjualan per baris (item.salesAccountId, fallback kategori)    lineNet (per line)
 *   Cr  PPN Keluaran per baris (tax.saleAccountId, override header)     line.tax1Amount/tax2Amount
 *   Dr  Diskon Penjualan (header discountAccountId)                     discountAmount (bila ada)
 *
 * Mirrors CashBankPostingService / InvStockMovementPostingService: append-on-post,
 * reverseLedger hard-deletes this document's own rows before re-posting.
 */
@Injectable()
export class SlsInvoicePostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!invoice.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }

    const receivableAccountId = await this.resolveReceivableAccount(tx, invoice);
    const itemIds = [...new Set(invoice.lines.map((l) => l.itemId))];
    const items = await tx.erpItem.findMany({
      where: { id: { in: itemIds } },
      select: {
        id: true,
        salesAccountId: true,
        category: { select: { salesAccountId: true } },
      },
    });
    const itemById = new Map(items.map((i) => [i.id.toString(), i]));

    const taxIds = [
      ...new Set(
        invoice.lines.flatMap((l) => [l.tax1Id?.toString(), l.tax2Id?.toString()]).filter((v): v is string => !!v),
      ),
    ];
    const taxes = taxIds.length
      ? await tx.erpTax.findMany({
          where: { id: { in: taxIds.map(BigInt) } },
          select: { id: true, saleAccountId: true },
        })
      : [];
    const taxAccountById = new Map(taxes.map((t) => [t.id.toString(), t.saleAccountId]));

    const legs: LedgerLeg[] = [
      {
        accountId: receivableAccountId,
        debit: new Prisma.Decimal(invoice.grandTotal),
        credit: new Prisma.Decimal(0),
        description: invoice.description,
        partnerId: invoice.customerId,
      },
    ];

    for (const line of invoice.lines) {
      const item = itemById.get(line.itemId.toString());
      const salesAccountId = item?.salesAccountId ?? item?.category?.salesAccountId;
      if (!salesAccountId) {
        throw new BadRequestException(
          `Item pada baris ${line.lineNo} tidak punya akun penjualan (sales account) — set di master item atau kategori.`,
        );
      }
      const gross = new Prisma.Decimal(line.quantity).mul(new Prisma.Decimal(line.unitValue)).mul(new Prisma.Decimal(line.unitPrice));
      const discount = line.discountAmount
        ? new Prisma.Decimal(line.discountAmount)
        : line.discountPercent
          ? gross.mul(new Prisma.Decimal(line.discountPercent)).div(100)
          : new Prisma.Decimal(0);
      const net = gross.sub(discount);

      legs.push({
        accountId: salesAccountId,
        debit: new Prisma.Decimal(0),
        credit: net,
        description: line.notes,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });

      if (line.tax1Id && line.tax1Amount) {
        legs.push(this.taxLeg(invoice, line.tax1Id, line.tax1Amount, taxAccountById, invoice.tax1AccountId));
      }
      if (line.tax2Id && line.tax2Amount) {
        legs.push(this.taxLeg(invoice, line.tax2Id, line.tax2Amount, taxAccountById, invoice.tax2AccountId));
      }
    }

    if (invoice.discountAmount && new Prisma.Decimal(invoice.discountAmount).gt(0)) {
      if (!invoice.discountAccountId) {
        throw new BadRequestException('Ada diskon header tapi akun diskon (discountAccountId) belum diset.');
      }
      legs.push({
        accountId: invoice.discountAccountId,
        debit: new Prisma.Decimal(invoice.discountAmount),
        credit: new Prisma.Decimal(0),
        description: 'Diskon penjualan',
      });
    }

    const base: LedgerBase = {
      branchId: invoice.branchId,
      locationId: invoice.locationId,
      sourceDocType: SLS_GL_DOCTYPE,
      sourceId: invoice.id,
      docNumber: invoice.docNumber,
      entryDate: invoice.docDate,
      fiscalPeriodId: invoice.fiscalPeriodId,
      currencyId: invoice.currencyId,
      exchangeRate: invoice.exchangeRate,
      actorId,
    };

    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: SLS_GL_SOURCE }));
    const created = await tx.erpFinLedgerEntry.createMany({ data: rows });
    void created;

    const arRow = await tx.erpFinLedgerEntry.findFirst({
      where: { sourceDocType: SLS_GL_DOCTYPE, sourceId: invoice.id, accountId: receivableAccountId },
      orderBy: { id: 'asc' },
      select: { id: true },
    });
    await tx.erpSlsInvoice.update({
      where: { id: invoice.id },
      data: { arLedgerEntryId: arRow?.id ?? null },
    });
  }

  async reverseLedger(tx: Prisma.TransactionClient, invoiceId: bigint): Promise<void> {
    await reverseInvLedger(tx, SLS_GL_DOCTYPE, invoiceId);
    await tx.erpSlsInvoice.update({
      where: { id: invoiceId },
      data: { arLedgerEntryId: null },
    });
  }

  private async resolveReceivableAccount(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
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
      'Tidak bisa posting: akun piutang (receivable account) tidak ditemukan di dokumen maupun master pelanggan.',
    );
  }

  private taxLeg(
    invoice: InvoiceWithLines,
    taxId: bigint,
    amount: Prisma.Decimal,
    taxAccountById: Map<string, bigint | null>,
    headerOverrideAccountId: bigint | null,
  ): LedgerLeg {
    const accountId = headerOverrideAccountId ?? taxAccountById.get(taxId.toString());
    if (!accountId) {
      throw new BadRequestException('Pajak pada baris tidak punya akun PPN Keluaran — set di master pajak.');
    }
    return {
      accountId,
      debit: new Prisma.Decimal(0),
      credit: new Prisma.Decimal(amount),
      description: 'PPN Keluaran',
    };
  }
}
