// Fase 2 P2 — Job Cetak API (profil cetak di atas Work Order).
// Endpoints: /mfg/print-jobs (controller erp/mfg/print-jobs)

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export type PrintJobStage =
  | 'PRE_PRESS'
  | 'CETAK'
  | 'FINISHING'
  | 'QC'
  | 'SELESAI'
  | 'CANCELLED';

export const PRINT_JOB_STAGE_LABELS: Record<PrintJobStage, string> = {
  PRE_PRESS: 'Pre-press',
  CETAK: 'Cetak',
  FINISHING: 'Finishing',
  QC: 'QC',
  SELESAI: 'Selesai',
  CANCELLED: 'Batal',
};

export const PRINT_JOB_STAGE_FLOW: PrintJobStage[] = [
  'PRE_PRESS',
  'CETAK',
  'FINISHING',
  'QC',
  'SELESAI',
];

export interface PrintJobChecklistItem {
  key: string;
  label: string;
  required: boolean;
  done: boolean;
  doneById?: string | null;
  doneAt?: string | null;
}

export interface PrintJob {
  id: string;
  workOrderId: string;
  workOrderDocNumber?: string | null;
  workOrderStatus?: string | null;
  workOrderDocDate?: string | null;
  title?: string | null;
  itemName?: string | null;
  outputItemId?: string | null;
  estimateId?: string | null;
  estimateDocNumber?: string | null;
  stage: PrintJobStage;
  paperSize?: string | null;
  pageCount?: number | null;
  printQuantity: string;
  colorSpec?: string | null;
  finishing?: string | null;
  masterFileName?: string | null;
  checklist: PrintJobChecklistItem[];
  checklistProgress: { done: number; total: number };
  stageLog: { from: string | null; to: string; at: string; byId?: string | null }[];
  notes?: string | null;
}

export interface PrintJobPayload {
  branchId?: string;
  docDate: string;
  estimateId?: string;
  itemId?: string;
  printQuantity?: string;
  title?: string;
  paperSize?: string;
  pageCount?: number;
  colorSpec?: string;
  finishing?: string;
  masterFileName?: string;
  notes?: string;
}

export async function listPrintJobs(params: {
  page?: number;
  limit?: number;
  search?: string;
  stage?: string;
}): Promise<PaginatedResponse<PrintJob>> {
  const qs = new URLSearchParams();
  if (params.page) qs.set('page', String(params.page));
  if (params.limit) qs.set('limit', String(params.limit));
  if (params.search) qs.set('search', params.search);
  if (params.stage) qs.set('stage', params.stage);
  return apiGet<PaginatedResponse<PrintJob>>(`/mfg/print-jobs?${qs.toString()}`);
}

export async function getPrintJob(id: string): Promise<PrintJob> {
  const res = await apiGet<ApiResponse<PrintJob>>(`/mfg/print-jobs/${id}`);
  return (res as any).data ?? res;
}

export async function createPrintJob(payload: PrintJobPayload): Promise<PrintJob> {
  const res = await apiPost<ApiResponse<PrintJob>>(`/mfg/print-jobs`, payload);
  return (res as any).data ?? res;
}

export async function updatePrintJob(
  id: string,
  payload: Partial<PrintJobPayload>,
): Promise<PrintJob> {
  const res = await apiPatch<ApiResponse<PrintJob>>(`/mfg/print-jobs/${id}`, payload);
  return (res as any).data ?? res;
}

export async function deletePrintJob(id: string): Promise<void> {
  await apiDelete(`/mfg/print-jobs/${id}`);
}

export async function setPrintJobChecklist(
  id: string,
  key: string,
  done: boolean,
): Promise<PrintJob> {
  const res = await apiPost<ApiResponse<PrintJob>>(`/mfg/print-jobs/${id}/checklist`, {
    key,
    done,
  });
  return (res as any).data ?? res;
}

export async function advancePrintJob(id: string, toStage: PrintJobStage): Promise<PrintJob> {
  const res = await apiPost<ApiResponse<PrintJob>>(`/mfg/print-jobs/${id}/advance`, {
    toStage,
  });
  return (res as any).data ?? res;
}
