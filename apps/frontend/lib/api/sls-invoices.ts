// ERP m5 Sales — Sales Invoice (SI).
// Master-detail document (header + item lines) on the erp-sls-invoices backend.
// Endpoint: /sls/invoices  (see apps/api-gateway/src/erp-sls-invoices).
// SI posts GL: AR (receivable) + revenue accounts on POST transition.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedMeta, PaginationParams } from './types';
import type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPriceMode,
  ErpRef,
  SlsOrderLinePayload,
} from './sls-orders';

export type { ErpDocumentStatus, ErpPostingStatus, ErpPriceMode, ErpRef };

export type SlsSettlementStatus = 'UNPAID' | 'PARTIAL' | 'PAID';

export interface SlsInvoiceListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsInvoiceListResponse {
  success: boolean;
  data: ErpSlsInvoice[];
  meta: SlsInvoiceListMeta;
}

const BASE = '/sls/invoices';

export interface ErpSlsInvoiceLine {
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
  customFields?: Record<string, unknown> | null;
  lineNo: number;
}

export interface ErpSlsInvoice {
  id: string;
  code: string;
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
  customerId?: string | null;
  customer?: ErpRef | null;
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
  salesDeptId?: string | null;
  salesDept?: ErpRef | null;
  receivableAccountId?: string | null;
  receivableAccount?: ErpRef | null;
  // SI-specific fields
  orderId?: string | null;
  deliveryOrderId?: string | null;
  advanceId?: string | null;
  advanceAmount?: string | null;
  taxInvoiceNo?: string | null;
  channel?: string | null;
  isOpeningBalance?: boolean;
  settlementStatus?: SlsSettlementStatus | null;
  customFields?: Record<string, unknown> | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpSlsInvoiceLine[];
}

export interface CreateSlsInvoicePayload {
  docNumber?: string;
  auto?: boolean;
  docDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  warehouseId?: string;
  customerId?: string;
  paymentTermId?: string;
  dueDate?: string;
  currencyId: string;
  exchangeRate: string;
  priceMode?: ErpPriceMode;
  description?: string;
  notes?: string;
  referenceNo?: string;
  referenceDate?: string;
  receivableAccountId?: string;
  salesDeptId?: string;
  orderId?: string;
  deliveryOrderId?: string;
  advanceId?: string;
  advanceAmount?: string;
  taxInvoiceNo?: string;
  channel?: string;
  isOpeningBalance?: boolean;
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  customFields?: Record<string, unknown>;
  lines: SlsOrderLinePayload[];
}

export type UpdateSlsInvoicePayload = Partial<Omit<CreateSlsInvoicePayload, 'lines'>> & {
  lines?: SlsOrderLinePayload[];
};

export type SlsInvoiceTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsInvoicesParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  customerId?: string;
  locationId?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  createdById?: string;
  isOpeningBalance?: boolean;
  settlementStatus?: SlsSettlementStatus;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listSlsInvoices(
  params?: ListSlsInvoicesParams,
): Promise<SlsInvoiceListResponse> {
  return apiGet<SlsInvoiceListResponse>(BASE, params as Query);
}

export async function getSlsInvoice(id: string): Promise<ErpSlsInvoice> {
  const res = await apiGet<ApiResponse<ErpSlsInvoice>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsInvoice(payload: CreateSlsInvoicePayload): Promise<ErpSlsInvoice> {
  const res = await apiPost<ApiResponse<ErpSlsInvoice>>(BASE, payload);
  return res.data;
}

export async function updateSlsInvoice(
  id: string,
  payload: UpdateSlsInvoicePayload,
): Promise<ErpSlsInvoice> {
  const res = await apiPatch<ApiResponse<ErpSlsInvoice>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsInvoice(
  id: string,
  action: SlsInvoiceTransition,
  reason?: string,
): Promise<ErpSlsInvoice> {
  const res = await apiPost<ApiResponse<ErpSlsInvoice>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsInvoice(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
