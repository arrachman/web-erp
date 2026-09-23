// ERP Warehouse resource API — CRUD for md_warehouses
// Endpoints: /warehouses

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpWarehouseLocationRef {
  id: string;
  code: string;
  name: string;
  branch?: { id: string; code: string; name: string } | null;
}

export interface ErpWarehouse {
  id: string;
  code: string;
  name: string;
  locationId: string;
  location?: ErpWarehouseLocationRef | null;
  allowNegativeStock: boolean;
  notes?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateWarehousePayload {
  code: string;
  name: string;
  locationId: string;
  allowNegativeStock?: boolean;
  notes?: string;
  isActive?: boolean;
}

export interface UpdateWarehousePayload {
  code?: string;
  name?: string;
  locationId?: string;
  allowNegativeStock?: boolean;
  notes?: string;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listWarehouses(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpWarehouse>> {
  return apiGet<PaginatedResponse<ErpWarehouse>>('/warehouses', params as Record<string, string | number | boolean | undefined>);
}

export async function createWarehouse(
  payload: CreateWarehousePayload,
): Promise<ErpWarehouse> {
  const res = await apiPost<ApiResponse<ErpWarehouse>>('/warehouses', payload);
  return res.data;
}

export async function updateWarehouse(
  id: string,
  payload: UpdateWarehousePayload,
): Promise<ErpWarehouse> {
  const res = await apiPatch<ApiResponse<ErpWarehouse>>(`/warehouses/${id}`, payload);
  return res.data;
}

export async function deleteWarehouse(id: string): Promise<void> {
  await apiDelete<void>(`/warehouses/${id}`);
}

export async function bulkUpdateWarehouseStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/warehouses/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteWarehouses(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/warehouses/bulk', { ids });
  return { affected: res.affected };
}
