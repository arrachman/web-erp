// ERP m2 Finance — General Ledger (read-only) client

import { apiGet } from './client';
import type { PaginatedResponse, PaginationParams } from './types';

export interface ErpLedgerEntry {
  id: string;
  branchId: string;
  source: string;
  sourceDocType?: string | null;
  sourceId?: string | null;
  docNumber: string;
  entryDate: string;
  fiscalPeriodId: string;
  partnerId?: string | null;
  accountId: string;
  description?: string | null;
  currencyId: string;
  exchangeRate: string;
  referenceNo?: string | null;
  debit: string;
  credit: string;
  status: string;
  postingStatus: string;
  reconciliationStatus: string;
  createdAt: string;
  updatedAt: string;
}

export interface ListLedgerParams extends PaginationParams {
  accountId?: string;
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
  includeTotal?: boolean;
}

export async function listLedgerEntries(
  params?: ListLedgerParams,
): Promise<PaginatedResponse<ErpLedgerEntry>> {
  return apiGet<PaginatedResponse<ErpLedgerEntry>>(
    '/fin/ledger',
    params as Record<string, string | number | boolean | undefined>,
  );
}
