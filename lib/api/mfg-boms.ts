// ERP m6 Manufacturing — Bill of Materials (BOM)
// Master-detail: header + inputs (material consumed) + outputs (product produced)
// Endpoint: /mfg/boms

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/mfg/boms';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';
export type MfgBomTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'REOPEN';

export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpMfgBomLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  unitPrice: string;
  unitCost: string;
  costPercent?: string | null;
  sourceWarehouseId?: string | null;
  productionWarehouseId?: string | null;
  destinationWarehouseId?: string | null;
  inventoryAccountId?: string | null;
  costCenterId?: string | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpMfgBom {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  sourceWarehouseId?: string | null;
  sourceWarehouse?: ErpRef | null;
  productionWarehouseId?: string | null;
  productionWarehouse?: ErpRef | null;
  destinationWarehouseId?: string | null;
  destinationWarehouse?: ErpRef | null;
  docDate: string;
  fiscalPeriodId: string;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
  neededDate?: string | null;
  workEstimate?: string | null;
  inputTotalPrice?: string | null;
  outputTotalPrice?: string | null;
  description?: string | null;
  notes?: string | null;
  referenceNo?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  inputs: ErpMfgBomLine[];
  outputs: ErpMfgBomLine[];
}

export interface MfgBomLinePayload {
  itemId: string;
  quantity: string;
  unitId: string;
  unitPrice?: string;
  unitCost?: string;
  costPercent?: string;
  sourceWarehouseId?: string;
  productionWarehouseId?: string;
  destinationWarehouseId?: string;
  inventoryAccountId?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateMfgBomPayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  sourceWarehouseId?: string;
  productionWarehouseId?: string;
  destinationWarehouseId?: string;
  currencyId: string;
  exchangeRate: string;
  neededDate?: string;
  workEstimate?: string;
  description?: string;
  notes?: string;
  referenceNo?: string;
  referenceDate?: string;
  legacyCode?: string;
  inputs: MfgBomLinePayload[];
  outputs: MfgBomLinePayload[];
}

export type UpdateMfgBomPayload = Partial<CreateMfgBomPayload>;

export interface ListMfgBomsParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  search?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listMfgBoms(
  params?: ListMfgBomsParams,
): Promise<PaginatedResponse<ErpMfgBom>> {
  return apiGet<PaginatedResponse<ErpMfgBom>>(BASE, params as Query);
}

export async function getMfgBom(id: string): Promise<ErpMfgBom> {
  const res = await apiGet<ApiResponse<ErpMfgBom>>(`${BASE}/${id}`);
  return res.data;
}

export async function createMfgBom(payload: CreateMfgBomPayload): Promise<ErpMfgBom> {
  const res = await apiPost<ApiResponse<ErpMfgBom>>(BASE, payload);
  return res.data;
}

export async function updateMfgBom(
  id: string,
  payload: UpdateMfgBomPayload,
): Promise<ErpMfgBom> {
  const res = await apiPatch<ApiResponse<ErpMfgBom>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionMfgBom(
  id: string,
  action: MfgBomTransition,
  reason?: string,
): Promise<ErpMfgBom> {
  const res = await apiPost<ApiResponse<ErpMfgBom>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteMfgBom(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
