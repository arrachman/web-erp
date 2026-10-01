import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  buildLedgerRows,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';
import { assertLedgerRowsPeriodOpen } from '../erp-common/utils/ledger-period-guard';

const PUR_GL_SOURCE = 'PURCHASING';
const PUR_GL_DOCTYPE = 'pur_returns';
const MOVEMENT_SOURCE = 'PUR_RETURN';
const MOVEMENT_DOC_CODE = 'DNRI';

type ReturnWithLines = Prisma.ErpPurReturnGetPayload<{
  include: { lines: true };
}>;
type ReturnLine = ReturnWithLines['lines'][number];

const D = (v: Prisma.Decimal | number | null | undefined) => new Prisma.Decimal(v ?? 0);

function netUnitCost(line: ReturnLine): Prisma.Decimal {
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
 * Stock + GL posting for Purchase Returns (one model covers both PRD
 * concepts, distinguished by `returnType` — §2 posting matrix, DECISIONS.md
 * "Aturan anti posting ganda": "Barang retur ke vendor diposting oleh DNR.
 * Kalau PRT dibuat tanpa DNR, PRT yang memposting"; FR-PUR-06):
 *
 * - `RETURN_TO_VENDOR` (= DNR, physical goods leaving to the vendor): posts
 *   its own stock movement (ISSUE direction qty-wise, but — like GRN/PI — NOT
 *   routed through InvStockMovementPostingService.postMovement: that engine's
 *   ISSUE valuation is Dr COGS/Cr Inventory, which is wrong here. A purchase
 *   return reverses a purchase, not a sale, so GL is Dr Utang Usaha / Cr
 *   Persediaan, written manually) + the AP/inventory reversal journal below.
 * - `DEBIT_NOTE` (= PRT, pure financial debit note, no physical movement):
 *   GL only, no stock movement at all.
 *
 * GL (both types): Dr Utang Usaha (header payableAccountId, fallback
 * supplier) / Cr Persediaan or Retur Pembelian (line.inventoryAccountId for
 * RETURN_TO_VENDOR since it reverses inventory directly, else
 * item.purchaseReturnAccountId for DEBIT_NOTE since no physical stock moved)
 * / Cr PPN Masukan reversal per line.
 *
 * Direct/undirect application to a specific PI (FR-PUR-05: whether this
 * return reduces invoice.invoiceId's outstanding immediately vs becomes a
 * VPP-pooled credit) is OUT OF SCOPE here — that is outstanding-qty/amount
 * tracking, not yet built anywhere in this codebase (see DECISIONS.md § Plan
 * — Fase 0). This pass only posts the GL/stock movement.
 */
@Injectable()
export class PurReturnPostingService {
  async postToLedger(
    tx: Prisma.TransactionClient,
    purReturn: ReturnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!purReturn.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }

    if (purReturn.returnType === 'RETURN_TO_VENDOR') {
      await this.postStockMovement(tx, purReturn, actorId);
    }
    await this.postReversalLedger(tx, purReturn, actorId);
  }

  private async postStockMovement(
    tx: Prisma.TransactionClient,
    purReturn: ReturnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const missingWarehouse = purReturn.lines.find((l) => !l.warehouseId && !purReturn.warehouseId);
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
        movementType: 'ISSUE',
        branchId: purReturn.branchId,
        locationId: purReturn.locationId,
        sourceWarehouseId: purReturn.warehouseId,
        source: MOVEMENT_SOURCE,
        movementDate: purReturn.docDate,
        fiscalPeriodId: purReturn.fiscalPeriodId,
        requestedPartnerId: purReturn.supplierId,
        referenceNo: purReturn.docNumber,
        referenceDate: purReturn.docDate,
        description: `Barang keluar retur DNR ${purReturn.docNumber}`,
        status: 'POSTED',
        postingStatus: 'POSTED',
        postedAt: new Date(),
        metadata: { sourceDocType: PUR_GL_DOCTYPE, sourceId: purReturn.id.toString() },
        createdById: actorId,
        updatedById: actorId,
        lines: {
          create: purReturn.lines.map((l, i) => ({
            itemId: l.itemId,
            quantity: l.quantity,
            unitId: l.unitId,
            unitValue: l.unitValue,
            baseQuantity: l.baseQuantity,
            baseUnitId: l.baseUnitId,
            unitCost: netUnitCost(l),
            sourceWarehouseId: l.warehouseId ?? purReturn.warehouseId,
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
    // Deliberately NOT posted via InvStockMovementPostingService.postMovement:
    // its ISSUE valuation (Dr COGS/Cr Inventory) assumes a sale, not a
    // purchase reversal. GL for this document is written in
    // postReversalLedger below instead (same split as GRN/PI).
  }

  private async postReversalLedger(
    tx: Prisma.TransactionClient,
    purReturn: ReturnWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    const payableAccountId = await this.resolvePayableAccount(tx, purReturn);
    const taxAccountById = await this.purchaseTaxAccountMap(tx, purReturn.lines);

    const itemIds = [...new Set(purReturn.lines.map((l) => l.itemId))];
    const items =
      purReturn.returnType === 'DEBIT_NOTE'
        ? await tx.erpItem.findMany({
            where: { id: { in: itemIds } },
            select: { id: true, purchaseReturnAccountId: true },
          })
        : [];
    const itemById = new Map(items.map((i) => [i.id.toString(), i]));

    const legs: LedgerLeg[] = [];
    for (const line of purReturn.lines) {
      const creditAccountId =
        purReturn.returnType === 'RETURN_TO_VENDOR'
          ? (line.inventoryAccountId ?? purReturn.returnPurchaseAccountId)
          : (purReturn.returnPurchaseAccountId ?? itemById.get(line.itemId.toString())?.purchaseReturnAccountId);
      if (!creditAccountId) {
        throw new BadRequestException(
          `Baris ${line.lineNo} tidak punya akun persediaan/retur pembelian — set di baris, header (returnPurchaseAccountId), atau master item.`,
        );
      }
      const amount = D(line.quantity).mul(netUnitCost(line));
      if (amount.gt(0)) {
        legs.push({
          accountId: creditAccountId,
          debit: new Prisma.Decimal(0),
          credit: amount,
          description: line.notes,
          costCenterId: line.costCenterId,
          divisionId: line.divisionId,
          subdivisionId: line.subdivisionId,
          projectId: line.projectId,
        });
      }
      if (line.tax1Id && line.tax1Amount) {
        legs.push(this.taxLeg(line.tax1Id, line.tax1Amount, taxAccountById, purReturn.tax1AccountId));
      }
      if (line.tax2Id && line.tax2Amount) {
        legs.push(this.taxLeg(line.tax2Id, line.tax2Amount, taxAccountById, purReturn.tax2AccountId));
      }
    }

    legs.push({
      accountId: payableAccountId,
      debit: new Prisma.Decimal(purReturn.grandTotal),
      credit: new Prisma.Decimal(0),
      description: purReturn.description,
      partnerId: purReturn.supplierId,
    });

    if (!legs.length) return;
    const base: LedgerBase = {
      branchId: purReturn.branchId,
      locationId: purReturn.locationId,
      sourceDocType: PUR_GL_DOCTYPE,
      sourceId: purReturn.id,
      docNumber: purReturn.docNumber,
      entryDate: purReturn.docDate,
      fiscalPeriodId: purReturn.fiscalPeriodId,
      currencyId: purReturn.currencyId,
      exchangeRate: purReturn.exchangeRate,
      actorId,
    };
    const rows = buildLedgerRows(base, legs).map((r) => ({ ...r, source: PUR_GL_SOURCE }));
    await assertLedgerRowsPeriodOpen(tx, rows);
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  async reverseLedger(tx: Prisma.TransactionClient, returnId: bigint): Promise<void> {
    await reverseInvLedger(tx, PUR_GL_DOCTYPE, returnId);
    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: MOVEMENT_SOURCE,
        deletedAt: null,
        metadata: { path: ['sourceId'], equals: returnId.toString() },
      },
      select: { id: true },
    });
    for (const movement of movements) {
      await tx.erpInvStockMovementLine.deleteMany({ where: { stockMovementId: movement.id } });
      await tx.erpInvStockMovement.delete({ where: { id: movement.id } });
    }
  }

  private async resolvePayableAccount(
    tx: Prisma.TransactionClient,
    purReturn: ReturnWithLines,
  ): Promise<bigint> {
    if (purReturn.payableAccountId) return purReturn.payableAccountId;
    if (purReturn.supplierId) {
      const supplier = await tx.erpPartner.findUnique({
        where: { id: purReturn.supplierId },
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
    lines: ReturnLine[],
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
      debit: new Prisma.Decimal(0),
      credit: new Prisma.Decimal(amount),
      description: 'PPN Masukan (retur)',
    };
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
