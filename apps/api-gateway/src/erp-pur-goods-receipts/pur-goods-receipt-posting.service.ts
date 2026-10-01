import { BadRequestException, Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';

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
 * GL posting for Goods Receipts → fin_ledger_entries.
 *
 * TODO (follow-up): a posted GRN SHOULD generate inventory accrual entries:
 *   Dr Inventory (inventoryAccountId) by acceptedQty × unitCost
 *   Cr GR/IR Accrual (accruedPayableAccountId)
 * Only acceptedQty increases stock (QC-gated). This double-entry posting is
 * deferred to a dedicated pass that wires `inv_*` stock movements.
 *
 * `postToLedger` writes no fin_ledger_entries yet (parity with PO/PR/PI), but it
 * DOES update each received item's cost basis (Harga Beli Terakhir + HPP
 * Terakhir) — the latest posted receipt is the source of truth for those fields.
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
    // Intentionally no fin_ledger_entries written yet — GRN GL posting is a follow-up.
  }

  /**
   * Stamp each received item with the latest purchase cost. Lines are applied in
   * order, so when the same item appears twice the last line wins (= most recent).
   * average_cost (HPP Rata-rata) is only seeded when still zero; a true moving
   * average belongs to the deferred inv stock-movement pass.
   */
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

  async reverseLedger(_tx: Prisma.TransactionClient, _grnId: bigint): Promise<void> {
    // No GL entries to remove — GRN posts none yet. Item cost stamps are NOT
    // reverted: they reflect "latest known purchase cost" and a reopen/repost
    // simply re-stamps with current line values.
  }
}
