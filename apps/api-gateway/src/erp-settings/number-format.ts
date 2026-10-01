import { BadRequestException } from '@nestjs/common';

export interface NumberFormat {
  thousandsSep: string;
  decimalSep: string;
  decimals: number;
  example: string;
}

const ALLOWED_THOUSANDS = new Set(['', '.', ',', ' ', "'"]);
const ALLOWED_DECIMAL = new Set(['.', ',']);
const MIN_DECIMALS = 0;
const MAX_DECIMALS = 6;

export function parseThousandsSep(raw: string | null | undefined): string {
  const sep = raw ?? '.';
  if (!ALLOWED_THOUSANDS.has(sep)) {
    throw new BadRequestException(
      `number_thousands_sep must be one of: "" (empty), ".", ",", " " (space), "'"`,
    );
  }
  return sep;
}

export function parseDecimalSep(raw: string | null | undefined): string {
  const sep = raw ?? ',';
  if (!ALLOWED_DECIMAL.has(sep)) {
    throw new BadRequestException(`number_decimal_sep must be one of: ".", ","`);
  }
  return sep;
}

export function parseDecimals(raw: string | null | undefined): number {
  const v = raw ?? '0';
  const n = Number(v);
  if (!Number.isInteger(n) || n < MIN_DECIMALS || n > MAX_DECIMALS) {
    throw new BadRequestException(
      `number_decimals must be integer ${MIN_DECIMALS}–${MAX_DECIMALS}`,
    );
  }
  return n;
}

export function assertCompatible(thousandsSep: string, decimalSep: string): void {
  if (thousandsSep && thousandsSep === decimalSep) {
    throw new BadRequestException(
      'number_thousands_sep dan number_decimal_sep tidak boleh sama',
    );
  }
}

export function buildNumberFormat(
  thousandsSep: string,
  decimalSep: string,
  decimals: number,
): NumberFormat {
  assertCompatible(thousandsSep, decimalSep);
  const intPart = '1234567';
  const grouped = thousandsSep
    ? intPart.replace(/\B(?=(\d{3})+(?!\d))/g, thousandsSep)
    : intPart;
  const example =
    decimals > 0 ? `${grouped}${decimalSep}${'8'.repeat(decimals)}` : grouped;
  return { thousandsSep, decimalSep, decimals, example };
}
