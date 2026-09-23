'use client';

/** Analisis Umur Piutang (AR Aging) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'ar-aging',
  title: 'Analisis Umur Piutang',
  codeTag: 'ARA',
  mode: 'asof',
};

export function ErpArAgingPage() {
  return <ReportPage config={CONFIG} />;
}
