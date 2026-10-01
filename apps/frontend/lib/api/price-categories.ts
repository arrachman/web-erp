// ERP Price Category resource API — CRUD for md_price_categories
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpPriceCategory {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpPriceCategoryPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpPriceCategoryPayload = Partial<CreateErpPriceCategoryPayload>;

const BASE = '/price-categories';

export async function listPriceCategories(params?: PaginationParams): Promise<PaginatedResponse<ErpPriceCategory>> {
  return apiGet<PaginatedResponse<ErpPriceCategory>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpPriceCategory(payload: CreateErpPriceCategoryPayload): Promise<ErpPriceCategory> {
  const res = await apiPost<ApiResponse<ErpPriceCategory>>(BASE, payload);
  return res.data;
}

export async function updateErpPriceCategory(id: string, payload: UpdateErpPriceCategoryPayload): Promise<ErpPriceCategory> {
  const res = await apiPatch<ApiResponse<ErpPriceCategory>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpPriceCategory(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpPriceCategoryStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpPriceCategories(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
