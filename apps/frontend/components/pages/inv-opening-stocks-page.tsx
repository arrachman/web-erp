'use client';

/**
 * Inventory opening-stock transaction page (Saldo Awal Persediaan). Thin wrapper
 * that binds its config (transactionCode + base + title) to the shared
 * `InvOpeningStocksPage` core. Atomic tier: Page.
 *
 * Rides the erp-inv-opening-stocks backend; header form + line grid are
 * config-driven (Form Builder / Kustomisasi Grid, code INV.IB).
 */

import type { TrxFormPageProps } from '@/lib/trx-route';
import {
  InvOpeningStocksPage,
  type InvOpeningStockPageConfig,
} from './inv-opening-stocks-page-core';

/** Opening Stock (IB) — /warehouse/opening-stocks, code INV.IB. */
const OPENING_STOCK: InvOpeningStockPageConfig = {
  transactionCode: 'INV.IB',
  base: '/warehouse/opening-stocks',
  title: 'Opening Stock',
  code: 'IB',
};

export function ErpInvOpeningStocksPage(props: TrxFormPageProps = {}) {
  return InvOpeningStocksPage(OPENING_STOCK, props);
}
