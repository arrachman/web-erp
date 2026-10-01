// ERP Sub Class resource API — CRUD for md_sub_classes
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpSubClass {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpSubClassPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpSubClassPayload = Partial<CreateErpSubClassPayload>;

const BASE = '/sub-classes';

export async function listSubClasses(params?: PaginationParams): Promise<PaginatedResponse<ErpSubClass>> {
  return apiGet<PaginatedResponse<ErpSubClass>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpSubClass(payload: CreateErpSubClassPayload): Promise<ErpSubClass> {
  const res = await apiPost<ApiResponse<ErpSubClass>>(BASE, payload);
  return res.data;
}

export async function updateErpSubClass(id: string, payload: UpdateErpSubClassPayload): Promise<ErpSubClass> {
  const res = await apiPatch<ApiResponse<ErpSubClass>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpSubClass(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpSubClassStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpSubClasses(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
