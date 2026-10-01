// ERP OEM resource API — CRUD for md_oems (legacy "Atribut" lookup)
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpOem {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpOemPayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpOemPayload = Partial<CreateErpOemPayload>;

const BASE = '/oems';

export async function listOems(params?: PaginationParams): Promise<PaginatedResponse<ErpOem>> {
  return apiGet<PaginatedResponse<ErpOem>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpOem(payload: CreateErpOemPayload): Promise<ErpOem> {
  const res = await apiPost<ApiResponse<ErpOem>>(BASE, payload);
  return res.data;
}

export async function updateErpOem(id: string, payload: UpdateErpOemPayload): Promise<ErpOem> {
  const res = await apiPatch<ApiResponse<ErpOem>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpOem(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpOemStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpOems(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
