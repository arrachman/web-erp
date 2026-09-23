import type { RsElement } from './types';
import { resolveField, evalExpr } from './format';

type Mode = 'design' | 'preview';

/** Inline CSS string for an element box, given render mode + selection. */
export function elBox(el: RsElement, mode: Mode, selected: boolean): string {
  const data = el.kind === 'field' || el.kind === 'expr';
  let s = 'position:absolute;box-sizing:border-box;left:' + el.x + 'px;top:' + el.y + 'px;width:' + el.w + 'px;height:' + el.h + 'px;overflow:hidden;';
  const bc = el.bColor || '#1f2937'; const bw = (el.bWidth || 1) + 'px';
  let bord = '';
  if (el.bTop) bord += 'border-top:' + bw + ' solid ' + bc + ';';
  if (el.bBottom) bord += 'border-bottom:' + bw + ' solid ' + bc + ';';
  if (el.bLeft) bord += 'border-left:' + bw + ' solid ' + bc + ';';
  if (el.bRight) bord += 'border-right:' + bw + ' solid ' + bc + ';';
  if (el.kind === 'line' || el.kind === 'box') {
    s += 'background:' + (el.bg || (el.kind === 'line' ? '#1f2937' : 'transparent')) + ';';
    if (el.kind === 'box') s += 'border:1px solid ' + (el.color || '#1f2937') + ';';
    s += bord;
    if (mode === 'design' && selected) s += 'outline:1.5px solid #2563eb;z-index:6;';
    return s;
  }
  const va = el.valign === 'top' ? 'flex-start' : el.valign === 'bottom' ? 'flex-end' : 'center';
  s += 'display:flex;align-items:' + va + ';padding:1px 3px;font-size:' + el.size + 'px;line-height:1.15;';
  s += (el.wordWrap ? 'white-space:normal;' : 'white-space:nowrap;');
  const j = el.align === 'right' ? 'flex-end' : el.align === 'center' ? 'center' : 'flex-start';
  s += 'justify-content:' + j + ';text-align:' + el.align + ';';
  s += 'font-weight:' + (el.bold ? 700 : 400) + ';'; if (el.italic) s += 'font-style:italic;';
  let deco = ''; if (el.underline) deco += 'underline '; if (el.strike) deco += 'line-through '; if (deco) s += 'text-decoration:' + deco.trim() + ';';
  const fam = el.font ? ("'" + el.font + "',") : '';
  s += 'font-family:' + fam + ((data || el.mono) ? "'IBM Plex Mono',monospace" : "'IBM Plex Sans',sans-serif") + ';';
  let color = el.color || '#14181f';
  if (mode === 'design' && data && !el.color) color = '#1d4ed8';
  s += 'color:' + color + ';';
  if (el.bg) s += 'background:' + el.bg + ';';
  s += bord;
  if (mode === 'design' && data) {
    if (!el.bg) s += 'background:rgba(37,99,235,.06);';
    if (!(el.bTop || el.bBottom || el.bLeft || el.bRight)) s += 'outline:1px dashed rgba(37,99,235,.4);outline-offset:-1px;';
  }
  if (mode === 'design' && selected) s += 'outline:1.5px solid #2563eb;outline-offset:0;z-index:6;';
  return s;
}

/** Design-mode display text for an element. */
export function elDisplay(el: RsElement): string {
  if (el.kind === 'line' || el.kind === 'box') return '';
  if (el.kind === 'label') return el.text;
  if (el.kind === 'field') return '{' + (el.bind || '?') + '}';
  return '=' + (el.bind || '?');
}

/** Preview-mode resolved value for an element. */
export function elPreview(
  el: RsElement,
  row: Record<string, string | number> | null,
  ctx: Record<string, string> | null,
  rows: Array<Record<string, string | number>>,
  pageNo: number,
  pageCount: number,
): string {
  if (el.kind === 'line' || el.kind === 'box') return '';
  if (el.kind === 'label') return el.text;
  if (el.kind === 'field') return resolveField(el.bind, row, ctx);
  return evalExpr(el.bind, rows, ctx, pageNo, pageCount);
}
