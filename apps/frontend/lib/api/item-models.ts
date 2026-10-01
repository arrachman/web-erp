// ERP Model resource API — CRUD for md_item_models
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpItemModel {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpItemModelPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpItemModelPayload = Partial<CreateErpItemModelPayload>;

const BASE = '/item-models';

export async function listItemModels(params?: PaginationParams): Promise<PaginatedResponse<ErpItemModel>> {
  return apiGet<PaginatedResponse<ErpItemModel>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpItemModel(payload: CreateErpItemModelPayload): Promise<ErpItemModel> {
  const res = await apiPost<ApiResponse<ErpItemModel>>(BASE, payload);
  return res.data;
}

export async function updateErpItemModel(id: string, payload: UpdateErpItemModelPayload): Promise<ErpItemModel> {
  const res = await apiPatch<ApiResponse<ErpItemModel>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpItemModel(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpItemModelStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpItemModels(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
