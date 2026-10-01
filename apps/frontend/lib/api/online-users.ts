// API client for active user sessions — GET /erp/users/online
import { apiGet } from './client';

export interface ErpOnlineUser {
  id: string;
  userId: string;
  user: {
    id: string;
    code: string;
    name: string;
    email: string;
    isActive: boolean;
  } | null;
  deviceName: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  lastActiveAt: string;
  expiresAt: string;
  createdAt: string;
}

export interface OnlineUsersResponse {
  success: boolean;
  data: ErpOnlineUser[];
  meta: { count: number; windowMinutes: number };
}

export async function listOnlineUsers(windowMinutes?: number): Promise<OnlineUsersResponse> {
  return apiGet<OnlineUsersResponse>(
    '/users/online',
    windowMinutes !== undefined ? { window: windowMinutes } : undefined,
  );
}
