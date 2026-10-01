/**
 * Item-level direct-query report builders (group: item) split out of
 * `report-resolvers-txn-docs.ts` to keep every file under the 400-line cap.
 * Batch reports read `ErpInvLot` (a lot = a batch); serial reports read
 * `ErpInvSerial`. All queries are read-only.
 */

import { ReportDef, ReportSummaryItem } from './report-types';
import { ReportDeps } from './report-deps';
import {
  BATCH_ITEM_COLUMNS,
  SERIAL_ITEM_COLUMNS,
  SOFT_DELETE,
  display,
  isoDate,
  paginate,
  resolveNames,
} from './report-resolvers-txn-cols';

/* ============================================= BATCH ITEMS / CARDS (group: item) */

export function buildBatchReport(deps: ReportDeps, key: string, title: string): ReportDef {
  return {
    key,
    title,
    group: 'item',
    columns: BATCH_ITEM_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);

      // A "batch" = an inventory lot (ErpInvLot). Lots are not warehouse-scoped
      // (warehouse lives on the serial/stock); the warehouse column stays blank.
      const where: Record<string, unknown> = { ...SOFT_DELETE };
      if (filters.itemId) where.itemId = BigInt(filters.itemId);
      if (filters.search) where.lotNumber = { contains: filters.search, mode: 'insensitive' };

      const [total, rows] = await Promise.all([
        prisma.erpInvLot.count({ where }),
        prisma.erpInvLot.findMany({ where, orderBy: { createdAt: 'desc' }, skip, take }),
      ]);

      const names = await resolveNames(prisma, { itemIds: rows.map((r) => r.itemId) });

      const out = rows.map((r) => {
        const item = names.ref(names.item, r.itemId);
        return {
          itemCode: item?.code ?? '',
          itemName: item?.name ?? '',
          batchNo: r.lotNumber,
          warehouse: '',
          quantity: 0,
          expiryDate: isoDate(r.expiryDate),
        };
      });

      const summary: ReportSummaryItem[] = [{ label: 'Jumlah Batch', value: total, type: 'number' }];
      return { rows: out, summary, total };
    },
  };
}

/* ============================================ SERIAL ITEMS / CARDS (group: item) */

export function buildSerialReport(deps: ReportDeps, key: string, title: string): ReportDef {
  return {
    key,
    title,
    group: 'item',
    columns: SERIAL_ITEM_COLUMNS,
    resolve: async (filters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);

      const where: Record<string, unknown> = { ...SOFT_DELETE };
      if (filters.itemId) where.itemId = BigInt(filters.itemId);
      if (filters.warehouseId) where.currentWarehouseId = BigInt(filters.warehouseId);
      if (filters.status) where.status = filters.status;
      if (filters.search) where.serialNumber = { contains: filters.search, mode: 'insensitive' };

      const [total, rows] = await Promise.all([
        prisma.erpInvSerial.count({ where }),
        prisma.erpInvSerial.findMany({ where, orderBy: { createdAt: 'desc' }, skip, take }),
      ]);

      const names = await resolveNames(prisma, {
        itemIds: rows.map((r) => r.itemId),
        warehouseIds: rows.map((r) => r.currentWarehouseId),
      });

      const out = rows.map((r) => {
        const item = names.ref(names.item, r.itemId);
        return {
          itemCode: item?.code ?? '',
          itemName: item?.name ?? '',
          serialNo: r.serialNumber,
          warehouse: display(names.ref(names.warehouse, r.currentWarehouseId)),
          status: r.status,
        };
      });

      const summary: ReportSummaryItem[] = [{ label: 'Jumlah Serial', value: total, type: 'number' }];
      return { rows: out, summary, total };
    },
  };
}
