// ERP m4 Purchasing — Request for Quotation (RFQ).
// Lines = invited suppliers (pur_rfq_suppliers), NOT items.
// Endpoint: /pur/rfqs

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type { ErpDocumentStatus, ErpPostingStatus, ErpRef } from './pur-orders';

const BASE = '/pur/rfqs';
export type { ErpDocumentStatus, ErpPostingStatus, ErpRef } from './pur-orders';

export interface ErpPurRfqSupplier {
  id?: string; rfqId: string; supplierId: string; supplier?: ErpRef | null;
  notes?: string | null; lineNo: number;
}

export interface ErpPurRfq {
  id: string; docNumber: string; autoNumber?: string | null;
  branchId: string; branch?: ErpRef | null;
  locationId?: string | null; location?: ErpRef | null;
  docDate: string; fiscalPeriodId: string;
  requisitionId?: string | null;
  validFrom?: string | null; validTo?: string | null;
  description?: string | null; notes?: string | null; referenceNo?: string | null;
  status: ErpDocumentStatus; postingStatus: ErpPostingStatus; postedAt?: string | null;
  legacyCode?: string | null; createdAt: string; updatedAt: string;
  createdById?: string | null; updatedById?: string | null;
  suppliers: ErpPurRfqSupplier[];
}

export interface PurRfqSupplierPayload { supplierId: string; notes?: string; lineNo: number; }

export interface CreatePurRfqPayload {
  docNumber?: string; auto?: boolean; docDate: string; fiscalPeriodId?: string;
  branchId: string; locationId?: string; warehouseId?: string;
  currencyId?: string; exchangeRate?: string;
  requisitionId?: string;
  validFrom?: string; validTo?: string;
  description?: string; notes?: string; referenceNo?: string;
  legacyCode?: string;
  suppliers: PurRfqSupplierPayload[];
}

export type UpdatePurRfqPayload = Partial<CreatePurRfqPayload>;
export type PurRfqTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListPurRfqsParams extends PaginationParams {
  sortBy?: string; sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string; dateTo?: string; branchId?: string; createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listPurRfqs(p?: ListPurRfqsParams): Promise<PaginatedResponse<ErpPurRfq>> {
  return apiGet<PaginatedResponse<ErpPurRfq>>(BASE, p as Query);
}
export async function getPurRfq(id: string): Promise<ErpPurRfq> {
  return (await apiGet<ApiResponse<ErpPurRfq>>(`${BASE}/${id}`)).data;
}
export async function createPurRfq(payload: CreatePurRfqPayload): Promise<ErpPurRfq> {
  return (await apiPost<ApiResponse<ErpPurRfq>>(BASE, payload)).data;
}
export async function updatePurRfq(id: string, payload: UpdatePurRfqPayload): Promise<ErpPurRfq> {
  return (await apiPatch<ApiResponse<ErpPurRfq>>(`${BASE}/${id}`, payload)).data;
}
export async function transitionPurRfq(id: string, action: PurRfqTransition, reason?: string): Promise<ErpPurRfq> {
  return (await apiPost<ApiResponse<ErpPurRfq>>(`${BASE}/${id}/transition`, { action, reason })).data;
}
export async function deletePurRfq(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
