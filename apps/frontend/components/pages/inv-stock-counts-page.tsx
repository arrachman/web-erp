'use client';

/**
 * Inventory Stock Count (opname) transaction page. Thin wrapper binding the
 * per-document config (transactionCode + base + title) to the shared
 * `InvStockCountsPage` core. Atomic tier: Page.
 *
 * Rides the erp-inv-stock-counts backend; the seeded Form Builder / Kustomisasi
 * Grid config (per transactionCode) and the canonical route drive labels/columns.
 */

import type { TrxFormPageProps } from '@/lib/trx-route';
import {
  InvStockCountsPage,
  type InvStockCountPageConfig,
} from './inv-stock-counts-page-core';

/** Stock Count (SP) — /warehouse/stock-counts, code INV.SP. */
const STOCK_COUNT: InvStockCountPageConfig = {
  transactionCode: 'INV.SP',
  base: '/warehouse/stock-counts',
  title: 'Stock Count',
  code: 'SP',
};

export function ErpInvStockCountsPage(props: TrxFormPageProps = {}) {
  return InvStockCountsPage(STOCK_COUNT, props);
}
