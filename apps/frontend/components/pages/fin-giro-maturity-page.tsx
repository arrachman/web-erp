'use client';

/** Jatuh Tempo Giro (Giro Maturity) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'giro-maturity',
  title: 'Jatuh Tempo Giro',
  codeTag: 'GM',
  mode: 'range',
};

export function ErpGiroMaturityPage() {
  return <ReportPage config={CONFIG} />;
}
