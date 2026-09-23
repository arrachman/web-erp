// ERP Currency resource API — CRUD for md_currencies + dated rates sub-resource
// Endpoints: /currencies, /currencies/:id/rates

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpCurrency {
  id: string;
  code: string;
  name: string;
  symbol?: string | null;
  /** Display/entry decimal places for amounts (0–6). */
  decimalPlaces: number;
  /** Org home currency — at most one active base. */
  isBase: boolean;
  isActive: boolean;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCurrencyPayload {
  code: string;
  name: string;
  symbol?: string;
  decimalPlaces?: number;
  isBase?: boolean;
  isActive?: boolean;
}

export interface UpdateCurrencyPayload {
  code?: string;
  name?: string;
  symbol?: string;
  decimalPlaces?: number;
  isBase?: boolean;
  isActive?: boolean;
}

export interface ErpCurrencyRate {
  id: string;
  currencyId: string;
  rateDate: string;
  rate: number | string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCurrencyRatePayload {
  rateDate: string;
  rate: string;
  note?: string;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listCurrencies(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpCurrency>> {
  return apiGet<PaginatedResponse<ErpCurrency>>(
    '/currencies',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createCurrency(
  payload: CreateCurrencyPayload,
): Promise<ErpCurrency> {
  const res = await apiPost<ApiResponse<ErpCurrency>>('/currencies', payload);
  return res.data;
}

export async function updateCurrency(
  id: string,
  payload: UpdateCurrencyPayload,
): Promise<ErpCurrency> {
  const res = await apiPatch<ApiResponse<ErpCurrency>>(
    `/currencies/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteCurrency(id: string): Promise<void> {
  await apiDelete<void>(`/currencies/${id}`);
}

export async function bulkUpdateErpCurrencyStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/currencies/bulk/status',
    { ids, isActive },
  );
  return { affected: res.affected };
}

export async function bulkDeleteErpCurrencies(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/currencies/bulk',
    { ids },
  );
  return { affected: res.affected };
}

// ─── Rates sub-resource ────────────────────────────────────────────────────────

export async function listCurrencyRates(
  currencyId: string,
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpCurrencyRate>> {
  return apiGet<PaginatedResponse<ErpCurrencyRate>>(
    `/currencies/${currencyId}/rates`,
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function addCurrencyRate(
  currencyId: string,
  payload: CreateCurrencyRatePayload,
): Promise<ErpCurrencyRate> {
  const res = await apiPost<ApiResponse<ErpCurrencyRate>>(
    `/currencies/${currencyId}/rates`,
    payload,
  );
  return res.data;
}
