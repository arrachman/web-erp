import PDFDocument from 'pdfkit';
import { ReportColumn, ReportDocument, ReportRow } from './report-types';
import { formatCell } from './report-export.util';

const PAGE_MARGIN = 40;
const ROW_HEIGHT = 16;
const FONT_SIZE = 8;

export function renderPdf(doc: ReportDocument): Promise<Buffer> {
  return new Promise((resolve, reject) => {
    const pdf = new PDFDocument({ size: 'A4', margin: PAGE_MARGIN, layout: 'landscape' });
    const chunks: Buffer[] = [];
    pdf.on('data', (c: Buffer) => chunks.push(c));
    pdf.on('end', () => resolve(Buffer.concat(chunks)));
    pdf.on('error', reject);

    const pageLeft = PAGE_MARGIN;
    const usableWidth = pdf.page.width - PAGE_MARGIN * 2;

    // Compute column x positions from declared widths (fallback even spacing).
    const totalWeight = doc.columns.reduce((s, c) => s + (c.width ?? 1), 0);
    const colWidths = doc.columns.map((c) => ((c.width ?? 1) / totalWeight) * usableWidth);
    const colX: number[] = [];
    let acc = pageLeft;
    for (const w of colWidths) {
      colX.push(acc);
      acc += w;
    }

    // Title block
    pdf.fontSize(14).font('Helvetica-Bold').text(doc.title, pageLeft);
    if (doc.subtitle) pdf.fontSize(9).font('Helvetica-Oblique').text(doc.subtitle, pageLeft);
    pdf.fontSize(8).font('Helvetica');
    for (const m of doc.meta) pdf.text(`${m.label}: ${m.value}`, pageLeft);
    pdf.moveDown(0.5);

    let y = pdf.y;

    const ensureSpace = () => {
      if (y + ROW_HEIGHT > pdf.page.height - PAGE_MARGIN) {
        pdf.addPage();
        y = PAGE_MARGIN;
      }
    };

    const drawCell = (col: ReportColumn, x: number, w: number, text: string, bold: boolean) => {
      pdf.font(bold ? 'Helvetica-Bold' : 'Helvetica').fontSize(FONT_SIZE);
      pdf.text(text, x + 2, y + 3, {
        width: w - 4,
        align: col.align ?? 'left',
        ellipsis: true,
        lineBreak: false,
      });
    };

    const drawRow = (cells: Record<string, string | number | null>, bold: boolean) => {
      ensureSpace();
      doc.columns.forEach((col, i) => {
        drawCell(col, colX[i], colWidths[i], formatCell(col, cells[col.key]), bold);
      });
      y += ROW_HEIGHT;
    };

    const drawHeading = (text: string) => {
      ensureSpace();
      pdf.font('Helvetica-Bold').fontSize(FONT_SIZE).text(text, pageLeft, y + 3);
      y += ROW_HEIGHT;
    };

    const drawLine = () => {
      pdf.moveTo(pageLeft, y).lineTo(pageLeft + usableWidth, y).lineWidth(0.5).stroke();
    };

    // Column header
    doc.columns.forEach((col, i) => {
      pdf.font('Helvetica-Bold').fontSize(FONT_SIZE);
      pdf.text(col.label, colX[i] + 2, y + 3, {
        width: colWidths[i] - 4,
        align: col.align ?? 'left',
        lineBreak: false,
      });
    });
    y += ROW_HEIGHT;
    drawLine();

    const drawReportRow = (row: ReportRow) => drawRow(row.cells, !!row.bold);

    for (const section of doc.sections) {
      if (section.heading) drawHeading(section.heading);
      for (const row of section.rows) drawReportRow(row);
      if (section.subtotal) drawReportRow(section.subtotal);
    }

    if (doc.grandTotal) {
      drawLine();
      drawRow(doc.grandTotal.cells, true);
    }

    pdf.end();
  });
}
