// Sub-route convention for ERP transaction forms (§2.3 / §2.36).
//
// Every transaction page (CR/CD/BD/giro/jurnal…) is URL-addressable in three
// shapes, all derived from its canonical `sys_menus.path` (the list `base`):
//
//   <base>        → list view          e.g. /finance/cash-receipts
//   <base>/new    → create form        e.g. /finance/cash-receipts/new
//   <base>/:id    → edit form          e.g. /finance/cash-receipts/123
//
// The route string is the single source of truth for list-vs-form, so the URL,
// the active tab, and the breadcrumb all stay in sync without per-page state.

import type * as React from 'react';

/** A resolved transaction form sub-route. `null` base route = plain list. */
export interface TrxFormRoute {
  /** Canonical list path (the page's seeded `sys_menus.path`). */
  base: string;
  mode: 'create' | 'edit';
  /** Present only when `mode === 'edit'`. */
  recordId?: string;
}

/** Props every transaction form page accepts so the router can drive its view. */
export interface TrxFormPageProps {
  /** Render the create form when `'create'`, the edit form when `'edit'`. */
  formMode?: 'create' | 'edit';
  /** Record id to load in edit mode. */
  recordId?: string;
  /** Navigate within the active tab (drives list↔form transitions). */
  onNavigate?: (route: string) => void;
}

export type TrxFormPage = React.ComponentType<TrxFormPageProps>;

/** URL builders — pages never hand-concatenate sub-routes. */
export const trxNewRoute = (base: string): string => `${base}/new`;
export const trxEditRoute = (base: string, id: string): string => `${base}/${id}`;

/**
 * Resolve a route string into a transaction form sub-route, given the set of
 * known transaction base paths. Returns `null` for the bare list path (handled
 * by the normal page render) and for any non-transaction route.
 */
export function resolveTrxFormRoute(
  route: string,
  bases: readonly string[],
): TrxFormRoute | null {
  for (const base of bases) {
    if (route === trxNewRoute(base)) return { base, mode: 'create' };
    if (route.startsWith(`${base}/`)) {
      const recordId = route.slice(base.length + 1);
      if (recordId && recordId !== 'new') return { base, mode: 'edit', recordId };
    }
  }
  return null;
}
