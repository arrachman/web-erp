'use client';

/**
 * One row in a type-aware tree DnD master list. Atomic tier: molecule.
 * Used by `tree-dnd-master-page.tsx`. Renders drag handle + depth indent +
 * cells (code, name, extra columns, status, kebab). Drag handle is the
 * `grip-vertical` icon — checkbox column is intentionally absent because
 * the entity is not bulk-actionable in this layout.
 */

import * as React from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Badge } from '@/components/ui/badge';
import { Icon } from '@/components/ui/icons';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { TableRow, TableCell, CodeLinkCell } from '@/components/organisms/table';
import { tGlobal } from '@/lib/mock';

export interface TreeDndExtraColumn<T> {
  key: string;
  label: string;
  render: (row: T) => React.ReactNode;
}

interface BaseRow {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}

interface Props<T extends BaseRow> {
  row: T;
  depth: number;
  /** Keyboard-nav focus highlight (J/K). Renders the focused row outline. */
  focused?: boolean;
  extraColumns: TreeDndExtraColumn<T>[];
  rowActions: RowActionItem[];
  onOpenEdit: (row: T) => void;
}

export function TreeDndRow<T extends BaseRow>({
  row,
  depth,
  focused,
  extraColumns,
  rowActions,
  onOpenEdit,
}: Props<T>) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: row.id });

  const style: React.CSSProperties = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : undefined,
    backgroundColor: isDragging ? 'var(--bg-hover)' : undefined,
  };

  return (
    <RowContextMenu items={rowActions}>
      <TableRow
        ref={setNodeRef as React.Ref<HTMLTableRowElement>}
        style={style}
        data-focused={focused}
        className="cursor-pointer"
        onDoubleClick={() => onOpenEdit(row)}
      >
        <TableCell style={{ width: 36, padding: 0 }}>
          <button
            type="button"
            className="drag-handle"
            aria-label={tGlobal('Tarik untuk menata ulang')}
            title={tGlobal('Tarik untuk menata ulang')}
            {...attributes}
            {...listeners}
            style={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: '100%',
              height: '100%',
              minHeight: 28,
              cursor: 'grab',
              background: 'transparent',
              border: 0,
              color: 'var(--muted-fg, #888)',
              touchAction: 'none',
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <Icon name="grip-vertical" />
          </button>
        </TableCell>
        <CodeLinkCell code={row.code} onOpen={() => onOpenEdit(row)} />
        <TableCell>
          <span style={{ paddingLeft: depth * 18 }}>{row.name}</span>
        </TableCell>
        {extraColumns.map((c) => (
          <TableCell key={c.key} className="muted">
            {c.render(row)}
          </TableCell>
        ))}
        <TableCell>
          <Badge variant={row.isActive ? 'success' : 'default'} dot className="-ml-[7px]">
            {tGlobal(row.isActive ? 'Aktif' : 'Nonaktif')}
          </Badge>
        </TableCell>
        <TableCell>
          <RowActionsMenu items={rowActions} />
        </TableCell>
      </TableRow>
    </RowContextMenu>
  );
}
