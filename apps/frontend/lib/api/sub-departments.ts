// ERP SubDepartment resource API — CRUD for md_sub_departments
// Endpoints: /sub-departments

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpSubDepartment {
  id: string;
  code: string;
  name: string;
  departmentId: string;
  department?: { id: string; code: string; name: string } | null;
  parentId?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSubDepartmentPayload {
  code: string;
  name: string;
  departmentId: string;
  parentId?: string;
  isActive?: boolean;
}

export interface UpdateSubDepartmentPayload {
  code?: string;
  name?: string;
  departmentId?: string;
  parentId?: string;
  isActive?: boolean;
}

export async function listSubDepartments(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpSubDepartment>> {
  return apiGet<PaginatedResponse<ErpSubDepartment>>(
    '/sub-departments',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createSubDepartment(payload: CreateSubDepartmentPayload): Promise<ErpSubDepartment> {
  const res = await apiPost<ApiResponse<ErpSubDepartment>>('/sub-departments', payload);
  return res.data;
}

export async function updateSubDepartment(id: string, payload: UpdateSubDepartmentPayload): Promise<ErpSubDepartment> {
  const res = await apiPatch<ApiResponse<ErpSubDepartment>>(`/sub-departments/${id}`, payload);
  return res.data;
}

export async function deleteSubDepartment(id: string): Promise<void> {
  await apiDelete<void>(`/sub-departments/${id}`);
}

export async function bulkUpdateSubDepartmentStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/sub-departments/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteSubDepartments(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/sub-departments/bulk', { ids });
  return { affected: res.affected };
}
