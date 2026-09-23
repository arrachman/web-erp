// ERP Location resource API — CRUD for md_locations
// Endpoints: /locations

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpLocationBranchRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpLocation {
  id: string;
  code: string;
  name: string;
  branchId: string;
  branch?: ErpLocationBranchRef | null;
  addressLine1?: string | null;
  city?: string | null;
  postalCode?: string | null;
  phone?: string | null;
  notes?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLocationPayload {
  code: string;
  name: string;
  branchId: string;
  addressLine1?: string;
  city?: string;
  postalCode?: string;
  phone?: string;
  notes?: string;
  isActive?: boolean;
}

export interface UpdateLocationPayload {
  code?: string;
  name?: string;
  branchId?: string;
  addressLine1?: string;
  city?: string;
  postalCode?: string;
  phone?: string;
  notes?: string;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listLocations(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpLocation>> {
  return apiGet<PaginatedResponse<ErpLocation>>('/locations', params as Record<string, string | number | boolean | undefined>);
}

export async function createLocation(
  payload: CreateLocationPayload,
): Promise<ErpLocation> {
  const res = await apiPost<ApiResponse<ErpLocation>>('/locations', payload);
  return res.data;
}

export async function updateLocation(
  id: string,
  payload: UpdateLocationPayload,
): Promise<ErpLocation> {
  const res = await apiPatch<ApiResponse<ErpLocation>>(`/locations/${id}`, payload);
  return res.data;
}

export async function deleteLocation(id: string): Promise<void> {
  await apiDelete<void>(`/locations/${id}`);
}

export async function bulkUpdateLocationStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/locations/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteLocations(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/locations/bulk', { ids });
  return { affected: res.affected };
}
