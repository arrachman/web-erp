/**
 * Transaction route renderer — exact TRX_FORM_PAGES list lookup then
 * /new or /:id resolution via resolveTrxFormRoute.
 * Returns React.ReactNode | null (null = not a transaction route).
 * Extracted from shell-route-renderer to keep files under 400 lines.
 * The exact-list lookup MUST run BEFORE resolveTrxFormRoute.
 */

import * as React from 'react';
import { resolveTrxFormRoute } from '@/lib/trx-route';
import { TRX_FORM_PAGES } from '../shell-trx-pages';

/** Snapshot Object.keys(TRX_FORM_PAGES) once at module init. */
const TRX_BASES = Object.keys(TRX_FORM_PAGES);

export function renderTransactionRoute(
  route: string,
  onNavigate: (r: string) => void,
): React.ReactNode {
  const TrxListPage = TRX_FORM_PAGES[route];
  if (TrxListPage) return <TrxListPage onNavigate={onNavigate} />;

  const trx = resolveTrxFormRoute(route, TRX_BASES);
  if (trx) {
    const TrxFormPageCmp = TRX_FORM_PAGES[trx.base];
    return (
      <TrxFormPageCmp
        formMode={trx.mode}
        recordId={trx.recordId}
        onNavigate={onNavigate}
      />
    );
  }

  return null;
}