'use client';

/** Opening Balance / CoA (FIN.BB) — thin wrapper over the generic journal page. */

import { JournalEntriesPage, type JournalPageConfig } from './journal-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: JournalPageConfig = {
  base: '/finance/opening-balances',
  code: 'FIN.BB',
  journalType: 'OPENING_BALANCE',
  documentType: 'BANK_BALANCE',
  title: 'Saldo Awal (CoA)',
  codeTag: 'BB',
};

export function ErpOpeningBalancesPage(props: TrxFormPageProps = {}) {
  return <JournalEntriesPage config={CONFIG} {...props} />;
}
