'use client';

import * as React from 'react';
import { ErpListLayout } from '@/components/organisms/erp-list-layout';
import { FinDocReportToolbar } from '@/components/organisms/fin-doc-report-toolbar';
import { ReportCharts } from '@/components/organisms/report-charts';
import { ReportTable } from '@/components/organisms/report-table';
import { ErpApiError } from '@/lib/api/client';
import {
  getReportData,
  type ReportDataset,
  type ReportFilters,
} from '@/lib/api/fin-doc-reports';

const DEFAULT_LIMIT = 50;

export interface FinDocReportPageProps {
  reportKey: string;
  title?: string;
}

export function FinDocReportPage({ reportKey, title }: FinDocReportPageProps) {
  const [filters, setFilters] = React.useState<ReportFilters>({ page: 1, limit: DEFAULT_LIMIT });
  const [dataset, setDataset] = React.useState<ReportDataset | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [debouncedSearch, setDebouncedSearch] = React.useState<string | undefined>();
  const [applyTick, setApplyTick] = React.useState(0);

  React.useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(filters.search), 300);
    return () => clearTimeout(timer);
  }, [filters.search]);

  const fetchData = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setDataset(await getReportData(reportKey, { ...filters, search: debouncedSearch }));
    } catch (cause) {
      setError(cause instanceof ErpApiError || cause instanceof Error ? cause.message : 'Gagal memuat laporan');
      setDataset(null);
    } finally {
      setLoading(false);
    }
    // Filters are intentionally enumerated to avoid refetching on unrelated object identity changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [reportKey, debouncedSearch, filters.dateFrom, filters.dateTo, filters.partnerId, filters.status, filters.page, filters.limit, applyTick]);

  React.useEffect(() => {
    void fetchData();
  }, [fetchData]);

  const handleChange = React.useCallback((patch: Partial<ReportFilters>) => {
    setFilters((previous) => ({
      ...previous,
      ...patch,
      ...('page' in patch ? {} : { page: 1 }),
    }));
  }, []);

  const pageCount = dataset && filters.limit ? Math.max(1, Math.ceil(dataset.total / filters.limit)) : 1;

  return (
    <ErpListLayout
      title={dataset?.title || title || 'Laporan Dokumen Finance'}
      code={reportKey}
      loading={loading}
      error={error}
      search={filters.search ?? ''}
      onSearch={(search) => handleChange({ search: search || undefined })}
      onRefresh={() => setApplyTick((value) => value + 1)}
      toolbar={(
        <FinDocReportToolbar
          reportKey={reportKey}
          filters={filters}
          dataset={dataset}
          busy={loading}
          onChange={handleChange}
          onRefresh={() => setApplyTick((value) => value + 1)}
        />
      )}
      summary={dataset ? {
        metricLabel: 'Dibuat',
        metricValue: dataset.generatedAt ? new Date(dataset.generatedAt).toLocaleString('id-ID') : undefined,
        rowCount: dataset.rows.length,
        totalCount: dataset.total,
      } : undefined}
      pagination={dataset && pageCount > 1 ? {
        page: filters.page ?? 1,
        pageCount,
        pageSize: filters.limit ?? DEFAULT_LIMIT,
        totalRows: dataset.total,
        onPage: (page) => handleChange({ page }),
      } : undefined}
    >
      {dataset && (
        <div data-report-content className="flex min-h-0 flex-1 flex-col gap-3">
          <ReportCharts charts={dataset.charts} />
          <ReportTable dataset={dataset} searchTerm={debouncedSearch} />
        </div>
      )}
    </ErpListLayout>
  );
}
