// Fase 3 W3 — admin akun Portal Sekolah (antrean persetujuan + lead landing).
// Endpoints: /portal/admin/accounts, /portal/admin/leads (JWT internal ERP).

import { apiGet, apiPost } from './client';
import type { ApiResponse } from './types';

export interface PortalAccount {
  id: string;
  email: string;
  fullName: string;
  phone?: string | null;
  role: 'KEPALA_SEKOLAH' | 'BENDAHARA' | 'OPERATOR';
  status: 'PENDING' | 'ACTIVE' | 'REJECTED' | 'SUSPENDED';
  schoolName: string;
  npsn?: string | null;
  jenjang?: string | null;
  partnerId?: string | null;
  lastLoginAt?: string | null;
  createdAt: string;
}

export interface PortalLead {
  id: string;
  schoolName: string;
  contactName: string;
  phone?: string | null;
  email?: string | null;
  jenjang?: string | null;
  message?: string | null;
  status: string;
  partnerId?: string | null;
  createdAt: string;
}

export async function listPortalAccounts(
  status?: string,
  search?: string,
): Promise<{ data: PortalAccount[]; total: number }> {
  const q = new URLSearchParams();
  if (status) q.set('status', status);
  if (search) q.set('search', search);
  const qs = q.toString();
  const res: any = await apiGet(`/portal/admin/accounts${qs ? `?${qs}` : ''}`);
  return res?.data ? res : { data: [], total: 0 };
}

export async function listPortalLeads(): Promise<{ data: PortalLead[]; total: number }> {
  const res: any = await apiGet('/portal/admin/leads');
  return res?.data ? res : { data: [], total: 0 };
}

async function action(id: string, verb: string, payload?: unknown): Promise<PortalAccount> {
  const res = await apiPost<ApiResponse<PortalAccount>>(
    `/portal/admin/accounts/${id}/${verb}`,
    payload ?? {},
  );
  return (res as any).data ?? res;
}

export const approvePortalAccount = (id: string) => action(id, 'approve');
export const rejectPortalAccount = (id: string, reason?: string) =>
  action(id, 'reject', { reason });
export const suspendPortalAccount = (id: string) => action(id, 'suspend');
export const activatePortalAccount = (id: string) => action(id, 'activate');
