// ERP m2 Finance — AR Receipts skeleton CRUD client

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export type ErpDocumentStatus = 'DRAFT' | 'POSTED' | 'VOID' | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';
export type ErpSettlementStatus = 'UNPAID' | 'PARTIAL' | 'PAID';

export interface ErpArReceipt {
  id: string;
  docNumber: string;
  branchId: string;
  transactionDate: string;
  fiscalPeriodId: string;
  partnerId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  allocatedAmount: string;
  paymentStatus: ErpSettlementStatus;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  notes?: string | null;
  contactPerson?: string | null;
  bankAccountId?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateArReceiptPayload {
  docNumber: string;
  branchId: string;
  transactionDate: string;
  fiscalPeriodId: string;
  partnerId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  allocatedAmount: string;
  paymentStatus?: ErpSettlementStatus;
  status?: ErpDocumentStatus;
  notes?: string;
  bankAccountId?: string;
}

export type UpdateArReceiptPayload = Partial<CreateArReceiptPayload>;

export interface ListArReceiptsParams extends PaginationParams {
  status?: ErpDocumentStatus;
  paymentStatus?: ErpSettlementStatus;
}

export async function listArReceipts(
  params?: ListArReceiptsParams,
): Promise<PaginatedResponse<ErpArReceipt>> {
  return apiGet<PaginatedResponse<ErpArReceipt>>(
    '/fin/ar-receipts',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createArReceipt(
  payload: CreateArReceiptPayload,
): Promise<ErpArReceipt> {
  const res = await apiPost<ApiResponse<ErpArReceipt>>(
    '/fin/ar-receipts',
    payload,
  );
  return res.data;
}

export async function updateArReceipt(
  id: string,
  payload: UpdateArReceiptPayload,
): Promise<ErpArReceipt> {
  const res = await apiPatch<ApiResponse<ErpArReceipt>>(
    `/fin/ar-receipts/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteArReceipt(id: string): Promise<void> {
  await apiDelete<void>(`/fin/ar-receipts/${id}`);
}
