/**
 * Report route renderers — Warehouse (M3) & Purchasing (M4) prefix guards
 * plus the serial-card special case (all share InvReportPage / invReportOptions).
 * Each helper returns React.ReactNode | null (null = route not handled).
 * Extracted from shell-route-renderer to keep files under 400 lines.
 * Helper ordering here MUST match shell-route-renderer guard order:
 *   warehouse prefix → purchasing prefix → (REGISTER_CONFIGS in parent) →
 *   serial-cards special case → (transaction + generic registries in parent).
 */

import * as React from 'react';
import { InvReportPage } from '@/components/pages/inv-report-page';
import { invReportOptions } from '@/lib/inv-report-options';
import { PurReportPage } from '@/components/pages/pur-report-page';
import { purReportOptions } from '@/lib/pur-report-options';

/** Base path for the generic Warehouse (M3) report pages. */
const INV_REPORT_PREFIX = '/warehouse/reports/';

/** Base path for the generic Purchasing (M4) report pages. */
const PUR_REPORT_PREFIX = '/purchasing/reports/';

export function renderWarehouseReportRoute(route: string): React.ReactNode {
  if (route.startsWith(INV_REPORT_PREFIX)) {
    const reportKey = route.slice(INV_REPORT_PREFIX.length);
    if (reportKey) {
      const opt = invReportOptions(reportKey);
      return (
        <InvReportPage
          reportKey={reportKey}
          title={opt.title}
          asOfMode={opt.asOfMode}
          showItem={opt.showItem}
          statusOptions={opt.statusOptions}
        />
      );
    }
  }
  return null;
}

export function renderPurchasingReportRoute(route: string): React.ReactNode {
  if (route.startsWith(PUR_REPORT_PREFIX)) {
    const reportKey = route.slice(PUR_REPORT_PREFIX.length);
    if (reportKey) {
      const opt = purReportOptions(reportKey);
      return (
        <PurReportPage
          reportKey={reportKey}
          title={opt.title}
          showVendor={opt.showVendor}
          showItem={opt.showItem}
          statusOptions={opt.statusOptions}
        />
      );
    }
  }
  return null;
}

/**
 * Serial Item Cards "Data" entry is a report, not a document register.
 * Position: after REGISTER_CONFIGS, before transaction routes and generic
 * registries (must preserve current guard order in renderRoute).
 */
export function renderSerialCardsRoute(route: string): React.ReactNode {
  if (route !== '/warehouse/data/serial-cards') return null;
  const opt = invReportOptions('serial-cards');
  return (
    <InvReportPage
      reportKey="serial-cards"
      title={opt.title}
      asOfMode={opt.asOfMode}
      showItem={opt.showItem}
      statusOptions={opt.statusOptions}
    />
  );
}