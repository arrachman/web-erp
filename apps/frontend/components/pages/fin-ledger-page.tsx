'use client';

/** Buku Besar (General Ledger) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'general-ledger',
  title: 'Buku Besar',
  codeTag: 'GL',
  mode: 'range',
};

export function ErpLedgerPage() {
  return <ReportPage config={CONFIG} />;
}
