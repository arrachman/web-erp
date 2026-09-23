// ERP m4 Purchasing — Vendor Advance (Uang Muka ke Supplier, source='AP').
// Endpoint: /pur/vendor-advances

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/pur/vendor-advances';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

export interface ErpVendorAdvance {
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

export interface CreateVendorAdvancePayload {
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

export type UpdateVendorAdvancePayload = Partial<CreateVendorAdvancePayload>;

export interface ListVendorAdvancesParams extends PaginationParams {
  search?: string;
  status?: ErpDocumentStatus;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export type VendorAdvanceTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

type Query = Record<string, string | number | boolean | undefined>;

export function listVendorAdvances(
  params?: ListVendorAdvancesParams,
): Promise<PaginatedResponse<ErpVendorAdvance>> {
  return apiGet<PaginatedResponse<ErpVendorAdvance>>(BASE, params as Query);
}

export async function getVendorAdvance(id: string): Promise<ErpVendorAdvance> {
  const res = await apiGet<ApiResponse<ErpVendorAdvance>>(`${BASE}/${id}`);
  return res.data;
}

export async function createVendorAdvance(
  payload: CreateVendorAdvancePayload,
): Promise<ErpVendorAdvance> {
  const res = await apiPost<ApiResponse<ErpVendorAdvance>>(BASE, payload);
  return res.data;
}

export async function updateVendorAdvance(
  id: string,
  payload: UpdateVendorAdvancePayload,
): Promise<ErpVendorAdvance> {
  const res = await apiPatch<ApiResponse<ErpVendorAdvance>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionVendorAdvance(
  id: string,
  action: VendorAdvanceTransition,
  reason?: string,
): Promise<ErpVendorAdvance> {
  const res = await apiPost<ApiResponse<ErpVendorAdvance>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteVendorAdvance(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
