/**
 * Shell route renderer — maps a route string to the correct page component.
 * ErpPageCtx + ERP_PAGES  → shell-routes/erp-page-routes.tsx
 * Report prefix guards      → shell-routes/report-route-renderers.tsx
 * Transaction routes        → shell-routes/transaction-route-renderer.tsx
 * TRX_FORM_PAGES            → shell-trx-pages.ts
 * Public export `renderRoute` preserved (consumed by app-shell).
 */

import * as React from 'react';
import { makeTranslator } from '@/lib/mock';
import { Dashboard } from '@/components/pages/dashboard';
import { ComingSoon } from '@/components/pages/coming-soon';
import { Statistik } from '@/components/pages/statistik';
import { GenericList } from '@/components/pages/generic-list';
import { FinancialReport } from '@/components/pages/financial-report';
import { DataList } from '@/components/pages/data-list';
import { RecordForm } from '@/components/pages/record-form';
import { TrxForm } from '@/components/pages/trx-form';
import { REGISTRY, MODULES, REPORTS } from '@/lib/registry';
import { DocumentRegisterPage } from '@/components/organisms/document-register-page';
import { REGISTER_CONFIGS } from '@/lib/registers';
import type { Lang } from '@/lib/shell-constants';
import { ERP_PAGES } from './shell-routes/erp-page-routes';
import {
  renderWarehouseReportRoute,
  renderPurchasingReportRoute,
  renderSerialCardsRoute,
} from './shell-routes/report-route-renderers';
import { renderTransactionRoute } from './shell-routes/transaction-route-renderer';

function resolveNewRoute(route: string): string | null {
  if (!route.endsWith('-new')) return null;
  return route.slice(0, -'-new'.length);
}

export function renderRoute(
  route: string,
  onNavigate: (r: string) => void,
  onOpenTab: (r: string) => void,
  t: ReturnType<typeof makeTranslator>,
  lang: Lang,
): React.ReactNode {
  if (route === 'home') return <Dashboard t={t} onNavigate={onOpenTab} />;
  if (route === 'statistik') return <Statistik t={t} onNavigate={onOpenTab} />;

  const erpPage = ERP_PAGES[route];
  if (erpPage) return erpPage({ t });

  // ── Warehouse (M3) reports: one generic page driven by the report key ──────
  const invReport = renderWarehouseReportRoute(route);
  if (invReport !== null) return invReport;

  // ── Purchasing (M4) reports: one generic page driven by the report key ──────
  const purReport = renderPurchasingReportRoute(route);
  if (purReport !== null) return purReport;

  // ── Read-only "Data" registers (legacy DATA group): one config-driven page ──
  const registerCfg = REGISTER_CONFIGS[route];
  if (registerCfg) return <DocumentRegisterPage config={registerCfg} onNavigate={onNavigate} />;

  // Serial Item Cards "Data" entry is a report, not a document register.
  const serialCards = renderSerialCardsRoute(route);
  if (serialCards !== null) return serialCards;

  const trxResult = renderTransactionRoute(route, onNavigate);
  if (trxResult !== null) return trxResult;

  const baseRoute = resolveNewRoute(route);
  if (baseRoute) {
    if (MODULES[baseRoute])
      return <TrxForm moduleId={baseRoute} t={t} lang={lang} onNavigate={onNavigate} />;
    if (REGISTRY[baseRoute])
      return <RecordForm moduleId={baseRoute} t={t} onNavigate={onNavigate} />;
    return <ComingSoon route={route} />;
  }

  if (MODULES[route])
    return <GenericList moduleId={route} t={t} lang={lang} onNavigate={onNavigate} onOpenTab={onOpenTab} />;
  if (REPORTS[route]) return <FinancialReport moduleId={route} t={t} />;
  if (REGISTRY[route])
    return <DataList moduleId={route} t={t} onNavigate={onNavigate} onOpenTab={onOpenTab} />;
  return <ComingSoon route={route} />;
}