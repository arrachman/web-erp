// ERP Transaction Note Detail resource API — CRUD for md_transaction_note_details
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpTxnNoteDetail {
  id: string;
  transactionNoteId: string;
  sortOrder: number;
  content: string;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  transactionNote?: { id: string; code: string; name: string } | null;
}

export interface CreateErpTxnNoteDetailPayload {
  transactionNoteId: string;
  sortOrder?: number;
  content: string;
  legacyCode?: string;
}

export type UpdateErpTxnNoteDetailPayload = Partial<CreateErpTxnNoteDetailPayload>;

const BASE = '/txn-note-details';

export async function listTxnNoteDetails(params?: PaginationParams): Promise<PaginatedResponse<ErpTxnNoteDetail>> {
  return apiGet<PaginatedResponse<ErpTxnNoteDetail>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpTxnNoteDetail(payload: CreateErpTxnNoteDetailPayload): Promise<ErpTxnNoteDetail> {
  const res = await apiPost<ApiResponse<ErpTxnNoteDetail>>(BASE, payload);
  return res.data;
}

export async function updateErpTxnNoteDetail(id: string, payload: UpdateErpTxnNoteDetailPayload): Promise<ErpTxnNoteDetail> {
  const res = await apiPatch<ApiResponse<ErpTxnNoteDetail>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpTxnNoteDetail(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkDeleteErpTxnNoteDetails(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
