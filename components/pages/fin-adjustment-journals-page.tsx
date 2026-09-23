'use client';

/** Adjustment Journal (FIN.AJ) — thin wrapper over the generic journal page. */

import { JournalEntriesPage, type JournalPageConfig } from './journal-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: JournalPageConfig = {
  base: '/finance/adjustment-journals',
  code: 'FIN.AJ',
  journalType: 'ADJUSTMENT',
  documentType: 'ADJUSTING_JOURNAL',
  title: 'Jurnal Penyesuaian',
  codeTag: 'AJ',
};

export function ErpAdjustmentJournalsPage(props: TrxFormPageProps = {}) {
  return <JournalEntriesPage config={CONFIG} {...props} />;
}
