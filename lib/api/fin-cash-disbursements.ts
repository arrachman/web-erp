// ERP m2 Finance — Kas Keluar / Cash Disbursement (CD).
// Backed by the SHARED cash-bank-transactions resource with direction=DISBURSEMENT
// (same endpoint/types as Kas Masuk; see lib/api/fin-cash-receipts.ts + the
// erp-fin-cash-bank-transactions backend module). Only the `direction` differs.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';
import type {
  CashBankTransition,
  CreateCashReceiptPayload,
  ErpCashReceipt,
  ListCashReceiptsParams,
} from './fin-cash-receipts';

// Re-export the shared cash/bank transaction types under CD-flavoured names so
// the CD page/form read naturally without duplicating ~130 lines of interfaces.
export type {
  ErpDocumentStatus,
  ErpPostingStatus,
  CashBankDirection,
  ErpRef,
  ErpCashBankLine,
  CashBankLinePayload,
  CashBankTransition,
} from './fin-cash-receipts';

export type ErpCashDisbursement = ErpCashReceipt;
export type CreateCashDisbursementPayload = CreateCashReceiptPayload;
export type UpdateCashDisbursementPayload = Partial<CreateCashDisbursementPayload>;
export type ListCashDisbursementsParams = ListCashReceiptsParams;

const BASE = '/fin/cash-bank-transactions';
type Query = Record<string, string | number | boolean | undefined>;

export function listCashDisbursements(
  params?: ListCashDisbursementsParams,
): Promise<PaginatedResponse<ErpCashDisbursement>> {
  return apiGet<PaginatedResponse<ErpCashDisbursement>>(BASE, {
    ...(params as Query),
    direction: 'DISBURSEMENT',
  });
}

export async function getCashDisbursement(id: string): Promise<ErpCashDisbursement> {
  const res = await apiGet<ApiResponse<ErpCashDisbursement>>(`${BASE}/${id}`);
  return res.data;
}

export async function createCashDisbursement(
  payload: CreateCashDisbursementPayload,
): Promise<ErpCashDisbursement> {
  const res = await apiPost<ApiResponse<ErpCashDisbursement>>(BASE, payload);
  return res.data;
}

export async function updateCashDisbursement(
  id: string,
  payload: UpdateCashDisbursementPayload,
): Promise<ErpCashDisbursement> {
  const res = await apiPatch<ApiResponse<ErpCashDisbursement>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionCashDisbursement(
  id: string,
  action: CashBankTransition,
  reason?: string,
): Promise<ErpCashDisbursement> {
  const res = await apiPost<ApiResponse<ErpCashDisbursement>>(
    `${BASE}/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteCashDisbursement(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
