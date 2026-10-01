// ERP SubDivision resource API — CRUD for md_subdivisions
// Endpoints: /sub-divisions

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpSubDivision {
  id: string;
  code: string;
  name: string;
  divisionId: string;
  division?: { id: string; code: string; name: string } | null;
  parentId?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSubDivisionPayload {
  code: string;
  name: string;
  divisionId: string;
  parentId?: string;
  isActive?: boolean;
}

export interface UpdateSubDivisionPayload {
  code?: string;
  name?: string;
  divisionId?: string;
  parentId?: string;
  isActive?: boolean;
}

export async function listSubDivisions(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpSubDivision>> {
  return apiGet<PaginatedResponse<ErpSubDivision>>(
    '/sub-divisions',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createSubDivision(payload: CreateSubDivisionPayload): Promise<ErpSubDivision> {
  const res = await apiPost<ApiResponse<ErpSubDivision>>('/sub-divisions', payload);
  return res.data;
}

export async function updateSubDivision(id: string, payload: UpdateSubDivisionPayload): Promise<ErpSubDivision> {
  const res = await apiPatch<ApiResponse<ErpSubDivision>>(`/sub-divisions/${id}`, payload);
  return res.data;
}

export async function deleteSubDivision(id: string): Promise<void> {
  await apiDelete<void>(`/sub-divisions/${id}`);
}

export async function bulkUpdateSubDivisionStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/sub-divisions/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteSubDivisions(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/sub-divisions/bulk', { ids });
  return { affected: res.affected };
}
