'use client';

/**
 * F2 Admin — Audit Logs page.
 * Read-only list of sys_audit_logs with filter by action and date range.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { DateInput } from '@/components/ui/date-input';
import { ErpListLayout } from '@/components/organisms/erp-list-layout';
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableHead,
  TableCell,
  TableEmpty,
} from '@/components/organisms/table';
import { useErpList } from '@/lib/use-erp-list';
import { useListPagination } from '@/lib/use-list-pagination';
import { listAuditLogs } from '@/lib/api/audit-logs';
import type { ErpAuditLog, AuditAction } from '@/lib/api/audit-logs';

// ─── Constants ────────────────────────────────────────────────────────────────

type BadgeVariant = 'success' | 'info' | 'danger' | 'warn' | 'default';

const ACTION_VARIANT: Record<AuditAction, BadgeVariant> = {
  CREATE: 'success',
  UPDATE: 'info',
  DELETE: 'danger',
  RESTORE: 'warn',
  LOGIN: 'info',
  LOGOUT: 'default',
  APPROVE: 'success',
  REJECT: 'danger',
  POST: 'info',
  REOPEN: 'warn',
};

const ACTION_OPTIONS = [
  { label: 'Semua', value: '' },
  { label: 'CREATE', value: 'CREATE' },
  { label: 'UPDATE', value: 'UPDATE' },
  { label: 'DELETE', value: 'DELETE' },
  { label: 'RESTORE', value: 'RESTORE' },
  { label: 'LOGIN', value: 'LOGIN' },
  { label: 'LOGOUT', value: 'LOGOUT' },
  { label: 'APPROVE', value: 'APPROVE' },
  { label: 'REJECT', value: 'REJECT' },
  { label: 'POST', value: 'POST' },
  { label: 'REOPEN', value: 'REOPEN' },
];

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatDateTime(iso: string): string {
  try {
    const d = new Date(iso);
    const dd = String(d.getDate()).padStart(2, '0');
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const yyyy = d.getFullYear();
    const hh = String(d.getHours()).padStart(2, '0');
    const min = String(d.getMinutes()).padStart(2, '0');
    const ss = String(d.getSeconds()).padStart(2, '0');
    return `${dd}/${mm}/${yyyy} ${hh}:${min}:${ss}`;
  } catch {
    return iso;
  }
}

function truncate(s: string | null, max: number): string {
  if (!s) return '—';
  return s.length > max ? s.slice(0, max) + '…' : s;
}

function resolveActor(row: ErpAuditLog): string {
  if (!row.actor) return 'System';
  return row.actor.name || row.actor.code || 'System';
}

// ─── Date filter toolbar ──────────────────────────────────────────────────────

interface DateRangeBarProps {
  from: string;
  to: string;
  onFrom: (v: string) => void;
  onTo: (v: string) => void;
}

function DateRangeBar({ from, to, onFrom, onTo }: DateRangeBarProps) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
      <span style={{ fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--fg-faint)' }}>
        Dari:
      </span>
      <DateInput
        id="al-from"
        value={from}
        onChange={(v) => onFrom(v)}
        className="w-[130px]"
      />
      <span style={{ fontSize: 'calc(11px * var(--font-scale, 1))', color: 'var(--fg-faint)' }}>
        Sampai:
      </span>
      <DateInput
        id="al-to"
        value={to}
        onChange={(v) => onTo(v)}
        className="w-[130px]"
      />
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpAuditLogsPage() {
  const { page, pageSize, setPage, setPageSize, resetPage } = useListPagination('audit-logs');

  // Search box text → forwarded to entityName filter on the backend
  const [search, setSearch] = React.useState('');
  const [debouncedSearch, setDebouncedSearch] = React.useState('');

  const [actionFilter, setActionFilter] = React.useState('');
  const [dateFrom, setDateFrom] = React.useState('');
  const [dateTo, setDateTo] = React.useState('');

  // Debounce search 300ms
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  // Reset page on filter/search change
  React.useEffect(() => {
    resetPage();
  }, [debouncedSearch, actionFilter, dateFrom, dateTo, resetPage]);

  const { rows, meta, loading, fetching, error, reload } = useErpList(
    () =>
      listAuditLogs({
        page,
        limit: pageSize,
        entityName: debouncedSearch || undefined,
        action: (actionFilter as AuditAction) || undefined,
        from: dateFrom || undefined,
        to: dateTo || undefined,
      }),
    [page, pageSize, debouncedSearch, actionFilter, dateFrom, dateTo],
  );

  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const hasFilter = !!actionFilter || !!dateFrom || !!dateTo;
  const isFiltered = !!debouncedSearch || hasFilter;

  const filters = [
    {
      key: 'action',
      label: 'Aksi',
      options: ACTION_OPTIONS,
      value: actionFilter,
      onChange: setActionFilter,
    },
  ];

  const toolbar = (
    <DateRangeBar
      from={dateFrom}
      to={dateTo}
      onFrom={setDateFrom}
      onTo={setDateTo}
    />
  );

  return (
    <ErpListLayout
      title="Audit Log"
      code="AUDITLOG"
      loading={loading}
      fetching={fetching}
      error={error}
      search={search}
      onSearch={setSearch}
      onRefresh={reload}
      filters={filters}
      toolbar={toolbar}
      summary={{
        metricLabel: 'Total log',
        rowCount: rows.length,
        totalCount: totalRows,
      }}
      pagination={{
        page,
        pageCount,
        pageSize,
        totalRows,
        onPage: setPage,
        onPageSize: setPageSize,
      }}
      keyboardHints={false}
    >
      <div className="lines">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead style={{ width: 150 }}>Waktu</TableHead>
              <TableHead style={{ width: 90 }}>Aksi</TableHead>
              <TableHead>Entitas</TableHead>
              <TableHead style={{ width: 100 }}>ID Entitas</TableHead>
              <TableHead>Ringkasan</TableHead>
              <TableHead style={{ width: 120 }}>Aktor</TableHead>
              <TableHead style={{ width: 120 }}>IP</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {rows.length === 0 ? (
              <TableEmpty
                colSpan={7}
                variant={isFiltered ? 'filtered' : 'empty'}
                searchTerm={debouncedSearch || undefined}
                icon="file"
                entityLabel="audit log"
              />
            ) : (
              rows.map((row) => (
                <TableRow key={row.id}>
                  <TableCell className="mono" style={{ whiteSpace: 'nowrap' }}>
                    {formatDateTime(row.occurredAt)}
                  </TableCell>
                  <TableCell>
                    <Badge variant={ACTION_VARIANT[row.action]} dot>
                      {row.action}
                    </Badge>
                  </TableCell>
                  <TableCell className="mono">{row.entityName}</TableCell>
                  <TableCell className="mono">{row.entityId ?? '—'}</TableCell>
                  <TableCell title={row.summary ?? undefined}>
                    {truncate(row.summary, 60)}
                  </TableCell>
                  <TableCell>{resolveActor(row)}</TableCell>
                  <TableCell className="mono">{row.actorIp ?? '—'}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </ErpListLayout>
  );
}
