// ERP m4 Purchasing — Freight Payable (Tagihan Ekspedisi, source='PP').
// Endpoint: /pur/freight-payables

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/pur/freight-payables';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

export interface ErpFreightPayable {
  id: string;
  docNumber: string;
  transactionDate: string;
  fiscalPeriodId: string;
  branchId: string;
  partner?: { id: string; code: string; name: string } | null;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  notes?: string | null;
  status: string;
  postingStatus: string;
  source: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFreightPayablePayload {
  docNumber: string;
  transactionDate: string;
  fiscalPeriodId: string;
  branchId: string;
  partnerId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  notes?: string;
}

export type UpdateFreightPayablePayload = Partial<CreateFreightPayablePayload>;

export interface ListFreightPayablesParams extends PaginationParams {
  search?: string;
  status?: ErpDocumentStatus;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export type FreightPayableTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

type Query = Record<string, string | number | boolean | undefined>;

export function listFreightPayables(
  params?: ListFreightPayablesParams,
): Promise<PaginatedResponse<ErpFreightPayable>> {
  return apiGet<PaginatedResponse<ErpFreightPayable>>(BASE, params as Query);
}

export async function getFreightPayable(id: string): Promise<ErpFreightPayable> {
  const res = await apiGet<ApiResponse<ErpFreightPayable>>(`${BASE}/${id}`);
  return res.data;
}

export async function createFreightPayable(
  payload: CreateFreightPayablePayload,
): Promise<ErpFreightPayable> {
  const res = await apiPost<ApiResponse<ErpFreightPayable>>(BASE, payload);
  return res.data;
}

export async function updateFreightPayable(
  id: string,
  payload: UpdateFreightPayablePayload,
): Promise<ErpFreightPayable> {
  const res = await apiPatch<ApiResponse<ErpFreightPayable>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionFreightPayable(
  id: string,
  action: FreightPayableTransition,
  reason?: string,
): Promise<ErpFreightPayable> {
  const res = await apiPost<ApiResponse<ErpFreightPayable>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteFreightPayable(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
