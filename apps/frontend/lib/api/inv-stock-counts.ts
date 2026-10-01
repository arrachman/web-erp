// ERP m3 Inventory — Stock Counts (opname / physical inventory count). One
// master-detail document (header + count lines): system qty vs physical qty,
// variance derived per line. On the erp-inv-stock-counts backend.
// Endpoint: /inv/stock-counts (see apps/api-gateway/src/erp-inv-stock-counts).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/stock-counts';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/** Count scope: FULL = whole warehouse, CYCLE = recurring partial, SPOT = ad-hoc. */
export type ErpInvCountType = 'FULL' | 'CYCLE' | 'SPOT';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvStockCountLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  unitId: string;
  unit?: ErpRef | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  systemQty?: string | null;
  physicalQty: string;
  goodQty?: string | null;
  damagedQty?: string | null;
  varianceQty?: string | null;
  costCenterId?: string | null;
  costCenter?: ErpRef | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpInvStockCount {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  countDate: string;
  fiscalPeriodId: string;
  countType?: ErpInvCountType | null;
  stepNo?: number | null;
  branchId: string;
  branch?: ErpRef | null;
  warehouseId: string;
  warehouse?: ErpRef | null;
  description?: string | null;
  notes?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpInvStockCountLine[];
}

export interface InvStockCountLinePayload {
  itemId: string;
  unitId: string;
  warehouseId?: string;
  systemQty?: string;
  physicalQty: string;
  goodQty?: string;
  damagedQty?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateInvStockCountPayload {
  auto?: boolean;
  docNumber?: string;
  countDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  warehouseId: string;
  countType?: ErpInvCountType;
  stepNo?: number;
  description?: string;
  notes?: string;
  status?: ErpDocumentStatus;
  legacyCode?: string;
  lines: InvStockCountLinePayload[];
}

export type UpdateInvStockCountPayload = Partial<CreateInvStockCountPayload>;

export type InvStockCountTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

export interface ListInvStockCountsParams extends PaginationParams {
  sortBy?: 'countDate' | 'docNumber' | 'status' | 'createdAt';
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  branchId?: string;
  warehouseId?: string;
  dateFrom?: string;
  dateTo?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listInvStockCounts(
  params?: ListInvStockCountsParams,
): Promise<PaginatedResponse<ErpInvStockCount>> {
  return apiGet<PaginatedResponse<ErpInvStockCount>>(BASE, params as Query);
}

export async function getInvStockCount(id: string): Promise<ErpInvStockCount> {
  const res = await apiGet<ApiResponse<ErpInvStockCount>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvStockCount(
  payload: CreateInvStockCountPayload,
): Promise<ErpInvStockCount> {
  const res = await apiPost<ApiResponse<ErpInvStockCount>>(BASE, payload);
  return res.data;
}

export async function updateInvStockCount(
  id: string,
  payload: UpdateInvStockCountPayload,
): Promise<ErpInvStockCount> {
  const res = await apiPatch<ApiResponse<ErpInvStockCount>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionInvStockCount(
  id: string,
  action: InvStockCountTransition,
  reason?: string,
): Promise<ErpInvStockCount> {
  const res = await apiPost<ApiResponse<ErpInvStockCount>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteInvStockCount(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
