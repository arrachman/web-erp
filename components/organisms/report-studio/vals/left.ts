import type { RsCtrl } from '../hooks/use-report-studio';
import type { RsLeftTab } from '@/lib/report-studio/types';
import { SCHEMA, RELATIONS, PARAMS } from '@/lib/report-studio/constants';
import { isMoney } from '@/lib/report-studio/format';
import { GROUP_OPTIONS, tr } from '@/lib/report-studio/i18n';
import { dtBase } from './styles';

export function leftVals(c: RsCtrl) {
  const id = c.isId; const st = c.st; const a = c.actions; const t = tr(id);

  const docTabs = [
    { label: 'Page 1', onClick: () => c.set({ view: 'design' }), style: dtBase + (st.view === 'design' ? 'background:var(--canvas,#c9ced6);color:var(--text,#1d2330);' : 'color:var(--muted,#6b7280);') },
    { label: 'Preview', onClick: () => c.set({ view: 'preview' }), style: dtBase + (st.view === 'preview' ? 'background:var(--canvas,#c9ced6);color:var(--text,#1d2330);' : 'color:var(--muted,#6b7280);') },
  ];

  const tabs: Array<[RsLeftTab, string]> = [['data', t.data], ['relations', t.relations], ['params', t.params], ['funcs', t.funcs]];
  const leftTabs = tabs.map((tb) => {
    const on = st.leftTab === tb[0];
    return { label: tb[1], onClick: () => c.set({ leftTab: tb[0] }), style: 'height:28px;padding:0 7px;border:none;border-bottom:2px solid ' + (on ? 'var(--accent,#2563eb)' : 'transparent') + ';background:transparent;color:' + (on ? 'var(--accent,#2563eb)' : 'var(--muted,#6b7280)') + ';font-size:11px;font-weight:600;cursor:pointer;' };
  });

  const groupOptions = (GROUP_OPTIONS[st.tplKey] || ['']).map((v) => ({ v, label: v === '' ? (id ? '(tanpa pengelompokan)' : '(no grouping)') : v }));

  const dataTree = c.sqlActive
    ? [{
      name: 'Query', tables: [{
        name: 'Hasil', caret: '▾', open: true, onToggle: () => { /* always open */ },
        fields: c.dataCols.map((col) => ({ name: col, badge: isMoney(col) ? '$' : 'T', path: col })),
      }],
    }]
    : SCHEMA.map((ds) => ({
      name: ds.name,
      tables: ds.tables.map((tb) => ({
        name: tb.name, caret: st.openTables[tb.name] ? '▾' : '▸', open: !!st.openTables[tb.name],
        onToggle: () => c.set((s) => ({ openTables: { ...s.openTables, [tb.name]: !s.openTables[tb.name] } })),
        fields: tb.fields.map((f) => ({ name: f[0], badge: f[1], path: tb.name + '.' + f[0] })),
      })),
    }));

  const relations = RELATIONS.map((r) => {
    const opt = (r.id in st.relOpt) ? st.relOpt[r.id] : r.opt;
    return {
      left: r.left, right: r.right,
      mode: opt ? (id ? 'Opsional · LEFT JOIN' : 'Optional · LEFT JOIN') : (id ? 'Wajib · INNER JOIN' : 'Required · INNER JOIN'),
      onToggle: () => c.set((s) => ({ relOpt: { ...s.relOpt, [r.id]: !((r.id in s.relOpt) ? s.relOpt[r.id] : r.opt) } })),
      btnStyle: 'width:100%;height:26px;border:1px solid ' + (opt ? '#c8923a' : 'var(--accent,#2563eb)') + ';border-radius:6px;background:' + (opt ? 'rgba(200,146,58,.1)' : 'var(--accent-weak,#e7efff)') + ';color:' + (opt ? '#b07d22' : 'var(--accent,#2563eb)') + ";font-size:11px;font-weight:600;cursor:pointer;font-family:'IBM Plex Mono',monospace;",
    };
  });

  const params = PARAMS.map((p) => ({ name: p.name, val: p.val, path: 'param:' + p.name }));

  const funcs = [
    ['Sum(field)', 'Sum(InvoiceLines.Amount)', id ? 'Total dari kolom angka' : 'Total of a numeric column'],
    ['Avg(field)', 'Avg(InvoiceLines.Amount)', id ? 'Rata-rata' : 'Average'],
    ['Count()', 'Count()', id ? 'Jumlah baris' : 'Row count'],
    ['Max(field)', 'Max(InvoiceLines.Amount)', id ? 'Nilai tertinggi' : 'Maximum'],
    ['Min(field)', 'Min(InvoiceLines.Amount)', id ? 'Nilai terendah' : 'Minimum'],
    ['Today()', 'Today()', id ? 'Tanggal hari ini' : 'Current date'],
    ['PageNumber()', 'PageNumber()', id ? 'Nomor halaman' : 'Page number'],
    ['TotalPages()', 'TotalPages()', id ? 'Total halaman' : 'Total pages'],
  ].map((f) => ({ sig: f[0], path: 'expr:' + f[1], desc: f[2] }));

  return {
    docTabs, isDesign: st.view === 'design', isPreview: st.view === 'preview',
    leftOpen: st.leftOpen, leftClosed: !st.leftOpen, leftTabs,
    tabData: st.leftTab === 'data', tabRel: st.leftTab === 'relations', tabParam: st.leftTab === 'params', tabFunc: st.leftTab === 'funcs',
    groupBy: st.groupBy, onGroupBy: a.onGroupBy, groupOptions, dataTree, onFieldDragStart: a.onFieldDragStart, relations, params, funcs,
    sql: c.sql, onSql: a.setSql, sqlErr: c.sqlErr, sqlLoading: c.sqlLoading,
    toggleLeftPanel: () => c.set((s) => ({ leftOpen: !s.leftOpen })),
  };
}
