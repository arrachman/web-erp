// ERP m5 Sales — Delivery Order (DO).
// Master-detail document (header + item lines) on the erp-sls-delivery-orders backend.
// Endpoint: /sls/delivery-orders  (see apps/api-gateway/src/erp-sls-delivery-orders).

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

export interface SlsDeliveryOrderListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsDeliveryOrderListResponse {
  success: boolean;
  data: ErpSlsDeliveryOrder[];
  meta: SlsDeliveryOrderListMeta;
}

const BASE = '/sls/delivery-orders';

export interface ErpSlsDeliveryOrderLine {
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

export interface ErpSlsDeliveryOrder {
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
  orderId?: string | null;
  customFields?: Record<string, unknown> | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpSlsDeliveryOrderLine[];
}

export interface CreateSlsDeliveryOrderPayload {
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
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  customFields?: Record<string, unknown>;
  lines: SlsOrderLinePayload[];
}

export type UpdateSlsDeliveryOrderPayload = Partial<Omit<CreateSlsDeliveryOrderPayload, 'lines'>> & {
  lines?: SlsOrderLinePayload[];
};

export type SlsDeliveryOrderTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsDeliveryOrdersParams extends PaginationParams {
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

export function listSlsDeliveryOrders(
  params?: ListSlsDeliveryOrdersParams,
): Promise<SlsDeliveryOrderListResponse> {
  return apiGet<SlsDeliveryOrderListResponse>(BASE, params as Query);
}

export async function getSlsDeliveryOrder(id: string): Promise<ErpSlsDeliveryOrder> {
  const res = await apiGet<ApiResponse<ErpSlsDeliveryOrder>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsDeliveryOrder(
  payload: CreateSlsDeliveryOrderPayload,
): Promise<ErpSlsDeliveryOrder> {
  const res = await apiPost<ApiResponse<ErpSlsDeliveryOrder>>(BASE, payload);
  return res.data;
}

export async function updateSlsDeliveryOrder(
  id: string,
  payload: UpdateSlsDeliveryOrderPayload,
): Promise<ErpSlsDeliveryOrder> {
  const res = await apiPatch<ApiResponse<ErpSlsDeliveryOrder>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsDeliveryOrder(
  id: string,
  action: SlsDeliveryOrderTransition,
  reason?: string,
): Promise<ErpSlsDeliveryOrder> {
  const res = await apiPost<ApiResponse<ErpSlsDeliveryOrder>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsDeliveryOrder(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
