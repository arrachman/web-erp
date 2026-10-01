'use client';

/** General Journal (FIN.GJ) — thin wrapper over the generic journal page. */

import { JournalEntriesPage, type JournalPageConfig } from './journal-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: JournalPageConfig = {
  base: '/finance/general-journals',
  code: 'FIN.GJ',
  journalType: 'GENERAL',
  documentType: 'JOURNAL_ENTRY',
  title: 'Jurnal Umum',
  codeTag: 'GJ',
};

export function ErpGeneralJournalsPage(props: TrxFormPageProps = {}) {
  return <JournalEntriesPage config={CONFIG} {...props} />;
}
