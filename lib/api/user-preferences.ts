// Current-user preferences API — backed by adm_user_preferences.
// Endpoints: GET /user-preferences/me, PUT /user-preferences/me

import { apiGet, apiPut } from './client';
import type { ApiResponse } from './types';

export interface ErpUserPreferences {
  userId: string;
  theme: string | null;
  language: string | null;
  timezone: string | null;
  dateFormat: string | null;
  numberFormat: string | null;
  tablePageSize: number | null;
  sidebarCollapsed: boolean;
  metadata: Record<string, unknown> | null;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateUserPreferencesInput {
  theme?: string;
  language?: string;
  timezone?: string;
  dateFormat?: string;
  numberFormat?: string;
  tablePageSize?: number;
  sidebarCollapsed?: boolean;
  metadata?: Record<string, unknown>;
}

export async function getMyPreferences(): Promise<ErpUserPreferences | null> {
  const res = await apiGet<ApiResponse<ErpUserPreferences | null>>(
    '/user-preferences/me',
  );
  return res.data ?? null;
}

export async function updateMyPreferences(
  input: UpdateUserPreferencesInput,
): Promise<ErpUserPreferences> {
  const res = await apiPut<ApiResponse<ErpUserPreferences>>(
    '/user-preferences/me',
    input,
  );
  return res.data;
}
