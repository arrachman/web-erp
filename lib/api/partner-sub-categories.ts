// ERP Partner Sub Category resource API — CRUD for md_partner_sub_categories
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpPartnerSubCategory {
  id: string;
  code: string;
  name: string;
  type: 'CUSTOMER'|'SUPPLIER'|'SALESMAN';
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpPartnerSubCategoryPayload {
  code: string;
  name: string;
  type: 'CUSTOMER'|'SUPPLIER'|'SALESMAN';
  isActive?: boolean;
}

export type UpdateErpPartnerSubCategoryPayload = Partial<CreateErpPartnerSubCategoryPayload>;

const BASE = '/partner-sub-categories';

export type PartnerSubCategoryType = 'CUSTOMER' | 'SUPPLIER' | 'SALESMAN';

export interface ListPartnerSubCategoriesParams extends PaginationParams {
  type?: PartnerSubCategoryType;
}

export async function listPartnerSubCategories(params?: ListPartnerSubCategoriesParams): Promise<PaginatedResponse<ErpPartnerSubCategory>> {
  return apiGet<PaginatedResponse<ErpPartnerSubCategory>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpPartnerSubCategory(payload: CreateErpPartnerSubCategoryPayload): Promise<ErpPartnerSubCategory> {
  const res = await apiPost<ApiResponse<ErpPartnerSubCategory>>(BASE, payload);
  return res.data;
}

export async function updateErpPartnerSubCategory(id: string, payload: UpdateErpPartnerSubCategoryPayload): Promise<ErpPartnerSubCategory> {
  const res = await apiPatch<ApiResponse<ErpPartnerSubCategory>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpPartnerSubCategory(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpPartnerSubCategoryStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpPartnerSubCategories(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
