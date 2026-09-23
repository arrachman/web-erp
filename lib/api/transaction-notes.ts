// ERP Transaction Note resource API — CRUD for md_transaction_notes
import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ErpTransactionNote {
  id: string;
  code: string;
  name: string;

  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateErpTransactionNotePayload {
  code: string;
  name: string;

  isActive?: boolean;
}

export type UpdateErpTransactionNotePayload = Partial<CreateErpTransactionNotePayload>;

const BASE = '/transaction-notes';

export async function listTransactionNotes(params?: PaginationParams): Promise<PaginatedResponse<ErpTransactionNote>> {
  return apiGet<PaginatedResponse<ErpTransactionNote>>(BASE, params as Record<string, string | number | boolean | undefined>);
}

export async function createErpTransactionNote(payload: CreateErpTransactionNotePayload): Promise<ErpTransactionNote> {
  const res = await apiPost<ApiResponse<ErpTransactionNote>>(BASE, payload);
  return res.data;
}

export async function updateErpTransactionNote(id: string, payload: UpdateErpTransactionNotePayload): Promise<ErpTransactionNote> {
  const res = await apiPatch<ApiResponse<ErpTransactionNote>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function deleteErpTransactionNote(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

export async function bulkUpdateErpTransactionNoteStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(`${BASE}/bulk/status`, { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteErpTransactionNotes(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(`${BASE}/bulk`, { ids });
  return { affected: res.affected };
}
