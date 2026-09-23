// ERP m5 Sales — Customer Advance (AS).
// Header-only payment document (no item lines).
// Endpoint: /sls/customer-advances

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedMeta, PaginationParams } from './types';
import type { ErpDocumentStatus, ErpRef } from './sls-orders';

export type { ErpDocumentStatus, ErpRef };

export interface SlsCustomerAdvanceListMeta extends PaginatedMeta {
  sumAmount: string;
}
export interface SlsCustomerAdvanceListResponse {
  success: boolean;
  data: ErpSlsCustomerAdvance[];
  meta: SlsCustomerAdvanceListMeta;
}

const BASE = '/sls/customer-advances';

export interface ErpSlsCustomerAdvance {
  id: string;
  code: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  customerId?: string | null;
  customer?: ErpRef | null;
  docDate: string;
  fiscalPeriodId: string;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
  // Header-only payment fields
  amount: string;
  amountFx?: string | null;
  appliedAmount?: string | null;
  settlementStatus?: string | null;
  paymentTermId?: string | null;
  paymentTerm?: ErpRef | null;
  dueDate?: string | null;
  description?: string | null;
  notes?: string | null;
  customFields?: Record<string, unknown> | null;
  status: ErpDocumentStatus;
  postingStatus: string;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  // NO lines array — header-only document
}

export interface CreateSlsCustomerAdvancePayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  customerId?: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  amountFx?: string;
  paymentTermId?: string;
  dueDate?: string;
  description?: string;
  notes?: string;
  legacyCode?: string;
  customFields?: Record<string, unknown>;
}

export type UpdateSlsCustomerAdvancePayload = Partial<CreateSlsCustomerAdvancePayload>;

export type SlsCustomerAdvanceTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsCustomerAdvancesParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  customerId?: string;
  docNumberFrom?: string;
  description?: string;
  settlementStatus?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listSlsCustomerAdvances(
  params?: ListSlsCustomerAdvancesParams,
): Promise<SlsCustomerAdvanceListResponse> {
  return apiGet<SlsCustomerAdvanceListResponse>(BASE, params as Query);
}

export async function getSlsCustomerAdvance(id: string): Promise<ErpSlsCustomerAdvance> {
  const res = await apiGet<ApiResponse<ErpSlsCustomerAdvance>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsCustomerAdvance(
  payload: CreateSlsCustomerAdvancePayload,
): Promise<ErpSlsCustomerAdvance> {
  const res = await apiPost<ApiResponse<ErpSlsCustomerAdvance>>(BASE, payload);
  return res.data;
}

export async function updateSlsCustomerAdvance(
  id: string,
  payload: UpdateSlsCustomerAdvancePayload,
): Promise<ErpSlsCustomerAdvance> {
  const res = await apiPatch<ApiResponse<ErpSlsCustomerAdvance>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsCustomerAdvance(
  id: string,
  action: SlsCustomerAdvanceTransition,
  reason?: string,
): Promise<ErpSlsCustomerAdvance> {
  const res = await apiPost<ApiResponse<ErpSlsCustomerAdvance>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsCustomerAdvance(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
