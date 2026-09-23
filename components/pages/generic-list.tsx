'use client';

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { StatusBadge } from '@/components/ui/badge';
import { MODULES } from '@/lib/registry';
import {
  fmtIDR,
  genKasMasuk,
  type KasMasukRow,
  type Translator,
} from '@/lib/mock';
import { useTabKey } from '@/lib/tab-context';
import { bulkAction } from '@/lib/feedback';
import { BukuBesar } from './buku-besar';
import {
  ListHeader,
  ListToolbar,
  ListPager,
  ListBulkBar,
  DEFAULT_FILTERS,
  type Filters,
  type SortCol,
} from './generic-list-parts';

interface ListTableProps {
  t: Translator;
  col: string;
  view: KasMasukRow[];
  selected: Set<number>;
  setSelected: React.Dispatch<React.SetStateAction<Set<number>>>;
  focused: number;
  setFocused: (i: number) => void;
  allSelected: boolean;
  someSelected: boolean;
  toggle: (id: number) => void;
  setSortCol: (col: SortCol) => void;
  sortInd: (col: SortCol) => React.ReactNode;
  openForm: () => void;
}

function ListTable({
  t,
  col,
  view,
  selected,
  setSelected,
  focused,
  setFocused,
  allSelected,
  someSelected,
  toggle,
  setSortCol,
  sortInd,
  openForm,
}: ListTableProps) {
  return (
    <div className="tbl-wrap scrollbar">
      <table className="tbl">
        <thead>
          <tr>
            <th className="col-check">
              <input
                type="checkbox"
                className="checkbox"
                checked={allSelected}
                ref={(el) => {
                  if (el) el.indeterminate = someSelected;
                }}
                onChange={() =>
                  setSelected(
                    allSelected
                      ? new Set()
                      : new Set(view.map((r) => r.id)),
                  )
                }
              />
            </th>
            <th className="sortable" onClick={() => setSortCol('no')}>
              {t('No Transaksi')}
              {sortInd('no')}
            </th>
            <th className="sortable" onClick={() => setSortCol('tanggal')}>
              {t('Tanggal')}
              {sortInd('tanggal')}
            </th>
            <th
              className="sortable"
              onClick={() => setSortCol('terimaDari')}
            >
              {col}
              {sortInd('terimaDari')}
            </th>
            <th>{t('Uraian')}</th>
            <th>{t('Status')}</th>
            <th>{t('Cabang')}</th>
            <th>{t('User')}</th>
            <th
              className="col-num sortable"
              onClick={() => setSortCol('total')}
            >
              {t('Total')} (IDR){sortInd('total')}
            </th>
          </tr>
        </thead>
        <tbody>
          {view.map((r, i) => (
            <tr
              key={r.id}
              className={`${selected.has(r.id) ? 'selected' : ''} ${
                i === focused ? 'focused' : ''
              }`}
              onClick={() => setFocused(i)}
            >
              <td className="col-check">
                <input
                  type="checkbox"
                  className="checkbox"
                  checked={selected.has(r.id)}
                  onChange={() => toggle(r.id)}
                />
              </td>
              <td className="mono">
                <a
                  style={{
                    color: 'var(--primary-soft-fg)',
                    textDecoration: 'none',
                    cursor: 'pointer',
                  }}
                  onClick={() => openForm()}
                >
                  {r.no}
                </a>
              </td>
              <td className="mono muted">{r.tanggal}</td>
              <td>{r.terimaDari}</td>
              <td
                className="muted"
                style={{
                  maxWidth: 280,
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                }}
              >
                {r.uraian}
              </td>
              <td>
                <StatusBadge status={r.status} />
              </td>
              <td className="mono muted">{r.cabang}</td>
              <td className="muted">{r.user}</td>
              <td
                className="num"
                style={{
                  color: r.total < 0 ? 'var(--danger)' : 'inherit',
                }}
              >
                {fmtIDR(r.total)}
              </td>
            </tr>
          ))}
          {view.length === 0 && (
            <tr>
              <td colSpan={9} className="tbl-empty">
                Tidak ada transaksi yang cocok dengan filter
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}

interface GenericListProps {
  moduleId: string;
  t: Translator;
  lang?: 'id' | 'en' | 'ja';
  onNavigate: (route: string) => void;
  onOpenTab?: (route: string) => void;
}

/** Generic list for Keuangan transaksi — ported from prototype `pages/generic-list.jsx`. */
export function GenericList({
  moduleId,
  t,
  onNavigate,
  onOpenTab,
}: GenericListProps) {
  const openForm = () => (onOpenTab || onNavigate)(`${moduleId}-new`);
  const mod = MODULES[moduleId];

  const neg = mod ? /^(CD|SM|SGC)/.test(mod.prefix) : false;
  const prefix = mod?.prefix ?? '';
  const [rows] = React.useState<KasMasukRow[]>(() =>
    genKasMasuk(48).map((r) => ({
      ...r,
      no: `${prefix}-26${r.no.slice(5)}`,
      total: neg ? -Math.abs(r.total) : r.total,
    })),
  );

  const [q, setQ] = React.useState('');
  const [filters, setFilters] = React.useState<Filters>(DEFAULT_FILTERS);
  const [activeFilters, setActiveFilters] = React.useState<string[]>([
    'status',
    'tanggal',
    'cabang',
  ]);
  const [selected, setSelected] = React.useState<Set<number>>(new Set());
  const [focused, setFocused] = React.useState(0);
  const [sort, setSort] = React.useState<{ col: SortCol; dir: 'asc' | 'desc' }>({
    col: 'tanggal',
    dir: 'desc',
  });
  const [page, setPage] = React.useState(1);
  const pageSize = 24;

  const filtered = React.useMemo(() => {
    let arr = rows.filter((r) => {
      if (filters.status !== 'Semua' && r.status !== filters.status) return false;
      if (filters.cabang !== 'Semua' && r.cabang !== filters.cabang) return false;
      if (filters.lokasi !== 'Semua' && r.lokasi !== filters.lokasi) return false;
      if (filters.user !== 'Semua' && r.user !== filters.user) return false;
      if (q) {
        const ql = q.toLowerCase();
        if (
          ![r.no, r.terimaDari, r.uraian, r.status, r.cabang, r.user].some((v) =>
            String(v).toLowerCase().includes(ql),
          )
        )
          return false;
      }
      return true;
    });
    arr = [...arr].sort((a, b) => {
      let av: string | number = a[sort.col];
      let bv: string | number = b[sort.col];
      if (sort.col === 'total') {
        av = Number(av);
        bv = Number(bv);
      }
      if (av < bv) return sort.dir === 'asc' ? -1 : 1;
      if (av > bv) return sort.dir === 'asc' ? 1 : -1;
      return 0;
    });
    return arr;
  }, [rows, q, filters, sort]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const safePage = Math.min(page, totalPages);
  const start = (safePage - 1) * pageSize;
  const view = filtered.slice(start, start + pageSize);
  const sumTotal = filtered.reduce((s, r) => s + Number(r.total || 0), 0);

  const toggle = (id: number) =>
    setSelected((s) => {
      const n = new Set(s);
      if (n.has(id)) n.delete(id);
      else n.add(id);
      return n;
    });
  const allSelected = view.length > 0 && view.every((r) => selected.has(r.id));
  const someSelected = view.some((r) => selected.has(r.id)) && !allSelected;
  const clearSel = () => setSelected(new Set());

  useTabKey((e: KeyboardEvent) => {
    const tag = (e.target as HTMLElement).tagName;
    if (['INPUT', 'TEXTAREA', 'SELECT'].includes(tag)) return;
    if (e.key === 'ArrowDown' || e.key === 'j') {
      e.preventDefault();
      setFocused((f) => Math.min(view.length - 1, f + 1));
    } else if (e.key === 'ArrowUp' || e.key === 'k') {
      e.preventDefault();
      setFocused((f) => Math.max(0, f - 1));
    } else if (e.key === 'x' || e.key === ' ') {
      e.preventDefault();
      if (view[focused]) toggle(view[focused].id);
    } else if (e.key.toLowerCase() === 'n') {
      e.preventDefault();
      openForm();
    } else if (e.key === 'Enter' && view[focused]) {
      e.preventDefault();
      openForm();
    }
  });

  if (mod && mod.isLedger) return <BukuBesar t={t} />;
  if (!mod)
    return (
      <div style={{ padding: 24 }}>Modul tidak ditemukan: {moduleId}</div>
    );

  const setSortCol = (col: SortCol) =>
    setSort((s) => ({
      col,
      dir: s.col === col && s.dir === 'asc' ? 'desc' : 'asc',
    }));
  const sortInd = (col: SortCol) =>
    sort.col !== col ? undefined : (
      <span className="sort-ind">
        <Icon name={sort.dir === 'asc' ? 'chevup' : 'chevdown'} size={10} />
      </span>
    );
  const setF = (k: keyof Filters, v: string) => {
    setFilters((f) => ({ ...f, [k]: v }));
    setPage(1);
  };
  const removeF = (id: string) =>
    setActiveFilters((a) => a.filter((x) => x !== id));
  const bulk = (kind: string) =>
    bulkAction(kind, selected.size, clearSel);

  const availFilters = [
    { id: 'status', label: t('Status') },
    { id: 'tanggal', label: t('Tanggal') },
    { id: 'cabang', label: t('Cabang') },
    { id: 'lokasi', label: t('Lokasi') },
    { id: 'user', label: t('User') },
  ].filter((f) => !activeFilters.includes(f.id));

  return (
    <div className="page">
      <ListHeader
        t={t}
        label={mod.label}
        code={mod.code}
        q={q}
        setQ={setQ}
        setPage={setPage}
        filteredLen={filtered.length}
        openForm={openForm}
      />

      <ListToolbar
        t={t}
        filters={filters}
        setFilters={setFilters}
        activeFilters={activeFilters}
        setActiveFilters={setActiveFilters}
        setF={setF}
        removeF={removeF}
        setPage={setPage}
        setQ={setQ}
        availFilters={availFilters}
        sumTotal={sumTotal}
        filteredLen={filtered.length}
      />

      <ListTable
        t={t}
        col={mod.col}
        view={view}
        selected={selected}
        setSelected={setSelected}
        focused={focused}
        setFocused={setFocused}
        allSelected={allSelected}
        someSelected={someSelected}
        toggle={toggle}
        setSortCol={setSortCol}
        sortInd={sortInd}
        openForm={openForm}
      />

      <ListPager
        t={t}
        safePage={safePage}
        totalPages={totalPages}
        viewLen={view.length}
        filteredLen={filtered.length}
        setPage={setPage}
      />

      {selected.size > 0 && (
        <ListBulkBar
          t={t}
          count={selected.size}
          bulk={bulk}
          clearSel={clearSel}
        />
      )}
    </div>
  );
}
