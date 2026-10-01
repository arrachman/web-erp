'use client';

/** Neraca Saldo (Trial Balance) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'trial-balance',
  title: 'Neraca Saldo',
  codeTag: 'TB',
  mode: 'range',
};

export function ErpTrialBalancePage() {
  return <ReportPage config={CONFIG} />;
}
