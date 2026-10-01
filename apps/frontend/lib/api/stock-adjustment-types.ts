// ERP Stock Adjustment Type resource API — CRUD for md_stock_adjustment_types
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface StockAdjustmentTypeAccountSummary {
  id: string;
  code: string;
  name: string;
}

export interface ErpStockAdjustmentType {
  id: string;
  code: string;
  name: string;
  direction?: string | null;
  accountId?: string | null;
  account?: StockAdjustmentTypeAccountSummary | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpStockAdjustmentTypePayload {
  code: string;
  name: string;
  direction?: string;
  accountId?: string | null;
  isActive?: boolean;
}

export type UpdateErpStockAdjustmentTypePayload = Partial<CreateErpStockAdjustmentTypePayload>;

const BASE = '/stock-adjustment-types';

export async function listStockAdjustmentTypes(params?: PaginationParams): Promise<PaginatedResponse<ErpStockAdjustmentType>> {
  return apiGet<PaginatedResponse<ErpStockAdjustmentType>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpStockAdjustmentType(payload: CreateErpStockAdjustmentTypePayload): Promise<ErpStockAdjustmentType> {
  const res = await apiPost<ApiResponse<ErpStockAdjustmentType>>(BASE, payload);
  return res.data;
}

export async function updateErpStockAdjustmentType(id: string, payload: UpdateErpStockAdjustmentTypePayload): Promise<ErpStockAdjustmentType> {
  const res = await apiPatch<ApiResponse<ErpStockAdjustmentType>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpStockAdjustmentType(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpStockAdjustmentTypeStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpStockAdjustmentTypes(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
