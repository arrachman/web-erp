// ERP m3 Inventory — Stock Adjustments. One master-detail document (header +
// adjustment lines) discriminated by per-line `direction` (INCREASE/DECREASE),
// on the erp-inv-stock-adjustments backend.
// Endpoint: /inv/stock-adjustments (see apps/api-gateway/src/erp-inv-stock-adjustments).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/stock-adjustments';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/** Per-line adjustment direction: INCREASE = stock-in, DECREASE = stock-out. */
export type ErpInvAdjustmentDirection = 'INCREASE' | 'DECREASE';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvStockAdjustmentLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  direction: ErpInvAdjustmentDirection;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  unitCost?: string | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  inventoryAccountId?: string | null;
  inventoryAccount?: ErpRef | null;
  contraAccountId?: string | null;
  contraAccount?: ErpRef | null;
  costCenterId?: string | null;
  costCenter?: ErpRef | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpInvStockAdjustment {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  adjustmentDate: string;
  fiscalPeriodId: string;
  branchId: string;
  branch?: ErpRef | null;
  warehouseId: string;
  warehouse?: ErpRef | null;
  kind?: string | null;
  description?: string | null;
  notes?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpInvStockAdjustmentLine[];
}

export interface InvStockAdjustmentLinePayload {
  itemId: string;
  direction: ErpInvAdjustmentDirection;
  quantity: string;
  unitId: string;
  unitCost?: string;
  warehouseId?: string;
  inventoryAccountId?: string;
  contraAccountId?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateInvStockAdjustmentPayload {
  auto?: boolean;
  docNumber?: string;
  adjustmentDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  warehouseId: string;
  kind?: string;
  description?: string;
  notes?: string;
  status?: ErpDocumentStatus;
  legacyCode?: string;
  lines: InvStockAdjustmentLinePayload[];
}

export type UpdateInvStockAdjustmentPayload = Partial<CreateInvStockAdjustmentPayload>;

export type InvStockAdjustmentTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

export interface ListInvStockAdjustmentsParams extends PaginationParams {
  sortBy?: 'adjustmentDate' | 'docNumber' | 'status' | 'createdAt';
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

export function listInvStockAdjustments(
  params?: ListInvStockAdjustmentsParams,
): Promise<PaginatedResponse<ErpInvStockAdjustment>> {
  return apiGet<PaginatedResponse<ErpInvStockAdjustment>>(BASE, params as Query);
}

export async function getInvStockAdjustment(id: string): Promise<ErpInvStockAdjustment> {
  const res = await apiGet<ApiResponse<ErpInvStockAdjustment>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvStockAdjustment(
  payload: CreateInvStockAdjustmentPayload,
): Promise<ErpInvStockAdjustment> {
  const res = await apiPost<ApiResponse<ErpInvStockAdjustment>>(BASE, payload);
  return res.data;
}

export async function updateInvStockAdjustment(
  id: string,
  payload: UpdateInvStockAdjustmentPayload,
): Promise<ErpInvStockAdjustment> {
  const res = await apiPatch<ApiResponse<ErpInvStockAdjustment>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionInvStockAdjustment(
  id: string,
  action: InvStockAdjustmentTransition,
  reason?: string,
): Promise<ErpInvStockAdjustment> {
  const res = await apiPost<ApiResponse<ErpInvStockAdjustment>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteInvStockAdjustment(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
