// ERP m3 Inventory — Price Adjustments (PA). Header-only document (no item
// lines). Triggers a server-side re-costing operation over a date range /
// optional warehouse+item filter. No workflow transitions — result status is
// set by the backend job (PENDING → COMPLETED | FAILED).
// Endpoint: /inv/price-adjustments

import { apiDelete, apiGet, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/price-adjustments';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';

export type ErpInvPriceAdjustmentStatus = 'PENDING' | 'COMPLETED' | 'FAILED';
export type ErpInvCostingMethod = 'AVG' | 'FIFO' | 'STD';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvPriceAdjustment {
  id: string;
  docNumber: string;
  fromDate: string;
  toDate?: string | null;
  fiscalPeriodId?: string | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  itemId?: string | null;
  item?: ErpRef | null;
  costingMethod: ErpInvCostingMethod;
  status: ErpInvPriceAdjustmentStatus;
  totalDelta?: string | null;
  notes?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
}

export interface CreateInvPriceAdjustmentPayload {
  docNumber?: string;
  fromDate: string;
  toDate?: string;
  fiscalPeriodId?: string;
  warehouseId?: string;
  itemId?: string;
  costingMethod?: ErpInvCostingMethod;
  notes?: string;
  legacyCode?: string;
}

export interface ListInvPriceAdjustmentsParams extends PaginationParams {
  sortBy?: 'fromDate' | 'docNumber' | 'status' | 'createdAt';
  sortDir?: 'asc' | 'desc';
  status?: ErpInvPriceAdjustmentStatus;
  itemId?: string;
  warehouseId?: string;
  dateFrom?: string;
  dateTo?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listInvPriceAdjustments(
  params?: ListInvPriceAdjustmentsParams,
): Promise<PaginatedResponse<ErpInvPriceAdjustment>> {
  return apiGet<PaginatedResponse<ErpInvPriceAdjustment>>(BASE, params as Query);
}

export async function getInvPriceAdjustment(id: string): Promise<ErpInvPriceAdjustment> {
  const res = await apiGet<ApiResponse<ErpInvPriceAdjustment>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvPriceAdjustment(
  payload: CreateInvPriceAdjustmentPayload,
): Promise<ErpInvPriceAdjustment> {
  const res = await apiPost<ApiResponse<ErpInvPriceAdjustment>>(BASE, payload);
  return res.data;
}

export async function deleteInvPriceAdjustment(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}

/**
 * Trigger the server-side cost recalculation for a PENDING/FAILED PA. The
 * backend derives recalc lines + deltas from moving-average cost — no body. */
export async function processInvPriceAdjustment(id: string): Promise<ErpInvPriceAdjustment> {
  const res = await apiPost<ApiResponse<ErpInvPriceAdjustment>>(`${BASE}/${id}/process`, {});
  return res.data;
}
