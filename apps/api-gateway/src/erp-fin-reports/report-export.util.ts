import { ReportColumn } from './report-types';

/** Format a number with id-ID thousands separators and 2 decimals. */
export function formatNumber(value: number): string {
  return new Intl.NumberFormat('id-ID', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

/** Format a cell value for textual renderers (pdf/doc). */
export function formatCell(col: ReportColumn, value: string | number | null): string {
  if (value === null || value === undefined) return '';
  if (col.type === 'number') {
    return typeof value === 'number' ? formatNumber(value) : String(value);
  }
  return String(value);
}
