// Fase 2 P3 — Penjadwalan Produksi API (mesin + jadwal job).
// Endpoints: /mfg/machines, /mfg/job-schedules

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse } from './types';

export const MACHINE_TYPES = ['OFFSET', 'DIGITAL', 'FINISHING', 'LAIN'] as const;
export type MachineType = (typeof MACHINE_TYPES)[number];
export const MACHINE_STATUSES = ['ACTIVE', 'MAINTENANCE', 'INACTIVE'] as const;
export type MachineStatus = (typeof MACHINE_STATUSES)[number];
export const SCHEDULE_STATUSES = ['TERJADWAL', 'BERJALAN', 'SELESAI', 'BATAL'] as const;
export type ScheduleStatus = (typeof SCHEDULE_STATUSES)[number];

export interface MfgMachine {
  id: string;
  code: string;
  name: string;
  machineType: MachineType;
  capacityPerHour: string;
  capacityUnit?: string | null;
  workStart: string;
  workEnd: string;
  status: MachineStatus;
  legacyCode?: string | null;
  notes?: string | null;
}

export interface JobSchedule {
  id: string;
  jobId: string;
  machineId: string;
  stage?: string | null;
  plannedStart: string;
  plannedEnd: string;
  actualStart?: string | null;
  actualEnd?: string | null;
  status: ScheduleStatus;
  sequenceNo: number;
  notes?: string | null;
  workOrderDocNumber?: string | null;
  jobTitle?: string | null;
  jobStage?: string | null;
  printQuantity?: string | null;
  machineCode?: string | null;
  machineName?: string | null;
}

export async function listMachines(): Promise<MfgMachine[]> {
  const res = await apiGet<ApiResponse<MfgMachine[]>>(`/mfg/machines`);
  return (res as any)?.data ?? [];
}

export async function createMachine(payload: {
  code: string;
  name: string;
  machineType: MachineType;
  capacityPerHour?: string;
  capacityUnit?: string;
}): Promise<MfgMachine> {
  const res = await apiPost<ApiResponse<MfgMachine>>(`/mfg/machines`, payload);
  return (res as any).data ?? res;
}

export async function updateMachine(
  id: string,
  payload: Partial<{ name: string; status: MachineStatus; capacityPerHour: string }>,
): Promise<MfgMachine> {
  const res = await apiPatch<ApiResponse<MfgMachine>>(`/mfg/machines/${id}`, payload);
  return (res as any).data ?? res;
}

export async function deleteMachine(id: string): Promise<void> {
  await apiDelete(`/mfg/machines/${id}`);
}

export async function listSchedules(params: {
  from?: string;
  to?: string;
  machineId?: string;
  jobId?: string;
} = {}): Promise<JobSchedule[]> {
  const qs = new URLSearchParams();
  if (params.from) qs.set('from', params.from);
  if (params.to) qs.set('to', params.to);
  if (params.machineId) qs.set('machineId', params.machineId);
  if (params.jobId) qs.set('jobId', params.jobId);
  const res = await apiGet<ApiResponse<JobSchedule[]>>(
    `/mfg/job-schedules${qs.toString() ? `?${qs}` : ''}`,
  );
  return (res as any)?.data ?? [];
}

export async function createSchedule(payload: {
  jobId: string;
  machineId: string;
  stage?: string;
  plannedStart: string;
  plannedEnd: string;
}): Promise<JobSchedule> {
  const res = await apiPost<ApiResponse<JobSchedule>>(`/mfg/job-schedules`, payload);
  return (res as any).data ?? res;
}

export async function setScheduleStatus(id: string, status: ScheduleStatus): Promise<JobSchedule> {
  const res = await apiPost<ApiResponse<JobSchedule>>(`/mfg/job-schedules/${id}/status`, { status });
  return (res as any).data ?? res;
}

export async function deleteSchedule(id: string): Promise<void> {
  await apiDelete(`/mfg/job-schedules/${id}`);
}
