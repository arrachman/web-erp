// ERP m5 Sales — Delivery Report (DR).
// Master-detail document (header + item lines) on the erp-sls-delivery-reports backend.
// Endpoint: /sls/delivery-reports  (see apps/api-gateway/src/erp-sls-delivery-reports).

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

export interface SlsDeliveryReportListMeta extends PaginatedMeta {
  sumGrandTotal: string;
}
export interface SlsDeliveryReportListResponse {
  success: boolean;
  data: ErpSlsDeliveryReport[];
  meta: SlsDeliveryReportListMeta;
}

const BASE = '/sls/delivery-reports';

export interface ErpSlsDeliveryReportLine {
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

export interface ErpSlsDeliveryReport {
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
  deliveryOrderId?: string | null;
  customFields?: Record<string, unknown> | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpSlsDeliveryReportLine[];
}

export interface CreateSlsDeliveryReportPayload {
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
  deliveryOrderId?: string;
  discountPercent?: string;
  discountAmount?: string;
  tax1Amount?: string;
  tax2Amount?: string;
  otherCostAmount?: string;
  legacyCode?: string;
  customFields?: Record<string, unknown>;
  lines: SlsOrderLinePayload[];
}

export type UpdateSlsDeliveryReportPayload = Partial<Omit<CreateSlsDeliveryReportPayload, 'lines'>> & {
  lines?: SlsOrderLinePayload[];
};

export type SlsDeliveryReportTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export interface ListSlsDeliveryReportsParams extends PaginationParams {
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

export function listSlsDeliveryReports(
  params?: ListSlsDeliveryReportsParams,
): Promise<SlsDeliveryReportListResponse> {
  return apiGet<SlsDeliveryReportListResponse>(BASE, params as Query);
}

export async function getSlsDeliveryReport(id: string): Promise<ErpSlsDeliveryReport> {
  const res = await apiGet<ApiResponse<ErpSlsDeliveryReport>>(`${BASE}/${id}`);
  return res.data;
}

export async function createSlsDeliveryReport(
  payload: CreateSlsDeliveryReportPayload,
): Promise<ErpSlsDeliveryReport> {
  const res = await apiPost<ApiResponse<ErpSlsDeliveryReport>>(BASE, payload);
  return res.data;
}

export async function updateSlsDeliveryReport(
  id: string,
  payload: UpdateSlsDeliveryReportPayload,
): Promise<ErpSlsDeliveryReport> {
  const res = await apiPatch<ApiResponse<ErpSlsDeliveryReport>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionSlsDeliveryReport(
  id: string,
  action: SlsDeliveryReportTransition,
  reason?: string,
): Promise<ErpSlsDeliveryReport> {
  const res = await apiPost<ApiResponse<ErpSlsDeliveryReport>>(`${BASE}/${id}/transition`, {
    action,
    reason,
  });
  return res.data;
}

export async function deleteSlsDeliveryReport(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
