// ERP m2 Finance — Cash/Bank Transfers (header + lines) skeleton CRUD client.
// Endpoints: /fin/cashbank-transfers

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';
import type {
  ErpDocumentStatus,
  ErpPostingStatus,
  ErpJournalLine,
} from './fin-journal-entries';

export interface ErpCashbankTransfer {
  id: string;
  docNumber: string;
  branchId: string;
  locationId?: string | null;
  cashAccountId: string;
  entryDate: string;
  fiscalPeriodId: string;
  partnerId?: string | null;
  description: string;
  notes?: string | null;
  currencyId: string;
  exchangeRate: string;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  lines: ErpJournalLine[];
}

export interface CreateCashbankTransferPayload {
  docNumber: string;
  branchId: string;
  cashAccountId: string;
  entryDate: string;
  fiscalPeriodId: string;
  description: string;
  currencyId: string;
  exchangeRate: string;
  status?: ErpDocumentStatus;
  postingStatus?: ErpPostingStatus;
  notes?: string;
  partnerId?: string;
  locationId?: string;
  lines: ErpJournalLine[];
}

export type UpdateCashbankTransferPayload =
  Partial<CreateCashbankTransferPayload>;

export interface ListCashbankTransfersParams extends PaginationParams {
  status?: ErpDocumentStatus;
}

export async function listCashbankTransfers(
  params?: ListCashbankTransfersParams,
): Promise<PaginatedResponse<ErpCashbankTransfer>> {
  return apiGet<PaginatedResponse<ErpCashbankTransfer>>(
    '/fin/cashbank-transfers',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createCashbankTransfer(
  payload: CreateCashbankTransferPayload,
): Promise<ErpCashbankTransfer> {
  const res = await apiPost<ApiResponse<ErpCashbankTransfer>>(
    '/fin/cashbank-transfers',
    payload,
  );
  return res.data;
}

export async function updateCashbankTransfer(
  id: string,
  payload: UpdateCashbankTransferPayload,
): Promise<ErpCashbankTransfer> {
  const res = await apiPatch<ApiResponse<ErpCashbankTransfer>>(
    `/fin/cashbank-transfers/${id}`,
    payload,
  );
  return res.data;
}

export async function deleteCashbankTransfer(id: string): Promise<void> {
  await apiDelete<void>(`/fin/cashbank-transfers/${id}`);
}
