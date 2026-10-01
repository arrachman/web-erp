// ERP m3 Inventory — Stock Movements (Material Request / Stock Transfer /
// Transfer Receipt / Fuel Refill). One master-detail document (header + lines)
// discriminated by `movementType`, on the erp-inv-stock-movements backend.
// Endpoint: /inv/stock-movements (see apps/api-gateway/src/erp-inv-stock-movements).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/stock-movements';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/**
 * Movement discriminator. One backend, four transaction faces:
 * REQUEST = Material Request (MR), TRANSFER = Stock Transfer (TS),
 * TRANSFER_RECEIPT = Transfer Receipt (RS), ISSUE = Fuel Refill (RF).
 * RETURN reserved by the backend for future use.
 */
export type ErpInvMovementType =
  | 'REQUEST'
  | 'TRANSFER'
  | 'TRANSFER_RECEIPT'
  | 'ISSUE'
  | 'RETURN';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvStockMovementLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  unitCost?: string | null;
  salePrice?: string | null;
  sourceWarehouseId?: string | null;
  sourceWarehouse?: ErpRef | null;
  destinationWarehouseId?: string | null;
  destinationWarehouse?: ErpRef | null;
  costCenterId?: string | null;
  costCenter?: ErpRef | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpInvStockMovement {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  movementType: ErpInvMovementType;
  movementDate: string;
  fiscalPeriodId: string;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  sourceWarehouseId?: string | null;
  sourceWarehouse?: ErpRef | null;
  transitWarehouseId?: string | null;
  transitWarehouse?: ErpRef | null;
  destinationWarehouseId?: string | null;
  destinationWarehouse?: ErpRef | null;
  requestedPartnerId?: string | null;
  requestedPartner?: ErpRef | null;
  requestedTo?: string | null;
  neededDate?: string | null;
  referenceNo?: string | null;
  referenceDate?: string | null;
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
  lines: ErpInvStockMovementLine[];
}

export interface InvStockMovementLinePayload {
  itemId: string;
  quantity: string;
  unitId: string;
  unitCost?: string;
  salePrice?: string;
  sourceWarehouseId?: string;
  destinationWarehouseId?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateInvStockMovementPayload {
  auto?: boolean;
  docNumber?: string;
  movementType: ErpInvMovementType;
  movementDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  sourceWarehouseId?: string;
  transitWarehouseId?: string;
  destinationWarehouseId?: string;
  requestedPartnerId?: string;
  requestedTo?: string;
  neededDate?: string;
  referenceNo?: string;
  referenceDate?: string;
  description?: string;
  notes?: string;
  legacyCode?: string;
  lines: InvStockMovementLinePayload[];
}

export type UpdateInvStockMovementPayload = Partial<CreateInvStockMovementPayload>;

export type InvStockMovementTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

export interface ListInvStockMovementsParams extends PaginationParams {
  sortBy?: 'movementDate' | 'docNumber' | 'status' | 'createdAt';
  sortDir?: 'asc' | 'desc';
  movementType?: ErpInvMovementType;
  status?: ErpDocumentStatus;
  branchId?: string;
  locationId?: string;
  warehouseId?: string;
  dateFrom?: string;
  dateTo?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listInvStockMovements(
  params?: ListInvStockMovementsParams,
): Promise<PaginatedResponse<ErpInvStockMovement>> {
  return apiGet<PaginatedResponse<ErpInvStockMovement>>(BASE, params as Query);
}

export async function getInvStockMovement(id: string): Promise<ErpInvStockMovement> {
  const res = await apiGet<ApiResponse<ErpInvStockMovement>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvStockMovement(
  payload: CreateInvStockMovementPayload,
): Promise<ErpInvStockMovement> {
  const res = await apiPost<ApiResponse<ErpInvStockMovement>>(BASE, payload);
  return res.data;
}

export async function updateInvStockMovement(
  id: string,
  payload: UpdateInvStockMovementPayload,
): Promise<ErpInvStockMovement> {
  const res = await apiPatch<ApiResponse<ErpInvStockMovement>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionInvStockMovement(
  id: string,
  action: InvStockMovementTransition,
  reason?: string,
): Promise<ErpInvStockMovement> {
  const res = await apiPost<ApiResponse<ErpInvStockMovement>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteInvStockMovement(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
