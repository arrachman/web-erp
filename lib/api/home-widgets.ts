// ERP Home Widget resource API — CRUD for sys_home_widgets (dashboard layout).
// Endpoint: /home-widgets
//
// The backend entity uses `enabled` for its boolean status, but SimpleMasterPage
// (BaseEntity) expects `isActive` + `code` + `name`. This client aliases those:
// list rows are decorated with code=widgetKey, name=title, isActive=enabled, and
// the isActive query param / bulk-status body are translated to `enabled`.

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpHomeWidget {
  id: string;
  widgetKey: string;
  title: string;
  description?: string | null;
  enabled: boolean;
  sortOrder: number;
  colSpan: number;
  config?: Record<string, unknown> | null;
  createdAt: string;
  updatedAt: string;
  // SimpleMasterPage (BaseEntity) aliases — injected client-side by listHomeWidgets.
  code: string;
  name: string;
  isActive: boolean;
}

export interface HomeWidgetForm {
  id?: string;
  widgetKey: string;
  title: string;
  description: string;
  enabled: boolean;
  sortOrder: number;
  colSpan: number;
}

export interface HomeWidgetPayload {
  widgetKey: string;
  title: string;
  description?: string;
  enabled: boolean;
  sortOrder: number;
  colSpan: number;
}

// Raw shape returned by the API (before BaseEntity aliasing).
type RawHomeWidget = Omit<ErpHomeWidget, 'code' | 'name' | 'isActive'>;

const withAliases = (row: RawHomeWidget): ErpHomeWidget => ({
  ...row,
  code: row.widgetKey,
  name: row.title,
  isActive: row.enabled,
});

// ─── API functions ────────────────────────────────────────────────────────────

export async function listHomeWidgets(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpHomeWidget>> {
  // Backend filters on `enabled`, not `isActive` — translate before forwarding.
  const { isActive, ...rest } = params ?? {};
  const query: Record<string, string | number | boolean | undefined> = { ...rest };
  if (isActive !== undefined) query.enabled = isActive;

  const res = await apiGet<PaginatedResponse<RawHomeWidget>>('/home-widgets', query);
  return { ...res, data: res.data.map(withAliases) };
}

export async function createHomeWidget(
  payload: HomeWidgetPayload,
): Promise<ErpHomeWidget> {
  const res = await apiPost<ApiResponse<RawHomeWidget>>('/home-widgets', payload);
  return withAliases(res.data);
}

export async function updateHomeWidget(
  id: string,
  payload: HomeWidgetPayload,
): Promise<ErpHomeWidget> {
  const res = await apiPatch<ApiResponse<RawHomeWidget>>(`/home-widgets/${id}`, payload);
  return withAliases(res.data);
}

export async function deleteHomeWidget(id: string): Promise<void> {
  await apiDelete<void>(`/home-widgets/${id}`);
}

export async function bulkUpdateErpHomeWidgetStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/home-widgets/bulk/status',
    { ids, enabled: isActive },
  );
  return { affected: res.affected };
}

export async function bulkDeleteErpHomeWidgets(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/home-widgets/bulk',
    { ids },
  );
  return { affected: res.affected };
}
