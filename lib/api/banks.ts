// ERP Bank resource API — CRUD for md_banks
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpBank {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpBankPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpBankPayload = Partial<CreateErpBankPayload>;

const BASE = '/banks';

export async function listBanks(params?: PaginationParams): Promise<PaginatedResponse<ErpBank>> {
  return apiGet<PaginatedResponse<ErpBank>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpBank(payload: CreateErpBankPayload): Promise<ErpBank> {
  const res = await apiPost<ApiResponse<ErpBank>>(BASE, payload);
  return res.data;
}

export async function updateErpBank(id: string, payload: UpdateErpBankPayload): Promise<ErpBank> {
  const res = await apiPatch<ApiResponse<ErpBank>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpBank(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpBankStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpBanks(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
