// ERP Payment Term resource API — CRUD for md_payment_terms
// Endpoints: /payment-terms

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpPaymentTerm {
  id: string;
  code: string;
  name: string;
  netDays: number;
  discountDays1?: number | null;
  discountPercent1?: string | null;
  discountDays2?: number | null;
  discountPercent2?: string | null;
  penaltyPercent?: string | null;
  penaltyPeriod?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePaymentTermPayload {
  code: string;
  name: string;
  netDays: number;
  discountDays1?: number;
  discountPercent1?: string;
  discountDays2?: number;
  discountPercent2?: string;
  penaltyPercent?: string;
  penaltyPeriod?: string;
  isActive?: boolean;
}

export interface UpdatePaymentTermPayload {
  code?: string;
  name?: string;
  netDays?: number;
  discountDays1?: number;
  discountPercent1?: string;
  discountDays2?: number;
  discountPercent2?: string;
  penaltyPercent?: string;
  penaltyPeriod?: string;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listPaymentTerms(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpPaymentTerm>> {
  return apiGet<PaginatedResponse<ErpPaymentTerm>>('/payment-terms', params as Record<string, string | number | boolean | undefined>);
}

export async function createPaymentTerm(
  payload: CreatePaymentTermPayload,
): Promise<ErpPaymentTerm> {
  const res = await apiPost<ApiResponse<ErpPaymentTerm>>('/payment-terms', payload);
  return res.data;
}

export async function updatePaymentTerm(
  id: string,
  payload: UpdatePaymentTermPayload,
): Promise<ErpPaymentTerm> {
  const res = await apiPatch<ApiResponse<ErpPaymentTerm>>(`/payment-terms/${id}`, payload);
  return res.data;
}

export async function deletePaymentTerm(id: string): Promise<void> {
  await apiDelete<void>(`/payment-terms/${id}`);
}

export async function bulkUpdateErpPaymentTermStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/payment-terms/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpPaymentTerms(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/payment-terms/bulk', { ids });
  return { affected: res.affected };
}
