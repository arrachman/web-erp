import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { InvStockMovementPostingService } from '../erp-inv-stock-movements/inv-stock-movement-posting.service';

const SOURCE = 'SLS_DELIVERY_ORDER';
const SOURCE_DOC_TYPE = 'sls_delivery_orders';
const MOVEMENT_DOC_CODE = 'DOI';

type DeliveryOrderWithLines = Prisma.ErpSlsDeliveryOrderGetPayload<{
  include: { lines: true };
}>;

/**
 * Stock posting for Delivery Orders → inv_stock_movements (ISSUE, goods out).
 *
 * A Delivery Order is the FIRST document in the sales chain to move physical
 * stock (§2 posting matrix, DECISIONS.md "Aturan anti posting ganda": "Barang
 * keluar ke pelanggan diposting oleh DO"). POST creates one ErpInvStockMovement
 * (movementType=ISSUE, already POSTED — stock balance is a derived view of
 * posted movements) with one line per DO line, then delegates GL valuation
 * (COGS/inventory, gated by the inventory glPostingEnabled setting) to
 * InvStockMovementPostingService — the same engine TS/MR/RF use. No duplicate
 * costing logic here.
 *
 * Traceability: the movement has no FK column back to its source document
 * (ErpInvStockMovement is a standalone transaction type), so the link is kept
 * in `metadata.sourceDocType`/`sourceId` — queried back on reverseLedger to
 * find and undo this DO's movement idempotently.
 */
@Injectable()
export class SlsDeliveryOrderPostingService {
  constructor(private readonly invPosting: InvStockMovementPostingService) {}

  async postToLedger(
    tx: Prisma.TransactionClient,
    deliveryOrder: DeliveryOrderWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!deliveryOrder.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    const missingWarehouse = deliveryOrder.lines.find(
      (l) => !l.warehouseId && !deliveryOrder.warehouseId,
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
        movementType: 'ISSUE',
        branchId: deliveryOrder.branchId,
        locationId: deliveryOrder.locationId,
        sourceWarehouseId: deliveryOrder.warehouseId,
        source: SOURCE,
        movementDate: deliveryOrder.docDate,
        fiscalPeriodId: deliveryOrder.fiscalPeriodId,
        requestedPartnerId: deliveryOrder.customerId,
        referenceNo: deliveryOrder.docNumber,
        referenceDate: deliveryOrder.docDate,
        description: `Barang keluar DO ${deliveryOrder.docNumber}`,
        status: 'POSTED',
        postingStatus: 'UNPOSTED',
        postedAt: new Date(),
        metadata: { sourceDocType: SOURCE_DOC_TYPE, sourceId: deliveryOrder.id.toString() },
        createdById: actorId,
        updatedById: actorId,
        lines: {
          create: deliveryOrder.lines.map((l, i) => ({
            itemId: l.itemId,
            quantity: l.quantity,
            unitId: l.unitId,
            unitValue: l.unitValue,
            baseQuantity: l.baseQuantity,
            baseUnitId: l.baseUnitId,
            unitCost: l.unitCost,
            sourceWarehouseId: l.warehouseId ?? deliveryOrder.warehouseId,
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

  async reverseLedger(tx: Prisma.TransactionClient, deliveryOrderId: bigint): Promise<void> {
    const movements = await tx.erpInvStockMovement.findMany({
      where: {
        source: SOURCE,
        deletedAt: null,
        metadata: { path: ['sourceId'], equals: deliveryOrderId.toString() },
      },
      include: { lines: true },
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
