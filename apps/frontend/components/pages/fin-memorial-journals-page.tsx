'use client';

/** Memorial Journal (FIN.JM) — thin wrapper over the generic journal page. */

import { JournalEntriesPage, type JournalPageConfig } from './journal-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: JournalPageConfig = {
  base: '/finance/memorial-journals',
  code: 'FIN.JM',
  journalType: 'MEMORIAL',
  documentType: 'JOURNAL_MEMO',
  title: 'Jurnal Memorial',
  codeTag: 'JM',
};

export function ErpMemorialJournalsPage(props: TrxFormPageProps = {}) {
  return <JournalEntriesPage config={CONFIG} {...props} />;
}
