// ERP User resource API — CRUD for adm_users
// Endpoints: /users

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpUserLevel =
  | 'POS'
  | 'CENTRAL'
  | 'POS_AND_CENTRAL'
  | 'BI'
  | 'BI_AND_CENTRAL';

export interface ErpUserRoleRef {
  role: { id: string; code: string; name: string };
}

export interface ErpUser {
  id: string;
  username: string;
  fullName: string;
  email?: string | null;
  language: string;
  erpLevel: ErpUserLevel;
  homeBranchId?: string | null;
  homeWarehouseId?: string | null;
  expiresAt?: string | null;
  isActive: boolean;
  roles?: ErpUserRoleRef[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateUserPayload {
  username: string;
  fullName: string;
  email?: string;
  password: string;
  erpLevel: ErpUserLevel;
  language?: string;
  branchId?: string;
  homeWarehouseId?: string;
  expiresAt?: string;
  isActive?: boolean;
  roleIds?: string[];
}

export interface UpdateUserPayload {
  username?: string;
  fullName?: string;
  email?: string;
  password?: string;
  erpLevel?: ErpUserLevel;
  language?: string;
  branchId?: string;
  homeWarehouseId?: string;
  expiresAt?: string | null;
  isActive?: boolean;
  roleIds?: string[];
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listUsers(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpUser>> {
  return apiGet<PaginatedResponse<ErpUser>>('/users', params as Record<string, string | number | boolean | undefined>);
}

export async function getUser(id: string): Promise<ErpUser> {
  const res = await apiGet<ApiResponse<ErpUser>>(`/users/${id}`);
  return res.data;
}

export async function createUser(
  payload: CreateUserPayload,
): Promise<ErpUser> {
  const res = await apiPost<ApiResponse<ErpUser>>('/users', payload);
  return res.data;
}

export async function updateUser(
  id: string,
  payload: UpdateUserPayload,
): Promise<ErpUser> {
  const res = await apiPatch<ApiResponse<ErpUser>>(`/users/${id}`, payload);
  return res.data;
}

export async function deleteUser(id: string): Promise<void> {
  await apiDelete<void>(`/users/${id}`);
}
