// ERP Department resource API — CRUD for md_departments
// Endpoints: /departments

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpDepartment {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDepartmentPayload {
  code: string;
  name: string;
  parentId?: string;
  isActive?: boolean;
}

export interface UpdateDepartmentPayload {
  code?: string;
  name?: string;
  parentId?: string;
  isActive?: boolean;
}

export async function listDepartments(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpDepartment>> {
  return apiGet<PaginatedResponse<ErpDepartment>>(
    '/departments',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createDepartment(payload: CreateDepartmentPayload): Promise<ErpDepartment> {
  const res = await apiPost<ApiResponse<ErpDepartment>>('/departments', payload);
  return res.data;
}

export async function updateDepartment(id: string, payload: UpdateDepartmentPayload): Promise<ErpDepartment> {
  const res = await apiPatch<ApiResponse<ErpDepartment>>(`/departments/${id}`, payload);
  return res.data;
}

export async function deleteDepartment(id: string): Promise<void> {
  await apiDelete<void>(`/departments/${id}`);
}

export async function bulkUpdateDepartmentStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/departments/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteDepartments(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/departments/bulk', { ids });
  return { affected: res.affected };
}
