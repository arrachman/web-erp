'use client';

/**
 * Table for TreeDndMasterPage — loading/error, DndContext, table header,
 * SortableContext, row actions的内联構築. Atomic tier: organism (co-located
 * sibling of tree-dnd-master-page.tsx). Per-row action array dibuat di
 * dalam row map (bukan di luar) agar setiap baris punya closures sendiri.
 */

import * as React from 'react';
import {
  DndContext,
  PointerSensor,
  KeyboardSensor,
  closestCenter,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import {
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { type RowActionItem } from '@/components/molecules/row-actions-menu';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableEmpty,
} from '@/components/organisms/table';
import { TreeDndRow } from './tree-dnd-row';
import type { TreeDndExtraColumn } from './tree-dnd-row';
import { tGlobal } from '@/lib/mock';
import type { TreeRow } from './tree-dnd-master-page.types';

export interface TreeDndMasterTableProps<T extends TreeRow> {
  loading: boolean;
  error: string | null;
  visibleFlat: { row: T; depth: number }[];
  sortableIds: string[];
  extraColumns: TreeDndExtraColumn<T>[];
  colCount: number;
  filterActive: boolean;
  debouncedSearch: string;
  entityLabel: string;
  focusedIndex: number;
  onDragEnd: (event: DragEndEvent) => void;
  onOpenEdit: (row: T) => void;
  onDelete: (row: T) => void;
  onOpenAudit: (row: T) => void;
}

export function TreeDndMasterTable<T extends TreeRow>({
  loading,
  error,
  visibleFlat,
  sortableIds,
  extraColumns,
  colCount,
  filterActive,
  debouncedSearch,
  entityLabel,
  focusedIndex,
  onDragEnd,
  onOpenEdit,
  onDelete,
  onOpenAudit,
}: TreeDndMasterTableProps<T>) {
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  return (
    <div className="lines" style={{ flex: 1, minHeight: 0, overflowY: 'auto' }}>
      {loading ? (
        <div style={{ padding: 16 }} className="muted">{tGlobal('Memuat...')}</div>
      ) : error ? (
        <div style={{ padding: 16, color: 'var(--danger, #c33)' }}>{tGlobal('Gagal memuat data')}: {error}</div>
      ) : (
        <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={onDragEnd}>
          <Table className="table-fixed">
            <colgroup>
              <col style={{ width: 36 }} />
              <col style={{ width: 200 }} />
              <col />
              {extraColumns.map((c) => <col key={c.key} style={{ width: 160 }} />)}
              <col style={{ width: 120 }} />
              <col style={{ width: 48 }} />
            </colgroup>
            <TableHeader>
              <TableRow>
                <TableHead aria-label={tGlobal('Tarik')} />
                <TableHead>{tGlobal('Kode')}</TableHead>
                <TableHead>{tGlobal('Nama')}</TableHead>
                {extraColumns.map((c) => <TableHead key={c.key}>{tGlobal(c.label)}</TableHead>)}
                <TableHead>{tGlobal('Status')}</TableHead>
                <TableHead />
              </TableRow>
            </TableHeader>
            <TableBody>
              <SortableContext items={sortableIds} strategy={verticalListSortingStrategy}>
                {visibleFlat.length === 0 ? (
                  <TableEmpty
                    colSpan={colCount}
                    variant={debouncedSearch || filterActive ? 'filtered' : 'empty'}
                    entityLabel={tGlobal(entityLabel)}
                    searchTerm={debouncedSearch || undefined}
                  />
                ) : (
                  visibleFlat.map(({ row, depth }, idx) => {
                    const actions: RowActionItem[] = [
                      { label: tGlobal('Edit'), onSelect: () => onOpenEdit(row) },
                      { label: tGlobal('Riwayat'), onSelect: () => onOpenAudit(row) },
                      {
                        label: tGlobal('Hapus'),
                        onSelect: () => onDelete(row),
                        danger: true,
                        separatorBefore: true,
                      },
                    ];
                    return (
                      <TreeDndRow
                        key={row.id}
                        row={row}
                        depth={depth}
                        focused={focusedIndex === idx}
                        extraColumns={extraColumns}
                        rowActions={actions}
                        onOpenEdit={onOpenEdit}
                      />
                    );
                  })
                )}
              </SortableContext>
            </TableBody>
          </Table>
        </DndContext>
      )}
    </div>
  );
}