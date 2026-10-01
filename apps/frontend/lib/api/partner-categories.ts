// ERP Partner Category resource API — CRUD for md_partner_categories
// Endpoints: /partner-categories

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpPartnerCategoryKind =
  | 'CUSTOMER'
  | 'SUPPLIER'
  | 'SALESMAN'
  | 'GENERAL';

export const PARTNER_CATEGORY_KINDS: ErpPartnerCategoryKind[] = [
  'CUSTOMER',
  'SUPPLIER',
  'SALESMAN',
  'GENERAL',
];

export interface ErpPartnerCategory {
  id: string;
  code: string;
  name: string;
  kind: ErpPartnerCategoryKind;
  salesTier?: number | null; // Tingkat Harga/Diskon Jual (1–10) untuk pelanggan kategori ini
  isActive: boolean;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePartnerCategoryPayload {
  code: string;
  name: string;
  kind: ErpPartnerCategoryKind;
  salesTier?: number | null;
  isActive?: boolean;
}

export interface UpdatePartnerCategoryPayload {
  code?: string;
  name?: string;
  kind?: ErpPartnerCategoryKind;
  salesTier?: number | null;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listPartnerCategories(
  params?: PaginationParams & { kind?: 'CUSTOMER' | 'SUPPLIER' | 'SALESMAN'; isActive?: boolean },
): Promise<PaginatedResponse<ErpPartnerCategory>> {
  return apiGet<PaginatedResponse<ErpPartnerCategory>>(
    '/partner-categories',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createPartnerCategory(
  payload: CreatePartnerCategoryPayload,
): Promise<ErpPartnerCategory> {
  const res = await apiPost<ApiResponse<ErpPartnerCategory>>(
    '/partner-categories',
    payload,
  );
  return res.data;
}

export async function updatePartnerCategory(
  id: string,
  payload: UpdatePartnerCategoryPayload,
): Promise<ErpPartnerCategory> {
  const res = await apiPatch<ApiResponse<ErpPartnerCategory>>(
    `/partner-categories/${id}`,
    payload,
  );
  return res.data;
}

export async function deletePartnerCategory(id: string): Promise<void> {
  await apiDelete<void>(`/partner-categories/${id}`);
}

export async function bulkUpdateErpPartnerCategoryStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/partner-categories/bulk/status',
    { ids, isActive },
  );
  return { affected: res.affected };
}

export async function bulkDeleteErpPartnerCategories(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/partner-categories/bulk',
    { ids },
  );
  return { affected: res.affected };
}
