'use client';

/** Send Giro Clearing (FIN.SGC) — thin wrapper over the generic giro page. */

import { GiroEntriesPage, type GiroPageConfig } from './giro-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: GiroPageConfig = {
  base: '/finance/send-giro-clearings',
  code: 'FIN.SGC',
  kind: 'CLEAR',
  type: 'OUTGOING',
  documentType: 'GIRO_CLEARING_OUT',
  title: 'Kliring Giro Keluar',
  codeTag: 'SGC',
};

export function ErpSendGiroClearingsPage(props: TrxFormPageProps = {}) {
  return <GiroEntriesPage config={CONFIG} {...props} />;
}
