/**
 * Raw SQL aggregations over EXISTING sls_invoices + sls_invoice_lines for the
 * warehouse statistics endpoints. READ-ONLY. No schema changes.
 *
 * Scope of "sold" lines (shared by all sales aggregations):
 *   - parent sls_invoices.status IN ('POSTED','APPROVED')
 *   - sls_invoices.is_opening_balance = false
 *   - sls_invoices.deleted_at IS NULL
 *   - optional doc_date range [dateFrom, dateTo], branch_id, line.warehouse_id
 *
 * Per-line money (sls_invoice_lines has no stored line-total column):
 *   revenue = quantity * unit_price - COALESCE(discount_amount, 0)
 *   cogs    = quantity * COALESCE(unit_cost, 0)   (unit_cost is captured per line)
 */

import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { StatsFilters } from './inv-stats.types';

/** WHERE fragments shared by every sales aggregation. */
function salesWhere(f: StatsFilters): Prisma.Sql {
  const parts: Prisma.Sql[] = [
    Prisma.sql`inv.status IN ('POSTED','APPROVED')`,
    Prisma.sql`inv.is_opening_balance = false`,
    Prisma.sql`inv.deleted_at IS NULL`,
  ];
  if (f.dateFrom) parts.push(Prisma.sql`inv.doc_date >= ${f.dateFrom}::date`);
  if (f.dateTo) parts.push(Prisma.sql`inv.doc_date <= ${f.dateTo}::date`);
  if (f.branchId) parts.push(Prisma.sql`inv.branch_id = ${f.branchId}::bigint`);
  if (f.warehouseId)
    parts.push(Prisma.sql`l.warehouse_id = ${f.warehouseId}::bigint`);
  return Prisma.join(parts, ' AND ');
}

const LINE_REVENUE = Prisma.sql`(l.quantity * l.unit_price - COALESCE(l.discount_amount, 0))`;
const LINE_COGS = Prisma.sql`(l.quantity * COALESCE(l.unit_cost, 0))`;

export interface ItemAggRow {
  item_id: bigint;
  code: string;
  name: string;
  revenue: Prisma.Decimal | null;
  qty: Prisma.Decimal | null;
  cogs: Prisma.Decimal | null;
  unit_name: string | null;
}

/**
 * Per-item aggregation of revenue, quantity and cogs over the sold-line scope.
 * Ordered by the requested metric, limited to top-N. base_unit name joined for
 * display. Returns [] gracefully when no sales exist.
 */
export async function itemAggregate(
  prisma: PrismaService,
  f: StatsFilters,
  orderBy: 'revenue' | 'qty',
): Promise<ItemAggRow[]> {
  const order =
    orderBy === 'revenue'
      ? Prisma.sql`SUM(${LINE_REVENUE}) DESC`
      : Prisma.sql`SUM(l.quantity) DESC`;
  return prisma.$queryRaw<ItemAggRow[]>(Prisma.sql`
    SELECT
      l.item_id                 AS item_id,
      it.code                   AS code,
      it.name                   AS name,
      SUM(${LINE_REVENUE})      AS revenue,
      SUM(l.quantity)           AS qty,
      SUM(${LINE_COGS})         AS cogs,
      u.name                    AS unit_name
    FROM sls_invoice_lines l
    JOIN sls_invoices inv ON inv.id = l.invoice_id
    JOIN md_items it      ON it.id = l.item_id
    LEFT JOIN md_units u  ON u.id = it.base_unit_id
    WHERE ${salesWhere(f)}
    GROUP BY l.item_id, it.code, it.name, u.name
    ORDER BY ${order}
    LIMIT ${f.limit}::int
  `);
}

export interface SalesTotals {
  revenue: Prisma.Decimal | null;
  qty: Prisma.Decimal | null;
}

/** Period-wide revenue + qty totals (no item grouping) for the KPI endpoint. */
export async function salesTotals(
  prisma: PrismaService,
  f: StatsFilters,
): Promise<SalesTotals> {
  const rows = await prisma.$queryRaw<SalesTotals[]>(Prisma.sql`
    SELECT SUM(${LINE_REVENUE}) AS revenue, SUM(l.quantity) AS qty
    FROM sls_invoice_lines l
    JOIN sls_invoices inv ON inv.id = l.invoice_id
    WHERE ${salesWhere(f)}
  `);
  return rows[0] ?? { revenue: null, qty: null };
}
