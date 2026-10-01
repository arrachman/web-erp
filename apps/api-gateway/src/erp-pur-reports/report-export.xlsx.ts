/**
 * Excel renderer (exceljs). Single worksheet: merged title row, generated-at,
 * bold frozen header, number formats per numeric column, autofilter, then a
 * blank separator and the summary block. Values stay numeric for money/qty/
 * number/percent so Excel keeps them sortable; date/text use the shared
 * formatter for the display string.
 */

import { Workbook, Worksheet } from 'exceljs';
import { ReportColType, ReportDataset } from './report-types';
import {
  alignFor,
  excelNumFmt,
  formatCellValue,
  formatGeneratedAt,
  formatSummaryValue,
  isNumericType,
} from './report-export.format';

const HEADER_FILL = 'FFE9ECEF';
const SUMMARY_FILL = 'FFF1F3F5';

function rawNumber(value: unknown): number | null {
  if (value === null || value === undefined || value === '') return null;
  const n = typeof value === 'number' ? value : Number(value);
  return Number.isFinite(n) ? n : null;
}

function cellFor(value: unknown, type: ReportColType): number | string {
  if (isNumericType(type)) {
    const n = rawNumber(value);
    return n === null ? '' : n;
  }
  return formatCellValue(value, type);
}

function sheetName(key: string): string {
  // Excel sheet names max 31 chars and forbid : \ / ? * [ ]
  return key.replace(/[\\/?*[\]:]/g, '_').slice(0, 31) || 'Report';
}

export async function renderXlsx(dataset: ReportDataset): Promise<Buffer> {
  const wb = new Workbook();
  wb.creator = 'Sentient Factory ERP';
  wb.created = new Date(dataset.generatedAt) || new Date();
  const ws: Worksheet = wb.addWorksheet(sheetName(dataset.key), {
    views: [{ state: 'frozen', ySplit: 4 }],
  });

  const colCount = dataset.columns.length || 1;
  const lastCol = ws.getColumn(colCount).letter;

  // Row 1: title (merged across all columns)
  ws.mergeCells(`A1:${lastCol}1`);
  const titleCell = ws.getCell('A1');
  titleCell.value = dataset.title;
  titleCell.font = { bold: true, size: 14 };
  titleCell.alignment = { vertical: 'middle' };

  // Row 2: generated-at
  ws.mergeCells(`A2:${lastCol}2`);
  const genCell = ws.getCell('A2');
  genCell.value = `Dibuat: ${formatGeneratedAt(dataset.generatedAt)}`;
  genCell.font = { italic: true, size: 9 };

  // Row 3 left blank as separator; Row 4 = header.
  const headerRowIdx = 4;
  const headerRow = ws.getRow(headerRowIdx);
  dataset.columns.forEach((col, i) => {
    const cell = headerRow.getCell(i + 1);
    cell.value = col.header;
    cell.font = { bold: true };
    cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: HEADER_FILL } };
    cell.alignment = { horizontal: alignFor(col), vertical: 'middle' };
    cell.border = { bottom: { style: 'thin' } };
  });
  headerRow.commit();

  // Data rows
  let rowIdx = headerRowIdx + 1;
  for (const row of dataset.rows) {
    const r = ws.getRow(rowIdx);
    dataset.columns.forEach((col, i) => {
      const cell = r.getCell(i + 1);
      cell.value = cellFor(row[col.key], col.type);
      const fmt = excelNumFmt(col.type);
      if (fmt) cell.numFmt = fmt;
      cell.alignment = { horizontal: alignFor(col) };
    });
    r.commit();
    rowIdx += 1;
  }

  const lastDataRow = rowIdx - 1;

  // Autofilter over header + data
  if (dataset.columns.length > 0 && dataset.rows.length > 0) {
    ws.autoFilter = {
      from: { row: headerRowIdx, column: 1 },
      to: { row: lastDataRow, column: colCount },
    };
  }

  // Blank separator then summary block
  if (dataset.summary.length > 0) {
    rowIdx += 1; // blank separator row
    const sumHeader = ws.getRow(rowIdx);
    sumHeader.getCell(1).value = 'Ringkasan';
    sumHeader.getCell(1).font = { bold: true };
    rowIdx += 1;
    for (const item of dataset.summary) {
      const r = ws.getRow(rowIdx);
      const labelCell = r.getCell(1);
      labelCell.value = item.label;
      labelCell.font = { bold: true };
      labelCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: SUMMARY_FILL } };
      const valueCell = r.getCell(2);
      if (item.type && isNumericType(item.type)) {
        const n = rawNumber(item.value);
        valueCell.value = n === null ? formatSummaryValue(item) : n;
        const fmt = excelNumFmt(item.type);
        if (fmt) valueCell.numFmt = fmt;
        valueCell.alignment = { horizontal: 'right' };
      } else {
        valueCell.value = formatSummaryValue(item);
      }
      valueCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: SUMMARY_FILL } };
      r.commit();
      rowIdx += 1;
    }
  }

  // Column widths from header + content length (capped)
  dataset.columns.forEach((col, i) => {
    let max = col.header.length;
    for (const row of dataset.rows) {
      const display = formatCellValue(row[col.key], col.type);
      if (display.length > max) max = display.length;
    }
    ws.getColumn(i + 1).width = Math.min(Math.max(max + 2, 10), 50);
  });

  const out = await wb.xlsx.writeBuffer();
  return Buffer.from(out as ArrayBuffer);
}
