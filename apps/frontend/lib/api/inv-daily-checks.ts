// ERP m3 Inventory — Daily Check (DC). Master-detail document: header + item
// lines. Full workflow (SUBMIT/APPROVE/REJECT/POST/REOPEN).
// Endpoint: /inv/daily-checks

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/daily-checks';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvDailyCheckLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  costCenterId?: string | null;
  costCenter?: ErpRef | null;
  machineRef?: string | null;
  workHours?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpInvDailyCheck {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  checkDate: string;
  fiscalPeriodId?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  machineRef?: string | null;
  operatorRef?: string | null;
  description?: string | null;
  notes?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  lines: ErpInvDailyCheckLine[];
}

export interface InvDailyCheckLinePayload {
  itemId: string;
  quantity: string;
  unitId: string;
  warehouseId?: string;
  costCenterId?: string;
  machineRef?: string;
  workHours?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateInvDailyCheckPayload {
  auto?: boolean;
  docNumber?: string;
  checkDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  machineRef?: string;
  operatorRef?: string;
  description?: string;
  notes?: string;
  status?: ErpDocumentStatus;
  legacyCode?: string;
  lines: InvDailyCheckLinePayload[];
}

export type UpdateInvDailyCheckPayload = Partial<CreateInvDailyCheckPayload>;

export type InvDailyCheckTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

export interface ListInvDailyChecksParams extends PaginationParams {
  sortBy?: 'checkDate' | 'docNumber' | 'status' | 'createdAt';
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  branchId?: string;
  dateFrom?: string;
  dateTo?: string;
  search?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listInvDailyChecks(
  params?: ListInvDailyChecksParams,
): Promise<PaginatedResponse<ErpInvDailyCheck>> {
  return apiGet<PaginatedResponse<ErpInvDailyCheck>>(BASE, params as Query);
}

export async function getInvDailyCheck(id: string): Promise<ErpInvDailyCheck> {
  const res = await apiGet<ApiResponse<ErpInvDailyCheck>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvDailyCheck(
  payload: CreateInvDailyCheckPayload,
): Promise<ErpInvDailyCheck> {
  const res = await apiPost<ApiResponse<ErpInvDailyCheck>>(BASE, payload);
  return res.data;
}

export async function updateInvDailyCheck(
  id: string,
  payload: UpdateInvDailyCheckPayload,
): Promise<ErpInvDailyCheck> {
  const res = await apiPatch<ApiResponse<ErpInvDailyCheck>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionInvDailyCheck(
  id: string,
  action: InvDailyCheckTransition,
  reason?: string,
): Promise<ErpInvDailyCheck> {
  const res = await apiPost<ApiResponse<ErpInvDailyCheck>>(
    `${BASE}/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteInvDailyCheck(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
