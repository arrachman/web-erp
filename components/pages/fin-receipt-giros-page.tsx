'use client';

/** Receipt Giro (FIN.RG) — thin wrapper over the generic giro page. */

import { GiroEntriesPage, type GiroPageConfig } from './giro-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: GiroPageConfig = {
  base: '/finance/receipt-giros',
  code: 'FIN.RG',
  kind: 'REGISTER',
  type: 'INCOMING',
  documentType: 'INCOMING_GIRO',
  title: 'Giro Masuk',
  codeTag: 'RG',
};

export function ErpReceiptGirosPage(props: TrxFormPageProps = {}) {
  return <GiroEntriesPage config={CONFIG} {...props} />;
}
