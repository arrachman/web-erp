import { apiDelete, apiGet, apiPatch, apiPost } from './client';

/**
 * Fase 3 W6 — WhatsApp admin API (gateway self-hosted :3204 via backend).
 * Token akun/device tidak pernah sampai ke browser; semua lewat backend.
 */

export interface WaSettingsSummary {
  gatewayUrl: string;
  accountConfigured: boolean;
  webhookSecretConfigured: boolean;
  activeDeviceConfigured: boolean;
  senderNumber: string | null;
  sendEnabled: boolean;
}

export interface WaHealth {
  gatewayUrl: string;
  accountConfigured: boolean;
  activeDeviceConfigured: boolean;
  senderNumber: string | null;
  sendEnabled: boolean;
  gateway: { up: boolean; degraded?: boolean; devices?: number };
  activeDevice: { connected: boolean; devicePhone?: string; deviceName?: string };
}

export interface WaDevice {
  name?: string;
  device?: string;
  status?: string;
  token?: string;
  autoread?: string;
  isActive: boolean;
}

export interface WaTemplate {
  id: string;
  name: string;
  category: string;
  triggerEvent: string | null;
  body: string;
  isActive: boolean;
}

export interface WaLog {
  id: string;
  templateName: string | null;
  recipientType: string;
  recipientPhone: string;
  body: string;
  status: string;
  errorReason: string | null;
  createdAt: string;
}

interface Envelope<T> {
  success: boolean;
  data: T;
  message?: string;
  meta?: { page: number; limit: number; total: number; totalPages: number };
}

const unwrap = <T,>(r: Envelope<T> | T): T => (r as Envelope<T>)?.data ?? (r as T);

export async function getWaHealth(): Promise<WaHealth> {
  return unwrap(await apiGet<Envelope<WaHealth>>('/wa/connection-health'));
}

export async function updateWaSettings(sendEnabled: boolean): Promise<WaSettingsSummary> {
  return unwrap(await apiPatch<Envelope<WaSettingsSummary>>('/wa/settings', { sendEnabled }));
}

export async function listWaDevices(): Promise<{ devices: WaDevice[]; hasActiveDevice: boolean }> {
  return unwrap(await apiGet('/settings/wa-devices'));
}

export async function addWaDevice(body: {
  name: string;
  phone: string;
  autoread?: 'on' | 'off';
}): Promise<{ deviceToken: string; devicePhone: string; reused?: boolean }> {
  return unwrap(await apiPost('/settings/wa-devices', body));
}

export async function getWaDeviceQr(deviceToken: string, force?: boolean) {
  return unwrap(
    await apiPost<{ qrUrl?: string; alreadyConnected?: boolean }>('/settings/wa-devices/qr', {
      deviceToken,
      force,
    }),
  );
}

export async function checkWaDevice(deviceToken: string) {
  return unwrap(
    await apiPost<{ connected: boolean; devicePhone?: string; deviceName?: string }>(
      '/settings/wa-devices/check',
      { deviceToken },
    ),
  );
}

export async function activateWaDevice(deviceToken: string, devicePhone?: string) {
  return unwrap(
    await apiPost('/settings/wa-devices/activate', { deviceToken, devicePhone }),
  ) as { activeDeviceConfigured: boolean; senderNumber: string | null };
}

export async function removeWaDevice(phone: string): Promise<void> {
  await apiDelete(`/settings/wa-devices/${encodeURIComponent(phone)}`);
}

export async function listWaTemplates(): Promise<WaTemplate[]> {
  const r = await apiGet<Envelope<WaTemplate[]>>('/wa/templates?limit=100');
  return unwrap(r);
}

export async function createWaTemplate(body: Partial<WaTemplate>): Promise<WaTemplate> {
  return unwrap(await apiPost<Envelope<WaTemplate>>('/wa/templates', body));
}

export async function updateWaTemplate(id: string, body: Partial<WaTemplate>): Promise<WaTemplate> {
  return unwrap(await apiPatch<Envelope<WaTemplate>>(`/wa/templates/${id}`, body));
}

export async function removeWaTemplate(id: string): Promise<void> {
  await apiDelete(`/wa/templates/${id}`);
}

export async function listWaLogs(): Promise<WaLog[]> {
  const r = await apiGet<Envelope<WaLog[]>>('/wa/logs?limit=30');
  return unwrap(r);
}

export async function getWaStats(): Promise<{
  sentToday: number;
  readToday: number;
  failedToday: number;
  readRate: number;
}> {
  return unwrap(await apiGet('/wa/stats'));
}

export async function sendWaTest(body: {
  phone: string;
  templateId?: string;
  body?: string;
}): Promise<{ logId: string; status: string }> {
  const r = await apiPost<Envelope<{ logId: string; status: string }>>('/wa/send-test', body);
  return unwrap(r);
}

export async function resendWaLog(id: string): Promise<{ logId: string; status: string }> {
  const r = await apiPost<Envelope<{ logId: string; status: string }>>(`/wa/logs/${id}/resend`, {});
  return unwrap(r);
}
