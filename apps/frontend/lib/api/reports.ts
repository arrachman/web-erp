// ERP Report Templates — CRUD + SQL executor
// Endpoints: /erp/reports

import { apiDelete, apiGet, apiPatch, apiPost, buildApiUrl } from './client';
import type { PaginatedResponse, PaginationParams } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface RptTemplateRecord {
  id: string;
  code: string;
  name: string;
  module: string;
  description?: string | null;
  templateJson: Record<string, unknown>;
  /** Binds this template to a report (`<module>.<report>`, e.g. `fin.trial-balance`). */
  reportKey?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SqlQueryResult {
  rows: Record<string, unknown>[];
  count: number;
  columns: string[];
}

export interface ReportListParams extends PaginationParams {
  module?: string;
  reportKey?: string;
  isActive?: boolean;
}

// ─── API calls ────────────────────────────────────────────────────────────────

export function listReportTemplates(params: ReportListParams = {}) {
  const q: Record<string, string> = {};
  if (params.page) q.page = String(params.page);
  if (params.limit) q.limit = String(params.limit);
  if (params.search) q.search = params.search;
  if (params.module) q.module = params.module;
  if (params.reportKey) q.reportKey = params.reportKey;
  if (params.isActive !== undefined) q.isActive = String(params.isActive);
  if (params.sortBy) q.sortBy = params.sortBy;
  if (params.sortDir) q.sortDir = params.sortDir;
  return apiGet<PaginatedResponse<RptTemplateRecord>>('/reports', q);
}

export function getReportTemplate(id: string) {
  return apiGet<RptTemplateRecord>(`/reports/${id}`);
}

export function createReportTemplate(data: {
  code: string;
  name: string;
  module: string;
  description?: string;
  templateJson?: Record<string, unknown>;
  reportKey?: string | null;
}) {
  return apiPost<RptTemplateRecord>('/reports', data);
}

export function updateReportTemplate(
  id: string,
  data: Partial<{ name: string; module: string; description: string; templateJson: Record<string, unknown>; reportKey: string | null; isActive: boolean }>,
) {
  return apiPatch<RptTemplateRecord>(`/reports/${id}`, data);
}

export function deleteReportTemplate(id: string) {
  return apiDelete<{ id: string }>(`/reports/${id}`);
}

export function executeSqlQuery(sql: string, params: Record<string, unknown> = {}, limit = 100) {
  return apiPost<SqlQueryResult>('/reports/execute-sql', { sql, params, limit });
}

/** Materialize an auto-template into explicit editable bands from the report's real columns. */
export function materializeReportTemplate(id: string) {
  return apiPost<RptTemplateRecord>(`/reports/${id}/materialize`, {});
}

/** Render a template (as edited in the designer) to a PDF blob with sample data. */
export async function previewReportTemplate(templateJson: Record<string, unknown>): Promise<Blob> {
  const res = await fetch(buildApiUrl('/reports/preview'), {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ templateJson }),
  });
  if (!res.ok) throw new Error(`Preview gagal (HTTP ${res.status})`);
  return res.blob();
}
