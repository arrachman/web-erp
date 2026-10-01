'use client';

/**
 * BOM list table — extracted from mfg-boms-page.tsx to stay under 400 lines (§3).
 * Atomic tier: Organism.
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
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { deleteMfgBom, type ErpMfgBom } from '@/lib/api/mfg-boms';

export function MfgBomTable({
  rows,
  focused,
  selected,
  onSelect,
  onSelectAll,
  onOpen,
  rowActions,
  onBulkDelete,
  onBulkClearSel,
  reload,
}: {
  rows: ErpMfgBom[];
  focused: number;
  selected: Set<string>;
  onSelect: (id: string) => void;
  onSelectAll: (checked: boolean) => void;
  onOpen: (r: ErpMfgBom) => void;
  rowActions: (r: ErpMfgBom) => RowActionItem[];
  onBulkDelete: () => void;
  onBulkClearSel: () => void;
  reload: () => void;
}) {
  const allChecked = rows.length > 0 && rows.every((r) => selected.has(r.id));
  const indeterminate = selected.size > 0 && !allChecked;

  return (
    <>
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button
            className="btn sm danger"
            onClick={() =>
              confirmAction({
                title: 'Hapus terpilih?',
                message: `${selected.size} Bill of Materials akan dihapus permanen.`,
                variant: 'danger',
                confirmLabel: 'Hapus',
                onConfirm: async () => {
                  await Promise.all(
                    [...selected].map((id) => deleteMfgBom(id).catch(() => null)),
                  );
                  notify(`${selected.size} dokumen dihapus`, 'success');
                  onBulkClearSel();
                  reload();
                },
              })
            }
          >
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={onBulkClearSel}>
            Batal pilihan
          </button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36, textAlign: 'center' }}>
              <input
                type="checkbox"
                checked={allChecked}
                ref={(el) => {
                  if (el) el.indeterminate = indeterminate;
                }}
                onChange={(e) => onSelectAll(e.target.checked)}
                title="Pilih semua"
              />
            </TableHead>
            <TableHead>No BOM</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Cabang</TableHead>
            <TableHead>Gudang Produksi</TableHead>
            <TableHead>Keterangan</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={8} />
          ) : (
            rows.map((r, i) => {
              const actions = rowActions(r);
              return (
                <RowContextMenu key={r.id} items={actions}>
                  <TableRow
                    style={
                      focused === i
                        ? { boxShadow: 'inset 2px 0 0 var(--primary)' }
                        : undefined
                    }
                    className="cursor-pointer"
                  >
                    <TableCell style={{ textAlign: 'center' }}>
                      <input
                        type="checkbox"
                        checked={selected.has(r.id)}
                        onChange={() => onSelect(r.id)}
                      />
                    </TableCell>
                    <CodeLinkCell code={r.docNumber} onOpen={() => onOpen(r)} />
                    <TableCell>{r.docDate.slice(0, 10)}</TableCell>
                    <TableCell>{r.branch?.name ?? '—'}</TableCell>
                    <TableCell>{r.productionWarehouse?.name ?? '—'}</TableCell>
                    <TableCell>{r.description ?? '—'}</TableCell>
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
    </>
  );
}
