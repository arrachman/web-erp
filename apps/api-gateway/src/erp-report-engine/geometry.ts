/**
 * Geometry + style mapping: template units (mm) → PDF points, and component
 * {@link CompStyle} → @react-pdf style objects. Pure helpers, no React import.
 */

import type {
  CompStyle,
  Margins,
  Orientation,
  PageSize,
  BorderSide,
} from './engine-types';

/** 1 mm in PDF points (72 dpi). */
export const MM = 72 / 25.4;

export const mm = (value: number): number => Math.round(value * MM * 100) / 100;

/** Page dimensions in mm (portrait). */
const PAGE_MM: Record<PageSize, { w: number; h: number }> = {
  A4: { w: 210, h: 297 },
  A5: { w: 148, h: 210 },
  Letter: { w: 215.9, h: 279.4 },
  Legal: { w: 215.9, h: 355.6 },
};

export interface PageGeom {
  /** Page width/height in points, accounting for orientation. */
  widthPt: number;
  heightPt: number;
  /** Inner content width in points (page width minus left+right margins). */
  contentWidthPt: number;
}

export function pageGeom(
  size: PageSize,
  orientation: Orientation,
  margins: Margins,
): PageGeom {
  const base = PAGE_MM[size] ?? PAGE_MM.A4;
  const wMm = orientation === 'landscape' ? base.h : base.w;
  const hMm = orientation === 'landscape' ? base.w : base.h;
  return {
    widthPt: mm(wMm),
    heightPt: mm(hMm),
    contentWidthPt: mm(wMm - margins.left - margins.right),
  };
}

type PdfStyle = Record<string, string | number>;

const FONT_HELVETICA = 'Helvetica';

/** @react-pdf ships Helvetica/Times/Courier. Map any family to Helvetica to
 * avoid font-file registration; bold/italic handled via fontWeight/fontStyle. */
function fontFamily(): string {
  return FONT_HELVETICA;
}

function applyBorder(out: PdfStyle, sides: BorderSide[], width: number, color: string): void {
  const has = (s: BorderSide) => sides.includes(s) || sides.includes('all');
  if (has('top')) {
    out.borderTopWidth = width;
    out.borderTopColor = color;
  }
  if (has('bottom')) {
    out.borderBottomWidth = width;
    out.borderBottomColor = color;
  }
  if (has('left')) {
    out.borderLeftWidth = width;
    out.borderLeftColor = color;
  }
  if (has('right')) {
    out.borderRightWidth = width;
    out.borderRightColor = color;
  }
}

/** Map component text style → @react-pdf style. */
export function textStyle(style: CompStyle | undefined): PdfStyle {
  const s = style ?? {};
  const out: PdfStyle = {
    fontFamily: fontFamily(),
    fontSize: s.fontSize ?? 9,
    color: s.color ?? '#000000',
    textAlign: s.align ?? 'left',
  };
  if (s.bold) out.fontWeight = 'bold';
  if (s.italic) out.fontStyle = 'italic';
  if (s.background && s.background !== 'transparent') out.backgroundColor = s.background;
  if (s.border?.sides?.length) {
    applyBorder(out, s.border.sides, s.border.width ?? 0.5, s.border.color ?? '#000000');
  }
  return out;
}

/** Absolute box (left/top/width/height in pt) for a component within its band. */
export function boxStyle(x: number, y: number, width: number, height: number): PdfStyle {
  return {
    position: 'absolute',
    left: mm(x),
    top: mm(y),
    width: mm(width),
    height: mm(height),
  };
}
