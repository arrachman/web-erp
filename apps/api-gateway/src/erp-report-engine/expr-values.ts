/**
 * Value semantics for the Stimulsoft evaluator: numeric coercion (incl.
 * id-ID formatted strings), truthiness, comparison, stringification and
 * dotted property walking (incl. Date parts). Pure helpers.
 */

import { formatDateDisplay } from './report-format';
import { ciGet, type StiScope } from './expr-scope';

/* ------------------------------------------------------------------ */
/* Value semantics                                                     */
/* ------------------------------------------------------------------ */

export function toNum(v: unknown): number {
  if (typeof v === 'number') return v;
  if (typeof v === 'boolean') return v ? 1 : 0;
  if (v instanceof Date) return v.getTime();
  if (v === null || v === undefined || v === '') return 0;
  const s = String(v).trim();
  const n = Number(s);
  if (!Number.isNaN(n)) return n;
  // id-ID formatted strings: 1.234,56
  const idn = Number(s.replace(/\./g, '').replace(',', '.'));
  return Number.isNaN(idn) ? 0 : idn;
}

export function isNumericLike(v: unknown): boolean {
  if (typeof v === 'number') return true;
  if (typeof v === 'string' && v.trim() !== '') return !Number.isNaN(Number(v));
  return false;
}

export function truthy(v: unknown): boolean {
  if (typeof v === 'boolean') return v;
  if (typeof v === 'number') return v !== 0;
  if (v === null || v === undefined) return false;
  if (typeof v === 'string') return v !== '' && v.toLowerCase() !== 'false';
  return true;
}

export function stringifyValue(v: unknown, scope: StiScope): string {
  if (v === null || v === undefined) return '';
  if (v instanceof Date) return formatDateDisplay(v, scope.format);
  if (typeof v === 'boolean') return v ? 'True' : 'False';
  if (typeof v === 'number') return Number.isInteger(v) ? String(v) : String(Math.round(v * 1e10) / 1e10);
  return String(v);
}

export function compareVals(l: unknown, r: unknown): number {
  if (l instanceof Date || r instanceof Date) {
    const a = l instanceof Date ? l.getTime() : new Date(String(l)).getTime();
    const b = r instanceof Date ? r.getTime() : new Date(String(r)).getTime();
    return a - b;
  }
  if (isNumericLike(l) && isNumericLike(r)) return toNum(l) - toNum(r);
  const a = l === null || l === undefined ? '' : String(l);
  const b = r === null || r === undefined ? '' : String(r);
  return a < b ? -1 : a > b ? 1 : 0;
}

export function dateProp(d: Date, prop: string): unknown {
  switch (prop.toLowerCase()) {
    case 'year': return d.getFullYear();
    case 'month': return d.getMonth() + 1;
    case 'day': return d.getDate();
    case 'hour': return d.getHours();
    case 'minute': return d.getMinutes();
    case 'second': return d.getSeconds();
    case 'dayofweek': return d.getDay();
    case 'date': return new Date(d.getFullYear(), d.getMonth(), d.getDate());
    default: return undefined;
  }
}

export function walkProps(value: unknown, path: string[]): unknown {
  let cur = value;
  for (const seg of path) {
    if (cur === null || cur === undefined) return undefined;
    if (cur instanceof Date) {
      cur = dateProp(cur, seg);
      continue;
    }
    if (Array.isArray(cur)) {
      const idx = Number(seg);
      cur = Number.isNaN(idx) ? undefined : cur[idx];
      continue;
    }
    if (typeof cur === 'object') {
      cur = ciGet(cur as Record<string, unknown>, seg);
      continue;
    }
    return undefined;
  }
  return cur;
}


