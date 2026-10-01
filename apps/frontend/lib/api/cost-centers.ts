// ERP CostCenter resource API — CRUD for md_cost_centers
// Endpoints: /cost-centers

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpCostCenter {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCostCenterPayload {
  code: string;
  name: string;
  parentId?: string;
  isActive?: boolean;
}

export interface UpdateCostCenterPayload {
  code?: string;
  name?: string;
  parentId?: string;
  isActive?: boolean;
}

export async function listCostCenters(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpCostCenter>> {
  return apiGet<PaginatedResponse<ErpCostCenter>>(
    '/cost-centers',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createCostCenter(payload: CreateCostCenterPayload): Promise<ErpCostCenter> {
  const res = await apiPost<ApiResponse<ErpCostCenter>>('/cost-centers', payload);
  return res.data;
}

export async function updateCostCenter(id: string, payload: UpdateCostCenterPayload): Promise<ErpCostCenter> {
  const res = await apiPatch<ApiResponse<ErpCostCenter>>(`/cost-centers/${id}`, payload);
  return res.data;
}

export async function deleteCostCenter(id: string): Promise<void> {
  await apiDelete<void>(`/cost-centers/${id}`);
}

export async function bulkUpdateCostCenterStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/cost-centers/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteCostCenters(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/cost-centers/bulk', { ids });
  return { affected: res.affected };
}
