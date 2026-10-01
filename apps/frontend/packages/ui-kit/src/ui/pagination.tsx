'use client';

import * as React from 'react';
import { cn } from '../utils';
import { Icon, type IconName } from './icons';

/**
 * Pager bar. Mirrors prototype `.pager` + segmented `.pager .seg`
 * stepper (first / prev / next / last). Stateless: parent owns `page`
 * and reacts to `onPageChange`.
 */
export interface PaginationProps extends React.HTMLAttributes<HTMLDivElement> {
  page: number;
  pageCount: number;
  onPageChange: (page: number) => void;
  /** Optional summary slot rendered on the left (e.g. "1–20 of 240"). */
  summary?: React.ReactNode;
}

function SegButton({
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
      disabled={disabled}
      onClick={onClick}
      className="border-0 border-r border-border bg-transparent px-[7px] py-[3px] text-[11.5px] text-muted-foreground outline-none transition-colors last:border-r-0 hover:bg-[var(--panel-hover)] hover:text-foreground focus-visible:bg-[var(--panel-hover)] disabled:cursor-not-allowed disabled:opacity-40"
    >
      <Icon name={icon} />
    </button>
  );
}

export function Pagination({
  className,
  page,
  pageCount,
  onPageChange,
  summary,
  ...props
}: PaginationProps) {
  const clamp = (p: number) => Math.min(Math.max(p, 1), Math.max(pageCount, 1));
  const go = (p: number) => onPageChange(clamp(p));
  const atStart = page <= 1;
  const atEnd = page >= pageCount;

  return (
    <div
      className={cn(
        'flex items-center gap-2 border-t border-border bg-card px-4 py-1.5 text-[11.5px] text-muted-foreground',
        className,
      )}
      {...props}
    >
      {summary}
      <div className="flex-1" />
      <span className="font-mono tabular-nums">
        {page} / {Math.max(pageCount, 1)}
      </span>
      <div className="inline-flex items-center overflow-hidden rounded border border-border bg-card">
        <SegButton
          icon="chevdoubleleft"
          label="Halaman pertama"
          disabled={atStart}
          onClick={() => go(1)}
        />
        <SegButton
          icon="chevleft"
          label="Halaman sebelumnya"
          disabled={atStart}
          onClick={() => go(page - 1)}
        />
        <SegButton
          icon="chevright"
          label="Halaman berikutnya"
          disabled={atEnd}
          onClick={() => go(page + 1)}
        />
        <SegButton
          icon="chevdoubleright"
          label="Halaman terakhir"
          disabled={atEnd}
          onClick={() => go(pageCount)}
        />
      </div>
    </div>
  );
}
