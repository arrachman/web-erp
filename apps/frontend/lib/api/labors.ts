// ERP Labor resource API — CRUD for md_labors
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpLabor {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpLaborPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpLaborPayload = Partial<CreateErpLaborPayload>;

const BASE = '/labors';

export async function listLabors(params?: PaginationParams): Promise<PaginatedResponse<ErpLabor>> {
  return apiGet<PaginatedResponse<ErpLabor>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpLabor(payload: CreateErpLaborPayload): Promise<ErpLabor> {
  const res = await apiPost<ApiResponse<ErpLabor>>(BASE, payload);
  return res.data;
}

export async function updateErpLabor(id: string, payload: UpdateErpLaborPayload): Promise<ErpLabor> {
  const res = await apiPatch<ApiResponse<ErpLabor>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpLabor(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpLaborStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpLabors(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
