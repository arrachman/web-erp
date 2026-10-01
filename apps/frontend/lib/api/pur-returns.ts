// ERP m4 Purchasing — Purchase Returns (shared endpoint for DNR + PRT).
// ONE backend table `pur_returns` serves two menu transactions via `returnType`:
//   DEBIT_NOTE        = Return Shipment (DNR, m4_dnr)
//   RETURN_TO_VENDOR  = Purchase Return  (PRT, m4_prt)
// Endpoint: /pur/returns (see apps/api-gateway/src/erp-pur-returns).

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPriceMode,
  ErpRef,
} from './pur-orders';

const BASE = '/pur/returns';

export type { ErpDocumentStatus, ErpPostingStatus, ErpPriceMode, ErpRef } from './pur-orders';
export type ErpSettlementStatus = 'UNPAID' | 'PARTIAL' | 'PAID';
export type PurchaseReturnType = 'DEBIT_NOTE' | 'RETURN_TO_VENDOR';

export interface ErpPurReturnLine {
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

export interface ErpPurReturn {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  returnType: PurchaseReturnType;
  settlementStatus: ErpSettlementStatus;
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
  taxInvoiceNo?: string | null;
  orderId?: string | null;
  goodsReceiptId?: string | null;
  invoiceId?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpPurReturnLine[];
}

export interface PurReturnLinePayload {
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

export interface CreatePurReturnPayload {
  returnType: PurchaseReturnType;
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
  taxInvoiceNo?: string;
  orderId?: string;
  goodsReceiptId?: string;
  invoiceId?: string;
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  lines: PurReturnLinePayload[];
}

export type UpdatePurReturnPayload = Partial<CreatePurReturnPayload>;
export type PurReturnTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListPurReturnsParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  returnType?: PurchaseReturnType;
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

export function listPurReturns(
  params?: ListPurReturnsParams,
): Promise<PaginatedResponse<ErpPurReturn>> {
  return apiGet<PaginatedResponse<ErpPurReturn>>(BASE, params as Query);
}

export async function getPurReturn(id: string): Promise<ErpPurReturn> {
  const res = await apiGet<ApiResponse<ErpPurReturn>>(`${BASE}/${id}`);
  return res.data;
}

export async function createPurReturn(payload: CreatePurReturnPayload): Promise<ErpPurReturn> {
  const res = await apiPost<ApiResponse<ErpPurReturn>>(BASE, payload);
  return res.data;
}

export async function updatePurReturn(
  id: string,
  payload: UpdatePurReturnPayload,
): Promise<ErpPurReturn> {
  const res = await apiPatch<ApiResponse<ErpPurReturn>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionPurReturn(
  id: string,
  action: PurReturnTransition,
  reason?: string,
): Promise<ErpPurReturn> {
  const res = await apiPost<ApiResponse<ErpPurReturn>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deletePurReturn(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
