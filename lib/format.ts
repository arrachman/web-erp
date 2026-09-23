/**
 * Number & currency formatting helpers.
 * Single source of truth — import here, never use Intl.NumberFormat inline.
 *
 * Format separator + decimals dynamic via sys_settings group `number-format`
 * (lihat apps/web-erp/CLAUDE.md §2.31). Cache di module-level + hook.
 */

import * as React from 'react';
import { getNumberFormat, type NumberFormat } from '@/lib/api/number-format';

const DEFAULT_FORMAT: NumberFormat = {
  thousandsSep: '.',
  decimalSep: ',',
  decimals: 0,
  example: '1.234.567',
};

let cached: NumberFormat | null = null;
let inflight: Promise<NumberFormat> | null = null;
const listeners = new Set<(f: NumberFormat) => void>();

export function getNumberFormatSync(): NumberFormat {
  return cached ?? DEFAULT_FORMAT;
}

export async function loadNumberFormat(): Promise<NumberFormat> {
  if (cached) return cached;
  if (!inflight) {
    inflight = getNumberFormat()
      .then((f) => {
        cached = f;
        inflight = null;
        listeners.forEach((cb) => cb(f));
        return f;
      })
      .catch((err) => {
        inflight = null;
        console.warn('[format] failed to load number format, using default', err);
        cached = DEFAULT_FORMAT;
        listeners.forEach((cb) => cb(DEFAULT_FORMAT));
        return DEFAULT_FORMAT;
      });
  }
  return inflight;
}

export function invalidateNumberFormatCache(next?: NumberFormat) {
  if (next) {
    cached = next;
    listeners.forEach((cb) => cb(next));
  } else {
    cached = null;
  }
}

/** React hook — load once on mount, returns latest (cached or default). */
export function useNumberFormat(): NumberFormat {
  const [fmt, setFmt] = React.useState<NumberFormat>(() => cached ?? DEFAULT_FORMAT);
  React.useEffect(() => {
    if (!cached) void loadNumberFormat().then(setFmt);
    listeners.add(setFmt);
    return () => {
      listeners.delete(setFmt);
    };
  }, []);
  return fmt;
}

// ─── Pure format/parse helpers (sep injected, no cache lookup) ─────────────

/**
 * Format a raw numeric string ("12345.5" / "12345" / "") for display
 * with the given format. Empty/invalid → "" (so input field stays empty).
 *
 * - `decimals` (optional) — override default decimals from format.
 *   Pass 0 for integer qty, 2 for currency, undefined to use format default.
 */
export function formatRawForDisplay(
  raw: string,
  format: NumberFormat,
  decimals?: number,
): string {
  if (raw == null || raw === '' || raw === '-') return raw ?? '';
  // Defensive: callers may leak a number (e.g. Decimal field from API). Contract is string.
  if (typeof raw !== 'string') raw = String(raw);
  // Normalize: raw is canonical with '.' as decimal sep.
  const negative = raw.startsWith('-');
  const body = negative ? raw.slice(1) : raw;
  if (!/^\d*\.?\d*$/.test(body)) return raw;
  const [intRaw, fracRaw = ''] = body.split('.');
  const intDigits = intRaw.replace(/^0+(?=\d)/, '') || '0';
  const grouped = format.thousandsSep
    ? intDigits.replace(/\B(?=(\d{3})+(?!\d))/g, format.thousandsSep)
    : intDigits;
  const dec = decimals ?? format.decimals;
  const sign = negative ? '-' : '';
  // Integer field: never show decimal separator regardless of raw content.
  if (dec === 0) {
    return `${sign}${grouped}`;
  }
  // Decimal field: preserve mid-typing state (trailing sep, partial digits).
  const fracOut = fracRaw;
  if (fracOut === '' && !body.includes('.')) return `${sign}${grouped}`;
  return `${sign}${grouped}${format.decimalSep}${fracOut}`;
}

/**
 * Parse a display string ("12.345,5" / "12,345.5") back to raw canonical
 * ("12345.5"). Strips thousandsSep, replaces decimalSep with ".".
 * Returns "" for empty/invalid.
 */
export function parseDisplayToRaw(display: string, format: NumberFormat): string {
  if (display == null) return '';
  let s = String(display).trim();
  if (s === '' || s === '-') return s;
  const negative = s.startsWith('-');
  if (negative) s = s.slice(1);
  // Strip thousands sep (if non-empty).
  if (format.thousandsSep) {
    const re = new RegExp(escapeRegex(format.thousandsSep), 'g');
    s = s.replace(re, '');
  }
  // Replace decimal sep with '.'.
  if (format.decimalSep !== '.') {
    s = s.replace(format.decimalSep, '.');
  }
  // Drop anything that's not a digit or single dot.
  s = s.replace(/[^0-9.]/g, '');
  const firstDot = s.indexOf('.');
  if (firstDot >= 0) {
    s = s.slice(0, firstDot + 1) + s.slice(firstDot + 1).replace(/\./g, '');
  }
  return negative ? `-${s}` : s;
}

function escapeRegex(s: string): string {
  return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

/** Count digits in a string (for caret restore). */
export function countDigits(s: string, upTo?: number): number {
  const slice = upTo == null ? s : s.slice(0, upTo);
  let n = 0;
  for (let i = 0; i < slice.length; i++) {
    if (slice.charCodeAt(i) >= 48 && slice.charCodeAt(i) <= 57) n++;
  }
  return n;
}

/** Find offset in `s` after the n-th digit (1-indexed). */
export function offsetAfterNthDigit(s: string, n: number): number {
  if (n <= 0) return 0;
  let seen = 0;
  for (let i = 0; i < s.length; i++) {
    if (s.charCodeAt(i) >= 48 && s.charCodeAt(i) <= 57) {
      seen++;
      if (seen === n) return i + 1;
    }
  }
  return s.length;
}

// ─── Public formatters (cache-backed) ──────────────────────────────────────

/**
 * Format a number with thousands separator (dynamic from sys_settings).
 * decimals override → use given; else use sys_settings default.
 * e.g. (1500, 0) → "1.500"; (46666000, 2) → "46.666.000,00"
 */
export function formatNumber(
  value: number | string | null | undefined,
  decimals?: number,
): string {
  if (value == null || value === '') return '—';
  const fmt = getNumberFormatSync();
  const dec = decimals ?? fmt.decimals;
  const num = typeof value === 'string' ? Number(value) : value;
  if (!Number.isFinite(num)) return '—';
  // Use built-in fixed for decimals, then group manually.
  const fixed = num.toFixed(dec);
  const [intPart, fracPart] = fixed.split('.');
  const negative = intPart.startsWith('-');
  const intDigits = negative ? intPart.slice(1) : intPart;
  const grouped = fmt.thousandsSep
    ? intDigits.replace(/\B(?=(\d{3})+(?!\d))/g, fmt.thousandsSep)
    : intDigits;
  const sign = negative ? '-' : '';
  return fracPart
    ? `${sign}${grouped}${fmt.decimalSep}${fracPart}`
    : `${sign}${grouped}`;
}

/** Format as Rupiah (no symbol, fixed 2 decimals). e.g. 46666000 → "46.666.000,00" */
export function formatRupiah(value: number | null | undefined): string {
  return formatNumber(value, 2);
}

/** Format quantity (no decimals). e.g. 1500 → "1.500" */
export function formatQty(value: number | null | undefined): string {
  return formatNumber(value, 0);
}
