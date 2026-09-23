// ERP Class resource API — CRUD for md_classes
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpClass {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpClassPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpClassPayload = Partial<CreateErpClassPayload>;

const BASE = '/classes';

export async function listClasses(params?: PaginationParams): Promise<PaginatedResponse<ErpClass>> {
  return apiGet<PaginatedResponse<ErpClass>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpClass(payload: CreateErpClassPayload): Promise<ErpClass> {
  const res = await apiPost<ApiResponse<ErpClass>>(BASE, payload);
  return res.data;
}

export async function updateErpClass(id: string, payload: UpdateErpClassPayload): Promise<ErpClass> {
  const res = await apiPatch<ApiResponse<ErpClass>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpClass(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpClassStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpClasses(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
