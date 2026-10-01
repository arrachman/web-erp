'use client';

/** Send Giro (FIN.SG) — thin wrapper over the generic giro page. */

import { GiroEntriesPage, type GiroPageConfig } from './giro-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: GiroPageConfig = {
  base: '/finance/send-giros',
  code: 'FIN.SG',
  kind: 'REGISTER',
  type: 'OUTGOING',
  documentType: 'OUTGOING_GIRO',
  title: 'Giro Keluar',
  codeTag: 'SG',
};

export function ErpSendGirosPage(props: TrxFormPageProps = {}) {
  return <GiroEntriesPage config={CONFIG} {...props} />;
}
