// ERP Province resource API — CRUD for md_provinces
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpProvince {
  id: string;
  code: string;
  name: string;
  countryId: string | null;
  countryName?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpProvincePayload {
  code: string;
  name: string;
  countryId: string;
  isActive?: boolean;
}

export type UpdateErpProvincePayload = Partial<CreateErpProvincePayload>;

const BASE = '/provinces';

export async function listProvinces(params?: PaginationParams & { countryId?: string }): Promise<PaginatedResponse<ErpProvince>> {
  return apiGet<PaginatedResponse<ErpProvince>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpProvince(payload: CreateErpProvincePayload): Promise<ErpProvince> {
  const res = await apiPost<ApiResponse<ErpProvince>>(BASE, payload);
  return res.data;
}

export async function updateErpProvince(id: string, payload: UpdateErpProvincePayload): Promise<ErpProvince> {
  const res = await apiPatch<ApiResponse<ErpProvince>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpProvince(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpProvinceStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpProvinces(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
