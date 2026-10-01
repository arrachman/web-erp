/**
 * Shared cell-formatting for all three exporters (xlsx / pdf / docx) so every
 * format renders identical display strings. Alignment + numeric/date formatting
 * are driven purely by the column `type`, matching the on-screen table.
 */

import { ReportColType, ReportColumn, ReportSummaryItem } from './report-types';

const money2 = new Intl.NumberFormat('id-ID', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

const int0 = new Intl.NumberFormat('id-ID', {
  minimumFractionDigits: 0,
  maximumFractionDigits: 0,
});

const percent2 = new Intl.NumberFormat('id-ID', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
});

/** Right-aligned numeric types. */
export function isNumericType(type: ReportColType): boolean {
  return type === 'money' || type === 'qty' || type === 'number' || type === 'percent';
}

/** Resolve effective alignment from explicit override or column type. */
export function alignFor(col: Pick<ReportColumn, 'type' | 'align'>): 'left' | 'right' | 'center' {
  if (col.align) return col.align;
  return isNumericType(col.type) ? 'right' : 'left';
}

function toNumber(value: unknown): number | null {
  if (value === null || value === undefined || value === '') return null;
  const n = typeof value === 'number' ? value : Number(value);
  return Number.isFinite(n) ? n : null;
}

/** Format an ISO date string (or Date) to dd/MM/yyyy; passthrough if unparseable. */
function formatDate(value: unknown): string {
  if (value === null || value === undefined || value === '') return '';
  const d = value instanceof Date ? value : new Date(String(value));
  if (Number.isNaN(d.getTime())) return String(value);
  const dd = String(d.getUTCDate()).padStart(2, '0');
  const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = d.getUTCFullYear();
  return `${dd}/${mm}/${yyyy}`;
}

/** Format a generated-at timestamp to dd/MM/yyyy HH:mm. */
export function formatGeneratedAt(value: string): string {
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return value;
  const dd = String(d.getUTCDate()).padStart(2, '0');
  const mm = String(d.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = d.getUTCFullYear();
  const hh = String(d.getUTCHours()).padStart(2, '0');
  const mi = String(d.getUTCMinutes()).padStart(2, '0');
  return `${dd}/${mm}/${yyyy} ${hh}:${mi}`;
}

/** value + column type → display string, identical across exporters. */
export function formatCellValue(value: unknown, type: ReportColType): string {
  switch (type) {
    case 'money':
    case 'qty': {
      const n = toNumber(value);
      return n === null ? '' : money2.format(n);
    }
    case 'number': {
      const n = toNumber(value);
      return n === null ? '' : int0.format(n);
    }
    case 'percent': {
      const n = toNumber(value);
      return n === null ? '' : `${percent2.format(n)}%`;
    }
    case 'date':
      return formatDate(value);
    case 'status':
    case 'text':
    default:
      return value === null || value === undefined ? '' : String(value);
  }
}

/** Format one summary item to its display string. */
export function formatSummaryValue(item: ReportSummaryItem): string {
  if (item.type) return formatCellValue(item.value, item.type);
  return item.value === null || item.value === undefined ? '' : String(item.value);
}

/** Excel number-format strings for numeric column types (id-ID style with '.' grouping). */
export function excelNumFmt(type: ReportColType): string | null {
  switch (type) {
    case 'money':
    case 'qty':
      return '#,##0.00';
    case 'number':
      return '#,##0';
    case 'percent':
      return '#,##0.00"%"';
    default:
      return null;
  }
}

/** Build the export filename: `${key}-${YYYYMMDD}.${ext}`. */
export function buildFilename(key: string, generatedAt: string, ext: string): string {
  const d = new Date(generatedAt);
  const base = Number.isNaN(d.getTime()) ? new Date() : d;
  const yyyy = base.getUTCFullYear();
  const mm = String(base.getUTCMonth() + 1).padStart(2, '0');
  const dd = String(base.getUTCDate()).padStart(2, '0');
  return `${key}-${yyyy}${mm}${dd}.${ext}`;
}
