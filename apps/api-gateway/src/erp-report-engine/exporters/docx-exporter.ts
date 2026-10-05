/**
 * DOCX exporter (Wave G0) — real .docx via the `docx` package.
 * Word is a flow format, so the absolutely-positioned model is
 * reconstructed as flow: text components clustered per band into table
 * rows (consecutive bands with the same column signature merge into
 * one table), other components become paragraphs. Borders, fills,
 * bold/italic/underline, alignment and font sizes are carried over.
 */

import {
  AlignmentType,
  BorderStyle,
  Document,
  ImageRun,
  Packer,
  PageBreak,
  Paragraph,
  ShadingType,
  Table,
  TableCell,
  TableRow,
  TextRun,
  WidthType,
} from 'docx';
import type { ModelBand, ModelComponent, RenderModel } from '../engine-types-v2';
import { barcodePng } from '../barcode';

const mmToTwips = (mm: number) => Math.round(mm * 56.7);
const hex = (c?: string) => (c ?? '000000').replace('#', '').toUpperCase();

function alignmentOf(c: ModelComponent): (typeof AlignmentType)[keyof typeof AlignmentType] {
  switch (c.style?.align) {
    case 'center':
      return AlignmentType.CENTER;
    case 'right':
      return AlignmentType.RIGHT;
    default:
      return AlignmentType.LEFT;
  }
}

function borderOf(c: ModelComponent, side: 'top' | 'bottom' | 'left' | 'right') {
  const b = c.style?.border;
  if (!b) return undefined;
  const applies = b.sides.includes('all') || b.sides.includes(side);
  if (!applies) return undefined;
  return {
    style: b.style === 'dashed' ? BorderStyle.DASHED : b.style === 'dotted' ? BorderStyle.DOTTED : BorderStyle.SINGLE,
    size: Math.max(1, Math.round((b.width ?? 0.4) * 8)),
    color: hex(b.color ?? '#000000'),
    space: 0,
  };
}

function textRun(c: ModelComponent): TextRun {
  const st = c.style;
  return new TextRun({
    text: c.text ?? '',
    bold: st?.bold,
    italics: st?.italic,
    size: Math.round((st?.fontSize ?? 9) * 2),
    color: st?.color ? hex(st.color) : undefined,
    font: 'Calibri',
  });
}

function cellOf(c: ModelComponent | undefined, widthMm: number): TableCell {
  const borders = c
    ? { top: borderOf(c, 'top'), bottom: borderOf(c, 'bottom'), left: borderOf(c, 'left'), right: borderOf(c, 'right') }
    : undefined;
  const bg = c?.fill ?? c?.style?.background;
  return new TableCell({
    width: { size: mmToTwips(widthMm), type: WidthType.DXA },
    borders,
    shading:
      bg && bg.toLowerCase() !== 'transparent'
        ? { type: ShadingType.CLEAR, fill: hex(bg.startsWith('#') ? bg : '#FFFFFF') }
        : undefined,
    children: [
      new Paragraph({
        alignment: c ? alignmentOf(c) : AlignmentType.LEFT,
        children: c ? [textRun(c)] : [],
        spacing: { before: 0, after: 0 },
      }),
    ],
  });
}

function columnSignature(comps: ModelComponent[]): string {
  return comps
    .filter((c) => c.type === 'text' || c.type === 'box' || c.type === 'checkbox' || c.type === 'barcode')
    .map((c) => `${Math.round(c.x * 2) / 2}:${Math.round(c.width * 2) / 2}`)
    .join('|');
}

interface PendingTable {
  signature: string;
  columns: Array<{ x: number; width: number }>;
  rows: ModelComponent[][];
}

function bandToRow(band: ModelBand): { comps: ModelComponent[]; signature: string } | null {
  const comps = band.components
    .filter((c) => c.type !== 'image' && c.type !== 'subreport')
    .sort((a, b) => a.x - b.x);
  if (comps.length === 0) return null;
  // Multi-line bands (wrapped text rows) collapse to their text lines.
  return { comps, signature: columnSignature(band.components) };
}

export async function renderModelDocx(model: RenderModel): Promise<Buffer> {
  const children: Array<Paragraph | Table> = [];
  let pending: PendingTable | null = null;

  const flushTable = () => {
    if (!pending || pending.rows.length === 0) {
      pending = null;
      return;
    }
    const table = new Table({
      width: { size: 100, type: WidthType.PERCENTAGE },
      columnWidths: pending.columns.map((c) => mmToTwips(c.width)),
      rows: pending.rows.map((rowComps) => {
        const cells = pending!.columns.map((col) => {
          const comp = rowComps.find((c) => Math.abs(c.x - col.x) < 1.5);
          return cellOf(comp, col.width);
        });
        return new TableRow({ children: cells });
      }),
    });
    children.push(table);
    pending = null;
  };

  for (let pi = 0; pi < model.pages.length; pi++) {
    if (pi > 0) {
      flushTable();
      children.push(new Paragraph({ children: [new PageBreak()] }));
    }
    const page = model.pages[pi];
    for (const band of page.bands) {
      const row = bandToRow(band);
      const isTabular = row !== null && row.comps.length >= 2;
      if (isTabular) {
        if (pending && pending.signature === row.signature) {
          pending.rows.push(row.comps);
        } else {
          flushTable();
          const columns = row.comps.map((c) => ({ x: c.x, width: c.width }));
          pending = { signature: row.signature, columns, rows: [row.comps] };
        }
        continue;
      }
      flushTable();
      // Flow content: one paragraph per component row.
      const comps = [...band.components].sort((a, b) => a.y - b.y || a.x - b.x);
      for (const comp of comps) {
        if (comp.type === 'text') {
          children.push(
            new Paragraph({ alignment: alignmentOf(comp), children: [textRun(comp)], spacing: { before: 0, after: 0 } }),
          );
        } else if (comp.type === 'checkbox') {
          children.push(new Paragraph({ children: [new TextRun({ text: comp.checked ? '☑' : '☐', size: 18 })] }));
        } else if (comp.type === 'barcode' && comp.barcode) {
          const png = await barcodePng(comp.barcode.value, comp.barcode.symbology);
          if (png) {
            children.push(
              new Paragraph({
                children: [
                  new ImageRun({
                    data: png,
                    transformation: { width: Math.round(comp.width * 3.78), height: Math.round(comp.height * 3.78) },
                    type: 'png',
                  }),
                ],
              }),
            );
          } else {
            children.push(new Paragraph({ children: [new TextRun({ text: comp.barcode.value, size: 16 })] }));
          }
        } else if (comp.type === 'line') {
          children.push(
            new Paragraph({
              border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: '000000', space: 1 } },
              children: [],
            }),
          );
        }
      }
    }
  }
  flushTable();

  const doc = new Document({
    sections: [
      {
        properties: {
          page: {
            size: { width: mmToTwips(model.pageWidthMm), height: mmToTwips(model.pageHeightMm) },
            margin: {
              top: mmToTwips(model.margins.top),
              bottom: mmToTwips(model.margins.bottom),
              left: mmToTwips(model.margins.left),
              right: mmToTwips(model.margins.right),
            },
          },
        },
        children: children.length ? children : [new Paragraph({ children: [] })],
      },
    ],
  });
  return Packer.toBuffer(doc);
}
