// ERP m5 Sales — Sales Quotation (SQ).
// Master-detail document (header + item lines) on the erp-sls-quotations backend.
// Endpoint: /sls/quotations

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedMeta, PaginationParams } from './types';

export type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPriceMode,
  ErpRef,
  ItemAutoFill,
  PartnerAutoFill,
} from './sls-orders';
export { getItemForAutoFill, getPartnerForAutoFill } from './sls-orders';

export interface SlsQuotationListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsQuotationListResponse {
  success: boolean;
  data: ErpSlsQuotation[];
  meta: SlsQuotationListMeta;
}

import type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPriceMode,
  ErpRef,
} from './sls-orders';

const BASE = '/sls/quotations';

export interface ErpSlsQuotationLine {
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

export interface ErpSlsQuotation {
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
  customFields?: Record<string, unknown> | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpSlsQuotationLine[];
}

export interface SlsQuotationLinePayload {
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
  customFields?: Record<string, unknown>;
  lineNo: number;
}

export interface CreateSlsQuotationPayload {
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
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  customFields?: Record<string, unknown>;
  lines: SlsQuotationLinePayload[];
}

export type UpdateSlsQuotationPayload = Partial<Omit<CreateSlsQuotationPayload, 'lines'>> & {
  lines?: SlsQuotationLinePayload[];
};

export type SlsQuotationTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsQuotationsParams extends PaginationParams {
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
}

type Query = Record<string, string | number | boolean | undefined>;

export function listSlsQuotations(
  params?: ListSlsQuotationsParams,
): Promise<SlsQuotationListResponse> {
  return apiGet<SlsQuotationListResponse>(BASE, params as Query);
}

export async function getSlsQuotation(id: string): Promise<ErpSlsQuotation> {
  const res = await apiGet<ApiResponse<ErpSlsQuotation>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsQuotation(
  payload: CreateSlsQuotationPayload,
): Promise<ErpSlsQuotation> {
  const res = await apiPost<ApiResponse<ErpSlsQuotation>>(BASE, payload);
  return res.data;
}

export async function updateSlsQuotation(
  id: string,
  payload: UpdateSlsQuotationPayload,
): Promise<ErpSlsQuotation> {
  const res = await apiPatch<ApiResponse<ErpSlsQuotation>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsQuotation(
  id: string,
  action: SlsQuotationTransition,
  reason?: string,
): Promise<ErpSlsQuotation> {
  const res = await apiPost<ApiResponse<ErpSlsQuotation>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsQuotation(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
