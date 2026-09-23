// ERP Production Activity resource API — CRUD for md_production_activities
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpProductionActivity {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpProductionActivityPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpProductionActivityPayload = Partial<CreateErpProductionActivityPayload>;

const BASE = '/production-activities';

export async function listProductionActivities(params?: PaginationParams): Promise<PaginatedResponse<ErpProductionActivity>> {
  return apiGet<PaginatedResponse<ErpProductionActivity>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpProductionActivity(payload: CreateErpProductionActivityPayload): Promise<ErpProductionActivity> {
  const res = await apiPost<ApiResponse<ErpProductionActivity>>(BASE, payload);
  return res.data;
}

export async function updateErpProductionActivity(id: string, payload: UpdateErpProductionActivityPayload): Promise<ErpProductionActivity> {
  const res = await apiPatch<ApiResponse<ErpProductionActivity>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpProductionActivity(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpProductionActivityStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpProductionActivities(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
