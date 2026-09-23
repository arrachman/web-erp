// ERP Product Class resource API — CRUD for md_product_classes
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpProductClass {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpProductClassPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpProductClassPayload = Partial<CreateErpProductClassPayload>;

const BASE = '/product-classes';

export async function listProductClasses(params?: PaginationParams): Promise<PaginatedResponse<ErpProductClass>> {
  return apiGet<PaginatedResponse<ErpProductClass>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpProductClass(payload: CreateErpProductClassPayload): Promise<ErpProductClass> {
  const res = await apiPost<ApiResponse<ErpProductClass>>(BASE, payload);
  return res.data;
}

export async function updateErpProductClass(id: string, payload: UpdateErpProductClassPayload): Promise<ErpProductClass> {
  const res = await apiPatch<ApiResponse<ErpProductClass>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpProductClass(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpProductClassStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpProductClasses(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
