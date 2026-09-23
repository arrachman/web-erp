// ERP Machine resource API — CRUD for md_machines
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpMachine {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpMachinePayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpMachinePayload = Partial<CreateErpMachinePayload>;

const BASE = '/machines';

export async function listMachines(params?: PaginationParams): Promise<PaginatedResponse<ErpMachine>> {
  return apiGet<PaginatedResponse<ErpMachine>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpMachine(payload: CreateErpMachinePayload): Promise<ErpMachine> {
  const res = await apiPost<ApiResponse<ErpMachine>>(BASE, payload);
  return res.data;
}

export async function updateErpMachine(id: string, payload: UpdateErpMachinePayload): Promise<ErpMachine> {
  const res = await apiPatch<ApiResponse<ErpMachine>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpMachine(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpMachineStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpMachines(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
