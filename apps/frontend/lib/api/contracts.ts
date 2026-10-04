/**
 * Fase 3 W7 — API client harga kontrak per sekolah + paket/bundle
 * (`/erp/contracts`). Resolusi harga berlapis: item > kategori > sekolah.
 */
import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { PaginatedResponse } from './types';

export type ContractScope = 'ITEM' | 'KATEGORI' | 'SEKOLAH';

export interface ContractPrice {
  id: string;
  partnerId: string;
  partnerName: string | null;
  itemId: string | null;
  itemName: string | null;
  categoryId: string | null;
  categoryName: string | null;
  scope: ContractScope;
  price: string | null;
  discountPercent: string | null;
  validFrom: string | null;
  validTo: string | null;
  isActive: boolean;
  notes: string | null;
  createdAt: string;
}

export interface CreateContractPricePayload {
  partnerId: string;
  itemId?: string;
  categoryId?: string;
  price?: string;
  discountPercent?: string;
  validFrom?: string;
  validTo?: string;
  notes?: string;
}

export interface BundleLine {
  id?: string;
  componentItemId: string;
  componentName?: string | null;
  componentCode?: string | null;
  quantity: string;
  notes?: string | null;
}

export interface ItemBundle {
  id: string;
  itemId: string;
  itemName: string | null;
  itemCode: string | null;
  name: string | null;
  notes: string | null;
  isActive: boolean;
  lines: BundleLine[];
  createdAt: string;
}

export async function listContractPrices(params: {
  page?: number; limit?: number; partnerId?: string;
}): Promise<PaginatedResponse<ContractPrice>> {
  const qs = new URLSearchParams();
  qs.set('page', String(params.page ?? 1));
  qs.set('limit', String(params.limit ?? 50));
  if (params.partnerId) qs.set('partnerId', params.partnerId);
  return apiGet<PaginatedResponse<ContractPrice>>(`/contracts/prices?${qs.toString()}`);
}

export async function createContractPrice(payload: CreateContractPricePayload): Promise<ContractPrice> {
  const res = await apiPost<{ data: ContractPrice } | ContractPrice>('/contracts/prices', payload);
  return (res as { data: ContractPrice }).data ?? (res as ContractPrice);
}

export async function updateContractPrice(
  id: string,
  payload: { price?: string; discountPercent?: string; validFrom?: string | null; validTo?: string | null; isActive?: boolean; notes?: string | null },
): Promise<ContractPrice> {
  const res = await apiPatch<{ data: ContractPrice } | ContractPrice>(`/contracts/prices/${id}`, payload);
  return (res as { data: ContractPrice }).data ?? (res as ContractPrice);
}

export async function deleteContractPrice(id: string): Promise<void> {
  await apiDelete(`/contracts/prices/${id}`);
}

export async function listBundles(): Promise<{ data: ItemBundle[]; total: number }> {
  const res = await apiGet<{ data: ItemBundle[]; total: number }>('/contracts/bundles');
  return { data: (res as any).data ?? [], total: (res as any).total ?? 0 };
}

export async function upsertBundle(payload: {
  itemId: string;
  name?: string;
  notes?: string;
  isActive?: boolean;
  lines: { componentItemId: string; quantity: string; notes?: string }[];
}): Promise<ItemBundle> {
  const res = await apiPost<{ data: ItemBundle } | ItemBundle>('/contracts/bundles', payload);
  return (res as { data: ItemBundle }).data ?? (res as ItemBundle);
}

export async function deleteBundle(id: string): Promise<void> {
  await apiDelete(`/contracts/bundles/${id}`);
}
