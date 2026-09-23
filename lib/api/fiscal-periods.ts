// ERP Fiscal Period resource API — CRUD + open/close for sys_fiscal_periods
// Endpoints: /fiscal-periods, /fiscal-periods/:id/open, /fiscal-periods/:id/close

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpFiscalPeriodStatus =
  | 'OPEN'
  | 'SOFT_CLOSED'
  | 'CLOSED'
  | 'REOPENED';

export interface ErpFiscalPeriod {
  id: string;
  year: number;
  periodNo: number;
  name: string;
  startDate: string;
  endDate: string;
  status: ErpFiscalPeriodStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFiscalPeriodPayload {
  year: number;
  periodNo: number;
  name: string;
  startDate: string;
  endDate: string;
  status?: ErpFiscalPeriodStatus;
}

export type UpdateFiscalPeriodPayload = Partial<CreateFiscalPeriodPayload>;

// ─── API functions ────────────────────────────────────────────────────────────

export async function listFiscalPeriods(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpFiscalPeriod>> {
  return apiGet<PaginatedResponse<ErpFiscalPeriod>>(
    '/fiscal-periods',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createFiscalPeriod(
  payload: CreateFiscalPeriodPayload,
): Promise<ErpFiscalPeriod> {
  const res = await apiPost<ApiResponse<ErpFiscalPeriod>>(
    '/fiscal-periods',
    payload,
  );
  return res.data;
}

export async function updateFiscalPeriod(
  id: string,
  payload: UpdateFiscalPeriodPayload,
): Promise<ErpFiscalPeriod> {
  const res = await apiPatch<ApiResponse<ErpFiscalPeriod>>(
    `/fiscal-periods/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteFiscalPeriod(id: string): Promise<void> {
  await apiDelete<void>(`/fiscal-periods/${id}`);
}

export async function openFiscalPeriod(
  id: string,
): Promise<ErpFiscalPeriod> {
  const res = await apiPatch<ApiResponse<ErpFiscalPeriod>>(
    `/fiscal-periods/${id}/open`,
  );
  return res.data;
}

export async function closeFiscalPeriod(
  id: string,
): Promise<ErpFiscalPeriod> {
  const res = await apiPatch<ApiResponse<ErpFiscalPeriod>>(
    `/fiscal-periods/${id}/close`,
  );
  return res.data;
}
