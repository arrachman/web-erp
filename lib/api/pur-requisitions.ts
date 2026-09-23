// ERP m4 Purchasing — Purchase Requisition (PR).
// Master-detail document (header + item lines) on the erp-pur-requisitions backend.
// Endpoint: /pur/requisitions  (see apps/api-gateway/src/erp-pur-requisitions).
// Mirrors the Purchase Order client shape; no `code`, plus optional
// requestedById / neededDate (PR often runs pre-sourcing, before a supplier).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPriceMode,
  ErpRef,
} from './pur-orders';

const BASE = '/pur/requisitions';

export type { ErpDocumentStatus, ErpPostingStatus, ErpPriceMode, ErpRef } from './pur-orders';

export interface ErpPurRequisitionLine {
  id?: string;
  itemId: string;
  item?: ErpRef | null;
  quantity: string;
  unitId: string;
  unit?: ErpRef | null;
  unitPrice: string;
  discountPercent?: string | null;
  discountAmount?: string | null;
  tax1Id?: string | null;
  tax1?: ErpRef | null;
  tax1Amount?: string | null;
  tax2Id?: string | null;
  tax2?: ErpRef | null;
  tax2Amount?: string | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  inventoryAccountId?: string | null;
  costCenterId?: string | null;
  divisionId?: string | null;
  subdivisionId?: string | null;
  projectId?: string | null;
  notes?: string | null;
  lineNo: number;
}

export interface ErpPurRequisition {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  warehouseId?: string | null;
  warehouse?: ErpRef | null;
  docDate: string;
  fiscalPeriodId: string;
  supplierId?: string | null;
  supplier?: ErpRef | null;
  requestedById?: string | null;
  requestedBy?: ErpRef | null;
  neededDate?: string | null;
  paymentTermId?: string | null;
  paymentTerm?: ErpRef | null;
  dueDate?: string | null;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
  priceMode: ErpPriceMode;
  subtotal: string;
  discountPercent?: string | null;
  discountAmount?: string | null;
  tax1Amount?: string | null;
  tax2Amount?: string | null;
  otherCostAmount?: string | null;
  grandTotal: string;
  description?: string | null;
  notes?: string | null;
  referenceNo?: string | null;
  referenceDate?: string | null;
  payableAccountId?: string | null;
  payableAccount?: ErpRef | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpPurRequisitionLine[];
}

export interface PurRequisitionLinePayload {
  itemId: string;
  quantity: string;
  unitId: string;
  unitPrice?: string;
  discountPercent?: string;
  discountAmount?: string;
  tax1Id?: string;
  tax1Amount?: string;
  tax2Id?: string;
  tax2Amount?: string;
  warehouseId?: string;
  inventoryAccountId?: string;
  costCenterId?: string;
  divisionId?: string;
  subdivisionId?: string;
  projectId?: string;
  notes?: string;
  lineNo: number;
}

export interface CreatePurRequisitionPayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  warehouseId?: string;
  supplierId?: string;
  requestedById?: string;
  neededDate?: string;
  paymentTermId?: string;
  dueDate?: string;
  currencyId: string;
  exchangeRate: string;
  priceMode?: ErpPriceMode;
  description?: string;
  notes?: string;
  referenceNo?: string;
  referenceDate?: string;
  payableAccountId?: string;
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  lines: PurRequisitionLinePayload[];
}

export type UpdatePurRequisitionPayload = Partial<CreatePurRequisitionPayload>;

export type PurRequisitionTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListPurRequisitionsParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  supplierId?: string;
  locationId?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listPurRequisitions(
  params?: ListPurRequisitionsParams,
): Promise<PaginatedResponse<ErpPurRequisition>> {
  return apiGet<PaginatedResponse<ErpPurRequisition>>(BASE, params as Query);
}

export async function getPurRequisition(id: string): Promise<ErpPurRequisition> {
  const res = await apiGet<ApiResponse<ErpPurRequisition>>(`${BASE}/${id}`);
  return res.data;
}

export async function createPurRequisition(
  payload: CreatePurRequisitionPayload,
): Promise<ErpPurRequisition> {
  const res = await apiPost<ApiResponse<ErpPurRequisition>>(BASE, payload);
  return res.data;
}

export async function updatePurRequisition(
  id: string,
  payload: UpdatePurRequisitionPayload,
): Promise<ErpPurRequisition> {
  const res = await apiPatch<ApiResponse<ErpPurRequisition>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionPurRequisition(
  id: string,
  action: PurRequisitionTransition,
  reason?: string,
): Promise<ErpPurRequisition> {
  const res = await apiPost<ApiResponse<ErpPurRequisition>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deletePurRequisition(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
