'use client';

import * as React from 'react';
import * as DialogPrimitive from '@radix-ui/react-dialog';
import { cn } from '@/lib/utils';
import { Icon } from '@/components/ui/icons';
import { Kbd } from '@/components/ui/kbd';
import { Checkbox } from '@/components/ui/checkbox';
import { SearchSelectColumn, SearchSelectOption } from './search-select-types';

interface SearchSelectModalProps {
  open: boolean;
  onOpenChange: (next: boolean) => void;
  resolvedTitle: string;
  isMulti: boolean;
  loading: boolean;
  total: number;
  totalPages: number;
  page: number;
  limit: number;
  query: string;
  columns: SearchSelectColumn[];
  displayOptions: SearchSelectOption[];
  colSpan: number;
  confirmCount: number;
  focusedIdx: number;
  tableActive: boolean;
  localSingle: string;
  localSelected: Set<string>;
  allChecked: boolean;
  someChecked: boolean;
  searchRef: React.RefObject<HTMLInputElement | null>;
  scrollRef: React.RefObject<HTMLDivElement | null>;
  onKeyDown: (e: React.KeyboardEvent) => void;
  onQueryChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onToggleAll: () => void;
  onToggleMulti: (val: string) => void;
  onSelectSingle: (opt: SearchSelectOption, idx: number) => void;
  onConfirmRow: (opt: SearchSelectOption) => void;
  /** Cancel/ESC pass `true`/`'trigger'`; submit handlers use `'next'`. */
  onClose: (refocus?: boolean | 'trigger' | 'next') => void;
  onConfirm: () => void;
}

export function SearchSelectModal({
  open, onOpenChange, resolvedTitle, isMulti, loading, total, totalPages,
  page, limit, query, columns, displayOptions, colSpan, confirmCount,
  focusedIdx, tableActive, localSingle, localSelected, allChecked, someChecked,
  searchRef, scrollRef,
  onKeyDown, onQueryChange, onToggleAll, onToggleMulti,
  onSelectSingle, onConfirmRow, onClose, onConfirm,
}: SearchSelectModalProps) {
  return (
    <DialogPrimitive.Root open={open} onOpenChange={(next) => { if (!next) onClose(true); else onOpenChange(next); }}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="fixed inset-0 z-[900] bg-black/40 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0" />
        <DialogPrimitive.Content
          onKeyDown={onKeyDown}
          // We own post-close focus ourselves (trigger on cancel, next field on
          // submit) via closeModal — suppress Radix's restore-to-trigger default
          // so it doesn't race with focusNextFrom.
          onCloseAutoFocus={(e) => e.preventDefault()}
          className="fixed left-1/2 top-1/2 z-[901] flex w-[640px] max-w-[calc(100vw-2rem)] -translate-x-1/2 -translate-y-1/2 flex-col overflow-hidden rounded-lg border border-border bg-card shadow-[var(--shadow-flyout)] outline-none data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95"
          style={{ maxHeight: 'min(680px, calc(100vh - 4rem))' }}
        >
          <DialogPrimitive.Title className="sr-only">{resolvedTitle}</DialogPrimitive.Title>
          <DialogPrimitive.Description className="sr-only">
            {isMulti ? 'Pilih satu atau lebih item dari daftar' : 'Pilih satu item dari daftar'}
          </DialogPrimitive.Description>

          {/* Header */}
          <div className="flex shrink-0 items-center justify-between border-b border-border px-4 py-3">
            <div className="flex items-center gap-2">
              <span className="text-[calc(13px*var(--font-scale,1))] font-semibold text-foreground">{resolvedTitle}</span>
              {total > 0 && (
                <span className="text-[calc(12px*var(--font-scale,1))] text-muted-foreground tabular-nums">
                  · {total}
                </span>
              )}
            </div>
            <DialogPrimitive.Close asChild>
              <button type="button" style={{ cursor: 'pointer' }}
                className="flex items-center gap-1.5 rounded-md border border-border px-2 py-1 text-[calc(12px*var(--font-scale,1))] text-muted-foreground hover:bg-accent hover:text-accent-foreground">
                Tutup <Kbd>ESC</Kbd>
              </button>
            </DialogPrimitive.Close>
          </div>

          {/* Search */}
          <div className="flex shrink-0 items-center gap-2 border-b border-border px-4 py-2">
            <Icon name="search" className="shrink-0 text-muted-foreground" style={{ fontSize: 'calc(14px * var(--font-scale, 1))' }} />
            <input
              ref={searchRef}
              value={query}
              onChange={onQueryChange}
              placeholder="Ketik kode atau nama…"
              className="w-full bg-transparent text-[calc(13px*var(--font-scale,1))] text-foreground outline-none placeholder:text-[var(--fg-subtle)]"
            />
          </div>

          {/* Table */}
          <div ref={scrollRef} className="min-h-0 flex-1 overflow-auto"
            style={{ minHeight: `calc(${limit} * 2.25rem)` }}>
            <table className="w-full table-fixed border-collapse text-[calc(12.5px*var(--font-scale,1))]">
              <thead className="sticky top-0 z-10 bg-card">
                <tr className="border-b border-border">
                  {isMulti && (
                    <th className="w-10 px-4 py-2">
                      <Checkbox
                        checked={allChecked ? true : someChecked ? 'indeterminate' : false}
                        onCheckedChange={onToggleAll}
                      />
                    </th>
                  )}
                  {columns.map((col) => (
                    <th key={col.key} style={col.width ? { width: col.width } : undefined}
                      className="px-4 py-2 text-left text-[calc(11px*var(--font-scale,1))] font-medium uppercase tracking-wide text-muted-foreground">
                      {col.header}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody
                aria-busy={loading || undefined}
                className={cn(
                  'transition-opacity duration-150',
                  loading && displayOptions.length > 0 && 'pointer-events-none opacity-50',
                )}
              >
                {loading && displayOptions.length === 0 && (
                  <tr><td colSpan={colSpan} className="px-4 py-4 text-[var(--fg-subtle)]">Memuat…</td></tr>
                )}
                {!loading && displayOptions.length === 0 && (
                  <tr><td colSpan={colSpan} className="px-4 py-4 text-[var(--fg-subtle)]">Tidak ada hasil</td></tr>
                )}
                {displayOptions.length > 0 && displayOptions.map((opt, i) => {
                  const isActive = isMulti ? localSelected.has(opt.value) : opt.value === localSingle;
                  const isFocused = !loading && tableActive && i === focusedIdx;
                  return (
                    <tr
                      key={opt.value}
                      data-idx={i}
                      onClick={() => (isMulti ? onToggleMulti(opt.value) : onSelectSingle(opt, i))}
                      onDoubleClick={() => onConfirmRow(opt)}
                      style={{ cursor: 'pointer' }}
                      className={cn(
                        'border-b border-border/40 transition-[background-color,color] last:border-0',
                        'hover:bg-[var(--panel-hover)] hover:text-foreground',
                        isFocused && 'outline outline-2 -outline-offset-1 outline-primary bg-accent/40',
                        isActive && !isFocused && 'bg-accent/20',
                      )}
                    >
                      {isMulti && (
                        <td className="w-10 px-4 py-2" onClick={(e) => e.stopPropagation()}>
                          <Checkbox checked={localSelected.has(opt.value)} onCheckedChange={() => onToggleMulti(opt.value)} />
                        </td>
                      )}
                      {columns.map((col) => {
                        if (col.key === '_no') return (
                          <td key={col.key} className="px-4 py-2">
                            <span className="text-muted-foreground">{(page - 1) * limit + i + 1}</span>
                          </td>
                        );
                        if (col.key === 'code') {
                          const codeStr = String(opt.code ?? '');
                          return (
                            <td key={col.key} className="overflow-hidden px-4 py-2" title={codeStr}>
                              <span className="block truncate font-mono text-[calc(12px*var(--font-scale,1))] text-muted-foreground">{codeStr}</span>
                            </td>
                          );
                        }
                        return (
                          <td key={col.key} className="px-4 py-2">{String(opt[col.key] ?? '')}</td>
                        );
                      })}
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* Footer */}
          <div className="flex shrink-0 items-center justify-between border-t border-border px-4 py-2">
            <div className="flex items-center gap-3 text-[calc(11px*var(--font-scale,1))] text-muted-foreground">
              <span className="flex items-center gap-1"><Kbd>↑</Kbd><Kbd>↓</Kbd> navigasi</span>
              {totalPages > 1 && (
                <span className="flex items-center gap-1"><Kbd>←</Kbd><Kbd>→</Kbd> halaman</span>
              )}
              {isMulti
                ? <><span className="flex items-center gap-1"><Kbd>Space</Kbd> pilih baris</span><span className="flex items-center gap-1"><Kbd>↵</Kbd> submit</span></>
                : <span className="flex items-center gap-1"><Kbd>↵</Kbd> pilih</span>
              }
              <span className="flex items-center gap-1"><Kbd>ESC</Kbd> tutup</span>
            </div>
            <div className="flex items-center gap-3">
              {totalPages > 1 && (
                <span className="text-[calc(11px*var(--font-scale,1))] text-muted-foreground">
                  Hal. {page}/{totalPages}
                </span>
              )}
              <button type="button" onClick={() => onClose(true)} style={{ cursor: 'pointer' }}
                className="rounded-md border border-border px-3 py-1 text-[calc(12px*var(--font-scale,1))] font-medium hover:bg-accent">
                Batal
              </button>
              <button type="button" onClick={onConfirm} style={{ cursor: 'pointer' }}
                className="rounded-md bg-primary px-3 py-1 text-[calc(12px*var(--font-scale,1))] font-medium text-primary-foreground hover:bg-primary/90">
                Pilih{confirmCount > 0 ? ` (${confirmCount})` : ''}
              </button>
            </div>
          </div>
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  );
}
