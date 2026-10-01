/**
 * PDF renderer (pdfkit, A4 landscape). pdfkit ships the standard-14 PDF fonts
 * (Helvetica family) built in, so NO .ttf files are needed — chosen over
 * pdfmake 0.3 to avoid font-file/vfs plumbing.
 *
 * Layout: title heading, generated-at line, a table with a bold gray header
 * row, body rows (numeric columns right-aligned), then a summary block.
 */

import PDFDocument from 'pdfkit';
import { ReportColumn, ReportDataset } from './report-types';
import { alignFor, formatCellValue, formatGeneratedAt, formatSummaryValue } from './report-export.format';

const MARGIN = 40;
const ROW_HEIGHT = 16;
const FONT_SIZE = 8;
const HEADER_FILL = '#e9ecef';

/** Weight a column's width so numeric/date columns stay narrow, text wider. */
function colWeight(col: ReportColumn): number {
  switch (col.type) {
    case 'money':
    case 'qty':
    case 'number':
      return 1.4;
    case 'percent':
    case 'date':
      return 1.1;
    case 'status':
      return 1;
    default:
      return 2;
  }
}

export function renderPdf(dataset: ReportDataset): Promise<Buffer> {
  return new Promise((resolve, reject) => {
    const pdf = new PDFDocument({ size: 'A4', margin: MARGIN, layout: 'landscape' });
    const chunks: Buffer[] = [];
    pdf.on('data', (c: Buffer) => chunks.push(c));
    pdf.on('end', () => resolve(Buffer.concat(chunks)));
    pdf.on('error', reject);

    const pageLeft = MARGIN;
    const usableWidth = pdf.page.width - MARGIN * 2;

    const columns = dataset.columns.length > 0 ? dataset.columns : [];
    const totalWeight = columns.reduce((s, c) => s + colWeight(c), 0) || 1;
    const colWidths = columns.map((c) => (colWeight(c) / totalWeight) * usableWidth);
    const colX: number[] = [];
    let acc = pageLeft;
    for (const w of colWidths) {
      colX.push(acc);
      acc += w;
    }

    // Title + generated-at
    pdf.fontSize(14).font('Helvetica-Bold').text(dataset.title, pageLeft);
    pdf
      .fontSize(9)
      .font('Helvetica-Oblique')
      .text(`Dibuat: ${formatGeneratedAt(dataset.generatedAt)}`, pageLeft);
    pdf.moveDown(0.5);

    let y = pdf.y;

    const ensureSpace = (drawHeader = true) => {
      if (y + ROW_HEIGHT > pdf.page.height - MARGIN) {
        pdf.addPage();
        y = MARGIN;
        if (drawHeader) drawHeaderRow();
      }
    };

    function drawHeaderRow() {
      pdf
        .rect(pageLeft, y, usableWidth, ROW_HEIGHT)
        .fill(HEADER_FILL);
      pdf.fillColor('black');
      columns.forEach((col, i) => {
        pdf.font('Helvetica-Bold').fontSize(FONT_SIZE);
        pdf.text(col.header, colX[i] + 2, y + 4, {
          width: colWidths[i] - 4,
          align: alignFor(col),
          lineBreak: false,
          ellipsis: true,
        });
      });
      y += ROW_HEIGHT;
      pdf.moveTo(pageLeft, y).lineTo(pageLeft + usableWidth, y).lineWidth(0.5).stroke();
    }

    const drawDataRow = (row: Record<string, unknown>) => {
      ensureSpace();
      columns.forEach((col, i) => {
        pdf.font('Helvetica').fontSize(FONT_SIZE).fillColor('black');
        pdf.text(formatCellValue(row[col.key], col.type), colX[i] + 2, y + 4, {
          width: colWidths[i] - 4,
          align: alignFor(col),
          lineBreak: false,
          ellipsis: true,
        });
      });
      y += ROW_HEIGHT;
    };

    // Table
    drawHeaderRow();
    for (const row of dataset.rows) drawDataRow(row);

    // Summary block
    if (dataset.summary.length > 0) {
      y += ROW_HEIGHT * 0.5;
      ensureSpace(false);
      pdf.moveTo(pageLeft, y).lineTo(pageLeft + usableWidth, y).lineWidth(0.8).stroke();
      y += 4;
      ensureSpace(false);
      pdf.font('Helvetica-Bold').fontSize(FONT_SIZE + 1).fillColor('black').text('Ringkasan', pageLeft, y + 3);
      y += ROW_HEIGHT;
      for (const item of dataset.summary) {
        ensureSpace(false);
        pdf.font('Helvetica-Bold').fontSize(FONT_SIZE).fillColor('black');
        pdf.text(`${item.label}:`, pageLeft + 2, y + 3, { width: usableWidth * 0.5, lineBreak: false });
        pdf.font('Helvetica').fontSize(FONT_SIZE);
        pdf.text(formatSummaryValue(item), pageLeft + usableWidth * 0.5, y + 3, {
          width: usableWidth * 0.5 - 4,
          align: 'right',
          lineBreak: false,
        });
        y += ROW_HEIGHT;
      }
    }

    pdf.end();
  });
}
