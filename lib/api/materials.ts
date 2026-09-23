// ERP Material resource API — CRUD for md_materials
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpMaterial {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpMaterialPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpMaterialPayload = Partial<CreateErpMaterialPayload>;

const BASE = '/materials';

export async function listMaterials(params?: PaginationParams): Promise<PaginatedResponse<ErpMaterial>> {
  return apiGet<PaginatedResponse<ErpMaterial>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpMaterial(payload: CreateErpMaterialPayload): Promise<ErpMaterial> {
  const res = await apiPost<ApiResponse<ErpMaterial>>(BASE, payload);
  return res.data;
}

export async function updateErpMaterial(id: string, payload: UpdateErpMaterialPayload): Promise<ErpMaterial> {
  const res = await apiPatch<ApiResponse<ErpMaterial>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpMaterial(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpMaterialStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpMaterials(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
