'use client';

/**
 * Reusable pagination control for list pages. Renders 3 zones:
 *
 *   [counter range]   [limit selector?]   ·   [« ‹ › »]
 *
 * Atomic tier: Molecule. Composes atoms (Icon, Select, native button).
 * Does NOT own keyboard hints — those live in the organism since they
 * concern row navigation, not pagination.
 */

import * as React from 'react';
import { Icon, type IconName } from '@/components/ui/icons';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { tGlobal } from '@/lib/mock';

export const DEFAULT_PAGE_SIZE_OPTIONS = [10, 25, 50, 100] as const;

export interface TablePaginationProps {
  /** 1-indexed current page. */
  page: number;
  /** Total number of pages. */
  pageCount: number;
  /** Rows shown on the current page (used to compute the range end). */
  rowCount: number;
  /** Total rows across all pages. Omit if not cheaply available. */
  totalRows?: number;
  /** Active page size (rows per page). Required for the range calculation. */
  pageSize: number;
  /** Jump to page (clamped by caller; molecule passes raw value). */
  onPage: (page: number) => void;
  /** When provided, the limit selector is shown. */
  onPageSize?: (size: number) => void;
  pageSizeOptions?: readonly number[];
  className?: string;
}

function NavBtn({
  icon,
  label,
  disabled,
  onClick,
}: {
  icon: IconName;
  label: string;
  disabled?: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      aria-label={label}
      title={label}
      disabled={disabled}
      onClick={onClick}
      className="border-0 border-r border-border bg-transparent px-[7px] py-[3px] text-[11.5px] text-muted-foreground outline-none transition-colors last:border-r-0 hover:bg-[var(--panel-hover)] hover:text-foreground focus-visible:bg-[var(--panel-hover)] disabled:cursor-not-allowed disabled:opacity-40"
    >
      <Icon name={icon} />
    </button>
  );
}

export function TablePagination({
  page,
  pageCount,
  rowCount,
  totalRows,
  pageSize,
  onPage,
  onPageSize,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  className,
}: TablePaginationProps) {
  const atStart = page <= 1;
  const atEnd = page >= pageCount;

  const rangeStart = rowCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const rangeEnd = rowCount === 0 ? 0 : rangeStart + rowCount - 1;

  return (
    <div
      className={['flex items-center gap-3 text-[11px] text-muted-foreground', className]
        .filter(Boolean)
        .join(' ')}
    >
      <span className="tabular-nums">
        <strong className="text-foreground">
          {rangeStart}–{rangeEnd}
        </strong>
        {totalRows !== undefined ? (
          <>
            {' '}{tGlobal('dari')}{' '}
            <strong className="text-foreground">{totalRows}</strong>
          </>
        ) : null}
        {' '}{tGlobal('baris')}
      </span>

      {onPageSize && (
        <label className="flex items-center gap-1.5">
          <span className="opacity-70">{tGlobal('Tampilkan')}</span>
          <Select
            value={String(pageSize)}
            onValueChange={(v) => onPageSize(Number(v))}
          >
            <SelectTrigger
              aria-label={tGlobal('Jumlah baris per halaman')}
              style={{ width: 'auto', minWidth: '4.5rem', height: 24, padding: '0 6px' }}
            >
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              {pageSizeOptions.map((opt) => (
                <SelectItem key={opt} value={String(opt)}>
                  {opt}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </label>
      )}

      <div className="inline-flex items-center overflow-hidden rounded border border-border bg-card">
        <NavBtn
          icon="chevdoubleleft"
          label={tGlobal('Halaman pertama')}
          disabled={atStart}
          onClick={() => onPage(1)}
        />
        <NavBtn
          icon="chevleft"
          label={tGlobal('Halaman sebelumnya')}
          disabled={atStart}
          onClick={() => onPage(page - 1)}
        />
        <span className="border-r border-border px-2 py-[3px] tabular-nums">
          <strong className="text-foreground">{page}</strong>
          {' / '}
          {Math.max(pageCount, 1)}
        </span>
        <NavBtn
          icon="chevright"
          label={tGlobal('Halaman berikutnya')}
          disabled={atEnd}
          onClick={() => onPage(page + 1)}
        />
        <NavBtn
          icon="chevdoubleright"
          label={tGlobal('Halaman terakhir')}
          disabled={atEnd}
          onClick={() => onPage(Math.max(pageCount, 1))}
        />
      </div>
    </div>
  );
}
