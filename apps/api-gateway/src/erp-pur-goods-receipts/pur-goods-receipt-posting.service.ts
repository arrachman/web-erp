import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

const PUR_GL_SOURCE = 'PURCHASING';
const PUR_GL_DOCTYPE = 'pur_goods_receipts';
const MOVEMENT_SOURCE = 'PUR_GOODS_RECEIPT';
const MOVEMENT_DOC_CODE = 'GRI';

type GrnWithLines = Prisma.ErpPurGoodsReceiptGetPayload<{ include: { lines: true } }>;
type GrnLine = GrnWithLines['lines'][number];

const D = (v: Prisma.Decimal | number | null | undefined) => new Prisma.Decimal(v ?? 0);

/**
 * Net unit landed cost for an item line = HPP Terakhir.
 * Gross unit price less per-unit discount (explicit amount wins over percent).
 * `unitCost` (if the receipt already carries a computed landed cost) takes
 * precedence over the derived value.
 */
function netUnitCost(line: GrnLine): Prisma.Decimal {
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
 * Stock + GL posting for Goods Receipts (§2 posting matrix, DECISIONS.md
 * "Aturan anti posting ganda": "Barang masuk dari vendor diposting oleh GRN").
 *
 * Only `acceptedQty` (QC-gated) increases stock — rejected/quarantine qty does
 * not move. POST does two things:
 *  1. Creates one ErpInvStockMovement (movementType=TRANSFER_RECEIPT, already
 *     POSTED — stock balance is a derived view) with one line per GRN line
 *     with acceptedQty > 0, so inv_stock_balances reflects the receipt. This
 *     type deliberately does NOT trigger InvStockMovementPostingService's
 *     auto-GL (its ISSUE/RETURN valuation assumes Dr COGS/Cr Inventory, wrong
 *     direction for a purchase — GRN needs Dr Inventory/Cr Accrual below).
 *  2. Writes the real GL accrual journal:
 *       Dr Inventory (line.inventoryAccountId, fallback item)   acceptedQty × netUnitCost
 *       Cr GR/IR Accrual (line.accruedPayableAccountId, fallback header payableAccountId)
 *     via the same buildLedgerRows/reverseInvLedger helpers SI/DO use.
 *
 * Also stamps each received item's cost basis (Harga Beli Terakhir + HPP
 * Terakhir), same as before this pass.
 */
@Injectable()
export class PurGoodsReceiptPostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    grn: GrnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!grn.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    await this.updateItemCosts(tx, grn, actorId);
    await this.postStockMovement(tx, grn, actorId);
    await this.postAccrualLedger(tx, grn, actorId);
  }

  private async updateItemCosts(
    tx: Prisma.TransactionClient,
    grn: GrnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    for (const line of grn.lines) {
      const lastHpp = netUnitCost(line);
      const current = await tx.erpItem.findUnique({
        where: { id: line.itemId },
        select: { averageCost: true },
      });
      if (!current) continue;
      await tx.erpItem.update({
        where: { id: line.itemId },
        data: {
          purchasePrice: D(line.unitPrice),
          lastHpp,
          ...(D(current.averageCost).isZero() ? { averageCost: lastHpp } : {}),
          ...(actorId ? { updatedById: actorId } : {}),
        },
      });
    }
  }

  private async postStockMovement(
    tx: Prisma.TransactionClient,
    grn: GrnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const acceptedLines = grn.lines.filter((l) => D(l.acceptedQty).gt(0));
    if (!acceptedLines.length) return;

    const missingWarehouse = acceptedLines.find((l) => !l.warehouseId && !grn.warehouseId);
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
        branchId: grn.branchId,
        locationId: grn.locationId,
        destinationWarehouseId: grn.warehouseId,
        source: MOVEMENT_SOURCE,
        movementDate: grn.docDate,
        fiscalPeriodId: grn.fiscalPeriodId,
        requestedPartnerId: grn.supplierId,
        referenceNo: grn.docNumber,
        referenceDate: grn.docDate,
        description: `Barang masuk GRN ${grn.docNumber}`,
        status: 'POSTED',
        postingStatus: 'POSTED',
        postedAt: new Date(),
        metadata: { sourceDocType: PUR_GL_DOCTYPE, sourceId: grn.id.toString() },
        createdById: actorId,
        updatedById: actorId,
        lines: {
          create: acceptedLines.map((l, i) => ({
            itemId: l.itemId,
            quantity: l.acceptedQty,
            unitId: l.unitId,
            unitValue: l.unitValue,
            baseQuantity: l.acceptedQty,
            baseUnitId: l.baseUnitId,
            unitCost: netUnitCost(l),
            destinationWarehouseId: l.warehouseId ?? grn.warehouseId,
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

  private async postAccrualLedger(
    tx: Prisma.TransactionClient,
    grn: GrnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const acceptedLines = grn.lines.filter((l) => D(l.acceptedQty).gt(0));
    if (!acceptedLines.length) return;

    const itemIds = [...new Set(acceptedLines.map((l) => l.itemId))];
    const items = await tx.erpItem.findMany({
      where: { id: { in: itemIds } },
      select: { id: true, inventoryAccountId: true },
    });
    const itemById = new Map(items.map((i) => [i.id.toString(), i]));

    const legs: LedgerLeg[] = [];
    for (const line of acceptedLines) {
      const item = itemById.get(line.itemId.toString());
      const inventoryAccountId = line.inventoryAccountId ?? item?.inventoryAccountId;
      const accrualAccountId = line.accruedPayableAccountId ?? grn.payableAccountId;
      if (!inventoryAccountId) {
        throw new BadRequestException(
          `Baris ${line.lineNo} tidak punya akun persediaan (inventory account) — set di baris atau master item.`,
        );
      }
      if (!accrualAccountId) {
        throw new BadRequestException(
          `Baris ${line.lineNo} tidak punya akun GR/IR Accrual — set di baris atau akun hutang di header.`,
        );
      }
      const amount = D(line.acceptedQty).mul(netUnitCost(line));
      if (amount.lte(0)) continue;

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
      legs.push({
        accountId: accrualAccountId,
        debit: new Prisma.Decimal(0),
        credit: amount,
        description: 'Barang Diterima Belum Ditagih',
        partnerId: grn.supplierId,
      });
    }
    if (!legs.length) return;

    const base: LedgerBase = {
      branchId: grn.branchId,
      locationId: grn.locationId,
      sourceDocType: PUR_GL_DOCTYPE,
      sourceId: grn.id,
      docNumber: grn.docNumber,
      entryDate: grn.docDate,
      fiscalPeriodId: grn.fiscalPeriodId,
      currencyId: grn.currencyId,
      exchangeRate: grn.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: PUR_GL_SOURCE }));
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, grnId: bigint): Promise<void> {
    await reverseInvLedger(tx, PUR_GL_DOCTYPE, grnId);

    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: MOVEMENT_SOURCE,
        deletedAt: null,
        metadata: { path: ['sourceId'], equals: grnId.toString() },
      },
      select: { id: true },
    });
    for (const movement of movements) {
      await tx.erpInvStockMovementLine.deleteMany({ where: { stockMovementId: movement.id } });
      await tx.erpInvStockMovement.delete({ where: { id: movement.id } });
    }
    // Item cost stamps are NOT reverted: they reflect "latest known purchase
    // cost" and a reopen/repost simply re-stamps with current line values.
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
