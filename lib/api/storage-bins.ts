// ERP Storage Bin resource API — CRUD for md_storage_bins (lokasi gudang hierarkis: zona → rak → bin)
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export type ErpBinType = 'ZONE' | 'RACK' | 'BIN';

export interface ErpStorageBin {
  id: string;
  code: string;
  name: string;
  warehouseId: string;
  parentId?: string | null;
  binType: ErpBinType;
  notes?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  warehouse?: { id: string; code: string; name: string } | null;
  parent?: { id: string; code: string; name: string; binType: ErpBinType } | null;
}

export interface CreateErpStorageBinPayload {
  code: string;
  name: string;
  warehouseId: string;
  parentId?: string;
  binType?: ErpBinType;
  notes?: string;
  isActive?: boolean;
}

export type UpdateErpStorageBinPayload = Partial<CreateErpStorageBinPayload>;

export interface StorageBinListParams extends PaginationParams {
  warehouseId?: string;
  parentId?: string;
  binType?: ErpBinType;
}

const BASE = '/storage-bins';

export async function listStorageBins(params?: StorageBinListParams): Promise<PaginatedResponse<ErpStorageBin>> {
  return apiGet<PaginatedResponse<ErpStorageBin>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function listStorageBinTree(warehouseId: string): Promise<ErpStorageBin[]> {
  const res = await apiGet<ApiResponse<ErpStorageBin[]>>(`${BASE}/tree/${warehouseId}`);
  return res.data;
}

export async function createErpStorageBin(payload: CreateErpStorageBinPayload): Promise<ErpStorageBin> {
  const res = await apiPost<ApiResponse<ErpStorageBin>>(BASE, payload);
  return res.data;
}

export async function updateErpStorageBin(id: string, payload: UpdateErpStorageBinPayload): Promise<ErpStorageBin> {
  const res = await apiPatch<ApiResponse<ErpStorageBin>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpStorageBin(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpStorageBinStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpStorageBins(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
