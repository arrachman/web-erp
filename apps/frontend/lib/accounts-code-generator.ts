/**
 * Client-side next-code suggestion for Chart of Accounts (md_accounts).
 *
 * When inserting a new account under a parent, continue the sibling sequence
 * (increment by inferred step) or start from 1 if none. Parent structure is inherited.
 *
 * Not transactional — race if two users generate at once; backend unique
 * constraint still applies (same pattern as items-code-generator).
 */

import { listAccounts, type AccountCodeFormat } from '@/lib/api/accounts';

export function splitAccountCode(code: string, format: AccountCodeFormat): string[] {
  if (!format.separator) {
    let offset = 0;
    return format.segments.map((length) => {
      const part = code.slice(offset, offset + length);
      offset += length;
      return part;
    });
  }
  return code.split(format.separator);
}

export function joinAccountCode(parts: string[], format: AccountCodeFormat): string {
  return parts.join(format.separator);
}

function padSegment(n: number, length: number): string {
  return String(n).padStart(length, '0');
}

/** First root segment style: 1 + trailing zeros (PSAK `1000`, not `0001`). */
function firstRootSegment(length: number): string {
  if (length <= 0) return '';
  return `1${'0'.repeat(length - 1)}`;
}

function lastNonZeroIndex(parts: string[]): number {
  for (let i = parts.length - 1; i >= 0; i -= 1) {
    if (!/^0+$/.test(parts[i] ?? '')) return i;
  }
  return 0;
}

function gcd(a: number, b: number): number {
  let x = Math.abs(a);
  let y = Math.abs(b);
  while (y !== 0) {
    const t = y;
    y = x % y;
    x = t;
  }
  return x;
}

/**
 * Infer increment step from existing sibling values at the sequence segment.
 * Multi: GCD of consecutive diffs. Single: 10^k from trailing zeros (1100→100).
 */
export function inferSequenceStep(values: number[]): number {
  const sorted = [...new Set(values.filter((n) => Number.isFinite(n)))].sort((a, b) => a - b);
  if (sorted.length >= 2) {
    let g = 0;
    for (let i = 1; i < sorted.length; i += 1) {
      const d = sorted[i]! - sorted[i - 1]!;
      if (d <= 0) continue;
      g = g === 0 ? d : gcd(g, d);
    }
    return Math.max(1, g || 1);
  }
  if (sorted.length === 1) {
    const v = sorted[0]!;
    if (v <= 0) return 1;
    let step = 1;
    while (v % (step * 10) === 0 && step * 10 <= v) {
      step *= 10;
    }
    return step;
  }
  return 1;
}

/**
 * Increment the rightmost segment (with carry), keeping zero-padded width.
 * Used as overflow fallback when sequence segment is full.
 */
export function incrementAccountCode(code: string, format: AccountCodeFormat): string {
  const parts = splitAccountCode(code, format);
  for (let i = parts.length - 1; i >= 0; i -= 1) {
    const width = format.segments[i] ?? 0;
    const max = 10 ** width - 1;
    const next = parseInt(parts[i] ?? '0', 10) + 1;
    if (Number.isFinite(next) && next <= max) {
      parts[i] = padSegment(next, width);
      return joinAccountCode(parts, format);
    }
    parts[i] = '0'.repeat(width);
  }
  return joinAccountCode(parts, format);
}

/**
 * Suggest the first child code under a parent (no siblings yet).
 * Sets the first free segment after the parent's last non-zero to 1.
 * Root with no siblings → `1000…` style first segment + zeros.
 */
export function firstChildAccountCode(
  parentCode: string | null | undefined,
  format: AccountCodeFormat,
): string {
  if (!parentCode) {
    return joinAccountCode(
      format.segments.map((n, i) => (i === 0 ? firstRootSegment(n) : '0'.repeat(n))),
      format,
    );
  }

  const parentParts = splitAccountCode(parentCode, format);
  if (parentParts.length !== format.segments.length) {
    return firstChildAccountCode(null, format);
  }

  const pivot = lastNonZeroIndex(parentParts);
  const child = [...parentParts];
  if (pivot < format.segments.length - 1) {
    const seqIdx = pivot + 1;
    child[seqIdx] = padSegment(1, format.segments[seqIdx] ?? 1);
    for (let j = seqIdx + 1; j < format.segments.length; j += 1) {
      child[j] = '0'.repeat(format.segments[j] ?? 0);
    }
    return joinAccountCode(child, format);
  }

  return incrementAccountCode(parentCode, format);
}

/**
 * Among siblings, pick the sequence segment (first that differs from parent,
 * else first that varies across siblings, else last) and return max+step there,
 * keeping the trailing structure of the max sibling.
 */
export function suggestNextAccountCode(
  parentCode: string | null | undefined,
  siblingCodes: string[],
  format: AccountCodeFormat,
): string {
  const valid = siblingCodes.filter((c) => {
    try {
      return splitAccountCode(c, format).length === format.segments.length;
    } catch {
      return false;
    }
  });

  if (valid.length === 0) {
    return firstChildAccountCode(parentCode, format);
  }

  const parentParts = parentCode ? splitAccountCode(parentCode, format) : null;
  const siblingParts = valid.map((c) => splitAccountCode(c, format));

  let seqIdx = format.segments.length - 1;
  if (parentParts && parentParts.length === format.segments.length) {
    for (let i = 0; i < format.segments.length; i += 1) {
      if (siblingParts.some((sp) => sp[i] !== parentParts[i])) {
        seqIdx = i;
        break;
      }
    }
  } else {
    for (let i = 0; i < format.segments.length; i += 1) {
      const values = new Set(siblingParts.map((sp) => sp[i]));
      if (values.size > 1) {
        seqIdx = i;
        break;
      }
      if (i === 0) seqIdx = 0;
    }
  }

  const seqValues: number[] = [];
  let maxSeq = -1;
  let maxParts = siblingParts[0]!;
  for (const sp of siblingParts) {
    const n = parseInt(sp[seqIdx] ?? '0', 10);
    if (!Number.isFinite(n)) continue;
    seqValues.push(n);
    if (n >= maxSeq) {
      maxSeq = n;
      maxParts = sp;
    }
  }

  const width = format.segments[seqIdx] ?? 1;
  const maxVal = 10 ** width - 1;
  const step = inferSequenceStep(seqValues);
  const next = maxSeq + step;
  if (next > maxVal) {
    return incrementAccountCode(joinAccountCode(maxParts, format), format);
  }

  const result = [...maxParts];
  result[seqIdx] = padSegment(next, width);
  return joinAccountCode(result, format);
}

/**
 * Load direct children of parent (or root when parentId empty) and suggest next code.
 */
export async function generateNextAccountCode(
  parentId: string | null | undefined,
  parentCode: string | null | undefined,
  format: AccountCodeFormat,
): Promise<string> {
  try {
    const res = await listAccounts({
      page: 1,
      limit: 100,
      sortBy: 'code',
      sortDir: 'desc',
      parentId: parentId ? parentId : 'null',
    });
    const siblingCodes = res.data.map((a) => a.code);
    return suggestNextAccountCode(parentCode ?? null, siblingCodes, format);
  } catch {
    return firstChildAccountCode(parentCode ?? null, format);
  }
}
