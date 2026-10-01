// ERP m3 Inventory — Opening Stock (Saldo Awal Persediaan). One master-detail
// document (header + lines) that seeds initial on-hand quantities/values per item
// for a warehouse. Endpoint: /inv/opening-stocks
// (see apps/api-gateway/src/erp-inv-opening-stocks).
//
// Unlike stock movements, the inventory GL account per line is resolved
// SERVER-SIDE (from item / warehouse config) — it is NOT part of the grid or the
// create/update payload, only echoed back on the enriched response line.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/opening-stocks';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/** Resolved cross-domain reference (item/unit/warehouse/account/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvOpeningStockLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  unitCost?: string | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  /** Resolved server-side from item/warehouse config; read-only on the line. */
  inventoryAccountId?: string | null;
  inventoryAccount?: ErpRef | null;
  costCenterId?: string | null;
  costCenter?: ErpRef | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpInvOpeningStock {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  openingDate: string;
  fiscalPeriodId: string;
  branchId: string;
  branch?: ErpRef | null;
  warehouseId: string;
  warehouse?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  kind?: string | null;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
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
  lines: ErpInvOpeningStockLine[];
}

export interface InvOpeningStockLinePayload {
  itemId: string;
  quantity: string;
  unitId: string;
  unitCost: string;
  warehouseId?: string;
  inventoryAccountId?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreateInvOpeningStockPayload {
  auto?: boolean;
  docNumber?: string;
  openingDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  warehouseId: string;
  locationId?: string;
  kind?: string;
  currencyId: string;
  exchangeRate: string;
  description?: string;
  notes?: string;
  status?: ErpDocumentStatus;
  legacyCode?: string;
  lines: InvOpeningStockLinePayload[];
}

export type UpdateInvOpeningStockPayload = Partial<CreateInvOpeningStockPayload>;

export type InvOpeningStockTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

export interface ListInvOpeningStocksParams extends PaginationParams {
  sortBy?: 'openingDate' | 'docNumber' | 'status' | 'createdAt';
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

export function listInvOpeningStocks(
  params?: ListInvOpeningStocksParams,
): Promise<PaginatedResponse<ErpInvOpeningStock>> {
  return apiGet<PaginatedResponse<ErpInvOpeningStock>>(BASE, params as Query);
}

export async function getInvOpeningStock(id: string): Promise<ErpInvOpeningStock> {
  const res = await apiGet<ApiResponse<ErpInvOpeningStock>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvOpeningStock(
  payload: CreateInvOpeningStockPayload,
): Promise<ErpInvOpeningStock> {
  const res = await apiPost<ApiResponse<ErpInvOpeningStock>>(BASE, payload);
  return res.data;
}

export async function updateInvOpeningStock(
  id: string,
  payload: UpdateInvOpeningStockPayload,
): Promise<ErpInvOpeningStock> {
  const res = await apiPatch<ApiResponse<ErpInvOpeningStock>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionInvOpeningStock(
  id: string,
  action: InvOpeningStockTransition,
  reason?: string,
): Promise<ErpInvOpeningStock> {
  const res = await apiPost<ApiResponse<ErpInvOpeningStock>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteInvOpeningStock(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
