/**
 * Read-only Warehouse Statistics aggregations. Sales rankings come from
 * sls_invoices/lines (inv-stats.queries); below-minimum + stock value reuse the
 * derived on-hand/avg-cost engine (InvMovingAverageCostService) exactly as the
 * erp-inv-reports below-minimum resolver does. No schema changes, no writes.
 */

import { Injectable } from '@nestjs/common';
import { Prisma } from '@prisma/client';
import { PrismaService } from '../prisma/prisma.service';
import { InvMovingAverageCostService } from '../erp-inv-gl/inv-moving-average-cost.service';
import { scopeItemIds } from '../erp-inv-reports/stock-report-queries';
import { warehouseNameMap, num } from '../erp-inv-reports/stock-report-helpers';
import { itemAggregate, salesTotals } from './inv-stats.queries';
import { APPROVAL_DOCS, countApprovals } from './inv-stats.approvals';
import {
  BelowMinimumRow,
  BestSellingRow,
  KpiSummary,
  MostProfitableRow,
  StatsFilters,
  TopRevenueRow,
} from './inv-stats.types';

@Injectable()
export class InvStatsService {
  constructor(
    private readonly prisma: PrismaService,
    private readonly movingAvg: InvMovingAverageCostService,
  ) {}

  async topRevenue(f: StatsFilters): Promise<TopRevenueRow[]> {
    const rows = await itemAggregate(this.prisma, f, 'revenue');
    return rows.map((r) => ({
      itemId: r.item_id.toString(),
      itemCode: r.code,
      itemName: r.name,
      revenue: num(r.revenue),
    }));
  }

  async bestSelling(f: StatsFilters): Promise<BestSellingRow[]> {
    const rows = await itemAggregate(this.prisma, f, 'qty');
    return rows.map((r) => ({
      itemId: r.item_id.toString(),
      itemCode: r.code,
      itemName: r.name,
      qty: num(r.qty),
      unitName: r.unit_name ?? undefined,
    }));
  }

  async mostProfitable(f: StatsFilters): Promise<MostProfitableRow[]> {
    // COGS is sourced from sls_invoice_lines.unit_cost (captured per line at
    // posting). It is nullable per line; null contributes 0 to that line's cogs.
    const rows = await itemAggregate(this.prisma, f, 'revenue');
    const out = rows.map((r) => {
      const revenue = num(r.revenue);
      const cogs = num(r.cogs);
      const profit = revenue - cogs;
      const marginPct = revenue !== 0 ? (profit / revenue) * 100 : 0;
      return {
        itemId: r.item_id.toString(),
        itemCode: r.code,
        itemName: r.name,
        revenue,
        cogs,
        profit,
        marginPct: Math.round(marginPct * 100) / 100,
      };
    });
    return out.sort((a, b) => b.profit - a.profit);
  }

  async belowMinimum(f: StatsFilters): Promise<BelowMinimumRow[]> {
    const whId = f.warehouseId;
    const ids = await scopeItemIds(this.prisma, whId);
    if (ids.length === 0) return [];
    const [snap, meta, whMap] = await Promise.all([
      this.movingAvg.costAndQtyByItem(ids, whId),
      this.prisma.erpItem.findMany({
        where: { id: { in: ids } },
        select: { id: true, code: true, name: true, minStock: true },
      }),
      whId ? warehouseNameMap(this.prisma, [whId]) : Promise.resolve(null),
    ]);
    const whName = whId ? whMap?.get(whId.toString())?.name : undefined;

    return meta
      .map((it) => {
        const onHand = num(snap.get(it.id.toString())?.qty ?? null);
        const minQty = num(it.minStock);
        return {
          itemId: it.id.toString(),
          itemCode: it.code,
          itemName: it.name,
          warehouseName: whName,
          onHand,
          minQty,
          shortage: minQty - onHand,
        };
      })
      .filter((r) => r.onHand < r.minQty)
      .sort((a, b) => b.shortage - a.shortage);
  }

  /** NEED_APPROVE counts per warehouse doc type + total. */
  async approvals(f: StatsFilters) {
    const counts = await countApprovals(this.prisma, f.branchId);
    const data = APPROVAL_DOCS.map((d) => ({
      docType: d.docType,
      label: d.label,
      count: counts[d.docType] ?? 0,
    }));
    const total = data.reduce((s, r) => s + r.count, 0);
    return { data, total };
  }

  /** Derived stock value = SUM(onHand * avgCost) over the scoped item set. */
  private async stockValue(whId: bigint | null): Promise<number> {
    const ids = await scopeItemIds(this.prisma, whId);
    if (ids.length === 0) return 0;
    const snap = await this.movingAvg.costAndQtyByItem(ids, whId);
    let total = new Prisma.Decimal(0);
    for (const v of snap.values()) total = total.add(v.qty.mul(v.avgCost));
    return num(total);
  }

  async kpi(f: StatsFilters): Promise<KpiSummary> {
    const [totalItems, below, approvals, totals, stockValue] =
      await Promise.all([
        this.prisma.erpItem.count({ where: { deletedAt: null } }),
        this.belowMinimum(f),
        this.approvals(f),
        salesTotals(this.prisma, f),
        this.stockValue(f.warehouseId),
      ]);
    return {
      totalItems,
      belowMinCount: below.length,
      pendingApprovals: approvals.total,
      periodRevenue: num(totals.revenue),
      periodQtySold: num(totals.qty),
      stockValue,
    };
  }
}
