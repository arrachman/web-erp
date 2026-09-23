'use client';

/**
 * Hierarchical CoA table body — expand/collapse chevron + depth indent.
 * Atomic tier: Organism sub-part (used only by accounts-page).
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
  CheckboxHead,
  CheckboxCell,
} from '@/components/organisms/table';
import type { ErpAccount } from '@/lib/api/accounts';
import type { FlatAccountRow } from '@/lib/accounts-tree';
import { tGlobal } from '@/lib/mock';

const COL_COUNT = 8;

export function AccountsTreeTable({
  nodes,
  expanded,
  selectedIds,
  focusedIndex,
  hasActiveFilter,
  searchTerm,
  onToggleExpand,
  onToggleRow,
  onToggleAll,
  onFocus,
  onOpenEdit,
  onOpenDuplicate,
  onOpenAudit,
  onDelete,
  onResetFilters,
  onCreate,
}: {
  nodes: FlatAccountRow<ErpAccount>[];
  expanded: Set<string>;
  selectedIds: Set<string>;
  focusedIndex: number;
  hasActiveFilter: boolean;
  searchTerm: string;
  onToggleExpand: (id: string) => void;
  onToggleRow: (id: string) => void;
  onToggleAll: (checked: boolean) => void;
  onFocus: (idx: number) => void;
  onOpenEdit: (row: ErpAccount) => void;
  onOpenDuplicate: (row: ErpAccount) => void;
  onOpenAudit: (row: ErpAccount) => void;
  onDelete: (row: ErpAccount) => void;
  onResetFilters: () => void;
  onCreate: () => void;
}) {
  const visible = nodes.map((n) => n.row);
  const allSelected =
    visible.length > 0 && visible.every((r) => selectedIds.has(r.id));
  const someSelected =
    !allSelected && visible.some((r) => selectedIds.has(r.id));

  return (
    <div className="lines">
      <Table className="table-fixed">
        <colgroup>
          <col style={{ width: 36 }} />
          <col style={{ width: 220 }} />
          <col />
          <col style={{ width: 110 }} />
          <col style={{ width: 100 }} />
          <col style={{ width: 70 }} />
          <col style={{ width: 120 }} />
          <col style={{ width: 48 }} />
        </colgroup>
        <TableHeader>
          <TableRow>
            <CheckboxHead
              checked={someSelected ? 'indeterminate' : allSelected}
              onCheckedChange={onToggleAll}
            />
            <TableHead>{tGlobal('Kode')}</TableHead>
            <TableHead>{tGlobal('Nama')}</TableHead>
            <TableHead>{tGlobal('Tipe')}</TableHead>
            <TableHead>{tGlobal('Jenis')}</TableHead>
            <TableHead>{tGlobal('Level')}</TableHead>
            <TableHead>{tGlobal('Status')}</TableHead>
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {nodes.length === 0 ? (
            <TableEmpty
              colSpan={COL_COUNT}
              variant={hasActiveFilter ? 'filtered' : 'empty'}
              entityLabel={tGlobal('akun')}
              searchTerm={searchTerm || undefined}
              onAction={hasActiveFilter ? onResetFilters : onCreate}
              actionLabel={
                hasActiveFilter
                  ? tGlobal('Reset filter')
                  : `${tGlobal('Tambah')} ${tGlobal('akun')}`
              }
              actionShortcut={hasActiveFilter ? undefined : 'N'}
            />
          ) : (
            nodes.map((node, idx) => {
              const row = node.row;
              const rowActions: RowActionItem[] = [
                { label: tGlobal('Edit'), onSelect: () => onOpenEdit(row) },
                {
                  label: tGlobal('Duplikat'),
                  onSelect: () => onOpenDuplicate(row),
                },
                {
                  label: tGlobal('Riwayat'),
                  onSelect: () => onOpenAudit(row),
                },
                {
                  label: tGlobal('Hapus'),
                  onSelect: () => onDelete(row),
                  danger: true,
                  separatorBefore: true,
                },
              ];
              const isOpen = expanded.has(row.id);
              return (
                <RowContextMenu key={row.id} items={rowActions}>
                  <TableRow
                    data-selected={selectedIds.has(row.id)}
                    data-focused={focusedIndex === idx}
                    className="cursor-pointer"
                    onClick={() => onFocus(idx)}
                    onDoubleClick={() => onOpenEdit(row)}
                  >
                    <CheckboxCell
                      checked={selectedIds.has(row.id)}
                      onCheckedChange={() => onToggleRow(row.id)}
                    />
                    <TableCell>
                      <div
                        className="flex min-w-0 items-center gap-0.5"
                        style={{ paddingLeft: node.depth * 16 }}
                      >
                        {node.hasChildren ? (
                          <button
                            type="button"
                            className="inline-flex h-5 w-5 shrink-0 items-center justify-center rounded text-muted-foreground hover:bg-[var(--bg-hover)]"
                            aria-label={
                              isOpen ? tGlobal('Collapse') : tGlobal('Expand')
                            }
                            aria-expanded={isOpen}
                            onClick={(e) => {
                              e.stopPropagation();
                              onToggleExpand(row.id);
                            }}
                          >
                            <Icon
                              name={isOpen ? 'chevdown' : 'chevright'}
                              size={12}
                            />
                          </button>
                        ) : (
                          <span className="inline-block h-5 w-5 shrink-0" />
                        )}
                        <button
                          type="button"
                          className="mono tabular-nums m-0 border-0 bg-transparent p-0 text-primary hover:underline focus:underline focus:outline-none"
                          onClick={(e) => {
                            e.stopPropagation();
                            onOpenEdit(row);
                          }}
                        >
                          {row.code}
                        </button>
                      </div>
                    </TableCell>
                    <TableCell>{row.name}</TableCell>
                    <TableCell className="muted">{row.type}</TableCell>
                    <TableCell className="muted">{row.kind}</TableCell>
                    <TableCell className="muted tabular-nums">
                      {row.level ?? '—'}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={row.isActive ? 'success' : 'default'}
                        dot
                        className="-ml-[7px]"
                      >
                        {tGlobal(row.isActive ? 'Aktif' : 'Nonaktif')}
                      </Badge>
                    </TableCell>
                    <TableCell>
                      <RowActionsMenu items={rowActions} />
                    </TableCell>
                  </TableRow>
                </RowContextMenu>
              );
            })
          )}
        </TableBody>
      </Table>
    </div>
  );
}
