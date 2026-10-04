// ERP Sales — Freight Receivable (RP). Endpoint: /sls/freight-receivables
// Standalone document: own backend module + table (sls_freight_receivables),
// numbering via sys_document_numberings code 'RP' (RP000001, …).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/sls/freight-receivables';

export interface ErpRefLite {
  id: string;
  code: string;
  name: string;
}

export interface ErpSlsFreightReceivable {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRefLite | null;
  locationId?: string | null;
  transactionDate: string;
  fiscalPeriodId: string;
  customerId: string;
  customer?: ErpRefLite | null;
  description: string;
  notes?: string | null;
  currencyId: string;
  currency?: ErpRefLite | null;
  exchangeRate: string;
  amount: string;
  receivableAccountId?: string | null;
  receivableAccount?: ErpRefLite | null;
  incomeAccountId?: string | null;
  incomeAccount?: ErpRefLite | null;
  settlementStatus: string;
  settledDate?: string | null;
  status: string;
  postingStatus: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSlsFreightReceivablePayload {
  docNumber?: string;
  transactionDate: string;
  branchId: string;
  partnerId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  notes?: string;
  receivableAccountId?: string;
  incomeAccountId?: string;
}

export type UpdateSlsFreightReceivablePayload = Partial<CreateSlsFreightReceivablePayload>;

export interface ListSlsFreightReceivablesParams extends PaginationParams {
  search?: string;
  status?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export type SlsFreightReceivableTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

type Query = Record<string, string | number | boolean | undefined>;

export function listSlsFreightReceivables(
  params?: ListSlsFreightReceivablesParams,
): Promise<PaginatedResponse<ErpSlsFreightReceivable>> {
  return apiGet<PaginatedResponse<ErpSlsFreightReceivable>>(BASE, params as Query);
}

export async function getSlsFreightReceivable(id: string): Promise<ErpSlsFreightReceivable> {
  const res = await apiGet<ApiResponse<ErpSlsFreightReceivable>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsFreightReceivable(
  payload: CreateSlsFreightReceivablePayload,
): Promise<ErpSlsFreightReceivable> {
  const res = await apiPost<ApiResponse<ErpSlsFreightReceivable>>(BASE, payload);
  return res.data;
}

export async function updateSlsFreightReceivable(
  id: string,
  payload: UpdateSlsFreightReceivablePayload,
): Promise<ErpSlsFreightReceivable> {
  const res = await apiPatch<ApiResponse<ErpSlsFreightReceivable>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsFreightReceivable(
  id: string,
  action: SlsFreightReceivableTransition,
  reason?: string,
): Promise<ErpSlsFreightReceivable> {
  const res = await apiPost<ApiResponse<ErpSlsFreightReceivable>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsFreightReceivable(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
