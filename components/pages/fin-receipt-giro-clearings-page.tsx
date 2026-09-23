'use client';

/** Receipt Giro Clearing (FIN.RGC) — thin wrapper over the generic giro page. */

import { GiroEntriesPage, type GiroPageConfig } from './giro-entries-page';
import type { TrxFormPageProps } from '@/lib/trx-route';

const CONFIG: GiroPageConfig = {
  base: '/finance/receipt-giro-clearings',
  code: 'FIN.RGC',
  kind: 'CLEAR',
  type: 'INCOMING',
  documentType: 'GIRO_CLEARING_IN',
  title: 'Kliring Giro Masuk',
  codeTag: 'RGC',
};

export function ErpReceiptGiroClearingsPage(props: TrxFormPageProps = {}) {
  return <GiroEntriesPage config={CONFIG} {...props} />;
}
