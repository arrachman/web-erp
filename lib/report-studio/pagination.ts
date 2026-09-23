import type { RsReport, RsReportData } from './types';
import { elBox, elPreview } from './el-style';

export interface RsPreviewBand { style: string; els: Array<{ boxStyle: string; display: string }>; }
export interface RsPreviewPage { top: RsPreviewBand[]; bottom: RsPreviewBand[]; }

export interface RsPreviewOpts {
  rows: RsReportData['rows'];
  ctx: RsReportData['headerCtx'];
  groupBy: string;
  pageW: number;
  pageH: number;
}

/** Lay out the report into paginated preview pages (group + flow rows). */
export function getPreviewPages(report: RsReport, opts: RsPreviewOpts): RsPreviewPage[] {
  const { rows: allRows, ctx, groupBy, pageW: PW, pageH: PH } = opts;
  const find = (t: string) => report.bands.find((b) => b.type === t);
  const rh = find('ReportHeader'); const ch = find('ColumnHeader'); const gh = find('GroupHeader');
  const gf = find('GroupFooter'); const det = find('Detail'); const pf = find('PageFooter'); const rf = find('ReportFooter');

  type Unit = { band: NonNullable<ReturnType<typeof find>>; row: RsReportData['rows'][number] | null; rows: RsReportData['rows'] };
  const body: Unit[] = [];
  if (groupBy && gh) {
    const sorted = [...allRows].sort((a, b) => String(a[groupBy] == null ? '' : a[groupBy]).localeCompare(String(b[groupBy] == null ? '' : b[groupBy])));
    const map = new Map<string | number, RsReportData['rows']>();
    sorted.forEach((r) => { const k = r[groupBy]; if (!map.has(k)) map.set(k, []); map.get(k)!.push(r); });
    for (const entry of map) {
      const grp = entry[1];
      body.push({ band: gh, row: grp[0], rows: grp });
      if (det) grp.forEach((r) => body.push({ band: det, row: r, rows: grp }));
      if (gf) body.push({ band: gf, row: grp[0], rows: grp });
    }
  } else if (det) {
    allRows.forEach((r) => body.push({ band: det, row: r, rows: allRows }));
  }

  const usable = PH - 80;
  const colH = ch ? ch.h : 0; const pfH = pf ? pf.h : 0; const rfH = rf ? rf.h : 0; const rhH = rh ? rh.h : 0;
  const avail = (i: number) => Math.max(60, usable - colH - pfH - rfH - (i === 0 ? rhH : 0));
  const pagesBody: Unit[][] = []; let cur: Unit[] = []; let curH = 0; let idx = 0;
  body.forEach((u) => { const h = u.band.h; if (cur.length && curH + h > avail(idx)) { pagesBody.push(cur); cur = []; curH = 0; idx++; } cur.push(u); curH += h; });
  pagesBody.push(cur);
  const pageCount = Math.max(1, pagesBody.length);

  const inst = (band: Unit['band'], row: Unit['row'], rows: RsReportData['rows'], pno: number): RsPreviewBand => ({
    style: 'position:relative;width:' + PW + 'px;height:' + band.h + 'px;' + (band.bg ? 'background:' + band.bg + ';' : ''),
    els: band.els.map((el) => ({ boxStyle: elBox(el, 'preview', false), display: elPreview(el, row, ctx, rows, pno, pageCount) })),
  });

  const pages: RsPreviewPage[] = [];
  pagesBody.forEach((list, i) => {
    const top: RsPreviewBand[] = []; const bottom: RsPreviewBand[] = [];
    if (rh && i === 0) top.push(inst(rh, null, allRows, i + 1));
    if (ch) top.push(inst(ch, null, allRows, i + 1));
    list.forEach((u) => top.push(inst(u.band, u.row, u.rows, i + 1)));
    if (rf && i === pageCount - 1) bottom.push(inst(rf, null, allRows, i + 1));
    if (pf) bottom.push(inst(pf, null, allRows, i + 1));
    pages.push({ top, bottom });
  });
  return pages;
}
