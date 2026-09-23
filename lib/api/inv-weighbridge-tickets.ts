// ERP m3 Inventory — Receipt Weigher / Weighbridge Tickets (RW). Header-only
// document (no item lines). Full workflow (SUBMIT/APPROVE/REJECT/POST/REOPEN).
// netWeight = grossWeight - tareWeight computed server-side.
// Endpoint: /inv/weighbridge-tickets

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

const BASE = '/inv/weighbridge-tickets';

export type ErpDocumentStatus =
  | 'DRAFT'
  | 'NEED_APPROVE'
  | 'APPROVED'
  | 'REJECTED'
  | 'POSTED'
  | 'VOID'
  | 'CANCELLED';
export type ErpPostingStatus = 'UNPOSTED' | 'POSTED';

/** Resolved cross-domain reference (item/unit/warehouse/…): code + name. */
export interface ErpRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpInvWeighbridgeTicket {
  id: string;
  docNumber: string;
  ticketDate: string;
  fiscalPeriodId?: string | null;
  branchId: string;
  branch?: ErpRef | null;
  locationId?: string | null;
  location?: ErpRef | null;
  vehiclePlate?: string | null;
  driverName?: string | null;
  partnerId?: string | null;
  partner?: ErpRef | null;
  itemId?: string | null;
  item?: ErpRef | null;
  grossWeight: string;
  tareWeight: string;
  netWeight: string;
  unitPrice?: string | null;
  description?: string | null;
  notes?: string | null;
  status: ErpDocumentStatus;
  postingStatus: ErpPostingStatus;
  postedAt?: string | null;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
  createdById?: string | null;
}

export interface CreateInvWeighbridgeTicketPayload {
  docNumber?: string;
  ticketDate: string;
  fiscalPeriodId?: string;
  branchId: string;
  locationId?: string;
  vehiclePlate?: string;
  driverName?: string;
  partnerId?: string;
  itemId?: string;
  grossWeight: string;
  tareWeight: string;
  unitPrice?: string;
  description?: string;
  notes?: string;
  status?: ErpDocumentStatus;
  legacyCode?: string;
}

export type UpdateInvWeighbridgeTicketPayload = Partial<CreateInvWeighbridgeTicketPayload>;

export type InvWeighbridgeTicketTransition =
  | 'SUBMIT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

export interface ListInvWeighbridgeTicketsParams extends PaginationParams {
  sortBy?: 'ticketDate' | 'docNumber' | 'status' | 'createdAt';
  sortDir?: 'asc' | 'desc';
  status?: ErpDocumentStatus;
  branchId?: string;
  partnerId?: string;
  dateFrom?: string;
  dateTo?: string;
  search?: string;
  createdById?: string;
}

type Query = Record<string, string | number | boolean | undefined>;

export function listInvWeighbridgeTickets(
  params?: ListInvWeighbridgeTicketsParams,
): Promise<PaginatedResponse<ErpInvWeighbridgeTicket>> {
  return apiGet<PaginatedResponse<ErpInvWeighbridgeTicket>>(BASE, params as Query);
}

export async function getInvWeighbridgeTicket(id: string): Promise<ErpInvWeighbridgeTicket> {
  const res = await apiGet<ApiResponse<ErpInvWeighbridgeTicket>>(`${BASE}/${id}`);
  return res.data;
}

export async function createInvWeighbridgeTicket(
  payload: CreateInvWeighbridgeTicketPayload,
): Promise<ErpInvWeighbridgeTicket> {
  const res = await apiPost<ApiResponse<ErpInvWeighbridgeTicket>>(BASE, payload);
  return res.data;
}

export async function updateInvWeighbridgeTicket(
  id: string,
  payload: UpdateInvWeighbridgeTicketPayload,
): Promise<ErpInvWeighbridgeTicket> {
  const res = await apiPatch<ApiResponse<ErpInvWeighbridgeTicket>>(`${BASE}/${id}`, payload);
  return res.data;
}

export async function transitionInvWeighbridgeTicket(
  id: string,
  action: InvWeighbridgeTicketTransition,
  reason?: string,
): Promise<ErpInvWeighbridgeTicket> {
  const res = await apiPost<ApiResponse<ErpInvWeighbridgeTicket>>(
    `${BASE}/${id}/transition`,
    { action, reason },
  );
  return res.data;
}

export async function deleteInvWeighbridgeTicket(id: string): Promise<void> {
  await apiDelete<void>(`${BASE}/${id}`);
}
