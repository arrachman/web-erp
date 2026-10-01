'use client';

/** Neraca Mutasi (Trial Balance with movement) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'movement-balance',
  title: 'Neraca Mutasi',
  codeTag: 'NM',
  mode: 'range',
};

export function ErpMovementBalancePage() {
  return <ReportPage config={CONFIG} />;
}
