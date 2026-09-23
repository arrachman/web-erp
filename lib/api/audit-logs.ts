// ERP Audit Logs — read-only access to sys_audit_logs
// Endpoint: GET /api/erp/audit-logs (apiGet base = /api/erp, so path = /audit-logs)

import { apiGet } from './client';
import type { PaginatedResponse } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export type AuditAction =
  | 'CREATE'
  | 'UPDATE'
  | 'DELETE'
  | 'RESTORE'
  | 'LOGIN'
  | 'LOGOUT'
  | 'APPROVE'
  | 'REJECT'
  | 'POST'
  | 'REOPEN';

/** Alias matching the task-spec naming convention. */
export type ErpAuditAction = AuditAction;

export interface ErpAuditActor {
  id: string;
  code: string;
  name: string;
}

export interface FieldDiff {
  from: unknown;
  to: unknown;
}

export interface ErpAuditLog {
  id: string;
  action: AuditAction;
  entityName: string;
  entityId: string | null;
  summary: string | null;
  changes: Record<string, FieldDiff> | null;
  actorIp: string | null;
  occurredAt: string;
  actor: ErpAuditActor | null;
}

export interface AuditLogParams {
  entityName?: string;
  entityId?: string;
  action?: AuditAction;
  actorId?: string;
  from?: string;
  to?: string;
  page?: number;
  limit?: number;
}

/** Alias matching the task-spec naming convention. */
export type QueryAuditLogsParams = AuditLogParams;

// ─── API ──────────────────────────────────────────────────────────────────────

export async function listAuditLogs(
  params: AuditLogParams,
): Promise<PaginatedResponse<ErpAuditLog>> {
  const query: Record<string, string | number | boolean | undefined> = {};
  if (params.entityName !== undefined) query.entityName = params.entityName;
  if (params.entityId !== undefined) query.entityId = params.entityId;
  if (params.action !== undefined) query.action = params.action;
  if (params.actorId !== undefined) query.actorId = params.actorId;
  if (params.from !== undefined) query.from = params.from;
  if (params.to !== undefined) query.to = params.to;
  if (params.page !== undefined) query.page = params.page;
  if (params.limit !== undefined) query.limit = params.limit;
  return apiGet<PaginatedResponse<ErpAuditLog>>('/audit-logs', query);
}
