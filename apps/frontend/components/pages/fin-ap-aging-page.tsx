'use client';

/** Analisis Umur Hutang (AP Aging) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'ap-aging',
  title: 'Analisis Umur Hutang',
  codeTag: 'APA',
  mode: 'asof',
};

export function ErpApAgingPage() {
  return <ReportPage config={CONFIG} />;
}
