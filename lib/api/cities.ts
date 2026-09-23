// ERP City resource API — CRUD for md_cities
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpCity {
  id: string;
  code: string;
  name: string;
  provinceId: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpCityPayload {
  code: string;
  name: string;
  provinceId: string;
  isActive?: boolean;
}

export type UpdateErpCityPayload = Partial<CreateErpCityPayload>;

const BASE = '/cities';

export async function listCities(params?: PaginationParams & { provinceId?: string }): Promise<PaginatedResponse<ErpCity>> {
  return apiGet<PaginatedResponse<ErpCity>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpCity(payload: CreateErpCityPayload): Promise<ErpCity> {
  const res = await apiPost<ApiResponse<ErpCity>>(BASE, payload);
  return res.data;
}

export async function updateErpCity(id: string, payload: UpdateErpCityPayload): Promise<ErpCity> {
  const res = await apiPatch<ApiResponse<ErpCity>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpCity(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpCityStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpCities(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
