// ERP m5 Sales — Return Receipt (RNR).
// Master-detail document (header + item lines).
// Endpoint: /sls/return-receipts

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

export interface SlsReturnReceiptListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsReturnReceiptListResponse {
  success: boolean;
  data: ErpSlsReturnReceipt[];
  meta: SlsReturnReceiptListMeta;
}

const BASE = '/sls/return-receipts';

export interface ErpSlsReturnReceipt {
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
  // RNR-specific fields
  invoiceId?: string | null;
  invoice?: ErpRef | null;
  returnId?: string | null;
  return?: ErpRef | null;
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

export interface CreateSlsReturnReceiptPayload {
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
  returnId?: string;
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

export type UpdateSlsReturnReceiptPayload = Partial<
  Omit<CreateSlsReturnReceiptPayload, 'lines'>
> & {
  lines?: SlsOrderLinePayload[];
};

export type SlsReturnReceiptTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsReturnReceiptsParams extends PaginationParams {
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

export function listSlsReturnReceipts(
  params?: ListSlsReturnReceiptsParams,
): Promise<SlsReturnReceiptListResponse> {
  return apiGet<SlsReturnReceiptListResponse>(BASE, params as Query);
}

export async function getSlsReturnReceipt(id: string): Promise<ErpSlsReturnReceipt> {
  const res = await apiGet<ApiResponse<ErpSlsReturnReceipt>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsReturnReceipt(
  payload: CreateSlsReturnReceiptPayload,
): Promise<ErpSlsReturnReceipt> {
  const res = await apiPost<ApiResponse<ErpSlsReturnReceipt>>(BASE, payload);
  return res.data;
}

export async function updateSlsReturnReceipt(
  id: string,
  payload: UpdateSlsReturnReceiptPayload,
): Promise<ErpSlsReturnReceipt> {
  const res = await apiPatch<ApiResponse<ErpSlsReturnReceipt>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsReturnReceipt(
  id: string,
  action: SlsReturnReceiptTransition,
  reason?: string,
): Promise<ErpSlsReturnReceipt> {
  const res = await apiPost<ApiResponse<ErpSlsReturnReceipt>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsReturnReceipt(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
