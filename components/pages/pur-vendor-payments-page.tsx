'use client';

/**
 * Vendor Payment (VP) — list. URL: /purchasing/vendor-payments · /new · /:id.
 * Reuses `fin_ap_payments` (Finance domain, sourced from purchasing side).
 * The list shows payments where source relates to purchasing; form = coming soon.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout, type ListPaginationConfig, type SummaryConfig,
} from '@/components/organisms/erp-list-layout';
import {
  Table, TableHeader, TableBody, TableRow, TableHead, TableCell, TableEmpty, CodeLinkCell,
} from '@/components/organisms/table';
import { RowActionsMenu, RowContextMenu, type RowActionItem } from '@/components/molecules/row-actions-menu';
import { type TrxFormPageProps, trxNewRoute, trxEditRoute } from '@/lib/trx-route';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { formatNumber } from '@/lib/format';
import { statusBadgeVariant, statusLabel } from '@/lib/status';
import { listApPayments, type ErpApPayment } from '@/lib/api/fin-ap-payments';
import { PurVendorPaymentForm } from './pur-vendor-payment-form';

const BASE = '/purchasing/vendor-payments';

export function ErpVendorPaymentsPage({ formMode, recordId, onNavigate }: TrxFormPageProps = {}) {
  const mode: 'list' | 'form' = formMode ? 'form' : 'list';
  const goList = React.useCallback(() => onNavigate?.(BASE), [onNavigate]);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination('pur-vendor-payments');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => { const t = setTimeout(() => setDebouncedSearch(search), 300); return () => clearTimeout(t); }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () => listApPayments({ page, limit: pageSize, search: debouncedSearch || undefined, status: (statusFilter || undefined) as ErpApPayment['status'] | undefined, sortBy: 'transactionDate', sortDir: 'desc' }),
    [page, pageSize, debouncedSearch, statusFilter],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, statusFilter, pageSize]);

  const [focused, setFocused] = React.useState(-1);
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openEdit = (r: ErpApPayment) => onNavigate?.(trxEditRoute(BASE, r.id));
  const rowActions = (r: ErpApPayment): RowActionItem[] => [{ label: 'Lihat', onSelect: () => openEdit(r) }];

  if (mode === 'form') {
    return (
      <div className="page">
        <div className="page-header">
          <h1 className="page-title flex items-center gap-2">
            <button className="iconbtn" onClick={goList} title="Kembali" style={{ fontSize: 18, lineHeight: 1 }}>←</button>
            Pembayaran Vendor <span className="code-tag">VP</span>
          </h1>
        </div>
        <div className="page-body overflow-auto p-4">
          <PurVendorPaymentForm transactionCode="PUR.VP" formMode={formMode} recordId={recordId} onBack={goList} />
        </div>
      </div>
    );
  }

  const summary: SummaryConfig = { metricLabel: 'Σ Pembayaran Vendor', rowCount: rows.length, totalCount: totalRows };
  const pagination: ListPaginationConfig = { page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize };

  return (
    <ErpListLayout title="Pembayaran Vendor (VP)" code="VP" loading={loading} error={error}
      search={search} onSearch={setSearch} onAdd={() => onNavigate?.(trxNewRoute(BASE))} onRefresh={reload}
      toolbar={null} summary={summary} pagination={pagination}
      keyboardRows={{ rowCount: rows.length, focusedIndex: focused, onFocusChange: setFocused, onToggle: () => null, onOpen: (i) => rows[i] && openEdit(rows[i]) }}>
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>No Transaksi</TableHead>
            <TableHead>Tanggal</TableHead>
            <TableHead>Supplier</TableHead>
            <TableHead>Uraian</TableHead>
            <TableHead style={{ textAlign: 'right' }}>Jumlah</TableHead>
            <TableHead>Status</TableHead>
            <TableHead style={{ width: 44 }} />
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? <TableEmpty colSpan={7} /> : rows.map((r, i) => {
            const actions = rowActions(r);
            return (
              <RowContextMenu key={r.id} items={actions}>
                <TableRow style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined} className="cursor-pointer">
                  <CodeLinkCell code={r.docNumber} onOpen={() => openEdit(r)} />
                  <TableCell>{r.transactionDate.slice(0, 10)}</TableCell>
                  <TableCell>{r.partnerId}</TableCell>
                  <TableCell>{r.description}</TableCell>
                  <TableCell className="tabular-nums" style={{ textAlign: 'right' }}>{formatNumber(Number(r.amount), 2)}</TableCell>
                  <TableCell><Badge variant={statusBadgeVariant(r.status)} dot>{statusLabel(r.status)}</Badge></TableCell>
                  <TableCell><RowActionsMenu items={actions} /></TableCell>
                </TableRow>
              </RowContextMenu>
            );
          })}
        </TableBody>
      </Table>
    </ErpListLayout>
  );
}
