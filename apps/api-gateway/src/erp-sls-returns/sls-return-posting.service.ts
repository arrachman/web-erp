import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const SLS_GL_SOURCE = 'SALES';
const SLS_GL_DOCTYPE = 'sls_returns';

type ReturnWithLines = Prisma.ErpSlsReturnGetPayload<{
  include: { lines: true };
}>;

/**
 * GL posting for Sales Returns → fin_ledger_entries.
 *
 * SR is the AR credit-note event in the sales chain (§2 posting matrix,
 * DECISIONS.md "Pola jurnal usulan" — mirror image of SI):
 *   Dr  Retur Penjualan per baris (item.salesReturnAccountId, fallback kategori) lineNet
 *   Dr  PPN Keluaran per baris (tax.saleAccountId, override header)              line.tax1Amount/tax2Amount
 *   Cr  Piutang Usaha (header receivableAccountId, fallback customer)           grandTotal
 *
 * Reduces AR rather than creating it — exact debit/credit mirror of
 * SlsInvoicePostingService. Mirrors CashBankPostingService append-on-post /
 * reverseLedger hard-delete pattern.
 */
@Injectable()
export class SlsReturnPostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    slsReturn: ReturnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!slsReturn.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }

    const receivableAccountId = await this.resolveReceivableAccount(tx, slsReturn);
    const itemIds = [...new Set(slsReturn.lines.map((l) => l.itemId))];
    const items = await tx.erpItem.findMany({
      where: { id: { in: itemIds } },
      select: {
        id: true,
        salesReturnAccountId: true,
        category: { select: { salesAccountId: true } },
      },
    });
    const itemById = new Map(items.map((i) => [i.id.toString(), i]));

    const taxIds = [
      ...new Set(
        slsReturn.lines
          .flatMap((l) => [l.tax1Id?.toString(), l.tax2Id?.toString()])
          .filter((v): v is string => !!v),
      ),
    ];
    const taxes = taxIds.length
      ? await tx.erpTax.findMany({
          where: { id: { in: taxIds.map(BigInt) } },
          select: { id: true, saleAccountId: true },
        })
      : [];
    const taxAccountById = new Map(taxes.map((t) => [t.id.toString(), t.saleAccountId]));

    const legs: LedgerLeg[] = [];

    for (const line of slsReturn.lines) {
      const item = itemById.get(line.itemId.toString());
      const returnAccountId = item?.salesReturnAccountId ?? item?.category?.salesAccountId;
      if (!returnAccountId) {
        throw new BadRequestException(
          `Item pada baris ${line.lineNo} tidak punya akun retur penjualan (sales return account) — set di master item atau kategori.`,
        );
      }
      const gross = new Prisma.Decimal(line.quantity)
        .mul(new Prisma.Decimal(line.unitValue))
        .mul(new Prisma.Decimal(line.unitPrice));
      const discount = line.discountAmount
        ? new Prisma.Decimal(line.discountAmount)
        : line.discountPercent
          ? gross.mul(new Prisma.Decimal(line.discountPercent)).div(100)
          : new Prisma.Decimal(0);
      const net = gross.sub(discount);

      legs.push({
        accountId: returnAccountId,
        debit: net,
        credit: new Prisma.Decimal(0),
        description: line.notes,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });

      if (line.tax1Id && line.tax1Amount) {
        legs.push(this.taxLeg(line.tax1Id, line.tax1Amount, taxAccountById, slsReturn.tax1AccountId));
      }
      if (line.tax2Id && line.tax2Amount) {
        legs.push(this.taxLeg(line.tax2Id, line.tax2Amount, taxAccountById, slsReturn.tax2AccountId));
      }
    }

    legs.push({
      accountId: receivableAccountId,
      debit: new Prisma.Decimal(0),
      credit: new Prisma.Decimal(slsReturn.grandTotal),
      description: slsReturn.description,
      partnerId: slsReturn.customerId,
    });

    const base: LedgerBase = {
      branchId: slsReturn.branchId,
      locationId: slsReturn.locationId,
      sourceDocType: SLS_GL_DOCTYPE,
      sourceId: slsReturn.id,
      docNumber: slsReturn.docNumber,
      entryDate: slsReturn.docDate,
      fiscalPeriodId: slsReturn.fiscalPeriodId,
      currencyId: slsReturn.currencyId,
      exchangeRate: slsReturn.exchangeRate,
      actorId,
    };

    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: SLS_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, returnId: bigint): Promise<void> {
    await reverseInvLedger(tx, SLS_GL_DOCTYPE, returnId);
  }

  private async resolveReceivableAccount(
    tx: Prisma.TransactionClient,
    slsReturn: ReturnWithLines,
  ): Promise<bigint> {
    if (slsReturn.receivableAccountId) return slsReturn.receivableAccountId;
    if (slsReturn.customerId) {
      const customer = await tx.erpPartner.findUnique({
        where: { id: slsReturn.customerId },
        select: { receivableAccountId: true },
      });
      if (customer?.receivableAccountId) return customer.receivableAccountId;
    }
    throw new BadRequestException(
      'Tidak bisa posting: akun piutang (receivable account) tidak ditemukan di dokumen maupun master pelanggan.',
    );
  }

  private taxLeg(
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
      debit: new Prisma.Decimal(amount),
      credit: new Prisma.Decimal(0),
      description: 'PPN Keluaran (retur)',
    };
  }
}
