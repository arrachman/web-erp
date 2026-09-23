// ERP Partner resource API — CRUD for md_partners
// Endpoints: /partners

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ErpPartnerTypeKind } from './partner-types';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

export interface ListPartnersParams extends PaginationParams {
  partnerTypeId?: string;
  typeKind?: ErpPartnerTypeKind;
  categoryId?: string;
}

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpPartnerAccountRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpMasterRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpPartnerDimBranch {
  branchId: string;
  branch?: ErpMasterRef | null;
}

export interface ErpPartnerDimWarehouse {
  warehouseId: string;
  warehouse?: ErpMasterRef | null;
}

export interface ErpPartnerDimLocation {
  locationId: string;
  location?: ErpMasterRef | null;
}

export interface ErpPartnerTermRef {
  id: string;
  code: string;
  name: string;
}

export interface ErpPartnerCurrencyRef {
  id: string;
  code: string;
  name: string;
  symbol?: string | null;
}

export interface ErpPartner {
  id: string;
  code: string;
  name: string;
  categoryId?: string | null;
  partnerTypeId: string;
  partnerType?: (ErpMasterRef & { kind: ErpPartnerTypeKind }) | null;
  customerCategoryId?: string | null;
  supplierCategoryId?: string | null;
  salesmanCategoryId?: string | null;
  salesmanId?: string | null;
  customerCategory?: ErpMasterRef | null;
  supplierCategory?: ErpMasterRef | null;
  salesmanCategory?: ErpMasterRef | null;
  salesman?: ErpMasterRef | null;
  taxNumber?: string | null;
  isTaxable: boolean;
  receivableAccountId?: string | null;
  payableAccountId?: string | null;
  receivableAccount?: ErpPartnerAccountRef | null;
  payableAccount?: ErpPartnerAccountRef | null;
  currencyId?: string | null;
  currency?: ErpPartnerCurrencyRef | null;
  saleTermId?: string | null;
  saleTerm?: ErpPartnerTermRef | null;
  purchaseTermId?: string | null;
  purchaseTerm?: ErpPartnerTermRef | null;
  arCreditLimit?: string | null;
  apCreditLimit?: string | null;
  salesPriceTier?: number | null;
  dimBranches?: ErpPartnerDimBranch[];
  dimWarehouses?: ErpPartnerDimWarehouse[];
  dimLocations?: ErpPartnerDimLocation[];
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePartnerPayload {
  code: string;
  name: string;
  categoryId?: string;
  partnerTypeId: string;
  customerCategoryId?: string | null;
  supplierCategoryId?: string | null;
  salesmanCategoryId?: string | null;
  salesmanId?: string | null;
  taxNumber?: string;
  isTaxable?: boolean;
  receivableAccountId?: string | null;
  payableAccountId?: string | null;
  currencyId?: string | null;
  saleTermId?: string | null;
  purchaseTermId?: string | null;
  arCreditLimit?: string | null;
  apCreditLimit?: string | null;
  salesPriceTier?: number | null;
  branchIds?: string[];
  warehouseIds?: string[];
  locationIds?: string[];
  isActive?: boolean;
}

export interface UpdatePartnerPayload {
  code?: string;
  name?: string;
  categoryId?: string;
  partnerTypeId?: string;
  customerCategoryId?: string | null;
  supplierCategoryId?: string | null;
  salesmanCategoryId?: string | null;
  salesmanId?: string | null;
  taxNumber?: string;
  isTaxable?: boolean;
  receivableAccountId?: string | null;
  payableAccountId?: string | null;
  currencyId?: string | null;
  saleTermId?: string | null;
  purchaseTermId?: string | null;
  arCreditLimit?: string | null;
  apCreditLimit?: string | null;
  salesPriceTier?: number | null;
  branchIds?: string[];
  warehouseIds?: string[];
  locationIds?: string[];
  isActive?: boolean;
}

// ─── Nested sub-resources (contacts / addresses) ────────────────────────────────
// Phone ("no hp") lives here — never in the partner's main fields.

export type ErpAddressType = 'BILLING' | 'SHIPPING' | 'OFFICE' | 'OTHER';

export interface ErpPartnerContact {
  id: string;
  name: string;
  title?: string | null;
  phone?: string | null;
  email?: string | null;
  isDefault: boolean;
}

export interface ErpPartnerAddress {
  id: string;
  type: ErpAddressType;
  addressLine1: string;
  addressLine2?: string | null;
  countryId?: string | null;
  provinceId?: string | null;
  cityId?: string | null;
  areaId?: string | null;
  subAreaId?: string | null;
  postalCode?: string | null;
  phone?: string | null;
  fax?: string | null;
  email?: string | null;
  website?: string | null;
  isDefault: boolean;
  country?: { id: string; name: string } | null;
  province?: { id: string; name: string } | null;
  city?: { id: string; name: string } | null;
  area?: { id: string; name: string; postalCode?: string | null } | null;
  subArea?: { id: string; name: string; postalCode?: string | null } | null;
}

export interface ErpPartnerDetail extends ErpPartner {
  contacts: ErpPartnerContact[];
  addresses: ErpPartnerAddress[];
}

export interface CreatePartnerContactPayload {
  name: string;
  title?: string;
  phone?: string;
  email?: string;
  isDefault?: boolean;
}

export type UpdatePartnerContactPayload = Partial<CreatePartnerContactPayload>;

export interface CreatePartnerAddressPayload {
  type: ErpAddressType;
  addressLine1: string;
  addressLine2?: string;
  countryId?: string;
  provinceId?: string;
  cityId?: string;
  areaId?: string;
  subAreaId?: string;
  postalCode?: string;
  phone?: string;
  fax?: string;
  email?: string;
  website?: string;
  isDefault?: boolean;
}

export type UpdatePartnerAddressPayload = Partial<CreatePartnerAddressPayload>;

// ─── API functions ────────────────────────────────────────────────────────────

export async function listPartners(
  params?: ListPartnersParams,
): Promise<PaginatedResponse<ErpPartner>> {
  return apiGet<PaginatedResponse<ErpPartner>>('/partners', params as Record<string, string | number | boolean | undefined>);
}

export async function createPartner(
  payload: CreatePartnerPayload,
): Promise<ErpPartner> {
  const res = await apiPost<ApiResponse<ErpPartner>>('/partners', payload);
  return res.data;
}

export async function updatePartner(
  id: string,
  payload: UpdatePartnerPayload,
): Promise<ErpPartner> {
  const res = await apiPatch<ApiResponse<ErpPartner>>(`/partners/${id}`, payload);
  return res.data;
}

export async function deletePartner(id: string): Promise<void> {
  await apiDelete<void>(`/partners/${id}`);
}

export async function bulkUpdatePartnerStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/partners/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeletePartners(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/partners/bulk', { ids });
  return { affected: res.affected };
}

// ─── Sub-resources: detail + contacts + addresses ───────────────────────────────

export async function getPartner(id: string): Promise<ErpPartnerDetail> {
  const res = await apiGet<ApiResponse<ErpPartnerDetail>>(`/partners/${id}`);
  return res.data;
}

export async function addPartnerContact(
  partnerId: string,
  payload: CreatePartnerContactPayload,
): Promise<ErpPartnerContact> {
  const res = await apiPost<ApiResponse<ErpPartnerContact>>(`/partners/${partnerId}/contacts`, payload);
  return res.data;
}

export async function updatePartnerContact(
  partnerId: string,
  contactId: string,
  payload: UpdatePartnerContactPayload,
): Promise<ErpPartnerContact> {
  const res = await apiPatch<ApiResponse<ErpPartnerContact>>(`/partners/${partnerId}/contacts/${contactId}`, payload);
  return res.data;
}

export async function removePartnerContact(partnerId: string, contactId: string): Promise<void> {
  await apiDelete<void>(`/partners/${partnerId}/contacts/${contactId}`);
}

export async function addPartnerAddress(
  partnerId: string,
  payload: CreatePartnerAddressPayload,
): Promise<ErpPartnerAddress> {
  const res = await apiPost<ApiResponse<ErpPartnerAddress>>(`/partners/${partnerId}/addresses`, payload);
  return res.data;
}

export async function updatePartnerAddress(
  partnerId: string,
  addressId: string,
  payload: UpdatePartnerAddressPayload,
): Promise<ErpPartnerAddress> {
  const res = await apiPatch<ApiResponse<ErpPartnerAddress>>(`/partners/${partnerId}/addresses/${addressId}`, payload);
  return res.data;
}

export async function removePartnerAddress(partnerId: string, addressId: string): Promise<void> {
  await apiDelete<void>(`/partners/${partnerId}/addresses/${addressId}`);
}
