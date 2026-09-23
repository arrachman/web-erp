// ERP Branch resource API — CRUD for adm_branches (or sys_branches)
// Endpoints: /branches

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpBranch {
  id: string;
  code: string;
  name: string;
  addressLine1?: string | null;
  addressLine2?: string | null;
  city?: string | null;
  postalCode?: string | null;
  phone?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBranchPayload {
  code: string;
  name: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  postalCode?: string;
  phone?: string;
  isActive?: boolean;
}

export interface UpdateBranchPayload {
  code?: string;
  name?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  postalCode?: string;
  phone?: string;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listBranches(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpBranch>> {
  return apiGet<PaginatedResponse<ErpBranch>>('/branches', params as Record<string, string | number | boolean | undefined>);
}

export async function createBranch(
  payload: CreateBranchPayload,
): Promise<ErpBranch> {
  const res = await apiPost<ApiResponse<ErpBranch>>('/branches', payload);
  return res.data;
}

export async function updateBranch(
  id: string,
  payload: UpdateBranchPayload,
): Promise<ErpBranch> {
  const res = await apiPatch<ApiResponse<ErpBranch>>(`/branches/${id}`, payload);
  return res.data;
}

export async function deleteBranch(id: string): Promise<void> {
  await apiDelete<void>(`/branches/${id}`);
}

export async function bulkUpdateBranchStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/branches/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteBranches(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/branches/bulk', { ids });
  return { affected: res.affected };
}
