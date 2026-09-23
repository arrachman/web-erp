'use client';

/** Kartu Piutang (AR Card) — finance report view + export. */

import { ReportPage, type ReportPageConfig } from './report-page';

const CONFIG: ReportPageConfig = {
  report: 'ar-card',
  title: 'Kartu Piutang',
  codeTag: 'ARC',
  mode: 'range',
};

export function ErpArCardPage() {
  return <ReportPage config={CONFIG} />;
}
