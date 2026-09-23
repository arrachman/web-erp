// ERP Role resource API — CRUD for adm_roles
// Endpoints: /roles

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpRole {
  id: string;
  code: string;
  name: string;
  description?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateRolePayload {
  code: string;
  name: string;
  description?: string;
}

export interface UpdateRolePayload {
  code?: string;
  name?: string;
  description?: string;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listRoles(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpRole>> {
  return apiGet<PaginatedResponse<ErpRole>>('/roles', params as Record<string, string | number | boolean | undefined>);
}

export async function createRole(
  payload: CreateRolePayload,
): Promise<ErpRole> {
  const res = await apiPost<ApiResponse<ErpRole>>('/roles', payload);
  return res.data;
}

export async function updateRole(
  id: string,
  payload: UpdateRolePayload,
): Promise<ErpRole> {
  const res = await apiPatch<ApiResponse<ErpRole>>(`/roles/${id}`, payload);
  return res.data;
}

export async function deleteRole(id: string): Promise<void> {
  await apiDelete<void>(`/roles/${id}`);
}
