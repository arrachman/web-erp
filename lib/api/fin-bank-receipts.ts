// ERP m2 Finance — Bank Masuk / Bank Receipt (RM).
// Twin of Kas Masuk (fin-cash-receipts): same shared cash-bank-transactions
// resource, but kind=BANK + direction=RECEIPT, plus Cara Bayar (payment method)
// and a Giro tab. Endpoint: /fin/cash-bank-transactions.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type {
  CashBankDirection,
  CashBankLinePayload,
  CashBankTransition,
  ErpCashBankLine,
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpRef,
} from './fin-cash-receipts';

export type {
  CashBankDirection,
  CashBankLinePayload,
  CashBankTransition,
  ErpCashBankLine,
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpRef,
} from './fin-cash-receipts';

const BASE = '/fin/cash-bank-transactions';

/** Cara Bayar — mirrors DB ErpPaymentMethod. */
export type ErpPaymentMethod =
  | 'CASH'
  | 'TRANSFER'
  | 'GIRO'
  | 'CHEQUE'
  | 'CARD'
  | 'OTHER';

export type ErpCashBankKind = 'CASH' | 'BANK';

/** Giro instrument captured on the Giro tab (persisted as a linked fin_giros row). */
export interface ErpCashBankGiro {
  id?: string;
  giroNumber: string;
  bankName?: string | null;
  bankAccountNo?: string | null;
  amount: string;
  dueDate: string;
  status?: string;
  notes?: string | null;
  lineNo: number;
}

export interface CashBankGiroPayload {
  giroNumber: string;
  bankName?: string;
  bankAccountNo?: string;
  amount: string;
  dueDate: string;
  notes?: string;
  lineNo: number;
}

export interface ErpBankReceipt {
  id: string;
  docNumber: string;
  autoNumber?: string | null;
  direction: CashBankDirection;
  kind: ErpCashBankKind;
  paymentMethod?: ErpPaymentMethod | null;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  transactionDate: string;
  fiscalPeriodId: string;
  bankAccountId: string;
  bankAccount?: ErpRef | null;
  partnerId?: string | null;
  partner?: ErpRef | null;
  contactPerson?: string | null;
  description: string;
  notes?: string | null;
  currencyId: string;
  currency?: ErpRef | null;
  exchangeRate: string;
  amount: string;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
  updatedById?: string | null;
  lines: ErpCashBankLine[];
  giros?: ErpCashBankGiro[];
}

export interface CreateBankReceiptPayload {
  docNumber?: string;
  auto?: boolean;
  direction: CashBankDirection;
  kind: ErpCashBankKind;
  paymentMethod?: ErpPaymentMethod;
  branchId: string;
  locationId?: string;
  transactionDate: string;
  fiscalPeriodId?: string;
  bankAccountId: string;
  partnerId?: string;
  contactPerson?: string;
  description: string;
  notes?: string;
  currencyId: string;
  exchangeRate: string;
  legacyCode?: string;
  lines: CashBankLinePayload[];
  giros?: CashBankGiroPayload[];
}

export type UpdateBankReceiptPayload = Partial<CreateBankReceiptPayload>;

export interface ListBankReceiptsParams extends PaginationParams {
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  dateFrom?: string;
  dateTo?: string;
  branchId?: string;
  locationId?: string;
  partnerId?: string;
  docNumberFrom?: string;
  docNumberTo?: string;
  description?: string;
  notes?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listBankReceipts(
  params?: ListBankReceiptsParams,
): Promise<PaginatedResponse<ErpBankReceipt>> {
  return apiGet<PaginatedResponse<ErpBankReceipt>>(BASE, {
    ...(params as Query),
    kind: 'BANK',
    direction: 'RECEIPT',
  });
}

export async function getBankReceipt(id: string): Promise<ErpBankReceipt> {
  const res = await apiGet<ApiResponse<ErpBankReceipt>>(`${BASE}/${id}`);
  return res.data;
}

export async function createBankReceipt(
  payload: CreateBankReceiptPayload,
): Promise<ErpBankReceipt> {
  const res = await apiPost<ApiResponse<ErpBankReceipt>>(BASE, payload);
  return res.data;
}

export async function updateBankReceipt(
  id: string,
  payload: UpdateBankReceiptPayload,
): Promise<ErpBankReceipt> {
  const res = await apiPatch<ApiResponse<ErpBankReceipt>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionBankReceipt(
  id: string,
  action: CashBankTransition,
  reason?: string,
): Promise<ErpBankReceipt> {
  const res = await apiPost<ApiResponse<ErpBankReceipt>>(
    `${BASE}/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteBankReceipt(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
