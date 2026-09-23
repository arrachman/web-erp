'use client';

// Presentational sub-components for the Keuangan generic list page.
// Split out of `generic-list.tsx` to keep each file ≤400 lines.
import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import {
  fmtIDR,
  STATUSES,
  CABANGS,
  LOKASIS,
  USERS,
  type Translator,
} from '@/lib/mock';
import {
  FilterChip,
  DateRangeChip,
  AddFilterChip,
} from '@/components/organisms/filter-chips';
import { notify } from '@/lib/feedback';

export type SortCol = 'no' | 'tanggal' | 'terimaDari' | 'total';

export interface Filters {
  status: string;
  cabang: string;
  lokasi: string;
  user: string;
  from: string;
  to: string;
}

export const DEFAULT_FILTERS: Filters = {
  status: 'Semua',
  cabang: 'Semua',
  lokasi: 'Semua',
  user: 'Semua',
  from: '01/05/2026',
  to: '12/05/2026',
};

interface ListHeaderProps {
  t: Translator;
  label: string;
  code: string;
  q: string;
  setQ: (v: string) => void;
  setPage: (p: number) => void;
  filteredLen: number;
  openForm: () => void;
}

export function ListHeader({
  t,
  label,
  code,
  q,
  setQ,
  setPage,
  filteredLen,
  openForm,
}: ListHeaderProps) {
  return (
    <div className="page-header">
      <h1 className="page-title">
        {t(label)}
        <span className="code-tag">{code}</span>
      </h1>
      <div className="page-actions">
        <div className="search-input">
          <Icon name="search" size={12} />
          <input
            placeholder={t('Cari semua...')}
            value={q}
            onChange={(e) => {
              setQ(e.target.value);
              setPage(1);
            }}
          />
          <Kbd>/</Kbd>
        </div>
        <button
          className="btn"
          onClick={() =>
            notify(`${filteredLen} baris diekspor (.xlsx)`, 'success')
          }
        >
          <Icon name="download" size={12} /> {t('Export')}
        </button>
        <button
          className="btn"
          onClick={() => notify('Data dimuat ulang', 'info')}
        >
          <Icon name="refresh" size={12} />
        </button>
        <div className="btn-split">
          <button className="btn primary" onClick={() => openForm()}>
            <Icon name="plus" size={12} /> {t('Tambah')} <Kbd>N</Kbd>
          </button>
          <button className="btn primary">
            <Icon name="chevdown" size={12} />
          </button>
        </div>
      </div>
    </div>
  );
}

interface ListToolbarProps {
  t: Translator;
  filters: Filters;
  setFilters: React.Dispatch<React.SetStateAction<Filters>>;
  activeFilters: string[];
  setActiveFilters: React.Dispatch<React.SetStateAction<string[]>>;
  setF: (k: keyof Filters, v: string) => void;
  removeF: (id: string) => void;
  setPage: (p: number) => void;
  setQ: (v: string) => void;
  availFilters: { id: string; label: string }[];
  sumTotal: number;
  filteredLen: number;
}

export function ListToolbar({
  t,
  filters,
  setFilters,
  activeFilters,
  setActiveFilters,
  setF,
  removeF,
  setPage,
  setQ,
  availFilters,
  sumTotal,
  filteredLen,
}: ListToolbarProps) {
  return (
    <div className="toolbar">
      <Icon name="filter" size={13} className="muted" />
      {activeFilters.includes('status') && (
        <FilterChip
          label={t('Status')}
          val={filters.status}
          options={['Semua', ...STATUSES]}
          onChange={(v) => setF('status', v)}
          onRemove={() => removeF('status')}
        />
      )}
      {activeFilters.includes('tanggal') && (
        <DateRangeChip
          from={filters.from}
          to={filters.to}
          onChange={(f, to) => {
            setFilters((s) => ({ ...s, from: f, to }));
            setPage(1);
          }}
          onRemove={() => removeF('tanggal')}
        />
      )}
      {activeFilters.includes('cabang') && (
        <FilterChip
          label={t('Cabang')}
          val={filters.cabang}
          options={['Semua', ...CABANGS]}
          onChange={(v) => setF('cabang', v)}
          onRemove={() => removeF('cabang')}
        />
      )}
      {activeFilters.includes('lokasi') && (
        <FilterChip
          label={t('Lokasi')}
          val={filters.lokasi}
          options={['Semua', ...LOKASIS]}
          onChange={(v) => setF('lokasi', v)}
          onRemove={() => removeF('lokasi')}
        />
      )}
      {activeFilters.includes('user') && (
        <FilterChip
          label={t('User')}
          val={filters.user}
          options={['Semua', ...USERS]}
          onChange={(v) => setF('user', v)}
          onRemove={() => removeF('user')}
        />
      )}
      <AddFilterChip
        available={availFilters}
        onAdd={(id) => setActiveFilters((a) => [...a, id])}
        t={t}
      />
      <div style={{ flex: 1 }} />
      <span className="muted" style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))' }}>
        Σ {fmtIDR(sumTotal)}
      </span>
      <span className="muted" style={{ fontSize: 'calc(11.5px * var(--font-scale, 1))' }}>
        · {filteredLen} {t('baris')}
      </span>
      <button
        className="btn ghost sm"
        onClick={() => {
          setFilters(DEFAULT_FILTERS);
          setQ('');
          setPage(1);
        }}
      >
        {t('Reset')}
      </button>
    </div>
  );
}

interface ListPagerProps {
  t: Translator;
  safePage: number;
  totalPages: number;
  viewLen: number;
  filteredLen: number;
  setPage: React.Dispatch<React.SetStateAction<number>>;
}

export function ListPager({
  t,
  safePage,
  totalPages,
  viewLen,
  filteredLen,
  setPage,
}: ListPagerProps) {
  return (
    <div className="pager">
      <span>
        {t('Halaman')}{' '}
        <strong style={{ color: 'var(--fg)' }}>{safePage}</strong>{' '}
        {t('dari')} {totalPages}
      </span>
      <span>
        · {viewLen} {t('dari')} {filteredLen} {t('baris')}
      </span>
      <div className="spacer" />
      <span className="muted">
        Pintasan: <Kbd>J</Kbd>/<Kbd>K</Kbd> · <Kbd>X</Kbd> pilih ·{' '}
        <Kbd>N</Kbd> baru
      </span>
      <div className="seg">
        <button disabled={safePage === 1} onClick={() => setPage(1)}>
          <Icon name="chevdoubleleft" size={11} />
        </button>
        <button
          disabled={safePage === 1}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          <Icon name="chevleft" size={11} />
        </button>
        <button
          disabled={safePage === totalPages}
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
        >
          <Icon name="chevright" size={11} />
        </button>
        <button
          disabled={safePage === totalPages}
          onClick={() => setPage(totalPages)}
        >
          <Icon name="chevdoubleright" size={11} />
        </button>
      </div>
    </div>
  );
}

interface ListBulkBarProps {
  t: Translator;
  count: number;
  bulk: (kind: string) => void;
  clearSel: () => void;
}

export function ListBulkBar({ t, count, bulk, clearSel }: ListBulkBarProps) {
  return (
    <div className="bulk-bar fade-in">
      <span className="count">{count}</span>
      <span>dipilih</span>
      <span className="divider" />
      <button className="ba-btn" onClick={() => bulk('approve')}>
        <Icon name="check" size={12} /> {t('Approve')}
      </button>
      <button className="ba-btn" onClick={() => bulk('reject')}>
        <Icon name="x" size={12} /> {t('Reject')}
      </button>
      <button className="ba-btn" onClick={() => bulk('post')}>
        <Icon name="play" size={12} /> {t('Posting')}
      </button>
      <button className="ba-btn" onClick={() => bulk('export')}>
        <Icon name="download" size={12} /> {t('Export')}
      </button>
      <span className="divider" />
      <button className="ba-btn danger" onClick={() => bulk('delete')}>
        <Icon name="trash" size={12} /> {t('Hapus')}
      </button>
      <span className="divider" />
      <button className="ba-btn" onClick={clearSel}>
        <Icon name="x" size={12} />
      </button>
    </div>
  );
}
