/**
 * Report catalog API client — feeds the per-module report hub pages
 * (`/finance/reports`, `/warehouse/reports`, `/purchasing/reports`,
 * `/sales/reports`).
 *
 * Two backend shapes are normalised here into one `ReportCatalogEntry`:
 *  - inv/pur/sls: `GET /<mod>/reports` → registry listing `{key,title,group}`;
 *    the frontend route is always `/<base>/reports/<key>`.
 *  - finance:     `GET /fin/reports/catalog` → `{key,title,group,route,period}`;
 *    routes are NOT uniform (statements live on flat paths like
 *    `/finance/trial-balance`), so the backend supplies the route.
 */

import { apiGet } from './client';

/** A module whose reports are collected in a hub page. */
export type ReportModule = 'fin' | 'inv' | 'pur' | 'sls';

/** One card on a report hub page. */
export interface ReportCatalogEntry {
  key: string;
  title: string;
  /** Free-form backend group id, used to section the cards. */
  group: string;
  /** Frontend route that renders the report. */
  route: string;
}

/** Raw registry item from inv/pur/sls (`ReportCatalogItem`). */
interface RegistryItem {
  key: string;
  title: string;
  group: string;
}

/** Raw catalog item from finance (`FinReportCatalogItem`). */
interface FinanceItem extends RegistryItem {
  route: string;
  period?: string;
}

type CatalogResponse<T> = T[] | { success?: boolean; data: T[] };

/** The backend may wrap payloads in `{ data }` (ApiResponse) or return bare. */
function unwrap<T>(res: CatalogResponse<T>): T[] {
  if (Array.isArray(res)) return res;
  return Array.isArray(res?.data) ? res.data : [];
}

/** Per-module endpoint + route base for the registry-driven modules. */
const REGISTRY_MODULES: Record<
  Exclude<ReportModule, 'fin'>,
  { endpoint: string; routeBase: string }
> = {
  inv: { endpoint: '/inv/reports', routeBase: '/warehouse/reports' },
  pur: { endpoint: '/pur/reports', routeBase: '/purchasing/reports' },
  sls: { endpoint: '/sls/reports', routeBase: '/sales/reports' },
};

/**
 * Fetch the report catalog for one module, normalised to `ReportCatalogEntry`
 * and sorted by group then title so the hub renders deterministically.
 */
export async function getReportCatalog(
  module: ReportModule,
): Promise<ReportCatalogEntry[]> {
  const entries =
    module === 'fin' ? await fetchFinance() : await fetchRegistry(module);
  return entries.sort(
    (a, b) => a.group.localeCompare(b.group) || a.title.localeCompare(b.title),
  );
}

async function fetchFinance(): Promise<ReportCatalogEntry[]> {
  const res = await apiGet<CatalogResponse<FinanceItem>>(
    '/fin/reports/catalog',
  );
  return unwrap(res).map(({ key, title, group, route }) => ({
    key,
    title,
    group,
    route,
  }));
}

async function fetchRegistry(
  module: Exclude<ReportModule, 'fin'>,
): Promise<ReportCatalogEntry[]> {
  const { endpoint, routeBase } = REGISTRY_MODULES[module];
  const res = await apiGet<CatalogResponse<RegistryItem>>(endpoint);
  return unwrap(res).map(({ key, title, group }) => ({
    key,
    title,
    group,
    route: `${routeBase}/${key}`,
  }));
}
