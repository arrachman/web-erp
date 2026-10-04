// Fase 2 P8 — Sales Lapangan API (memakai endpoint yang sudah live):
// sekolah & aktivitas A1, order penjualan (channel SALES), faktur untuk piutang.

import { apiGet, apiPost } from './client';
import type { ApiResponse } from './types';

export interface FieldSchool {
  id: string;
  name: string;
}

export interface FieldItem {
  id: string;
  name: string;
  baseUnitId?: string | null;
  salePrice: number;
}

export interface FieldInvoice {
  id: string;
  docNumber: string;
  docDate?: string | null;
  grandTotal: string;
  settlementStatus: string;
  status: string;
  customerId?: string | null;
}

export async function listFieldSchools(): Promise<FieldSchool[]> {
  const res: any = await apiGet(`/schools?limit=200`);
  const rows = res?.data ?? [];
  return rows.map((s: any) => ({ id: String(s.partnerId ?? s.id), name: s.name ?? s.partnerName ?? '' }));
}

export async function postSchoolVisit(schoolId: string, notes: string): Promise<void> {
  await apiPost(`/schools/${schoolId}/activities`, { type: 'VISIT', notes });
}

export async function listFieldItems(): Promise<FieldItem[]> {
  const res: any = await apiGet(`/items?limit=200`);
  const rows = res?.data ?? [];
  return rows.map((it: any) => ({
    id: String(it.id),
    name: it.name ?? '',
    baseUnitId: it.baseUnitId ? String(it.baseUnitId) : null,
    salePrice: Number(it.salePrice ?? it.price ?? it.masterPrice ?? 0),
  }));
}

export async function createFieldOrder(payload: {
  customerId: string;
  lines: { itemId: string; unitId: string; quantity: string; unitPrice: string }[];
}): Promise<{ id: string; docNumber: string }> {
  const today = new Date().toISOString().slice(0, 10);
  const res = await apiPost<ApiResponse<any>>(`/sls/orders`, {
    branchId: '1033',
    customerId: payload.customerId,
    currencyId: '1',
    exchangeRate: '1',
    docDate: today,
    channel: 'SALES',
    lines: payload.lines.map((l, i) => ({ lineNo: i + 1, ...l })),
  });
  const d: any = (res as any).data ?? res;
  return { id: String(d.id), docNumber: d.docNumber };
}

export async function listSchoolInvoices(customerId: string): Promise<FieldInvoice[]> {
  const res: any = await apiGet(`/sls/invoices?limit=200`);
  const rows: any[] = res?.data ?? [];
  return rows
    .filter((inv) => String(inv.customerId ?? '') === String(customerId))
    .map((inv) => ({
      id: String(inv.id),
      docNumber: inv.docNumber,
      docDate: inv.docDate ? String(inv.docDate).slice(0, 10) : null,
      grandTotal: String(inv.grandTotal ?? '0'),
      settlementStatus: inv.settlementStatus ?? 'UNPAID',
      status: inv.status ?? '',
      customerId: inv.customerId ? String(inv.customerId) : null,
    }));
}
