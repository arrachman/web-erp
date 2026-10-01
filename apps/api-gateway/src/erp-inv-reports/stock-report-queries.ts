/**
 * Raw SQL aggregations for the stock reports. Stock balance is DERIVED (no
 * stock-ledger table): every query re-aggregates POSTED inv_stock_movement_lines
 * (signed by movement_type) UNION POSTED inv_opening_stock_lines, mirroring
 * InvMovingAverageCostService's sign convention:
 *   inbound  (TRANSFER_RECEIPT, RETURN) → +base_quantity
 *   outbound (ISSUE, TRANSFER)          → -base_quantity
 *   REQUEST                             → 0
 *   opening                             → +quantity
 * Filters: status='POSTED', deleted_at IS NULL.
 *
 * All Prisma.Decimal results are returned as-is (callers convert to number).
 */

import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

/** Movement-line warehouse filter on COALESCE(dest, source); empty when no wh. */
function movementWh(whId: bigint | null): Prisma.Sql {
  return whId
    ? Prisma.sql`AND COALESCE(l.destination_warehouse_id, l.source_warehouse_id) = ${whId}::bigint`
    : Prisma.empty;
}

function openingWh(whId: bigint | null): Prisma.Sql {
  return whId ? Prisma.sql`AND ol.warehouse_id = ${whId}::bigint` : Prisma.empty;
}

/** Signed-qty CASE expression shared by aggregations. */
const SIGNED_QTY = Prisma.sql`
  CASE m.movement_type
    WHEN 'TRANSFER_RECEIPT' THEN l.base_quantity
    WHEN 'RETURN'           THEN l.base_quantity
    WHEN 'ISSUE'            THEN -l.base_quantity
    WHEN 'TRANSFER'         THEN -l.base_quantity
    ELSE 0
  END`;

/**
 * Distinct item ids that appear in POSTED opening + movement lines, optionally
 * scoped to a warehouse. This is the "scope" item set for stock-style reports.
 */
export async function scopeItemIds(
  prisma: PrismaService,
  whId: bigint | null,
): Promise<bigint[]> {
  const rows = await prisma.$queryRaw<{ item_id: bigint }[]>(Prisma.sql`
    SELECT DISTINCT u.item_id FROM (
      SELECT l.item_id
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
        ${movementWh(whId)}
      UNION
      SELECT ol.item_id
      FROM inv_opening_stock_lines ol
      JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
      WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
        ${openingWh(whId)}
    ) u
  `);
  return rows.map((r) => r.item_id);
}

export interface LedgerRow {
  movementDate: Date;
  docNumber: string;
  refType: string;
  signedQty: Prisma.Decimal;
}

/**
 * Chronological POSTED ledger rows for a single item (opening rows first via
 * date, then movements). Caller computes the running balance. Optionally scoped
 * by warehouse and by an inclusive date range [from, to] (on the event date).
 */
export async function itemLedger(
  prisma: PrismaService,
  itemId: bigint,
  whId: bigint | null,
  from: Date | null,
  to: Date | null,
): Promise<LedgerRow[]> {
  const movDate = from || to ? dateRange('m.movement_date', from, to) : Prisma.empty;
  const opnDate = from || to ? dateRange('o.opening_date', from, to) : Prisma.empty;

  return prisma.$queryRaw<LedgerRow[]>(Prisma.sql`
    SELECT
      x.movement_date AS "movementDate",
      x.doc_number    AS "docNumber",
      x.ref_type      AS "refType",
      x.signed_qty    AS "signedQty",
      x.sort_key      AS sort_key
    FROM (
      SELECT
        m.movement_date AS movement_date,
        m.doc_number    AS doc_number,
        m.movement_type::text AS ref_type,
        ${SIGNED_QTY}   AS signed_qty,
        1               AS sort_key
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
        AND l.item_id = ${itemId}::bigint
        ${movementWh(whId)}
        ${movDate}
      UNION ALL
      SELECT
        o.opening_date AS movement_date,
        o.doc_number   AS doc_number,
        'Opening'      AS ref_type,
        ol.quantity    AS signed_qty,
        0              AS sort_key
      FROM inv_opening_stock_lines ol
      JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
      WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
        AND ol.item_id = ${itemId}::bigint
        ${openingWh(whId)}
        ${opnDate}
    ) x
    ORDER BY x.movement_date ASC, x.sort_key ASC, x.doc_number ASC
  `);
}

/** Inclusive date range fragment on a date column (handles open ends). */
function dateRange(col: string, from: Date | null, to: Date | null): Prisma.Sql {
  const c = Prisma.raw(col);
  if (from && to) return Prisma.sql`AND ${c} >= ${from}::date AND ${c} <= ${to}::date`;
  if (from) return Prisma.sql`AND ${c} >= ${from}::date`;
  if (to) return Prisma.sql`AND ${c} <= ${to}::date`;
  return Prisma.empty;
}

export interface MutationRow {
  itemId: bigint;
  openingQty: Prisma.Decimal | null;
  inQty: Prisma.Decimal | null;
  outQty: Prisma.Decimal | null;
}

/**
 * Per-item mutation buckets over [from, to): opening = net signed qty of POSTED
 * docs strictly BEFORE `from`; in = positive signed events within [from, to];
 * out = absolute of negative signed events within [from, to]. Warehouse-scoped.
 */
export async function mutationByItem(
  prisma: PrismaService,
  whId: bigint | null,
  from: Date,
  to: Date,
): Promise<MutationRow[]> {
  return prisma.$queryRaw<MutationRow[]>(Prisma.sql`
    SELECT
      u.item_id AS "itemId",
      SUM(CASE WHEN u.evt_date < ${from}::date THEN u.qty ELSE 0 END) AS "openingQty",
      SUM(CASE WHEN u.evt_date >= ${from}::date AND u.evt_date <= ${to}::date AND u.qty > 0 THEN u.qty ELSE 0 END) AS "inQty",
      SUM(CASE WHEN u.evt_date >= ${from}::date AND u.evt_date <= ${to}::date AND u.qty < 0 THEN -u.qty ELSE 0 END) AS "outQty"
    FROM (
      SELECT l.item_id AS item_id, m.movement_date AS evt_date, ${SIGNED_QTY} AS qty
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
        ${movementWh(whId)}
      UNION ALL
      SELECT ol.item_id AS item_id, o.opening_date AS evt_date, ol.quantity AS qty
      FROM inv_opening_stock_lines ol
      JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
      WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
        ${openingWh(whId)}
    ) u
    GROUP BY u.item_id
  `);
}

export interface DailyEventRow {
  evtDate: Date;
  qty: Prisma.Decimal;
}

/**
 * Net signed qty per event date for the daily-stock report. Optionally scoped by
 * warehouse and a single item. Caller buckets into a running closing balance per
 * day across the requested window (events before the window form the opening).
 */
export async function dailyEvents(
  prisma: PrismaService,
  whId: bigint | null,
  itemId: bigint | null,
): Promise<DailyEventRow[]> {
  const movItem = itemId ? Prisma.sql`AND l.item_id = ${itemId}::bigint` : Prisma.empty;
  const opnItem = itemId ? Prisma.sql`AND ol.item_id = ${itemId}::bigint` : Prisma.empty;

  return prisma.$queryRaw<DailyEventRow[]>(Prisma.sql`
    SELECT u.evt_date AS "evtDate", SUM(u.qty) AS qty
    FROM (
      SELECT m.movement_date AS evt_date, ${SIGNED_QTY} AS qty
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
        ${movementWh(whId)} ${movItem}
      UNION ALL
      SELECT o.opening_date AS evt_date, ol.quantity AS qty
      FROM inv_opening_stock_lines ol
      JOIN inv_opening_stocks o ON o.id = ol.opening_stock_id
      WHERE o.status = 'POSTED' AND o.deleted_at IS NULL
        ${openingWh(whId)} ${opnItem}
    ) u
    GROUP BY u.evt_date
    ORDER BY u.evt_date ASC
  `);
}

export interface RecalcLineRow {
  itemId: bigint;
  oldUnitCost: Prisma.Decimal;
  newUnitCost: Prisma.Decimal;
  affectedQty: Prisma.Decimal;
  deltaAmount: Prisma.Decimal;
  toDate: Date;
}

/**
 * Cost-recalculation lines from COMPLETED recalcs, optionally bounded by the
 * recalc from/to date window. One row per recalc line (item + warehouse), most
 * recent first. Soft-deleted recalcs excluded.
 */
export async function recalcLines(
  prisma: PrismaService,
  from: Date | null,
  to: Date | null,
): Promise<RecalcLineRow[]> {
  const dFrom = from ? Prisma.sql`AND r.from_date >= ${from}::date` : Prisma.empty;
  const dTo = to ? Prisma.sql`AND r.from_date <= ${to}::date` : Prisma.empty;

  return prisma.$queryRaw<RecalcLineRow[]>(Prisma.sql`
    SELECT
      rl.item_id       AS "itemId",
      rl.old_unit_cost AS "oldUnitCost",
      rl.new_unit_cost AS "newUnitCost",
      rl.affected_qty  AS "affectedQty",
      rl.delta_amount  AS "deltaAmount",
      r.from_date      AS "toDate"
    FROM inv_cost_recalculation_lines rl
    JOIN inv_cost_recalculations r ON r.id = rl.cost_recalculation_id
    WHERE r.deleted_at IS NULL
      AND r.status = 'COMPLETED'
      ${dFrom} ${dTo}
    ORDER BY r.from_date DESC, rl.item_id ASC
  `);
}
