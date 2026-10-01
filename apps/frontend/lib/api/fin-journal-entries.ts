// ERP m2 Finance — Journal Entries (header + lines) client.
// Endpoints: /fin/journal-entries (+ /:id/transition). Backend = posting +
// workflow state machine at cash/bank parity.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export type ErpJournalType =
  | 'GENERAL'
  | 'MEMORIAL'
  | 'ADJUSTMENT'
  | 'OPENING_BALANCE'
  | 'REVALUATION'
  | 'CLOSING';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVE_1'
  | 'APPROVE_2'
  | 'APPROVE_3'
  | 'APPROVE_4'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/** Workflow actions (§2.7 state machine), 1:1 with the backend transition DTO. */
export type JournalTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ErpJournalLine {
  id?: string;
  accountId: string;
  currencyId?: string;
  exchangeRate?: string;
  debit: string;
  credit: string;
  debitFx?: string | null;
  creditFx?: string | null;
  notes?: string | null;
  costCenterId?: string | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  lineNo: number;
}

export interface ErpJournalEntry {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  journalType: ErpJournalType;
  branchId: string;
  locationId?: string | null;
  source?: string | null;
  entryDate: string;
  fiscalPeriodId: string;
  partnerId?: string | null;
  contactPerson?: string | null;
  description: string;
  notes?: string | null;
  currencyId: string;
  exchangeRate: string;
  status: ErpDocumentStatus;
  previousStatus?: ErpDocumentStatus | null;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  lines: ErpJournalLine[];
}

export interface CreateJournalEntryPayload {
  /** Omit + auto=true for an auto-generated number (per journalType numbering). */
  docNumber?: string;
  auto?: boolean;
  journalType: ErpJournalType;
  branchId: string;
  entryDate: string;
  /** Optional — backend derives the period from entryDate when omitted. */
  fiscalPeriodId?: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  notes?: string;
  partnerId?: string;
  locationId?: string;
  lines: ErpJournalLine[];
}

export type UpdateJournalEntryPayload = Partial<CreateJournalEntryPayload>;

export interface ListJournalEntriesParams extends PaginationParams {
  search?: string;
  journalType?: ErpJournalType;
  status?: ErpDocumentStatus;
  branchId?: string;
  partnerId?: string;
  createdById?: string;
  dateFrom?: string;
  dateTo?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  notes?: string;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  includeTotal?: boolean;
}

export async function listJournalEntries(
  params?: ListJournalEntriesParams,
): Promise<PaginatedResponse<ErpJournalEntry>> {
  return apiGet<PaginatedResponse<ErpJournalEntry>>(
    '/fin/journal-entries',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function getJournalEntry(id: string): Promise<ErpJournalEntry> {
  const res = await apiGet<ApiResponse<ErpJournalEntry>>(`/fin/journal-entries/${id}`);
  return res.data;
}

export async function createJournalEntry(
  payload: CreateJournalEntryPayload,
): Promise<ErpJournalEntry> {
  const res = await apiPost<ApiResponse<ErpJournalEntry>>('/fin/journal-entries', payload);
  return res.data;
}

export async function updateJournalEntry(
  id: string,
  payload: UpdateJournalEntryPayload,
): Promise<ErpJournalEntry> {
  const res = await apiPatch<ApiResponse<ErpJournalEntry>>(`/fin/journal-entries/${id}`, payload);
  return res.data;
}

export async function transitionJournalEntry(
  id: string,
  action: JournalTransition,
  reason?: string,
): Promise<ErpJournalEntry> {
  const res = await apiPost<ApiResponse<ErpJournalEntry>>(
    `/fin/journal-entries/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteJournalEntry(id: string): Promise<void> {
  await apiDelete<void>(`/fin/journal-entries/${id}`);
}
