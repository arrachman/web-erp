// ERP Item Type resource API — CRUD for md_item_types
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpItemKind {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpItemKindPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpItemKindPayload = Partial<CreateErpItemKindPayload>;

const BASE = '/item-types';

export async function listItemKinds(params?: PaginationParams): Promise<PaginatedResponse<ErpItemKind>> {
  return apiGet<PaginatedResponse<ErpItemKind>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpItemKind(payload: CreateErpItemKindPayload): Promise<ErpItemKind> {
  const res = await apiPost<ApiResponse<ErpItemKind>>(BASE, payload);
  return res.data;
}

export async function updateErpItemKind(id: string, payload: UpdateErpItemKindPayload): Promise<ErpItemKind> {
  const res = await apiPatch<ApiResponse<ErpItemKind>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpItemKind(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpItemKindStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpItemKinds(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
