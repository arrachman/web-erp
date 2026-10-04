// ERP School CRM resource API — A1: profil sekolah berbasis NPSN di atas
// md_partners (partner_type SCHOOL) + md_school_profiles.
// Endpoints: /schools (controller erp/schools, global prefix api + base /erp)

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type ErpSchoolJenjang = 'PAUD' | 'TK' | 'SD' | 'SMP' | 'SMA' | 'SMK' | 'SLB' | 'OTHER';
export type ErpSchoolStatus = 'NEGERI' | 'SWASTA';
export type ErpBosPeriodStage = 'TAHAP_1' | 'TAHAP_2';
export type ErpSchoolPipelineStage = 'PROSPEK' | 'PENAWARAN' | 'PESANAN' | 'TERKIRIM' | 'LUNAS';
export type ErpPartnerContactRole = 'KEPALA_SEKOLAH' | 'BENDAHARA' | 'OPERATOR' | 'TU' | 'OTHER';
export type ErpSchoolActivityType = 'VISIT' | 'NEGOTIATION' | 'NOTE';

export interface ErpSchoolProfile {
  id: string;
  npsn?: string | null;
  jenjang?: ErpSchoolJenjang | null;
  negeriSwasta?: ErpSchoolStatus | null;
  accreditation?: string | null;
  studentCount?: number | null;
  classCount?: number | null;
  studentsPerGrade?: Record<string, number> | null;
  pipelineStage?: ErpSchoolPipelineStage | null;
  /** Decimal serialised as string by the API. */
  bosPagu?: string | null;
  bosRealisasi?: string | null;
  bosPeriodLabel?: string | null;
  bosPeriodStage?: ErpBosPeriodStage | null;
  bosPeriodYear?: number | null;
  lastVisitAt?: string | null;
  visitNotes?: string | null;
  negotiationNotes?: string | null;
  contractExpiryAt?: string | null;
  isActive: boolean;
}

export interface ErpSchoolContact {
  id: string;
  name: string;
  role?: ErpPartnerContactRole | null;
  title?: string | null;
  phone?: string | null;
  email?: string | null;
  isDefault: boolean;
}

export interface ErpSchoolActivity {
  id: string;
  type: ErpSchoolActivityType;
  activityAt: string;
  notes: string;
  contactId?: string | null;
  contact?: { id: string; name: string } | null;
}

export interface ErpSchoolAddress {
  id: string;
  addressLine1?: string | null;
  city?: { id: string; name: string } | null;
  province?: { id: string; name: string } | null;
}

export interface ErpSchoolOrderSummary {
  quotationCount: number;
  orderCount: number;
  deliveryCount: number;
  invoiceCount: number;
  paidInvoiceCount: number;
  lastOrderAt: string | null;
  lastInvoiceAt: string | null;
  derivedPipelineStage: ErpSchoolPipelineStage;
}

export interface ErpSchool {
  id: string;
  code: string;
  name: string;
  taxNumber?: string | null;
  isTaxable?: boolean;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
  schoolProfile?: ErpSchoolProfile | null;
  contacts?: ErpSchoolContact[];
  schoolActivities?: ErpSchoolActivity[];
  addresses?: ErpSchoolAddress[];
  orderSummary?: ErpSchoolOrderSummary;
}

export interface SchoolContactPayload {
  id?: string;
  name: string;
  role?: ErpPartnerContactRole;
  title?: string;
  phone?: string;
  email?: string;
  isDefault?: boolean;
}

export interface SchoolActivityPayload {
  type: ErpSchoolActivityType;
  activityAt?: string;
  notes: string;
}

export interface CreateSchoolPayload {
  code: string;
  name: string;
  taxNumber?: string;
  isActive?: boolean;
  npsn?: string;
  jenjang?: ErpSchoolJenjang;
  negeriSwasta?: ErpSchoolStatus;
  accreditation?: string;
  studentCount?: number;
  classCount?: number;
  studentsPerGrade?: Record<string, number>;
  bosPagu?: number;
  bosRealisasi?: number;
  bosPeriodLabel?: string;
  bosPeriodStage?: ErpBosPeriodStage;
  bosPeriodYear?: number;
  pipelineStage?: ErpSchoolPipelineStage;
  lastVisitAt?: string;
  visitNotes?: string;
  negotiationNotes?: string;
  contractExpiryAt?: string;
  contacts?: SchoolContactPayload[];
  newActivity?: SchoolActivityPayload;
}

export type UpdateSchoolPayload = Partial<CreateSchoolPayload>;

export interface ErpSchoolAlerts {
  bosYear: number;
  notOrdered: { id: string; code: string; name: string; npsn: string | null; jenjang: string | null }[];
  contractExpiring: {
    id: string;
    code: string;
    name: string;
    npsn: string | null;
    contractExpiryAt: string | null;
    daysRemaining: number;
  }[];
}

export interface ErpSchoolOrderHistoryRow {
  type: 'QUOTATION' | 'ORDER' | 'DELIVERY' | 'INVOICE' | 'RECEIPT';
  id: string;
  docNumber: string;
  docDate: string;
  status: string;
  grandTotal: string;
  channel?: string | null;
  settlementStatus?: string | null;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listSchools(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpSchool>> {
  return apiGet<PaginatedResponse<ErpSchool>>('/schools', params as Record<string, string | number | boolean | undefined>);
}

export async function getSchool(id: string): Promise<ErpSchool> {
  const res = await apiGet<ApiResponse<ErpSchool>>(`/schools/${id}`);
  return res.data;
}

export async function createSchool(payload: CreateSchoolPayload): Promise<ErpSchool> {
  const res = await apiPost<ApiResponse<ErpSchool>>('/schools', payload);
  return res.data;
}

export async function updateSchool(id: string, payload: UpdateSchoolPayload): Promise<ErpSchool> {
  const res = await apiPatch<ApiResponse<ErpSchool>>(`/schools/${id}`, payload);
  return res.data;
}

export async function deleteSchool(id: string): Promise<void> {
  await apiDelete<void>(`/schools/${id}`);
}

export async function bulkUpdateSchoolStatus(ids: string[], isActive: boolean): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>('/schools/bulk/status', { ids, isActive });
  return { affected: res.affected };
}

export async function bulkDeleteSchools(ids: string[]): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>('/schools/bulk', { ids });
  return { affected: res.affected };
}

export async function getSchoolAlerts(bosYear?: number): Promise<ErpSchoolAlerts> {
  const res = await apiGet<ApiResponse<ErpSchoolAlerts>>('/schools/alerts', bosYear ? { bosYear } : undefined);
  return res.data;
}

export async function getSchoolOrderHistory(id: string): Promise<ErpSchoolOrderHistoryRow[]> {
  const res = await apiGet<ApiResponse<ErpSchoolOrderHistoryRow[]>>(`/schools/${id}/order-history`);
  return res.data;
}

export async function listSchoolActivities(
  id: string,
  params?: { page?: number; limit?: number },
): Promise<PaginatedResponse<ErpSchoolActivity>> {
  return apiGet<PaginatedResponse<ErpSchoolActivity>>(`/schools/${id}/activities`, params);
}

export async function createSchoolActivity(
  id: string,
  payload: SchoolActivityPayload & { contactId?: string },
): Promise<ErpSchoolActivity> {
  const res = await apiPost<ApiResponse<ErpSchoolActivity>>(`/schools/${id}/activities`, payload);
  return res.data;
}

export async function deleteSchoolActivity(id: string, activityId: string): Promise<void> {
  await apiDelete<void>(`/schools/${id}/activities/${activityId}`);
}

export async function addSchoolContact(id: string, payload: SchoolContactPayload): Promise<ErpSchoolContact> {
  const res = await apiPost<ApiResponse<ErpSchoolContact>>(`/schools/${id}/contacts`, payload);
  return res.data;
}

export async function updateSchoolContact(
  id: string,
  contactId: string,
  payload: SchoolContactPayload,
): Promise<ErpSchoolContact> {
  const res = await apiPatch<ApiResponse<ErpSchoolContact>>(`/schools/${id}/contacts/${contactId}`, payload);
  return res.data;
}

export async function deleteSchoolContact(id: string, contactId: string): Promise<void> {
  await apiDelete<void>(`/schools/${id}/contacts/${contactId}`);
}
