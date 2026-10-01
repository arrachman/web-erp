'use client';

/**
 * Item form chrome: the live identity header shown above the active section
 * and the progress / prev-next footer for the Lengkap layout.
 * Atomic tier: Molecule. Consumed by items-form-fields.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
import type { ItemFormData } from './items-form-model';

/** Always-visible identity card so the user keeps context while paging sections. */
export function ItemFormContextHeader({ data }: { data: ItemFormData }) {
  return (
    <div className="flex items-center gap-3 border-b border-border bg-card px-5 py-3">
      <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-[var(--radius)] bg-[var(--primary-soft)] text-[var(--primary-soft-fg)]">
        <Icon name="box" size={20} />
      </span>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-semibold text-foreground">
          {data.name?.trim() || <span className="text-[var(--fg-subtle)]">Item baru</span>}
        </p>
        <p className="truncate font-mono text-[11px] text-[var(--fg-muted)]">
          {data.code?.trim() || 'kode belum diisi'}
        </p>
      </div>
      <div className="flex shrink-0 items-center gap-1.5">
        <Badge variant="info">{data.itemType}</Badge>
        <Badge variant={data.isActive ? 'success' : 'default'} dot>
          {data.isActive ? 'Aktif' : 'Nonaktif'}
        </Badge>
      </div>
    </div>
  );
}

export interface ItemFormFooterProps {
  filledCount: number;
  totalCount: number;
  position: string;
  canPrev: boolean;
  canNext: boolean;
  onPrev: () => void;
  onNext: () => void;
}

export function ItemFormFooter({
  filledCount, totalCount, position, canPrev, canNext, onPrev, onNext,
}: ItemFormFooterProps) {
  const pct = totalCount > 0 ? Math.round((filledCount / totalCount) * 100) : 0;
  return (
    <div className="flex items-center gap-3 border-t border-border bg-[var(--panel-2)] px-5 py-2">
      <div className="flex items-center gap-2">
        <div className="h-1.5 w-24 overflow-hidden rounded-full bg-border">
          <div className="h-full rounded-full bg-success transition-[width]" style={{ width: `${pct}%` }} />
        </div>
        <span className="text-[11px] text-[var(--fg-muted)]">Terisi {filledCount}/{totalCount}</span>
      </div>
      <span className="ml-auto text-[11px] text-[var(--fg-subtle)]">{position}</span>
      <div className="inline-flex overflow-hidden rounded-[var(--radius)] border border-border">
        <button
          type="button"
          onClick={onPrev}
          disabled={!canPrev}
          className="flex items-center gap-1 px-2.5 py-1 text-[11px] font-medium text-foreground transition-colors hover:bg-[var(--panel-hover)] disabled:cursor-not-allowed disabled:opacity-40"
        >
          <Icon name="undo" size={11} /> Sebelumnya
        </button>
        <button
          type="button"
          onClick={onNext}
          disabled={!canNext}
          className="flex items-center gap-1 border-l border-border px-2.5 py-1 text-[11px] font-medium text-foreground transition-colors hover:bg-[var(--panel-hover)] disabled:cursor-not-allowed disabled:opacity-40"
        >
          Berikutnya <Icon name="redo" size={11} />
        </button>
      </div>
    </div>
  );
}
