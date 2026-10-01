'use client';

/** Kartu Hutang (AP Card) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'ap-card',
  title: 'Kartu Hutang',
  codeTag: 'APC',
  mode: 'range',
};

export function ErpApCardPage() {
  return <ReportPage config={CONFIG} />;
}
