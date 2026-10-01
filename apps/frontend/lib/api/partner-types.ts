// ERP Partner Type resource API — CRUD for md_partner_types
// Endpoints: /partner-types

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpPartnerTypeKind =
  | 'CUSTOMER'
  | 'SUPPLIER'
  | 'SALESMAN'
  | 'GENERAL';

export const PARTNER_TYPE_KINDS: ErpPartnerTypeKind[] = [
  'CUSTOMER',
  'SUPPLIER',
  'SALESMAN',
  'GENERAL',
];

export const PARTNER_TYPE_KIND_LABEL: Record<ErpPartnerTypeKind, string> = {
  CUSTOMER: 'Customer',
  SUPPLIER: 'Supplier',
  SALESMAN: 'Salesman',
  GENERAL: 'General',
};

/** System kind is derived from protected codes — not user-editable. */
const CODE_TO_KIND: Record<string, ErpPartnerTypeKind> = {
  CUST: 'CUSTOMER',
  SUP: 'SUPPLIER',
  SLS: 'SALESMAN',
};

/** Map partner-type `code` → system `kind`. Unknown codes → GENERAL. */
export function derivePartnerTypeKindFromCode(code: string): ErpPartnerTypeKind {
  const key = code.trim().toUpperCase();
  return CODE_TO_KIND[key] ?? 'GENERAL';
}

export interface ErpPartnerType {
  id: string;
  code: string;
  name: string;
  kind: ErpPartnerTypeKind;
  isActive: boolean;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePartnerTypePayload {
  code: string;
  name: string;
  kind: ErpPartnerTypeKind;
  isActive?: boolean;
}

export interface UpdatePartnerTypePayload {
  code?: string;
  name?: string;
  kind?: ErpPartnerTypeKind;
  isActive?: boolean;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listPartnerTypes(
  params?: PaginationParams & { kind?: ErpPartnerTypeKind; isActive?: boolean },
): Promise<PaginatedResponse<ErpPartnerType>> {
  return apiGet<PaginatedResponse<ErpPartnerType>>(
    '/partner-types',
    params as Record<string, string | number | boolean | undefined>,
  );
}

export async function createPartnerType(
  payload: CreatePartnerTypePayload,
): Promise<ErpPartnerType> {
  const res = await apiPost<ApiResponse<ErpPartnerType>>(
    '/partner-types',
    payload,
  );
  return res.data;
}

export async function updatePartnerType(
  id: string,
  payload: UpdatePartnerTypePayload,
): Promise<ErpPartnerType> {
  const res = await apiPatch<ApiResponse<ErpPartnerType>>(
    `/partner-types/${id}`,
    payload,
  );
  return res.data;
}

export async function deletePartnerType(id: string): Promise<void> {
  await apiDelete<void>(`/partner-types/${id}`);
}

export async function bulkUpdateErpPartnerTypeStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/partner-types/bulk/status',
    { ids, isActive },
  );
  return { affected: res.affected };
}

export async function bulkDeleteErpPartnerTypes(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/partner-types/bulk',
    { ids },
  );
  return { affected: res.affected };
}