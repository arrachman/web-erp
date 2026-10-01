// ERP Project resource API — CRUD for md_projects
// Endpoints: /projects

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpProject {
  id: string;
  code: string;
  name: string;
  parentId?: string | null;
  branchId?: string | null;
  startDate?: string | null;
  endDate?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProjectPayload {
  code: string;
  name: string;
  parentId?: string;
  branchId?: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

export interface UpdateProjectPayload {
  code?: string;
  name?: string;
  parentId?: string;
  branchId?: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

export async function listProjects(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpProject>> {
  return apiGet<PaginatedResponse<ErpProject>>(
    '/projects',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createProject(payload: CreateProjectPayload): Promise<ErpProject> {
  const res = await apiPost<ApiResponse<ErpProject>>('/projects', payload);
  return res.data;
}

export async function updateProject(id: string, payload: UpdateProjectPayload): Promise<ErpProject> {
  const res = await apiPatch<ApiResponse<ErpProject>>(`/projects/${id}`, payload);
  return res.data;
}

export async function deleteProject(id: string): Promise<void> {
  await apiDelete<void>(`/projects/${id}`);
}

export async function bulkUpdateProjectStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/projects/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteProjects(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/projects/bulk', { ids });
  return { affected: res.affected };
}
