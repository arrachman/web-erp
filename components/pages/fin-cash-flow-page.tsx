'use client';

/** Arus Kas (Cash Flow) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'cash-flow',
  title: 'Arus Kas',
  codeTag: 'CF',
  mode: 'range',
};

export function ErpCashFlowPage() {
  return <ReportPage config={CONFIG} />;
}
