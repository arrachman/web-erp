// Fase 2 P6 — Packing per Siswa API.
// Endpoints: /sls/packing-lists/:id/units, /sls/packing-units/...

import { apiDelete, apiGet, apiPost } from './client';
import type { ApiResponse } from './types';

export interface PackingListOverview {
  id: string;
  docNumber: string;
  docDate?: string | null;
  customerName?: string | null;
  status?: string | null;
  units: { total: number; packed: number };
}

export interface PackingUnitContent {
  itemId: string;
  name?: string | null;
  quantity: string;
}

export interface PackingUnit {
  id: string;
  packingListId: string;
  sequenceNo: number;
  studentName: string;
  className?: string | null;
  contents: PackingUnitContent[];
  status: 'PENDING' | 'PACKED';
  packedAt?: string | null;
}

export async function getPackingOverview(): Promise<PackingListOverview[]> {
  const res = await apiGet<ApiResponse<PackingListOverview[]>>(
    `/sls/packing-units/overview`,
  );
  return (res as any)?.data ?? [];
}

export async function listPackingUnits(
  packingListId: string,
): Promise<{ data: PackingUnit[]; progress: { total: number; packed: number } }> {
  const res: any = await apiGet<any>(`/sls/packing-lists/${packingListId}/units`);
  // Layanan mengembalikan { data, progress } apa adanya (tanpa envelope).
  return res?.progress ? res : { data: res?.data ?? [], progress: { total: 0, packed: 0 } };
}

export async function generatePackingUnits(
  packingListId: string,
  payload: { students?: { studentName: string; className?: string }[]; rosterCsv?: string },
): Promise<{ data: PackingUnit[]; progress: { total: number; packed: number } }> {
  const res: any = await apiPost<any>(
    `/sls/packing-lists/${packingListId}/units/generate`,
    payload,
  );
  return res?.progress ? res : { data: res?.data ?? [], progress: { total: 0, packed: 0 } };
}

export async function setPackingUnitPacked(
  unitId: string,
  packed: boolean,
): Promise<PackingUnit> {
  const res = await apiPost<ApiResponse<PackingUnit>>(
    `/sls/packing-units/${unitId}/pack`,
    { packed },
  );
  return (res as any).data ?? res;
}

export async function deletePackingUnit(unitId: string): Promise<void> {
  await apiDelete(`/sls/packing-units/${unitId}`);
}
