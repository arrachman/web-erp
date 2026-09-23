'use client';

/** Kas Bank Harian (Daily Cash & Bank) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'daily-cash-bank',
  title: 'Kas Bank Harian',
  codeTag: 'DCB',
  mode: 'range',
};

export function ErpDailyCashBankPage() {
  return <ReportPage config={CONFIG} />;
}
