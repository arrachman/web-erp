'use client';

/**
 * Receipt Weigher (RW) transaction page. Thin wrapper binding per-page config
 * (transactionCode + base + title) to the shared InvWeighbridgeTicketsPage
 * core. Rides the erp-inv-weighbridge-tickets backend module. Atomic tier: Page.
 *
 * transactionCode: 'INV.RW'
 * base: '/warehouse/receipt-weighers'
 */

import type { TrxFormPageProps } from '@/lib/trx-route';
import {
  InvWeighbridgeTicketsPage,
  type InvWeighbridgeTicketPageConfig,
} from './inv-weighbridge-tickets-page-core';

/** Receipt Weigher (RW) — /warehouse/receipt-weighers, code INV.RW. */
const RECEIPT_WEIGHER: InvWeighbridgeTicketPageConfig = {
  transactionCode: 'INV.RW',
  base: '/warehouse/receipt-weighers',
  title: 'Receipt Weigher',
  code: 'RW',
};

export function ErpInvWeighbridgeTicketsPage(props: TrxFormPageProps = {}) {
  return InvWeighbridgeTicketsPage(RECEIPT_WEIGHER, props);
}
