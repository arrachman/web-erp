/**
 * Builds the @react-pdf element tree from a {@link ReportTemplate} + {@link RenderContext}.
 *
 * Layout model (structured-template MVP, see DECISIONS.md 2026-06-13):
 *  - pageHeader / columnHeader → `fixed` at top, repeat every page.
 *  - pageFooter → `fixed` at bottom (page numbers via @react-pdf render callback).
 *  - reportTitle → flows once at the top of page 1.
 *  - data band → repeats per row; optional level-1 grouping via groupHeader/groupFooter.
 * Page padding reserves space for the fixed header/footer so flowing rows never overlap.
 *
 * No JSX (backend has no `jsx` tsconfig) — elements built with React.createElement (`h`).
 */

import { createElement as h, type ReactElement } from 'react';
import { Document, Page, View, Text, Image } from '@react-pdf/renderer';
import type {
  Band,
  Component,
  ImageComp,
  LineComp,
  ReportTemplate,
  RenderContext,
  TextComp,
} from './engine-types';
import { boxStyle, mm, pageGeom, textStyle } from './geometry';
import {
  evalCondition,
  evalExpression,
  type ExprScope,
  type ExprVars,
} from './expr';

let keySeq = 0;
const nextKey = (): string => {
  keySeq += 1;
  return `n${keySeq}`;
};

function mergeConditionStyles(comp: TextComp, scope: ExprScope): TextComp['style'] {
  if (!comp.conditions?.length) return comp.style;
  let style = { ...(comp.style ?? {}) };
  for (const cond of comp.conditions) {
    if (evalCondition(cond.when, scope)) style = { ...style, ...cond.style };
  }
  return style;
}

const usesPageVars = (expr: string): boolean =>
  expr.includes('PageNumber') || expr.includes('TotalPageCount');

function textElement(comp: TextComp, scope: ExprScope): ReactElement {
  const style = mergeConditionStyles(comp, scope);
  const box = boxStyle(comp.x, comp.y, comp.width, comp.height);
  const pdfStyle = { ...box, ...textStyle(style) };

  if (usesPageVars(comp.expression)) {
    return h(Text as never, {
      key: nextKey(),
      style: pdfStyle,
      render: ({ pageNumber, totalPages }: { pageNumber: number; totalPages: number }) =>
        evalExpression(comp.expression, {
          ...scope,
          vars: { ...scope.vars, PageNumber: pageNumber, TotalPageCount: totalPages },
        }),
    });
  }
  return h(Text as never, { key: nextKey(), style: pdfStyle }, evalExpression(comp.expression, scope));
}

function lineElement(comp: LineComp): ReactElement {
  const horizontal = comp.height === 0 || comp.width >= comp.height;
  const width = comp.style?.width ?? 0.5;
  const color = comp.style?.color ?? '#000000';
  const style: Record<string, string | number> = {
    ...boxStyle(comp.x, comp.y, comp.width, comp.height),
  };
  if (horizontal) {
    style.borderTopWidth = width;
    style.borderTopColor = color;
    style.height = mm(comp.height) || width;
  } else {
    style.borderLeftWidth = width;
    style.borderLeftColor = color;
    style.width = mm(comp.width) || width;
  }
  return h(View as never, { key: nextKey(), style });
}

function imageElement(comp: ImageComp, scope: ExprScope): ReactElement | null {
  const src = evalExpression(comp.src, scope).trim();
  if (!src) return null;
  const style = {
    ...boxStyle(comp.x, comp.y, comp.width, comp.height),
    objectFit: comp.fit ?? 'contain',
  };
  return h(Image as never, { key: nextKey(), src, style });
}

function componentElement(comp: Component, scope: ExprScope): ReactElement | null {
  switch (comp.type) {
    case 'text':
      return textElement(comp, scope);
    case 'line':
      return lineElement(comp);
    case 'image':
      return imageElement(comp, scope);
    default:
      return null;
  }
}

/** A band rendered as a fixed-height View; absolutely-positioned children. */
function bandBlock(
  band: Band,
  scope: ExprScope,
  opts: { fixed?: boolean; top?: number; bottom?: number; left: number; width: number },
): ReactElement {
  const style: Record<string, string | number> = {
    position: 'absolute',
    left: opts.left,
    width: opts.width,
    height: mm(band.height),
  };
  if (opts.top !== undefined) style.top = opts.top;
  if (opts.bottom !== undefined) style.bottom = opts.bottom;
  const children = band.components
    .map((c) => componentElement(c, scope))
    .filter((e): e is ReactElement => e !== null);
  return h(View as never, { key: nextKey(), fixed: opts.fixed, style }, children);
}

/** A flowing data row (relative box; absolute children). */
function rowBlock(band: Band, scope: ExprScope, contentWidthPt: number): ReactElement {
  const children = band.components
    .map((c) => componentElement(c, scope))
    .filter((e): e is ReactElement => e !== null);
  return h(
    View as never,
    {
      key: nextKey(),
      wrap: false,
      style: { position: 'relative', width: contentWidthPt, height: mm(band.height) },
    },
    children,
  );
}

function groupRows(
  rows: Record<string, unknown>[],
  groupBy: string,
): { key: unknown; rows: Record<string, unknown>[] }[] {
  const out: { key: unknown; rows: Record<string, unknown>[] }[] = [];
  for (const row of rows) {
    const key = row[groupBy];
    const last = out[out.length - 1];
    if (last && last.key === key) last.rows.push(row);
    else out.push({ key, rows: [row] });
  }
  return out;
}

function buildBodyRows(
  template: ReportTemplate,
  ctx: RenderContext,
  contentWidthPt: number,
  baseVars: ExprVars,
): ReactElement[] {
  const dataBand = template.bands.find((b) => b.type === 'data');
  if (!dataBand) return [];
  const groupHeader = template.bands.find((b) => b.type === 'groupHeader' && b.groupBy);
  const groupFooter = template.bands.find((b) => b.type === 'groupFooter');
  const company = ctx.company;
  const rowScope = (row: Record<string, unknown>, line: number): ExprScope => ({
    d: row,
    c: company,
    vars: { ...baseVars, Line: line },
  });

  const out: ReactElement[] = [];
  let line = 0;

  if (groupHeader?.groupBy) {
    for (const group of groupRows(ctx.rows, groupHeader.groupBy)) {
      const headScope: ExprScope = { d: group.rows[0], c: company, vars: baseVars };
      out.push(bandBlockFlow(groupHeader, headScope, contentWidthPt));
      for (const row of group.rows) {
        line += 1;
        out.push(rowBlock(dataBand, rowScope(row, line), contentWidthPt));
      }
      if (groupFooter) {
        const footScope: ExprScope = { d: { ...ctx.report, ...group.rows[0] }, c: company, vars: baseVars };
        out.push(bandBlockFlow(groupFooter, footScope, contentWidthPt));
      }
    }
  } else {
    for (const row of ctx.rows) {
      line += 1;
      out.push(rowBlock(dataBand, rowScope(row, line), contentWidthPt));
    }
    const minRows = dataBand.minRows ?? 0;
    for (let i = ctx.rows.length; i < minRows; i += 1) {
      out.push(rowBlock(dataBand, { d: {}, c: company, vars: baseVars }, contentWidthPt));
    }
  }
  return out;
}

/** A band that participates in the flow (group header/footer, report title). */
function bandBlockFlow(band: Band, scope: ExprScope, contentWidthPt: number): ReactElement {
  const children = band.components
    .map((c) => componentElement(c, scope))
    .filter((e): e is ReactElement => e !== null);
  return h(
    View as never,
    {
      key: nextKey(),
      wrap: false,
      style: { position: 'relative', width: contentWidthPt, height: mm(band.height) },
    },
    children,
  );
}

export function buildReportPdf(template: ReportTemplate, ctx: RenderContext): ReactElement {
  const geom = pageGeom(template.pageSize, template.orientation, template.margins);
  const m = template.margins;
  const baseVars: ExprVars = { Time: ctx.now ?? new Date() };
  const reportScope: ExprScope = { d: ctx.report, c: ctx.company, vars: baseVars };

  const pageHeader = template.bands.find((b) => b.type === 'pageHeader');
  const columnHeader = template.bands.find((b) => b.type === 'columnHeader');
  const pageFooter = template.bands.find((b) => b.type === 'pageFooter');
  const reportTitle = template.bands.find((b) => b.type === 'reportTitle');

  const headerH = pageHeader ? pageHeader.height : 0;
  const colHeaderH = columnHeader ? columnHeader.height : 0;
  const footerH = pageFooter ? pageFooter.height : 0;
  const leftPt = mm(m.left);

  const pageStyle = {
    paddingTop: mm(m.top + headerH + colHeaderH),
    paddingBottom: mm(m.bottom + footerH),
    paddingLeft: leftPt,
    paddingRight: mm(m.right),
    fontSize: 9,
    fontFamily: 'Helvetica',
  };

  const fixedChildren: ReactElement[] = [];
  if (pageHeader) {
    fixedChildren.push(
      bandBlock(pageHeader, reportScope, { fixed: true, top: mm(m.top), left: leftPt, width: geom.contentWidthPt }),
    );
  }
  if (columnHeader) {
    fixedChildren.push(
      bandBlock(columnHeader, reportScope, {
        fixed: true,
        top: mm(m.top + headerH),
        left: leftPt,
        width: geom.contentWidthPt,
      }),
    );
  }
  if (pageFooter) {
    fixedChildren.push(
      bandBlock(pageFooter, reportScope, { fixed: true, bottom: mm(m.bottom), left: leftPt, width: geom.contentWidthPt }),
    );
  }

  const flow: ReactElement[] = [];
  if (reportTitle) flow.push(bandBlockFlow(reportTitle, reportScope, geom.contentWidthPt));
  flow.push(...buildBodyRows(template, ctx, geom.contentWidthPt, baseVars));

  const page = h(
    Page as never,
    { size: template.pageSize, orientation: template.orientation, style: pageStyle },
    [...fixedChildren, ...flow],
  );
  return h(Document as never, {}, page);
}
