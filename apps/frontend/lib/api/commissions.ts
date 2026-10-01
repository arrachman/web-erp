// ERP Commission resource API — CRUD for md_commissions
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpCommission {
  id: string;
  code: string;
  name: string;
  amount?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpCommissionPayload {
  code: string;
  name: string;
  amount?: string;
  isActive?: boolean;
}

export type UpdateErpCommissionPayload = Partial<CreateErpCommissionPayload>;

const BASE = '/commissions';

export async function listCommissions(params?: PaginationParams): Promise<PaginatedResponse<ErpCommission>> {
  return apiGet<PaginatedResponse<ErpCommission>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpCommission(payload: CreateErpCommissionPayload): Promise<ErpCommission> {
  const res = await apiPost<ApiResponse<ErpCommission>>(BASE, payload);
  return res.data;
}

export async function updateErpCommission(id: string, payload: UpdateErpCommissionPayload): Promise<ErpCommission> {
  const res = await apiPatch<ApiResponse<ErpCommission>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpCommission(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpCommissionStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpCommissions(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
