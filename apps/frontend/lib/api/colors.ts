// ERP Color resource API — CRUD for md_colors
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpColor {
  id: string;
  code: string;
  name: string;
  hexCode?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpColorPayload {
  code: string;
  name: string;
  hexCode?: string;
  isActive?: boolean;
}

export type UpdateErpColorPayload = Partial<CreateErpColorPayload>;

const BASE = '/colors';

export async function listColors(params?: PaginationParams): Promise<PaginatedResponse<ErpColor>> {
  return apiGet<PaginatedResponse<ErpColor>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpColor(payload: CreateErpColorPayload): Promise<ErpColor> {
  const res = await apiPost<ApiResponse<ErpColor>>(BASE, payload);
  return res.data;
}

export async function updateErpColor(id: string, payload: UpdateErpColorPayload): Promise<ErpColor> {
  const res = await apiPatch<ApiResponse<ErpColor>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpColor(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpColorStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpColors(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
