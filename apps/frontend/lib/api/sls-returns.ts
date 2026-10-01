// ERP m5 Sales — Sales Return (SR).
// Master-detail document (header + item lines) on the erp-sls-returns backend.
// Endpoint: /sls/returns

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedMeta, PaginationParams } from './types';
import type {
  ErpDocumentStatus,
  ErpPriceMode,
  ErpRef,
  SlsOrderLinePayload,
  ErpSlsOrderLine,
} from './sls-orders';

export type { ErpDocumentStatus, ErpPriceMode, ErpRef, SlsOrderLinePayload };

export interface SlsReturnListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsReturnListResponse {
  success: boolean;
  data: ErpSlsReturn[];
  meta: SlsReturnListMeta;
}

const BASE = '/sls/returns';

export interface ErpSlsReturn {
  id: string;
  code: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  docDate: string;
  fiscalPeriodId: string;
  customerId?: string | null;
  customer?: ErpRef | null;
  paymentTermId?: string | null;
  paymentTerm?: ErpRef | null;
  dueDate?: string | null;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
  priceMode: ErpPriceMode;
  subtotal: string;
  discountPercent?: string | null;
  discountAmount?: string | null;
  tax1Amount?: string | null;
  tax2Amount?: string | null;
  otherCostAmount?: string | null;
  grandTotal: string;
  description?: string | null;
  notes?: string | null;
  referenceNo?: string | null;
  referenceDate?: string | null;
  // SR-specific fields
  invoiceId?: string | null;
  invoice?: ErpRef | null;
  remainingAccountId?: string | null;
  remainingAccount?: ErpRef | null;
  settlementStatus?: string | null;
  receivableAccountId?: string | null;
  receivableAccount?: ErpRef | null;
  customFields?: Record<string, unknown> | null;
  status: ErpDocumentStatus;
  postingStatus: string;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpSlsOrderLine[];
}

export interface CreateSlsReturnPayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  warehouseId?: string;
  customerId?: string;
  paymentTermId?: string;
  dueDate?: string;
  currencyId: string;
  exchangeRate: string;
  priceMode?: ErpPriceMode;
  invoiceId?: string;
  remainingAccountId?: string;
  settlementStatus?: string;
  description?: string;
  notes?: string;
  referenceNo?: string;
  receivableAccountId?: string;
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  customFields?: Record<string, unknown>;
  lines: SlsOrderLinePayload[];
}

export type UpdateSlsReturnPayload = Partial<Omit<CreateSlsReturnPayload, 'lines'>> & {
  lines?: SlsOrderLinePayload[];
};

export type SlsReturnTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsReturnsParams extends PaginationParams {
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

export function listSlsReturns(params?: ListSlsReturnsParams): Promise<SlsReturnListResponse> {
  return apiGet<SlsReturnListResponse>(BASE, params as Query);
}

export async function getSlsReturn(id: string): Promise<ErpSlsReturn> {
  const res = await apiGet<ApiResponse<ErpSlsReturn>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsReturn(payload: CreateSlsReturnPayload): Promise<ErpSlsReturn> {
  const res = await apiPost<ApiResponse<ErpSlsReturn>>(BASE, payload);
  return res.data;
}

export async function updateSlsReturn(
  id: string,
  payload: UpdateSlsReturnPayload,
): Promise<ErpSlsReturn> {
  const res = await apiPatch<ApiResponse<ErpSlsReturn>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsReturn(
  id: string,
  action: SlsReturnTransition,
  reason?: string,
): Promise<ErpSlsReturn> {
  const res = await apiPost<ApiResponse<ErpSlsReturn>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsReturn(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
