/**
 * Stock aggregation report resolvers (group: stock). Computed from POSTED
 * movements UNION opening stock via the derived moving-average engine and
 * bespoke raw aggregations (stock-report-queries.ts) — there is no stock-ledger
 * table. Covers Stock (on-hand), Stock Card, Stock Mutation, Below Minimum,
 * Daily Available Stock, COGS Balance, Stock Minus, Consignment Summary.
 *
 * All Prisma.Decimal values are coerced to numbers; dates to ISO strings.
 * Reports whose underlying data does not exist yet (consignment) register their
 * columns and return empty rows with an explanatory summary note.
 */

import { ReportColumn, ReportDef, ReportFilters } from './report-types';
import { ReportDeps } from './report-deps';
import { buildStockReportsExtra } from './report-resolvers-stock-extra';
import { itemLedger, mutationByItem, scopeItemIds } from './stock-report-queries';
import {
  isoDate,
  itemNameMap,
  num,
  paginate,
  parseDate,
  parseId,
  warehouseNameMap,
} from './stock-report-helpers';

const COL = {
  itemCode: { key: 'itemCode', header: 'Kode Item', type: 'text' } as ReportColumn,
  itemName: { key: 'itemName', header: 'Nama Item', type: 'text' } as ReportColumn,
  warehouse: { key: 'warehouse', header: 'Gudang', type: 'text' } as ReportColumn,
};

export function buildStockReports(deps: ReportDeps): ReportDef[] {
  const { prisma, movingAvg } = deps;

  /** Stock (Saldo): on-hand qty + value per item, optionally warehouse-scoped. */
  const stock: ReportDef = {
    key: 'stock',
    title: 'Stock (Saldo)',
    group: 'stock',
    columns: [
      COL.itemCode,
      COL.itemName,
      COL.warehouse,
      { key: 'quantity', header: 'Kuantitas', type: 'qty' },
      { key: 'avgCost', header: 'Harga Rata2', type: 'money' },
      { key: 'stockValue', header: 'Nilai Stok', type: 'money' },
    ],
    async resolve(filters: ReportFilters) {
      const whId = parseId(filters.warehouseId);
      const ids = await scopeItemIds(prisma, whId);
      const snap = await movingAvg.costAndQtyByItem(ids, whId);
      const [items, whMap] = await Promise.all([
        itemNameMap(prisma, ids),
        whId ? warehouseNameMap(prisma, [whId]) : Promise.resolve(null),
      ]);
      const whLabel = whId ? whMap?.get(whId.toString())?.name ?? '' : '';

      const all = [...snap.entries()]
        .map(([id, v]) => {
          const it = items.get(id);
          const quantity = num(v.qty);
          const avgCost = num(v.avgCost);
          return {
            itemCode: it?.code ?? id,
            itemName: it?.name ?? '',
            warehouse: whLabel,
            quantity,
            avgCost,
            stockValue: quantity * avgCost,
          };
        })
        .sort((a, b) => a.itemCode.localeCompare(b.itemCode));

      const totalValue = all.reduce((s, r) => s + r.stockValue, 0);
      return {
        rows: paginate(all, filters),
        total: all.length,
        summary: [
          { label: 'Total Nilai', value: totalValue, type: 'money' as const },
          { label: 'Jumlah Item', value: all.length, type: 'number' as const },
        ],
      };
    },
  };

  /** Stock Card (Kartu Stok): per-item ledger with running balance. */
  const stockCards: ReportDef = {
    key: 'stock-cards',
    title: 'Stock Card (Kartu Stok)',
    group: 'stock',
    columns: [
      { key: 'date', header: 'Tanggal', type: 'date' },
      { key: 'docNumber', header: 'No. Dokumen', type: 'text' },
      { key: 'refType', header: 'Jenis', type: 'text' },
      { key: 'inQty', header: 'Masuk', type: 'qty' },
      { key: 'outQty', header: 'Keluar', type: 'qty' },
      { key: 'runningBalance', header: 'Saldo', type: 'qty' },
    ],
    async resolve(filters: ReportFilters) {
      const itemId = parseId(filters.itemId);
      if (!itemId) {
        return { rows: [], total: 0, summary: [{ label: 'Info', value: 'Pilih item', type: 'text' as const }] };
      }
      const whId = parseId(filters.warehouseId);
      const from = parseDate(filters.dateFrom);
      const to = parseDate(filters.dateTo);
      const ledger = await itemLedger(prisma, itemId, whId, from, to);

      let running = 0;
      let totalIn = 0;
      let totalOut = 0;
      let opening = 0;
      const rows = ledger.map((r, idx) => {
        const signed = num(r.signedQty);
        running += signed;
        const inQty = signed > 0 ? signed : 0;
        const outQty = signed < 0 ? -signed : 0;
        totalIn += inQty;
        totalOut += outQty;
        if (idx === 0 && r.refType === 'Opening') opening = signed;
        return {
          date: isoDate(r.movementDate),
          docNumber: r.docNumber,
          refType: r.refType,
          inQty,
          outQty,
          runningBalance: running,
        };
      });

      return {
        rows,
        total: rows.length,
        summary: [
          { label: 'Saldo Awal', value: opening, type: 'qty' as const },
          { label: 'Total Masuk', value: totalIn, type: 'qty' as const },
          { label: 'Total Keluar', value: totalOut, type: 'qty' as const },
          { label: 'Saldo Akhir', value: running, type: 'qty' as const },
        ],
      };
    },
  };

  /** Stock Mutation (Mutasi Stok): opening/in/out/closing per item over range. */
  const stockMutations: ReportDef = {
    key: 'stock-mutations',
    title: 'Stock Mutation (Mutasi Stok)',
    group: 'stock',
    columns: [
      COL.itemCode,
      COL.itemName,
      { key: 'openingQty', header: 'Saldo Awal', type: 'qty' },
      { key: 'inQty', header: 'Masuk', type: 'qty' },
      { key: 'outQty', header: 'Keluar', type: 'qty' },
      { key: 'closingQty', header: 'Saldo Akhir', type: 'qty' },
    ],
    async resolve(filters: ReportFilters) {
      const whId = parseId(filters.warehouseId);
      const to = parseDate(filters.dateTo) ?? new Date();
      const from = parseDate(filters.dateFrom) ?? new Date(to.getTime() - 30 * 86400000);
      const buckets = await mutationByItem(prisma, whId, from, to);
      const items = await itemNameMap(prisma, buckets.map((b) => b.itemId));

      const all = buckets
        .map((b) => {
          const it = items.get(b.itemId.toString());
          const openingQty = num(b.openingQty);
          const inQty = num(b.inQty);
          const outQty = num(b.outQty);
          return {
            itemCode: it?.code ?? b.itemId.toString(),
            itemName: it?.name ?? '',
            openingQty,
            inQty,
            outQty,
            closingQty: openingQty + inQty - outQty,
          };
        })
        .filter((r) => r.openingQty !== 0 || r.inQty !== 0 || r.outQty !== 0)
        .sort((a, b) => a.itemCode.localeCompare(b.itemCode));

      return {
        rows: paginate(all, filters),
        total: all.length,
        summary: [
          { label: 'Total Masuk', value: all.reduce((s, r) => s + r.inQty, 0), type: 'qty' as const },
          { label: 'Total Keluar', value: all.reduce((s, r) => s + r.outQty, 0), type: 'qty' as const },
          { label: 'Jumlah Item', value: all.length, type: 'number' as const },
        ],
      };
    },
  };

  /** Below Minimum Stock: on-hand < md_items.min_stock. */
  const belowMinimum: ReportDef = {
    key: 'below-minimum',
    title: 'Below Minimum Stock',
    group: 'stock',
    columns: [
      COL.itemCode,
      COL.itemName,
      COL.warehouse,
      { key: 'onHand', header: 'Stok', type: 'qty' },
      { key: 'minStock', header: 'Min. Stok', type: 'qty' },
      { key: 'shortage', header: 'Kekurangan', type: 'qty' },
    ],
    async resolve(filters: ReportFilters) {
      const whId = parseId(filters.warehouseId);
      const ids = await scopeItemIds(prisma, whId);
      const snap = await movingAvg.costAndQtyByItem(ids, whId);
      const [meta, whMap] = await Promise.all([
        prisma.erpItem.findMany({
          where: { id: { in: ids } },
          select: { id: true, code: true, name: true, minStock: true },
        }),
        whId ? warehouseNameMap(prisma, [whId]) : Promise.resolve(null),
      ]);
      const whLabel = whId ? whMap?.get(whId.toString())?.name ?? '' : '';

      const all = meta
        .map((it) => {
          const onHand = num(snap.get(it.id.toString())?.qty ?? null);
          const minStock = num(it.minStock);
          return {
            itemCode: it.code,
            itemName: it.name,
            warehouse: whLabel,
            onHand,
            minStock,
            shortage: minStock - onHand,
          };
        })
        .filter((r) => r.onHand < r.minStock)
        .sort((a, b) => b.shortage - a.shortage);

      return {
        rows: paginate(all, filters),
        total: all.length,
        summary: [{ label: 'Jumlah Item', value: all.length, type: 'number' as const }],
      };
    },
  };

  return [
    stock,
    stockCards,
    stockMutations,
    belowMinimum,
    ...buildStockReportsExtra(deps),
  ];
}
