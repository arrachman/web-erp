import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  INV_GL_DOCTYPE,
  buildLedgerRows,
  glPostingEnabled,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

type AdjustmentWithLines = Prisma.ErpInvStockAdjustmentGetPayload<{
  include: { lines: true };
}>;

/**
 * Stock posting for inventory adjustments.
 *
 * Stock balance in Senti is a DERIVED view (`inv_stock_balances`) computed from
 * POSTED transactions — there is no separate stock-ledger table to write into.
 * Therefore POST is a status flip: once status = POSTED the adjustment is counted
 * by the balance view.
 *
 * GL valuation is gated behind the `glPostingEnabled` inventory setting (default
 * OFF). When OFF the seam is a byte-for-byte no-op (status flip only, zero ledger
 * rows). When ON, each line books a balanced inventory/contra journal:
 *   - INCREASE ⇒ Dr inventory / Cr contra
 *   - DECREASE ⇒ Dr contra / Cr inventory
 */
@Injectable()
export class InvStockAdjustmentPostingService {
  /**
   * Validates the adjustment has at least one line. When GL posting is enabled,
   * also writes the balanced inventory/contra journal for each line.
   */
  async postAdjustment(
    tx: Prisma.TransactionClient,
    adjustment: AdjustmentWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!adjustment.lines.length) {
      throw new BadRequestException('Tidak bisa posting: belum ada baris item.');
    }
    // Helpers type their param as PrismaService; a TransactionClient exposes the
    // same `.erpSetting.findUnique` they use, so the cast is safe within `tx`.
    const db = tx as unknown as Parameters<typeof glPostingEnabled>[0];
    if (!(await glPostingEnabled(db))) {
      // GL posting OFF (default): no-op, balance is derived from POSTED status.
      return;
    }

    // Idempotent re-post: clear any prior ledger rows for this document first.
    await reverseInvLedger(tx, INV_GL_DOCTYPE.adjustment, adjustment.id);

    const zero = new Prisma.Decimal(0);
    const legs: LedgerLeg[] = [];

    for (const line of adjustment.lines) {
      const amount = new Prisma.Decimal(line.quantity).times(line.unitCost ?? 0);
      if (amount.lessThanOrEqualTo(0)) {
        // Skip zero/negative-value lines — they cannot form a posting leg.
        continue;
      }
      const isIncrease = line.direction === 'INCREASE';
      const debitAccountId = isIncrease ? line.inventoryAccountId : line.contraAccountId;
      const creditAccountId = isIncrease ? line.contraAccountId : line.inventoryAccountId;

      legs.push({
        accountId: debitAccountId,
        debit: amount,
        credit: zero,
        description: line.notes ?? adjustment.description,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });
      legs.push({
        accountId: creditAccountId,
        debit: zero,
        credit: amount,
        description: line.notes ?? adjustment.description,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });
    }

    const base: LedgerBase = {
      branchId: adjustment.branchId,
      locationId: null,
      sourceDocType: INV_GL_DOCTYPE.adjustment,
      sourceId: adjustment.id,
      docNumber: adjustment.docNumber,
      entryDate: adjustment.adjustmentDate,
      fiscalPeriodId: adjustment.fiscalPeriodId,
      // Adjustment header carries no currency — default to functional currency.
      currencyId: BigInt(1),
      exchangeRate: new Prisma.Decimal(1),
      actorId,
    };

    const rows = buildLedgerRows(base, legs);
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  /**
   * Remove this adjustment's posted inventory ledger rows (REOPEN / re-post).
   * Unconditional — safe even when nothing was posted (deleteMany of zero rows).
   */
  async reverseAdjustment(tx: Prisma.TransactionClient, adjustmentId: bigint): Promise<void> {
    await reverseInvLedger(tx, INV_GL_DOCTYPE.adjustment, adjustmentId);
  }
}
