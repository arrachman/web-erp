/**
 * Purpose-specific Warehouse recaps for sources that are measured in physical
 * quantities, not partner monetary balances.
 */

import { ErpStockMovementType } from '@prisma/client';
import { ReportDef, ReportFilters, ReportSummaryItem } from './report-types';
import { ReportDeps } from './report-deps';
import { SOFT_DELETE, dateRange, num, searchWhere, statusWhere } from './report-resolvers-txn-cols';

const STATUS_RECAP_COLUMNS = [
  { key: 'status', header: 'Status', type: 'status' as const },
  { key: 'documentCount', header: 'Jumlah Dokumen', type: 'number' as const },
  { key: 'lineCount', header: 'Jumlah Baris', type: 'number' as const },
  { key: 'totalQty', header: 'Total Qty', type: 'qty' as const },
  { key: 'workHours', header: 'Jam Kerja', type: 'number' as const },
];

function movementWhere(filters: ReportFilters): Record<string, unknown> {
  const where: Record<string, unknown> = {
    ...SOFT_DELETE,
    movementType: ErpStockMovementType.ISSUE,
    ...dateRange('movementDate', filters),
    ...statusWhere(filters),
    ...searchWhere(filters),
  };
  if (filters.warehouseId) {
    const warehouseId = BigInt(filters.warehouseId);
    where.OR = [{ sourceWarehouseId: warehouseId }, { destinationWarehouseId: warehouseId }];
  }
  return where;
}

function dailyCheckWhere(filters: ReportFilters): Record<string, unknown> {
  return {
    ...SOFT_DELETE,
    ...dateRange('checkDate', filters),
    ...statusWhere(filters),
    ...searchWhere(filters),
  };
}

function statusChart(rows: Array<{ status: string; documentCount: number }>) {
  return {
    kind: 'donut' as const,
    title: 'Dokumen per Status',
    labels: rows.map((row) => row.status),
    values: rows.map((row) => row.documentCount),
  };
}

export function buildFuelRefillRecap(deps: ReportDeps): ReportDef {
  return {
    key: 'fuel-refills-by-status',
    title: 'Rekap Fuel Refill per Status',
    group: 'transaction',
    columns: STATUS_RECAP_COLUMNS.slice(0, 4),
    resolve: async (filters) => {
      const records = await deps.prisma.erpInvStockMovement.findMany({
        where: movementWhere(filters),
        select: {
          status: true,
          lines: { select: { baseQuantity: true } },
        },
      });
      const grouped = new Map<string, { documentCount: number; lineCount: number; totalQty: number }>();
      for (const record of records) {
        const summary = grouped.get(record.status) ?? { documentCount: 0, lineCount: 0, totalQty: 0 };
        summary.documentCount += 1;
        summary.lineCount += record.lines.length;
        summary.totalQty += record.lines.reduce((total, line) => total + num(line.baseQuantity), 0);
        grouped.set(record.status, summary);
      }
      const rows = [...grouped.entries()].map(([status, value]) => ({ status, ...value }));
      const summary: ReportSummaryItem[] = [
        { label: 'Jumlah Dokumen', value: records.length, type: 'number' },
        { label: 'Total Qty', value: rows.reduce((total, row) => total + row.totalQty, 0), type: 'qty' },
      ];
      return { rows, total: rows.length, summary, charts: [statusChart(rows)] };
    },
  };
}

export function buildDailyCheckRecap(deps: ReportDeps): ReportDef {
  return {
    key: 'daily-checks-by-status',
    title: 'Rekap Daily Check per Status',
    group: 'transaction',
    columns: STATUS_RECAP_COLUMNS,
    resolve: async (filters) => {
      const records = await deps.prisma.erpInvDailyCheck.findMany({
        where: dailyCheckWhere(filters),
        select: {
          status: true,
          lines: { select: { quantity: true, workHours: true } },
        },
      });
      const grouped = new Map<string, { documentCount: number; lineCount: number; totalQty: number; workHours: number }>();
      for (const record of records) {
        const summary = grouped.get(record.status) ?? {
          documentCount: 0,
          lineCount: 0,
          totalQty: 0,
          workHours: 0,
        };
        summary.documentCount += 1;
        summary.lineCount += record.lines.length;
        summary.totalQty += record.lines.reduce((total, line) => total + num(line.quantity), 0);
        summary.workHours += record.lines.reduce((total, line) => total + num(line.workHours), 0);
        grouped.set(record.status, summary);
      }
      const rows = [...grouped.entries()].map(([status, value]) => ({ status, ...value }));
      const summary: ReportSummaryItem[] = [
        { label: 'Jumlah Dokumen', value: records.length, type: 'number' },
        { label: 'Total Qty', value: rows.reduce((total, row) => total + row.totalQty, 0), type: 'qty' },
        { label: 'Total Jam Kerja', value: rows.reduce((total, row) => total + row.workHours, 0), type: 'number' },
      ];
      return { rows, total: rows.length, summary, charts: [statusChart(rows)] };
    },
  };
}
