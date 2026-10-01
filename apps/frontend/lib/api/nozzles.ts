// ERP Nozzle resource API — CRUD for md_nozzles (legacy "Atribut" lookup)
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpNozzle {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpNozzlePayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpNozzlePayload = Partial<CreateErpNozzlePayload>;

const BASE = '/nozzles';

export async function listNozzles(params?: PaginationParams): Promise<PaginatedResponse<ErpNozzle>> {
  return apiGet<PaginatedResponse<ErpNozzle>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpNozzle(payload: CreateErpNozzlePayload): Promise<ErpNozzle> {
  const res = await apiPost<ApiResponse<ErpNozzle>>(BASE, payload);
  return res.data;
}

export async function updateErpNozzle(id: string, payload: UpdateErpNozzlePayload): Promise<ErpNozzle> {
  const res = await apiPatch<ApiResponse<ErpNozzle>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpNozzle(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpNozzleStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpNozzles(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
