/**
 * Second half of the stock report resolvers, split out to keep each file under
 * 400 lines. Covers Daily Available Stock, COGS Balance, Stock Minus, and the
 * placeholder Consignment Summary. Same derived-balance conventions as
 * report-resolvers-stock.ts.
 */

import { Prisma } from '@prisma/client';
import { ReportDef, ReportFilters, ReportSummaryItem } from './report-types';
import { ReportDeps } from './report-deps';
import {
  dailyEvents,
  mutationByItem,
  recalcLines,
  scopeItemIds,
} from './stock-report-queries';
import {
  isoDate,
  itemNameMap,
  num,
  paginate,
  parseDate,
  parseId,
  warehouseNameMap,
} from './stock-report-helpers';

const ITEM_CODE = { key: 'itemCode', header: 'Kode Item', type: 'text' } as const;
const ITEM_NAME = { key: 'itemName', header: 'Nama Item', type: 'text' } as const;
const WAREHOUSE = { key: 'warehouse', header: 'Gudang', type: 'text' } as const;
const DAY_MS = 86400000;

export function buildStockReportsExtra(deps: ReportDeps): ReportDef[] {
  const { prisma, movingAvg } = deps;

  /** Daily Available Stock: closing on-hand qty (+value) per day in window. */
  const dailyStock: ReportDef = {
    key: 'daily-stock',
    title: 'Daily Available Stock',
    group: 'stock',
    columns: [
      { key: 'date', header: 'Tanggal', type: 'date' },
      { key: 'quantity', header: 'Kuantitas', type: 'qty' },
      { key: 'value', header: 'Nilai', type: 'money' },
    ],
    async resolve(filters: ReportFilters) {
      const whId = parseId(filters.warehouseId);
      const itemId = parseId(filters.itemId);
      const to = parseDate(filters.dateTo) ?? new Date();
      let from = parseDate(filters.dateFrom) ?? new Date(to.getTime() - 30 * DAY_MS);
      const MAX_DAYS = 90;
      let capped = false;
      if ((to.getTime() - from.getTime()) / DAY_MS > MAX_DAYS) {
        from = new Date(to.getTime() - MAX_DAYS * DAY_MS);
        capped = true;
      }

      const events = await dailyEvents(prisma, whId, itemId);
      let unitCost = 0;
      if (itemId) {
        const m = await movingAvg.averageCostByItem([itemId], whId);
        unitCost = num(m.get(itemId.toString()) ?? null);
      }

      // Closing balance per event day; carry forward over the continuous series.
      const byDay = new Map<string, number>();
      let running = 0;
      for (const e of events) {
        running += num(e.qty);
        byDay.set(isoDate(e.evtDate), running);
      }

      const rows: Record<string, unknown>[] = [];
      const fromKey = isoDate(from);
      const toKey = isoDate(to);
      let bal = 0;
      for (const e of events) if (isoDate(e.evtDate) < fromKey) bal += num(e.qty);
      for (let d = new Date(from); isoDate(d) <= toKey; d = new Date(d.getTime() + DAY_MS)) {
        const key = isoDate(d);
        if (byDay.has(key)) bal = byDay.get(key)!;
        rows.push({ date: key, quantity: bal, value: itemId ? bal * unitCost : 0 });
      }

      const summary: ReportSummaryItem[] = [{ label: 'Hari', value: rows.length, type: 'number' }];
      if (capped) summary.push({ label: 'Catatan', value: `Rentang dibatasi ${MAX_DAYS} hari`, type: 'text' });
      if (!itemId) summary.push({ label: 'Catatan', value: 'Nilai 0: pilih item untuk valuasi', type: 'text' });
      return { rows: paginate(rows, filters), total: rows.length, summary };
    },
  };

  /** COGS Balance: cost-recalculation deltas (latest recalc per item in range). */
  const cogsBalance: ReportDef = {
    key: 'cogs-balance',
    title: 'COGS Balance',
    group: 'stock',
    columns: [
      ITEM_CODE,
      ITEM_NAME,
      { key: 'oldUnitCost', header: 'Harga Lama', type: 'money' },
      { key: 'newUnitCost', header: 'Harga Baru', type: 'money' },
      { key: 'affectedQty', header: 'Qty Terdampak', type: 'qty' },
      { key: 'deltaAmount', header: 'Selisih', type: 'money' },
    ],
    async resolve(filters: ReportFilters) {
      const from = parseDate(filters.dateFrom);
      const to = parseDate(filters.dateTo);
      const lines = await recalcLines(prisma, from, to);
      // Latest recalc per item (lines already ordered from_date DESC).
      const seen = new Set<string>();
      const latest = lines.filter((l) => {
        const k = l.itemId.toString();
        if (seen.has(k)) return false;
        seen.add(k);
        return true;
      });
      const items = await itemNameMap(prisma, latest.map((l) => l.itemId));

      const all = latest
        .map((l) => {
          const it = items.get(l.itemId.toString());
          return {
            itemCode: it?.code ?? l.itemId.toString(),
            itemName: it?.name ?? '',
            oldUnitCost: num(l.oldUnitCost),
            newUnitCost: num(l.newUnitCost),
            affectedQty: num(l.affectedQty),
            deltaAmount: num(l.deltaAmount),
          };
        })
        .sort((a, b) => a.itemCode.localeCompare(b.itemCode));

      const totalDelta = all.reduce((s, r) => s + r.deltaAmount, 0);
      const summary: ReportSummaryItem[] = [
        { label: 'Total Selisih', value: totalDelta, type: 'money' },
        { label: 'Jumlah Item', value: all.length, type: 'number' },
      ];
      if (!all.length) summary.push({ label: 'Catatan', value: 'Belum ada recalc COGS', type: 'text' });
      return { rows: paginate(all, filters), total: all.length, summary };
    },
  };

  /** Stock Minus: items with NEGATIVE on-hand (oversold/negative balance). */
  const stockMinus: ReportDef = {
    key: 'stock-minus',
    title: 'Stock Minus',
    group: 'stock',
    columns: [
      ITEM_CODE,
      ITEM_NAME,
      WAREHOUSE,
      { key: 'quantity', header: 'Kuantitas', type: 'qty' },
      { key: 'avgCost', header: 'Harga Rata2', type: 'money' },
      { key: 'value', header: 'Nilai', type: 'money' },
    ],
    async resolve(filters: ReportFilters) {
      const whId = parseId(filters.warehouseId);
      const ids = await scopeItemIds(prisma, whId);
      // costAndQtyByItem omits qty<=0 items, so derive true net qty via a wide
      // mutation window to surface negative balances.
      const buckets = await mutationByItem(
        prisma,
        whId,
        new Date('1970-01-01T00:00:00.000Z'),
        new Date(),
      );
      const [items, whMap, costMap] = await Promise.all([
        itemNameMap(prisma, ids),
        whId ? warehouseNameMap(prisma, [whId]) : Promise.resolve(null),
        avgCostFallback(deps, ids, whId),
      ]);
      const whLabel = whId ? whMap?.get(whId.toString())?.name ?? '' : '';

      const all = buckets
        .map((b) => ({
          itemId: b.itemId.toString(),
          net: num(b.openingQty) + num(b.inQty) - num(b.outQty),
        }))
        .filter((r) => r.net < 0)
        .map((r) => {
          const it = items.get(r.itemId);
          const avgCost = costMap.get(r.itemId) ?? 0;
          return {
            itemCode: it?.code ?? r.itemId,
            itemName: it?.name ?? '',
            warehouse: whLabel,
            quantity: r.net,
            avgCost,
            value: r.net * avgCost,
          };
        })
        .sort((a, b) => a.quantity - b.quantity);

      return {
        rows: paginate(all, filters),
        total: all.length,
        summary: [{ label: 'Jumlah Item', value: all.length, type: 'number' }],
      };
    },
  };

  /** Consignment Summary: no consignment document/model exists yet. */
  const consignment: ReportDef = {
    key: 'consignment',
    title: 'Consignment Summary',
    group: 'stock',
    columns: [
      { key: 'partner', header: 'Partner', type: 'text' },
      ITEM_CODE,
      ITEM_NAME,
      { key: 'quantity', header: 'Kuantitas', type: 'qty' },
      { key: 'value', header: 'Nilai', type: 'money' },
    ],
    async resolve(_filters: ReportFilters) {
      return {
        rows: [],
        total: 0,
        summary: [{ label: 'Catatan', value: 'belum ada data konsinyasi', type: 'text' }],
      };
    },
  };

  return [dailyStock, cogsBalance, stockMinus, consignment];
}

/**
 * Average cost per item, tolerant of items omitted by costAndQtyByItem (qty<=0).
 * Negative-stock items have no positive net qty, so the moving-avg engine drops
 * them; fall back to md_items.average_cost / standard_cost for valuation.
 */
async function avgCostFallback(
  deps: ReportDeps,
  ids: bigint[],
  whId: bigint | null,
): Promise<Map<string, number>> {
  const out = new Map<string, number>();
  const [snap, fallback] = await Promise.all([
    deps.movingAvg.costAndQtyByItem(ids, whId),
    deps.prisma.erpItem.findMany({
      where: { id: { in: ids } },
      select: { id: true, averageCost: true, standardCost: true },
    }),
  ]);
  for (const it of fallback) {
    const key = it.id.toString();
    const snapCost = snap.get(key)?.avgCost;
    const cost = snapCost
      ? snapCost.toNumber()
      : num(it.averageCost as Prisma.Decimal) || num(it.standardCost as Prisma.Decimal);
    out.set(key, cost);
  }
  return out;
}
