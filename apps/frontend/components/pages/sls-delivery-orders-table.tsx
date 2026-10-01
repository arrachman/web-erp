'use client';

/**
 * Delivery Order list table — extracted sub-component to keep sls-delivery-orders-page
 * under 400 lines (§3 clean code). Receives pre-fetched rows + callbacks.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
  CodeLinkCell,
} from '@/components/organisms/table';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { deleteSlsDeliveryOrder, type ErpSlsDeliveryOrder } from '@/lib/api/sls-delivery-orders';

export function SlsDeliveryOrdersTable({
  rows,
  focused,
  selected,
  sortBy,
  sortDir,
  onToggleSort,
  onToggleSel,
  onSelectAll,
  onOpen,
  onDelete,
  extraActions,
}: {
  rows: ErpSlsDeliveryOrder[];
  focused: number;
  selected: Set<string>;
  sortBy: string;
  sortDir: 'asc' | 'desc';
  onToggleSort: (col: string) => void;
  onToggleSel: (id: string) => void;
  onSelectAll: (checked: boolean) => void;
  onOpen: (r: ErpSlsDeliveryOrder) => void;
  onDelete: (r: ErpSlsDeliveryOrder) => void;
  extraActions?: (r: ErpSlsDeliveryOrder) => RowActionItem[];
}) {
  const rowActions = (r: ErpSlsDeliveryOrder): RowActionItem[] => [
    { label: 'Edit / Lihat', onSelect: () => onOpen(r) },
    ...(extraActions?.(r) ?? []),
    { label: 'Hapus', onSelect: () => onDelete(r), danger: true, separatorBefore: true },
  ];

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead style={{ width: 36, textAlign: 'center' }}>
            <input
              type="checkbox"
              checked={rows.length > 0 && rows.every((r) => selected.has(r.id))}
              ref={(el) => { if (el) el.indeterminate = selected.size > 0 && !rows.every((r) => selected.has(r.id)); }}
              onChange={(e) => onSelectAll(e.target.checked)}
              title="Pilih semua"
            />
          </TableHead>
          {([
            ['docNumber', 'No Transaksi'],
            ['docDate', 'Tanggal'],
            [null, 'Pelanggan'],
            [null, 'Uraian'],
            ['grandTotal', 'Total'],
            [null, 'Uang'],
            ['status', 'Status'],
          ] as [string | null, string][]).map(([col, label]) => (
            <TableHead
              key={label}
              style={col === 'grandTotal' ? { textAlign: 'right', cursor: col ? 'pointer' : undefined } : { cursor: col ? 'pointer' : undefined }}
              onClick={col ? () => onToggleSort(col) : undefined}
            >
              {label}
              {col && sortBy === col && (
                <span className="ml-1 text-muted-foreground text-xs">{sortDir === 'asc' ? '↑' : '↓'}</span>
              )}
            </TableHead>
          ))}
          <TableHead style={{ width: 44 }} />
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.length === 0 ? (
          <TableEmpty colSpan={9} />
        ) : (
          rows.map((r, i) => {
            const actions = rowActions(r);
            return (
              <RowContextMenu key={r.id} items={actions}>
                <TableRow
                  style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined}
                  className="cursor-pointer"
                >
                  <TableCell style={{ textAlign: 'center' }}>
                    <input type="checkbox" checked={selected.has(r.id)} onChange={() => onToggleSel(r.id)} />
                  </TableCell>
                  <CodeLinkCell code={r.docNumber} onOpen={() => onOpen(r)} />
                  <TableCell>{r.docDate.slice(0, 10)}</TableCell>
                  <TableCell>{r.customer?.name ?? '—'}</TableCell>
                  <TableCell>{r.description ?? '—'}</TableCell>
                  <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                    {formatNumber(Number(r.grandTotal), 2)}
                  </TableCell>
                  <TableCell>{r.currency?.code ?? '—'}</TableCell>
                  <TableCell>
                    <Badge variant={statusBadgeVariant(r.status)} dot>
                      {statusLabel(r.status)}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <RowActionsMenu items={actions} />
                  </TableCell>
                </TableRow>
              </RowContextMenu>
            );
          })
        )}
      </TableBody>
    </Table>
  );
}

export function SlsDeliveryOrdersBulkBar({
  selected,
  onClear,
  onReload,
}: {
  selected: Set<string>;
  onClear: () => void;
  onReload: () => void;
}) {
  if (selected.size === 0) return null;
  return (
    <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
      <strong>{selected.size}</strong> baris dipilih
      <button
        className="btn sm danger"
        onClick={() =>
          confirmAction({
            title: 'Hapus terpilih?',
            message: `${selected.size} Delivery Order akan dihapus permanen.`,
            variant: 'danger',
            confirmLabel: 'Hapus',
            onConfirm: async () => {
              await Promise.all([...selected].map((id) => deleteSlsDeliveryOrder(id).catch(() => null)));
              notify(`${selected.size} dokumen dihapus`, 'success');
              onClear();
              onReload();
            },
          })
        }
      >
        <Icon name="trash" size={12} /> Hapus
      </button>
      <button className="btn ghost sm" onClick={onClear}>
        Batal pilihan
      </button>
    </div>
  );
}
