import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import {
  INV_GL_DOCTYPE,
  buildLedgerRows,
  glPostingEnabled,
  readInvSetting,
  reverseInvLedger,
  type LedgerBase,
  type LedgerLeg,
} from '../erp-inv-gl/inv-gl-posting.helpers';

type OpeningStockWithLines = Prisma.ErpInvOpeningStockGetPayload<{
  include: { lines: true };
}>;

/**
 * Stock posting for inventory opening balances.
 *
 * Stock balance in Senti is a DERIVED view (`inv_stock_balances`) computed from
 * POSTED transactions — there is no separate stock-ledger table to write into.
 * Therefore POST is a status flip: once status = POSTED the opening stock is
 * counted by the balance view.
 *
 * GL valuation is gated behind the `glPostingEnabled` inventory setting (default
 * OFF). When OFF the seam is a byte-for-byte no-op (status flip only). When ON,
 * each line debits its inventory account and a single credit leg posts the total
 * to the opening-equity account (N debits + 1 balanced credit).
 */
@Injectable()
export class InvOpeningStockPostingService {
  /**
   * Validates the opening stock has at least one line. When GL posting is
   * enabled, also writes the balanced opening-balance journal.
   */
  async postOpeningStock(
    tx: Prisma.TransactionClient,
    openingStock: OpeningStockWithLines,
    actorId: bigint | null,
  ): Promise<void> {
    if (!openingStock.lines.length) {
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
    await reverseInvLedger(tx, INV_GL_DOCTYPE.opening, openingStock.id);

    const equityAccountId = await readInvSetting(db, 'defaultOpeningEquityAccountId');
    if (equityAccountId == null) {
      throw new BadRequestException(
        'Akun ekuitas pembukaan (defaultOpeningEquityAccountId) belum diset di Setting Inventory.',
      );
    }

    const zero = new Prisma.Decimal(0);
    const legs: LedgerLeg[] = [];
    let total = zero;

    for (const line of openingStock.lines) {
      const amount = new Prisma.Decimal(line.quantity).times(line.unitCost);
      if (amount.lessThanOrEqualTo(0)) {
        continue;
      }
      total = total.add(amount);
      legs.push({
        accountId: line.inventoryAccountId,
        debit: amount,
        credit: zero,
        description: line.notes ?? openingStock.description,
        costCenterId: line.costCenterId,
        divisionId: line.divisionId,
        subdivisionId: line.subdivisionId,
        projectId: line.projectId,
      });
    }

    // Single balancing credit to opening-equity for the summed inventory value.
    legs.push({
      accountId: equityAccountId,
      debit: zero,
      credit: total,
      description: openingStock.description ?? 'Ekuitas pembukaan persediaan',
    });

    const base: LedgerBase = {
      branchId: openingStock.branchId,
      locationId: openingStock.locationId,
      sourceDocType: INV_GL_DOCTYPE.opening,
      sourceId: openingStock.id,
      docNumber: openingStock.docNumber,
      entryDate: openingStock.openingDate,
      fiscalPeriodId: openingStock.fiscalPeriodId,
      currencyId: openingStock.currencyId,
      exchangeRate: openingStock.exchangeRate,
      actorId,
    };

    const rows = buildLedgerRows(base, legs);
    await tx.erpFinLedgerEntry.createMany({ data: rows });
  }

  /**
   * Remove this opening stock's posted inventory ledger rows (REOPEN / re-post).
   * Unconditional — safe even when nothing was posted.
   */
  async reverseOpeningStock(tx: Prisma.TransactionClient, openingStockId: bigint): Promise<void> {
    await reverseInvLedger(tx, INV_GL_DOCTYPE.opening, openingStockId);
  }
}
