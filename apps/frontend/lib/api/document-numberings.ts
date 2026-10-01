// ERP Document Numbering resource API — CRUD for sys_document_numberings
// Endpoints: /document-numberings, /document-numberings/:documentCode/next

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpNumberingReset = 'NEVER' | 'YEARLY' | 'MONTHLY';

export const NUMBERING_RESET_OPTIONS: ErpNumberingReset[] = [
  'NEVER',
  'YEARLY',
  'MONTHLY',
];

export interface ErpDocumentNumbering {
  id: string;
  documentCode: string;
  name: string;
  prefix: string;
  digitCount: number;
  resetPolicy: ErpNumberingReset;
  nextNumber: number;
  menuId: string | null;
  affectsLedger: boolean;
  affectsInventory: boolean;
  affectsCost: boolean;
  notes: string | null;
  legacyCode: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDocumentNumberingPayload {
  documentCode: string;
  name: string;
  prefix: string;
  digitCount: number;
  resetPolicy: ErpNumberingReset;
  nextNumber?: number;
  affectsLedger?: boolean;
  affectsInventory?: boolean;
  affectsCost?: boolean;
  notes?: string;
}

export type UpdateDocumentNumberingPayload =
  Partial<CreateDocumentNumberingPayload>;

export interface NextDocumentNumber {
  documentCode: string;
  docNumber: string;
  sequence: number;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listDocumentNumberings(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpDocumentNumbering>> {
  return apiGet<PaginatedResponse<ErpDocumentNumbering>>(
    '/document-numberings',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createDocumentNumbering(
  payload: CreateDocumentNumberingPayload,
): Promise<ErpDocumentNumbering> {
  const res = await apiPost<ApiResponse<ErpDocumentNumbering>>(
    '/document-numberings',
    payload,
  );
  return res.data;
}

export async function updateDocumentNumbering(
  id: string,
  payload: UpdateDocumentNumberingPayload,
): Promise<ErpDocumentNumbering> {
  const res = await apiPatch<ApiResponse<ErpDocumentNumbering>>(
    `/document-numberings/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteDocumentNumbering(id: string): Promise<void> {
  await apiDelete<void>(`/document-numberings/${id}`);
}

export async function getNextDocumentNumber(
  documentCode: string,
): Promise<NextDocumentNumber> {
  const res = await apiPost<ApiResponse<NextDocumentNumber>>(
    `/document-numberings/${encodeURIComponent(documentCode)}/next`,
  );
  return res.data;
}
