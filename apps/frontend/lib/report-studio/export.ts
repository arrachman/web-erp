import type { RsReportData } from './types';
import type { RsPreviewPage } from './pagination';
import { fmtField } from './format';

export function esc(s: unknown): string {
  return String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

export function fileName(name: string, ext: string): string {
  return (name || 'report').replace(/[^a-z0-9]+/gi, '_').replace(/^_|_$/g, '').toLowerCase() + '.' + ext;
}

export function docHead(name: string, pageW: number, pageH: number, orient: string): string {
  const sz = orient === 'landscape' ? 'landscape' : 'portrait';
  return '<!DOCTYPE html><html><head><meta charset="utf-8"><title>' + esc(name)
    + '</title><link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:wght@400;500;600;700&family=IBM+Plex+Mono:wght@400;500&display=swap" rel="stylesheet"><style>*{box-sizing:border-box;margin:0}body{font-family:\'IBM Plex Sans\',sans-serif;background:#e9ecf0;padding:24px;display:flex;flex-direction:column;align-items:center;gap:20px}.sheet{width:'
    + pageW + 'px;min-height:' + pageH + 'px;background:#fff;color:#14181f;box-shadow:0 4px 20px rgba(0,0,0,.18);display:flex;flex-direction:column;padding:40px 0}@media print{body{background:#fff;padding:0;gap:0}.sheet{box-shadow:none;margin:0;page-break-after:always}@page{size:A4 '
    + sz + ';margin:0}}</style></head><body>';
}

export function buildPagesHTML(pages: RsPreviewPage[], name: string, pageW: number, pageH: number, orient: string): string {
  let b = '';
  const band = (bi: RsPreviewPage['top'][number]) => {
    let s = '<div style="' + bi.style + '">';
    bi.els.forEach((el) => { s += '<div style="' + el.boxStyle + '">' + esc(el.display) + '</div>'; });
    return s + '</div>';
  };
  pages.forEach((pg) => {
    b += '<div class="sheet"><div>';
    pg.top.forEach((bi) => { b += band(bi); });
    b += '</div><div style="flex:1"></div><div>';
    pg.bottom.forEach((bi) => { b += band(bi); });
    b += '</div></div>';
  });
  return docHead(name, pageW, pageH, orient) + b + '</body></html>';
}

export function buildTableHTML(name: string, data: RsReportData): string {
  const rows = data.rows;
  const cols = rows.length ? Object.keys(rows[0]) : [];
  const head = cols.map((c) => '<th style="border:1px solid #c8ccd3;padding:6px 9px;background:#1f2937;color:#fff;text-align:left;font-size:12px">' + esc(c.split('.').pop()) + '</th>').join('');
  let body = '';
  rows.forEach((r) => {
    body += '<tr>' + cols.map((c) => {
      const v = r[c];
      const txt = typeof v === 'number' ? fmtField(c, v) : esc(v);
      const al = typeof v === 'number' ? 'right' : 'left';
      return '<td style="border:1px solid #d7dbe1;padding:5px 9px;font-size:12px;text-align:' + al + '">' + txt + '</td>';
    }).join('') + '</tr>';
  });
  const ctx = data.headerCtx; let ctxHTML = '';
  if (ctx && Object.keys(ctx).length) {
    ctxHTML = '<table style="margin-bottom:14px;font-size:12px">' + Object.keys(ctx).map((k) => '<tr><td style="padding:2px 12px 2px 0;color:#6b7280">' + esc(k) + '</td><td style="padding:2px 0;font-weight:600">' + esc(ctx[k]) + '</td></tr>').join('') + '</table>';
  }
  return '<!DOCTYPE html><html><head><meta charset="utf-8"><title>' + esc(name)
    + '</title></head><body style="font-family:Arial,sans-serif;padding:18px"><h2 style="margin:0 0 4px">' + esc(name)
    + '</h2><div style="color:#6b7280;font-size:12px;margin-bottom:14px">ReportStudio · ' + new Date().toLocaleDateString('id-ID')
    + '</div>' + ctxHTML + '<table style="border-collapse:collapse;width:100%"><thead><tr>' + head + '</tr></thead><tbody>' + body + '</tbody></table></body></html>';
}

export function download(name: string, mime: string, content: string): boolean {
  try {
    const blob = new Blob([content], { type: mime });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = name;
    document.body.appendChild(a); a.click(); a.remove();
    setTimeout(() => URL.revokeObjectURL(url), 3000);
    return true;
  } catch { return false; }
}

export function printHTML(html: string): void {
  try {
    const f = document.createElement('iframe');
    f.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;opacity:0';
    document.body.appendChild(f);
    const d = f.contentWindow!.document;
    d.open(); d.write(html); d.close();
    setTimeout(() => {
      try { f.contentWindow!.focus(); f.contentWindow!.print(); } catch { /* ignore */ }
      setTimeout(() => f.remove(), 2000);
    }, 500);
  } catch { /* ignore */ }
}
