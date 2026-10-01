import { PARAMS } from './constants';

/** Coerce a value to a number, stripping non-numeric chars. */
export function num(v: unknown): number {
  if (typeof v === 'number') return v;
  if (v == null || v === '') return NaN;
  return parseFloat(String(v).replace(/[^0-9.\-]/g, ''));
}

export function isMoney(p: string): boolean {
  return /price|harga|amount|jumlah|total|subtotal|tax|ppn|cost|biaya|debit|kredit|credit|saldo|nilai/i.test(p);
}

export function fmtNum(n: number): string {
  return (Math.round(n * 100) / 100).toLocaleString('id-ID');
}

export function fmtMoney(n: number): string {
  return (n < 0 ? '(' : '') + 'Rp ' + fmtNum(Math.abs(n)) + (n < 0 ? ')' : '');
}

export function fmtField(p: string, v: number): string {
  return isMoney(p) ? fmtMoney(v) : fmtNum(v);
}

export function resolveField(
  bind: string,
  row: Record<string, string | number> | null,
  ctx: Record<string, string> | null,
): string {
  let v: string | number | undefined;
  if (row && Object.prototype.hasOwnProperty.call(row, bind)) v = row[bind];
  else if (ctx && Object.prototype.hasOwnProperty.call(ctx, bind)) v = ctx[bind];
  else if (bind.charAt(0) === '@') {
    const p = PARAMS.find((x) => x.name === bind);
    v = p ? p.val : bind;
  }
  if (v === undefined || v === null || v === '') return (v as unknown) === 0 ? '0' : '';
  return typeof v === 'number' ? fmtField(bind, v) : String(v);
}

/** Evaluate an aggregate/system expression (Sum/Avg/Count/Max/Min/Today/Now/PageNumber/TotalPages). */
export function evalExpr(
  expr: string,
  rows: Array<Record<string, string | number>>,
  _ctx: Record<string, string> | null,
  pageNo: number,
  pageCount: number,
): string {
  expr = (expr || '').trim();
  let m: RegExpMatchArray | null;
  if ((m = expr.match(/^Sum\(([^)]+)\)(?:\s*\*\s*([\d.]+))?$/))) {
    const f = m[1].trim(); let sum = 0;
    rows.forEach((r) => { const n = num(r && r[f]); if (!isNaN(n)) sum += n; });
    if (m[2]) sum *= parseFloat(m[2]);
    return isMoney(f) ? fmtMoney(sum) : fmtNum(sum);
  }
  if ((m = expr.match(/^Avg\(([^)]+)\)$/))) {
    const f = m[1].trim(); let sum = 0; let c = 0;
    rows.forEach((r) => { const n = num(r && r[f]); if (!isNaN(n)) { sum += n; c++; } });
    const a = c ? sum / c : 0;
    return isMoney(f) ? fmtMoney(a) : fmtNum(a);
  }
  if (/^Count\(\s*\)$/.test(expr)) return String(rows.length);
  if ((m = expr.match(/^Max\(([^)]+)\)$/))) {
    const f = m[1].trim(); let mx = -Infinity;
    rows.forEach((r) => { const n = num(r && r[f]); if (!isNaN(n)) mx = Math.max(mx, n); });
    return fmtNum(mx === -Infinity ? 0 : mx);
  }
  if ((m = expr.match(/^Min\(([^)]+)\)$/))) {
    const f = m[1].trim(); let mn = Infinity;
    rows.forEach((r) => { const n = num(r && r[f]); if (!isNaN(n)) mn = Math.min(mn, n); });
    return fmtNum(mn === Infinity ? 0 : mn);
  }
  if (/^Today\(\s*\)$/.test(expr)) return '17 Jun 2026';
  if (/^Now\(\s*\)$/.test(expr)) return '17 Jun 2026 14:30';
  if (/^PageNumber\(\s*\)$/.test(expr)) return String(pageNo);
  if (/^TotalPages\(\s*\)$/.test(expr)) return String(pageCount);
  return '= ' + expr;
}
