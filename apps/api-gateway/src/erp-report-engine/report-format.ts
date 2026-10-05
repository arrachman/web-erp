/**
 * Central report format settings (Wave G0, design D7) — replaces the legacy
 * m0_setting-backed helper datasets (formatNominal / formatMinus /
 * formatTgl / formatQty). Values live in sys_settings group 'company' with
 * keys report_format_*; ReportFormatService loads them, this module holds
 * the pure defaults + display formatting built on net-format.
 */

import { ID_SEPS, netFormat } from './net-format';

export interface ReportFormatSettings {
  nominalPattern: string;
  qtyPattern: string;
  nominalDecimals: number;
  qtyDecimals: number;
  negativeStyle: 'minus' | 'parentheses';
  groupSeparator: string;
  decimalSeparator: string;
  datePattern: string;
  currency: string;
}

export const DEFAULT_FORMAT_SETTINGS: ReportFormatSettings = {
  nominalPattern: '#,##0',
  qtyPattern: '#,##0.##',
  nominalDecimals: 0,
  qtyDecimals: 2,
  negativeStyle: 'parentheses',
  groupSeparator: '.',
  decimalSeparator: ',',
  datePattern: 'dd/MM/yyyy',
  currency: 'IDR',
};

export function parseFormatSettings(rows: Record<string, string>): ReportFormatSettings {
  const d = DEFAULT_FORMAT_SETTINGS;
  const decimals = (key: string, fallback: number) => {
    const n = Number(rows[key]);
    return Number.isFinite(n) && rows[key] !== undefined ? n : fallback;
  };
  const patternWithDecimals = (base: string, dec: number) =>
    dec > 0 ? `${base}.${'0'.repeat(dec)}` : base;
  return {
    nominalPattern: rows.report_format_nominal ?? patternWithDecimals('#,##0', decimals('report_format_nominal_decimals', d.nominalDecimals)),
    qtyPattern:
      rows.report_format_qty ??
      `#,##0${decimals('report_format_qty_decimals', d.qtyDecimals) > 0 ? '.' + '#'.repeat(decimals('report_format_qty_decimals', d.qtyDecimals)) : ''}`,
    nominalDecimals: decimals('report_format_nominal_decimals', d.nominalDecimals),
    qtyDecimals: decimals('report_format_qty_decimals', d.qtyDecimals),
    negativeStyle: rows.report_format_negative === 'minus' ? 'minus' : 'parentheses',
    groupSeparator: rows.report_format_group_separator ?? d.groupSeparator,
    decimalSeparator: rows.report_format_decimal_separator ?? d.decimalSeparator,
    datePattern: rows.report_format_date ?? d.datePattern,
    currency: rows.currency ?? d.currency,
  };
}

function applyNegative(formatted: string, value: number, s: ReportFormatSettings): string {
  if (value >= 0 || s.negativeStyle === 'minus') return formatted;
  return formatted.startsWith('-') ? `(${formatted.slice(1)})` : formatted;
}

/** Display-format a nominal amount using locale separators + negative style. */
export function formatNominalDisplay(value: number, s: ReportFormatSettings): string {
  const body = netFormat(Math.abs(value), s.nominalPattern, {
    group: s.groupSeparator,
    decimal: s.decimalSeparator,
  });
  if (value < 0) return s.negativeStyle === 'minus' ? `-${body}` : `(${body})`;
  return body;
}

export function formatQtyDisplay(value: number, s: ReportFormatSettings): string {
  const body = netFormat(Math.abs(value), s.qtyPattern, {
    group: s.groupSeparator,
    decimal: s.decimalSeparator,
  });
  if (value < 0) return s.negativeStyle === 'minus' ? `-${body}` : `(${body})`;
  return body;
}

export function formatDateDisplay(value: Date, s: ReportFormatSettings): string {
  return netFormat(value, s.datePattern, { group: s.groupSeparator, decimal: s.decimalSeparator });
}

/**
 * Pseudo-dataset rows emulating the legacy helper datasets the templates
 * reference (formatNominal.fromat — note legacy typo kept — .digitGrup,
 * formatMinus.kiri/kanan, formatTgl.format, formatQty). Injected into
 * ReportData.datasets by the render pipeline; lookup is case-insensitive.
 */
export function helperDatasets(s: ReportFormatSettings): Record<string, Array<Record<string, unknown>>> {
  const minus = s.negativeStyle === 'minus' ? { kiri: '-', kanan: '' } : { kiri: '(', kanan: ')' };
  return {
    formatNominal: [
      {
        fromat: s.nominalPattern,
        format: s.nominalPattern,
        digitGrup: s.groupSeparator,
        digitDesimal: s.decimalSeparator,
        digit: s.nominalDecimals,
      },
    ],
    formatQty: [
      {
        fromat: s.qtyPattern,
        format: s.qtyPattern,
        digitGrup: s.groupSeparator,
        digitDesimal: s.decimalSeparator,
        digit: s.qtyDecimals,
      },
    ],
    formatMinus: [minus],
    formatTgl: [{ format: s.datePattern, fromat: s.datePattern }],
  };
}
