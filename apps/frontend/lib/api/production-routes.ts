// ERP Production Route resource API — CRUD for md_production_routes
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpProductionRoute {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpProductionRoutePayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpProductionRoutePayload = Partial<CreateErpProductionRoutePayload>;

const BASE = '/production-routes';

export async function listProductionRoutes(params?: PaginationParams): Promise<PaginatedResponse<ErpProductionRoute>> {
  return apiGet<PaginatedResponse<ErpProductionRoute>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpProductionRoute(payload: CreateErpProductionRoutePayload): Promise<ErpProductionRoute> {
  const res = await apiPost<ApiResponse<ErpProductionRoute>>(BASE, payload);
  return res.data;
}

export async function updateErpProductionRoute(id: string, payload: UpdateErpProductionRoutePayload): Promise<ErpProductionRoute> {
  const res = await apiPatch<ApiResponse<ErpProductionRoute>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpProductionRoute(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpProductionRouteStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpProductionRoutes(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
