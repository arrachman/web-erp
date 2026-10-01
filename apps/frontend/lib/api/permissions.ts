// ERP Permission resource API — READ-ONLY list for adm_permissions
// Endpoints: /permissions (paginated)

import { apiGet } from './client';
import type { PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpPermission {
  id: string;
  code: string;
  name: string;
  group: string | null;
  description: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ListPermissionParams extends PaginationParams {
  resource?: string;
  action?: string;
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function listPermissions(
  params?: ListPermissionParams,
): Promise<PaginatedResponse<ErpPermission>> {
  return apiGet<PaginatedResponse<ErpPermission>>(
    '/permissions',
    params as Record<string, string | number | boolean | undefined>,
  );
}
