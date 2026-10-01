// ERP M5 Sales — AR Collection (IC) CRUD client
// Backend: GET/POST /erp/sls/ar-collections  (source='IC')

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export type ErpDocumentStatus = 'DRAFT' | 'NEED_APPROVE' | 'APPROVED' | 'REJECTED' | 'POSTED' | 'VOID' | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

export interface ErpArCollection {
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
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  source: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateArCollectionPayload {
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

export type UpdateArCollectionPayload = Partial<CreateArCollectionPayload>;

export interface ListArCollectionsParams extends PaginationParams {
  search?: string;
  status?: ErpDocumentStatus;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export type ArCollectionTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export async function listArCollections(
  params?: ListArCollectionsParams,
): Promise<PaginatedResponse<ErpArCollection>> {
  return apiGet<PaginatedResponse<ErpArCollection>>(
    '/sls/ar-collections',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function getArCollection(id: string): Promise<ErpArCollection> {
  const res = await apiGet<ApiResponse<ErpArCollection>>(`/sls/ar-collections/${id}`);
  return res.data;
}

export async function createArCollection(
  payload: CreateArCollectionPayload,
): Promise<ErpArCollection> {
  const res = await apiPost<ApiResponse<ErpArCollection>>('/sls/ar-collections', payload);
  return res.data;
}

export async function updateArCollection(
  id: string,
  payload: UpdateArCollectionPayload,
): Promise<ErpArCollection> {
  const res = await apiPatch<ApiResponse<ErpArCollection>>(
    `/sls/ar-collections/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteArCollection(id: string): Promise<void> {
  await apiDelete<void>(`/sls/ar-collections/${id}`);
}

export async function transitionArCollection(
  id: string,
  action: ArCollectionTransition,
): Promise<ErpArCollection> {
  const res = await apiPost<ApiResponse<ErpArCollection>>(
    `/sls/ar-collections/${id}/transition`,
    { action },
  );
  return res.data;
}
