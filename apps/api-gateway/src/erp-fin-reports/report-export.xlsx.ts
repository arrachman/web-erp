import * as ExcelJS from 'exceljs';
import { ReportDocument, ReportRow } from './report-types';

const NUM_FMT = '#,##0.00';

export async function renderXlsx(doc: ReportDocument): Promise<Buffer> {
  const wb = new ExcelJS.Workbook();
  const ws = wb.addWorksheet(doc.title.slice(0, 31));
  const colCount = doc.columns.length;

  const numberKeys = new Set(doc.columns.filter((c) => c.type === 'number').map((c) => c.key));

  const mergeAcross = (rowIndex: number) => {
    if (colCount > 1) ws.mergeCells(rowIndex, 1, rowIndex, colCount);
  };

  // Title
  let r = 1;
  ws.getCell(r, 1).value = doc.title;
  ws.getCell(r, 1).font = { bold: true, size: 14 };
  mergeAcross(r);
  r += 1;

  if (doc.subtitle) {
    ws.getCell(r, 1).value = doc.subtitle;
    ws.getCell(r, 1).font = { italic: true };
    mergeAcross(r);
    r += 1;
  }

  for (const m of doc.meta) {
    ws.getCell(r, 1).value = `${m.label}: ${m.value}`;
    mergeAcross(r);
    r += 1;
  }
  r += 1; // spacer

  // Header row
  doc.columns.forEach((col, i) => {
    const cell = ws.getCell(r, i + 1);
    cell.value = col.label;
    cell.font = { bold: true };
    cell.alignment = { horizontal: col.align ?? 'left' };
    cell.border = { bottom: { style: 'thin' } };
    if (col.width) ws.getColumn(i + 1).width = col.width;
  });
  r += 1;

  const writeDataRow = (row: ReportRow, bold = false) => {
    doc.columns.forEach((col, i) => {
      const cell = ws.getCell(r, i + 1);
      const val = row.cells[col.key];
      cell.value = val ?? '';
      cell.alignment = { horizontal: col.align ?? 'left' };
      if (numberKeys.has(col.key) && typeof val === 'number') cell.numFmt = NUM_FMT;
      if (bold || row.bold) cell.font = { bold: true };
    });
    r += 1;
  };

  for (const section of doc.sections) {
    if (section.heading) {
      ws.getCell(r, 1).value = section.heading;
      ws.getCell(r, 1).font = { bold: true };
      mergeAcross(r);
      r += 1;
    }
    for (const row of section.rows) writeDataRow(row);
    if (section.subtotal) writeDataRow(section.subtotal, true);
  }

  if (doc.grandTotal) {
    doc.columns.forEach((col, i) => {
      const cell = ws.getCell(r, i + 1);
      const val = doc.grandTotal!.cells[col.key];
      cell.value = val ?? '';
      cell.alignment = { horizontal: col.align ?? 'left' };
      cell.font = { bold: true };
      cell.border = { top: { style: 'thin' } };
      if (numberKeys.has(col.key) && typeof val === 'number') cell.numFmt = NUM_FMT;
    });
    r += 1;
  }

  const arrayBuffer = await wb.xlsx.writeBuffer();
  return Buffer.from(arrayBuffer);
}
