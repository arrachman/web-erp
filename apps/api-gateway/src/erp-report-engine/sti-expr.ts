/**
 * Public entry point for Stimulsoft text evaluation (Wave G0):
 *  - evalStiText: full component text with { … } expression segments and
 *    literal parts; `{{` / `}}` are literal-brace escapes.
 *  - evalStiExpression: a bare expression body (band conditions, group
 *    keys, filters) — returns the raw value.
 *  - evalStiFormatted: single-expression text + component TextFormat —
 *    returns the formatted display string.
 * Evaluation failures never throw into rendering: they yield '' and are
 * reported through the optional onWarn callback.
 */

import { parseStiExpression, StiParseError } from './expr-parser';
import { evalNode, stringifyValue } from './expr-functions';
import type { StiScope } from './expr-scope';
import { netFormat } from './net-format';

export type StiWarn = (message: string) => void;

interface Segment {
  expr?: string;
  literal?: string;
}

/** Split component text into literal / expression segments (brace + string aware). */
export function splitStiText(text: string): Segment[] {
  const segments: Segment[] = [];
  let literal = '';
  let i = 0;
  const n = text.length;
  const flush = () => {
    if (literal) segments.push({ literal });
    literal = '';
  };
  while (i < n) {
    const ch = text[i];
    if (ch === '{' && text[i + 1] === '{') {
      literal += '{';
      i += 2;
      continue;
    }
    if (ch === '}' && text[i + 1] === '}') {
      literal += '}';
      i += 2;
      continue;
    }
    if (ch === '{') {
      let depth = 1;
      let j = i + 1;
      let quote = '';
      while (j < n && depth > 0) {
        const cj = text[j];
        if (quote) {
          if (cj === quote) quote = '';
        } else if (cj === '"' || cj === "'") quote = cj;
        else if (cj === '{') depth++;
        else if (cj === '}') depth--;
        j++;
      }
      flush();
      segments.push({ expr: text.slice(i + 1, j - 1) });
      i = j;
      continue;
    }
    literal += ch;
    i++;
  }
  flush();
  return segments;
}

function evalExprRaw(body: string, scope: StiScope, onWarn?: StiWarn): unknown {
  try {
    return evalNode(parseStiExpression(body), scope);
  } catch (err) {
    onWarn?.(`Ekspresi gagal dievaluasi {${body.slice(0, 80)}}: ${err instanceof Error ? err.message : String(err)}`);
    return undefined;
  }
}

export function evalStiText(text: string, scope: StiScope, onWarn?: StiWarn): string {
  if (!text) return '';
  if (!text.includes('{')) return text;
  const segments = splitStiText(text);
  let out = '';
  for (const seg of segments) {
    if (seg.literal !== undefined) out += seg.literal;
    else if (seg.expr !== undefined) out += stringifyValue(evalExprRaw(seg.expr, scope, onWarn), scope);
  }
  return out;
}

/** Evaluate a bare expression (no surrounding braces) and return the raw value. */
export function evalStiExpression(body: string, scope: StiScope, onWarn?: StiWarn): unknown {
  const trimmed = (body ?? '').trim();
  if (!trimmed) return undefined;
  if (!trimmed.includes('{')) return evalExprRaw(trimmed, scope, onWarn);
  // A single { … } block spanning the whole text → raw typed value.
  const segments = splitStiText(trimmed);
  if (segments.length === 1 && segments[0].expr !== undefined) {
    return evalExprRaw(segments[0].expr, scope, onWarn);
  }
  // Mixed segment text (e.g. group keys "{DS1.lokasi} {DS1.kode}") → concatenated string.
  return evalStiText(trimmed, scope, onWarn);
}

export function evalStiCondition(body: string, scope: StiScope, onWarn?: StiWarn): boolean {
  const v = evalStiExpression(body, scope, onWarn);
  if (typeof v === 'boolean') return v;
  if (typeof v === 'number') return v !== 0;
  return v !== undefined && v !== null && v !== '' && v !== 'False';
}

/**
 * Evaluate a component whose whole text is one expression and apply its
 * TextFormat (from the .mrt). Non-single-expression texts fall back to
 * evalStiText. Display formatting uses the configured locale separators.
 */
export function evalStiFormatted(
  text: string,
  format: { kind: string; pattern?: string } | undefined,
  scope: StiScope,
  onWarn?: StiWarn,
): string {
  const segments = splitStiText(text ?? '');
  const single = segments.length === 1 && segments[0].expr !== undefined;
  if (!single || !format) return evalStiText(text, scope, onWarn);
  const raw = evalExprRaw(segments[0].expr!, scope, onWarn);
  if (raw === undefined || raw === null) return '';
  const kind = format.kind;
  const displaySeps = { group: scope.format.groupSeparator, decimal: scope.format.decimalSeparator };
  if (kind === 'DateFormat' || kind === 'TimeFormat' || (kind === 'CustomFormat' && raw instanceof Date)) {
    const pattern = format.pattern || scope.format.datePattern;
    return netFormat(raw instanceof Date ? raw : new Date(String(raw)), pattern, displaySeps);
  }
  if (kind === 'NumberFormat' || kind === 'CurrencyFormat' || kind === 'PercentFormat' || kind === 'CustomFormat') {
    const pattern =
      format.pattern ||
      (kind === 'CurrencyFormat' || kind === 'NumberFormat'
        ? scope.format.nominalPattern
        : kind === 'PercentFormat'
          ? '#,##0.00%'
          : scope.format.nominalPattern);
    const num = typeof raw === 'number' ? raw : Number(raw);
    if (!Number.isNaN(num)) return netFormat(num, pattern, displaySeps);
    return String(raw);
  }
  if (kind === 'BooleanFormat') return raw ? 'Ya' : 'Tidak';
  return stringifyValue(raw, scope);
}

export { StiParseError };
