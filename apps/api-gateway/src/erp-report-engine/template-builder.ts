/**
 * Generates a default band-based {@link ReportTemplate} from a report's column list,
 * so every report gets a real, editable template without hand-authoring mm geometry.
 * Seeds use this; the Report Designer then tweaks the result.
 *
 * Layout: pageHeader (title + subtitle + meta), columnHeader (labels, bordered),
 * data band (one cell/column, type-aware formatter), pageFooter (print time + page #).
 */

import type {
  Band,
  Margins,
  Orientation,
  PageSize,
  ReportTemplate,
  TextComp,
  TextAlign,
} from './engine-types';

export type ColumnType = 'text' | 'number' | 'money' | 'date' | 'percent';

export interface TableColumnDef {
  key: string;
  label: string;
  type?: ColumnType;
  align?: TextAlign;
  /** Relative weight for width distribution (default 1). */
  width?: number;
}

export interface BuildTableTemplateOptions {
  name: string;
  module: string;
  columns: TableColumnDef[];
  pageSize?: PageSize;
  orientation?: Orientation;
  margins?: Margins;
  /** Title expression (default `{d.title}`). */
  titleExpr?: string;
}

const PAGE_W_MM: Record<PageSize, { w: number; h: number }> = {
  A4: { w: 210, h: 297 },
  A5: { w: 148, h: 210 },
  Letter: { w: 215.9, h: 279.4 },
  Legal: { w: 215.9, h: 355.6 },
};

const DEFAULT_MARGINS: Margins = { top: 12, right: 10, bottom: 12, left: 10 };

const alignFor = (col: TableColumnDef): TextAlign => {
  if (col.align) return col.align;
  if (col.type === 'number' || col.type === 'money' || col.type === 'percent') return 'right';
  if (col.type === 'date') return 'center';
  return 'left';
};

const formatterFor = (col: TableColumnDef): string => {
  switch (col.type) {
    case 'number':
    case 'money':
      return ':formatN(2)';
    case 'percent':
      return ':formatN(2):append(%)';
    case 'date':
      return ':formatDate(DD/MM/YYYY)';
    default:
      return '';
  }
};

interface ColLayout extends TableColumnDef {
  x: number;
  w: number;
}

/** Distribute columns across the content width (mm) by weight. */
function layoutColumns(columns: TableColumnDef[], contentWidthMm: number): ColLayout[] {
  const totalWeight = columns.reduce((s, c) => s + (c.width ?? 1), 0) || 1;
  let x = 0;
  return columns.map((c) => {
    const w = ((c.width ?? 1) / totalWeight) * contentWidthMm;
    const col = { ...c, x, w };
    x += w;
    return col;
  });
}

function headerCell(col: ColLayout, height: number): TextComp {
  return {
    type: 'text',
    name: `H_${col.key}`,
    x: col.x,
    y: 0,
    width: col.w,
    height,
    expression: col.label,
    style: {
      fontSize: 8,
      bold: true,
      align: alignFor(col),
      border: { sides: ['bottom'], width: 0.75, color: '#333333' },
    },
  };
}

function dataCell(col: ColLayout, height: number): TextComp {
  return {
    type: 'text',
    name: `D_${col.key}`,
    x: col.x,
    y: 0,
    width: col.w,
    height,
    expression: `{d.${col.key}${formatterFor(col)}}`,
    style: { fontSize: 8, align: alignFor(col) },
  };
}

export function buildTableTemplate(opts: BuildTableTemplateOptions): ReportTemplate {
  const pageSize = opts.pageSize ?? 'A4';
  const orientation = opts.orientation ?? 'portrait';
  const margins = opts.margins ?? DEFAULT_MARGINS;
  const base = PAGE_W_MM[pageSize] ?? PAGE_W_MM.A4;
  const pageWmm = orientation === 'landscape' ? base.h : base.w;
  const contentWidthMm = pageWmm - margins.left - margins.right;
  const cols = layoutColumns(opts.columns, contentWidthMm);

  const pageHeader: Band = {
    type: 'pageHeader',
    height: 24,
    components: [
      {
        type: 'text',
        name: 'Title',
        x: 0,
        y: 0,
        width: contentWidthMm,
        height: 7,
        expression: opts.titleExpr ?? '{d.title}',
        style: { fontSize: 14, bold: true, align: 'center' },
      },
      {
        type: 'text',
        name: 'Subtitle',
        x: 0,
        y: 7,
        width: contentWidthMm,
        height: 5,
        expression: '{d.subtitle}',
        style: { fontSize: 9, italic: true, align: 'center', color: '#555555' },
      },
      {
        type: 'text',
        name: 'Meta',
        x: 0,
        y: 13,
        width: contentWidthMm,
        height: 10,
        expression: '{d.metaText}',
        style: { fontSize: 8, align: 'left', color: '#333333' },
        canGrow: true,
      },
    ],
  };

  const columnHeader: Band = {
    type: 'columnHeader',
    height: 6,
    components: cols.map((c) => headerCell(c, 6)),
  };

  const data: Band = {
    type: 'data',
    height: 5,
    canGrow: true,
    components: cols.map((c) => dataCell(c, 5)),
  };

  const pageFooter: Band = {
    type: 'pageFooter',
    height: 8,
    components: [
      {
        type: 'text',
        name: 'PrintedAt',
        x: 0,
        y: 2,
        width: contentWidthMm / 2,
        height: 5,
        expression: 'Dicetak {Time:formatDate(DD/MM/YYYY HH:mm)}',
        style: { fontSize: 7, align: 'left', color: '#777777' },
      },
      {
        type: 'text',
        name: 'PageNo',
        x: contentWidthMm / 2,
        y: 2,
        width: contentWidthMm / 2,
        height: 5,
        expression: 'Halaman {PageNumber} / {TotalPageCount}',
        style: { fontSize: 7, align: 'right', color: '#777777' },
      },
    ],
  };

  return {
    name: opts.name,
    module: opts.module,
    version: 1,
    pageSize,
    orientation,
    margins,
    fonts: ['Helvetica'],
    bands: [pageHeader, columnHeader, data, pageFooter],
  };
}
