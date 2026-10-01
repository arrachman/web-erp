// ERP Sub-Area resource API — CRUD for md_sub_areas (kelurahan)
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpSubArea {
  id: string;
  code: string;
  name: string;
  areaId: string;
  postalCode?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpSubAreaPayload {
  code: string;
  name: string;
  areaId: string;
  postalCode?: string;
  isActive?: boolean;
}

export type UpdateErpSubAreaPayload = Partial<CreateErpSubAreaPayload>;

const BASE = '/sub-areas';

export async function listSubAreas(params?: PaginationParams & { areaId?: string }): Promise<PaginatedResponse<ErpSubArea>> {
  return apiGet<PaginatedResponse<ErpSubArea>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpSubArea(payload: CreateErpSubAreaPayload): Promise<ErpSubArea> {
  const res = await apiPost<ApiResponse<ErpSubArea>>(BASE, payload);
  return res.data;
}

export async function updateErpSubArea(id: string, payload: UpdateErpSubAreaPayload): Promise<ErpSubArea> {
  const res = await apiPatch<ApiResponse<ErpSubArea>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpSubArea(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpSubAreaStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpSubAreas(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
