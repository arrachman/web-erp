// ERP Section resource API — CRUD for md_sections
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpSection {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpSectionPayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpSectionPayload = Partial<CreateErpSectionPayload>;

const BASE = '/sections';

export async function listSections(params?: PaginationParams): Promise<PaginatedResponse<ErpSection>> {
  return apiGet<PaginatedResponse<ErpSection>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpSection(payload: CreateErpSectionPayload): Promise<ErpSection> {
  const res = await apiPost<ApiResponse<ErpSection>>(BASE, payload);
  return res.data;
}

export async function updateErpSection(id: string, payload: UpdateErpSectionPayload): Promise<ErpSection> {
  const res = await apiPatch<ApiResponse<ErpSection>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpSection(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpSectionStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpSections(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
