// ERP Price Index resource API — CRUD for md_price_indices
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpPriceIndex {
  id: string;
  code: string;
  name: string;
  margin: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpPriceIndexPayload {
  code: string;
  name: string;
  margin?: number;
  notes?: string;
  isActive?: boolean;
}

export type UpdateErpPriceIndexPayload = Partial<CreateErpPriceIndexPayload>;

const BASE = '/price-indices';

export async function listPriceIndices(params?: PaginationParams): Promise<PaginatedResponse<ErpPriceIndex>> {
  return apiGet<PaginatedResponse<ErpPriceIndex>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpPriceIndex(payload: CreateErpPriceIndexPayload): Promise<ErpPriceIndex> {
  const res = await apiPost<ApiResponse<ErpPriceIndex>>(BASE, payload);
  return res.data;
}

export async function updateErpPriceIndex(id: string, payload: UpdateErpPriceIndexPayload): Promise<ErpPriceIndex> {
  const res = await apiPatch<ApiResponse<ErpPriceIndex>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpPriceIndex(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpPriceIndexStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpPriceIndices(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
