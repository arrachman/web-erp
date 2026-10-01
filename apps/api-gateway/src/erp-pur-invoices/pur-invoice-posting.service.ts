import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const PUR_GL_SOURCE = 'PURCHASING';
const PUR_GL_DOCTYPE = 'pur_invoices';
const MOVEMENT_SOURCE = 'PUR_INVOICE';
const MOVEMENT_DOC_CODE = 'PII';

type InvoiceWithLines = Prisma.ErpPurInvoiceGetPayload<{
  include: { lines: true };
}>;
type InvoiceLine = InvoiceWithLines['lines'][number];

const D = (v: Prisma.Decimal | number | null | undefined) => new Prisma.Decimal(v ?? 0);

function netUnitCost(line: InvoiceLine): Prisma.Decimal {
  if (line.unitCost != null && !D(line.unitCost).isZero()) return D(line.unitCost);
  const gross = D(line.unitPrice);
  const qty = D(line.quantity);
  if (line.discountAmount != null && !D(line.discountAmount).isZero() && !qty.isZero()) {
    return gross.minus(D(line.discountAmount).dividedBy(qty));
  }
  if (line.discountPercent != null && !D(line.discountPercent).isZero()) {
    return gross.minus(gross.times(D(line.discountPercent)).dividedBy(100));
  }
  return gross;
}

/**
 * Stock + GL posting for Purchase Invoices (§2 posting matrix, DECISIONS.md
 * "Aturan anti posting ganda": "Barang masuk dari vendor diposting oleh GRN.
 * Kalau PI dibuat tanpa GRN, PI yang memposting" — FR-PUR-03/04).
 *
 * Two mutually exclusive paths, selected by whether `invoice.goodsReceiptId`
 * is set:
 *
 * PATH A — PI FROM a GRN (goods, and the accrual, already posted there):
 *   No new stock movement. GL is a pure reclassification from the GR/IR
 *   accrual to real AP:
 *     Dr  GR/IR Accrual (line.accruedPayableAccountId, fallback header payableAccountId)
 *     Dr  PPN Masukan (line tax → tax.purchaseAccountId, header override)
 *     Cr  Utang Usaha (header payableAccountId, fallback supplier.payableAccountId)
 *
 * PATH B — PI WITHOUT a GRN (stand-alone purchase, mirrors GRN's own posting):
 *   Creates its own stock movement (TRANSFER_RECEIPT, qty-only — same
 *   reasoning as GRN: ISSUE/RETURN auto-GL direction is wrong for a
 *   purchase) AND the full GL:
 *     Dr  Persediaan (line.inventoryAccountId, fallback item)
 *     Dr  PPN Masukan
 *     Cr  Utang Usaha
 *
 * Both paths reuse buildLedgerRows/reverseInvLedger (same helpers SI/SR/GRN
 * use).
 */
@Injectable()
export class PurInvoicePostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!invoice.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }

    if (invoice.goodsReceiptId) {
      await this.postAccrualReclass(tx, invoice, actorId);
    } else {
      await this.postStockMovement(tx, invoice, actorId);
      await this.postDirectPurchase(tx, invoice, actorId);
    }
  }

  // ── PATH A: from GRN — reclass accrual → AP ─────────────────────────────────
  private async postAccrualReclass(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const payableAccountId = await this.resolvePayableAccount(tx, invoice);
    const taxAccountById = await this.purchaseTaxAccountMap(tx, invoice.lines);

    const legs: LedgerLeg[] = [];
    for (const line of invoice.lines) {
      const accrualAccountId = line.accruedPayableAccountId;
      if (!accrualAccountId) {
        throw new BadRequestException(
          `Baris ${line.lineNo} tidak punya akun GR/IR Accrual — PI dari GRN wajib isi accruedPayableAccountId per baris (reklasifikasi dari accrual GRN).`,
        );
      }
      const amount = D(line.quantity).mul(netUnitCost(line));
      if (amount.gt(0)) {
        legs.push({
          accountId: accrualAccountId,
          debit: amount,
          credit: new Prisma.Decimal(0),
          description: line.notes,
          costCenterId: line.costCenterId,
          divisionId: line.divisionId,
          subdivisionId: line.subdivisionId,
          projectId: line.projectId,
        });
      }
      if (line.tax1Id && line.tax1Amount) {
        legs.push(this.taxLeg(line.tax1Id, line.tax1Amount, taxAccountById, invoice.tax1AccountId));
      }
      if (line.tax2Id && line.tax2Amount) {
        legs.push(this.taxLeg(line.tax2Id, line.tax2Amount, taxAccountById, invoice.tax2AccountId));
      }
    }

    legs.push({
      accountId: payableAccountId,
      debit: new Prisma.Decimal(0),
      credit: new Prisma.Decimal(invoice.grandTotal),
      description: invoice.description,
      partnerId: invoice.supplierId,
    });

    await this.writeLedger(tx, invoice, legs, actorId);
  }

  // ── PATH B: stand-alone — own stock + full AP ───────────────────────────────
  private async postStockMovement(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const missingWarehouse = invoice.lines.find((l) => !l.warehouseId && !invoice.warehouseId);
    if (missingWarehouse) {
      throw new BadRequestException(
        `Baris ${missingWarehouse.lineNo} tidak punya gudang (warehouse) — set di header atau baris.`,
      );
    }

    const docNumber = await this.genMovementDocNumber(tx);
    await tx.erpInvStockMovement.create({
      data: {
        docNumber,
        autoNumber: docNumber,
        movementType: 'TRANSFER_RECEIPT',
        branchId: invoice.branchId,
        locationId: invoice.locationId,
        destinationWarehouseId: invoice.warehouseId,
        source: MOVEMENT_SOURCE,
        movementDate: invoice.docDate,
        fiscalPeriodId: invoice.fiscalPeriodId,
        requestedPartnerId: invoice.supplierId,
        referenceNo: invoice.docNumber,
        referenceDate: invoice.docDate,
        description: `Barang masuk PI ${invoice.docNumber} (tanpa GRN)`,
        status: 'POSTED',
        postingStatus: 'POSTED',
        postedAt: new Date(),
        metadata: { sourceDocType: PUR_GL_DOCTYPE, sourceId: invoice.id.toString() },
        createdById: actorId,
        updatedById: actorId,
        lines: {
          create: invoice.lines.map((l, i) => ({
            itemId: l.itemId,
            quantity: l.quantity,
            unitId: l.unitId,
            unitValue: l.unitValue,
            baseQuantity: l.baseQuantity,
            baseUnitId: l.baseUnitId,
            unitCost: netUnitCost(l),
            destinationWarehouseId: l.warehouseId ?? invoice.warehouseId,
            costCenterId: l.costCenterId,
            divisionId: l.divisionId,
            subdivisionId: l.subdivisionId,
            projectId: l.projectId,
            notes: l.notes,
            lineNo: i + 1,
          })),
        },
      },
    });
  }

  private async postDirectPurchase(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const payableAccountId = await this.resolvePayableAccount(tx, invoice);
    const taxAccountById = await this.purchaseTaxAccountMap(tx, invoice.lines);

    const itemIds = [...new Set(invoice.lines.map((l) => l.itemId))];
    const items = await tx.erpItem.findMany({
      where: { id: { in: itemIds } },
      select: { id: true, inventoryAccountId: true },
    });
    const itemById = new Map(items.map((i) => [i.id.toString(), i]));

    const legs: LedgerLeg[] = [];
    for (const line of invoice.lines) {
      const item = itemById.get(line.itemId.toString());
      const inventoryAccountId = line.inventoryAccountId ?? item?.inventoryAccountId;
      if (!inventoryAccountId) {
        throw new BadRequestException(
          `Baris ${line.lineNo} tidak punya akun persediaan (inventory account) — set di baris atau master item.`,
        );
      }
      const amount = D(line.quantity).mul(netUnitCost(line));
      if (amount.gt(0)) {
        legs.push({
          accountId: inventoryAccountId,
          debit: amount,
          credit: new Prisma.Decimal(0),
          description: line.notes,
          costCenterId: line.costCenterId,
          divisionId: line.divisionId,
          subdivisionId: line.subdivisionId,
          projectId: line.projectId,
        });
      }
      if (line.tax1Id && line.tax1Amount) {
        legs.push(this.taxLeg(line.tax1Id, line.tax1Amount, taxAccountById, invoice.tax1AccountId));
      }
      if (line.tax2Id && line.tax2Amount) {
        legs.push(this.taxLeg(line.tax2Id, line.tax2Amount, taxAccountById, invoice.tax2AccountId));
      }
    }

    legs.push({
      accountId: payableAccountId,
      debit: new Prisma.Decimal(0),
      credit: new Prisma.Decimal(invoice.grandTotal),
      description: invoice.description,
      partnerId: invoice.supplierId,
    });

    await this.writeLedger(tx, invoice, legs, actorId);
  }

  // ── shared ───────────────────────────────────────────────────────────────────
  private async writeLedger(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
    legs: LedgerLeg[],
    actorId: bigint | null,
  ): Promise<void> {
    if (!legs.length) return;
    const base: LedgerBase = {
      branchId: invoice.branchId,
      locationId: invoice.locationId,
      sourceDocType: PUR_GL_DOCTYPE,
      sourceId: invoice.id,
      docNumber: invoice.docNumber,
      entryDate: invoice.docDate,
      fiscalPeriodId: invoice.fiscalPeriodId,
      currencyId: invoice.currencyId,
      exchangeRate: invoice.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: PUR_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  private async resolvePayableAccount(
    tx: Prisma.TransactionClient,
    invoice: InvoiceWithLines,
  ): Promise<bigint> {
    if (invoice.payableAccountId) return invoice.payableAccountId;
    if (invoice.supplierId) {
      const supplier = await tx.erpPartner.findUnique({
        where: { id: invoice.supplierId },
        select: { payableAccountId: true },
      });
      if (supplier?.payableAccountId) return supplier.payableAccountId;
    }
    throw new BadRequestException(
      'Tidak bisa posting: akun hutang (payable account) tidak ditemukan di dokumen maupun master vendor.',
    );
  }

  private async purchaseTaxAccountMap(
    tx: Prisma.TransactionClient,
    lines: InvoiceLine[],
  ): Promise<Map<string, bigint | null>> {
    const taxIds = [
      ...new Set(
        lines.flatMap((l) => [l.tax1Id?.toString(), l.tax2Id?.toString()]).filter((v): v is string => !!v),
      ),
    ];
    if (!taxIds.length) return new Map();
    const taxes = await tx.erpTax.findMany({
      where: { id: { in: taxIds.map(BigInt) } },
      select: { id: true, purchaseAccountId: true },
    });
    return new Map(taxes.map((t) => [t.id.toString(), t.purchaseAccountId]));
  }

  private taxLeg(
    taxId: bigint,
    amount: Prisma.Decimal,
    taxAccountById: Map<string, bigint | null>,
    headerOverrideAccountId: bigint | null,
  ): LedgerLeg {
    const accountId = headerOverrideAccountId ?? taxAccountById.get(taxId.toString());
    if (!accountId) {
      throw new BadRequestException('Pajak pada baris tidak punya akun PPN Masukan — set di master pajak.');
    }
    return {
      accountId,
      debit: new Prisma.Decimal(amount),
      credit: new Prisma.Decimal(0),
      description: 'PPN Masukan',
    };
  }

  async reverseLedger(tx: Prisma.TransactionClient, invoiceId: bigint): Promise<void> {
    await reverseInvLedger(tx, PUR_GL_DOCTYPE, invoiceId);
    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: MOVEMENT_SOURCE,
        deletedAt: null,
        metadata: { path: ['sourceId'], equals: invoiceId.toString() },
      },
      select: { id: true },
    });
    for (const movement of movements) {
      await tx.erpInvStockMovementLine.deleteMany({ where: { stockMovementId: movement.id } });
      await tx.erpInvStockMovement.delete({ where: { id: movement.id } });
    }
  }

  private async genMovementDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: MOVEMENT_DOC_CODE, deletedAt: null },
    });
    if (numbering) {
      const seq = numbering.nextNumber;
      await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: seq + 1 },
      });
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpInvStockMovement.count();
    return `${MOVEMENT_DOC_CODE}${String(count + 1).padStart(6, '0')}`;
  }
}
