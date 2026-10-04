// D2 — supplier/publisher rebate agreements API (erp/pur/rebates).
import { apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface PurRebateRef {
  id: number;
  code: string;
  name: string;
}

export interface PurRebate {
  id: number;
  supplierId: number;
  supplier: PurRebateRef | null;
  categoryId: number | null;
  category: PurRebateRef | null;
  percent: number;
  periodYear: number;
  notes: string | null;
  isActive: boolean;
}

export interface PurRebatePayload {
  supplierId: number;
  categoryId?: number;
  percent: number;
  periodYear: number;
  notes?: string;
  isActive?: boolean;
}

export interface PurRebateAccrualRow extends PurRebate {
  baseAmount: number;
  accruedAmount: number;
  invoiceCount: number;
}

export interface PurRebateAccrual {
  periodYear: number;
  data: PurRebateAccrualRow[];
  totalAccrued: number;
}

export async function listRebates(
  params?: PaginationParams & Record<string, unknown>,
): Promise<PaginatedResponse<PurRebate>> {
  return apiGet<PaginatedResponse<PurRebate>>(
    '/pur/rebates',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createRebate(payload: PurRebatePayload): Promise<PurRebate> {
  const res = await apiPost<ApiResponse<PurRebate>>('/pur/rebates', payload);
  return res.data;
}

export async function updateRebate(id: string, payload: Partial<PurRebatePayload>): Promise<PurRebate> {
  const res = await apiPatch<ApiResponse<PurRebate>>(`/pur/rebates/${id}`, payload);
  return res.data;
}

export async function deleteRebate(id: string): Promise<void> {
  const { apiDelete } = await import('./client');
  await apiDelete(`/pur/rebates/${id}`);
}

export async function bulkRebateStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<ApiResponse<{ affected: number }>>('/pur/rebates/bulk-status', {
    ids: ids.map(Number),
    isActive,
  });
  return res.data;
}

export async function bulkDeleteRebates(ids: string[]): Promise<{ affected: number }> {
  const res = await apiPost<ApiResponse<{ affected: number }>>('/pur/rebates/bulk-delete', {
    ids: ids.map(Number),
  });
  return res.data;
}

export async function getRebateAccrual(year: number): Promise<PurRebateAccrual> {
  const res = await apiGet<ApiResponse<PurRebateAccrual>>('/pur/rebates/accrual', { year });
  return res.data;
}
