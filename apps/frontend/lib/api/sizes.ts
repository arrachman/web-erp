// ERP Size resource API — CRUD for md_sizes
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpSize {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpSizePayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpSizePayload = Partial<CreateErpSizePayload>;

const BASE = '/sizes';

export async function listSizes(params?: PaginationParams): Promise<PaginatedResponse<ErpSize>> {
  return apiGet<PaginatedResponse<ErpSize>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpSize(payload: CreateErpSizePayload): Promise<ErpSize> {
  const res = await apiPost<ApiResponse<ErpSize>>(BASE, payload);
  return res.data;
}

export async function updateErpSize(id: string, payload: UpdateErpSizePayload): Promise<ErpSize> {
  const res = await apiPatch<ApiResponse<ErpSize>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpSize(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpSizeStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpSizes(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
