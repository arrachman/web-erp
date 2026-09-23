'use client';

/**
 * The four inventory stock-movement transaction pages. Each is a thin wrapper
 * that binds its per-face config (movementType + transactionCode + base + title)
 * to the shared `InvStockMovementsPage` core. Atomic tier: Page.
 *
 * All four ride the same backend module (erp-inv-stock-movements) discriminated
 * by `movementType`; only the labels, the seeded Form Builder / Kustomisasi Grid
 * config (per transactionCode), and the canonical route differ.
 */

import type { TrxFormPageProps } from '@/lib/trx-route';
import {
  InvStockMovementsPage,
  type InvStockMovementPageConfig,
} from './inv-stock-movements-page-core';

/** Material Request (MR) — /warehouse/material-requests, code INV.MR. */
const MATERIAL_REQUEST: InvStockMovementPageConfig = {
  movementType: 'REQUEST',
  transactionCode: 'INV.MR',
  base: '/warehouse/material-requests',
  title: 'Material Request',
  code: 'MR',
};

/** Stock Transfer (TS) — /warehouse/transfers, code INV.TS. */
const STOCK_TRANSFER: InvStockMovementPageConfig = {
  movementType: 'TRANSFER',
  transactionCode: 'INV.TS',
  base: '/warehouse/transfers',
  title: 'Stock Transfer',
  code: 'TS',
};

/** Transfer Receipt (RS) — /warehouse/transfer-receipts, code INV.RS. */
const TRANSFER_RECEIPT: InvStockMovementPageConfig = {
  movementType: 'TRANSFER_RECEIPT',
  transactionCode: 'INV.RS',
  base: '/warehouse/transfer-receipts',
  title: 'Transfer Receipt',
  code: 'RS',
};

/** Fuel Refill (RF) — /warehouse/fuel-refills, code INV.RF. */
const FUEL_REFILL: InvStockMovementPageConfig = {
  movementType: 'ISSUE',
  transactionCode: 'INV.RF',
  base: '/warehouse/fuel-refills',
  title: 'Fuel Refill',
  code: 'RF',
};

export function ErpInvMaterialRequestsPage(props: TrxFormPageProps = {}) {
  return InvStockMovementsPage(MATERIAL_REQUEST, props);
}

export function ErpInvTransfersPage(props: TrxFormPageProps = {}) {
  return InvStockMovementsPage(STOCK_TRANSFER, props);
}

export function ErpInvTransferReceiptsPage(props: TrxFormPageProps = {}) {
  return InvStockMovementsPage(TRANSFER_RECEIPT, props);
}

export function ErpInvFuelRefillsPage(props: TrxFormPageProps = {}) {
  return InvStockMovementsPage(FUEL_REFILL, props);
}
