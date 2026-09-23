// ERP Expedition resource API — CRUD for md_expeditions
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpExpedition {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpExpeditionPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpExpeditionPayload = Partial<CreateErpExpeditionPayload>;

const BASE = '/expeditions';

export async function listExpeditions(params?: PaginationParams): Promise<PaginatedResponse<ErpExpedition>> {
  return apiGet<PaginatedResponse<ErpExpedition>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpExpedition(payload: CreateErpExpeditionPayload): Promise<ErpExpedition> {
  const res = await apiPost<ApiResponse<ErpExpedition>>(BASE, payload);
  return res.data;
}

export async function updateErpExpedition(id: string, payload: UpdateErpExpeditionPayload): Promise<ErpExpedition> {
  const res = await apiPatch<ApiResponse<ErpExpedition>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpExpedition(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpExpeditionStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpExpeditions(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
