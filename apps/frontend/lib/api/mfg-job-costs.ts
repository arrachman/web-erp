// Fase 2 P5 — HPP per Job API (biaya aktual, ringkasan, jurnal).
// Endpoints: /mfg/print-jobs/:jobId/costs, /mfg/job-costs/...

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export const JOB_COST_TYPES = [
  'MATERIAL',
  'TENAGA_KERJA',
  'OVERHEAD',
  'MAKLOON',
  'LAIN',
] as const;
export type JobCostType = (typeof JOB_COST_TYPES)[number];

export const JOB_COST_TYPE_LABELS: Record<JobCostType, string> = {
  MATERIAL: 'Material',
  TENAGA_KERJA: 'Tenaga Kerja',
  OVERHEAD: 'Overhead',
  MAKLOON: 'Makloon',
  LAIN: 'Lainnya',
};

export interface JobCostEntry {
  id: string;
  jobId: string;
  entryDate: string;
  costType: JobCostType;
  stage?: string | null;
  description?: string | null;
  itemId?: string | null;
  quantity: string;
  unitCost: string;
  amount: string;
  sourceType: string;
}

export interface JobCostSummary {
  jobId: string;
  stage: string;
  printQuantity: string;
  entryCount: number;
  estimateDocNumber?: string | null;
  estimateTotalCost?: string | null;
  estimateTotalPrice?: string | null;
  actualTotal: string;
  actualUnitCost: string;
  varianceVsEstimate?: string | null;
  variancePercent?: string | null;
  marginVsEstimatePrice?: string | null;
  byType: Record<string, string>;
  byStage: Record<string, string>;
  costPostedAt?: string | null;
  costJournalDoc?: string | null;
}

export interface JobCostReportRow {
  jobId: string;
  workOrderDocNumber?: string | null;
  title?: string | null;
  stage: string;
  estimateDocNumber?: string | null;
  estimateTotalCost?: string | null;
  estimateTotalPrice?: string | null;
  actualTotal: string;
  actualUnitCost: string;
  varianceVsEstimate?: string | null;
  marginVsEstimatePrice?: string | null;
  costPostedAt?: string | null;
  costJournalDoc?: string | null;
}

export async function listJobCostEntries(jobId: string): Promise<JobCostEntry[]> {
  const res = await apiGet<PaginatedResponse<JobCostEntry>>(
    `/mfg/print-jobs/${jobId}/costs?limit=100`,
  );
  return (res as any)?.data ?? [];
}

export async function createJobCostEntry(
  jobId: string,
  payload: {
    entryDate: string;
    costType: JobCostType;
    stage?: string;
    description?: string;
    quantity?: string;
    unitCost: string;
  },
): Promise<JobCostEntry> {
  const res = await apiPost<ApiResponse<JobCostEntry>>(
    `/mfg/print-jobs/${jobId}/costs`,
    payload,
  );
  return (res as any).data ?? res;
}

export async function deleteJobCostEntry(id: string): Promise<void> {
  await apiDelete(`/mfg/job-costs/${id}`);
}

export async function getJobCostSummary(jobId: string): Promise<JobCostSummary> {
  const res = await apiGet<ApiResponse<JobCostSummary>>(
    `/mfg/print-jobs/${jobId}/cost-summary`,
  );
  return (res as any).data ?? res;
}

export async function postJobCostJournal(jobId: string): Promise<JobCostSummary> {
  const res = await apiPost<ApiResponse<JobCostSummary>>(
    `/mfg/print-jobs/${jobId}/post-cost-journal`,
    {},
  );
  return (res as any).data ?? res;
}

export async function voidJobCostJournal(jobId: string): Promise<JobCostSummary> {
  const res = await apiPost<ApiResponse<JobCostSummary>>(
    `/mfg/print-jobs/${jobId}/void-cost-journal`,
    {},
  );
  return (res as any).data ?? res;
}

export async function getJobCostReport(): Promise<JobCostReportRow[]> {
  const res = await apiGet<ApiResponse<JobCostReportRow[]>>(`/mfg/job-costs/report`);
  return (res as any)?.data ?? [];
}

export { apiPatch };
