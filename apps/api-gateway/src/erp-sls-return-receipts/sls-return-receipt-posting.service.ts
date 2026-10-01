import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { InvStockMovementPostingService } from '../erp-inv-stock-movements/inv-stock-movement-posting.service';

const SOURCE = 'SLS_RETURN_RECEIPT';
const SOURCE_DOC_TYPE = 'sls_return_receipts';
const MOVEMENT_DOC_CODE = 'RNRI';

type ReturnReceiptWithLines = Prisma.ErpSlsReturnReceiptGetPayload<{
  include: { lines: true };
}>;

/**
 * Stock posting for Return Receipts → inv_stock_movements (RETURN, goods in).
 *
 * A Return Receipt records the physical arrival of goods the customer sent
 * back (§2 posting matrix, DECISIONS.md "Aturan anti posting ganda": "Barang
 * retur dari pelanggan diposting oleh RNR"). Unlike GRN (a NEW purchase, no
 * COGS to reverse), RNR goods are reversing a prior DO's ISSUE — Dr
 * Inventory / Cr COGS is the textbook-correct direction, which is exactly
 * what InvStockMovementPostingService's RETURN valuation already does. So,
 * unlike GRN, this DOES delegate to postMovement for GL instead of writing a
 * separate journal.
 *
 * Mirrors SlsDeliveryOrderPostingService's pattern: create one
 * ErpInvStockMovement (POSTED), one line per RNR line, traceability kept in
 * metadata (no FK column exists for external document references on
 * inv_stock_movements).
 */
@Injectable()
export class SlsReturnReceiptPostingService {
  constructor(private readonly invPosting: InvStockMovementPostingService) {}

  async postToLedger(
    tx: Prisma.TransactionClient,
    returnReceipt: ReturnReceiptWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!returnReceipt.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    const missingWarehouse = returnReceipt.lines.find(
      (l) => !l.warehouseId && !returnReceipt.warehouseId,
    );
    if (missingWarehouse) {
      throw new BadRequestException(
        `Baris ${missingWarehouse.lineNo} tidak punya gudang (warehouse) — set di header atau baris.`,
      );
    }

    const docNumber = await this.genMovementDocNumber(tx);
    const movement = await tx.erpInvStockMovement.create({
      data: {
        docNumber,
        autoNumber: docNumber,
        movementType: 'RETURN',
        branchId: returnReceipt.branchId,
        locationId: returnReceipt.locationId,
        destinationWarehouseId: returnReceipt.warehouseId,
        source: SOURCE,
        movementDate: returnReceipt.docDate,
        fiscalPeriodId: returnReceipt.fiscalPeriodId,
        requestedPartnerId: returnReceipt.customerId,
        referenceNo: returnReceipt.docNumber,
        referenceDate: returnReceipt.docDate,
        description: `Barang masuk retur RNR ${returnReceipt.docNumber}`,
        status: 'POSTED',
        postingStatus: 'UNPOSTED',
        postedAt: new Date(),
        metadata: { sourceDocType: SOURCE_DOC_TYPE, sourceId: returnReceipt.id.toString() },
        createdById: actorId,
        updatedById: actorId,
        lines: {
          create: returnReceipt.lines.map((l, i) => ({
            itemId: l.itemId,
            quantity: l.quantity,
            unitId: l.unitId,
            unitValue: l.unitValue,
            baseQuantity: l.baseQuantity,
            baseUnitId: l.baseUnitId,
            unitCost: l.unitCost,
            destinationWarehouseId: l.warehouseId ?? returnReceipt.warehouseId,
            costCenterId: l.costCenterId,
            divisionId: l.divisionId,
            subdivisionId: l.subdivisionId,
            projectId: l.projectId,
            notes: l.notes,
            lineNo: i + 1,
          })),
        },
      },
      include: { lines: true },
    });

    await this.invPosting.postMovement(tx, movement, actorId);
    await tx.erpInvStockMovement.update({
      where: { id: movement.id },
      data: { postingStatus: 'POSTED' },
    });
  }

  async reverseLedger(tx: Prisma.TransactionClient, returnReceiptId: bigint): Promise<void> {
    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: SOURCE,
        deletedAt: null,
        metadata: { path: ['sourceId'], equals: returnReceiptId.toString() },
      },
    });
    for (const movement of movements) {
      await this.invPosting.reverseMovement(tx, movement.id);
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
