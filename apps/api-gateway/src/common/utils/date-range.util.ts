import { BadRequestException } from '@nestjs/common';

/** Default max span for ledger/journal-style transactional lists (days). */
export const DEFAULT_MAX_DATE_SPAN_DAYS = 366;

/**
 * Parse YYYY-MM-DD (or ISO) into a Date at UTC midnight for range compare.
 * Returns null when empty.
 */
export function parseDateInput(value?: string | null): Date | null {
  if (!value?.trim()) return null;
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) {
    throw new BadRequestException(`Tanggal tidak valid: ${value}`);
  }
  return d;
}

export interface DateRangeResult {
  from: Date | null;
  to: Date | null;
}

/**
 * Validate optional dateFrom/dateTo.
 * - When `requireRange` is true, both ends (or at least one bound covering a
 *   window) must be present.
 * - When both present, span must be ≤ maxSpanDays (inclusive calendar days).
 * - When only one bound present and requireRange is false, OK (open-ended).
 * - When requireRange is true and neither bound present, default last
 *   `defaultSpanDays` ending today (UTC date).
 */
export function resolveDateRange(opts: {
  dateFrom?: string;
  dateTo?: string;
  maxSpanDays?: number;
  requireRange?: boolean;
  defaultSpanDays?: number;
  fieldLabel?: string;
}): DateRangeResult {
  const maxSpan = opts.maxSpanDays ?? DEFAULT_MAX_DATE_SPAN_DAYS;
  const label = opts.fieldLabel ?? 'rentang tanggal';
  let from = parseDateInput(opts.dateFrom);
  let to = parseDateInput(opts.dateTo);

  if (opts.requireRange && !from && !to) {
    const end = new Date();
    end.setUTCHours(0, 0, 0, 0);
    const start = new Date(end);
    start.setUTCDate(start.getUTCDate() - (opts.defaultSpanDays ?? 31) + 1);
    from = start;
    to = end;
  }

  if (from && to && from.getTime() > to.getTime()) {
    throw new BadRequestException(
      `${label}: tanggal awal tidak boleh sesudah tanggal akhir`,
    );
  }

  if (from && to) {
    const ms = to.getTime() - from.getTime();
    const days = Math.floor(ms / (24 * 60 * 60 * 1000)) + 1;
    if (days > maxSpan) {
      throw new BadRequestException(
        `${label}: maksimal ${maxSpan} hari (diminta ~${days} hari). ` +
          `Persempit filter tanggal.`,
      );
    }
  }

  return { from, to };
}

/** Prisma-friendly DateTime filter fragment for a date column. */
export function prismaDateFilter(
  from: Date | null,
  to: Date | null,
): { gte?: Date; lte?: Date } | undefined {
  if (!from && !to) return undefined;
  return {
    ...(from ? { gte: from } : {}),
    ...(to ? { lte: to } : {}),
  };
}
