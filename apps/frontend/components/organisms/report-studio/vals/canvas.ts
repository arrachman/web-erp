import type { RsCtrl } from '../hooks/use-report-studio';
import { PX_PER_CM } from '@/lib/report-studio/constants';
import { elBox, elDisplay } from '@/lib/report-studio/el-style';
import { bandLabel } from '@/lib/report-studio/i18n';

export function canvasVals(c: RsCtrl) {
  const id = c.isId; const st = c.st; const a = c.actions; const z = c.zoom;
  const PW = c.pageW; const PH = c.pageH;
  const selElId = st.selEl; const selBandId = st.selBand;
  const report = c.report;

  const paperH = report.bands.reduce((acc, b) => acc + b.h, 0);
  const paperWrapStyle = 'width:' + (PW * z) + 'px;height:' + (paperH * z) + 'px;';
  const paperStyle = 'position:relative;width:' + PW + 'px;height:' + paperH + 'px;background:#fff;box-shadow:0 8px 40px rgba(0,0,0,.22);transform:scale(' + z + ');transform-origin:top left;';
  const m = c.marginPx;
  const guides = st.guidesOn ? [
    'position:absolute;left:' + m + 'px;top:0;bottom:0;width:1px;background:repeating-linear-gradient(180deg,rgba(37,99,235,.22) 0 4px,transparent 4px 8px);pointer-events:none;z-index:3;',
    'position:absolute;left:' + (PW - m) + 'px;top:0;bottom:0;width:1px;background:repeating-linear-gradient(180deg,rgba(37,99,235,.22) 0 4px,transparent 4px 8px);pointer-events:none;z-index:3;',
  ] : [];
  const g = st.grid || 8;
  const gridImg = st.showGrid ? ('background-image:linear-gradient(to right,rgba(99,110,130,.13) 1px,transparent 1px),linear-gradient(to bottom,rgba(99,110,130,.13) 1px,transparent 1px);background-size:' + g + 'px ' + g + 'px;background-position:0 0;') : '';

  const bands = report.bands.map((b) => {
    let bs = 'position:relative;width:' + PW + 'px;height:' + b.h + 'px;border-bottom:1px dashed #c9cfd8;';
    if (b.bg) bs += 'background-color:' + b.bg + ';';
    if (gridImg) bs += gridImg;
    if (selBandId === b.id) bs += 'box-shadow:inset 0 0 0 1.5px rgba(37,99,235,.55);';
    return {
      id: b.id, label: bandLabel(b.type, id), style: bs,
      tabStyle: 'position:absolute;left:0;top:0;padding:1px 7px;font:600 8.5px/1.4 \'IBM Plex Mono\',monospace;letter-spacing:.02em;text-transform:uppercase;color:' + (selBandId === b.id ? '#fff' : '#7e879a') + ';background:' + (selBandId === b.id ? 'var(--accent,#2563eb)' : 'rgba(120,130,150,.14)') + ';border-bottom-right-radius:4px;cursor:pointer;user-select:none;z-index:5;',
      els: b.els.map((el) => ({ id: el.id, boxStyle: elBox(el, 'design', selElId === el.id), display: elDisplay(el), selected: selElId === el.id })),
    };
  });

  const rulerH: Array<{ n: number; style: string }> = [];
  for (let i = 0; i * PX_PER_CM <= PW + 1; i++) rulerH.push({ n: i, style: 'position:absolute;top:0;left:' + (i * PX_PER_CM * z) + 'px;height:16px;border-left:1px solid var(--muted,#9aa3b2);padding-left:2px;font:8px/16px \'IBM Plex Mono\',monospace;color:var(--muted,#6b7280);' });
  const rulerV: Array<{ n: number; style: string }> = [];
  for (let i = 0; i * PX_PER_CM <= paperH + 1; i++) rulerV.push({ n: i, style: 'position:absolute;left:0;top:' + (i * PX_PER_CM * z) + 'px;width:16px;border-top:1px solid var(--muted,#9aa3b2);font:7px/8px \'IBM Plex Mono\',monospace;color:var(--muted,#6b7280);text-align:center;padding-top:1px;' });
  const hRulerStyle = 'position:relative;width:' + (PW * z) + 'px;height:16px;background:var(--panel2,#eceff3);border-bottom:1px solid var(--border,#cfd5dd);';
  const vRulerStyle = 'position:relative;width:16px;height:' + (paperH * z) + 'px;background:var(--panel2,#eceff3);border-right:1px solid var(--border,#cfd5dd);';
  const previewSheetStyle = 'width:' + PW + 'px;min-height:' + PH + 'px;background:#fff;color:#14181f;box-shadow:0 8px 40px rgba(0,0,0,.28);display:flex;flex-direction:column;padding:40px 0;flex:0 0 auto;';

  return {
    rulerOn: st.rulerOn, rulerH, rulerV, hRulerStyle, vRulerStyle, paperWrapStyle, paperStyle, guides, bands, paperH,
    allowDrop: a.allowDrop, onCanvasDrop: a.onCanvasDrop, onBandMouseDown: a.onBandMouseDown, onBandLabelDown: a.onBandLabelDown,
    onBandResizeDown: a.onBandResizeDown, onElementMouseDown: a.onElementMouseDown, onResizeMouseDown: a.onResizeMouseDown,
    onCanvasWheel: a.onCanvasWheel,
    previewPages: c.previewPages(), previewSheetStyle,
  };
}
