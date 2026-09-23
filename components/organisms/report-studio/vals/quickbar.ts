import type * as React from 'react';
import type { RsCtrl } from '../hooks/use-report-studio';
import { templateOptions } from '@/lib/report-studio/i18n';
import { isBuiltinKey } from '@/lib/report-studio/template-io';
import { qIconLight } from './styles';

export function quickbarVals(c: RsCtrl) {
  const id = c.isId; const st = c.st; const a = c.actions;
  const undoStyle = qIconLight + 'color:' + (st.undoN ? 'var(--text,#1d2330)' : 'var(--muted,#aab2c0)') + (st.undoN ? '' : ';cursor:not-allowed') + ';';
  const redoStyle = qIconLight + 'color:' + (st.redoN ? 'var(--text,#1d2330)' : 'var(--muted,#aab2c0)') + (st.redoN ? '' : ';cursor:not-allowed') + ';';
  const exportItems = [
    { ext: 'PDF', label: 'PDF', onClick: () => a.exportPagesPrint(true) },
    { ext: 'XLS', label: 'Excel', onClick: () => a.exportTable('xls') },
    { ext: 'DOC', label: 'Word', onClick: () => a.exportTable('doc') },
    { ext: 'HTM', label: 'HTML', onClick: () => a.exportHTMLfile() },
    { ext: 'PRN', label: id ? 'Cetak' : 'Print', onClick: () => a.exportPagesPrint(false) },
  ];
  // Real templates from the reports API; fall back to built-in keys when empty/offline.
  const tplOptions = st.tplList.length
    ? st.tplList.map((t) => ({ v: t.id, label: t.name }))
    : templateOptions(id);
  return {
    reportName: c.effName, templateOptions: tplOptions, tplKey: st.currentId ?? st.tplKey,
    onName: (e: React.ChangeEvent<HTMLInputElement>) => c.set({ reportName: e.target.value }),
    onTemplate: (e: React.ChangeEvent<HTMLSelectElement>) => {
      const v = e.target.value;
      if (isBuiltinKey(v)) a.loadTemplate(v, true); else a.selectTemplate(v);
    },
    onSave: a.save,
    onUndo: a.undo, onRedo: a.redo, undoStyle, redoStyle,
    langBtnLabel: id ? 'ID' : 'EN', toggleLang: () => c.set((s) => ({ lang: s.lang === 'id' ? 'en' : 'id' })),
    themeGlyph: c.theme === 'dark' ? '☾' : '☀', toggleTheme: () => c.set((s) => ({ theme: s.theme === 'light' ? 'dark' : 'light' })),
    toggleExport: () => c.set((s) => ({ expOpen: !s.expOpen, menu: null })), expOpen: st.expOpen, exportItems,
  };
}
