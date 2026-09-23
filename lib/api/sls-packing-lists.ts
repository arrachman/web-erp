// ERP m5 Sales — Packing List (PL).
// Master-detail document (header + item lines) on the erp-sls-packing-lists backend.
// Endpoint: /sls/packing-lists

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

export interface SlsPackingListListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsPackingListListResponse {
  success: boolean;
  data: ErpSlsPackingList[];
  meta: SlsPackingListListMeta;
}

import type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPriceMode,
  ErpRef,
} from './sls-orders';

const BASE = '/sls/packing-lists';

export interface ErpSlsPackingListLine {
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

export interface ErpSlsPackingList {
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
  lines: ErpSlsPackingListLine[];
}

export interface SlsPackingListLinePayload {
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

export interface CreateSlsPackingListPayload {
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
  lines: SlsPackingListLinePayload[];
}

export type UpdateSlsPackingListPayload = Partial<Omit<CreateSlsPackingListPayload, 'lines'>> & {
  lines?: SlsPackingListLinePayload[];
};

export type SlsPackingListTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsPackingListsParams extends PaginationParams {
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

export function listSlsPackingLists(
  params?: ListSlsPackingListsParams,
): Promise<SlsPackingListListResponse> {
  return apiGet<SlsPackingListListResponse>(BASE, params as Query);
}

export async function getSlsPackingList(id: string): Promise<ErpSlsPackingList> {
  const res = await apiGet<ApiResponse<ErpSlsPackingList>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsPackingList(
  payload: CreateSlsPackingListPayload,
): Promise<ErpSlsPackingList> {
  const res = await apiPost<ApiResponse<ErpSlsPackingList>>(BASE, payload);
  return res.data;
}

export async function updateSlsPackingList(
  id: string,
  payload: UpdateSlsPackingListPayload,
): Promise<ErpSlsPackingList> {
  const res = await apiPatch<ApiResponse<ErpSlsPackingList>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsPackingList(
  id: string,
  action: SlsPackingListTransition,
  reason?: string,
): Promise<ErpSlsPackingList> {
  const res = await apiPost<ApiResponse<ErpSlsPackingList>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsPackingList(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
