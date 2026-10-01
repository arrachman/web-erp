// ERP m6 Manufacturing — Work Order (WO).
// Master-detail document (header + input/output lines) on the erp-mfg-work-orders backend.
// Endpoint: /mfg/work-orders  (see apps/api-gateway/src/erp-mfg-work-orders).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/mfg/work-orders';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

/** A single input or output line on a Work Order. */
export interface ErpMfgWorkOrderLine {
  id?: string;
  lineType: 'INPUT' | 'OUTPUT';
  itemId: string;
  item?: ErpRef | null;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  costCenterId?: string | null;
  divisionId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpMfgWorkOrder {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  bomId?: string | null;
  bom?: { id: string; docNumber: string } | null;
  docDate: string;
  fiscalPeriodId: string;
  description?: string | null;
  notes?: string | null;
  referenceNo?: string | null;
  status: ErpDocumentStatus;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpMfgWorkOrderLine[];
}

export interface MfgWorkOrderLinePayload {
  lineType: 'INPUT' | 'OUTPUT';
  itemId: string;
  quantity: string;
  unitId: string;
  warehouseId?: string;
  costCenterId?: string;
  divisionId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateMfgWorkOrderPayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  warehouseId?: string;
  bomId?: string;
  description?: string;
  notes?: string;
  referenceNo?: string;
  legacyCode?: string;
  lines: MfgWorkOrderLinePayload[];
}

export type UpdateMfgWorkOrderPayload = Partial<CreateMfgWorkOrderPayload>;

export type MfgWorkOrderTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'REOPEN';

export interface ListMfgWorkOrdersParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  warehouseId?: string;
  bomId?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listMfgWorkOrders(
  params?: ListMfgWorkOrdersParams,
): Promise<PaginatedResponse<ErpMfgWorkOrder>> {
  return apiGet<PaginatedResponse<ErpMfgWorkOrder>>(BASE, params as Query);
}

export async function getMfgWorkOrder(id: string): Promise<ErpMfgWorkOrder> {
  const res = await apiGet<ApiResponse<ErpMfgWorkOrder>>(`${BASE}/${id}`);
  return res.data;
}

export async function createMfgWorkOrder(
  payload: CreateMfgWorkOrderPayload,
): Promise<ErpMfgWorkOrder> {
  const res = await apiPost<ApiResponse<ErpMfgWorkOrder>>(BASE, payload);
  return res.data;
}

export async function updateMfgWorkOrder(
  id: string,
  payload: UpdateMfgWorkOrderPayload,
): Promise<ErpMfgWorkOrder> {
  const res = await apiPatch<ApiResponse<ErpMfgWorkOrder>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionMfgWorkOrder(
  id: string,
  action: MfgWorkOrderTransition,
  reason?: string,
): Promise<ErpMfgWorkOrder> {
  const res = await apiPost<ApiResponse<ErpMfgWorkOrder>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteMfgWorkOrder(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
