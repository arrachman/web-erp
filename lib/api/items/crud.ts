// ERP Item resource — CRUD for md_items
// Endpoints: /items

import { apiGet, apiPost, apiPatch, apiDelete } from '../client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from '../types';
import type { ErpItem, ErpItemType } from './types';
import type { CreateItemPayload, UpdateItemPayload } from './payloads';

export async function listItems(
  params?: PaginationParams & {
    itemType?: ErpItemType;
    kindId?: string;
    productClassId?: string;
    branchId?: string;
    defaultWarehouseId?: string;
  },
): Promise<PaginatedResponse<ErpItem>> {
  return apiGet<PaginatedResponse<ErpItem>>('/items', params as Record<string, string | number | boolean | undefined>);
}

export async function createItem(
  payload: CreateItemPayload,
): Promise<ErpItem> {
  const res = await apiPost<ApiResponse<ErpItem>>('/items', payload);
  return res.data;
}

export async function updateItem(
  id: string,
  payload: UpdateItemPayload,
): Promise<ErpItem> {
  const res = await apiPatch<ApiResponse<ErpItem>>(`/items/${id}`, payload);
  return res.data;
}

export async function deleteItem(id: string): Promise<void> {
  await apiDelete<void>(`/items/${id}`);
}

export async function bulkUpdateItemStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/items/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteItems(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/items/bulk', { ids });
  return { affected: res.affected };
}