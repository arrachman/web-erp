'use client';

/**
 * Shared layout shell for ERP CRUD list pages.
 * Provides: action bar (search/export/refresh/add), filter bar, summary,
 * content area, pagination footer, and keyboard hints footer.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { ErrorState } from '@/components/molecules/error-state';
import { ListFooter, type ListPaginationConfig } from '@/components/organisms/list-footer';
import { tGlobal } from '@/lib/mock';

export type { ListPaginationConfig };

// ─── Public types ─────────────────────────────────────────────────────────────

export interface FilterOption {
  label: string;
  value: string;
}

export interface FilterConfig {
  key: string;
  label: string;
  options: FilterOption[];
  value: string;
  onChange: (value: string) => void;
}

export interface SummaryConfig {
  metricLabel: string;
  metricValue?: string;
  rowCount: number;
  totalCount?: number;
}

/** Wires J/K/X/Enter/Space keyboard nav to a table rendered inside ErpListLayout. */
export interface KeyboardRowConfig {
  rowCount: number;
  focusedIndex: number;
  onFocusChange: (i: number) => void;
  onToggle: (i: number) => void;
  onOpen?: (i: number) => void;
}

// ─── Sentinel for empty-string Select value ───────────────────────────────────
const ALL_VAL = '_all';
const toSel = (v: string) => v || ALL_VAL;
const fromSel = (v: string) => (v === ALL_VAL ? '' : v);

// ─── Component ────────────────────────────────────────────────────────────────

interface ErpListLayoutProps {
  title: string;
  code: string;
  loading?: boolean;
  /** True during any fetch (initial or refetch). Shows a thin top bar without blocking the table. */
  fetching?: boolean;
  error?: string | null;
  search: string;
  onSearch: (q: string) => void;
  onAdd?: () => void;
  onRefresh: () => void;
  onExport?: () => void;
  addLabel?: string;
  toolbar?: React.ReactNode;
  filters?: FilterConfig[];
  summary?: SummaryConfig;
  pagination?: ListPaginationConfig;
  /**
   * Count-only footer for non-paginated lists (e.g. CoA tree). Mutually
   * exclusive with `pagination` — when both set, pagination wins.
   */
  footerSummary?: { rowCount: number; totalRows: number };
  keyboardRows?: KeyboardRowConfig;
  keyboardHints?: boolean;
  /** Show "X pilih" hint in footer. Default true; false for non-selectable trees. */
  footerSelectable?: boolean;
  children: React.ReactNode;
}

export function ErpListLayout({
  title,
  code,
  loading,
  fetching,
  error,
  search,
  onSearch,
  onAdd,
  onRefresh,
  onExport,
  addLabel,
  toolbar,
  filters,
  summary,
  pagination,
  footerSummary,
  keyboardRows,
  keyboardHints = true,
  footerSelectable = true,
  children,
}: ErpListLayoutProps) {
  const searchRef = React.useRef<HTMLInputElement>(null);

  React.useEffect(() => {
    const kr = keyboardRows;
    const handler = (e: KeyboardEvent) => {
      const t = e.target as HTMLElement;
      const inField =
        t.tagName === 'INPUT' ||
        t.tagName === 'TEXTAREA' ||
        t.tagName === 'SELECT' ||
        t.isContentEditable ||
        !!t.closest('[role="dialog"]');

      if (e.key === '/' && !inField) {
        e.preventDefault();
        searchRef.current?.focus();
        return;
      }
      if (e.key === 'n' && !inField && !e.metaKey && !e.ctrlKey && onAdd) {
        e.preventDefault();
        onAdd();
        return;
      }
      if (e.key === 'ArrowLeft' && !inField && !e.metaKey && pagination && pagination.page > 1) {
        e.preventDefault();
        pagination.onPage(pagination.page - 1);
        return;
      }
      if (
        e.key === 'ArrowRight' &&
        !inField &&
        !e.metaKey &&
        pagination &&
        pagination.page < pagination.pageCount
      ) {
        e.preventDefault();
        pagination.onPage(pagination.page + 1);
        return;
      }
      // Row navigation
      if (!inField && kr && kr.rowCount > 0) {
        if (e.key === 'j' || e.key === 'J' || e.key === 'ArrowDown') {
          e.preventDefault();
          kr.onFocusChange(Math.min(Math.max(kr.focusedIndex, 0) + 1, kr.rowCount - 1));
          return;
        }
        if (e.key === 'k' || e.key === 'K' || e.key === 'ArrowUp') {
          e.preventDefault();
          kr.onFocusChange(Math.max(kr.focusedIndex <= 0 ? 0 : kr.focusedIndex - 1, 0));
          return;
        }
        if ((e.key === 'x' || e.key === 'X' || e.key === ' ') && kr.focusedIndex >= 0) {
          e.preventDefault();
          kr.onToggle(kr.focusedIndex);
          return;
        }
        if (e.key === 'Enter' && kr.focusedIndex >= 0 && kr.onOpen) {
          e.preventDefault();
          kr.onOpen(kr.focusedIndex);
        }
      }
    };
    window.addEventListener('keydown', handler);
    return () => window.removeEventListener('keydown', handler);
  }, [onAdd, pagination, keyboardRows]);

  const hasActiveFilter = filters?.some((f) => f.value !== '');
  const handleReset = () => filters?.forEach((f) => f.onChange(''));
  const showFilterBar = (filters && filters.length > 0) || !!summary || !!toolbar;


  return (
    <div className="page">
      {/* Action bar */}
      <div className="page-header">
        <h1 className="page-title">
          {tGlobal(title)}
          <span className="code-tag">{code}</span>
        </h1>
        <div className="page-actions">
          <div className="search-input">
            <Icon name="search" size={12} />
            <input
              ref={searchRef}
              placeholder={tGlobal('Cari semua...')}
              value={search}
              onChange={(e) => onSearch(e.target.value)}
            />
            <Kbd>/</Kbd>
          </div>
          {onExport && (
            <button className="btn" onClick={onExport} title={tGlobal('Export data')}>
              <Icon name="download" size={12} />
              {tGlobal('Export')}
            </button>
          )}
          <button className="btn" onClick={onRefresh} title={tGlobal('Muat ulang')}>
            <Icon name="refresh" size={12} />
          </button>
          {onAdd && (
            <button className="btn primary" onClick={onAdd}>
              <Icon name="plus" size={12} />
              {tGlobal(addLabel ?? 'Tambah')}
              <Kbd>N</Kbd>
            </button>
          )}
        </div>
      </div>

      {/* Filter bar */}
      {showFilterBar && (
        <div className="filter-bar">
          <button
            className="iconbtn"
            title={tGlobal('Filter')}
            style={{ opacity: hasActiveFilter ? 1 : 0.5 }}
          >
            <Icon name="filter" size={13} />
          </button>

          {filters?.map((f) => (
            <Select
              key={f.key}
              value={toSel(f.value)}
              onValueChange={(v) => f.onChange(fromSel(v))}
            >
              <SelectTrigger style={{ width: 'auto', minWidth: '7rem', height: 24 }}>
                <span style={{ color: 'var(--fg-faint)', fontSize: 'calc(10.5px * var(--font-scale, 1))', marginRight: 2 }}>
                  {tGlobal(f.label)}:
                </span>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {f.options.map((o) => (
                  <SelectItem key={toSel(o.value)} value={toSel(o.value)}>
                    {tGlobal(o.label)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          ))}

          {toolbar}

          <div style={{ flex: 1 }} />

          {summary && (
            <span className="filter-summary">
              {tGlobal(summary.metricLabel)}
              {summary.metricValue && (
                <> <strong>{summary.metricValue}</strong></>
              )}
              {' · '}
              <strong>{summary.rowCount}</strong> {tGlobal('baris')}
              {summary.totalCount !== undefined &&
                summary.totalCount !== summary.rowCount && (
                  <> {tGlobal('dari')} {summary.totalCount}</>
                )}
            </span>
          )}

          {hasActiveFilter && (
            <button className="btn ghost sm" onClick={handleReset}>
              <Icon name="x" size={11} />
              {tGlobal('Reset filter')}
            </button>
          )}
        </div>
      )}

      <div className="page-body">
        {fetching && !loading && !error && (
          <div
            className="h-[2px] w-full animate-pulse bg-primary/40"
            aria-hidden
            style={{ position: 'relative', zIndex: 1, marginBottom: -2 }}
          />
        )}

        <div className={`flex-1 min-h-0${(error || (loading && !error)) ? ' flex items-center justify-center' : ''}`}>
          {error && (
            <ErrorState
              message={error}
              onRetry={onRefresh}
              retrying={loading ?? fetching}
            />
          )}

          {loading && !error && (
            <div className="text-center text-xs text-muted-foreground">{tGlobal('Memuat...')}</div>
          )}

          {!loading && !error && children}
        </div>
      </div>

      {(pagination || footerSummary || keyboardHints) && (
        <ListFooter
          pagination={pagination}
          summary={!pagination ? footerSummary : undefined}
          keyboardHints={keyboardHints}
          selectable={footerSelectable}
          onAdd={onAdd}
        />
      )}
    </div>
  );
}
