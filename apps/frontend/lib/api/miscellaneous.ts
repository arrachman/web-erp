// ERP Miscellaneous resource API — CRUD for md_miscellaneous
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpMiscellaneous {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpMiscellaneousPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpMiscellaneousPayload = Partial<CreateErpMiscellaneousPayload>;

const BASE = '/miscellaneous';

export async function listMiscellaneous(params?: PaginationParams): Promise<PaginatedResponse<ErpMiscellaneous>> {
  return apiGet<PaginatedResponse<ErpMiscellaneous>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpMiscellaneous(payload: CreateErpMiscellaneousPayload): Promise<ErpMiscellaneous> {
  const res = await apiPost<ApiResponse<ErpMiscellaneous>>(BASE, payload);
  return res.data;
}

export async function updateErpMiscellaneous(id: string, payload: UpdateErpMiscellaneousPayload): Promise<ErpMiscellaneous> {
  const res = await apiPatch<ApiResponse<ErpMiscellaneous>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpMiscellaneous(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpMiscellaneousStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpMiscellaneous(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
