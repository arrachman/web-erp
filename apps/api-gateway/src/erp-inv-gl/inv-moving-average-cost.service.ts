import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

/**
 * Weighted moving-average unit cost per item, derived on the fly.
 *
 * WHY DERIVED: this MVP has no stock-ledger / running-balance table, and the
 * `md_items.averageCost` column is currently UNMAINTAINED (no posting service
 * keeps it up to date). So the only trustworthy source of truth for valuation
 * is to re-aggregate the POSTED inventory documents themselves.
 *
 * Sources UNION'd:
 *   - POSTED inv_stock_movements lines (status='POSTED', not soft-deleted)
 *   - POSTED inv_opening_stocks lines  (status='POSTED', not soft-deleted)
 *
 * Sign convention by movement_type:
 *   - inbound  (TRANSFER_RECEIPT, RETURN) → +base_quantity
 *   - outbound (ISSUE, TRANSFER)          → -base_quantity
 *   - REQUEST                             → 0 (a request is not a stock event)
 *
 * NOTE on transfers: a TRANSFER (outbound, -qty) and its paired
 * TRANSFER_RECEIPT (inbound, +qty) net to zero company-wide when no
 * warehouse filter is applied, which is correct for a company-wide average.
 *
 * NULL-cost handling: movement lines whose unit_cost IS NULL are excluded from
 * BOTH the qty numerator and the value sum used for the average. Including them
 * in qty but not value would skew the average towards zero, so they are dropped
 * entirely from the average computation (opening lines always have a cost).
 *
 * Average = SUM(signed_qty * unit_cost) / NULLIF(SUM(signed_qty), 0), scaled to
 * 4 dp. Items with no positive net qty or no cost data are omitted from the map
 * so the caller can fall back to its own default.
 */
@Injectable()
export class InvMovingAverageCostService {
  constructor(private readonly prisma: PrismaService) {}

  /**
   * Weighted moving-average unit cost keyed by item id (as string).
   *
   * @param itemIds  items to compute for (empty → empty map).
   * @param warehouseId  optional warehouse scope. When given, movement lines are
   *   filtered by COALESCE(destination_warehouse_id, source_warehouse_id) and
   *   opening lines by warehouse_id. Kept simple/single-query; documented above.
   * @returns Map<itemIdString, Prisma.Decimal> rounded to 4 dp; items with no
   *   positive qty or no costed rows are omitted.
   */
  async averageCostByItem(
    itemIds: bigint[],
    warehouseId?: bigint | null,
  ): Promise<Map<string, Prisma.Decimal>> {
    const snap = await this.costAndQtyByItem(itemIds, warehouseId);
    const result = new Map<string, Prisma.Decimal>();
    for (const [id, v] of snap) result.set(id, v.avgCost);
    return result;
  }

  /**
   * On-hand qty AND weighted moving-average unit cost per item, in one pass.
   *
   * Same aggregation as {@link averageCostByItem} (POSTED movement lines signed
   * by movement_type + POSTED opening lines, UNION ALL), but returns BOTH the
   * summed on-hand qty and the weighted avgCost so callers (e.g. cost
   * recalculation) can compute delta = (newCost - oldCost) * qty.
   *
   * Items with net qty <= 0 or no costed rows are omitted from the map.
   *
   * @returns Map<itemIdString, { qty, avgCost }>; avgCost rounded to 4 dp.
   */
  async costAndQtyByItem(
    itemIds: bigint[],
    warehouseId?: bigint | null,
  ): Promise<Map<string, { qty: Prisma.Decimal; avgCost: Prisma.Decimal }>> {
    const result = new Map<string, { qty: Prisma.Decimal; avgCost: Prisma.Decimal }>();
    if (!itemIds.length) return result;

    const ids = Prisma.join(itemIds.map((id) => Prisma.sql`${id}::bigint`));
    const whId = warehouseId ?? null;

    // Movement-line warehouse filter: inbound/outbound both reference the line's
    // warehouse columns; COALESCE picks dest then source. Applied only when a
    // warehouse is requested.
    const movementWhFilter = whId
      ? Prisma.sql`AND COALESCE(l.destination_warehouse_id, l.source_warehouse_id) = ${whId}::bigint`
      : Prisma.empty;
    const openingWhFilter = whId
      ? Prisma.sql`AND ol.warehouse_id = ${whId}::bigint`
      : Prisma.empty;

    const rows = await this.prisma.$queryRaw<
      { item_id: bigint; total_value: Prisma.Decimal | null; total_qty: Prisma.Decimal | null }[]
    >(Prisma.sql`
      SELECT
        u.item_id AS item_id,
        SUM(u.value) AS total_value,
        SUM(u.qty)   AS total_qty
      FROM (
        SELECT
          l.item_id AS item_id,
          CASE m.movement_type
            WHEN 'TRANSFER_RECEIPT' THEN l.base_quantity
            WHEN 'RETURN'           THEN l.base_quantity
            WHEN 'ISSUE'            THEN -l.base_quantity
            WHEN 'TRANSFER'         THEN -l.base_quantity
            ELSE 0
          END AS qty,
          CASE m.movement_type
            WHEN 'TRANSFER_RECEIPT' THEN l.base_quantity
            WHEN 'RETURN'           THEN l.base_quantity
            WHEN 'ISSUE'            THEN -l.base_quantity
            WHEN 'TRANSFER'         THEN -l.base_quantity
            ELSE 0
          END * l.unit_cost AS value
        FROM inv_stock_movement_lines l
        JOIN inv_stock_movements m ON m.id = l.stock_movement_id
        WHERE m.status = 'POSTED'
          AND m.deleted_at IS NULL
          AND l.unit_cost IS NOT NULL
          AND l.item_id IN (${ids})
          ${movementWhFilter}

        UNION ALL

        SELECT
          ol.item_id AS item_id,
          ol.quantity AS qty,
          ol.quantity * ol.unit_cost AS value
        FROM inv_opening_stock_lines ol
        JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
        WHERE o.status = 'POSTED'
          AND o.deleted_at IS NULL
          AND ol.item_id IN (${ids})
          ${openingWhFilter}
      ) u
      GROUP BY u.item_id
    `);

    for (const row of rows) {
      if (row.total_qty == null || row.total_value == null) continue;
      const qty = new Prisma.Decimal(row.total_qty);
      if (qty.lessThanOrEqualTo(0)) continue;
      const value = new Prisma.Decimal(row.total_value);
      const avgCost = value.dividedBy(qty).toDecimalPlaces(4);
      result.set(row.item_id.toString(), { qty, avgCost });
    }

    return result;
  }
}
