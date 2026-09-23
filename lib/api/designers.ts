// ERP Designer resource API — CRUD for md_designers
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpDesigner {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpDesignerPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpDesignerPayload = Partial<CreateErpDesignerPayload>;

const BASE = '/designers';

export async function listDesigners(params?: PaginationParams): Promise<PaginatedResponse<ErpDesigner>> {
  return apiGet<PaginatedResponse<ErpDesigner>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpDesigner(payload: CreateErpDesignerPayload): Promise<ErpDesigner> {
  const res = await apiPost<ApiResponse<ErpDesigner>>(BASE, payload);
  return res.data;
}

export async function updateErpDesigner(id: string, payload: UpdateErpDesignerPayload): Promise<ErpDesigner> {
  const res = await apiPatch<ApiResponse<ErpDesigner>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpDesigner(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpDesignerStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpDesigners(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
