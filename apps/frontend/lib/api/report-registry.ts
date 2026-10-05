/**
 * Report registry API client (Wave G0/G1) — the combo-box feed + render
 * endpoint for converted .mrt reports. Render is a POST returning HTML
 * (preview) or a binary download (pdf/docx/xlsx), so it cannot use the
 * GET-only downloadFile helper; it goes through fetch + blob directly.
 */

import { apiGet, buildApiUrl, ErpApiError } from './client';

export interface RegistryReportItem {
  code: string;
  title: string;
  reportKey: string | null;
  legacyModule: string;
  erpModule: string | null;
  translationStatus: string;
  urutan: number;
}

export interface RegistryParamField {
  name: string;
  label: string;
  type: 'date' | 'number' | 'text';
}

export interface RegistryReportDetail extends RegistryReportItem {
  params: Array<{ name: string; label: string }>;
  paramSchema: RegistryParamField[];
  pageSetup: unknown;
  exportFormats: string[];
  hasTemplate: boolean;
  hasDataProvider: boolean;
}

export type RegistryExportFormat = 'html' | 'pdf' | 'docx' | 'xlsx';

export async function getRegistryReports(module: string): Promise<RegistryReportItem[]> {
  return apiGet<RegistryReportItem[]>(`/report-registry?module=${encodeURIComponent(module)}`);
}

export async function getRegistryReport(code: string): Promise<RegistryReportDetail> {
  return apiGet<RegistryReportDetail>(`/report-registry/${encodeURIComponent(code)}`);
}

export interface RenderResult {
  blob: Blob;
  contentType: string;
}

export async function renderRegistryReport(
  code: string,
  params: Record<string, unknown>,
  format: RegistryExportFormat,
  mode: 'layout' | 'data' = 'layout',
): Promise<RenderResult> {
  const res = await fetch(buildApiUrl(`/report-registry/${encodeURIComponent(code)}/render`), {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ params, format, mode }),
  });
  if (!res.ok) {
    let message = `Render gagal (${res.status})`;
    try {
      const body = (await res.json()) as { message?: string | string[] };
      if (body.message) message = Array.isArray(body.message) ? body.message.join('; ') : body.message;
    } catch {
      /* keep default message */
    }
    throw new ErpApiError({ code: `HTTP_${res.status}`, message });
  }
  return { blob: await res.blob(), contentType: res.headers.get('content-type') ?? '' };
}

export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  a.remove();
  setTimeout(() => URL.revokeObjectURL(url), 5000);
}
