import type * as React from 'react';
import type { RsCtrl } from '../hooks/use-report-studio';
import type { RsElement, RsRibbon } from '@/lib/report-studio/types';
import { SWATCHES } from '@/lib/report-studio/constants';
import { FMT_SAMPLES, fmtLabel } from '@/lib/report-studio/i18n';
import {
  rtBase, rGroup, rLabel, rDiv, rBtn, rBtnWide, ribbonSelectWide, ribbonSelectSm,
  tgl, wideTgl, pasteBtnStyle, colorPopStyle, bar, swatchStyle,
} from './styles';

type SelEvt = React.ChangeEvent<HTMLSelectElement>;

export function ribbonVals(c: RsCtrl) {
  const id = c.isId; const st = c.st; const a = c.actions; const selObj = c.selObj;
  const sel = (): RsElement | null => c.findEl(st.selEl);
  const togF = (f: keyof RsElement) => () => { const el = sel(); if (el) c.editEl({ [f]: !el[f] } as Partial<RsElement>); };
  const swatch = (field: keyof RsElement, col: string) => ({
    onClick: () => c.editEl({ [field]: col } as Partial<RsElement>),
    style: swatchStyle(col, !!(selObj && selObj[field] === col)),
  });
  const al = (v: string) => tgl(!!(selObj && selObj.align === v));
  const va = (v: string) => tgl(!!(selObj && selObj.valign === v));
  const bd = (f: keyof RsElement) => tgl(!!(selObj && selObj[f]));

  const ribTabs: Array<[RsRibbon, string]> = [['home', 'Home'], ['page', id ? 'Halaman' : 'Page'], ['layout', 'Layout'], ['view', id ? 'Tampilan' : 'View']];
  const ribbonTabs = ribTabs.map((x) => {
    const on = st.ribbon === x[0];
    return { label: x[1], onClick: () => c.set({ ribbon: x[0], menu: null }), style: rtBase + (on ? 'background:var(--ribbon,#fafbfc);color:var(--accent,#2563eb);box-shadow:inset 0 -2px 0 var(--accent,#2563eb);' : 'color:var(--muted,#6b7280);') };
  });

  const fmtVal = selObj ? (selObj.format || 'General') : 'General';
  const orientCenter = (s: string) => s.replace('display:flex;align-items:center;gap:7px;height:24px;padding:0 9px;', 'display:flex;align-items:center;justify-content:center;');

  return {
    ribbonTabs, isHome: st.ribbon === 'home', isPage: st.ribbon === 'page', isLayout: st.ribbon === 'layout', isView: st.ribbon === 'view',
    rGroup, rLabel, rDiv, rBtn, rBtnWide, ribbonSelectWide, ribbonSelectSm, colorPopStyle, pasteBtnStyle: pasteBtnStyle(st.clipHas),

    // clipboard
    doPaste: a.doPaste, doCut: a.doCut, doCopy: a.doCopy, delSel: a.delSel,

    // font
    fontFamilyOpts: ['', 'IBM Plex Sans', 'Arial', 'Times New Roman', 'Courier New', 'Georgia', 'Verdana', 'Tahoma'].map((f) => ({ v: f, label: f === '' ? 'Default' : f })),
    fontFamilyVal: selObj ? (selObj.font || '') : '',
    fontSizeOpts: [6, 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 40, 48].map((n) => ({ v: String(n), label: String(n) })),
    fontSizeVal: selObj ? String(selObj.size || 10) : '10',
    onFontFamily: (e: SelEvt) => c.editEl({ font: e.target.value }),
    onFontSizeSel: (e: SelEvt) => c.editEl({ size: parseFloat(e.target.value) || 10 }),
    toggleBold: togF('bold'), biBold: tgl(!!(selObj && selObj.bold)),
    toggleItalic: togF('italic'), biItalic: tgl(!!(selObj && selObj.italic)),
    toggleUnderline: togF('underline'), biUnder: tgl(!!(selObj && selObj.underline)),
    toggleStrike: togF('strike'), biStrike: tgl(!!(selObj && selObj.strike)),
    fontGrow: () => { const el = sel(); if (el) c.editEl({ size: Math.min(96, (el.size || 10) + 1) }); },
    fontShrink: () => { const el = sel(); if (el) c.editEl({ size: Math.max(5, (el.size || 10) - 1) }); },
    openFontColor: () => c.set((s) => ({ menu: s.menu === 'fc' ? null : 'fc' })), menuFontColor: st.menu === 'fc',
    fontColorBar: bar(selObj?.color), swColor: SWATCHES.map((col) => swatch('color', col)),

    // alignment
    setVTop: () => c.editEl({ valign: 'top' }), setVMid: () => c.editEl({ valign: 'middle' }), setVBot: () => c.editEl({ valign: 'bottom' }),
    vT: va('top'), vM: va('middle'), vB: va('bottom'),
    toggleWrap: togF('wordWrap'), biWrap: tgl(!!(selObj && selObj.wordWrap)),
    setAlignL: () => c.editEl({ align: 'left' }), setAlignC: () => c.editEl({ align: 'center' }), setAlignR: () => c.editEl({ align: 'right' }), setAlignJ: () => c.editEl({ align: 'justify' }),
    alL: al('left'), alC: al('center'), alR: al('right'), alJ: al('justify'),

    // borders
    brdAll: () => c.editEl({ bTop: true, bBottom: true, bLeft: true, bRight: true }),
    brdBox: () => c.editEl({ bTop: true, bBottom: true, bLeft: true, bRight: true }),
    brdNone: () => c.editEl({ bTop: false, bBottom: false, bLeft: false, bRight: false }),
    brdTop: togF('bTop'), brdBottom: togF('bBottom'), brdLeft: togF('bLeft'), brdRight: togF('bRight'),
    bT: bd('bTop'), bB: bd('bBottom'), bL: bd('bLeft'), bR: bd('bRight'),
    openFillColor: () => c.set((s) => ({ menu: s.menu === 'fl' ? null : 'fl' })), menuFillColor: st.menu === 'fl', fillColorBar: bar(selObj?.bg), swFill: SWATCHES.map((col) => swatch('bg', col)),
    openLineColor: () => c.set((s) => ({ menu: s.menu === 'ln' ? null : 'ln' })), menuLineColor: st.menu === 'ln', lineColorBar: bar(selObj?.bColor), swLine: SWATCHES.map((col) => swatch('bColor', col)),
    lineWidthOpts: [1, 2, 3, 4].map((n) => ({ v: String(n), label: n + 'px' })), lineWidthVal: selObj ? String(selObj.bWidth || 1) : '1',
    onLineWidth: (e: SelEvt) => c.editEl({ bWidth: parseInt(e.target.value) || 1 }),

    // text format + style
    fmtOpts: ['General', 'Number', 'Currency', 'Date', 'Time', 'Percentage'].map((f) => ({ v: f, label: fmtLabel(f, id) })),
    fmtVal, fmtSample: FMT_SAMPLES[fmtVal] || 'Abc 123', onFormat: (e: SelEvt) => c.editEl({ format: e.target.value }),
    styleVal: '', onStyleApply: (e: SelEvt) => { const stl = c.STYLES[e.target.value]; if (stl && st.selEl) c.editEl({ ...stl }); },
    styleOpts: [{ v: '', label: id ? '(pilih gaya)' : '(pick style)' }].concat(Object.keys(c.STYLES).map((k) => ({ v: k, label: k }))),

    // page
    pageSizeOpts: [['a4', 'A4'], ['letter', 'Letter'], ['legal', 'Legal']].map((x) => ({ v: x[0], label: x[1] })), pageSizeVal: st.pageSize,
    onPageSize: (e: SelEvt) => c.set({ pageSize: e.target.value }),
    marginOpts: [['normal', id ? 'Normal' : 'Normal'], ['narrow', id ? 'Sempit' : 'Narrow'], ['wide', id ? 'Lebar' : 'Wide']].map((x) => ({ v: x[0], label: x[1] })), marginVal: st.margin,
    onMargin: (e: SelEvt) => c.set({ margin: e.target.value }),
    setOrientP: () => c.set({ orient: 'portrait' }), setOrientL: () => c.set({ orient: 'landscape' }),
    orientP: orientCenter(wideTgl(st.orient === 'portrait')), orientL: orientCenter(wideTgl(st.orient === 'landscape')),
    toggleGuides: () => c.set((s) => ({ guidesOn: !s.guidesOn })), guidesBtn: wideTgl(st.guidesOn), guidesLabel: (id ? 'Margin' : 'Guides') + (st.guidesOn ? ' ✓' : ''),
    toggleRuler: () => c.set((s) => ({ rulerOn: !s.rulerOn })), rulerBtnWide: wideTgl(st.rulerOn), rulerLabel: (id ? 'Penggaris' : 'Rulers') + (st.rulerOn ? ' ✓' : ''),

    // layout
    toggleSnap: () => c.set((s) => ({ snap: !s.snap })), snapBtn: wideTgl(st.snap),
    toggleShowGrid: () => c.set((s) => ({ showGrid: !s.showGrid })), gridBtn: wideTgl(st.showGrid),
    gridSizeOpts: [4, 8, 10, 16].map((n) => ({ v: String(n), label: n + 'px' })), gridSizeVal: String(st.grid),
    onGridSize: (e: SelEvt) => c.set({ grid: parseInt(e.target.value) || 8 }),
    alignPageL: () => { const el = sel(); if (el) c.editEl({ x: 0 }); },
    alignPageC: () => { const el = sel(); if (el) c.editEl({ x: Math.round((c.pageW - el.w) / 2) }); },
    alignPageR: () => { const el = sel(); if (el) c.editEl({ x: c.pageW - el.w }); },
    snapSel: () => { const el = sel(); if (el) c.editEl({ x: c.snapV(el.x), y: c.snapV(el.y), w: c.snapV(el.w), h: c.snapV(el.h) }); },
    zFront: () => a.zMove(true), zBack: () => a.zMove(false),

    // view
    zoomIn: () => c.set((s) => ({ zoom: Math.min(4, Math.round(((s.zoom || 1) + 0.1) * 10) / 10) })),
    zoomOut: () => c.set((s) => ({ zoom: Math.max(0.4, Math.round(((s.zoom || 1) - 0.1) * 10) / 10) })),
    zoom100: () => c.set({ zoom: 1 }), zoomFit: () => c.set({ zoom: 0.7 }), zoomPct: Math.round(c.zoom * 100) + '%',
    toggleLeftPanel: () => c.set((s) => ({ leftOpen: !s.leftOpen })), dataPanelBtn: wideTgl(st.leftOpen), dataPanelLabel: (id ? 'Panel Data' : 'Data Panel') + (st.leftOpen ? ' ✓' : ''),
  };
}
