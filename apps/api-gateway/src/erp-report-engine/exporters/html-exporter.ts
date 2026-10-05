/**
 * HTML exporter (Wave G0) — the preview serialization of the RenderModel:
 * absolutely-positioned pages in mm, inline CSS only, no external assets,
 * safe to embed via iframe srcDoc. Fonts fall back to Liberation Sans /
 * DejaVu Sans metric substitutes for the legacy Lao UI.
 */

import type { ModelComponent, RenderModel } from '../engine-types-v2';
import { barcodeSvg } from '../barcode';

export function escapeHtml(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

export function compStyleCss(c: ModelComponent): string {
  const st = c.style;
  const parts: string[] = [
    `position:absolute`,
    `left:${c.x}mm`,
    `top:${c.y}mm`,
    `width:${c.width}mm`,
    `min-height:${c.height}mm`,
    `box-sizing:border-box`,
    `overflow:hidden`,
  ];
  if (st) {
    parts.push(`font-family:'Liberation Sans','DejaVu Sans',Arial,sans-serif`);
    parts.push(`font-size:${st.fontSize ?? 9}pt`);
    if (st.bold) parts.push('font-weight:700');
    if (st.italic) parts.push('font-style:italic');
    if (st.color) parts.push(`color:${st.color}`);
    if (st.align === 'center') parts.push('text-align:center');
    else if (st.align === 'right') parts.push('text-align:right');
  }
  if (c.type === 'box' || c.type === 'text') {
    const bg = c.fill ?? st?.background;
    if (bg && bg.toLowerCase() !== 'transparent') parts.push(`background:${bg}`);
    const b = st?.border;
    if (b) {
      const w = b.width ?? 0.4;
      const style = b.style === 'dashed' ? 'dashed' : b.style === 'dotted' ? 'dotted' : 'solid';
      const color = b.color ?? '#000';
      if (b.sides.includes('all')) parts.push(`border:${w}pt ${style} ${color}`);
      else {
        if (b.sides.includes('top')) parts.push(`border-top:${w}pt ${style} ${color}`);
        if (b.sides.includes('bottom')) parts.push(`border-bottom:${w}pt ${style} ${color}`);
        if (b.sides.includes('left')) parts.push(`border-left:${w}pt ${style} ${color}`);
        if (b.sides.includes('right')) parts.push(`border-right:${w}pt ${style} ${color}`);
      }
    }
  }
  return parts.join(';');
}

function renderComponent(c: ModelComponent): string {
  switch (c.type) {
    case 'text':
      return `<div style="${compStyleCss(c)}">${escapeHtml(c.text ?? '').replace(/\n/g, '<br/>')}</div>`;
    case 'line': {
      const horizontal = c.height <= c.width;
      const style = horizontal
        ? `position:absolute;left:${c.x}mm;top:${c.y}mm;width:${c.width}mm;border-top:${c.lineWidth ?? 0.5}pt solid ${c.style?.color ?? '#000'}`
        : `position:absolute;left:${c.x}mm;top:${c.y}mm;height:${c.height}mm;border-left:${c.lineWidth ?? 0.5}pt solid ${c.style?.color ?? '#000'}`;
      return `<div style="${style}"></div>`;
    }
    case 'box':
      return `<div style="${compStyleCss(c)}"></div>`;
    case 'image':
      return c.src
        ? `<img src="${escapeHtml(c.src)}" style="position:absolute;left:${c.x}mm;top:${c.y}mm;width:${c.width}mm;height:${c.height}mm;object-fit:contain"/>`
        : '';
    case 'barcode': {
      const svg = c.barcode ? barcodeSvg(c.barcode.value, c.barcode.symbology) : '';
      if (!svg) return '';
      const sized = svg.replace('<svg ', `<svg width="100%" height="100%" preserveAspectRatio="none" `);
      return `<div style="position:absolute;left:${c.x}mm;top:${c.y}mm;width:${c.width}mm;height:${c.height}mm">${sized}</div>`;
    }
    case 'checkbox': {
      const size = Math.min(c.width, c.height);
      const mark = c.checked ? '&#10003;' : '';
      return `<div style="position:absolute;left:${c.x}mm;top:${c.y}mm;width:${size}mm;height:${size}mm;border:0.5pt solid #000;font-size:8pt;line-height:1;text-align:center">${mark}</div>`;
    }
    default:
      return '';
  }
}

export function renderModelHtml(model: RenderModel): string {
  const pages = model.pages
    .map((page) => {
      const comps = page.bands.flatMap((b) => b.components).map(renderComponent).join('');
      return `<div class="rpt-page" style="position:relative;width:${model.pageWidthMm}mm;height:${model.pageHeightMm}mm;background:#fff;margin:0 auto 8mm;box-shadow:0 1px 6px rgba(0,0,0,.25);overflow:hidden">${comps}</div>`;
    })
    .join('\n');
  return `<!DOCTYPE html>
<html><head><meta charset="utf-8"/><title>${escapeHtml(model.name ?? 'Laporan')}</title>
<style>@page{size:${model.pageWidthMm}mm ${model.pageHeightMm}mm;margin:0}body{margin:0;background:#e8e8e8;font-family:'Liberation Sans','DejaVu Sans',Arial,sans-serif}@media print{body{background:#fff}.rpt-page{box-shadow:none;margin:0 auto;page-break-after:always}}</style>
</head><body>${pages}</body></html>`;
}
