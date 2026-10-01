/**
 * NEED_APPROVE document counts across the existing warehouse (inv_*) header
 * tables. READ-ONLY. Each table is counted with a single parameterized query;
 * results keyed by docType. Soft-deleted rows excluded, optional branch filter.
 *
 * Note: the inv schema has no dedicated "price adjustment" header table — price
 * changes flow through cost recalculations (a COMPLETED-status workflow, not
 * NEED_APPROVE), so they are intentionally not part of the approval queue.
 */

import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

interface ApprovalDoc {
  docType: string;
  label: string;
  table: string;
}

export const APPROVAL_DOCS: ApprovalDoc[] = [
  { docType: 'STOCK_MOVEMENT', label: 'Stock Movement', table: 'inv_stock_movements' },
  { docType: 'OPENING_STOCK', label: 'Opening Stock', table: 'inv_opening_stocks' },
  { docType: 'STOCK_COUNT', label: 'Stock Count', table: 'inv_stock_counts' },
  { docType: 'STOCK_ADJUSTMENT', label: 'Stock Adjustment', table: 'inv_stock_adjustments' },
  { docType: 'DAILY_CHECK', label: 'Daily Check', table: 'inv_daily_checks' },
  { docType: 'WEIGHBRIDGE', label: 'Weighbridge Ticket', table: 'inv_weighbridge_tickets' },
];

/** Count NEED_APPROVE rows per table → { docType: count }. */
export async function countApprovals(
  prisma: PrismaService,
  branchId: bigint | null,
): Promise<Record<string, number>> {
  const branchFilter = branchId
    ? Prisma.sql`AND branch_id = ${branchId}::bigint`
    : Prisma.empty;

  const counts: Record<string, number> = {};
  await Promise.all(
    APPROVAL_DOCS.map(async (d) => {
      const rows = await prisma.$queryRaw<{ n: bigint }[]>(Prisma.sql`
        SELECT COUNT(*)::bigint AS n
        FROM ${Prisma.raw(d.table)}
        WHERE status = 'NEED_APPROVE' AND deleted_at IS NULL ${branchFilter}
      `);
      counts[d.docType] = Number(rows[0]?.n ?? 0);
    }),
  );
  return counts;
}
