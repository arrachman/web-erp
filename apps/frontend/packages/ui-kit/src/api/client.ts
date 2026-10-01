// Shared HTTP client factory for every Senti product frontend (ERP, MDP, HR, …).
//
// Behaviour is identical across products; only the base URL differs, so each
// app calls `createApiClient({ baseUrl })` once and re-exports the verbs.
//   - Cookies (auth token) are sent automatically via credentials: 'include'.
//   - Uniform error envelope decoded into SentiApiError { code, message, details }.
//   - 204 No Content → undefined.
//   - BigInt IDs are expected as strings (backend serialises BigInt → string).

import type { ApiError } from './types';

// ─── Error ────────────────────────────────────────────────────────────────────

export class SentiApiError extends Error {
  readonly code: string;
  readonly details?: unknown;

  constructor(error: ApiError) {
    super(error.message);
    this.name = 'SentiApiError';
    this.code = error.code;
    this.details = error.details;
  }
}

// ─── Config ───────────────────────────────────────────────────────────────────

export interface ApiClientConfig {
  /** Absolute base URL or same-origin prefix, e.g. `/api/erp` or `https://…/api/erp`. */
  baseUrl: string;
}

export interface ApiClient {
  apiGet: <T>(
    path: string,
    params?: Record<string, string | number | boolean | undefined>,
  ) => Promise<T>;
  apiPost: <T>(path: string, body?: unknown) => Promise<T>;
  apiPatch: <T>(path: string, body?: unknown) => Promise<T>;
  apiPut: <T>(path: string, body?: unknown) => Promise<T>;
  apiDelete: <T>(path: string, body?: unknown) => Promise<T>;
  apiUpload: <T>(path: string, form: FormData) => Promise<T>;
  downloadFile: (
    path: string,
    query: Record<string, string | number | undefined> | undefined,
    fallbackName: string,
  ) => Promise<void>;
  buildApiUrl: (
    path: string,
    query?: Record<string, string | number | undefined>,
  ) => string;
}

interface RequestOptions {
  method: string;
  path: string;
  body?: unknown;
  params?: Record<string, string | number | boolean | undefined>;
}

// ─── Factory ──────────────────────────────────────────────────────────────────

export function createApiClient(config: ApiClientConfig): ApiClient {
  const { baseUrl } = config;

  async function request<T>(options: RequestOptions): Promise<T> {
    const { method, path, body, params } = options;

    let url = `${baseUrl}${path}`;

    if (params) {
      const searchParams = new URLSearchParams();
      for (const [key, value] of Object.entries(params)) {
        if (value !== undefined && value !== null) {
          searchParams.set(key, String(value));
        }
      }
      const qs = searchParams.toString();
      if (qs) url = `${url}?${qs}`;
    }

    const init: RequestInit = {
      method,
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
    };

    if (body !== undefined) {
      init.body = JSON.stringify(body);
    }

    const response = await fetch(url, init);

    if (!response.ok) {
      throw await toApiError(response, `Request gagal (HTTP ${response.status})`);
    }

    // 204 No Content — return undefined cast as T
    if (response.status === 204) {
      return undefined as T;
    }

    return response.json() as Promise<T>;
  }

  function buildApiUrl(
    path: string,
    query?: Record<string, string | number | undefined>,
  ): string {
    let url = `${baseUrl}${path}`;
    if (query) {
      const searchParams = new URLSearchParams();
      for (const [key, value] of Object.entries(query)) {
        if (value !== undefined && value !== null) {
          searchParams.set(key, String(value));
        }
      }
      const qs = searchParams.toString();
      if (qs) url = `${url}?${qs}`;
    }
    return url;
  }

  /**
   * Fetch a streamed file (with cookie auth) and trigger a browser download.
   * Uses the server-provided filename from Content-Disposition when present,
   * else `fallbackName`. Throws SentiApiError on non-ok responses.
   */
  async function downloadFile(
    path: string,
    query: Record<string, string | number | undefined> | undefined,
    fallbackName: string,
  ): Promise<void> {
    const url = buildApiUrl(path, query);
    const response = await fetch(url, { credentials: 'include' });

    if (!response.ok) {
      throw await toApiError(response, `Unduhan gagal (HTTP ${response.status})`);
    }

    const blob = await response.blob();
    const name =
      filenameFromDisposition(response.headers.get('content-disposition')) ??
      fallbackName;

    const objectUrl = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = objectUrl;
    anchor.download = name;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(objectUrl);
  }

  /**
   * POST multipart/form-data (file upload). Content-Type is set by the browser
   * (with boundary) — never set it manually. Same error envelope as `request`.
   */
  async function apiUpload<T>(path: string, form: FormData): Promise<T> {
    const response = await fetch(buildApiUrl(path), {
      method: 'POST',
      credentials: 'include',
      body: form,
    });

    if (!response.ok) {
      throw await toApiError(response, `Unggahan gagal (HTTP ${response.status})`);
    }

    return response.json() as Promise<T>;
  }

  return {
    apiGet: <T>(
      path: string,
      params?: Record<string, string | number | boolean | undefined>,
    ) => request<T>({ method: 'GET', path, params }),
    apiPost: <T>(path: string, body?: unknown) =>
      request<T>({ method: 'POST', path, body }),
    apiPatch: <T>(path: string, body?: unknown) =>
      request<T>({ method: 'PATCH', path, body }),
    apiPut: <T>(path: string, body?: unknown) =>
      request<T>({ method: 'PUT', path, body }),
    apiDelete: <T>(path: string, body?: unknown) =>
      request<T>({ method: 'DELETE', path, body }),
    apiUpload,
    downloadFile,
    buildApiUrl,
  };
}

// ─── Internal helpers ─────────────────────────────────────────────────────────

/**
 * Decode a failed Response into a SentiApiError.
 *
 * Supports both envelopes used across Senti products:
 * 1. Nest AllExceptionsFilter:
 *    { success: false, statusCode, error: string, message: string|string[], details? }
 * 2. Nested ApiError object:
 *    { error: { code, message, details? } }  or  { code, message }
 *
 * Prefer the human-readable `message` over HTTP status text ("Bad Request").
 */
async function toApiError(
  response: Response,
  fallbackMessage: string,
): Promise<SentiApiError> {
  const statusFallback = response.statusText || fallbackMessage;
  let apiError: ApiError;
  try {
    const payload = (await response.json()) as Record<string, unknown>;
    apiError = parseErrorPayload(payload, response.status, statusFallback);
  } catch {
    apiError = { code: `HTTP_${response.status}`, message: statusFallback };
  }
  return new SentiApiError(apiError);
}

/** Normalise Nest / nested / flat error JSON into { code, message, details? }. */
function parseErrorPayload(
  payload: Record<string, unknown>,
  status: number,
  statusFallback: string,
): ApiError {
  const topMessage = normaliseMessage(payload.message);
  const topCode =
    (typeof payload.code === 'string' && payload.code) ||
    (typeof payload.statusCode === 'number' && `HTTP_${payload.statusCode}`) ||
    `HTTP_${status}`;
  const topDetails = payload.details;

  // Nested: { error: { code, message, details? } }
  if (payload.error && typeof payload.error === 'object') {
    const nested = payload.error as Record<string, unknown>;
    const nestedMessage = normaliseMessage(nested.message);
    return {
      code:
        (typeof nested.code === 'string' && nested.code) ||
        topCode,
      message: nestedMessage || topMessage || statusFallback,
      details: nested.details ?? topDetails,
    };
  }

  // Nest filter: { error: "Bad Request", message: "Cannot delete…" }
  if (typeof payload.error === 'string') {
    return {
      code: topCode,
      message: topMessage || payload.error || statusFallback,
      details: topDetails,
    };
  }

  // Flat: { code?, message?, details? } or empty body
  return {
    code: topCode,
    message: topMessage || statusFallback,
    details: topDetails,
  };
}

/** class-validator may return message as string[]. */
function normaliseMessage(value: unknown): string | undefined {
  if (typeof value === 'string') {
    const trimmed = value.trim();
    return trimmed || undefined;
  }
  if (Array.isArray(value)) {
    const parts = value
      .map((v) => (typeof v === 'string' ? v.trim() : ''))
      .filter(Boolean);
    return parts.length ? parts.join('; ') : undefined;
  }
  return undefined;
}

/** Extract a filename from a Content-Disposition header, if present. */
function filenameFromDisposition(header: string | null): string | null {
  if (!header) return null;
  const star = /filename\*=(?:UTF-8'')?["']?([^"';]+)/i.exec(header);
  if (star?.[1]) return decodeURIComponent(star[1]);
  const plain = /filename=["']?([^"';]+)/i.exec(header);
  return plain?.[1] ?? null;
}
