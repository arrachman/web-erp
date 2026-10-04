// Fase 2 P1 — Estimasi Cetak API.
// Endpoints: /mfg/print-estimates (controller erp/mfg/print-estimates)

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export type PrintEstimateStatus = 'DRAFT' | 'ISSUED' | 'CONVERTED' | 'CANCELLED';

export const PRINT_COMPONENTS = [
  'KERTAS',
  'TINTA',
  'PLAT',
  'PRE_PRESS',
  'CETAK',
  'FINISHING',
  'MAKLOON',
  'OVERHEAD',
  'LAIN',
] as const;
export type PrintComponent = (typeof PRINT_COMPONENTS)[number];

export const PRINT_COMPONENT_LABELS: Record<PrintComponent, string> = {
  KERTAS: 'Kertas',
  TINTA: 'Tinta',
  PLAT: 'Plat',
  PRE_PRESS: 'Pre-press',
  CETAK: 'Cetak',
  FINISHING: 'Finishing',
  MAKLOON: 'Makloon',
  OVERHEAD: 'Overhead',
  LAIN: 'Lainnya',
};

export interface PrintEstimateLine {
  id?: string;
  lineNo: number;
  component: PrintComponent;
  description?: string | null;
  itemId?: string | null;
  quantity: string;
  unitId?: string | null;
  unitCost: string;
  amount?: string;
}

export interface PrintEstimate {
  id: string;
  docNumber: string;
  branchId: string;
  docDate: string;
  customerId?: string | null;
  customerName?: string | null;
  itemId?: string | null;
  itemName?: string | null;
  title: string;
  paperSize?: string | null;
  pageCount?: number | null;
  printQuantity: string;
  colorSpec?: string | null;
  finishing?: string | null;
  marginPercent: string;
  totalCost: string;
  totalPrice: string;
  unitPrice: string;
  status: PrintEstimateStatus;
  quotationId?: string | null;
  quotationDocNumber?: string | null;
  notes?: string | null;
  lines?: PrintEstimateLine[];
}

export interface PrintEstimatePayload {
  branchId?: string;
  docDate: string;
  customerId?: string;
  itemId?: string;
  title: string;
  paperSize?: string;
  pageCount?: number;
  printQuantity: string;
  colorSpec?: string;
  finishing?: string;
  marginPercent?: string;
  notes?: string;
  lines: PrintEstimateLine[];
}

export async function listPrintEstimates(params: {
  page?: number;
  limit?: number;
  search?: string;
  status?: string;
}): Promise<PaginatedResponse<PrintEstimate>> {
  const qs = new URLSearchParams();
  if (params.page) qs.set('page', String(params.page));
  if (params.limit) qs.set('limit', String(params.limit));
  if (params.search) qs.set('search', params.search);
  if (params.status) qs.set('status', params.status);
  const res = await apiGet<PaginatedResponse<PrintEstimate>>(
    `/mfg/print-estimates?${qs.toString()}`,
  );
  return res as unknown as PaginatedResponse<PrintEstimate>;
}

export async function getPrintEstimate(id: string): Promise<PrintEstimate> {
  const res = await apiGet<ApiResponse<PrintEstimate>>(`/mfg/print-estimates/${id}`);
  return (res as any).data ?? res;
}

export async function createPrintEstimate(
  payload: PrintEstimatePayload,
): Promise<PrintEstimate> {
  const res = await apiPost<ApiResponse<PrintEstimate>>(`/mfg/print-estimates`, payload);
  return (res as any).data ?? res;
}

export async function updatePrintEstimate(
  id: string,
  payload: Partial<PrintEstimatePayload>,
): Promise<PrintEstimate> {
  const res = await apiPatch<ApiResponse<PrintEstimate>>(
    `/mfg/print-estimates/${id}`,
    payload,
  );
  return (res as any).data ?? res;
}

export async function deletePrintEstimate(id: string): Promise<void> {
  await apiDelete(`/mfg/print-estimates/${id}`);
}

export async function setPrintEstimateStatus(
  id: string,
  status: 'ISSUED' | 'CANCELLED' | 'DRAFT',
): Promise<PrintEstimate> {
  const res = await apiPost<ApiResponse<PrintEstimate>>(
    `/mfg/print-estimates/${id}/status`,
    { status },
  );
  return (res as any).data ?? res;
}

export async function convertPrintEstimate(id: string): Promise<PrintEstimate> {
  const res = await apiPost<ApiResponse<PrintEstimate>>(
    `/mfg/print-estimates/${id}/convert-to-quotation`,
    {},
  );
  return (res as any).data ?? res;
}
