// Current-user notifications API — backed by sys_notifications.
// Endpoints under /erp/notifications/me.

import { apiDelete, apiGet, apiPatch, apiPost } from './client';
import type { ApiResponse } from './types';

export type ErpNotificationType = 'success' | 'info' | 'warn' | 'danger' | string;

export interface ErpNotification {
  id: string;
  recipientId: string;
  type: ErpNotificationType;
  title: string;
  body: string;
  referenceEntity: string | null;
  referenceId: string | null;
  actionUrl: string | null;
  readAt: string | null;
  archivedAt: string | null;
  expiresAt: string | null;
  createdAt: string;
}

export interface ListNotificationsMeta {
  page: number;
  limit: number;
  total: number;
  totalPages: number;
  unreadCount: number;
}

export interface ListNotificationsParams {
  page?: number;
  limit?: number;
  unreadOnly?: boolean;
}

export async function listMyNotifications(
  params: ListNotificationsParams = {},
): Promise<{ data: ErpNotification[]; meta: ListNotificationsMeta }> {
  const qs = new URLSearchParams();
  if (params.page) qs.set('page', String(params.page));
  if (params.limit) qs.set('limit', String(params.limit));
  if (params.unreadOnly) qs.set('unreadOnly', 'true');
  const q = qs.toString();
  const res = await apiGet<ApiResponse<ErpNotification[]> & { meta: ListNotificationsMeta }>(
    `/notifications/me${q ? `?${q}` : ''}`,
  );
  return { data: res.data ?? [], meta: res.meta };
}

export async function getMyUnreadCount(): Promise<number> {
  const res = await apiGet<ApiResponse<{ unreadCount: number }>>(
    '/notifications/me/unread-count',
  );
  return res.data?.unreadCount ?? 0;
}

export async function markNotificationRead(id: string): Promise<ErpNotification> {
  const res = await apiPatch<ApiResponse<ErpNotification>>(
    `/notifications/me/${id}/read`,
  );
  return res.data;
}

export async function markAllNotificationsRead(): Promise<number> {
  const res = await apiPost<ApiResponse<{ updated: number }>>(
    '/notifications/me/read-all',
  );
  return res.data?.updated ?? 0;
}

export async function archiveNotification(id: string): Promise<void> {
  await apiDelete<ApiResponse<unknown>>(`/notifications/me/${id}`);
}
