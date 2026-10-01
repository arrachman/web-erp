/**
 * Word renderer (docx). Landscape A4 page; title heading, generated-at line,
 * a table with a shaded + bold header row and data rows (numeric columns
 * right-aligned), then a summary block of paragraphs. Packer.toBuffer → Buffer.
 */

import {
  AlignmentType,
  Document,
  HeadingLevel,
  Packer,
  PageOrientation,
  Paragraph,
  ShadingType,
  Table,
  TableCell,
  TableRow,
  TextRun,
  WidthType,
} from 'docx';
import { ReportColumn, ReportDataset } from './report-types';
import { alignFor, formatCellValue, formatGeneratedAt, formatSummaryValue } from './report-export.format';

const HEADER_SHADE = 'E9ECEF';

function docAlign(col: ReportColumn): (typeof AlignmentType)[keyof typeof AlignmentType] {
  const a = alignFor(col);
  if (a === 'right') return AlignmentType.RIGHT;
  if (a === 'center') return AlignmentType.CENTER;
  return AlignmentType.LEFT;
}

function headerCell(col: ReportColumn): TableCell {
  return new TableCell({
    shading: { type: ShadingType.CLEAR, color: 'auto', fill: HEADER_SHADE },
    children: [
      new Paragraph({
        alignment: docAlign(col),
        children: [new TextRun({ text: col.header, bold: true, size: 18 })],
      }),
    ],
  });
}

function dataCell(col: ReportColumn, value: unknown): TableCell {
  return new TableCell({
    children: [
      new Paragraph({
        alignment: docAlign(col),
        children: [new TextRun({ text: formatCellValue(value, col.type), size: 16 })],
      }),
    ],
  });
}

export async function renderDocx(dataset: ReportDataset): Promise<Buffer> {
  const columns = dataset.columns;

  const headerRow = new TableRow({
    tableHeader: true,
    children: columns.map((col) => headerCell(col)),
  });

  const dataRows = dataset.rows.map(
    (row) => new TableRow({ children: columns.map((col) => dataCell(col, row[col.key])) }),
  );

  const table = new Table({
    width: { size: 100, type: WidthType.PERCENTAGE },
    rows: [headerRow, ...dataRows],
  });

  const summaryParagraphs: Paragraph[] = [];
  if (dataset.summary.length > 0) {
    summaryParagraphs.push(
      new Paragraph({
        spacing: { before: 240 },
        children: [new TextRun({ text: 'Ringkasan', bold: true, size: 20 })],
      }),
    );
    for (const item of dataset.summary) {
      summaryParagraphs.push(
        new Paragraph({
          children: [
            new TextRun({ text: `${item.label}: `, bold: true, size: 18 }),
            new TextRun({ text: formatSummaryValue(item), size: 18 }),
          ],
        }),
      );
    }
  }

  const doc = new Document({
    sections: [
      {
        properties: {
          page: { size: { orientation: PageOrientation.LANDSCAPE } },
        },
        children: [
          new Paragraph({ heading: HeadingLevel.HEADING_1, children: [new TextRun(dataset.title)] }),
          new Paragraph({
            spacing: { after: 200 },
            children: [
              new TextRun({ text: `Dibuat: ${formatGeneratedAt(dataset.generatedAt)}`, italics: true, size: 16 }),
            ],
          }),
          table,
          ...summaryParagraphs,
        ],
      },
    ],
  });

  return Packer.toBuffer(doc);
}
