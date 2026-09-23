// ERP Brand resource API — CRUD for md_brands
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpBrand {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpBrandPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpBrandPayload = Partial<CreateErpBrandPayload>;

const BASE = '/brands';

export async function listBrands(params?: PaginationParams): Promise<PaginatedResponse<ErpBrand>> {
  return apiGet<PaginatedResponse<ErpBrand>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpBrand(payload: CreateErpBrandPayload): Promise<ErpBrand> {
  const res = await apiPost<ApiResponse<ErpBrand>>(BASE, payload);
  return res.data;
}

export async function updateErpBrand(id: string, payload: UpdateErpBrandPayload): Promise<ErpBrand> {
  const res = await apiPatch<ApiResponse<ErpBrand>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpBrand(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpBrandStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpBrands(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
