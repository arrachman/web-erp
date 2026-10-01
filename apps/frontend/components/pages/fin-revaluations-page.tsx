'use client';

/** FX Revaluation / Revaluasi Valas (FIN.RV) — thin wrapper over the generic journal page. */

import { JournalEntriesPage, type JournalPageConfig } from './journal-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: JournalPageConfig = {
  base: '/finance/revaluations',
  code: 'FIN.RV',
  journalType: 'REVALUATION',
  documentType: 'REVERSAL',
  title: 'Revaluasi Valas',
  codeTag: 'RV',
};

export function ErpRevaluationsPage(props: TrxFormPageProps = {}) {
  return <JournalEntriesPage config={CONFIG} {...props} />;
}
