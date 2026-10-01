// ERP Approval Rule resource API — CRUD for sys_approval_rules
// Endpoints: /approval-rules
//
// NOTE: server returns `documentType` (no `code`). SimpleMasterPage's
// BaseEntity requires `code`, so listApprovalRules maps each row to
// add `code: row.documentType`.

import { apiGet, apiPost, apiPatch, apiDelete } from './client';
import type { ApiResponse, PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ErpApprovalRule {
  id: string;
  code: string; // mirror of documentType — required by SimpleMasterPage
  documentType: string;
  name: string;
  level: number;
  requiresApproval: boolean;
  minAmount?: string | null;
  approverRoleId?: string | null;
  notes?: string | null;
  isActive: boolean;
  legacyCode?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateApprovalRulePayload {
  documentType: string;
  name: string;
  level?: number;
  requiresApproval?: boolean;
  minAmount?: string;
  approverRoleId?: string;
  notes?: string;
  isActive?: boolean;
}

export type UpdateApprovalRulePayload = Partial<CreateApprovalRulePayload>;

// ─── API functions ────────────────────────────────────────────────────────────

export async function listApprovalRules(
  params?: PaginationParams,
): Promise<PaginatedResponse<ErpApprovalRule>> {
  const res = await apiGet<PaginatedResponse<ErpApprovalRule>>(
    '/approval-rules',
    params as Record<string, string | number | boolean | undefined>,
  );
  return {
    ...res,
    data: res.data.map((row) => ({ ...row, code: row.documentType })),
  };
}

export async function createApprovalRule(
  payload: CreateApprovalRulePayload,
): Promise<ErpApprovalRule> {
  const res = await apiPost<ApiResponse<ErpApprovalRule>>('/approval-rules', payload);
  return { ...res.data, code: res.data.documentType };
}

export async function updateApprovalRule(
  id: string,
  payload: UpdateApprovalRulePayload,
): Promise<ErpApprovalRule> {
  const res = await apiPatch<ApiResponse<ErpApprovalRule>>(
    `/approval-rules/${id}`,
    payload,
  );
  return { ...res.data, code: res.data.documentType };
}

export async function deleteApprovalRule(id: string): Promise<void> {
  await apiDelete<void>(`/approval-rules/${id}`);
}

export async function bulkUpdateErpApprovalRuleStatus(
  ids: string[],
  isActive: boolean,
): Promise<{ affected: number }> {
  const res = await apiPatch<{ success: boolean; affected: number }>(
    '/approval-rules/bulk/status',
    { ids, isActive },
  );
  return { affected: res.affected };
}

export async function bulkDeleteErpApprovalRules(
  ids: string[],
): Promise<{ affected: number }> {
  const res = await apiDelete<{ success: boolean; affected: number }>(
    '/approval-rules/bulk',
    { ids },
  );
  return { affected: res.affected };
}
