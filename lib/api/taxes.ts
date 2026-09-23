// ERP Tax resource API — CRUD for md_taxes
// Endpoints: /taxes

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpTax {
  id: string;
  code: string;
  name: string;
  rate: string;
  saleAccountId?: string | null;
  purchaseAccountId?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaxPayload {
  code: string;
  name: string;
  rate: string;
  saleAccountId?: string | null;
  purchaseAccountId?: string | null;
  isActive?: boolean;
}

export interface UpdateTaxPayload {
  code?: string;
  name?: string;
  rate?: string;
  saleAccountId?: string | null;
  purchaseAccountId?: string | null;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listTaxes(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpTax>> {
  return apiGet<PaginatedResponse<ErpTax>>('/taxes', params as Record<string, string | number | boolean | undefined>);
}

export async function createTax(
  payload: CreateTaxPayload,
): Promise<ErpTax> {
  const res = await apiPost<ApiResponse<ErpTax>>('/taxes', payload);
  return res.data;
}

export async function updateTax(
  id: string,
  payload: UpdateTaxPayload,
): Promise<ErpTax> {
  const res = await apiPatch<ApiResponse<ErpTax>>(`/taxes/${id}`, payload);
  return res.data;
}

export async function deleteTax(id: string): Promise<void> {
  await apiDelete<void>(`/taxes/${id}`);
}

export async function bulkUpdateErpTaxStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/taxes/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpTaxes(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/taxes/bulk', { ids });
  return { affected: res.affected };
}
