// ERP m2 Finance — Bank Keluar / Bank Disbursement (SM).
// Backed by the SHARED cash-bank-transactions resource with
// direction=DISBURSEMENT + kind=BANK (same endpoint/types as Kas Masuk/Keluar;
// see lib/api/fin-cash-receipts.ts + the erp-fin-cash-bank-transactions module).
// Only `direction` + `kind` differ from the cash variants; giros + paymentMethod
// (Cara Bayar) ride on the shared payload.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';
import type {
  CashBankTransition,
  CreateCashReceiptPayload,
  ErpCashReceipt,
  ListCashReceiptsParams,
} from './fin-cash-receipts';

// Re-export the shared cash/bank transaction types under bank-flavoured names.
export type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpPaymentMethod,
  ErpCashBankKind,
  CashBankDirection,
  ErpRef,
  ErpCashBankLine,
  ErpCashBankGiro,
  CashBankLinePayload,
  CashBankGiroPayload,
  CashBankTransition,
} from './fin-cash-receipts';

export type ErpBankDisbursement = ErpCashReceipt;
export type CreateBankDisbursementPayload = CreateCashReceiptPayload;
export type UpdateBankDisbursementPayload = Partial<CreateBankDisbursementPayload>;
export type ListBankDisbursementsParams = ListCashReceiptsParams;

const BASE = '/fin/cash-bank-transactions';
type Query = Record<string, string | number | boolean | undefined>;

export function listBankDisbursements(
  params?: ListBankDisbursementsParams,
): Promise<PaginatedResponse<ErpBankDisbursement>> {
  return apiGet<PaginatedResponse<ErpBankDisbursement>>(BASE, {
    ...(params as Query),
    direction: 'DISBURSEMENT',
    kind: 'BANK',
  });
}

export async function getBankDisbursement(id: string): Promise<ErpBankDisbursement> {
  const res = await apiGet<ApiResponse<ErpBankDisbursement>>(`${BASE}/${id}`);
  return res.data;
}

export async function createBankDisbursement(
  payload: CreateBankDisbursementPayload,
): Promise<ErpBankDisbursement> {
  const res = await apiPost<ApiResponse<ErpBankDisbursement>>(BASE, payload);
  return res.data;
}

export async function updateBankDisbursement(
  id: string,
  payload: UpdateBankDisbursementPayload,
): Promise<ErpBankDisbursement> {
  const res = await apiPatch<ApiResponse<ErpBankDisbursement>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionBankDisbursement(
  id: string,
  action: CashBankTransition,
  reason?: string,
): Promise<ErpBankDisbursement> {
  const res = await apiPost<ApiResponse<ErpBankDisbursement>>(
    `${BASE}/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteBankDisbursement(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
