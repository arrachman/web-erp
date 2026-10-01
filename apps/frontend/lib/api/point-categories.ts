// ERP Point Category resource API — CRUD for md_point_categories
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpPointCategory {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpPointCategoryPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpPointCategoryPayload = Partial<CreateErpPointCategoryPayload>;

const BASE = '/point-categories';

export async function listPointCategories(params?: PaginationParams): Promise<PaginatedResponse<ErpPointCategory>> {
  return apiGet<PaginatedResponse<ErpPointCategory>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpPointCategory(payload: CreateErpPointCategoryPayload): Promise<ErpPointCategory> {
  const res = await apiPost<ApiResponse<ErpPointCategory>>(BASE, payload);
  return res.data;
}

export async function updateErpPointCategory(id: string, payload: UpdateErpPointCategoryPayload): Promise<ErpPointCategory> {
  const res = await apiPatch<ApiResponse<ErpPointCategory>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpPointCategory(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpPointCategoryStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpPointCategories(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
