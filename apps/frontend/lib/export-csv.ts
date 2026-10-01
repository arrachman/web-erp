/**
 * Reusable client-side CSV export for list/register pages.
 *
 * Generates a UTF-8 CSV (BOM-prefixed so Excel reads Indonesian characters
 * correctly) from an array of rows + a column spec, then triggers a download.
 * Use for read-only registers and simple list exports; server-side report
 * export (xlsx/pdf) stays in the dedicated report endpoints.
 */

export interface CsvColumn<Row> {
  /** Column header text. */
  header: string;
  /** Cell value extractor — return string | number | null. */
  value: (row: Row) => string | number | null | undefined;
}

/** Escape a single CSV field per RFC 4180 (quote when it contains , " or newline). */
function escapeField(raw: string | number | null | undefined): string {
  const s = raw == null ? '' : String(raw);
  if (/[",\n\r]/.test(s)) return `"${s.replace(/"/g, '""')}"`;
  return s;
}

/** Build CSV text (with header row) from rows + columns. */
export function rowsToCsv<Row>(rows: Row[], columns: CsvColumn<Row>[]): string {
  const head = columns.map((c) => escapeField(c.header)).join(',');
  const body = rows
    .map((r) => columns.map((c) => escapeField(c.value(r))).join(','))
    .join('\r\n');
  return body ? `${head}\r\n${body}` : head;
}

/** Trigger a browser download of `text` as `filename`. */
export function downloadTextFile(filename: string, text: string, mime = 'text/csv;charset=utf-8'): void {
  const blob = new Blob(['﻿', text], { type: mime });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

/** One-call helper: build CSV from rows/columns and download it. */
export function exportRowsToCsv<Row>(
  filename: string,
  rows: Row[],
  columns: CsvColumn<Row>[],
): void {
  downloadTextFile(filename, rowsToCsv(rows, columns));
}
