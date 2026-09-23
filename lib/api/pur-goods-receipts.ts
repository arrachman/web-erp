// ERP m4 Purchasing — Goods Receipt (GRN).
// Item lines + QC delta (acceptedQty/rejectedQty/quarantineQty/qcStatus).
// Endpoint: /pur/goods-receipts

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type { ErpDocumentStatus, ErpPostingStatus, ErpPriceMode, ErpRef } from './pur-orders';

const BASE = '/pur/goods-receipts';
export type { ErpDocumentStatus, ErpPostingStatus, ErpPriceMode, ErpRef } from './pur-orders';
export type ErpQcStatus = 'PENDING' | 'PASSED' | 'FAILED' | 'PARTIAL';

export interface ErpPurGoodsReceiptLine {
  id?: string;
  itemId: string; item?: ErpRef | null;
  quantity: string; unitId: string; unit?: ErpRef | null;
  unitPrice: string; unitCost?: string | null;
  acceptedQty: string; rejectedQty: string; quarantineQty: string;
  qcStatus: ErpQcStatus;
  discountPercent?: string | null; discountAmount?: string | null;
  tax1Id?: string | null; tax1?: ErpRef | null; tax1Amount?: string | null;
  tax2Id?: string | null; tax2?: ErpRef | null; tax2Amount?: string | null;
  warehouseId?: string | null; warehouse?: ErpRef | null;
  inventoryAccountId?: string | null; accruedPayableAccountId?: string | null;
  orderLineId?: string | null;
  costCenterId?: string | null; divisionId?: string | null; subdivisionId?: string | null; projectId?: string | null;
  notes?: string | null; lineNo: number;
}

export interface ErpPurGoodsReceipt {
  id: string; docNumber: string; autoNumber?: string | null;
  branchId: string; branch?: ErpRef | null;
  locationId?: string | null; location?: ErpRef | null;
  warehouseId?: string | null; warehouse?: ErpRef | null;
  docDate: string; fiscalPeriodId: string;
  supplierId?: string | null; supplier?: ErpRef | null;
  paymentTermId?: string | null; paymentTerm?: ErpRef | null;
  dueDate?: string | null; currencyId: string; currency?: ErpRef | null;
  exchangeRate: string; priceMode: ErpPriceMode;
  subtotal: string; grandTotal: string;
  description?: string | null; notes?: string | null;
  referenceNo?: string | null;
  payableAccountId?: string | null; payableAccount?: ErpRef | null;
  orderId?: string | null;
  status: ErpDocumentStatus; postingStatus: ErpPostingStatus; postedAt?: string | null;
  legacyCode?: string | null; createdAt: string; updatedAt: string;
  createdById?: string | null; updatedById?: string | null;
  lines: ErpPurGoodsReceiptLine[];
}

export interface PurGoodsReceiptLinePayload {
  itemId: string; quantity: string; unitId: string;
  unitPrice?: string; unitCost?: string;
  acceptedQty?: string; rejectedQty?: string; quarantineQty?: string; qcStatus?: ErpQcStatus;
  discountPercent?: string; discountAmount?: string;
  tax1Id?: string; tax1Amount?: string; tax2Id?: string; tax2Amount?: string;
  warehouseId?: string; inventoryAccountId?: string; accruedPayableAccountId?: string;
  orderLineId?: string;
  costCenterId?: string; divisionId?: string; subdivisionId?: string; projectId?: string;
  notes?: string; lineNo: number;
}

export interface CreatePurGoodsReceiptPayload {
  docNumber?: string; auto?: boolean; docDate: string; fiscalPeriodId?: string;
  branchId: string; locationId?: string; warehouseId?: string;
  supplierId?: string; paymentTermId?: string; dueDate?: string;
  currencyId: string; exchangeRate: string; priceMode?: ErpPriceMode;
  description?: string; notes?: string; referenceNo?: string; referenceDate?: string;
  payableAccountId?: string; orderId?: string;
  discountPercent?: string; discountAmount?: string;
  tax1Amount?: string; tax2Amount?: string; otherCostAmount?: string;
  legacyCode?: string; lines: PurGoodsReceiptLinePayload[];
}

export type UpdatePurGoodsReceiptPayload = Partial<CreatePurGoodsReceiptPayload>;
export type PurGoodsReceiptTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListPurGoodsReceiptsParams extends PaginationParams {
  sortBy?: string; sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string; dateTo?: string; branchId?: string;
  supplierId?: string; locationId?: string;
  docNumberFrom?: string; docNumberTo?: string;
  description?: string; createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listPurGoodsReceipts(p?: ListPurGoodsReceiptsParams): Promise<PaginatedResponse<ErpPurGoodsReceipt>> {
  return apiGet<PaginatedResponse<ErpPurGoodsReceipt>>(BASE, p as Query);
}
export async function getPurGoodsReceipt(id: string): Promise<ErpPurGoodsReceipt> {
  return (await apiGet<ApiResponse<ErpPurGoodsReceipt>>(`${BASE}/${id}`)).data;
}
export async function createPurGoodsReceipt(payload: CreatePurGoodsReceiptPayload): Promise<ErpPurGoodsReceipt> {
  return (await apiPost<ApiResponse<ErpPurGoodsReceipt>>(BASE, payload)).data;
}
export async function updatePurGoodsReceipt(id: string, payload: UpdatePurGoodsReceiptPayload): Promise<ErpPurGoodsReceipt> {
  return (await apiPatch<ApiResponse<ErpPurGoodsReceipt>>(`${BASE}/${id}`, payload)).data;
}
export async function transitionPurGoodsReceipt(id: string, action: PurGoodsReceiptTransition, reason?: string): Promise<ErpPurGoodsReceipt> {
  return (await apiPost<ApiResponse<ErpPurGoodsReceipt>>(`${BASE}/${id}/transition`, { action, reason })).data;
}
export async function deletePurGoodsReceipt(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
