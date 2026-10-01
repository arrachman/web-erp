/**
 * Transaction + direct-query report resolvers (group: transaction/item).
 * Covers the 11 M3 transaction documents (MR/TS/RS/RF/Return/SP/SA/PA/IB/DC/RW)
 * plus the batch/serial direct-query reports. Each reads Prisma read-only,
 * applies the common filter set (date range / status / warehouse / search /
 * pagination), and shapes rows to its declared columns.
 *
 * This file owns the five stock-movement reports + the catalog assembly; the
 * document-level (SP/SA/PA/IB/DC/RW) and batch/serial builders live in
 * `report-resolvers-txn-docs.ts` to keep every file under the 400-line cap.
 *
 * Cross-domain FKs (item/warehouse/partner) are scalar BigInt with no
 * @relation, so display code+name are batch-resolved via `resolveNames`.
 */

import { ErpStockMovementType } from '@prisma/client';
import { ReportDef, ReportFilters, ReportSummaryItem } from './report-types';
import { ReportDeps } from './report-deps';
import {
  MOVEMENT_COLUMNS,
  SOFT_DELETE,
  dateRange,
  display,
  isoDate,
  num,
  paginate,
  resolveNames,
  searchWhere,
  statusWhere,
} from './report-resolvers-txn-cols';
import {
  buildDailyCheckReport,
  buildOpeningStockReport,
  buildPriceAdjustmentReport,
  buildStockAdjustmentReport,
  buildStockCountReport,
  buildWeighbridgeReport,
} from './report-resolvers-txn-docs';
import { buildBatchReport, buildSerialReport } from './report-resolvers-txn-items';

/* ============================================================ MOVEMENTS */

function buildMovementReport(
  deps: ReportDeps,
  key: string,
  title: string,
  movementType: ErpStockMovementType,
): ReportDef {
  return {
    key,
    title,
    group: 'transaction',
    columns: MOVEMENT_COLUMNS,
    resolve: async (filters: ReportFilters) => {
      const { prisma } = deps;
      const { skip, take } = paginate(filters);

      const where: Record<string, unknown> = {
        ...SOFT_DELETE,
        movementType,
        ...dateRange('movementDate', filters),
        ...statusWhere(filters),
        ...searchWhere(filters),
      };
      if (filters.warehouseId) {
        const wh = BigInt(filters.warehouseId);
        where.OR = [{ sourceWarehouseId: wh }, { destinationWarehouseId: wh }];
      }

      const [total, rows] = await Promise.all([
        prisma.erpInvStockMovement.count({ where }),
        prisma.erpInvStockMovement.findMany({
          where,
          orderBy: { movementDate: 'desc' },
          skip,
          take,
          include: { lines: { select: { baseQuantity: true } } },
        }),
      ]);

      const names = await resolveNames(prisma, {
        warehouseIds: rows.flatMap((r) => [r.sourceWarehouseId, r.destinationWarehouseId]),
      });

      let grandQty = 0;
      const out = rows.map((r) => {
        const totalQty = r.lines.reduce((s, l) => s + num(l.baseQuantity), 0);
        grandQty += totalQty;
        return {
          docNumber: r.docNumber,
          movementDate: isoDate(r.movementDate),
          sourceWarehouse: display(names.ref(names.warehouse, r.sourceWarehouseId)),
          destinationWarehouse: display(names.ref(names.warehouse, r.destinationWarehouseId)),
          referenceNo: r.referenceNo ?? '',
          status: r.status,
          totalQty,
          lineCount: r.lines.length,
        };
      });

      const summary: ReportSummaryItem[] = [
        { label: 'Jumlah Dokumen', value: total, type: 'number' },
        { label: 'Total Qty (halaman)', value: grandQty, type: 'qty' },
      ];
      return { rows: out, summary, total };
    },
  };
}

/* ================================================================ EXPORT */

export function buildTxnReports(deps: ReportDeps): ReportDef[] {
  return [
    buildMovementReport(deps, 'material-requests', 'Material Request (MR)', ErpStockMovementType.REQUEST),
    buildMovementReport(deps, 'transfers', 'Stock Transfer (TS)', ErpStockMovementType.TRANSFER),
    buildMovementReport(deps, 'transfer-receipts', 'Transfer Receipt (RS)', ErpStockMovementType.TRANSFER_RECEIPT),
    buildMovementReport(deps, 'fuel-refills', 'Fuel Refill (RF)', ErpStockMovementType.ISSUE),
    buildMovementReport(deps, 'returns', 'Return Items', ErpStockMovementType.RETURN),
    buildStockCountReport(deps),
    buildStockAdjustmentReport(deps),
    buildPriceAdjustmentReport(deps),
    buildOpeningStockReport(deps),
    buildDailyCheckReport(deps),
    buildWeighbridgeReport(deps),
    buildBatchReport(deps, 'batch-items', 'Batch Items'),
    buildBatchReport(deps, 'batch-cards', 'Batch Item Cards'),
    buildSerialReport(deps, 'serial-items', 'Serial Items'),
    buildSerialReport(deps, 'serial-cards', 'Serial Item Cards'),
  ];
}
