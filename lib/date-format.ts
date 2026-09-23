/**
 * Date display formatting — single source of truth.
 * Format token (moment-style) is dynamic via sys_settings `system/format/date_format`
 * (lihat apps/web-erp/CLAUDE.md §2.31). Cache di module-level + hook, sejajar
 * dengan lib/format.ts untuk angka.
 *
 * Selalu pakai `formatDate()` untuk display tanggal — jangan `new Date().toLocale*`
 * inline atau native <input type="date">.
 */

import * as React from 'react';
import { format as fnsFormat, parse as fnsParse, isValid } from 'date-fns';
import { id as idLocale } from 'date-fns/locale';
import { getDateFormat, type DateFormat } from '@/lib/api/date-format';

const DEFAULT_TOKEN = 'DD/MM/YYYY';
const DEFAULT_FORMAT: DateFormat = { format: DEFAULT_TOKEN, example: '31/01/2026' };

/**
 * Convert a moment-style token (persisted in settings) to a date-fns pattern.
 * Only the fixed preset set is supported (validated backend-side).
 */
export function tokenToPattern(token: string): string {
  return token
    .replace(/YYYY/g, 'yyyy')
    .replace(/DD/g, 'dd')
    .replace(/\bD\b/g, 'd');
  // MM / MMM / MMMM are identical between moment and date-fns.
}

// ─── Cache + listeners (mirror lib/format.ts) ─────────────────────────────

let cached: DateFormat | null = null;
let inflight: Promise<DateFormat> | null = null;
const listeners = new Set<(f: DateFormat) => void>();

export function getDateFormatSync(): DateFormat {
  return cached ?? DEFAULT_FORMAT;
}

export async function loadDateFormat(): Promise<DateFormat> {
  if (cached) return cached;
  if (!inflight) {
    inflight = getDateFormat()
      .then((f) => {
        cached = f;
        inflight = null;
        listeners.forEach((cb) => cb(f));
        return f;
      })
      .catch((err) => {
        inflight = null;
        console.warn('[date-format] failed to load, using default', err);
        cached = DEFAULT_FORMAT;
        listeners.forEach((cb) => cb(DEFAULT_FORMAT));
        return DEFAULT_FORMAT;
      });
  }
  return inflight;
}

export function invalidateDateFormatCache(next?: DateFormat) {
  if (next) {
    cached = next;
    listeners.forEach((cb) => cb(next));
  } else {
    cached = null;
  }
}

/** React hook — load once on mount, returns latest (cached or default). */
export function useDateFormat(): DateFormat {
  const [fmt, setFmt] = React.useState<DateFormat>(() => cached ?? DEFAULT_FORMAT);
  React.useEffect(() => {
    if (!cached) void loadDateFormat().then(setFmt);
    listeners.add(setFmt);
    return () => {
      listeners.delete(setFmt);
    };
  }, []);
  return fmt;
}

// ─── Pure helpers ─────────────────────────────────────────────────────────

/** Parse an ISO date string (YYYY-MM-DD or full ISO) to a Date, or undefined. */
export function parseIsoDate(iso: string | null | undefined): Date | undefined {
  if (!iso) return undefined;
  // Date-only strings parse as UTC midnight in JS; force local to avoid off-by-one.
  const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso);
  if (m) {
    const d = new Date(Number(m[1]), Number(m[2]) - 1, Number(m[3]));
    return isValid(d) ? d : undefined;
  }
  const d = new Date(iso);
  return isValid(d) ? d : undefined;
}

/** Date → canonical ISO date string (YYYY-MM-DD) for storing in form state. */
export function toIsoDate(d: Date | undefined): string {
  return d ? fnsFormat(d, 'yyyy-MM-dd') : '';
}

/**
 * Format an ISO date string for display using the given (or current) format.
 * Empty/invalid input → "".
 */
export function formatDate(iso: string | null | undefined, fmt?: DateFormat): string {
  const date = parseIsoDate(iso);
  if (!date) return '';
  const token = (fmt ?? getDateFormatSync()).format;
  try {
    return fnsFormat(date, tokenToPattern(token), { locale: idLocale });
  } catch {
    return fnsFormat(date, 'dd/MM/yyyy', { locale: idLocale });
  }
}

/**
 * Parse user-typed date text → canonical ISO date string (YYYY-MM-DD).
 * Tries the active display format first, then a set of common fallbacks so
 * the user can type liberally (e.g. `5/5/2026`, `05-05-2026`, `2026-05-05`).
 * Returns `''` for empty input, or `null` when the text cannot be parsed.
 */
export function parseDisplayDate(text: string, fmt?: DateFormat): string | null {
  const trimmed = text.trim();
  if (!trimmed) return '';
  const token = (fmt ?? getDateFormatSync()).format;
  const patterns = [
    tokenToPattern(token),
    'dd/MM/yyyy',
    'd/M/yyyy',
    'dd-MM-yyyy',
    'd-M-yyyy',
    'yyyy-MM-dd',
    'd MMM yyyy',
    'd MMMM yyyy',
  ];
  for (const pattern of patterns) {
    const date = fnsParse(trimmed, pattern, new Date(), { locale: idLocale });
    if (isValid(date)) return toIsoDate(date);
  }
  return null;
}

/**
 * Navigation bounds for the day-picker month/year dropdowns
 * (`captionLayout="dropdown"`). Wide enough to cover historical records
 * (birth dates, legacy transactions back to the early 1900s) and a few years
 * ahead. End year is dynamic relative to "now" so it never goes stale.
 */
export function calendarNavBounds(): { startMonth: Date; endMonth: Date } {
  const year = new Date().getFullYear();
  return {
    startMonth: new Date(1920, 0, 1),
    endMonth: new Date(year + 10, 11, 31),
  };
}

export type { DateFormat };
