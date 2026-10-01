import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout,
  type ListPaginationConfig,
  type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
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
  CashReceiptFilters,
  type CrFilters,
} from './fin-cash-receipts-filters';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { confirmAction, notify } from '@/lib/feedback';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import {
  deleteCashReceipt,
  type ErpCashReceipt,
} from '@/lib/api/fin-cash-receipts';

export interface CashReceiptsListProps {
  rows: ErpCashReceipt[];
  loading: boolean;
  error: string | null;
  search: string;
  onSearch: (s: string) => void;
  onAdd: () => void;
  onRefresh: () => void;
  filters: CrFilters;
  onFiltersChange: (f: CrFilters) => void;
  selected: Set<string>;
  onToggleSelect: (id: string) => void;
  onClearSelection: () => void;
  focused: number;
  onFocusChange: (i: number) => void;
  rowActions: (r: ErpCashReceipt) => RowActionItem[];
  onEdit: (r: ErpCashReceipt) => void;
  summary: SummaryConfig;
  pagination: ListPaginationConfig;
}

export function CashReceiptsList({
  rows,
  loading,
  error,
  search,
  onSearch,
  onAdd,
  onRefresh,
  filters,
  onFiltersChange,
  selected,
  onToggleSelect,
  onClearSelection,
  focused,
  onFocusChange,
  rowActions,
  onEdit,
  summary,
  pagination,
}: CashReceiptsListProps) {
  return (
    <ErpListLayout
      title="Kas Masuk"
      code="CR"
      loading={loading}
      error={error}
      search={search}
      onSearch={onSearch}
      onAdd={onAdd}
      onRefresh={onRefresh}
      toolbar={<CashReceiptFilters value={filters} onChange={onFiltersChange} />}
      summary={summary}
      pagination={pagination}
      keyboardRows={{
        rowCount: rows.length,
        focusedIndex: focused,
        onFocusChange,
        onToggle: (i) => rows[i] && onToggleSelect(rows[i].id),
        onOpen: (i) => rows[i] && onEdit(rows[i]),
      }}
    >
      {selected.size > 0 && (
        <div className="bulk-bar flex items-center gap-3 px-3 py-2 mb-2 rounded-md bg-secondary text-sm">
          <strong>{selected.size}</strong> baris dipilih
          <button
            className="btn sm danger"
            onClick={() =>
              confirmAction({
                title: 'Hapus terpilih?',
                message: `${selected.size} Kas Masuk akan dihapus permanen.`,
                variant: 'danger',
                confirmLabel: 'Hapus',
                onConfirm: async () => {
                  await Promise.all(
                    [...selected].map((id) =>
                      deleteCashReceipt(id).catch(() => null),
                    ),
                  );
                  notify(`${selected.size} dokumen dihapus`, 'success');
                  onClearSelection();
                  onRefresh();
                },
              })
            }
          >
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={onClearSelection}>
            Batal pilihan
          </button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Terima Dari</TableHead>
            <TableHead>Uraian</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Total</TableHead>
            <TableHead>Uang</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Kurs</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={10} />
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
                        onChange={() => onToggleSelect(r.id)}
                      />
                    </TableCell>
                    <CodeLinkCell code={r.docNumber} onOpen={() => onEdit(r)} />
                    <TableCell>{r.transactionDate.slice(0, 10)}</TableCell>
                    <TableCell>
                      {r.partner?.name ?? r.contactPerson ?? '—'}
                    </TableCell>
                    <TableCell>{r.description}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(Number(r.amount), 2)}
                    </TableCell>
                    <TableCell>{r.currency?.code ?? '—'}</TableCell>
                    <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>
                      {formatNumber(Number(r.exchangeRate), 2)}
                    </TableCell>
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
    </ErpListLayout>
  );
}