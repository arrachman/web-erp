import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { InvMovingAverageCostService } from '../erp-inv-gl/inv-moving-average-cost.service';
import {
  INV_GL_DOCTYPE,
  buildLedgerRows,
  glPostingEnabled,
  readInvSetting,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

type MovementWithLines = Prisma.ErpInvStockMovementGetPayload<{
  include: { lines: true };
}>;

/**
 * Stock posting for inventory movements.
 *
 * Stock balance in Senti is a DERIVED view (`inv_stock_balances`) computed from
 * POSTED movements — there is no separate stock-ledger table to write into.
 * Therefore POST is a status flip: once status = POSTED the movement is counted
 * by the balance view.
 *
 * GL valuation is gated behind the `glPostingEnabled` inventory setting (default
 * OFF). When OFF the seam is a byte-for-byte no-op. When ON, only valued
 * goods-out/back movements post COGS/inventory:
 *   - ISSUE  ⇒ Dr COGS / Cr Inventory
 *   - RETURN ⇒ Dr Inventory / Cr COGS
 * TRANSFER / TRANSFER_RECEIPT / REQUEST post no GL (internal moves / demand).
 */
@Injectable()
export class InvStockMovementPostingService {
  constructor(private readonly movingAvg: InvMovingAverageCostService) {}

  /**
   * Validates the movement has at least one line. When GL posting is enabled and
   * the movement is a valued ISSUE/RETURN, also writes the COGS/inventory journal.
   */
  async postMovement(
    tx: Prisma.TransactionClient,
    movement: MovementWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!movement.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Helpers type their param as PrismaService; a TransactionClient exposes the
    // same `.erpSetting.findUnique` they use, so the cast is safe within `tx`.
    const db = tx as unknown as Parameters<typeof glPostingEnabled>[0];
    if (!(await glPostingEnabled(db))) {
      // GL posting OFF (default): no-op, balance is derived from POSTED status.
      return;
    }

    const isIssue = movement.movementType === 'ISSUE';
    const isReturn = movement.movementType === 'RETURN';
    if (!isIssue && !isReturn) {
      // TRANSFER / TRANSFER_RECEIPT / REQUEST: no GL valuation.
      return;
    }

    // Idempotent re-post: clear any prior ledger rows for this document first.
    await reverseInvLedger(tx, INV_GL_DOCTYPE.movement, movement.id);

    const itemIds = [...new Set(movement.lines.map((l) => l.itemId))];
    // ISSUE values goods leaving the source warehouse; RETURN goods arriving.
    const warehouseId = isIssue
      ? movement.sourceWarehouseId
      : movement.destinationWarehouseId;

    const [items, avgMap, fallbackCogs, fallbackInv] = await Promise.all([
      tx.erpItem.findMany({
        where: { id: { in: itemIds } },
        select: {
          id: true,
          inventoryAccountId: true,
          cogsAccountId: true,
          averageCost: true,
          standardCost: true,
        },
      }),
      this.movingAvg.averageCostByItem(itemIds, warehouseId),
      readInvSetting(db, 'defaultCogsAccountId'),
      readInvSetting(db, 'defaultInventoryAccountId'),
    ]);

    const itemMap = new Map(items.map((it) => [it.id.toString(), it]));
    const zero = new Prisma.Decimal(0);
    const legs: LedgerLeg[] = [];

    for (const line of movement.lines) {
      const key = line.itemId.toString();
      const item = itemMap.get(key);

      const cost =
        line.unitCost ??
        avgMap.get(key) ??
        item?.averageCost ??
        item?.standardCost ??
        null;
      if (cost == null) {
        throw new BadRequestException(
          `Tidak bisa posting: harga pokok untuk item ${line.itemId} tidak diketahui (unit_cost/avg/standard kosong).`,
        );
      }
      const amount = new Prisma.Decimal(line.baseQuantity).times(cost);
      if (amount.lessThanOrEqualTo(0)) {
        continue;
      }

      const cogsAccountId = item?.cogsAccountId ?? fallbackCogs;
      const invAccountId = item?.inventoryAccountId ?? fallbackInv;
      if (cogsAccountId == null) {
        throw new BadRequestException(
          `Tidak bisa posting: akun HPP (cogsAccountId) untuk item ${line.itemId} belum diset, dan defaultCogsAccountId belum diisi di Setting Inventory.`,
        );
      }
      if (invAccountId == null) {
        throw new BadRequestException(
          `Tidak bisa posting: akun persediaan (inventoryAccountId) untuk item ${line.itemId} belum diset, dan defaultInventoryAccountId belum diisi di Setting Inventory.`,
        );
      }

      const debitAccountId = isIssue ? cogsAccountId : invAccountId;
      const creditAccountId = isIssue ? invAccountId : cogsAccountId;

      legs.push({
        accountId: debitAccountId,
        debit: amount,
        credit: zero,
        description: line.notes ?? movement.description,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });
      legs.push({
        accountId: creditAccountId,
        debit: zero,
        credit: amount,
        description: line.notes ?? movement.description,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });
    }

    const base: LedgerBase = {
      branchId: movement.branchId,
      locationId: movement.locationId,
      sourceDocType: INV_GL_DOCTYPE.movement,
      sourceId: movement.id,
      docNumber: movement.docNumber,
      entryDate: movement.movementDate,
      fiscalPeriodId: movement.fiscalPeriodId,
      // Movement header carries no currency — default to functional currency.
      currencyId: BigInt(1),
      exchangeRate: new Prisma.Decimal(1),
      actorId,
    };

    const rows = buildLedgerRows(base, legs);
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  /**
   * Remove this movement's posted inventory ledger rows (REOPEN / re-post).
   * Unconditional — safe even when nothing was posted.
   */
  async reverseMovement(tx: Prisma.TransactionClient, movementId: bigint): Promise<void> {
    await reverseInvLedger(tx, INV_GL_DOCTYPE.movement, movementId);
  }
}
