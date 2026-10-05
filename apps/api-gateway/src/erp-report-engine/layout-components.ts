/**
 * Band component evaluation for the layout engine: turns template
 * components + an evaluation scope into positioned model components,
 * applying CanGrow/CanShrink text measurement (approximate: average
 * char width ≈ 0.52 × font size, matching the PDF reference renderer).
 */

import type { Component } from './engine-types';
import type { CompanyContext, ModelComponent, StiBand } from './engine-types-v2';
import type { StiScope } from './expr-scope';
import { evalStiCondition, evalStiFormatted, evalStiText } from './sti-expr';

export function measureTextHeight(comp: Extract<Component, { type: 'text' }>, text: string): number {
  const fontSize = comp.style?.fontSize ?? 9;
  const widthPt = Math.max(10, comp.width * 2.83465);
  const charW = Math.max(3, fontSize * 0.52);
  const perLine = Math.max(1, Math.floor(widthPt / charW));
  let lines = 0;
  for (const part of text.split('\n')) {
    lines += Math.max(1, Math.ceil(part.length / perLine));
  }
  if (!(comp.style?.wordWrap ?? true)) lines = Math.max(1, text.split('\n').length);
  return (lines * fontSize * 1.22) / 2.83465 + 0.6;
}

export function evaluateBandComponents(
  band: StiBand,
  scope: StiScope,
  company: CompanyContext,
  onWarn?: (m: string) => void,
): { height: number; components: ModelComponent[] } {
  const out: ModelComponent[] = [];
  let contentBottom = 0;
  for (const comp of band.components) {
    const mc = evaluateComponent(comp, scope, band, company, onWarn);
    if (mc) {
      out.push(mc);
      contentBottom = Math.max(contentBottom, mc.y + mc.height);
    }
  }
  let height = band.height;
  if (band.canGrow) height = Math.max(height, contentBottom);
  if (band.canShrink) height = Math.min(height, Math.max(contentBottom, 0.1));
  return { height, components: out };
}

export function evaluateComponent(
  comp: Component,
  scope: StiScope,
  band: StiBand,
  company: CompanyContext,
  onWarn?: (m: string) => void,
): ModelComponent | null {
  const base = {
    name: 'name' in comp ? comp.name : undefined,
    x: comp.x,
    y: comp.y,
    width: comp.width,
    height: comp.height,
    style: 'style' in comp ? comp.style : undefined,
  };
  switch (comp.type) {
    case 'text': {
      const text = comp.format
        ? evalStiFormatted(comp.expression, comp.format, scope, onWarn)
        : evalStiText(comp.expression, scope, onWarn);
      // Conditional styles (StiCondition): merge in order when the
      // verbatim Stimulsoft condition evaluates true in this scope.
      if (comp.conditions?.length) {
        let merged = { ...(comp.style ?? {}) };
        for (const cond of comp.conditions) {
          if (evalStiCondition(cond.when, scope, onWarn)) merged = { ...merged, ...cond.style };
        }
        base.style = merged;
      }
      let height = comp.height;
      if (band.canGrow || band.canShrink) {
        const needed = measureTextHeight(comp, text);
        if (band.canGrow) height = Math.max(height, needed);
        if (band.canShrink && text === '') height = Math.min(height, needed);
      }
      return { ...base, type: 'text', text, height };
    }
    case 'image': {
      const isLogo = (comp.name ?? '').toLowerCase().includes('logo');
      const src = comp.src && comp.src.length > 4 ? comp.src : isLogo ? (company.logoUrl ?? '') : '';
      return src ? { ...base, type: 'image', src } : null;
    }
    case 'line':
      return { ...base, type: 'line', lineWidth: comp.style?.width };
    case 'box':
      return { ...base, type: 'box', fill: comp.fill };
    case 'barcode': {
      const value = evalStiText(comp.expression, scope, onWarn);
      return { ...base, type: 'barcode', barcode: { value, symbology: comp.symbology } };
    }
    case 'checkbox':
      return { ...base, type: 'checkbox', checked: evalStiCondition(comp.expression, scope, onWarn) };
    case 'subreport':
      return { ...base, type: 'subreport' };
    default:
      return null;
  }
}
