/**
 * Fase 2 T1 — API client untuk Lot & Batch persediaan (`/erp/inv/lots`).
 * Saldo lot selalu diturunkan dari baris pergerakan stok POSTED.
 */
import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { PaginatedResponse } from './types';

export type InvLotStatus = 'ACTIVE' | 'QUARANTINE' | 'EXPIRED' | 'BLOCKED';

export interface InvLotWarehouseBalance {
  warehouseId: string;
  warehouseName: string | null;
  balance: string;
}

export interface InvLot {
  id: string;
  lotNumber: string;
  itemId: string;
  itemName: string | null;
  itemCode: string | null;
  supplierLotNo: string | null;
  manufactureDate: string | null;
  expiryDate: string | null;
  originGoodsReceiptId: string | null;
  status: InvLotStatus;
  notes: string | null;
  balance: string;
  daysToExpiry: number | null;
  isExpired: boolean;
  perWarehouse?: InvLotWarehouseBalance[];
  createdAt: string;
}

export interface FefoAllocation {
  lotId: string;
  lotNumber: string;
  expiryDate: string | null;
  quantity: string;
}

export interface FefoPlan {
  itemId: string;
  warehouseId: string;
  requested: string;
  allocations: FefoAllocation[];
  shortfall: string;
  covered: boolean;
}

export interface CreateInvLotPayload {
  lotNumber: string;
  itemId: string;
  supplierLotNo?: string;
  manufactureDate?: string;
  expiryDate?: string;
  status?: InvLotStatus;
  notes?: string;
}

export interface UpdateInvLotPayload {
  lotNumber?: string;
  supplierLotNo?: string | null;
  manufactureDate?: string | null;
  expiryDate?: string | null;
  status?: InvLotStatus;
  notes?: string | null;
}

export async function listInvLots(params: {
  page?: number; limit?: number; search?: string; itemId?: string;
  status?: string; expiringWithinDays?: number;
}): Promise<PaginatedResponse<InvLot>> {
  const qs = new URLSearchParams();
  qs.set('page', String(params.page ?? 1));
  qs.set('limit', String(params.limit ?? 20));
  if (params.search) qs.set('search', params.search);
  if (params.itemId) qs.set('itemId', params.itemId);
  if (params.status) qs.set('status', params.status);
  if (params.expiringWithinDays) qs.set('expiringWithinDays', String(params.expiringWithinDays));
  return apiGet<PaginatedResponse<InvLot>>(`/inv/lots?${qs.toString()}`);
}

export async function getInvLot(id: string): Promise<InvLot> {
  const res = await apiGet<{ data: InvLot } | InvLot>(`/inv/lots/${id}`);
  return (res as { data: InvLot }).data ?? (res as InvLot);
}

export async function createInvLot(payload: CreateInvLotPayload): Promise<InvLot> {
  const res = await apiPost<{ data: InvLot } | InvLot>('/inv/lots', payload);
  return (res as { data: InvLot }).data ?? (res as InvLot);
}

export async function updateInvLot(id: string, payload: UpdateInvLotPayload): Promise<InvLot> {
  const res = await apiPatch<{ data: InvLot } | InvLot>(`/inv/lots/${id}`, payload);
  return (res as { data: InvLot }).data ?? (res as InvLot);
}

export async function deleteInvLot(id: string): Promise<void> {
  await apiDelete(`/inv/lots/${id}`);
}

export async function fefoPlan(itemId: string, warehouseId: string, quantity: string): Promise<FefoPlan> {
  const qs = new URLSearchParams({ itemId, warehouseId, quantity });
  const res = await apiGet<{ data: FefoPlan } | FefoPlan>(`/inv/lots/fefo?${qs.toString()}`);
  return (res as { data: FefoPlan }).data ?? (res as FefoPlan);
}
