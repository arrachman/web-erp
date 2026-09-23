'use client';

import * as React from 'react';
import type {
  RsReport, RsElement, RsBand, RsTplKey, RsView, RsLeftTab, RsRightTab,
  RsRibbon, RsTheme, RsLang, RsDrag,
} from '@/lib/report-studio/types';
import { GROUP_DEF, STYLES, PAGE_DIMS, MARGINS } from '@/lib/report-studio/constants';
import { buildReport, createFactories } from '@/lib/report-studio/templates';
import { buildData } from '@/lib/report-studio/data';
import { getPreviewPages } from '@/lib/report-studio/pagination';
import {
  buildPagesHTML, buildTableHTML, download, printHTML, fileName,
} from '@/lib/report-studio/export';
import { defName } from '@/lib/report-studio/i18n';
import { reportFromTemplateJson } from '@/lib/report-studio/template-io';
import { listReportTemplates, getReportTemplate, updateReportTemplate, executeSqlQuery } from '@/lib/api/reports';
import type { RsReportData } from '@/lib/report-studio/types';

export interface RsState {
  report: RsReport; tplKey: RsTplKey; selEl: string | null; selBand: string | null;
  view: RsView; leftTab: RsLeftTab; leftOpen: boolean; zoom: number;
  theme: RsTheme; lang: RsLang; accent: string;
  openTables: Record<string, boolean>; relOpt: Record<string, boolean>;
  reportName: string | null; toast: string | null; expOpen: boolean; groupBy: string;
  ribbon: RsRibbon; rightTab: RsRightTab; menu: string | null;
  snap: boolean; grid: number; showGrid: boolean; rulerOn: boolean; guidesOn: boolean;
  pageSize: string; orient: string; margin: string;
  propOpen: Record<string, boolean>; clipHas: boolean; undoN: number; redoN: number;
  tplList: Array<{ id: string; name: string }>; currentId: string | null;
  sqlRows: Array<Record<string, unknown>> | null; sqlCols: string[]; sqlErr: string | null; sqlLoading: boolean;
}

type Patch = Partial<RsState> | ((s: RsState) => Partial<RsState>);

// Monotonic element/band id counter. Module-scoped so ids stay unique across
// renders and instances without needing a ref accessed during render.
let RS_UID = 0;
const nextUid = () => ++RS_UID;

export function useReportStudio() {
  const nextId = nextUid;

  const [st, setStRaw] = React.useState<RsState>(() => {
    const tpl: RsTplKey = 'invoice';
    return {
      report: buildReport(tpl, nextUid), tplKey: tpl, selEl: null, selBand: null,
      view: 'design', leftTab: 'data', leftOpen: true, zoom: 1, theme: 'light', lang: 'id', accent: '#2563eb',
      openTables: { Customers: true, Invoices: true, InvoiceLines: true, Accounts: true }, relOpt: {},
      reportName: null, toast: null, expOpen: false, groupBy: GROUP_DEF[tpl] || '',
      ribbon: 'home', rightTab: 'props', menu: null, snap: true, grid: 8, showGrid: true, rulerOn: true, guidesOn: true,
      pageSize: 'a4', orient: 'portrait', margin: 'normal',
      propOpen: { pos: true, text: true, appearance: true, behavior: false }, clipHas: false, undoN: 0, redoN: 0,
      tplList: [], currentId: null,
      sqlRows: null, sqlCols: [], sqlErr: null, sqlLoading: false,
    };
  });
  const stRef = React.useRef(st);
  React.useEffect(() => { stRef.current = st; });
  const set = React.useCallback((p: Patch) => setStRaw((s) => ({ ...s, ...(typeof p === 'function' ? p(s) : p) })), []);

  const undoRef = React.useRef<string[]>([]);
  const redoRef = React.useRef<string[]>([]);
  const clipRef = React.useRef<string | null>(null);
  const dragRef = React.useRef<RsDrag | null>(null);
  const toastTimer = React.useRef<ReturnType<typeof setTimeout> | undefined>(undefined);

  // ---------- pure state helpers (safe to call during render) ----------
  const dimsOf = (s: RsState): [number, number] => {
    const d = PAGE_DIMS[s.pageSize] || PAGE_DIMS.a4;
    return s.orient === 'landscape' ? [d[1], d[0]] : [d[0], d[1]];
  };
  const effNameOf = (s: RsState) => (s.reportName !== null ? s.reportName : defName(s.tplKey, s.lang === 'id'));

  // ---------- getters (read live ref; call only in handlers/effects) ----------
  const isId = () => stRef.current.lang === 'id';
  const effZoom = () => stRef.current.zoom || 1;
  const pageW = () => dimsOf(stRef.current)[0];
  const snapV = (v: number) => { const g = stRef.current.grid || 8; return stRef.current.snap ? Math.round(v / g) * g : Math.round(v); };

  // ---------- lookups & mutations ----------
  const findEl = (id: string | null): RsElement | null => {
    const r = stRef.current.report; if (!id) return null;
    for (const b of r.bands) { const e = b.els.find((x) => x.id === id); if (e) return e; } return null;
  };
  const findBand = (id: string | null) => (id ? stRef.current.report.bands.find((b) => b.id === id) || null : null);
  const findBandOfEl = (id: string | null) => (id ? stRef.current.report.bands.find((b) => b.els.some((x) => x.id === id)) || null : null);
  const updateEl = (id: string, patch: Partial<RsElement>) =>
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((b) => ({ ...b, els: b.els.map((e) => (e.id === id ? { ...e, ...patch } : e)) })) } }));
  const updateBand = (id: string, patch: Partial<RsBand>) =>
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((b) => (b.id === id ? { ...b, ...patch } : b)) } }));

  const pushUndo = React.useCallback(() => {
    redoRef.current = [];
    undoRef.current.push(JSON.stringify(stRef.current.report));
    if (undoRef.current.length > 60) undoRef.current.shift();
    set({ undoN: undoRef.current.length, redoN: 0 });
  }, [set]);
  const editEl = (patch: Partial<RsElement>) => { if (!stRef.current.selEl) return; pushUndo(); updateEl(stRef.current.selEl, patch); };

  const undo = () => {
    if (!undoRef.current.length) return;
    redoRef.current.push(JSON.stringify(stRef.current.report));
    const r = JSON.parse(undoRef.current.pop()!);
    set({ report: r, selEl: null, selBand: null, undoN: undoRef.current.length, redoN: redoRef.current.length });
  };
  const redo = () => {
    if (!redoRef.current.length) return;
    undoRef.current.push(JSON.stringify(stRef.current.report));
    const r = JSON.parse(redoRef.current.pop()!);
    set({ report: r, selEl: null, selBand: null, undoN: undoRef.current.length, redoN: redoRef.current.length });
  };

  const loadTemplate = (key: RsTplKey, keepView: boolean) => {
    const r = buildReport(key, nextId); r.key = key;
    undoRef.current = []; redoRef.current = [];
    set((s) => ({ report: r, tplKey: key, currentId: null, selEl: null, selBand: null, reportName: null, groupBy: GROUP_DEF[key] || '', view: keepView ? s.view : 'design', undoN: 0, redoN: 0 }));
  };

  // Load a real saved template from the reports API (native JSON or starter layout).
  const selectTemplate = (id: string) => {
    getReportTemplate(id).then((rec) => {
      const r = reportFromTemplateJson(rec.templateJson, rec.module, nextId);
      const key = (r.key as RsTplKey) || 'invoice';
      undoRef.current = []; redoRef.current = [];
      set({ report: r, currentId: id, tplKey: key, selEl: null, selBand: null, reportName: rec.name, groupBy: GROUP_DEF[key] || '', undoN: 0, redoN: 0, view: 'design' });
    }).catch((e) => toast(e instanceof Error ? e.message : String(e)));
  };

  // Persist the current design back to its template record (RsReport → templateJson).
  const save = () => {
    const s = stRef.current;
    if (!s.currentId) { toast(s.lang === 'id' ? 'Pilih template dulu untuk menyimpan' : 'Pick a template to save'); return; }
    updateReportTemplate(s.currentId, { templateJson: s.report as unknown as Record<string, unknown>, name: effNameOf(s) })
      .then(() => toast(s.lang === 'id' ? 'Template disimpan' : 'Template saved'))
      .catch((e) => toast(e instanceof Error ? e.message : String(e)));
  };

  const toast = (msg: string) => {
    set({ toast: msg, expOpen: false });
    clearTimeout(toastTimer.current);
    toastTimer.current = setTimeout(() => set({ toast: null }), 1900);
  };

  // ---------- clipboard ----------
  const doCopy = () => { const el = findEl(stRef.current.selEl); if (el) { clipRef.current = JSON.stringify(el); set({ clipHas: true }); toast(isId() ? 'Disalin' : 'Copied'); } };
  const doCut = () => {
    const el = findEl(stRef.current.selEl); if (!el) return;
    clipRef.current = JSON.stringify(el); pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((b) => ({ ...b, els: b.els.filter((e) => e.id !== el.id) })) }, selEl: null, clipHas: true }));
    toast(isId() ? 'Dipotong' : 'Cut');
  };
  const doPaste = () => {
    if (!clipRef.current) return;
    const src: RsElement = JSON.parse(clipRef.current);
    const s0 = stRef.current;
    const bId = s0.selBand || findBandOfEl(s0.selEl)?.id || s0.report.bands[0].id;
    const b = findBand(bId); const g = s0.grid || 8;
    const ne: RsElement = { ...src, id: 'e' + nextId(), x: Math.min(pageW() - src.w, snapV(src.x + g)), y: snapV(src.y + g) };
    if (b) ne.y = Math.min(Math.max(0, b.h - ne.h), ne.y);
    pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((bb) => (bb.id === bId ? { ...bb, els: [...bb.els, ne] } : bb)) }, selEl: ne.id, selBand: null }));
    toast(isId() ? 'Ditempel' : 'Pasted');
  };
  const dupSel = () => {
    const el = findEl(stRef.current.selEl); const b = findBandOfEl(stRef.current.selEl); if (!el || !b) return;
    const g = stRef.current.grid || 8;
    const ne: RsElement = { ...el, id: 'e' + nextId(), x: Math.min(pageW() - el.w, el.x + g), y: Math.min(Math.max(0, b.h - el.h), el.y + g) };
    pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((bb) => (bb.id === b.id ? { ...bb, els: [...bb.els, ne] } : bb)) }, selEl: ne.id }));
  };
  const delSel = () => {
    const id = stRef.current.selEl; if (!id) return; pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((b) => ({ ...b, els: b.els.filter((e) => e.id !== id) })) }, selEl: null }));
  };

  // ---------- drag / move / resize ----------
  const onMove = (e: MouseEvent) => {
    const d = dragRef.current; if (!d) return;
    const z = effZoom(); const dx = (e.clientX - (d.sx || 0)) / z; const dy = (e.clientY - d.sy) / z; const PW = pageW();
    if (d.mode === 'move') {
      let x = snapV((d.ox || 0) + dx); let y = snapV((d.oy || 0) + dy);
      x = Math.max(0, Math.min(PW - (d.w || 0), x)); y = Math.max(0, Math.min(Math.max(0, (d.bandH || 0) - (d.h || 0)), y));
      updateEl(d.id, { x, y });
    } else if (d.mode === 'resize') {
      let w = snapV((d.ow || 0) + dx); let h = snapV((d.oh || 0) + dy);
      w = Math.max(8, Math.min(PW - (d.ox || 0), w)); h = Math.max(6, Math.min((d.bandH || 0) - (d.oy || 0), h));
      updateEl(d.id, { w, h });
    } else if (d.mode === 'bandH') {
      updateBand(d.id, { h: Math.max(18, snapV((d.oh || 0) + dy)) });
    }
  };
  const onElementMouseDown = (e: React.MouseEvent) => {
    e.stopPropagation(); const id = (e.currentTarget as HTMLElement).dataset.id!; const el = findEl(id); const b = findBandOfEl(id); if (!el || !b) return;
    pushUndo(); set({ selEl: id, selBand: null });
    dragRef.current = { mode: 'move', id, sx: e.clientX, sy: e.clientY, ox: el.x, oy: el.y, w: el.w, h: el.h, bandH: b.h }; e.preventDefault();
  };
  const onResizeMouseDown = (e: React.MouseEvent) => {
    e.stopPropagation(); const id = (e.currentTarget as HTMLElement).dataset.id!; const el = findEl(id); const b = findBandOfEl(id); if (!el || !b) return;
    pushUndo(); set({ selEl: id, selBand: null });
    dragRef.current = { mode: 'resize', id, sx: e.clientX, sy: e.clientY, ow: el.w, oh: el.h, ox: el.x, oy: el.y, bandH: b.h }; e.preventDefault();
  };
  const onBandResizeDown = (e: React.MouseEvent) => {
    e.stopPropagation(); const id = (e.currentTarget as HTMLElement).dataset.band!; const b = findBand(id); if (!b) return;
    pushUndo(); set({ selBand: id, selEl: null });
    dragRef.current = { mode: 'bandH', id, sy: e.clientY, oh: b.h }; e.preventDefault();
  };
  const onBandLabelDown = (e: React.MouseEvent) => { e.stopPropagation(); set({ selBand: (e.currentTarget as HTMLElement).dataset.band!, selEl: null }); };
  const onBandMouseDown = (e: React.MouseEvent) => { if (e.target === e.currentTarget) set({ selEl: null, selBand: null }); };

  const onFieldDragStart = (e: React.DragEvent) => { const p = (e.currentTarget as HTMLElement).dataset.path!; e.dataTransfer.setData('text/plain', p); e.dataTransfer.effectAllowed = 'copy'; };
  const allowDrop = (e: React.DragEvent) => { e.preventDefault(); try { e.dataTransfer.dropEffect = 'copy'; } catch { /* ignore */ } };
  const onCanvasDrop = (e: React.DragEvent) => {
    e.preventDefault(); const bandId = (e.currentTarget as HTMLElement).dataset.band!; const path = e.dataTransfer.getData('text/plain'); if (!path || !bandId) return;
    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect(); const z = effZoom();
    let kind: RsElement['kind'] = 'field'; let bind = path; let w = 150; const h = 18;
    if (path.indexOf('expr:') === 0) { kind = 'expr'; bind = path.slice(5); w = 150; }
    else if (path.indexOf('param:') === 0) { kind = 'field'; bind = path.slice(6); w = 130; }
    let x = snapV((e.clientX - rect.left) / z - w / 2); let y = snapV((e.clientY - rect.top) / z - h / 2);
    const b = findBand(bandId); const bh = b ? b.h : 24; const PW = pageW();
    x = Math.max(0, Math.min(PW - w, x)); y = Math.max(0, Math.min(Math.max(0, bh - h), y));
    const { E } = createFactories(nextId); const el = E(kind, x, y, w, h, { bind, size: 10 });
    pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((bb) => (bb.id === bandId ? { ...bb, els: [...bb.els, el] } : bb)) }, selEl: el.id, selBand: null }));
  };

  // ---------- zoom (ribbon buttons + ctrl/cmd-wheel on canvas) ----------
  const ZOOM_MIN = 0.4; const ZOOM_MAX = 4;
  const setZoom = (z: number) => set({ zoom: Math.min(ZOOM_MAX, Math.max(ZOOM_MIN, Math.round(z * 100) / 100)) });
  // Ctrl/Cmd + wheel: zoom toward the cursor by adjusting scroll after the resize.
  const onCanvasWheel = (el: HTMLElement, clientX: number, clientY: number, deltaY: number) => {
    const old = stRef.current.zoom || 1;
    const next = Math.min(ZOOM_MAX, Math.max(ZOOM_MIN, Math.round(old * (deltaY < 0 ? 1.1 : 1 / 1.1) * 100) / 100));
    if (next === old) return;
    const rect = el.getBoundingClientRect();
    const cx = el.scrollLeft + (clientX - rect.left); const cy = el.scrollTop + (clientY - rect.top);
    const ratio = next / old;
    setZoom(next);
    requestAnimationFrame(() => { el.scrollLeft = cx * ratio - (clientX - rect.left); el.scrollTop = cy * ratio - (clientY - rect.top); });
  };

  // ---------- element creation ----------
  const addElement = (kind: 'label' | 'line' | 'box') => {
    const s0 = stRef.current;
    const bId = s0.selBand || findBandOfEl(s0.selEl)?.id || s0.report.bands[0].id;
    const { E } = createFactories(nextId);
    const el = kind === 'label' ? E('label', snapV(60), snapV(8), 160, 18, { text: isId() ? 'Teks baru' : 'New text', size: 11 })
      : kind === 'line' ? E('line', snapV(60), snapV(10), 260, 2, { bg: '#1f2937' })
        : E('box', snapV(60), snapV(8), 160, 40, { color: '#1f2937' });
    pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((b) => (b.id === bId ? { ...b, els: [...b.els, el] } : b)) }, selEl: el.id, selBand: null }));
  };

  // ---------- z-order ----------
  const zMove = (toFront: boolean) => {
    const id = stRef.current.selEl; const b = findBandOfEl(id); if (!b || !id) return; pushUndo();
    set((s) => ({ report: { ...s.report, bands: s.report.bands.map((bb) => {
      if (bb.id !== b.id) return bb; const el = bb.els.find((e) => e.id === id)!; const rest = bb.els.filter((e) => e.id !== id);
      return { ...bb, els: toFront ? [...rest, el] : [el, ...rest] };
    }) } }));
  };

  // ---------- group-by ----------
  const onGroupBy = (v: string) => {
    pushUndo(); set({ groupBy: v });
    const gh = stRef.current.report.bands.find((b) => b.type === 'GroupHeader');
    if (gh && v) { const f = gh.els.find((x) => x.kind === 'field'); if (f) updateEl(f.id, { bind: v }); }
  };

  // ---------- exports ----------
  // Pure of state so the render path (canvasVals) computes from current `st`,
  // while handlers pass the live `stRef.current`.
  // Effective preview dataset: live SQL rows when a query is set & loaded, else sample data.
  const effectiveData = (s: RsState): RsReportData =>
    (s.report.sql && s.sqlRows ? { headerCtx: {}, rows: s.sqlRows as unknown as RsReportData['rows'] } : buildData(s.tplKey));
  const previewPagesOf = (s: RsState) => {
    const d = effectiveData(s);
    return getPreviewPages(s.report, { rows: d.rows, ctx: d.headerCtx, groupBy: s.groupBy, pageW: dimsOf(s)[0], pageH: dimsOf(s)[1] });
  };
  const setSql = (sql: string) => set((s) => ({ report: { ...s.report, sql } }));
  const exportPagesPrint = (asPdf: boolean) => {
    const s = stRef.current; set({ expOpen: false });
    printHTML(buildPagesHTML(previewPagesOf(s), effNameOf(s), dimsOf(s)[0], dimsOf(s)[1], s.orient));
    toast(asPdf ? (isId() ? 'Membuka dialog cetak → simpan sebagai PDF' : 'Opening print → save as PDF') : (isId() ? 'Membuka dialog cetak…' : 'Opening print…'));
  };
  const exportHTMLfile = () => { const s = stRef.current; set({ expOpen: false }); const ok = download(fileName(effNameOf(s), 'html'), 'text/html', buildPagesHTML(previewPagesOf(s), effNameOf(s), dimsOf(s)[0], dimsOf(s)[1], s.orient)); toast(ok ? (isId() ? 'File HTML diunduh' : 'HTML downloaded') : 'Export blocked'); };
  const exportTable = (kind: 'xls' | 'doc') => {
    const s = stRef.current; set({ expOpen: false }); const mime = kind === 'xls' ? 'application/vnd.ms-excel' : 'application/msword';
    const ok = download(fileName(effNameOf(s), kind), mime, buildTableHTML(effNameOf(s), effectiveData(s)));
    toast(ok ? (isId() ? ('File ' + (kind === 'xls' ? 'Excel' : 'Word') + ' diunduh') : ((kind === 'xls' ? 'Excel' : 'Word') + ' downloaded')) : 'Export blocked');
  };

  // ---------- keyboard + drag listeners ----------
  React.useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const tag = (e.target && (e.target as HTMLElement).tagName) || '';
      const mod0 = e.ctrlKey || e.metaKey;
      // Ctrl/Cmd+S saves from anywhere, including while editing the report name.
      if (mod0 && e.key.toLowerCase() === 's') { save(); e.preventDefault(); return; }
      if (tag === 'INPUT' || tag === 'SELECT' || tag === 'TEXTAREA') return;
      const k = e.key.toLowerCase(); const mod = e.ctrlKey || e.metaKey;
      if (mod && k === 'c') { doCopy(); e.preventDefault(); }
      else if (mod && k === 'x') { doCut(); e.preventDefault(); }
      else if (mod && k === 'v') { doPaste(); e.preventDefault(); }
      else if (mod && k === 'd') { dupSel(); e.preventDefault(); }
      else if (mod && e.shiftKey && k === 'z') { redo(); e.preventDefault(); }
      else if (mod && k === 'z') { undo(); e.preventDefault(); }
      else if (mod && k === 'y') { redo(); e.preventDefault(); }
      else if (k === 'delete' || k === 'backspace') { if (stRef.current.selEl) { delSel(); e.preventDefault(); } }
    };
    const mm = (e: MouseEvent) => onMove(e);
    const mu = () => { dragRef.current = null; };
    window.addEventListener('mousemove', mm); window.addEventListener('mouseup', mu); window.addEventListener('keydown', onKey);
    return () => { window.removeEventListener('mousemove', mm); window.removeEventListener('mouseup', mu); window.removeEventListener('keydown', onKey); };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Load real templates from the reports API on mount; open the first one.
  React.useEffect(() => {
    listReportTemplates({ limit: 100, isActive: true }).then((res) => {
      const items = res.data.map((t) => ({ id: t.id, name: t.name }));
      set({ tplList: items });
      if (items.length) selectTemplate(items[0].id);
    }).catch(() => { /* offline → keep built-in default layout */ });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Run the report SQL (debounced) when present; feeds live rows into preview/dictionary.
  React.useEffect(() => {
    const sql = (st.report.sql || '').trim();
    if (!sql) { if (stRef.current.sqlRows || stRef.current.sqlErr || stRef.current.sqlLoading) set({ sqlRows: null, sqlCols: [], sqlErr: null, sqlLoading: false }); return; }
    const timer = setTimeout(() => {
      set({ sqlLoading: true });
      executeSqlQuery(sql, {}, 200)
        .then((res) => set({ sqlRows: res.rows, sqlCols: res.columns, sqlErr: null, sqlLoading: false }))
        .catch((e) => set({ sqlRows: null, sqlCols: [], sqlErr: e instanceof Error ? e.message : String(e), sqlLoading: false }));
    }, 500);
    return () => clearTimeout(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [st.report.sql]);

  const selObj: RsElement | null = st.selEl
    ? (st.report.bands.flatMap((b) => b.els).find((e) => e.id === st.selEl) || null)
    : null;
  const selBandObj = st.selBand ? (st.report.bands.find((b) => b.id === st.selBand) || null) : null;

  const dims = dimsOf(st);
  return {
    st, set, isId: st.lang === 'id', accent: st.accent, theme: st.theme,
    pageW: dims[0], pageH: dims[1], marginPx: MARGINS[st.margin] || 40, zoom: st.zoom || 1, snapV, effName: effNameOf(st),
    report: st.report, selObj, selBandObj, findEl, findBand, pushUndo, updateEl, updateBand, editEl,
    STYLES, previewPages: () => previewPagesOf(st),
    sql: st.report.sql || '', sqlErr: st.sqlErr, sqlLoading: st.sqlLoading,
    sqlActive: !!(st.report.sql && st.sqlRows), dataCols: st.sqlCols, rowCount: effectiveData(st).rows.length,
    actions: {
      undo, redo, loadTemplate, selectTemplate, save, setSql, onGroupBy, toast,
      doCopy, doCut, doPaste, dupSel, delSel, addElement, zMove, setZoom, onCanvasWheel,
      onElementMouseDown, onResizeMouseDown, onBandResizeDown, onBandLabelDown, onBandMouseDown,
      onFieldDragStart, allowDrop, onCanvasDrop,
      exportPagesPrint, exportHTMLfile, exportTable,
    },
  };
}

export type RsCtrl = ReturnType<typeof useReportStudio>;
