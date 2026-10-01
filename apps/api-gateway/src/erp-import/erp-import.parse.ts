import * as ExcelJS from 'exceljs';
import { ImportRow } from './erp-import.adapters';

export interface ParsedSheet {
  headers: string[];
  rows: ImportRow[];
}

/** Detect CSV by file extension. */
export function isCsv(fileName: string): boolean {
  return /\.csv$/i.test(fileName);
}

// ─── CSV (quote-aware single-line split) ────────────────────────────────────

function splitCsvLine(line: string): string[] {
  const out: string[] = [];
  let cur = '';
  let inQuotes = false;
  for (let i = 0; i < line.length; i += 1) {
    const ch = line[i];
    if (inQuotes) {
      if (ch === '"') {
        if (line[i + 1] === '"') {
          cur += '"';
          i += 1;
        } else {
          inQuotes = false;
        }
      } else {
        cur += ch;
      }
    } else if (ch === '"') {
      inQuotes = true;
    } else if (ch === ',') {
      out.push(cur);
      cur = '';
    } else {
      cur += ch;
    }
  }
  out.push(cur);
  return out.map((c) => c.trim());
}

function parseCsv(buffer: Buffer): ParsedSheet {
  const text = buffer.toString('utf8').replace(/^﻿/, '');
  const lines = text.split(/\r\n|\n|\r/).filter((l) => l.trim() !== '');
  if (lines.length === 0) return { headers: [], rows: [] };
  const headers = splitCsvLine(lines[0]);
  const rows: ImportRow[] = [];
  for (let i = 1; i < lines.length; i += 1) {
    const cells = splitCsvLine(lines[i]);
    const row: ImportRow = {};
    headers.forEach((h, idx) => {
      row[h] = cells[idx] ?? '';
    });
    rows.push(row);
  }
  return { headers, rows };
}

// ─── XLSX ───────────────────────────────────────────────────────────────────

function cellToString(value: ExcelJS.CellValue): string {
  if (value === null || value === undefined) return '';
  if (typeof value === 'object') {
    const v = value as { text?: string; result?: unknown; richText?: { text: string }[] };
    if (Array.isArray(v.richText)) return v.richText.map((r) => r.text).join('');
    if (typeof v.text === 'string') return v.text;
    if (v.result !== undefined) return String(v.result);
    if (value instanceof Date) return value.toISOString();
    return String(value);
  }
  return String(value);
}

async function parseXlsx(buffer: Buffer): Promise<ParsedSheet> {
  const wb = new ExcelJS.Workbook();
  await wb.xlsx.load(buffer as unknown as ArrayBuffer);
  const ws = wb.worksheets[0];
  if (!ws) return { headers: [], rows: [] };

  const headerRow = ws.getRow(1);
  const headers: string[] = [];
  headerRow.eachCell({ includeEmpty: true }, (cell, col) => {
    headers[col - 1] = cellToString(cell.value).trim();
  });
  const cleanHeaders = headers.map((h) => h ?? '').filter((_, i) => headers[i] !== undefined);

  const rows: ImportRow[] = [];
  ws.eachRow({ includeEmpty: false }, (excelRow, rowNumber) => {
    if (rowNumber === 1) return;
    const row: ImportRow = {};
    let hasValue = false;
    cleanHeaders.forEach((h, idx) => {
      if (!h) return;
      const cell = excelRow.getCell(idx + 1);
      const val = cellToString(cell.value).trim();
      row[h] = val;
      if (val !== '') hasValue = true;
    });
    if (hasValue) rows.push(row);
  });

  return { headers: cleanHeaders.filter((h) => h !== ''), rows };
}

export async function parseFile(buffer: Buffer, fileName: string): Promise<ParsedSheet> {
  if (isCsv(fileName)) return parseCsv(buffer);
  return parseXlsx(buffer);
}

// ─── Template builder ─────────────────────────────────────────────────────────

export async function buildTemplate(
  entityLabel: string,
  requiredHeaders: string[],
  optionalHeaders: string[],
): Promise<Buffer> {
  const wb = new ExcelJS.Workbook();
  const ws = wb.addWorksheet(entityLabel.slice(0, 31) || 'Template');
  const allHeaders = [...requiredHeaders, ...optionalHeaders];

  allHeaders.forEach((h, i) => {
    const cell = ws.getCell(1, i + 1);
    const isRequired = i < requiredHeaders.length;
    cell.value = isRequired ? `${h} *` : h;
    cell.font = { bold: true };
    cell.alignment = { horizontal: 'left' };
    cell.border = { bottom: { style: 'thin' } };
    ws.getColumn(i + 1).width = Math.max(14, h.length + 4);
  });

  const out = await wb.xlsx.writeBuffer();
  return Buffer.from(out);
}
