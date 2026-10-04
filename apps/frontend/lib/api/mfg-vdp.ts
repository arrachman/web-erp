// Fase 2 P4 — Variable Data Printing API (dataset per job).
// Endpoints: /mfg/print-jobs/:jobId/vdp-datasets, /mfg/vdp-datasets/...

import { apiDelete, apiGet, apiPost } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

export interface VdpDataset {
  id: string;
  jobId: string;
  name: string;
  sourceFilename?: string | null;
  version: number;
  columns: string[];
  requiredColumns: string[];
  rowCount: number;
  validCount: number;
  invalidCount: number;
  status: 'DRAFT' | 'TERKUNCI';
  lockedAt?: string | null;
  notes?: string | null;
}

export interface VdpRow {
  id: string;
  rowNo: number;
  data: Record<string, string>;
  isValid: boolean;
  errorNote?: string | null;
}

export async function listJobDatasets(
  jobId: string,
): Promise<{ data: VdpDataset[]; jobStage: string; printQuantity: string }> {
  const res: any = await apiGet(`/mfg/print-jobs/${jobId}/vdp-datasets`);
  return res?.data
    ? res
    : { data: [], jobStage: '', printQuantity: '0' };
}

export async function createDataset(
  jobId: string,
  payload: { name: string; sourceFilename?: string; requiredColumns?: string[] },
): Promise<VdpDataset> {
  const res = await apiPost<ApiResponse<VdpDataset>>(
    `/mfg/print-jobs/${jobId}/vdp-datasets`,
    payload,
  );
  return (res as any).data ?? res;
}

export async function importDatasetRows(
  datasetId: string,
  payload: { csvText?: string; rows?: Record<string, unknown>[]; sourceFilename?: string },
): Promise<VdpDataset> {
  const res = await apiPost<ApiResponse<VdpDataset>>(
    `/mfg/vdp-datasets/${datasetId}/import`,
    payload,
  );
  return (res as any).data ?? res;
}

export async function listDatasetRows(
  datasetId: string,
  onlyInvalid = false,
): Promise<VdpRow[]> {
  const res = await apiGet<PaginatedResponse<VdpRow>>(
    `/mfg/vdp-datasets/${datasetId}/rows?limit=100${onlyInvalid ? '&onlyInvalid=true' : ''}`,
  );
  return (res as any)?.data ?? [];
}

export async function lockDataset(datasetId: string): Promise<VdpDataset> {
  const res = await apiPost<ApiResponse<VdpDataset>>(`/mfg/vdp-datasets/${datasetId}/lock`, {});
  return (res as any).data ?? res;
}

export async function deleteDataset(datasetId: string): Promise<void> {
  await apiDelete(`/mfg/vdp-datasets/${datasetId}`);
}
