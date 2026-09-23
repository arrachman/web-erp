// ERP Production Category resource API — CRUD for md_production_categories
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpProductionCategory {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpProductionCategoryPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpProductionCategoryPayload = Partial<CreateErpProductionCategoryPayload>;

const BASE = '/production-categories';

export async function listProductionCategories(params?: PaginationParams): Promise<PaginatedResponse<ErpProductionCategory>> {
  return apiGet<PaginatedResponse<ErpProductionCategory>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpProductionCategory(payload: CreateErpProductionCategoryPayload): Promise<ErpProductionCategory> {
  const res = await apiPost<ApiResponse<ErpProductionCategory>>(BASE, payload);
  return res.data;
}

export async function updateErpProductionCategory(id: string, payload: UpdateErpProductionCategoryPayload): Promise<ErpProductionCategory> {
  const res = await apiPatch<ApiResponse<ErpProductionCategory>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpProductionCategory(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpProductionCategoryStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpProductionCategories(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
