'use client';

/** Neraca (Balance Sheet) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'balance-sheet',
  title: 'Neraca',
  codeTag: 'BS',
  mode: 'asof',
};

export function ErpBalanceSheetPage() {
  return <ReportPage config={CONFIG} />;
}
