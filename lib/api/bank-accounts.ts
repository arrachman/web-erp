// ERP Bank Account resource API — CRUD for sys_bank_accounts
// Endpoints: /bank-accounts

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpBankAccount {
  id: string;
  code: string;
  name: string;
  bankName: string;
  accountNumber: string;
  accountHolder: string;
  branch?: string | null;
  currencyId?: string | null;
  glAccountId?: string | null;
  swiftCode?: string | null;
  isPrimary: boolean;
  notes?: string | null;
  isActive: boolean;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBankAccountPayload {
  code: string;
  name: string;
  bankName: string;
  accountNumber: string;
  accountHolder: string;
  branch?: string;
  currencyId?: string;
  glAccountId?: string;
  swiftCode?: string;
  isPrimary?: boolean;
  notes?: string;
  isActive?: boolean;
}

export interface UpdateBankAccountPayload {
  code?: string;
  name?: string;
  bankName?: string;
  accountNumber?: string;
  accountHolder?: string;
  branch?: string;
  currencyId?: string;
  glAccountId?: string;
  swiftCode?: string;
  isPrimary?: boolean;
  notes?: string;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listBankAccounts(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpBankAccount>> {
  return apiGet<PaginatedResponse<ErpBankAccount>>(
    '/bank-accounts',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createBankAccount(
  payload: CreateBankAccountPayload,
): Promise<ErpBankAccount> {
  const res = await apiPost<ApiResponse<ErpBankAccount>>('/bank-accounts', payload);
  return res.data;
}

export async function updateBankAccount(
  id: string,
  payload: UpdateBankAccountPayload,
): Promise<ErpBankAccount> {
  const res = await apiPatch<ApiResponse<ErpBankAccount>>(
    `/bank-accounts/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteBankAccount(id: string): Promise<void> {
  await apiDelete<void>(`/bank-accounts/${id}`);
}

export async function bulkUpdateErpBankAccountStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/bank-accounts/bulk/status',
    { ids, isActive },
  );
  return { affected: res.affected };
}

export async function bulkDeleteErpBankAccounts(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/bank-accounts/bulk',
    { ids },
  );
  return { affected: res.affected };
}
