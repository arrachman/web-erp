// ERP Item Category resource API — CRUD for md_item_categories
// Endpoints: /item-categories

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpAccountRef {
  id: string;
  code: string;
  name: string;
}

/** 8 GL account FK fields on an item category (legacy MyERP+ parity). */
export interface ItemCategoryAccountIds {
  inventoryAccountId?: string | null;
  cogsAccountId?: string | null;
  salesAccountId?: string | null;
  salesReturnAccountId?: string | null;
  salesDiscountAccountId?: string | null;
  purchaseReturnAccountId?: string | null;
  purchaseDiscountAccountId?: string | null;
  consignmentAccountId?: string | null;
}

export interface ErpItemCategory extends ItemCategoryAccountIds {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  parentId?: string | null;
  createdAt: string;
  updatedAt: string;
  parent?: { id: string; name: string } | null;
  inventoryAccount?: ErpAccountRef | null;
  cogsAccount?: ErpAccountRef | null;
  salesAccount?: ErpAccountRef | null;
  salesReturnAccount?: ErpAccountRef | null;
  salesDiscountAccount?: ErpAccountRef | null;
  purchaseReturnAccount?: ErpAccountRef | null;
  purchaseDiscountAccount?: ErpAccountRef | null;
  consignmentAccount?: ErpAccountRef | null;
}

export interface CreateItemCategoryPayload extends ItemCategoryAccountIds {
  code: string;
  name: string;
  isActive?: boolean;
  parentId?: string | null;
}

export interface UpdateItemCategoryPayload extends ItemCategoryAccountIds {
  code?: string;
  name?: string;
  parentId?: string | null;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listItemCategories(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpItemCategory>> {
  return apiGet<PaginatedResponse<ErpItemCategory>>('/item-categories', params as Record<string, string | number | boolean | undefined>);
}

export async function createItemCategory(
  payload: CreateItemCategoryPayload,
): Promise<ErpItemCategory> {
  const res = await apiPost<ApiResponse<ErpItemCategory>>('/item-categories', payload);
  return res.data;
}

export async function updateItemCategory(
  id: string,
  payload: UpdateItemCategoryPayload,
): Promise<ErpItemCategory> {
  const res = await apiPatch<ApiResponse<ErpItemCategory>>(`/item-categories/${id}`, payload);
  return res.data;
}

export async function deleteItemCategory(id: string): Promise<void> {
  await apiDelete<void>(`/item-categories/${id}`);
}

export async function bulkUpdateErpItemCategoryStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/item-categories/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpItemCategories(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/item-categories/bulk', { ids });
  return { affected: res.affected };
}
