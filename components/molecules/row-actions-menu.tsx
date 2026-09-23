'use client';

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icons';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuSeparator,
  ContextMenuTrigger,
} from '@/components/ui/context-menu';

export interface RowActionItem {
  label: string;
  onSelect: () => void;
  danger?: boolean;
  separatorBefore?: boolean;
}

/**
 * Canonical inline row actions for list tables: semua aksi (Edit, Riwayat,
 * Hapus, …) di balik kebab `more-vertical`. Tabel tetap tenang; aksi
 * destruktif butuh klik sengaja sehingga lebih aman.
 */
export function RowActionsMenu({ items }: { items: RowActionItem[] }) {
  return (
    <div
      className="flex items-center justify-end"
      onClick={(e) => e.stopPropagation()}
    >
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            size="sm"
            variant="ghost"
            aria-label="Aksi baris"
            className="!px-1.5"
          >
            <Icon name="more-vertical" size={14} />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="z-[800]">
          {items.map((it, i) => (
            <React.Fragment key={`${it.label}-${i}`}>
              {it.separatorBefore && <DropdownMenuSeparator />}
              <DropdownMenuItem
                danger={it.danger}
                onSelect={() => it.onSelect()}
              >
                {it.label}
              </DropdownMenuItem>
            </React.Fragment>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}

/**
 * Right-click context menu for a table row. Renders the **same** items as
 * `RowActionsMenu` (kebab) — per `apps/web-erp/CLAUDE.md §2.11`, kanan-klik
 * di baris = paritas opsi dengan titik tiga. Pakai `asChild` agar trigger
 * langsung jadi `<tr>` tanpa membungkus elemen non-table di dalam `<tbody>`.
 */
export function RowContextMenu({
  items,
  children,
}: {
  items: RowActionItem[];
  children: React.ReactElement;
}) {
  return (
    <ContextMenu>
      <ContextMenuTrigger asChild>{children}</ContextMenuTrigger>
      <ContextMenuContent className="z-[800]">
        {items.map((it, i) => (
          <React.Fragment key={`${it.label}-${i}`}>
            {it.separatorBefore && <ContextMenuSeparator />}
            <ContextMenuItem
              danger={it.danger}
              onSelect={() => it.onSelect()}
            >
              {it.label}
            </ContextMenuItem>
          </React.Fragment>
        ))}
      </ContextMenuContent>
    </ContextMenu>
  );
}
