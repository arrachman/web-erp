/**
 * Fase 3 W5 — API client konfirmasi pembayaran portal (transfer manual).
 * Backend: `/erp/payments` (ErpPortalPaymentsAdminController).
 */
import { apiGet, apiPost } from './client';

export interface PortalPaymentAdmin {
  id: string;
  invoiceId: string;
  invoiceNumber: string | null;
  partnerId: string;
  partnerName: string | null;
  provider: string;
  providerRef: string;
  amount: string;
  status: 'PENDING' | 'MENUNGGU_KONFIRMASI' | 'PAID' | 'DITOLAK' | 'EXPIRED';
  paidAt: string | null;
  receiptId: string | null;
  claimedAt: string | null;
  claimNote: string | null;
  rejectedReason: string | null;
  createdAt: string;
}

export interface TransferTarget {
  bankName: string;
  accountNumber: string;
  accountHolder: string;
  accountName: string;
}

export async function listAdminPayments(status?: string): Promise<PortalPaymentAdmin[]> {
  const res = await apiGet(`/payments${status ? `?status=${status}` : ''}`);
  const rows = (res as any)?.data ?? res;
  return Array.isArray(rows) ? rows : [];
}

export async function confirmAdminPayment(id: string): Promise<PortalPaymentAdmin> {
  const res = await apiPost(`/payments/${id}/confirm`, {});
  return ((res as any)?.data ?? res) as PortalPaymentAdmin;
}

export async function rejectAdminPayment(id: string, reason: string): Promise<PortalPaymentAdmin> {
  const res = await apiPost(`/payments/${id}/reject`, { reason });
  return ((res as any)?.data ?? res) as PortalPaymentAdmin;
}

export async function getTransferTarget(): Promise<TransferTarget | null> {
  const res = await apiGet('/payments/transfer-target');
  const data = (res as any)?.data ?? res;
  return (data as TransferTarget) ?? null;
}
