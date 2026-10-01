/**
 * Expression engine — Carbone-style markers (ATM of carbone.io, see
 * `report-engine/README.md` §5). Evaluates a string containing `{...}` blocks:
 *
 *   "{d.title}"                     → value from current scope (d)
 *   "{d.total:formatN(2)}"          → value + formatter chain
 *   "Hal {PageNumber}/{TotalPageCount}" → system variables + literal text
 *   "{c.company.name:upperCase}"    → company/config context (c)
 *
 * Pure functions — no React, no IO. Used by the PDF document builder and unit-tested
 * in isolation.
 */

export interface ExprVars {
  PageNumber?: number;
  TotalPageCount?: number;
  Line?: number;
  Time?: Date;
}

export interface ExprScope {
  /** Current binding object: a data row in the data band, report-level elsewhere. */
  d?: unknown;
  /** Company / global config context. */
  c?: unknown;
  vars?: ExprVars;
}

type Formatter = (value: unknown, args: string[], scope: ExprScope) => unknown;

const idNumber = (value: unknown, decimals: number): string => {
  const n = typeof value === 'number' ? value : Number(value);
  if (!Number.isFinite(n)) return '';
  return new Intl.NumberFormat('id-ID', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  }).format(n);
};

const pad2 = (n: number): string => String(n).padStart(2, '0');

const toDate = (value: unknown): Date | null => {
  if (value instanceof Date) return value;
  if (typeof value === 'number') return new Date(value);
  if (typeof value === 'string' && value.trim()) {
    const d = new Date(value);
    return Number.isNaN(d.getTime()) ? null : d;
  }
  return null;
};

const MONTHS_ID = [
  'Januari',
  'Februari',
  'Maret',
  'April',
  'Mei',
  'Juni',
  'Juli',
  'Agustus',
  'September',
  'Oktober',
  'November',
  'Desember',
];

/** Format a date by a small token pattern: DD MM MMMM YYYY YY HH mm ss. */
const formatDate = (value: unknown, pattern: string): string => {
  const d = toDate(value);
  if (!d) return '';
  return pattern
    .replace(/MMMM/g, MONTHS_ID[d.getMonth()])
    .replace(/YYYY/g, String(d.getFullYear()))
    .replace(/YY/g, String(d.getFullYear()).slice(-2))
    .replace(/MM/g, pad2(d.getMonth() + 1))
    .replace(/DD/g, pad2(d.getDate()))
    .replace(/HH/g, pad2(d.getHours()))
    .replace(/mm/g, pad2(d.getMinutes()))
    .replace(/ss/g, pad2(d.getSeconds()));
};

const ucFirst = (s: string): string => (s ? s.charAt(0).toUpperCase() + s.slice(1) : s);

export const FORMATTERS: Record<string, Formatter> = {
  // Numbers — `formatN(2)` / `formatNumber(0)` / money (no currency symbol per ERP §2.9).
  formatN: (v, a) => idNumber(v, a[0] ? Number(a[0]) : 2),
  formatNumber: (v, a) => idNumber(v, a[0] ? Number(a[0]) : 2),
  money: (v, a) => idNumber(v, a[0] ? Number(a[0]) : 2),
  formatMoney: (v, a) => idNumber(v, a[0] ? Number(a[0]) : 2),
  int: (v) => idNumber(v, 0),
  // Dates — default Indonesian short date.
  formatDate: (v, a) => formatDate(v, a[0] || 'DD/MM/YYYY'),
  formatDateLong: (v) => formatDate(v, 'DD MMMM YYYY'),
  // Text transforms.
  upperCase: (v) => String(v ?? '').toUpperCase(),
  lowerCase: (v) => String(v ?? '').toLowerCase(),
  ucFirst: (v) => ucFirst(String(v ?? '')),
  trim: (v) => String(v ?? '').trim(),
  // Fallback when value is empty/nullish: `:default(-)`.
  default: (v, a) => (v === null || v === undefined || v === '' ? (a[0] ?? '') : v),
  // Concatenation helpers.
  prepend: (v, a) => `${a[0] ?? ''}${v ?? ''}`,
  append: (v, a) => `${v ?? ''}${a[0] ?? ''}`,
};

/** Walk a dotted/indexed path like `company.name` or `meta[0].value`. */
export function getPath(root: unknown, path: string): unknown {
  if (!path) return root;
  const parts = path.match(/[^.[\]]+/g) ?? [];
  let cur: unknown = root;
  for (const part of parts) {
    if (cur === null || cur === undefined) return undefined;
    cur = (cur as Record<string, unknown>)[part];
  }
  return cur;
}

/** Resolve a marker root token (`d` / `c` / system var) + remaining path. */
function resolveRoot(token: string, scope: ExprScope): unknown {
  const dot = token.indexOf('.');
  const root = dot === -1 ? token : token.slice(0, dot);
  const rest = dot === -1 ? '' : token.slice(dot + 1);
  switch (root) {
    case 'd':
      return getPath(scope.d, rest);
    case 'c':
      return getPath(scope.c, rest);
    case 'PageNumber':
      return scope.vars?.PageNumber ?? '';
    case 'TotalPageCount':
      return scope.vars?.TotalPageCount ?? '';
    case 'Line':
      return scope.vars?.Line ?? '';
    case 'Time':
      return scope.vars?.Time ?? new Date();
    default:
      return getPath(scope.d, token);
  }
}

/** Split a formatter call `name(a, b)` → ['name', ['a','b']]. */
function parseFormatter(token: string): { name: string; args: string[] } {
  const m = token.match(/^([A-Za-z_][\w]*)\s*(?:\((.*)\))?$/);
  if (!m) return { name: token.trim(), args: [] };
  const args =
    m[2] === undefined || m[2].trim() === ''
      ? []
      : m[2].split(',').map((s) => s.trim().replace(/^['"]|['"]$/g, ''));
  return { name: m[1], args };
}

/** Evaluate one `{...}` block body (without the braces) to a primitive. */
export function evalMarker(body: string, scope: ExprScope): unknown {
  // Split on ':' that separate path from formatter chain. Paths never contain ':'.
  const segments = body.split(':').map((s) => s.trim());
  let value = resolveRoot(segments[0], scope);
  for (let i = 1; i < segments.length; i += 1) {
    const { name, args } = parseFormatter(segments[i]);
    const fn = FORMATTERS[name];
    if (fn) value = fn(value, args, scope);
  }
  return value;
}

/** Evaluate a full expression string with embedded `{...}` markers. */
export function evalExpression(input: string, scope: ExprScope): string {
  if (!input) return '';
  return input.replace(/\{([^}]*)\}/g, (_, body: string) => {
    const v = evalMarker(body, scope);
    return v === null || v === undefined ? '' : String(v);
  });
}

const COND_RE = /^\s*(.+?)\s*(<=|>=|==|!=|<|>)\s*(.+?)\s*$/;

/** Parse a literal RHS: number, quoted string, or bare word. */
function parseLiteral(raw: string, scope: ExprScope): unknown {
  const t = raw.trim();
  if (/^-?\d+(\.\d+)?$/.test(t)) return Number(t);
  if (/^['"].*['"]$/.test(t)) return t.slice(1, -1);
  if (t === 'true') return true;
  if (t === 'false') return false;
  if (t === 'null') return null;
  // Otherwise treat as a path expression against the scope.
  return resolveRoot(t, scope);
}

/**
 * Evaluate a condition string like `d.total < 0` or `d.status == 'CANCELLED'`.
 * Supports a single comparison; returns false on parse failure (fail-safe).
 */
export function evalCondition(expr: string, scope: ExprScope): boolean {
  const m = expr.match(COND_RE);
  if (!m) return false;
  const left = parseLiteral(m[1], scope);
  const op = m[2];
  const right = parseLiteral(m[3], scope);
  const ln = typeof left === 'number' ? left : Number(left);
  const rn = typeof right === 'number' ? right : Number(right);
  const bothNumeric = Number.isFinite(ln) && Number.isFinite(rn);
  switch (op) {
    case '==':
      return left === right || (bothNumeric && ln === rn);
    case '!=':
      return !(left === right || (bothNumeric && ln === rn));
    case '<':
      return bothNumeric && ln < rn;
    case '>':
      return bothNumeric && ln > rn;
    case '<=':
      return bothNumeric && ln <= rn;
    case '>=':
      return bothNumeric && ln >= rn;
    default:
      return false;
  }
}
