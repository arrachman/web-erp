'use client';

/** Realisasi Anggaran (Budget vs Realization) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'budget-realization',
  title: 'Realisasi Anggaran',
  codeTag: 'BVR',
  mode: 'range',
};

export function ErpBudgetRealizationPage() {
  return <ReportPage config={CONFIG} />;
}
