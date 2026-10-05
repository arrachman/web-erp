/**
 * AST evaluator for the Stimulsoft dialect (see expr-parser.ts). Pure —
 * no eval. Aggregate functions (Sum/SumIf/CountIf/SumRunning/…) evaluate
 * their argument ASTs per row against the scope's band row-sets, exactly
 * like Stimulsoft: SumIf(DataBand1, DS1.gldebit, DS1.glid != 0).
 */

import type { AstNode } from './expr-parser';
import { ciBand, ciDataset, ciGet, type RowContext, type StiScope } from './expr-scope';
import { INVARIANT_SEPS, netFormat } from './net-format';
import {
  compareVals,
  dateProp,
  stringifyValue,
  toNum,
  truthy,
  walkProps,
} from './expr-values';
export { stringifyValue, toNum, truthy } from './expr-values';
import { formatDateDisplay } from './report-format';
import { terbilang } from './terbilang';

/* ------------------------------------------------------------------ */
/* Reference resolution                                                */
/* ------------------------------------------------------------------ */

const SYS_VARS = ['pagenumber', 'totalpagecount', 'line', 'time', 'today'];

export function resolveRef(path: string[], scope: StiScope, rc?: RowContext): unknown {
  if (path.length === 0) return undefined;
  const head = path[0];
  const headLower = head.toLowerCase();
  if (headLower === 'c' && scope.c) return walkProps(scope.c, path.slice(1));
  if (headLower === 'r' && scope.r) return walkProps(scope.r, path.slice(1));
  if (SYS_VARS.includes(headLower)) {
    const v =
      headLower === 'pagenumber'
        ? scope.vars.PageNumber
        : headLower === 'totalpagecount'
          ? scope.vars.TotalPageCount
          : headLower === 'line'
            ? (rc?.line ?? scope.vars.Line)
            : headLower === 'time'
              ? scope.vars.Time
              : scope.vars.Today;
    return walkProps(v, path.slice(1));
  }
  // Dataset field: DS1.field[.sub]
  const ds = ciDataset(scope, head);
  if (ds && path.length >= 2) {
    const row = rc?.rows.get(head) ?? rc?.rows.get(headLower) ?? ds.row;
    return walkProps(row, path.slice(1));
  }
  if (ds && path.length === 1) return ds.row;
  // Bare name: params, then primary dataset row, then single-row datasets.
  if (scope.params && head in scope.params) return walkProps(scope.params[head], path.slice(1));
  if (scope.primaryDataset) {
    const pds = ciDataset(scope, scope.primaryDataset);
    const row = rc?.rows.get(scope.primaryDataset) ?? pds?.row;
    const hit = walkProps(row, path);
    if (hit !== undefined) return hit;
  }
  for (const key of Object.keys(scope.datasets)) {
    const entry = scope.datasets[key];
    if (entry.rows.length === 1) {
      const hit = walkProps(entry.row ?? entry.rows[0], path);
      if (hit !== undefined) return hit;
    }
  }
  return undefined;
}

/* ------------------------------------------------------------------ */
/* Aggregates                                                          */
/* ------------------------------------------------------------------ */

interface AggRows {
  dataset: string;
  rows: Array<{ row: Record<string, unknown>; line: number }>;
}

function aggregateRows(bandArg: AstNode | undefined, fieldDs: string | undefined, scope: StiScope): AggRows | null {
  if (bandArg) {
    const name =
      bandArg.t === 'ref'
        ? bandArg.path.join('.')
        : bandArg.t === 'lit'
          ? String(bandArg.v)
          : '';
    const entry = ciBand(scope, name);
    if (entry) return entry;
  }
  if (fieldDs) {
    const ds = ciDataset(scope, fieldDs);
    if (ds) return { dataset: fieldDs, rows: ds.rows.map((row, i) => ({ row, line: i + 1 })) };
    for (const key of Object.keys(scope.bands)) {
      if (scope.bands[key].dataset.toLowerCase() === fieldDs.toLowerCase()) return scope.bands[key];
    }
  }
  if (scope.primaryDataset) {
    const ds = ciDataset(scope, scope.primaryDataset);
    if (ds) return { dataset: scope.primaryDataset, rows: ds.rows.map((row, i) => ({ row, line: i + 1 })) };
  }
  return null;
}

function fieldDatasetOf(node: AstNode | undefined): string | undefined {
  if (node?.t === 'ref' && node.path.length >= 2) return node.path[0];
  return undefined;
}

function evalPerRow(node: AstNode, scope: StiScope, agg: AggRows, item: { row: Record<string, unknown>; line: number }): unknown {
  const rc: RowContext = { rows: new Map([[agg.dataset, item.row]]), line: item.line };
  return evalNode(node, scope, rc);
}

function callAggregate(name: string, args: AstNode[], scope: StiScope, rc?: RowContext): unknown {
  const lower = name.toLowerCase();
  // Shapes: F(fieldExpr) | F(band, fieldExpr) | F(band, fieldExpr, cond) | CountIf(band, cond)
  let bandArg: AstNode | undefined;
  let fieldArg: AstNode | undefined;
  let condArg: AstNode | undefined;
  if (args.length >= 2 && (args[0].t === 'ref' && args[0].path.length === 1)) {
    bandArg = args[0];
    fieldArg = args[1];
    condArg = args[2];
  } else if (args.length >= 1) {
    fieldArg = args[0];
    condArg = args[1];
  }
  if (lower === 'count' && !fieldArg && bandArg) fieldArg = undefined;
  if (lower === 'countif' && bandArg && fieldArg && !condArg) {
    condArg = fieldArg;
    fieldArg = undefined;
  }
  const agg = aggregateRows(bandArg, fieldDatasetOf(fieldArg) ?? fieldDatasetOf(condArg), scope);
  if (!agg) return lower.startsWith('count') ? 0 : 0;
  const keep = agg.rows.filter((item) => !condArg || truthy(evalPerRow(condArg, scope, agg, item)));
  if (lower === 'count' || lower === 'countif') return keep.length;
  if (lower === 'countdistinct') {
    if (!fieldArg) return keep.length;
    return new Set(keep.map((it) => String(evalPerRow(fieldArg!, scope, agg, it)))).size;
  }
  if (!fieldArg) return 0;
  const values = keep.map((it) => evalPerRow(fieldArg!, scope, agg, it));
  switch (lower) {
    case 'sum':
    case 'sumif':
      return values.reduce<number>((a, v) => a + toNum(v), 0);
    case 'avg':
      return values.length ? values.reduce<number>((a, v) => a + toNum(v), 0) / values.length : 0;
    case 'min':
      return values.length ? values.reduce<unknown>((a, v) => (compareVals(v, a) < 0 ? v : a), values[0]) : 0;
    case 'max':
      return values.length ? values.reduce<unknown>((a, v) => (compareVals(v, a) > 0 ? v : a), values[0]) : 0;
    case 'first':
      return values[0];
    case 'last':
      return values[values.length - 1];
    case 'sumrunning': {
      const ds = fieldDatasetOf(fieldArg);
      const field = fieldArg.t === 'ref' ? fieldArg.path[fieldArg.path.length - 1] : '';
      return scope.sumRunning ? scope.sumRunning(ds, field) : values.reduce<number>((a, v) => a + toNum(v), 0);
    }
    default:
      return 0;
  }
}

const AGGREGATES = new Set(['sum', 'sumif', 'count', 'countif', 'countdistinct', 'avg', 'min', 'max', 'first', 'last', 'sumrunning']);

/* ------------------------------------------------------------------ */
/* Function calls                                                      */
/* ------------------------------------------------------------------ */

function formatArg(patternRaw: unknown, value: unknown, scope: StiScope): string {
  let pattern = String(patternRaw ?? '');
  const brace = /^\{\d+(?::(.*))?\}$/s.exec(pattern.trim());
  if (brace) pattern = brace[1] ?? '';
  if (!pattern) return stringifyValue(value, scope);
  return netFormat(value, pattern, INVARIANT_SEPS);
}

function callFunction(node: Extract<AstNode, { t: 'call' }>, scope: StiScope, rc?: RowContext): unknown {
  const rawName = node.name;
  const ev = (n: AstNode) => evalNode(n, scope, rc);
  if (rawName.startsWith('@method:')) {
    const method = rawName.slice('@method:'.length).toLowerCase();
    const receiver = ev(node.args[0]);
    if (method === 'tostring') {
      const fmt = node.args[1] ? String(ev(node.args[1])) : '';
      return fmt ? netFormat(receiver, fmt, INVARIANT_SEPS) : stringifyValue(receiver, scope);
    }
    return receiver;
  }
  const short = rawName.split('.').pop() ?? rawName;
  const lower = short.toLowerCase();
  const full = rawName.toLowerCase();
  if (AGGREGATES.has(lower)) return callAggregate(lower, node.args, scope, rc);
  const a = node.args;
  const s = (v: unknown) => (v === null || v === undefined ? '' : String(v));
  switch (lower) {
    case 'iif':
      return truthy(ev(a[0])) ? ev(a[1]) : ev(a[2]);
    case 'format':
      return formatArg(ev(a[0]), ev(a[1]), scope);
    case 'replace':
      return s(ev(a[0])).split(s(ev(a[1]))).join(s(ev(a[2])));
    case 'trim':
      return s(ev(a[0])).trim();
    case 'ltrim':
      return s(ev(a[0])).replace(/^\s+/, '');
    case 'rtrim':
      return s(ev(a[0])).replace(/\s+$/, '');
    case 'upper':
    case 'toupper':
    case 'ucase':
      return s(ev(a[0])).toUpperCase();
    case 'lower':
    case 'tolower':
    case 'lcase':
      return s(ev(a[0])).toLowerCase();
    case 'length':
    case 'len':
      return s(ev(a[0])).length;
    case 'substring': {
      const str = s(ev(a[0]));
      const start = toNum(ev(a[1]));
      return a[2] !== undefined ? str.substr(start, toNum(ev(a[2]))) : str.substr(start);
    }
    case 'indexof':
      return s(ev(a[0])).indexOf(s(ev(a[1])));
    case 'contains':
      return s(ev(a[0])).includes(s(ev(a[1])));
    case 'startswith':
      return s(ev(a[0])).startsWith(s(ev(a[1])));
    case 'endswith':
      return s(ev(a[0])).endsWith(s(ev(a[1])));
    case 'padleft':
      return s(ev(a[0])).padStart(toNum(ev(a[1])), a[2] ? s(ev(a[2]))[0] : ' ');
    case 'padright':
      return s(ev(a[0])).padEnd(toNum(ev(a[1])), a[2] ? s(ev(a[2]))[0] : ' ');
    case 'concat':
      return a.map((x) => s(ev(x))).join('');
    case 'tostring':
      return a[1] !== undefined ? netFormat(ev(a[0]), s(ev(a[1])), INVARIANT_SEPS) : stringifyValue(ev(a[0]), scope);
    case 'isnull':
      return ev(a[0]) ?? (a[1] !== undefined ? ev(a[1]) : '');
    case 'isnullorempty': {
      const v = ev(a[0]);
      return v === null || v === undefined || v === '';
    }
    case 'abs':
      return Math.abs(toNum(ev(a[0])));
    case 'round': {
      const v = toNum(ev(a[0]));
      const digits = a[1] !== undefined ? toNum(ev(a[1])) : 0;
      const f = 10 ** digits;
      return Math.round(v * f) / f;
    }
    case 'floor':
      return Math.floor(toNum(ev(a[0])));
    case 'ceiling':
      return Math.ceil(toNum(ev(a[0])));
    case 'min':
      return Math.min(toNum(ev(a[0])), toNum(ev(a[1])));
    case 'max':
      return Math.max(toNum(ev(a[0])), toNum(ev(a[1])));
    case 'pow':
      return Math.pow(toNum(ev(a[0])), toNum(ev(a[1])));
    case 'sqrt':
      return Math.sqrt(toNum(ev(a[0])));
    case 'year':
      return dateProp(toDate(ev(a[0]), scope), 'year');
    case 'month':
      return dateProp(toDate(ev(a[0]), scope), 'month');
    case 'day':
      return dateProp(toDate(ev(a[0]), scope), 'day');
    case 'today':
      return scope.vars.Today;
    case 'now':
      return scope.vars.Time;
    case 'terbilang':
    case 'f_nominal':
      return terbilang(toNum(ev(a[0])));
    case 'parse': {
      const v = ev(a[0]);
      return full.startsWith('double') || full.startsWith('decimal') || full.startsWith('float')
        ? toNum(v)
        : Math.trunc(toNum(v));
    }
    case 'todecimal':
    case 'todouble':
    case 'toint32':
    case 'toint':
      return toNum(ev(a[0]));
    default:
      return '';
  }
}

function toDate(v: unknown, scope: StiScope): Date {
  if (v instanceof Date) return v;
  const d = new Date(String(v ?? ''));
  return Number.isNaN(d.getTime()) ? scope.vars.Today : d;
}

/* ------------------------------------------------------------------ */
/* Main evaluator                                                      */
/* ------------------------------------------------------------------ */

export function evalNode(node: AstNode, scope: StiScope, rc?: RowContext): unknown {
  switch (node.t) {
    case 'lit':
      return node.v;
    case 'ref':
      return resolveRef(node.path, scope, rc);
    case 'idx': {
      const obj = evalNode(node.obj, scope, rc);
      const idx = evalNode(node.idx, scope, rc);
      if (Array.isArray(obj)) return obj[toNum(idx)];
      if (obj && typeof obj === 'object') return ciGet(obj as Record<string, unknown>, String(idx));
      if (typeof obj === 'string') return obj[toNum(idx)];
      return undefined;
    }
    case 'un': {
      const v = evalNode(node.e, scope, rc);
      return node.op === '!' ? !truthy(v) : -toNum(v);
    }
    case 'bin': {
      if (node.op === '&&') return truthy(evalNode(node.l, scope, rc)) && truthy(evalNode(node.r, scope, rc));
      if (node.op === '||') return truthy(evalNode(node.l, scope, rc)) || truthy(evalNode(node.r, scope, rc));
      const l = evalNode(node.l, scope, rc);
      const r = evalNode(node.r, scope, rc);
      switch (node.op) {
        case '==':
          return compareVals(l, r) === 0;
        case '!=':
          return compareVals(l, r) !== 0;
        case '<':
          return compareVals(l, r) < 0;
        case '<=':
          return compareVals(l, r) <= 0;
        case '>':
          return compareVals(l, r) > 0;
        case '>=':
          return compareVals(l, r) >= 0;
        case '+':
          if (typeof l === 'string' || typeof r === 'string') return stringifyValue(l, scope) + stringifyValue(r, scope);
          return toNum(l) + toNum(r);
        case '-':
          return toNum(l) - toNum(r);
        case '*':
          return toNum(l) * toNum(r);
        case '/':
          return toNum(r) === 0 ? 0 : toNum(l) / toNum(r);
        case '%':
          return toNum(r) === 0 ? 0 : toNum(l) % toNum(r);
        default:
          return undefined;
      }
    }
    case 'cond':
      return truthy(evalNode(node.c, scope, rc)) ? evalNode(node.a, scope, rc) : evalNode(node.b, scope, rc);
    case 'call':
      return callFunction(node, scope, rc);
    default:
      return undefined;
  }
}
