'use client';

/**
 * Generic purchasing (M4) report page.
 *
 * Given a report `key`, fetches the `ReportDataset` from
 * `GET /pur/reports/:key`, renders the filter/export `PurReportToolbar` plus a
 * `ReportTable` (columns + summary driven by the dataset). Filter state lives
 * here; the search field is debounced 300ms (per CLAUDE.md §2.12). Loading /
 * error / empty states come from `ErpListLayout`.
 *
 * Routing is wired by the shell-route renderer (not here).
 *
 * Atomic tier: Page.
 */

import * as React from 'react';
import { ErpListLayout } from '@/components/organisms/erp-list-layout';
import { PurReportToolbar, type StatusOption } from '@/components/organisms/pur-report-toolbar';
import { ReportTable } from '@/components/organisms/report-table';
import {
  getReportData,
  type ReportDataset,
  type ReportFilters,
} from '@/lib/api/pur-reports';
import { ErpApiError } from '@/lib/api/client';

const DEFAULT_LIMIT = 50;

export interface PurReportPageProps {
  /** Backend report key (path segment of /pur/reports/:key). */
  reportKey: string;
  /** Fallback title shown until the dataset arrives. */
  title?: string;
  /** Use the as-of date field instead of a date range. */
  asOfMode?: boolean;
  /** Show the vendor picker in the toolbar. */
  showVendor?: boolean;
  /** Show the item picker in the toolbar. */
  showItem?: boolean;
  /** Status filter options (hidden when omitted). */
  statusOptions?: StatusOption[];
}

export function PurReportPage({
  reportKey,
  title,
  asOfMode,
  showVendor,
  showItem,
  statusOptions,
}: PurReportPageProps) {
  const [filters, setFilters] = React.useState<ReportFilters>({
    page: 1,
    limit: DEFAULT_LIMIT,
  });
  const [dataset, setDataset] = React.useState<ReportDataset | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  // Debounced search — mirror §2.12. The toolbar updates `filters.search`
  // immediately (controlled input); we debounce the value that actually drives
  // the fetch so typing doesn't spam the backend.
  const [debouncedSearch, setDebouncedSearch] = React.useState<string | undefined>(
    undefined,
  );
  React.useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(filters.search), 300);
    return () => clearTimeout(t);
  }, [filters.search]);

  // Manual "apply" trigger (Tampilkan button / refresh) — bump to refetch.
  const [applyTick, setApplyTick] = React.useState(0);

  const fetchData = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await getReportData(reportKey, {
        ...filters,
        search: debouncedSearch,
      });
      setDataset(data);
    } catch (err) {
      const msg =
        err instanceof ErpApiError || err instanceof Error
          ? err.message
          : 'Gagal memuat laporan';
      setError(msg);
      setDataset(null);
    } finally {
      setLoading(false);
    }
    // filters captured fresh on each call; deps intentionally narrow.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    reportKey,
    debouncedSearch,
    filters.dateFrom,
    filters.dateTo,
    filters.asOfDate,
    filters.vendorId,
    filters.itemId,
    filters.status,
    filters.page,
    filters.limit,
    applyTick,
  ]);

  React.useEffect(() => {
    void fetchData();
  }, [fetchData]);

  const handleChange = React.useCallback((patch: Partial<ReportFilters>) => {
    setFilters((prev) => {
      const next = { ...prev, ...patch };
      // Any filter change (except paging) resets to page 1.
      if (!('page' in patch)) next.page = 1;
      return next;
    });
  }, []);

  const handleRefresh = React.useCallback(() => {
    setApplyTick((n) => n + 1);
  }, []);

  const resolvedTitle = dataset?.title || title || 'Laporan';

  const pageCount =
    dataset && filters.limit
      ? Math.max(1, Math.ceil(dataset.total / filters.limit))
      : 1;

  return (
    <ErpListLayout
      title={resolvedTitle}
      code={reportKey}
      loading={loading}
      error={error}
      search={filters.search ?? ''}
      onSearch={(q) => handleChange({ search: q || undefined })}
      onRefresh={handleRefresh}
      toolbar={
        <PurReportToolbar
          reportKey={reportKey}
          filters={{ ...filters, search: filters.search }}
          onChange={handleChange}
          onRefresh={handleRefresh}
          asOfMode={asOfMode}
          showVendor={showVendor}
          showItem={showItem}
          statusOptions={statusOptions}
          busy={loading}
        />
      }
      summary={
        dataset
          ? {
              metricLabel: 'Dibuat',
              metricValue: dataset.generatedAt
                ? new Date(dataset.generatedAt).toLocaleString('id-ID')
                : undefined,
              rowCount: dataset.rows.length,
              totalCount: dataset.total,
            }
          : undefined
      }
      pagination={
        dataset && pageCount > 1
          ? {
              page: filters.page ?? 1,
              pageCount,
              pageSize: filters.limit ?? DEFAULT_LIMIT,
              totalRows: dataset.total,
              onPage: (p) => handleChange({ page: p }),
            }
          : undefined
      }
    >
      {dataset && <ReportTable dataset={dataset} searchTerm={debouncedSearch} />}
    </ErpListLayout>
  );
}
