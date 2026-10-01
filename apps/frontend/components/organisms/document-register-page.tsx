'use client';

/**
 * Reusable read-only "Data register" page (legacy DATA group). Atomic tier: Organism.
 *
 * Config-driven (see `lib/registers/register-config.ts`): lists one document type
 * across ALL statuses with search + status filter + server-driven pagination +
 * CSV export + keyboard nav. The document code links out to the existing TX edit
 * form (`<editBase>/<id>`). Registers never create/edit/delete — entry stays in TX.
 *
 * Standard list features per CLAUDE.md §2.7/§2.9 (minus create/bulk, which a
 * read-only register has by design).
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout,
  type FilterConfig,
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
  RowActionsMenu,
  RowContextMenu,
  type RowActionItem,
} from '@/components/molecules/row-actions-menu';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { trxEditRoute } from '@/lib/trx-route';
import { formatDate } from '@/lib/date-format';
import { statusBadgeVariant, statusLabel, DOC_STATUS_FILTER_OPTIONS } from '@/lib/status';
import { exportRowsToCsv, type CsvColumn } from '@/lib/export-csv';
import { notify } from '@/lib/feedback';
import type { AnyDocumentRegisterConfig } from '@/lib/registers/register-config';

interface DocumentRegisterPageProps {
  config: AnyDocumentRegisterConfig;
  /** Navigate within the active tab (used to open the TX edit form). */
  onNavigate?: (route: string) => void;
}

// Row is erased at the registry boundary; the renderer treats rows opaquely and
// defers all field access to the config's accessors.
// eslint-disable-next-line @typescript-eslint/no-explicit-any
type Row = any;

export function DocumentRegisterPage({ config, onNavigate }: DocumentRegisterPageProps) {
  const showStatus = config.hasStatus !== false;
  const showDate = !!config.getDocDate;

  const [search, setSearch] = React.useState('');
  const [status, setStatus] = React.useState('');
  const { page, pageSize, setPage, setPageSize } = useListPagination(`register:${config.code}`);

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  React.useEffect(() => {
    setPage(1);
  }, [debouncedSearch, status, pageSize, setPage]);

  const { rows, meta, loading, fetching, error, reload } = useErpList<Row>(
    () =>
      config.list({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
        status: showStatus ? status || undefined : undefined,
        sortBy: config.sortBy,
        sortDir: config.sortDir ?? 'desc',
        ...(config.extraParams ?? {}),
      }),
    [page, pageSize, debouncedSearch, status],
  );

  const [focused, setFocused] = React.useState(-1);
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const openRow = React.useCallback(
    (r: Row) => {
      if (!config.editBase) return;
      if (!onNavigate) {
        notify('Navigasi tidak tersedia di konteks ini', 'warn');
        return;
      }
      onNavigate(trxEditRoute(config.editBase, config.getId(r)));
    },
    [config, onNavigate],
  );

  const rowActions = (r: Row): RowActionItem[] =>
    config.editBase ? [{ label: 'Lihat di Transaksi', onSelect: () => openRow(r) }] : [];

  const handleExport = () => {
    if (rows.length === 0) {
      notify('Tidak ada data untuk diexport', 'warn');
      return;
    }
    const csvCols: CsvColumn<Row>[] = [
      { header: 'No Transaksi', value: (r) => config.getDocNumber(r) },
      ...(showDate ? [{ header: 'Tanggal', value: (r: Row) => formatDate(config.getDocDate!(r)) }] : []),
      ...config.columns.map((c) => ({
        header: c.header,
        value: (r: Row) => (c.csv ? c.csv(r) : null),
      })),
      ...(showStatus
        ? [{ header: 'Status', value: (r: Row) => statusLabel(config.getStatus?.(r) ?? '') }]
        : []),
    ];
    exportRowsToCsv(`${config.code.toLowerCase()}-register.csv`, rows, csvCols);
    notify(`${rows.length} baris diexport`, 'success');
  };

  const statusOpts = config.statusOptions ?? DOC_STATUS_FILTER_OPTIONS;
  const filters: FilterConfig[] = showStatus
    ? [
        {
          key: 'status',
          label: 'Status',
          options: statusOpts.map((o) => ({ value: o.value, label: o.label })),
          value: status,
          onChange: setStatus,
        },
      ]
    : [];

  const summary: SummaryConfig = {
    metricLabel: config.summaryLabel ?? `Σ ${config.title}`,
    rowCount: rows.length,
    totalCount: totalRows,
  };
  const pagination: ListPaginationConfig = {
    page,
    pageCount,
    pageSize,
    totalRows,
    onPage: setPage,
    onPageSize: setPageSize,
  };

  const colCount = 1 + (showDate ? 1 : 0) + config.columns.length + (showStatus ? 1 : 0) + (config.editBase ? 1 : 0);

  return (
    <ErpListLayout
      title={config.title}
      code={config.code}
      loading={loading}
      fetching={fetching}
      error={error}
      search={search}
      onSearch={setSearch}
      onRefresh={reload}
      onExport={handleExport}
      filters={filters}
      summary={summary}
      pagination={pagination}
      keyboardRows={{
        rowCount: rows.length,
        focusedIndex: focused,
        onFocusChange: setFocused,
        onToggle: () => undefined,
        onOpen: (i) => rows[i] && openRow(rows[i]),
      }}
    >
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>No Transaksi</TableHead>
            {showDate && <TableHead>Tanggal</TableHead>}
            {config.columns.map((c) => (
              <TableHead key={c.header} style={c.align === 'right' ? { textAlign: 'right' } : undefined}>
                {c.header}
              </TableHead>
            ))}
            {showStatus && <TableHead>Status</TableHead>}
            {config.editBase && <TableHead style={{ width: 44 }} />}
          </TableRow>
        </TableHeader>
        <TableBody>
          {rows.length === 0 ? (
            <TableEmpty colSpan={colCount} />
          ) : (
            rows.map((r, i) => {
              const actions = rowActions(r);
              const id = config.getId(r);
              const docNumber = config.getDocNumber(r);
              const body = (
                <TableRow
                  style={focused === i ? { boxShadow: 'inset 2px 0 0 var(--primary)' } : undefined}
                  className={config.editBase ? 'cursor-pointer' : undefined}
                >
                  {config.editBase ? (
                    <CodeLinkCell code={docNumber} onOpen={() => openRow(r)} />
                  ) : (
                    <TableCell>{docNumber}</TableCell>
                  )}
                  {showDate && <TableCell>{formatDate(config.getDocDate!(r))}</TableCell>}
                  {config.columns.map((c) => (
                    <TableCell
                      key={c.header}
                      className={c.align === 'right' ? 'tabular-nums' : undefined}
                      style={c.align === 'right' ? { textAlign: 'right' } : c.align === 'center' ? { textAlign: 'center' } : undefined}
                    >
                      {c.render(r)}
                    </TableCell>
                  ))}
                  {showStatus && (
                    <TableCell>
                      <Badge variant={statusBadgeVariant(config.getStatus?.(r) ?? '')} dot>
                        {statusLabel(config.getStatus?.(r) ?? '')}
                      </Badge>
                    </TableCell>
                  )}
                  {config.editBase && (
                    <TableCell>
                      <RowActionsMenu items={actions} />
                    </TableCell>
                  )}
                </TableRow>
              );
              return config.editBase ? (
                <RowContextMenu key={id} items={actions}>
                  {body}
                </RowContextMenu>
              ) : (
                <React.Fragment key={id}>{body}</React.Fragment>
              );
            })
          )}
        </TableBody>
      </Table>
    </ErpListLayout>
  );
}
