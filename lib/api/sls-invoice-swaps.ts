// ERP m5 Sales — Invoice Swap (SIE).
// Special header-only document: swaps one invoice reference for another.
// Lines carry fromInvoiceId → toInvoiceId + amount (not item lines).
// Endpoint: /sls/invoice-swaps

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedMeta, PaginationParams } from './types';
import type { ErpDocumentStatus, ErpPostingStatus, ErpRef } from './sls-orders';

export type { ErpDocumentStatus, ErpPostingStatus, ErpRef };

// ─── line ────────────────────────────────────────────────────────────────────

export interface ErpSlsInvoiceSwapLine {
  id?: string;
  lineNo: number;
  fromInvoiceId?: string | null;
  fromInvoice?: ErpRef | null;
  toInvoiceId?: string | null;
  toInvoice?: ErpRef | null;
  amount?: string | null;
}

// ─── header ──────────────────────────────────────────────────────────────────

export interface ErpSlsInvoiceSwap {
  id: string;
  code: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  docDate: string;
  fiscalPeriodId: string;
  customerId?: string | null;
  customer?: ErpRef | null;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
  description?: string | null;
  notes?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpSlsInvoiceSwapLine[];
}

// ─── payloads ─────────────────────────────────────────────────────────────────

export interface CreateSlsInvoiceSwapLinePayload {
  lineNo: number;
  fromInvoiceId?: string;
  toInvoiceId?: string;
  amount?: string;
}

export interface CreateSlsInvoiceSwapPayload {
  docNumber?: string;
  auto?: boolean;
  branchId: string;
  docDate: string;
  fiscalPeriodId?: string;
  customerId?: string;
  currencyId: string;
  exchangeRate?: string;
  description?: string;
  notes?: string;
  legacyCode?: string;
  lines: CreateSlsInvoiceSwapLinePayload[];
}

export type UpdateSlsInvoiceSwapPayload = Partial<
  Omit<CreateSlsInvoiceSwapPayload, 'lines'>
> & {
  lines?: CreateSlsInvoiceSwapLinePayload[];
};

export type SlsInvoiceSwapTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

// ─── list params ─────────────────────────────────────────────────────────────

export interface ListSlsInvoiceSwapsParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  customerId?: string;
  docNumberFrom?: string;
  description?: string;
  createdById?: string;
}

export interface SlsInvoiceSwapListMeta extends PaginatedMeta {
  sumAmount?: string;
}

export interface SlsInvoiceSwapListResponse {
  success: boolean;
  data: ErpSlsInvoiceSwap[];
  meta: SlsInvoiceSwapListMeta;
}

// ─── CRUD ────────────────────────────────────────────────────────────────────

const BASE = '/sls/invoice-swaps';

type Query = Record<string, string | number | boolean | undefined>;

export function listSlsInvoiceSwaps(
  params?: ListSlsInvoiceSwapsParams,
): Promise<SlsInvoiceSwapListResponse> {
  return apiGet<SlsInvoiceSwapListResponse>(BASE, params as Query);
}

export async function getSlsInvoiceSwap(id: string): Promise<ErpSlsInvoiceSwap> {
  const res = await apiGet<ApiResponse<ErpSlsInvoiceSwap>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsInvoiceSwap(
  payload: CreateSlsInvoiceSwapPayload,
): Promise<ErpSlsInvoiceSwap> {
  const res = await apiPost<ApiResponse<ErpSlsInvoiceSwap>>(BASE, payload);
  return res.data;
}

export async function updateSlsInvoiceSwap(
  id: string,
  payload: UpdateSlsInvoiceSwapPayload,
): Promise<ErpSlsInvoiceSwap> {
  const res = await apiPatch<ApiResponse<ErpSlsInvoiceSwap>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsInvoiceSwap(
  id: string,
  action: SlsInvoiceSwapTransition,
  reason?: string,
): Promise<ErpSlsInvoiceSwap> {
  const res = await apiPost<ApiResponse<ErpSlsInvoiceSwap>>(
    `${BASE}/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteSlsInvoiceSwap(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
