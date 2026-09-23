// ERP Settings resource API — GET list + PATCH per key
// Endpoints: /settings, /settings/:key

import { apiGet, apiPatch } from './client';
import type { ApiResponse, PaginatedResponse } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpSetting {
  key: string;
  value: string | null;
  description?: string | null;
  group?: string | null;
  updatedAt: string;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getSettings(
  params?: { group?: string; search?: string },
): Promise<PaginatedResponse<ErpSetting>> {
  return apiGet<PaginatedResponse<ErpSetting>>('/settings', params as Record<string, string | number | boolean | undefined>);
}

export async function updateSetting(
  key: string,
  value: string,
): Promise<ErpSetting> {
  const res = await apiPatch<ApiResponse<ErpSetting>>(`/settings/${key}`, { value });
  return res.data;
}
