'use client';

/**
 * F2 Admin — Permissions page (read-only).
 * Lists adm_permissions; no create / edit / delete.
 * Server-driven pagination (page/limit/search → backend).
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import {
  ErpListLayout,
  type SummaryConfig,
  type ListPaginationConfig,
} from '@/components/organisms/erp-list-layout';
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
import { listPermissions } from '@/lib/api/permissions';

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpPermissionsPage() {
  const [search, setSearch] = React.useState('');
  const { page, pageSize, setPage, setPageSize } =
    useListPagination('permissions');

  const [debouncedSearch, setDebouncedSearch] = React.useState(search);
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { rows, meta, loading, error, reload } = useErpList(
    () =>
      listPermissions({
        page,
        limit: pageSize,
        search: debouncedSearch || undefined,
      }),
    [page, pageSize, debouncedSearch],
  );

  React.useEffect(() => { setPage(1); }, [debouncedSearch, pageSize]);

  const paged = rows;
  const totalRows = meta?.total ?? 0;
  const pageCount = meta?.totalPages ?? 1;

  const permSummary: SummaryConfig = {
    metricLabel: 'Σ permission',
    rowCount: totalRows,
    totalCount: totalRows,
  };
  const permPagination: ListPaginationConfig = {
    page, pageCount, pageSize, totalRows, onPage: setPage, onPageSize: setPageSize,
  };

  return (
    <ErpListLayout
      title="Hak Akses"
      code="PERM"
      loading={loading}
      error={error}
      search={search}
      onSearch={setSearch}
      onRefresh={reload}
      summary={permSummary}
      pagination={permPagination}
    >
      <div className="lines">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Kode</TableHead>
              <TableHead>Nama</TableHead>
              <TableHead>Grup</TableHead>
              <TableHead>Deskripsi</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {paged.length === 0 ? (
              <TableEmpty colSpan={4} />
            ) : (
              paged.map((p) => (
                <TableRow key={p.id}>
                  <TableCell className="mono">{p.code}</TableCell>
                  <TableCell>{p.name}</TableCell>
                  <TableCell>
                    {p.group ? (
                      <Badge variant="default">{p.group}</Badge>
                    ) : (
                      '—'
                    )}
                  </TableCell>
                  <TableCell>{p.description ?? '—'}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </ErpListLayout>
  );
}
