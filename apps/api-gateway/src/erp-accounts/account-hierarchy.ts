import { BadRequestException } from '@nestjs/common';
import { ErpAccountType, ErpNormalBalance } from '@prisma/client';
import type { AccountCodeFormat } from './account-code-format';

const TYPE_NORMAL_BALANCE_MAP: Record<ErpAccountType, ErpNormalBalance> = {
  ASSET: 'DEBIT',
  EXPENSE: 'DEBIT',
  LIABILITY: 'CREDIT',
  EQUITY: 'CREDIT',
  REVENUE: 'CREDIT',
};

export function normalBalanceForAccountType(type: ErpAccountType): ErpNormalBalance {
  return TYPE_NORMAL_BALANCE_MAP[type];
}

export function toBigIntId(value: string | bigint, label: string): bigint {
  if (typeof value === 'bigint') return value;
  const trimmed = value.trim();
  if (!/^\d+$/.test(trimmed)) {
    throw new BadRequestException(`${label} must be a numeric ID`);
  }
  return BigInt(trimmed);
}

export function toOptionalBigIntId(
  value: string | null | undefined,
  label: string,
): bigint | null | undefined {
  if (value === undefined) return undefined;
  if (value === null || value.trim() === '') return null;
  return toBigIntId(value, label);
}

export function isLeafAccountCode(code: string, format: AccountCodeFormat): boolean {
  const segments = splitAccountCode(code, format);
  const last = segments[segments.length - 1] ?? '';
  return /[1-9]/.test(last);
}

/** Strict CoA: leaf code → POSTABLE, non-leaf (trailing-zero last segment) → HEADER. */
export function accountKindFromCode(
  code: string,
  format: AccountCodeFormat,
): 'HEADER' | 'POSTABLE' {
  return isLeafAccountCode(code, format) ? 'POSTABLE' : 'HEADER';
}

function splitAccountCode(code: string, format: AccountCodeFormat): string[] {
  if (format.separator) return code.split(format.separator);

  let offset = 0;
  return format.segments.map((length) => {
    const part = code.slice(offset, offset + length);
    offset += length;
    return part;
  });
}
