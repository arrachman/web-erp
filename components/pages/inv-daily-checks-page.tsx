'use client';

/**
 * Daily Check (DC) transaction page. Thin wrapper binding per-page config
 * (transactionCode + base + title) to the shared InvDailyChecksPage core.
 * Rides the erp-inv-daily-checks backend module. Atomic tier: Page.
 *
 * transactionCode: 'INV.DC'
 * base: '/warehouse/daily-checks'
 */

import type { TrxFormPageProps } from '@/lib/trx-route';
import {
  InvDailyChecksPage,
  type InvDailyCheckPageConfig,
} from './inv-daily-checks-page-core';

/** Daily Check (DC) — /warehouse/daily-checks, code INV.DC. */
const DAILY_CHECK: InvDailyCheckPageConfig = {
  transactionCode: 'INV.DC',
  base: '/warehouse/daily-checks',
  title: 'Daily Check',
  code: 'DC',
};

export function ErpInvDailyChecksPage(props: TrxFormPageProps = {}) {
  return InvDailyChecksPage(DAILY_CHECK, props);
}
