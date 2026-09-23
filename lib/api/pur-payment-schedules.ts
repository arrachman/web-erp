// ERP M4 Purchasing — Payment Schedule (VPP) CRUD client
// Backend: GET/POST /erp/pur/payment-schedules  (source='VPP')

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export type ErpDocumentStatus = 'DRAFT' | 'NEED_APPROVE' | 'APPROVED' | 'REJECTED' | 'POSTED' | 'VOID' | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

export interface ErpPaymentSchedule {
  id: string;
  docNumber: string;
  transactionDate: string;
  fiscalPeriodId: string;
  branchId: string;
  partner?: { id: string; code: string; name: string } | null;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  notes?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  source: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePaymentSchedulePayload {
  docNumber: string;
  transactionDate: string;
  fiscalPeriodId: string;
  branchId: string;
  partnerId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  amount: string;
  notes?: string;
}

export type UpdatePaymentSchedulePayload = Partial<CreatePaymentSchedulePayload>;

export interface ListPaymentSchedulesParams extends PaginationParams {
  search?: string;
  status?: ErpDocumentStatus;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export type PaymentScheduleTransition = 'SUBMIT' | 'APPROVE' | 'REJECT' | 'POST' | 'REOPEN';

export async function listPaymentSchedules(
  params?: ListPaymentSchedulesParams,
): Promise<PaginatedResponse<ErpPaymentSchedule>> {
  return apiGet<PaginatedResponse<ErpPaymentSchedule>>(
    '/pur/payment-schedules',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function getPaymentSchedule(id: string): Promise<ErpPaymentSchedule> {
  const res = await apiGet<ApiResponse<ErpPaymentSchedule>>(`/pur/payment-schedules/${id}`);
  return res.data;
}

export async function createPaymentSchedule(
  payload: CreatePaymentSchedulePayload,
): Promise<ErpPaymentSchedule> {
  const res = await apiPost<ApiResponse<ErpPaymentSchedule>>('/pur/payment-schedules', payload);
  return res.data;
}

export async function updatePaymentSchedule(
  id: string,
  payload: UpdatePaymentSchedulePayload,
): Promise<ErpPaymentSchedule> {
  const res = await apiPatch<ApiResponse<ErpPaymentSchedule>>(
    `/pur/payment-schedules/${id}`,
    payload,
  );
  return res.data;
}

export async function deletePaymentSchedule(id: string): Promise<void> {
  await apiDelete<void>(`/pur/payment-schedules/${id}`);
}

export async function transitionPaymentSchedule(
  id: string,
  action: PaymentScheduleTransition,
): Promise<ErpPaymentSchedule> {
  const res = await apiPost<ApiResponse<ErpPaymentSchedule>>(
    `/pur/payment-schedules/${id}/transition`,
    { action },
  );
  return res.data;
}
