// ERP Other Cost resource API — CRUD for md_other_costs
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface OtherCostAccountSummary {
  id: string;
  code: string;
  name: string;
}

export interface ErpOtherCost {
  id: string;
  code: string;
  name: string;
  debitAccountId?: string | null;
  creditAccountId?: string | null;
  debitAccount?: OtherCostAccountSummary | null;
  creditAccount?: OtherCostAccountSummary | null;
  isHPP: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpOtherCostPayload {
  code: string;
  name: string;
  debitAccountId?: string | null;
  creditAccountId?: string | null;
  isHPP?: boolean;
  isActive?: boolean;
}

export type UpdateErpOtherCostPayload = Partial<CreateErpOtherCostPayload>;

const BASE = '/other-costs';

export async function listOtherCosts(params?: PaginationParams): Promise<PaginatedResponse<ErpOtherCost>> {
  return apiGet<PaginatedResponse<ErpOtherCost>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpOtherCost(payload: CreateErpOtherCostPayload): Promise<ErpOtherCost> {
  const res = await apiPost<ApiResponse<ErpOtherCost>>(BASE, payload);
  return res.data;
}

export async function updateErpOtherCost(id: string, payload: UpdateErpOtherCostPayload): Promise<ErpOtherCost> {
  const res = await apiPatch<ApiResponse<ErpOtherCost>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpOtherCost(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpOtherCostStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpOtherCosts(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
