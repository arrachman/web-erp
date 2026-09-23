// ERP auth API calls — login, logout, getMe
// Endpoints: POST /auth/login, POST /auth/logout, GET /auth/me

import { apiGet, apiPost } from './client';
import type { ApiResponse } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpAuthUser {
  id: string;
  username: string;
  name: string;
  email: string | null;
  erpLevel: string;
}

// ─── Auth API ─────────────────────────────────────────────────────────────────

export async function login(
  login: string,
  password: string,
): Promise<{ accessToken: string; user: ErpAuthUser }> {
  const response = await apiPost<ApiResponse<{ accessToken: string; user: ErpAuthUser }>>(
    '/auth/login',
    { login, password },
  );
  return response.data;
}

export async function logout(): Promise<void> {
  await apiPost<ApiResponse<unknown>>('/auth/logout');
}

export async function getMe(): Promise<ErpAuthUser> {
  const response = await apiGet<ApiResponse<ErpAuthUser>>('/auth/me');
  return response.data;
}
