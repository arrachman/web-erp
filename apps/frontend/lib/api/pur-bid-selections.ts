// ERP m4 Purchasing — Bid Selection / Comparison (BS).
// Lines = quotation line evaluations (quotationLineId + rank/selected).
// Endpoint: /pur/bid-selections

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type { ErpDocumentStatus, ErpPostingStatus, ErpRef } from './pur-orders';

const BASE = '/pur/bid-selections';
export type { ErpDocumentStatus, ErpPostingStatus, ErpRef } from './pur-orders';

export interface ErpPurBidSelectionLine {
  id?: string; bidSelectionId: string;
  quotationLineId: string;
  selected: boolean; priceRank: number;
  notes?: string | null; lineNo: number;
}

export interface ErpPurBidSelection {
  id: string; docNumber: string; autoNumber?: string | null;
  branchId: string; branch?: ErpRef | null;
  locationId?: string | null; location?: ErpRef | null;
  docDate: string; fiscalPeriodId: string;
  description?: string | null; notes?: string | null; referenceNo?: string | null;
  status: ErpDocumentStatus; postingStatus: ErpPostingStatus; postedAt?: string | null;
  legacyCode?: string | null; createdAt: string; updatedAt: string;
  createdById?: string | null; updatedById?: string | null;
  lines: ErpPurBidSelectionLine[];
}

export interface PurBidSelectionLinePayload {
  quotationLineId: string; selected?: boolean; priceRank: number; notes?: string; lineNo: number;
}

export interface CreatePurBidSelectionPayload {
  docNumber?: string; auto?: boolean; docDate: string; fiscalPeriodId?: string;
  branchId: string; locationId?: string;
  description?: string; notes?: string; referenceNo?: string;
  legacyCode?: string;
  lines: PurBidSelectionLinePayload[];
}

export type UpdatePurBidSelectionPayload = Partial<CreatePurBidSelectionPayload>;
export type PurBidSelectionTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListPurBidSelectionsParams extends PaginationParams {
  sortBy?: string; sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus; dateFrom?: string; dateTo?: string;
  branchId?: string; createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listPurBidSelections(p?: ListPurBidSelectionsParams): Promise<PaginatedResponse<ErpPurBidSelection>> {
  return apiGet<PaginatedResponse<ErpPurBidSelection>>(BASE, p as Query);
}
export async function getPurBidSelection(id: string): Promise<ErpPurBidSelection> {
  return (await apiGet<ApiResponse<ErpPurBidSelection>>(`${BASE}/${id}`)).data;
}
export async function createPurBidSelection(payload: CreatePurBidSelectionPayload): Promise<ErpPurBidSelection> {
  return (await apiPost<ApiResponse<ErpPurBidSelection>>(BASE, payload)).data;
}
export async function updatePurBidSelection(id: string, payload: UpdatePurBidSelectionPayload): Promise<ErpPurBidSelection> {
  return (await apiPatch<ApiResponse<ErpPurBidSelection>>(`${BASE}/${id}`, payload)).data;
}
export async function transitionPurBidSelection(id: string, action: PurBidSelectionTransition, reason?: string): Promise<ErpPurBidSelection> {
  return (await apiPost<ApiResponse<ErpPurBidSelection>>(`${BASE}/${id}/transition`, { action, reason })).data;
}
export async function deletePurBidSelection(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
