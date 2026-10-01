'use client';

/**
 * The inventory stock-adjustment transaction page. Thin wrapper that binds the
 * per-page config (transactionCode + base + title) to the shared
 * `InvStockAdjustmentsPage` core. Atomic tier: Page.
 *
 * Rides the erp-inv-stock-adjustments backend module; adjustment direction is
 * per-line (INCREASE/DECREASE). Inventory/contra GL accounts resolve server-side.
 */

import type { TrxFormPageProps } from '@/lib/trx-route';
import {
  InvStockAdjustmentsPage,
  type InvStockAdjustmentPageConfig,
} from './inv-stock-adjustments-page-core';

/** Stock Adjustment (SA) — /warehouse/stock-adjustments, code INV.SA. */
const STOCK_ADJUSTMENT: InvStockAdjustmentPageConfig = {
  transactionCode: 'INV.SA',
  base: '/warehouse/stock-adjustments',
  title: 'Stock Adjustment',
  code: 'SA',
};

export function ErpInvStockAdjustmentsPage(props: TrxFormPageProps = {}) {
  return InvStockAdjustmentsPage(STOCK_ADJUSTMENT, props);
}
