import type * as React from 'react';
import type { RsCtrl } from '../hooks/use-report-studio';
import type { RsElement, RsBand } from '@/lib/report-studio/types';
import { SCHEMA, PARAMS, SWATCHES } from '@/lib/report-studio/constants';
import { bandLabel, fmtLabel, tr } from '@/lib/report-studio/i18n';
import { headStyle, gridInput, gridSelect, pSwatchStyle } from './styles';

type InEvt = React.ChangeEvent<HTMLInputElement>;
type SelEvt = React.ChangeEvent<HTMLSelectElement>;
interface PropRow {
  label: string; value?: string | number; options?: Array<{ v: string; label: string }>;
  swatches?: Array<{ onClick: () => void; style: string }>;
  isText?: boolean; isNum?: boolean; isSelect?: boolean; isBool?: boolean; isColor?: boolean;
  onInput?: (e: InEvt) => void; onChange?: (e: SelEvt) => void;
}
interface PropGroup { title: string; key: string; open: boolean; chev: string; onToggle: () => void; headStyle: string; rows: PropRow[]; }

export function rightVals(c: RsCtrl) {
  const id = c.isId; const st = c.st; const report = c.report;
  const selObj = c.selObj; const selBandId = st.selBand;
  const t = tr(id);

  const niceType = (ty: string) => bandLabel(ty, id);
  const componentOptions: Array<{ v: string; label: string }> = [];
  report.bands.forEach((b) => {
    componentOptions.push({ v: 'b:' + b.id, label: niceType(b.type) });
    b.els.forEach((el) => {
      const lbl = el.kind === 'label' ? ('   T  "' + (el.text || '').slice(0, 16) + '"') : el.kind === 'field' ? ('   {} ' + el.bind) : el.kind === 'expr' ? ('   =  ' + el.bind) : ('   ' + (el.kind === 'line' ? '─ Line' : '▢ Box'));
      componentOptions.push({ v: 'e:' + el.id, label: lbl });
    });
  });
  const compSel = st.selEl ? ('e:' + st.selEl) : selBandId ? ('b:' + selBandId) : '';
  const onSelectComponent = (e: SelEvt) => {
    const v = e.target.value;
    if (v.indexOf('e:') === 0) c.set({ selEl: v.slice(2), selBand: null });
    else if (v.indexOf('b:') === 0) c.set({ selBand: v.slice(2), selEl: null });
    else c.set({ selEl: null, selBand: null });
  };

  const noSel = !selObj && !selBandId;
  const fmtOpts = ['General', 'Number', 'Currency', 'Date', 'Time', 'Percentage'].map((f) => ({ v: f, label: fmtLabel(f, id) }));
  const fontFamilyOpts = ['', 'IBM Plex Sans', 'Arial', 'Times New Roman', 'Courier New', 'Georgia', 'Verdana', 'Tahoma'].map((f) => ({ v: f, label: f === '' ? 'Default' : f }));
  const boolOpts = [{ v: 'true', label: 'True' }, { v: 'false', label: 'False' }];
  const grp = (title: string, key: string, rows: PropRow[]): PropGroup =>
    ({ title, key, open: !!st.propOpen[key], chev: st.propOpen[key] ? '▼' : '▶', onToggle: () => c.set((s) => ({ propOpen: { ...s.propOpen, [key]: !s.propOpen[key] } })), headStyle, rows });

  const bindOptions: Array<{ v: string; label: string }> = [{ v: '', label: id ? '(tidak ada)' : '(none)' }];
  SCHEMA.forEach((ds) => ds.tables.forEach((tb) => tb.fields.forEach((f) => bindOptions.push({ v: tb.name + '.' + f[0], label: tb.name + '.' + f[0] }))));
  ['Sum(InvoiceLines.Amount)', 'Count()', 'Today()', 'PageNumber()', 'TotalPages()'].forEach((x) => bindOptions.push({ v: x, label: x }));
  PARAMS.forEach((p) => bindOptions.push({ v: p.name, label: p.name }));
  c.dataCols.forEach((col) => bindOptions.push({ v: col, label: col }));

  const propGroups: PropGroup[] = []; let selName = ''; let selDesc = '';

  if (selObj) {
    const E = selObj; const up = (p: Partial<RsElement>) => c.updateEl(E.id, p); const upU = (p: Partial<RsElement>) => { c.pushUndo(); c.updateEl(E.id, p); };
    const getE = () => c.findEl(E.id);
    const mkNum = (label: string, f: keyof RsElement): PropRow => ({ label, isNum: true, value: E[f] as number, onInput: (ev) => up({ [f]: Math.round(parseFloat(ev.target.value) || 0) } as Partial<RsElement>) });
    const mkTxt = (label: string, f: keyof RsElement): PropRow => ({ label, isText: true, value: (E[f] as string) || '', onInput: (ev) => up({ [f]: ev.target.value } as Partial<RsElement>) });
    const mkSel = (label: string, f: keyof RsElement, opts: PropRow['options']): PropRow => ({ label, isSelect: true, value: String(E[f] == null ? '' : E[f]), options: opts, onChange: (ev) => upU({ [f]: ev.target.value } as Partial<RsElement>) });
    const mkBool = (label: string, f: keyof RsElement): PropRow => ({ label, isBool: true, value: E[f] ? 'true' : 'false', options: boolOpts, onChange: (ev) => upU({ [f]: ev.target.value === 'true' } as Partial<RsElement>) });
    const mkColor = (label: string, f: keyof RsElement): PropRow => ({ label, isColor: true, swatches: SWATCHES.slice(0, 12).map((col) => ({ onClick: () => { c.pushUndo(); up({ [f]: col } as Partial<RsElement>); }, style: pSwatchStyle(col, !!(getE() && getE()![f] === col)) })) });
    const isTxt = E.kind !== 'line' && E.kind !== 'box';
    propGroups.push(grp(id ? '1. Posisi & Ukuran' : '1. Position & Size', 'pos', [mkNum(id ? 'Kiri (X)' : 'Left (X)', 'x'), mkNum(id ? 'Atas (Y)' : 'Top (Y)', 'y'), mkNum(id ? 'Lebar' : 'Width', 'w'), mkNum(id ? 'Tinggi' : 'Height', 'h')]));
    if (isTxt) {
      propGroups.push(grp(id ? '2. Teks' : '2. Text', 'text', [
        ...(E.kind === 'label' ? [mkTxt(id ? 'Teks' : 'Text', 'text')] : []),
        ...(E.kind !== 'label' ? [mkSel(id ? 'Sumber Data' : 'Data Field', 'bind', bindOptions)] : []),
        mkSel(id ? 'Format' : 'Format', 'format', fmtOpts), mkSel('Font', 'font', fontFamilyOpts), mkNum(id ? 'Ukuran' : 'Size', 'size'),
        mkBool('Bold', 'bold'), mkBool('Italic', 'italic'), mkBool('Underline', 'underline'),
        mkSel(id ? 'Rata H' : 'H Align', 'align', [{ v: 'left', label: 'Left' }, { v: 'center', label: 'Center' }, { v: 'right', label: 'Right' }, { v: 'justify', label: 'Justify' }]),
        mkSel(id ? 'Rata V' : 'V Align', 'valign', [{ v: 'top', label: 'Top' }, { v: 'middle', label: 'Middle' }, { v: 'bottom', label: 'Bottom' }]),
        mkBool(id ? 'Bungkus' : 'Word Wrap', 'wordWrap'), mkColor(id ? 'Warna Teks' : 'Text Color', 'color'), mkColor(id ? 'Warna Latar' : 'Back Color', 'bg'),
      ]));
    }
    propGroups.push(grp(id ? '3. Tampilan' : '3. Appearance', 'appearance', [
      ...(isTxt ? [] : [mkColor(id ? 'Isi' : 'Fill', 'bg')]),
      mkBool(id ? 'Garis Atas' : 'Border Top', 'bTop'), mkBool(id ? 'Garis Bawah' : 'Border Bottom', 'bBottom'), mkBool(id ? 'Garis Kiri' : 'Border Left', 'bLeft'), mkBool(id ? 'Garis Kanan' : 'Border Right', 'bRight'),
      mkNum(id ? 'Tebal Garis' : 'Border Width', 'bWidth'), mkColor(id ? 'Warna Garis' : 'Border Color', 'bColor'),
    ]));
    propGroups.push(grp(id ? '4. Perilaku' : '4. Behavior', 'behavior', [mkBool(id ? 'Dapat Tumbuh' : 'Can Grow', 'canGrow'), mkBool(id ? 'Dapat Susut' : 'Can Shrink', 'canShrink'), mkBool(id ? 'Aktif' : 'Enabled', 'enabled')]));
    selName = ({ label: 'Text', field: 'DataText', expr: 'Expression', line: 'HorizontalLine', box: 'Shape' } as Record<string, string>)[E.kind] + E.id.replace('e', '');
    selDesc = E.kind === 'label' ? ('"' + (E.text || '') + '"') : E.kind === 'field' ? ('{' + E.bind + '}') : E.kind === 'expr' ? ('=' + E.bind) : (id ? 'Elemen grafis' : 'Graphic element');
  } else if (selBandId) {
    const B = c.findBand(selBandId);
    if (B) {
      const upB = (p: Partial<RsBand>) => c.updateBand(B.id, p); const upBU = (p: Partial<RsBand>) => { c.pushUndo(); c.updateBand(B.id, p); };
      const getB = () => c.findBand(B.id);
      const bNum = (label: string, f: keyof RsBand): PropRow => ({ label, isNum: true, value: B[f] as number, onInput: (ev) => upB({ [f]: Math.max(18, Math.round(parseFloat(ev.target.value) || 18)) } as Partial<RsBand>) });
      const bBool = (label: string, f: keyof RsBand): PropRow => ({ label, isBool: true, value: B[f] ? 'true' : 'false', options: boolOpts, onChange: (ev) => upBU({ [f]: ev.target.value === 'true' } as Partial<RsBand>) });
      const bColor = (label: string, f: keyof RsBand): PropRow => ({ label, isColor: true, swatches: SWATCHES.slice(0, 12).map((col) => ({ onClick: () => { c.pushUndo(); upB({ [f]: col } as Partial<RsBand>); }, style: pSwatchStyle(col, !!(getB() && getB()![f] === col)) })) });
      propGroups.push(grp(id ? '1. Posisi' : '1. Position', 'pos', [bNum(id ? 'Tinggi' : 'Height', 'h')]));
      propGroups.push(grp(id ? '3. Tampilan' : '3. Appearance', 'appearance', [bColor(id ? 'Warna Latar' : 'Back Color', 'bg')]));
      propGroups.push(grp(id ? '4. Perilaku' : '4. Behavior', 'behavior', [bBool(id ? 'Dapat Tumbuh' : 'Can Grow', 'canGrow'), bBool(id ? 'Dapat Susut' : 'Can Shrink', 'canShrink'), bBool(id ? 'Cetak Semua Hal.' : 'Print on All Pages', 'printAll')]));
      selName = B.type + selBandId.replace('b', '');
      selDesc = bandLabel(B.type, id) + ' · ' + (id ? 'Band laporan' : 'Report band');
    }
  }

  const structure: Array<{ label: string; onClick: () => void; style: string }> = [];
  report.bands.forEach((b) => {
    structure.push({ label: bandLabel(b.type, id), onClick: () => c.set({ selBand: b.id, selEl: null }), style: 'display:flex;align-items:center;padding:5px 8px;font-size:11.5px;font-weight:700;color:var(--text,#1d2330);border-radius:6px;cursor:pointer;' + (selBandId === b.id ? 'background:var(--accent-weak,#e7efff);color:var(--accent,#2563eb);' : '') });
    b.els.forEach((el) => {
      const lbl = el.kind === 'label' ? '“' + (el.text || '').slice(0, 20) + '”' : el.kind === 'field' ? '{' + el.bind + '}' : el.kind === 'expr' ? '=' + el.bind : (el.kind === 'line' ? '─ Garis' : '▢ Kotak');
      structure.push({ label: lbl, onClick: () => c.set({ selEl: el.id, selBand: null }), style: 'padding:3px 8px 3px 24px;font-size:11px;color:var(--muted,#6b7280);border-radius:6px;cursor:pointer;font-family:\'IBM Plex Mono\',monospace;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;' + (st.selEl === el.id ? 'background:var(--accent-weak,#e7efff);color:var(--accent,#2563eb);' : '') });
    });
  });

  const rbBaseS = 'flex:1;height:30px;border:none;background:transparent;font-size:11px;font-weight:600;cursor:pointer;border-top:2px solid transparent;';
  const rbtns: Array<[string, string]> = [['props', t.properties], ['dictionary', t.dictionary], ['tree', t.reportTree]];
  const rightBottomTabs = rbtns.map((x) => {
    const on = st.rightTab === x[0];
    return { label: x[1], onClick: () => c.set({ rightTab: x[0] as typeof st.rightTab }), style: rbBaseS + (on ? 'color:var(--accent,#2563eb);border-top-color:var(--accent,#2563eb);background:var(--panel,#fff);' : 'color:var(--muted,#6b7280);') };
  });
  const rightTitle = ({ props: t.properties, dictionary: t.dictionary, tree: t.reportTree } as Record<string, string>)[st.rightTab];

  return {
    rightTitle, rightProps: st.rightTab === 'props', rightTree: st.rightTab === 'tree', rightDict: st.rightTab === 'dictionary',
    componentOptions, compSel, onSelectComponent, noSel, gridInput, gridSelect, propGroups, selName, selDesc, structure, rightBottomTabs,
  };
}
