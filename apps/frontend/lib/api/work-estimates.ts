// ERP Work Estimate resource API — CRUD for md_work_estimates
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpWorkEstimate {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpWorkEstimatePayload {
  code: string;
  name: string;
  isActive?: boolean;
}

export type UpdateErpWorkEstimatePayload = Partial<CreateErpWorkEstimatePayload>;

const BASE = '/work-estimates';

export async function listWorkEstimates(params?: PaginationParams): Promise<PaginatedResponse<ErpWorkEstimate>> {
  return apiGet<PaginatedResponse<ErpWorkEstimate>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpWorkEstimate(payload: CreateErpWorkEstimatePayload): Promise<ErpWorkEstimate> {
  const res = await apiPost<ApiResponse<ErpWorkEstimate>>(BASE, payload);
  return res.data;
}

export async function updateErpWorkEstimate(id: string, payload: UpdateErpWorkEstimatePayload): Promise<ErpWorkEstimate> {
  const res = await apiPatch<ApiResponse<ErpWorkEstimate>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpWorkEstimate(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpWorkEstimateStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpWorkEstimates(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
