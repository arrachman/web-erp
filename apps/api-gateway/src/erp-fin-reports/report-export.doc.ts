import { ReportColumn, ReportDocument, ReportRow } from './report-types';
import { formatCell } from './report-export.util';

function esc(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}

function cellStyle(col: ReportColumn): string {
  const align = col.align ?? (col.type === 'number' ? 'right' : 'left');
  return `border:1px solid #999;padding:3px 6px;text-align:${align};`;
}

function rowHtml(columns: ReportColumn[], row: ReportRow): string {
  const weight = row.bold ? 'font-weight:bold;' : '';
  const tds = columns
    .map(
      (col) =>
        `<td style="${cellStyle(col)}${weight}">${esc(formatCell(col, row.cells[col.key]))}</td>`,
    )
    .join('');
  return `<tr>${tds}</tr>`;
}

export function renderDoc(doc: ReportDocument): Buffer {
  const columns = doc.columns;
  const headerCells = columns
    .map(
      (c) =>
        `<th style="border:1px solid #999;padding:3px 6px;background:#eee;text-align:${
          c.align ?? 'left'
        };">${esc(c.label)}</th>`,
    )
    .join('');

  const bodyParts: string[] = [];
  for (const section of doc.sections) {
    if (section.heading) {
      bodyParts.push(
        `<tr><td colspan="${columns.length}" style="border:1px solid #999;padding:3px 6px;font-weight:bold;background:#f5f5f5;">${esc(
          section.heading,
        )}</td></tr>`,
      );
    }
    for (const row of section.rows) bodyParts.push(rowHtml(columns, row));
    if (section.subtotal) bodyParts.push(rowHtml(columns, section.subtotal));
  }
  if (doc.grandTotal) bodyParts.push(rowHtml(columns, doc.grandTotal));

  const metaHtml = doc.meta.map((m) => `<div>${esc(m.label)}: ${esc(m.value)}</div>`).join('');

  const html = `<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:w="urn:schemas-microsoft-com:office:word" xmlns="http://www.w3.org/TR/REC-html40">
<head><meta charset="utf-8"><title>${esc(doc.title)}</title></head>
<body style="font-family:Arial,sans-serif;font-size:11px;">
<h2 style="margin:0;">${esc(doc.title)}</h2>
${doc.subtitle ? `<div style="font-style:italic;">${esc(doc.subtitle)}</div>` : ''}
${metaHtml}
<br/>
<table style="border-collapse:collapse;width:100%;">
<thead><tr>${headerCells}</tr></thead>
<tbody>${bodyParts.join('')}</tbody>
</table>
</body></html>`;

  return Buffer.from(html, 'utf-8');
}
