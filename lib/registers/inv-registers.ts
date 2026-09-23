/**
 * Warehouse (M3) "Data" registers — read-only document registers (legacy DATA group).
 *
 * Each entry is keyed by its canonical `sys_menus.path` (`/warehouse/data/*`) and
 * consumed by `DocumentRegisterPage`. The document code links to the existing
 * Warehouse TX edit form (`editBase`). Movement-type docs (MR/TS/RS/RF) share the
 * stock-movements endpoint, discriminated by `movementType` via `extraParams`.
 *
 * Note: `/warehouse/data/serial-cards` is a report (no document list) and is wired
 * to the serial-cards report page in shell-route-renderer, not as a register here.
 */

import {
  listInvStockMovements,
  type ErpInvStockMovement,
  type ListInvStockMovementsParams,
} from '@/lib/api/inv-stock-movements';
import {
  listInvStockCounts,
  type ErpInvStockCount,
  type ListInvStockCountsParams,
} from '@/lib/api/inv-stock-counts';
import {
  listInvStockAdjustments,
  type ErpInvStockAdjustment,
  type ListInvStockAdjustmentsParams,
} from '@/lib/api/inv-stock-adjustments';
import {
  listInvPriceAdjustments,
  type ErpInvPriceAdjustment,
  type ListInvPriceAdjustmentsParams,
} from '@/lib/api/inv-price-adjustments';
import {
  listInvOpeningStocks,
  type ErpInvOpeningStock,
  type ListInvOpeningStocksParams,
} from '@/lib/api/inv-opening-stocks';
import {
  listInvDailyChecks,
  type ErpInvDailyCheck,
  type ListInvDailyChecksParams,
} from '@/lib/api/inv-daily-checks';
import {
  listInvWeighbridgeTickets,
  type ErpInvWeighbridgeTicket,
  type ListInvWeighbridgeTicketsParams,
} from '@/lib/api/inv-weighbridge-tickets';
import { formatNumber } from '@/lib/format';
import type { DocumentRegisterConfig, AnyDocumentRegisterConfig } from './register-config';

const GROUP = 'Warehouse · Data';
const dash = (v: string | null | undefined) => v || '—';

/** Movement-type register factory (MR/TS/RS/RF share one endpoint). */
function movementRegister(
  movementType: ListInvStockMovementsParams['movementType'],
  title: string,
  code: string,
  editBase: string,
): DocumentRegisterConfig<ErpInvStockMovement> {
  return {
    group: GROUP,
    title,
    code,
    icon: 'database',
    editBase,
    extraParams: { movementType },
    sortBy: 'movementDate',
    list: (p) => listInvStockMovements(p as unknown as ListInvStockMovementsParams),
    getId: (r) => r.id,
    getDocNumber: (r) => r.docNumber,
    getDocDate: (r) => r.movementDate,
    getStatus: (r) => r.status,
    columns: [
      { header: 'Cabang', render: (r) => dash(r.branch?.name), csv: (r) => r.branch?.name ?? '' },
      { header: 'Lokasi', render: (r) => dash(r.location?.name), csv: (r) => r.location?.name ?? '' },
      { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
    ],
  };
}

const stockCounts: DocumentRegisterConfig<ErpInvStockCount> = {
  group: GROUP,
  title: 'Stock Count (SP)',
  code: 'SP',
  icon: 'database',
  editBase: '/warehouse/stock-counts',
  sortBy: 'countDate',
  list: (p) => listInvStockCounts(p as unknown as ListInvStockCountsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.countDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Gudang', render: (r) => dash(r.warehouse?.name), csv: (r) => r.warehouse?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

const stockAdjustments: DocumentRegisterConfig<ErpInvStockAdjustment> = {
  group: GROUP,
  title: 'Stock Adjustment (SA)',
  code: 'SA',
  icon: 'database',
  editBase: '/warehouse/stock-adjustments',
  sortBy: 'adjustmentDate',
  list: (p) => listInvStockAdjustments(p as unknown as ListInvStockAdjustmentsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.adjustmentDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Gudang', render: (r) => dash(r.warehouse?.name), csv: (r) => r.warehouse?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

const priceAdjustments: DocumentRegisterConfig<ErpInvPriceAdjustment> = {
  group: GROUP,
  title: 'Price Adjustment (PA)',
  code: 'PA',
  icon: 'database',
  editBase: '/warehouse/price-adjustments',
  sortBy: 'fromDate',
  statusOptions: [
    { value: '', label: 'Semua Status' },
    { value: 'PENDING', label: 'Pending' },
    { value: 'COMPLETED', label: 'Completed' },
    { value: 'FAILED', label: 'Failed' },
  ],
  list: (p) => listInvPriceAdjustments(p as unknown as ListInvPriceAdjustmentsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.fromDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Item', render: (r) => dash(r.item?.name), csv: (r) => r.item?.name ?? '' },
    { header: 'Gudang', render: (r) => dash(r.warehouse?.name), csv: (r) => r.warehouse?.name ?? '' },
    {
      header: 'Σ Delta',
      align: 'right',
      render: (r) => formatNumber(Number(r.totalDelta ?? 0), 2),
      csv: (r) => r.totalDelta ?? '0',
    },
  ],
};

const openingStocks: DocumentRegisterConfig<ErpInvOpeningStock> = {
  group: GROUP,
  title: 'Opening Stock (IB)',
  code: 'IB',
  icon: 'database',
  editBase: '/warehouse/opening-stocks',
  sortBy: 'openingDate',
  list: (p) => listInvOpeningStocks(p as unknown as ListInvOpeningStocksParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.openingDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Gudang', render: (r) => dash(r.warehouse?.name), csv: (r) => r.warehouse?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

const dailyChecks: DocumentRegisterConfig<ErpInvDailyCheck> = {
  group: GROUP,
  title: 'Time Sheet / Daily Check (DC)',
  code: 'DC',
  icon: 'database',
  editBase: '/warehouse/daily-checks',
  sortBy: 'checkDate',
  list: (p) => listInvDailyChecks(p as unknown as ListInvDailyChecksParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.checkDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Cabang', render: (r) => dash(r.branch?.name), csv: (r) => r.branch?.name ?? '' },
    { header: 'Lokasi', render: (r) => dash(r.location?.name), csv: (r) => r.location?.name ?? '' },
    { header: 'Uraian', render: (r) => dash(r.description), csv: (r) => r.description ?? '' },
  ],
};

const receiptWeighers: DocumentRegisterConfig<ErpInvWeighbridgeTicket> = {
  group: GROUP,
  title: 'Receipt Weigher (RW)',
  code: 'RW',
  icon: 'database',
  editBase: '/warehouse/receipt-weighers',
  sortBy: 'ticketDate',
  list: (p) => listInvWeighbridgeTickets(p as unknown as ListInvWeighbridgeTicketsParams),
  getId: (r) => r.id,
  getDocNumber: (r) => r.docNumber,
  getDocDate: (r) => r.ticketDate,
  getStatus: (r) => r.status,
  columns: [
    { header: 'Partner', render: (r) => dash(r.partner?.name), csv: (r) => r.partner?.name ?? '' },
    { header: 'Plat', render: (r) => dash(r.vehiclePlate), csv: (r) => r.vehiclePlate ?? '' },
    {
      header: 'Berat Netto',
      align: 'right',
      render: (r) => formatNumber(Number(r.netWeight ?? 0), 2),
      csv: (r) => r.netWeight ?? '0',
    },
  ],
};

export const INV_REGISTERS: Record<string, AnyDocumentRegisterConfig> = {
  '/warehouse/data/material-requests': movementRegister('REQUEST', 'Material Request (MR)', 'MR', '/warehouse/material-requests'),
  '/warehouse/data/transfers': movementRegister('TRANSFER', 'Stock Transfer (TS)', 'TS', '/warehouse/transfers'),
  '/warehouse/data/transfer-receipts': movementRegister('TRANSFER_RECEIPT', 'Transfer Receipt (RS)', 'RS', '/warehouse/transfer-receipts'),
  '/warehouse/data/fuel-refills': movementRegister('ISSUE', 'Fuel Refill (RF)', 'RF', '/warehouse/fuel-refills'),
  '/warehouse/data/stock-counts': stockCounts,
  '/warehouse/data/stock-adjustments': stockAdjustments,
  '/warehouse/data/price-adjustments': priceAdjustments,
  '/warehouse/data/opening-stocks': openingStocks,
  '/warehouse/data/daily-checks': dailyChecks,
  '/warehouse/data/receipt-weighers': receiptWeighers,
};
