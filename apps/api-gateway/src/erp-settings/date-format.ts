import { BadRequestException } from '@nestjs/common';

/**
 * Global date display format (sys_settings `system/format/date_format`).
 * Stored as a moment-style token string; the frontend converts it to a
 * date-fns pattern via the same preset table (lib/date-format.ts).
 *
 * Only a fixed preset set is allowed — arbitrary tokens are rejected so the
 * FE converter never sees a token it cannot map.
 */
export interface DateFormat {
  /** Moment-style token, e.g. "DD/MM/YYYY". This is what is persisted. */
  format: string;
  /** Rendered example for the picker date 2026-01-31. */
  example: string;
}

/** Reference date used to render the preview example: 31 Jan 2026. */
const SAMPLE = { d: 31, m: 1, y: 2026, monthLong: 'Januari', monthShort: 'Jan' };

interface Preset {
  format: string;
  build: () => string;
}

const PRESETS: Preset[] = [
  { format: 'DD/MM/YYYY', build: () => `${pad(SAMPLE.d)}/${pad(SAMPLE.m)}/${SAMPLE.y}` },
  { format: 'DD-MM-YYYY', build: () => `${pad(SAMPLE.d)}-${pad(SAMPLE.m)}-${SAMPLE.y}` },
  { format: 'MM/DD/YYYY', build: () => `${pad(SAMPLE.m)}/${pad(SAMPLE.d)}/${SAMPLE.y}` },
  { format: 'YYYY-MM-DD', build: () => `${SAMPLE.y}-${pad(SAMPLE.m)}-${pad(SAMPLE.d)}` },
  { format: 'DD MMMM YYYY', build: () => `${SAMPLE.d} ${SAMPLE.monthLong} ${SAMPLE.y}` },
  { format: 'D MMM YYYY', build: () => `${SAMPLE.d} ${SAMPLE.monthShort} ${SAMPLE.y}` },
];

const ALLOWED = new Map(PRESETS.map((p) => [p.format, p]));

function pad(n: number): string {
  return String(n).padStart(2, '0');
}

export function allowedDateFormats(): string[] {
  return PRESETS.map((p) => p.format);
}

export function parseDateFormatToken(raw: string | null | undefined): string {
  const token = (raw ?? 'DD/MM/YYYY').trim();
  if (!ALLOWED.has(token)) {
    throw new BadRequestException(
      `date_format must be one of: ${allowedDateFormats().join(', ')}`,
    );
  }
  return token;
}

export function buildDateFormat(raw: string | null | undefined): DateFormat {
  const format = parseDateFormatToken(raw);
  return { format, example: ALLOWED.get(format)!.build() };
}
