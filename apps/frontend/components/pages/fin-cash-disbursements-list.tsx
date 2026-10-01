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
  CashDisbursementFilters,
  type CdFilters,
} from './fin-cash-disbursements-filters';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import type { ErpCashDisbursement } from '@/lib/api/fin-cash-disbursements';

export interface CashDisbursementsListProps {
  rows: ErpCashDisbursement[];
  loading: boolean;
  error: string | null;
  search: string;
  onSearch: (s: string) => void;
  onAdd: () => void;
  onRefresh: () => void;
  filters: CdFilters;
  onFiltersChange: (f: CdFilters) => void;
  selected: Set<string>;
  onToggleSelect: (id: string) => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  focused: number;
  onFocusChange: (i: number) => void;
  rowActions: (r: ErpCashDisbursement) => RowActionItem[];
  onEdit: (r: ErpCashDisbursement) => void;
  summary: SummaryConfig;
  pagination: ListPaginationConfig;
}

export function CashDisbursementsList({
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
  onBulkDelete,
  focused,
  onFocusChange,
  rowActions,
  onEdit,
  summary,
  pagination,
}: CashDisbursementsListProps) {
  return (
    <ErpListLayout
      title="Kas Keluar"
      code="CD"
      loading={loading}
      error={error}
      search={search}
      onSearch={onSearch}
      onAdd={onAdd}
      onRefresh={onRefresh}
      toolbar={<CashDisbursementFilters value={filters} onChange={onFiltersChange} />}
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
          <button className="btn sm danger" onClick={onBulkDelete}>
            <Icon name="trash" size={12} /> Hapus
          </button>
          <button className="btn ghost sm" onClick={onClearSelection}>Batal pilihan</button>
        </div>
      )}
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead style={{ width: 36 }} />
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Bayar Ke</TableHead>
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
                    style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined}
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
                    <TableCell>{r.partner?.name ?? r.contactPerson ?? '—'}</TableCell>
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