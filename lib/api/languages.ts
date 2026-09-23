// API client for sys_languages — CRUD + bulk operations
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpLanguage {
  id: string;
  code: string;
  name: string;
  nativeName: string;
  isRtl: boolean;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpLanguagePayload {
  code: string;
  name: string;
  nativeName: string;
  isRtl?: boolean;
  isDefault?: boolean;
  isActive?: boolean;
}

export type UpdateErpLanguagePayload = Partial<CreateErpLanguagePayload>;

const BASE = '/languages';

export async function listLanguages(params?: PaginationParams): Promise<PaginatedResponse<ErpLanguage>> {
  return apiGet<PaginatedResponse<ErpLanguage>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpLanguage(payload: CreateErpLanguagePayload): Promise<ErpLanguage> {
  const res = await apiPost<ApiResponse<ErpLanguage>>(BASE, payload);
  return res.data;
}

export async function updateErpLanguage(id: string, payload: UpdateErpLanguagePayload): Promise<ErpLanguage> {
  const res = await apiPatch<ApiResponse<ErpLanguage>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpLanguage(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpLanguageStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpLanguages(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
