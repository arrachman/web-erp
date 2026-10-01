// ERP Division resource API — CRUD for md_divisions
// Endpoints: /divisions

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpDivision {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  isActive: boolean;
  barcode?: string | null;
  note?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDivisionPayload {
  code: string;
  name: string;
  parentId?: string;
  isActive?: boolean;
  barcode?: string;
  note?: string;
}

export interface UpdateDivisionPayload {
  code?: string;
  name?: string;
  parentId?: string;
  isActive?: boolean;
  barcode?: string;
  note?: string;
}

export async function listDivisions(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpDivision>> {
  return apiGet<PaginatedResponse<ErpDivision>>(
    '/divisions',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createDivision(payload: CreateDivisionPayload): Promise<ErpDivision> {
  const res = await apiPost<ApiResponse<ErpDivision>>('/divisions', payload);
  return res.data;
}

export async function updateDivision(id: string, payload: UpdateDivisionPayload): Promise<ErpDivision> {
  const res = await apiPatch<ApiResponse<ErpDivision>>(`/divisions/${id}`, payload);
  return res.data;
}

export async function deleteDivision(id: string): Promise<void> {
  await apiDelete<void>(`/divisions/${id}`);
}

export async function bulkUpdateDivisionStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/divisions/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteDivisions(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/divisions/bulk', { ids });
  return { affected: res.affected };
}
