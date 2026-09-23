// ERP m2 Finance — Giro Entries (unified register/clear, header + instruments).
// One model discriminated by kind (REGISTER|CLEAR) × type (INCOMING|OUTGOING):
//   RG = REGISTER+INCOMING, SG = REGISTER+OUTGOING,
//   RGC = CLEAR+INCOMING,   SGC = CLEAR+OUTGOING.
// Single endpoint /fin/giro-entries (+ /:id/transition) at journal/cash-bank
// parity. Outstanding picker for clearing via /fin/giros/outstanding.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type { ErpDocumentStatus, ErpPostingStatus } from './fin-journal-entries';

export type GiroKind = 'REGISTER' | 'CLEAR';
export type GiroType = 'INCOMING' | 'OUTGOING';
export type GiroInstrumentStatus = 'OUTSTANDING' | 'CLEARED' | 'BOUNCED' | 'CANCELLED';

/** Workflow actions (§2.7 state machine), 1:1 with the backend transition DTO. */
export type GiroTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface GiroInstrument {
  id: string;
  giroNumber: string;
  bankName?: string | null;
  dueDate: string;
  amount: string;
  status: GiroInstrumentStatus;
  giroAccountId?: string | null;
  clearedDate?: string | null;
  lineNo: number;
  notes?: string | null;
}

export interface ErpGiroEntry {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  kind: GiroKind;
  type: GiroType;
  branchId: string;
  partnerId?: string | null;
  entryDate: string;
  fiscalPeriodId: string;
  bankAccountId?: string | null;
  currencyId: string;
  exchangeRate: string;
  description?: string | null;
  notes?: string | null;
  status: ErpDocumentStatus;
  previousStatus?: ErpDocumentStatus | null;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  createdAt: string;
  updatedAt: string;
  registeredGiros: GiroInstrument[];
  clearedGiros: GiroInstrument[];
}

/** One register instrument row of a CreateGiroEntry (kind=REGISTER). */
export interface GiroRegisterRow {
  giroNumber: string;
  bankName?: string;
  dueDate: string;
  amount: string;
  notes?: string;
  giroAccountId?: string;
}

/** One clearing row of a CreateGiroEntry (kind=CLEAR) — picks an outstanding giro. */
export interface GiroClearRow {
  giroId: string;
  clearedDate: string;
}

export type GiroRow = GiroRegisterRow | GiroClearRow;

export interface CreateGiroEntryPayload {
  /** Omit + auto=true for an auto-generated number (per kind×type numbering). */
  docNumber?: string;
  auto?: boolean;
  kind: GiroKind;
  type: GiroType;
  branchId: string;
  partnerId?: string;
  entryDate: string;
  /** Optional — backend derives the period from entryDate when omitted. */
  fiscalPeriodId?: string;
  /** Required for CLEAR (settlement bank). */
  bankAccountId?: string;
  giroAccountId?: string;
  currencyId: string;
  exchangeRate: string;
  description?: string;
  notes?: string;
  rows: GiroRow[];
}

export type UpdateGiroEntryPayload = Partial<CreateGiroEntryPayload>;

export interface OutstandingGiro {
  id: string;
  giroNumber: string;
  bankName?: string | null;
  dueDate: string;
  amount: string;
  partnerId?: string | null;
  giroAccountId?: string | null;
}

export interface ListGiroEntriesParams extends PaginationParams {
  search?: string;
  kind?: GiroKind;
  type?: GiroType;
  status?: ErpDocumentStatus;
  branchId?: string;
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}

export async function listGiroEntries(
  params?: ListGiroEntriesParams,
): Promise<PaginatedResponse<ErpGiroEntry>> {
  return apiGet<PaginatedResponse<ErpGiroEntry>>(
    '/fin/giro-entries',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function getGiroEntry(id: string): Promise<ErpGiroEntry> {
  const res = await apiGet<ApiResponse<ErpGiroEntry>>(`/fin/giro-entries/${id}`);
  return res.data;
}

export async function createGiroEntry(
  payload: CreateGiroEntryPayload,
): Promise<ErpGiroEntry> {
  const res = await apiPost<ApiResponse<ErpGiroEntry>>('/fin/giro-entries', payload);
  return res.data;
}

export async function updateGiroEntry(
  id: string,
  payload: UpdateGiroEntryPayload,
): Promise<ErpGiroEntry> {
  const res = await apiPatch<ApiResponse<ErpGiroEntry>>(`/fin/giro-entries/${id}`, payload);
  return res.data;
}

export async function transitionGiroEntry(
  id: string,
  action: GiroTransition,
  reason?: string,
): Promise<ErpGiroEntry> {
  const res = await apiPost<ApiResponse<ErpGiroEntry>>(
    `/fin/giro-entries/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteGiroEntry(id: string): Promise<void> {
  await apiDelete<void>(`/fin/giro-entries/${id}`);
}

export async function listOutstandingGiros(params: {
  type: GiroType;
  search?: string;
  partnerId?: string;
}): Promise<OutstandingGiro[]> {
  const res = await apiGet<ApiResponse<OutstandingGiro[]>>(
    '/fin/giros/outstanding',
    params as Record<string, string | undefined>,
  );
  return res.data;
}
