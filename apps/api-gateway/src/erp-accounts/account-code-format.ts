import { BadRequestException } from '@nestjs/common';

export interface AccountCodeFormat {
  segments: number[];
  separator: string;
  pattern: RegExp;
  patternSource: string;
  maxLength: number;
  example: string;
}

const ALLOWED_SEPARATORS = new Set(['', '.', '-', '/']);
const MIN_SEGMENT = 1;
const MAX_SEGMENT = 12;
const MAX_SEGMENTS_COUNT = 5;

export function parseSegments(raw: string | null | undefined): number[] {
  if (!raw) return [4, 2, 3];
  let parsed: unknown;
  try {
    parsed = JSON.parse(raw);
  } catch {
    throw new BadRequestException(`account_code_segments invalid JSON: ${raw}`);
  }
  if (!Array.isArray(parsed) || parsed.length === 0) {
    throw new BadRequestException(
      'account_code_segments must be a non-empty JSON array of integers',
    );
  }
  if (parsed.length > MAX_SEGMENTS_COUNT) {
    throw new BadRequestException(`account_code_segments max ${MAX_SEGMENTS_COUNT} segments`);
  }
  const segments = parsed.map((n, i) => {
    if (typeof n !== 'number' || !Number.isInteger(n)) {
      throw new BadRequestException(`account_code_segments[${i}] must be integer`);
    }
    if (n < MIN_SEGMENT || n > MAX_SEGMENT) {
      throw new BadRequestException(
        `account_code_segments[${i}] must be ${MIN_SEGMENT}–${MAX_SEGMENT}`,
      );
    }
    return n;
  });
  return segments;
}

export function parseSeparator(raw: string | null | undefined): string {
  const sep = raw ?? '.';
  if (!ALLOWED_SEPARATORS.has(sep)) {
    throw new BadRequestException(
      `account_code_separator must be one of: "" (empty), ".", "-", "/"`,
    );
  }
  return sep;
}

export function buildAccountCodeFormat(segments: number[], separator: string): AccountCodeFormat {
  const escSep = separator.replace(/[.\\/$^?*+(){}[\]|]/g, '\\$&');
  const parts = segments.map((n) => `\\d{${n}}`);
  const patternSource = `^${parts.join(escSep)}$`;
  const pattern = new RegExp(patternSource);

  const sumDigits = segments.reduce((a, b) => a + b, 0);
  const sepCount = segments.length - 1;
  const maxLength = sumDigits + sepCount * separator.length;

  const example = segments
    .map((n, i) => (i === 0 ? '1'.padEnd(n, '1') : '0'.padStart(n, '0')))
    .join(separator);

  return { segments, separator, pattern, patternSource, maxLength, example };
}

export function validateAccountCode(code: string, format: AccountCodeFormat): void {
  if (typeof code !== 'string' || code.length === 0) {
    throw new BadRequestException('code is required');
  }
  if (code.length > format.maxLength) {
    throw new BadRequestException(`code exceeds max length ${format.maxLength}`);
  }
  if (!format.pattern.test(code)) {
    const layout = format.segments.join(format.separator || ' ');
    throw new BadRequestException(`code must match format ${layout} (e.g., ${format.example})`);
  }
}
