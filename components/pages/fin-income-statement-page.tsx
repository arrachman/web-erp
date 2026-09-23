'use client';

/** Laba Rugi (Income Statement) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'income-statement',
  title: 'Laba Rugi',
  codeTag: 'IS',
  mode: 'range',
};

export function ErpIncomeStatementPage() {
  return <ReportPage config={CONFIG} />;
}
