'use client';

/**
 * Shared sticky footer for ERP list pages. Two mutually-exclusive left zones:
 *
 *   - `pagination` → full TablePagination (range + limit selector + « ‹ › »).
 *     Used by paginated lists (SimpleMasterPage, fin/* pages).
 *   - `summary` → count-only ("X dari Y baris"). Used by non-paginated lists
 *     that load everything at once (e.g. the menu tree, which needs the whole
 *     hierarchy visible for cross-parent drag-and-drop).
 *
 * The right zone is the keyboard-shortcut hint strip. `selectable=false` drops
 * the "X pilih" hint for pages without row selection (e.g. the DnD tree).
 *
 * Atomic tier: Organism (composes TablePagination molecule + Kbd atoms).
 */

import * as React from 'react';
import { Kbd } from '@/components/ui/kbd';
import { TablePagination } from '@/components/molecules/table-pagination';
import { tGlobal } from '@/lib/mock';

export interface ListPaginationConfig {
  page: number;
  pageCount: number;
  pageSize: number;
  totalRows: number;
  onPage: (page: number) => void;
  /** When provided, the limit selector is rendered in the footer. */
  onPageSize?: (size: number) => void;
  pageSizeOptions?: readonly number[];
}

/** Count-only left zone for non-paginated lists. */
export interface ListFooterSummary {
  /** Rows currently shown (after any client-side filter). */
  rowCount: number;
  /** Total rows in the dataset. Shown as "X dari Y" only when it differs. */
  totalRows: number;
}

export function ListFooter({
  pagination,
  summary,
  keyboardHints = true,
  selectable = true,
  onAdd,
}: {
  pagination?: ListPaginationConfig;
  summary?: ListFooterSummary;
  keyboardHints?: boolean;
  /** Show the "X pilih" hint. Set false for pages without row selection. */
  selectable?: boolean;
  onAdd?: () => void;
}) {
  return (
    <div className="sticky bottom-0 flex items-center gap-3 border-t border-border bg-card px-4 py-[5px] text-[11px] text-muted-foreground">
      {pagination && (
        <TablePagination
          page={pagination.page}
          pageCount={pagination.pageCount}
          pageSize={pagination.pageSize}
          totalRows={pagination.totalRows}
          rowCount={Math.min(
            pagination.pageSize,
            Math.max(pagination.totalRows - (pagination.page - 1) * pagination.pageSize, 0),
          )}
          onPage={pagination.onPage}
          onPageSize={pagination.onPageSize}
          pageSizeOptions={pagination.pageSizeOptions}
        />
      )}

      {!pagination && summary && (
        <span className="tabular-nums">
          <strong className="text-foreground">{summary.rowCount}</strong>
          {summary.rowCount !== summary.totalRows && (
            <>
              {' '}{tGlobal('dari')}{' '}
              <strong className="text-foreground">{summary.totalRows}</strong>
            </>
          )}
          {' '}{tGlobal('baris')}
        </span>
      )}

      <div className="flex-1" />

      {keyboardHints && (
        <span className="flex items-center gap-[3px]">
          {tGlobal('Pintasan')}:&nbsp;
          <Kbd>J</Kbd>↓/<Kbd>K</Kbd>↑
          {selectable && (
            <>
              <span className="mx-1 opacity-40">·</span>
              <Kbd>X</Kbd>&nbsp;{tGlobal('pilih')}
            </>
          )}
          {onAdd && (
            <>
              <span className="mx-1 opacity-40">·</span>
              <Kbd>N</Kbd>&nbsp;{tGlobal('baru')}
            </>
          )}
        </span>
      )}
    </div>
  );
}
