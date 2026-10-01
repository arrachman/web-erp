// ERP m4 Purchasing — Purchase Order (PO).
// Master-detail document (header + item lines) on the erp-pur-orders backend.
// Endpoint: /pur/orders  (see apps/api-gateway/src/erp-pur-orders).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/pur/orders';

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
export type ErpPriceMode = 'TAX_INCLUSIVE' | 'TAX_EXCLUSIVE';

/** Resolved cross-domain reference (supplier/item/unit/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpPurOrderLine {
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

export interface ErpPurOrder {
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
  lines: ErpPurOrderLine[];
}

export interface PurOrderLinePayload {
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

export interface CreatePurOrderPayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  warehouseId?: string;
  supplierId?: string;
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
  lines: PurOrderLinePayload[];
}

export type UpdatePurOrderPayload = Partial<CreatePurOrderPayload>;

export type PurOrderTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListPurOrdersParams extends PaginationParams {
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

export function listPurOrders(
  params?: ListPurOrdersParams,
): Promise<PaginatedResponse<ErpPurOrder>> {
  return apiGet<PaginatedResponse<ErpPurOrder>>(BASE, params as Query);
}

export async function getPurOrder(id: string): Promise<ErpPurOrder> {
  const res = await apiGet<ApiResponse<ErpPurOrder>>(`${BASE}/${id}`);
  return res.data;
}

export async function createPurOrder(payload: CreatePurOrderPayload): Promise<ErpPurOrder> {
  const res = await apiPost<ApiResponse<ErpPurOrder>>(BASE, payload);
  return res.data;
}

export async function updatePurOrder(
  id: string,
  payload: UpdatePurOrderPayload,
): Promise<ErpPurOrder> {
  const res = await apiPatch<ApiResponse<ErpPurOrder>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionPurOrder(
  id: string,
  action: PurOrderTransition,
  reason?: string,
): Promise<ErpPurOrder> {
  const res = await apiPost<ApiResponse<ErpPurOrder>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deletePurOrder(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
