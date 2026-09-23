// ERP Country resource API — CRUD for md_countries
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpCountry {
  id: string;
  code: string;
  name: string;
  isoCode?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpCountryPayload {
  code: string;
  name: string;
  isoCode?: string;
  isActive?: boolean;
}

export type UpdateErpCountryPayload = Partial<CreateErpCountryPayload>;

const BASE = '/countries';

export async function listCountries(params?: PaginationParams): Promise<PaginatedResponse<ErpCountry>> {
  return apiGet<PaginatedResponse<ErpCountry>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpCountry(payload: CreateErpCountryPayload): Promise<ErpCountry> {
  const res = await apiPost<ApiResponse<ErpCountry>>(BASE, payload);
  return res.data;
}

export async function updateErpCountry(id: string, payload: UpdateErpCountryPayload): Promise<ErpCountry> {
  const res = await apiPatch<ApiResponse<ErpCountry>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpCountry(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpCountryStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpCountries(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
