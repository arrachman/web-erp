/**
 * Report route renderers — the four module report hubs, the Warehouse (M3) &
 * Purchasing (M4) prefix guards, plus the serial-card special case (all share
 * InvReportPage / invReportOptions).
 * Each helper returns React.ReactNode | null (null = route not handled).
 * Extracted from shell-route-renderer to keep files under 400 lines.
 * Helper ordering here MUST match shell-route-renderer guard order:
 *   report hubs → warehouse prefix → purchasing prefix →
 *   (REGISTER_CONFIGS in parent) → serial-cards special case →
 *   (transaction + generic registries in parent).
 */

import * as React from 'react';
import { FinDocReportPage } from '@/components/pages/fin-doc-report-page';
import { InvReportPage } from '@/components/pages/inv-report-page';
import { invReportOptions } from '@/lib/inv-report-options';
import { PurReportPage } from '@/components/pages/pur-report-page';
import { purReportOptions } from '@/lib/pur-report-options';
import { ReportHubPage } from '@/components/pages/report-hub-page';
import { MrtReportPage } from '@/components/pages/mrt-report-page';
import type { ReportModule } from '@/lib/api/report-catalog';

/**
 * The "Reports" node of each module is a `sys_menus` GROUP with `path = NULL`
 * (an invariant — GROUPs never carry a path), so these bare hub paths had no
 * renderer and fell through to <ComingSoon>. They now render the catalog hub.
 */
const REPORT_HUBS: Record<string, { module: ReportModule; title: string }> = {
  '/finance/reports': { module: 'fin', title: 'Laporan Keuangan' },
  '/warehouse/reports': { module: 'inv', title: 'Laporan Gudang' },
  '/purchasing/reports': { module: 'pur', title: 'Laporan Pembelian' },
  '/sales/reports': { module: 'sls', title: 'Laporan Penjualan' },
};

/**
 * Must run BEFORE the prefix guards below: those use `startsWith` on
 * `<base>/` and would not match the bare hub path, but keeping the hub first
 * makes the precedence explicit.
 */
export function renderReportHubRoute(
  route: string,
  onNavigate: (route: string) => void,
): React.ReactNode {
  const normalized = route.endsWith('/') ? route.slice(0, -1) : route;
  const mrtHub = MRT_HUBS[normalized];
  if (mrtHub) {
    return <MrtReportPage module={mrtHub.module} title={mrtHub.title} code={normalized.slice(1)} />;
  }
  const hub = REPORT_HUBS[normalized];
  if (!hub) return null;
  return (
    <ReportHubPage
      module={hub.module}
      title={hub.title}
      code={normalized.slice(1)}
      onNavigate={onNavigate}
    />
  );
}

/** Hub laporan .mrt (Wave G1): satu menu per modul + combo box jenis laporan. */
const MRT_HUBS: Record<string, { module: string; title: string }> = {
  '/master/reports': { module: 'M1', title: 'Laporan Master Data' },
  '/finance/reports': { module: 'FIN', title: 'Laporan Keuangan' },
  '/warehouse/reports': { module: 'M3', title: 'Laporan Gudang' },
};

/** Base path for the generic Finance document-report pages. */
const FIN_DOC_REPORT_PREFIX = '/finance/reports/';

/** Base path for the generic Warehouse (M3) report pages. */
const INV_REPORT_PREFIX = '/warehouse/reports/';

/** Base path for the generic Purchasing (M4) report pages. */
const PUR_REPORT_PREFIX = '/purchasing/reports/';

export function renderFinanceDocumentReportRoute(route: string): React.ReactNode {
  if (!route.startsWith(FIN_DOC_REPORT_PREFIX)) return null;
  const reportKey = route.slice(FIN_DOC_REPORT_PREFIX.length);
  return reportKey ? <FinDocReportPage reportKey={reportKey} /> : null;
}

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