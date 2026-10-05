/**
 * .NET-style custom format patterns ("#,##0.00", "dd MMMM yyyy", "N2",
 * "C0", "P1") as used by Stimulsoft's Format(pattern, value) and by
 * TextFormat on .mrt components. Pure functions, id-ID month/day names.
 *
 * Two separator conventions exist in the corpus:
 *  - Format() inside .mrt expressions runs with invariant separators
 *    (group ',', decimal '.') because templates then run a Replace-chain
 *    converting to digitGrup/digitDesimal from the format helper dataset.
 *  - Direct display formatting (component TextFormat, engine defaults)
 *    uses the configured locale separators (id-ID: '.' group, ',' decimal).
 * Callers pick via FormatSeparators.
 */

export interface FormatSeparators {
  group: string;
  decimal: string;
}

export const INVARIANT_SEPS: FormatSeparators = { group: ',', decimal: '.' };
export const ID_SEPS: FormatSeparators = { group: '.', decimal: ',' };

const MONTHS_ID = [
  'Januari', 'Februari', 'Maret', 'April', 'Mei', 'Juni',
  'Juli', 'Agustus', 'September', 'Oktober', 'November', 'Desember',
];
const MONTHS_ID_SHORT = MONTHS_ID.map((m) => m.slice(0, 3));
const DAYS_ID = ['Minggu', 'Senin', 'Selasa', 'Rabu', 'Kamis', 'Jumat', 'Sabtu'];
const DAYS_ID_SHORT = ['Min', 'Sen', 'Sel', 'Rab', 'Kam', 'Jum', 'Sab'];

const pad = (n: number, w = 2) => String(Math.abs(Math.trunc(n))).padStart(w, '0');

/** Expand standard .NET format strings (N2, C, C0, P1, F2, D4, X) to custom patterns. */
export function expandStandardFormat(pattern: string): string | null {
  const m = /^([a-zA-Z])(\d{0,2})$/.exec(pattern.trim());
  if (!m) return null;
  const kind = m[1].toUpperCase();
  const digits = m[2] === '' ? 2 : Number(m[2]);
  switch (kind) {
    case 'N':
      return `#,##0${digits > 0 ? '.' + '0'.repeat(digits) : ''}`;
    case 'F':
      return `0${digits > 0 ? '.' + '0'.repeat(digits) : ''}`;
    case 'C':
      return `#,##0${digits > 0 ? '.' + '0'.repeat(digits) : ''}`;
    case 'P':
      return `#,##0${digits > 0 ? '.' + '0'.repeat(digits) : ''}%`;
    case 'D':
      return 'D';
    default:
      return null;
  }
}

function isDatePattern(pattern: string): boolean {
  return /[dMy]/.test(pattern.replace(/'[^']*'/g, '')) && !/[#0]/.test(pattern.replace(/'[^']*'/g, ''));
}

export function formatDatePattern(d: Date, pattern: string): string {
  const tokens: Array<[string, string]> = [
    ['dddd', DAYS_ID[d.getDay()]],
    ['ddd', DAYS_ID_SHORT[d.getDay()]],
    ['dd', pad(d.getDate())],
    ['d', String(d.getDate())],
    ['MMMM', MONTHS_ID[d.getMonth()]],
    ['MMM', MONTHS_ID_SHORT[d.getMonth()]],
    ['MM', pad(d.getMonth() + 1)],
    ['M', String(d.getMonth() + 1)],
    ['yyyy', String(d.getFullYear())],
    ['yy', pad(d.getFullYear() % 100)],
    ['HH', pad(d.getHours())],
    ['H', String(d.getHours())],
    ['hh', pad(((d.getHours() + 11) % 12) + 1)],
    ['h', String(((d.getHours() + 11) % 12) + 1)],
    ['mm', pad(d.getMinutes())],
    ['m', String(d.getMinutes())],
    ['ss', pad(d.getSeconds())],
    ['s', String(d.getSeconds())],
    ['tt', d.getHours() < 12 ? 'AM' : 'PM'],
  ];
  let out = '';
  let i = 0;
  while (i < pattern.length) {
    const ch = pattern[i];
    if (ch === "'" || ch === '"') {
      const end = pattern.indexOf(ch, i + 1);
      out += end === -1 ? pattern.slice(i + 1) : pattern.slice(i + 1, end);
      i = end === -1 ? pattern.length : end + 1;
      continue;
    }
    if (ch === '\\') {
      out += pattern[i + 1] ?? '';
      i += 2;
      continue;
    }
    const hit = tokens.find(([t]) => pattern.startsWith(t, i));
    if (hit) {
      out += hit[1];
      i += hit[0].length;
    } else {
      out += ch;
      i += 1;
    }
  }
  return out;
}

function groupThousands(intPart: string, sep: string): string {
  if (!sep) return intPart;
  return intPart.replace(/\B(?=(\d{3})+(?!\d))/g, sep);
}

/** Format a number against a .NET custom numeric pattern (#/0 placeholders). */
export function formatNumberPattern(value: number, pattern: string, seps: FormatSeparators): string {
  let p = pattern;
  if (!/[#0]/.test(p)) return String(value);
  const percent = p.includes('%');
  let v = value;
  if (percent) v = v * 100;
  const negative = v < 0;
  const sections = p.split(';');
  let section = sections[0];
  if (negative && sections.length > 1) section = sections[1];
  if (v === 0 && sections.length > 2) section = sections[2];
  const dotIdx = section.indexOf('.');
  const intPat = (dotIdx === -1 ? section : section.slice(0, dotIdx)).replace(/%/g, '');
  const fracPat = dotIdx === -1 ? '' : section.slice(dotIdx + 1).replace(/%/g, '');
  const fracLen = (fracPat.match(/[0#]/g) ?? []).length;
  const factor = 10 ** fracLen;
  const rounded = Math.round(Math.abs(v) * factor) / factor;
  let intStr = String(Math.trunc(rounded));
  const minIntDigits = (intPat.match(/0/g) ?? []).length;
  intStr = intStr.padStart(Math.max(1, minIntDigits), '0');
  if (intPat.includes(',')) intStr = groupThousands(intStr, seps.group);
  let fracStr = '';
  if (fracLen > 0) {
    const fracRaw = pad(Math.round((rounded - Math.trunc(rounded)) * factor), fracLen);
    // '#' placeholders drop trailing zeros; '0' placeholders keep them.
    let keep = fracRaw.length;
    for (let i = fracRaw.length - 1; i >= 0; i--) {
      if (fracPat[i] === '#' && fracRaw[i] === '0') keep = i;
      else break;
    }
    fracStr = fracRaw.slice(0, keep);
  }
  const prefix = (section.match(/^[^#0.,]+/) ?? [''])[0];
  const suffix = (section.match(/[^#0.,]+$/) ?? [''])[0];
  const sign = negative && sections.length === 1 ? '-' : '';
  return `${sign}${prefix}${intStr}${fracStr ? seps.decimal + fracStr : ''}${percent ? '%' : ''}${suffix}`;
}

/**
 * Format any value with a .NET pattern. Percent standard formats multiply
 * by 100. Dates use formatDatePattern. Unknown patterns return String(value).
 */
export function netFormat(value: unknown, pattern: string, seps: FormatSeparators = INVARIANT_SEPS): string {
  if (value === null || value === undefined) return '';
  const raw = pattern.trim();
  if (!raw) return String(value);
  if (value instanceof Date) return formatDatePattern(value, raw);
  const num = typeof value === 'number' ? value : Number(value);
  if (!Number.isNaN(num) && (typeof value === 'number' || /^[-+]?[\d.,\s]+$/.test(String(value)))) {
    if (/^D\d*$/i.test(raw)) {
      const digits = Number(raw.slice(1) || 0);
      return (num < 0 ? '-' : '') + pad(num, digits);
    }
    const expanded = expandStandardFormat(raw) ?? raw;
    if (expanded === 'D') return String(Math.trunc(num));
    if (/%/.test(raw) && /^[Pp]\d{0,2}$/.test(raw)) {
      return formatNumberPattern(num, expanded, seps);
    }
    if (/[#0]/.test(expanded)) return formatNumberPattern(num, expanded, seps);
  }
  if (isDatePattern(raw)) {
    const d = value instanceof Date ? value : new Date(String(value));
    if (!Number.isNaN(d.getTime())) return formatDatePattern(d, raw);
  }
  return String(value);
}

export { MONTHS_ID, DAYS_ID };
