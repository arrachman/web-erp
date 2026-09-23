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
  SlsPackingListFiltersPanel,
  type SlsPackingListFilters,
} from './sls-packing-list-filters';
import {
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { notify } from '@/lib/feedback';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import type { ErpSlsPackingList } from '@/lib/api/sls-packing-lists';
import { LIST_COLS } from './sls-packing-lists-config';

export interface SlsPackingListsListProps {
  rows: ErpSlsPackingList[];
  loading: boolean;
  error: string | null;
  search: string;
  onSearch: (s: string) => void;
  onAdd: () => void;
  onRefresh: () => void;
  filters: SlsPackingListFilters;
  onFiltersChange: (f: SlsPackingListFilters) => void;
  selected: Set<string>;
  onToggleSelect: (id: string) => void;
  onSelectAll: (ids: string[]) => void;
  onClearSelection: () => void;
  onBulkDelete: () => void;
  sortBy: string;
  sortDir: 'asc' | 'desc';
  onSort: (col: string) => void;
  focused: number;
  onFocusChange: (i: number) => void;
  rowActions: (r: ErpSlsPackingList) => RowActionItem[];
  onEdit: (r: ErpSlsPackingList) => void;
  summary: SummaryConfig;
  pagination: ListPaginationConfig;
}

export function SlsPackingListsList({
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
  onSelectAll,
  onClearSelection,
  onBulkDelete,
  sortBy,
  sortDir,
  onSort,
  focused,
  onFocusChange,
  rowActions,
  onEdit,
  summary,
  pagination,
}: SlsPackingListsListProps) {
  return (
    <ErpListLayout
      title="Packing List"
      code="PL"
      loading={loading}
      error={error}
      search={search}
      onSearch={onSearch}
      onAdd={onAdd}
      onRefresh={onRefresh}
      toolbar={
        <>
          <SlsPackingListFiltersPanel value={filters} onChange={onFiltersChange} />
          <button
            type="button"
            className="btn ghost sm"
            onClick={() => notify('Export akan tersedia segera.', 'info')}
            title="Export ke CSV/Excel"
          >
            <Icon name="download" size={12} /> Export
          </button>
        </>
      }
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
            <TableHead style={{ width: 36, textAlign: 'center' }}>
              <input
                type="checkbox"
                checked={rows.length > 0 && rows.every((r) => selected.has(r.id))}
                ref={(el) => {
                  if (el) el.indeterminate = selected.size > 0 && !rows.every((r) => selected.has(r.id));
                }}
                onChange={(e) =>
                  onSelectAll(e.target.checked ? rows.map((r) => r.id) : [])
                }
                title="Pilih semua"
              />
            </TableHead>
            {LIST_COLS.map(([col, label]) => (
              <TableHead
                key={label}
                style={
                  col === 'grandTotal'
                    ? { textAlign: 'right', cursor: col ? 'pointer' : undefined }
                    : { cursor: col ? 'pointer' : undefined }
                }
                onClick={col ? () => onSort(col) : undefined}
              >
                {label}
                {col && sortBy === col && (
                  <span className="ml-1 text-muted-foreground text-xs">
                    {sortDir === 'asc' ? '↑' : '↓'}
                  </span>
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
                      <input
                        type="checkbox"
                        checked={selected.has(r.id)}
                        onChange={() => onToggleSelect(r.id)}
                      />
                    </TableCell>
                    <CodeLinkCell code={r.docNumber} onOpen={() => onEdit(r)} />
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
    </ErpListLayout>
  );
}