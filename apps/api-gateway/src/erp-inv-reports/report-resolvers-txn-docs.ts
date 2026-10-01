/**
 * Document-level transaction + item report builders split out of
 * `report-resolvers-txn.ts` to keep each file under the 400-line cap.
 * Covers SP/SA/PA/IB/DC/RW header docs plus batch/serial direct-query reports.
 * All queries are read-only and Decimal-safe.
 */

import { ReportDef, ReportSummaryItem } from './report-types';
import { ReportDeps } from './report-deps';
import {
  DAILY_CHECK_COLUMNS,
  OPENING_STOCK_COLUMNS,
  PRICE_ADJUSTMENT_COLUMNS,
  SOFT_DELETE,
  STOCK_ADJUSTMENT_COLUMNS,
  STOCK_COUNT_COLUMNS,
  WEIGHBRIDGE_COLUMNS,
  baseWhere,
  dateRange,
  display,
  isoDate,
  num,
  paginate,
  resolveNames,
  searchWhere,
  statusWhere,
} from './report-resolvers-txn-cols';

/* ========================================================= STOCK COUNTS */

export function buildStockCountReport(deps: ReportDeps): ReportDef {
  return {
    key: 'stock-counts',
    title: 'Stock Count (SP)',
    group: 'transaction',
    columns: STOCK_COUNT_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);
      const where = baseWhere('countDate', filters);

      const [total, rows] = await Promise.all([
        prisma.erpInvStockCount.count({ where }),
        prisma.erpInvStockCount.findMany({
          where,
          orderBy: { countDate: 'desc' },
          skip,
          take,
          include: { lines: { select: { varianceQty: true } } },
        }),
      ]);

      const names = await resolveNames(prisma, { warehouseIds: rows.map((r) => r.warehouseId) });

      const out = rows.map((r) => ({
        docNumber: r.docNumber,
        countDate: isoDate(r.countDate),
        warehouse: display(names.ref(names.warehouse, r.warehouseId)),
        status: r.status,
        lineCount: r.lines.length,
        totalVarianceQty: r.lines.reduce((s, l) => s + num(l.varianceQty), 0),
      }));

      const summary: ReportSummaryItem[] = [{ label: 'Jumlah Dokumen', value: total, type: 'number' }];
      return { rows: out, summary, total };
    },
  };
}

/* ==================================================== STOCK ADJUSTMENTS */

export function buildStockAdjustmentReport(deps: ReportDeps): ReportDef {
  return {
    key: 'stock-adjustments',
    title: 'Stock Adjustment (SA)',
    group: 'transaction',
    columns: STOCK_ADJUSTMENT_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);
      const where = baseWhere('adjustmentDate', filters);

      const [total, rows] = await Promise.all([
        prisma.erpInvStockAdjustment.count({ where }),
        prisma.erpInvStockAdjustment.findMany({
          where,
          orderBy: { adjustmentDate: 'desc' },
          skip,
          take,
          include: { lines: { select: { quantity: true, unitCost: true } } },
        }),
      ]);

      const names = await resolveNames(prisma, { warehouseIds: rows.map((r) => r.warehouseId) });

      let grandValue = 0;
      const out = rows.map((r) => {
        const totalValue = r.lines.reduce((s, l) => s + num(l.quantity) * num(l.unitCost), 0);
        grandValue += totalValue;
        return {
          docNumber: r.docNumber,
          adjustmentDate: isoDate(r.adjustmentDate),
          warehouse: display(names.ref(names.warehouse, r.warehouseId)),
          status: r.status,
          lineCount: r.lines.length,
          totalValue,
        };
      });

      const summary: ReportSummaryItem[] = [
        { label: 'Jumlah Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai (halaman)', value: grandValue, type: 'money' },
      ];
      return { rows: out, summary, total };
    },
  };
}

/* ==================================================== PRICE ADJUSTMENTS */

export function buildPriceAdjustmentReport(deps: ReportDeps): ReportDef {
  return {
    key: 'price-adjustments',
    title: 'Price Adjustment (PA)',
    group: 'transaction',
    columns: PRICE_ADJUSTMENT_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);

      const where: Record<string, unknown> = {
        ...SOFT_DELETE,
        ...dateRange('fromDate', filters),
        ...statusWhere(filters),
        ...searchWhere(filters),
      };
      if (filters.warehouseId) where.warehouseId = BigInt(filters.warehouseId);
      if (filters.itemId) where.itemId = BigInt(filters.itemId);

      const [total, rows] = await Promise.all([
        prisma.erpInvCostRecalculation.count({ where }),
        prisma.erpInvCostRecalculation.findMany({
          where,
          orderBy: { fromDate: 'desc' },
          skip,
          take,
        }),
      ]);

      const names = await resolveNames(prisma, {
        itemIds: rows.map((r) => r.itemId),
        warehouseIds: rows.map((r) => r.warehouseId),
      });

      let grandDelta = 0;
      const out = rows.map((r) => {
        const totalDelta = num(r.totalDelta);
        grandDelta += totalDelta;
        return {
          docNumber: r.docNumber,
          fromDate: isoDate(r.fromDate),
          toDate: isoDate(r.toDate),
          warehouse: display(names.ref(names.warehouse, r.warehouseId)),
          item: display(names.ref(names.item, r.itemId)),
          status: r.status,
          totalDelta,
        };
      });

      const summary: ReportSummaryItem[] = [
        { label: 'Jumlah Dokumen', value: total, type: 'number' },
        { label: 'Total Delta (halaman)', value: grandDelta, type: 'money' },
      ];
      return { rows: out, summary, total };
    },
  };
}

/* ====================================================== OPENING STOCKS */

export function buildOpeningStockReport(deps: ReportDeps): ReportDef {
  return {
    key: 'opening-stocks',
    title: 'Opening Stock (IB)',
    group: 'transaction',
    columns: OPENING_STOCK_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);
      const where = baseWhere('openingDate', filters);

      const [total, rows] = await Promise.all([
        prisma.erpInvOpeningStock.count({ where }),
        prisma.erpInvOpeningStock.findMany({
          where,
          orderBy: { openingDate: 'desc' },
          skip,
          take,
          include: { lines: { select: { quantity: true, unitCost: true } } },
        }),
      ]);

      const names = await resolveNames(prisma, { warehouseIds: rows.map((r) => r.warehouseId) });

      let grandValue = 0;
      const out = rows.map((r) => {
        const totalValue = r.lines.reduce((s, l) => s + num(l.quantity) * num(l.unitCost), 0);
        grandValue += totalValue;
        return {
          docNumber: r.docNumber,
          openingDate: isoDate(r.openingDate),
          warehouse: display(names.ref(names.warehouse, r.warehouseId)),
          status: r.status,
          lineCount: r.lines.length,
          totalValue,
        };
      });

      const summary: ReportSummaryItem[] = [
        { label: 'Jumlah Dokumen', value: total, type: 'number' },
        { label: 'Total Nilai (halaman)', value: grandValue, type: 'money' },
      ];
      return { rows: out, summary, total };
    },
  };
}

/* ======================================================= DAILY CHECKS */

export function buildDailyCheckReport(deps: ReportDeps): ReportDef {
  return {
    key: 'daily-checks',
    title: 'Time Sheet/Daily Check (DC)',
    group: 'transaction',
    columns: DAILY_CHECK_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);

      // Daily check has no warehouseId on the header → omit warehouse filter.
      const where: Record<string, unknown> = {
        ...SOFT_DELETE,
        ...dateRange('checkDate', filters),
        ...statusWhere(filters),
        ...searchWhere(filters),
      };

      const [total, rows] = await Promise.all([
        prisma.erpInvDailyCheck.count({ where }),
        prisma.erpInvDailyCheck.findMany({
          where,
          orderBy: { checkDate: 'desc' },
          skip,
          take,
          include: { _count: { select: { lines: true } } },
        }),
      ]);

      const out = rows.map((r) => ({
        docNumber: r.docNumber,
        checkDate: isoDate(r.checkDate),
        branch: r.branchId.toString(),
        machineRef: r.machineRef ?? '',
        operatorRef: r.operatorRef ?? '',
        status: r.status,
        lineCount: r._count.lines,
      }));

      const summary: ReportSummaryItem[] = [{ label: 'Jumlah Dokumen', value: total, type: 'number' }];
      return { rows: out, summary, total };
    },
  };
}

/* ================================================= WEIGHBRIDGE TICKETS */

export function buildWeighbridgeReport(deps: ReportDeps): ReportDef {
  return {
    key: 'receipt-weighers',
    title: 'Receipt Weigher (RW)',
    group: 'transaction',
    columns: WEIGHBRIDGE_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);

      // Weighbridge ticket has no warehouseId → omit warehouse filter.
      const where: Record<string, unknown> = {
        ...SOFT_DELETE,
        ...dateRange('ticketDate', filters),
        ...statusWhere(filters),
        ...searchWhere(filters),
      };
      if (filters.partnerId) where.partnerId = BigInt(filters.partnerId);

      const [total, rows] = await Promise.all([
        prisma.erpInvWeighbridgeTicket.count({ where }),
        prisma.erpInvWeighbridgeTicket.findMany({
          where,
          orderBy: { ticketDate: 'desc' },
          skip,
          take,
        }),
      ]);

      const names = await resolveNames(prisma, { partnerIds: rows.map((r) => r.partnerId) });

      const out = rows.map((r) => ({
        docNumber: r.docNumber,
        weighDate: isoDate(r.ticketDate),
        partner: display(names.ref(names.partner, r.partnerId)),
        grossWeight: num(r.grossWeight),
        tareWeight: num(r.tareWeight),
        netWeight: num(r.netWeight),
        status: r.status,
      }));

      const summary: ReportSummaryItem[] = [{ label: 'Jumlah Dokumen', value: total, type: 'number' }];
      return { rows: out, summary, total };
    },
  };
}

