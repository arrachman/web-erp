/**
 * Stimulsoft property decoding helpers for the .mrt importer: colors,
 * fonts, borders, alignments, and the serialized condition format used
 * by both component Conditions and band Filters (comma-separated values
 * with _xHHHH_ escaping protecting commas inside expressions).
 */

import type { BorderStyle, CompStyle, StyleCondition } from '../../src/erp-report-engine/engine-types';
import { decodeStiEscapes } from './mrt-xml';

const NAMED_COLORS: Record<string, string> = {
  black: '#000000',
  white: '#FFFFFF',
  red: '#FF0000',
  green: '#008000',
  blue: '#0000FF',
  yellow: '#FFFF00',
  gray: '#808080',
  grey: '#808080',
  silver: '#C0C0C0',
  maroon: '#800000',
  navy: '#000080',
  olive: '#808000',
  purple: '#800080',
  teal: '#008080',
  aqua: '#00FFFF',
  fuchsia: '#FF00FF',
  lime: '#00FF00',
  orange: '#FFA500',
  brown: '#A52A2A',
  pink: '#FFC0CB',
  gold: '#FFD700',
  darkgray: '#A9A9A9',
  darkgrey: '#A9A9A9',
  lightgray: '#D3D3D3',
  lightgrey: '#D3D3D3',
  control: '#F0F0F0',
  window: '#FFFFFF',
  windowtext: '#000000',
};

/** 'Black' | '[255:0:0:0]' (A:R:G:B) | '0:0:0' → #RRGGBB; Transparent → undefined. */
export function stiColor(raw: string | undefined): string | undefined {
  if (!raw) return undefined;
  const s = raw.trim();
  if (!s || s.toLowerCase() === 'transparent' || s.toLowerCase() === 'none') return undefined;
  const bracket = /^\[(\d+):(\d+):(\d+):(\d+)\]$/.exec(s);
  if (bracket) {
    const [, , r, g, b] = bracket.map(Number);
    const hex = (n: number) => n.toString(16).padStart(2, '0').toUpperCase();
    return `#${hex(r)}${hex(g)}${hex(b)}`;
  }
  const colon = /^(\d+):(\d+):(\d+)$/.exec(s);
  if (colon) {
    const hex = (n: number) => Number(n).toString(16).padStart(2, '0').toUpperCase();
    return `#${hex(Number(colon[1]))}${hex(Number(colon[2]))}${hex(Number(colon[3]))}`;
  }
  if (s.startsWith('#')) return s;
  return NAMED_COLORS[s.toLowerCase()] ?? '#000000';
}

export interface StiFont {
  size: number;
  bold: boolean;
  italic: boolean;
  underline: boolean;
  family: string;
}

/** 'Lao UI,10' | 'Arial,9,Bold' | 'Lao UI,10,Bold,Italic' → font parts. */
export function stiFont(raw: string | undefined): StiFont {
  const fallback: StiFont = { size: 9, bold: false, italic: false, underline: false, family: 'Lao UI' };
  if (!raw) return fallback;
  const decoded = decodeStiEscapes(raw);
  const parts = decoded.split(',').map((p) => p.trim());
  const sizePart = parts.find((p) => /^\d+(\.\d+)?$/.test(p));
  const lower = decoded.toLowerCase();
  return {
    family: parts[0] || 'Lao UI',
    size: sizePart ? Number(sizePart) : 9,
    bold: lower.includes('bold'),
    italic: lower.includes('italic'),
    underline: lower.includes('underline'),
  };
}

export function fontToStyle(raw: string | undefined): Partial<CompStyle> {
  const f = stiFont(raw);
  const style: Partial<CompStyle> = { fontSize: f.size, bold: f.bold, italic: f.italic };
  return style;
}

/**
 * Border serialization: 'Sides;Color;Size;Style;DropShadow;ShadowSize;ShadowColor'
 * e.g. 'None;Black;2;Solid;False;4;Black', 'All;Black;1;Solid;False;4;Black',
 * or comma side lists 'Top,Bottom;Black;1;Solid;…'.
 */
export function stiBorder(raw: string | undefined): BorderStyle | undefined {
  if (!raw) return undefined;
  const parts = raw.split(';');
  const sidesRaw = (parts[0] ?? '').trim();
  if (!sidesRaw || sidesRaw.toLowerCase() === 'none') return undefined;
  const sides = sidesRaw
    .split(',')
    .map((s) => s.trim().toLowerCase())
    .map((s) => (s === 'all' ? 'all' : s === 'top' ? 'top' : s === 'bottom' ? 'bottom' : s === 'left' ? 'left' : s === 'right' ? 'right' : ''))
    .filter(Boolean) as BorderStyle['sides'];
  if (sides.length === 0) return undefined;
  const size = Number(parts[2] ?? '1');
  const styleRaw = (parts[3] ?? 'Solid').toLowerCase();
  return {
    sides,
    color: stiColor(parts[1]) ?? '#000000',
    width: Math.min(3, Math.max(0.25, (Number.isFinite(size) ? size : 1) / 2)),
    style: styleRaw.startsWith('dash') ? 'dashed' : styleRaw.startsWith('dot') ? 'dotted' : 'solid',
  };
}

export function stiAlign(raw: string | undefined): CompStyle['align'] {
  switch ((raw ?? '').trim().toLowerCase()) {
    case 'center':
      return 'center';
    case 'right':
      return 'right';
    default:
      return 'left';
  }
}

export function stiVertAlign(raw: string | undefined): CompStyle['vertAlign'] {
  switch ((raw ?? '').trim().toLowerCase()) {
    case 'center':
      return 'middle';
    case 'bottom':
      return 'bottom';
    default:
      return 'top';
  }
}

export function stiBool(raw: string | undefined): boolean {
  return (raw ?? '').trim().toLowerCase() === 'true';
}

/* ------------------------------------------------------------------ */
/* Serialized conditions (component Conditions + band Filters)         */
/* Fields: column, type, value1, value2, dataType, textColor,          */
/* backColor, font, bold, italic, expression, styleName, …             */
/* ------------------------------------------------------------------ */

const CONDITION_BUILDERS: Record<string, (col: string, v1: string, v2: string) => string> = {
  equalto: (c, v) => `${c} == ${quoteVal(v)}`,
  notequalto: (c, v) => `${c} != ${quoteVal(v)}`,
  greaterthan: (c, v) => `${c} > ${quoteVal(v)}`,
  greaterthanorequalto: (c, v) => `${c} >= ${quoteVal(v)}`,
  lessthan: (c, v) => `${c} < ${quoteVal(v)}`,
  lessthanorequalto: (c, v) => `${c} <= ${quoteVal(v)}`,
  between: (c, v1, v2) => `${c} >= ${quoteVal(v1)} && ${c} <= ${quoteVal(v2)}`,
  notbetween: (c, v1, v2) => `${c} < ${quoteVal(v1)} || ${c} > ${quoteVal(v2)}`,
  containing: (c, v) => `Contains(${c}, ${quoteVal(v)})`,
  notcontaining: (c, v) => `!Contains(${c}, ${quoteVal(v)})`,
  beginningwith: (c, v) => `StartsWith(${c}, ${quoteVal(v)})`,
  endingwith: (c, v) => `EndsWith(${c}, ${quoteVal(v)})`,
};

function quoteVal(v: string): string {
  const t = (v ?? '').trim();
  if (/^-?\d+(\.\d+)?$/.test(t)) return t;
  return `"${t.replace(/"/g, '\\"')}"`;
}

export interface ParsedCondition {
  when: string;
  style: Partial<CompStyle>;
  styleName?: string;
}

export function parseSerializedCondition(rawValue: string): ParsedCondition | null {
  const fields = rawValue.split(',').map((f) => decodeStiEscapes(f.trim()));
  const [column, type, v1, v2, , textColor, backColor, font, bold, , expression, styleName] = fields;
  let when = '';
  if (column && type) {
    const builder = CONDITION_BUILDERS[type.toLowerCase().replace(/\s/g, '')];
    if (builder) when = builder(column, v1 ?? '', v2 ?? '');
    else if (type.toLowerCase().includes('expression')) when = (expression || v1 || '').trim();
  } else if (expression) {
    when = expression.trim();
  }
  if (!when) return null;
  const style: Partial<CompStyle> = {};
  const color = stiColor(textColor);
  if (color) style.color = color;
  const bg = stiColor(backColor);
  if (bg) style.background = bg;
  if (font) Object.assign(style, fontToStyle(font));
  if ((bold ?? '').toLowerCase() === 'true') style.bold = true;
  // 'when' is stored as a bare Stimulsoft expression body for the v2 evaluator.
  return { when, style, styleName: styleName || undefined };
}

export function parseConditionsList(values: string[]): StyleCondition[] {
  const out: StyleCondition[] = [];
  for (const v of values) {
    const parsed = parseSerializedCondition(v);
    if (parsed) out.push({ when: parsed.when, style: parsed.style });
  }
  return out;
}

export function parseFiltersList(values: string[]): string[] {
  const out: string[] = [];
  for (const v of values) {
    const parsed = parseSerializedCondition(v);
    if (parsed) out.push(parsed.when);
  }
  return out;
}
