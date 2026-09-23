// ERP data-import API client.
// - getImportEntities / listImportJobs use the standard JSON GET helper.
// - importFile uploads multipart/form-data via a raw fetch built on the same
//   base URL + cookie-credentials approach as client.ts (apiPost only sends
//   JSON, so we cannot reuse it for file uploads).
// - downloadTemplate reuses the shared cookie-auth blob downloader.

import { apiGet, buildApiUrl, downloadFile, ErpApiError } from './client';
import type { ApiError, ApiResponse } from './types';

// ─── Types ────────────────────────────────────────────────────────────────────

export interface ImportEntity {
  value: string;
  label: string;
  requiredHeaders: string[];
  optionalHeaders: string[];
}

export interface ImportRowError {
  row: number;
  message: string;
}

export interface ImportResult {
  jobId: string;
  total: number;
  ok: number;
  failed: number;
  errors: ImportRowError[];
  status?: string;
  async?: boolean;
}

export interface ImportJob {
  id: string;
  entity: string;
  fileName: string;
  status: string;
  rowsTotal: number;
  rowsOk: number;
  rowsFailed: number;
  errors?: ImportRowError[];
  createdAt: string;
}

// ─── Calls ──────────────────────────────────────────────────────────────────

export function getImportEntities(): Promise<ApiResponse<ImportEntity[]>> {
  return apiGet<ApiResponse<ImportEntity[]>>('/import/entities');
}

export function listImportJobs(): Promise<ApiResponse<ImportJob[]>> {
  return apiGet<ApiResponse<ImportJob[]>>('/import/jobs');
}

export function downloadTemplate(entity: string): Promise<void> {
  return downloadFile(`/import/template/${entity}`, undefined, `template-${entity}.xlsx`);
}

export async function importFile(
  entity: string,
  file: File,
): Promise<ApiResponse<ImportResult>> {
  const form = new FormData();
  form.append('file', file);

  const response = await fetch(buildApiUrl(`/import/${entity}`), {
    method: 'POST',
    credentials: 'include',
    body: form,
  });

  if (!response.ok) {
    // Mirror packages/ui-kit toApiError: Nest sends error as string + message at root.
    const fallbackMessage =
      response.statusText || `Impor gagal (HTTP ${response.status})`;
    let apiError: ApiError;
    try {
      const payload = (await response.json()) as Record<string, unknown>;
      const topMessage =
        typeof payload.message === 'string'
          ? payload.message
          : Array.isArray(payload.message)
            ? payload.message.filter((m): m is string => typeof m === 'string').join('; ')
            : undefined;
      if (payload.error && typeof payload.error === 'object') {
        const nested = payload.error as ApiError;
        apiError = {
          code: nested.code || `HTTP_${response.status}`,
          message: nested.message || topMessage || fallbackMessage,
          details: nested.details,
        };
      } else {
        apiError = {
          code: `HTTP_${response.status}`,
          message:
            topMessage ||
            (typeof payload.error === 'string' ? payload.error : undefined) ||
            fallbackMessage,
          details: payload.details,
        };
      }
    } catch {
      apiError = { code: `HTTP_${response.status}`, message: fallbackMessage };
    }
    throw new ErpApiError(apiError);
  }

  return response.json() as Promise<ApiResponse<ImportResult>>;
}
