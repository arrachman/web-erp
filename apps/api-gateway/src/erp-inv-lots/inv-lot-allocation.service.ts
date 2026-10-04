import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';

export interface LotBalanceRow {
  lotId: bigint;
  warehouseId: bigint | null;
  balance: Prisma.Decimal;
}

export interface FefoLot {
  lotId: string;
  lotNumber: string;
  expiryDate: string | null;
  createdAt: string;
  balance: string;
}

export interface FefoPlan {
  allocations: { lotId: string; lotNumber: string; expiryDate: string | null; quantity: string }[];
  shortfall: string;
  covered: string;
}

/**
 * Pure FEFO planner (unit-testable): only lots handed in (already filtered to
 * ACTIVE with positive balance) are used, ordered by earliest expiry first
 * (null expiry LAST), then oldest createdAt. Greedy fill.
 */
export function buildFefoPlan(lots: FefoLot[], quantity: Prisma.Decimal): FefoPlan {
  const sorted = [...lots].sort((a, b) => {
    if (a.expiryDate && b.expiryDate) {
      const d = a.expiryDate.localeCompare(b.expiryDate);
      if (d !== 0) return d;
    } else if (a.expiryDate && !b.expiryDate) return -1;
    else if (!a.expiryDate && b.expiryDate) return 1;
    return a.createdAt.localeCompare(b.createdAt);
  });
  const allocations: FefoPlan['allocations'] = [];
  let remaining = quantity;
  for (const lot of sorted) {
    if (remaining.lte(0)) break;
    const bal = new Prisma.Decimal(lot.balance);
    if (bal.lte(0)) continue;
    const take = Prisma.Decimal.min(bal, remaining);
    allocations.push({
      lotId: lot.lotId,
      lotNumber: lot.lotNumber,
      expiryDate: lot.expiryDate,
      quantity: take.toString(),
    });
    remaining = remaining.sub(take);
  }
  return {
    allocations,
    shortfall: remaining.toString(),
    covered: quantity.sub(remaining).toString(),
  };
}

/**
 * Fase 2 T1 — Lot balances are DERIVED from POSTED stock movement lines that
 * carry lot_id (same sign convention as the stock reports: TRANSFER_RECEIPT /
 * RETURN are +, ISSUE / TRANSFER are −, attributed to the destination /
 * source warehouse of the line). Nothing is stored, so reversals that delete
 * movements restore lot balances automatically.
 */
@Injectable()
export class InvLotAllocationService {
  constructor(private readonly prisma: PrismaService) {}

  async balances(
    client: Prisma.TransactionClient | PrismaService,
    filter: { itemId?: bigint; warehouseId?: bigint; lotId?: bigint } = {},
  ): Promise<LotBalanceRow[]> {
    const rows = await client.$queryRaw<
      { lot_id: bigint; warehouse_id: bigint | null; balance: string }[]
    >(Prisma.sql`
      SELECT l.lot_id AS lot_id,
             CASE WHEN m.movement_type IN ('TRANSFER_RECEIPT', 'RETURN')
                  THEN COALESCE(l.destination_warehouse_id, m.destination_warehouse_id)
                  ELSE COALESCE(l.source_warehouse_id, m.source_warehouse_id)
             END AS warehouse_id,
             SUM(CASE WHEN m.movement_type IN ('TRANSFER_RECEIPT', 'RETURN')
                      THEN l.base_quantity ELSE -l.base_quantity END)::text AS balance
      FROM inv_stock_movement_lines l
      JOIN inv_stock_movements m ON m.id = l.stock_movement_id
      WHERE m.status = 'POSTED' AND m.deleted_at IS NULL
        AND l.lot_id IS NOT NULL
        ${filter.itemId ? Prisma.sql`AND l.item_id = ${filter.itemId}::bigint` : Prisma.empty}
        ${filter.lotId ? Prisma.sql`AND l.lot_id = ${filter.lotId}::bigint` : Prisma.empty}
      GROUP BY l.lot_id, warehouse_id
    `);
    let out = rows.map((r) => ({
      lotId: r.lot_id,
      warehouseId: r.warehouse_id,
      balance: new Prisma.Decimal(r.balance ?? '0'),
    }));
    if (filter.warehouseId) {
      out = out.filter((r) => r.warehouseId?.toString() === filter.warehouseId!.toString());
    }
    return out;
  }

  /** Total balance of one lot across warehouses (optionally one warehouse). */
  async lotBalance(
    client: Prisma.TransactionClient | PrismaService,
    lotId: bigint,
    warehouseId?: bigint,
  ): Promise<Prisma.Decimal> {
    const rows = await this.balances(client, { lotId, warehouseId });
    return rows.reduce((s, r) => s.add(r.balance), new Prisma.Decimal(0));
  }

  /** FEFO plan for (item, warehouse, quantity) over ACTIVE lots with balance > 0. */
  async planFefo(
    client: Prisma.TransactionClient | PrismaService,
    itemId: bigint,
    warehouseId: bigint,
    quantity: Prisma.Decimal,
  ): Promise<FefoPlan> {
    const lots = await (client as PrismaService).erpInvLot.findMany({
      where: { itemId, status: 'ACTIVE', deletedAt: null },
      select: { id: true, lotNumber: true, expiryDate: true, createdAt: true },
    });
    if (!lots.length) return buildFefoPlan([], quantity);
    const bals = await this.balances(client, { itemId, warehouseId });
    const byLot = new Map<string, Prisma.Decimal>();
    for (const b of bals) {
      const key = b.lotId.toString();
      byLot.set(key, (byLot.get(key) ?? new Prisma.Decimal(0)).add(b.balance));
    }
    const fefoLots: FefoLot[] = lots
      .map((l) => ({
        lotId: l.id.toString(),
        lotNumber: l.lotNumber,
        expiryDate: l.expiryDate ? new Date(l.expiryDate).toISOString().slice(0, 10) : null,
        createdAt: new Date(l.createdAt).toISOString(),
        balance: (byLot.get(l.id.toString()) ?? new Prisma.Decimal(0)).toString(),
      }))
      .filter((l) => new Prisma.Decimal(l.balance).gt(0));
    return buildFefoPlan(fefoLots, quantity);
  }
}
