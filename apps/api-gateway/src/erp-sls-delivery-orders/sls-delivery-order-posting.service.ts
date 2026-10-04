import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { InvStockMovementPostingService } from '../erp-inv-stock-movements/inv-stock-movement-posting.service';
import { InvLotAllocationService } from '../erp-inv-lots/inv-lot-allocation.service';

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
  constructor(
    private readonly invPosting: InvStockMovementPostingService,
    private readonly lots: InvLotAllocationService,
  ) {}

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
          create: await this.buildMovementLines(tx, deliveryOrder),
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

  /**
   * Fase 2 T1 — movement lines carry lots: an explicit line lotId wins;
   * otherwise FEFO auto-allocation over ACTIVE lots at the line warehouse
   * (the line is split when several lots are needed; any shortfall stays
   * lot-less). Lot balances are derived from these lines, so reverseLedger
   * deleting the movement restores them automatically.
   */
  private async buildMovementLines(
    tx: Prisma.TransactionClient,
    deliveryOrder: DeliveryOrderWithLines,
  ): Promise<Prisma.ErpInvStockMovementLineUncheckedCreateWithoutStockMovementInput[]> {
    const out: Prisma.ErpInvStockMovementLineUncheckedCreateWithoutStockMovementInput[] = [];
    let lineNo = 0;
    for (const l of deliveryOrder.lines) {
      const base = {
        itemId: l.itemId,
        unitId: l.unitId,
        unitValue: l.unitValue,
        baseUnitId: l.baseUnitId,
        unitCost: l.unitCost,
        sourceWarehouseId: l.warehouseId ?? deliveryOrder.warehouseId,
        costCenterId: l.costCenterId,
        divisionId: l.divisionId,
        subdivisionId: l.subdivisionId,
        projectId: l.projectId,
        notes: l.notes,
      };
      if (l.lotId || l.baseQuantity.lte(0)) {
        out.push({ ...base, quantity: l.quantity, baseQuantity: l.baseQuantity, lotId: l.lotId ?? null, lineNo: ++lineNo });
        continue;
      }
      const warehouseId = (l.warehouseId ?? deliveryOrder.warehouseId) as bigint;
      const plan = await this.lots.planFefo(tx, l.itemId, warehouseId, l.baseQuantity);
      if (!plan.allocations.length) {
        out.push({ ...base, quantity: l.quantity, baseQuantity: l.baseQuantity, lotId: null, lineNo: ++lineNo });
        continue;
      }
      const shortfall = new Prisma.Decimal(plan.shortfall);
      let usedQty = new Prisma.Decimal(0);
      plan.allocations.forEach((a, idx) => {
        const aBase = new Prisma.Decimal(a.quantity);
        const isLastCovering = idx === plan.allocations.length - 1 && shortfall.lte(0);
        const aQty = isLastCovering
          ? l.quantity.sub(usedQty)
          : l.quantity.mul(aBase).div(l.baseQuantity).toDecimalPlaces(4);
        out.push({ ...base, quantity: aQty, baseQuantity: aBase, lotId: BigInt(a.lotId), lineNo: ++lineNo });
        usedQty = usedQty.add(aQty);
      });
      if (shortfall.gt(0)) {
        out.push({ ...base, quantity: l.quantity.sub(usedQty), baseQuantity: shortfall, lotId: null, lineNo: ++lineNo });
      }
    }
    return out;
  }

  private async genMovementDocNumber(tx: Prisma.TransactionClient): Promise<string> {
    const numbering = await tx.erpDocumentNumbering.findFirst({
      where: { documentCode: MOVEMENT_DOC_CODE, deletedAt: null },
    });
    if (numbering) {
      // Atomic increment: row lock serializes concurrent saves (no duplicate / skipped numbers).
      const bumped = await tx.erpDocumentNumbering.update({
        where: { id: numbering.id },
        data: { nextNumber: { increment: 1 } },
        select: { nextNumber: true },
      });
      const seq = bumped.nextNumber - 1;
      return `${numbering.prefix}${String(seq).padStart(numbering.digitCount, '0')}`;
    }
    const count = await tx.erpInvStockMovement.count();
    return `${MOVEMENT_DOC_CODE}${String(count + 1).padStart(6, '0')}`;
  }
}
