'use client';

/** Perubahan Modal (Statement of Changes in Equity) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'equity-changes',
  title: 'Laporan Perubahan Modal',
  codeTag: 'EQ',
  mode: 'range',
};

export function ErpEquityChangesPage() {
  return <ReportPage config={CONFIG} />;
}
