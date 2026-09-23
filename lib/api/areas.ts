// ERP Area resource API — CRUD for md_areas
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpArea {
  id: string;
  code: string;
  name: string;
  cityId: string;
  postalCode?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpAreaPayload {
  code: string;
  name: string;
  cityId: string;
  isActive?: boolean;
}

export type UpdateErpAreaPayload = Partial<CreateErpAreaPayload>;

const BASE = '/areas';

export async function listAreas(params?: PaginationParams & { cityId?: string }): Promise<PaginatedResponse<ErpArea>> {
  return apiGet<PaginatedResponse<ErpArea>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpArea(payload: CreateErpAreaPayload): Promise<ErpArea> {
  const res = await apiPost<ApiResponse<ErpArea>>(BASE, payload);
  return res.data;
}

export async function updateErpArea(id: string, payload: UpdateErpAreaPayload): Promise<ErpArea> {
  const res = await apiPatch<ApiResponse<ErpArea>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpArea(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpAreaStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpAreas(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
