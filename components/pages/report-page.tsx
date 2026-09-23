'use client';

/**
 * Generic finance-report page. Renders a page-header, a date-range (or as-of)
 * filter bar, an export toolbar, and the {@link ReportView} for the fetched
 * {@link ReportDocument}. Each concrete report (Trial Balance, Income
 * Statement, Balance Sheet, General Ledger) is a thin wrapper that supplies a
 * {@link ReportPageConfig}.
 */

import * as React from 'react';
import { Button } from '@/components/ui/button';
import { Icon } from '@/components/ui/icons';
import { DateInput } from '@/components/ui/date-input';
import { ErrorState } from '@/components/molecules/error-state';
import { ReportExportBar } from '@/components/molecules/report-export-bar';
import { ReportView } from '@/components/organisms/report-view';
import { notify } from '@/lib/feedback';
import {
  getReport,
  downloadReport,
  type ExportFormat,
  type ReportDocument,
  type ReportKey,
  type ReportParams,
} from '@/lib/api/fin-reports';

export interface ReportPageConfig {
  report: ReportKey;
  title: string;
  codeTag: string;
  /** 'range' → from/to dates; 'asof' → single as-of date. */
  mode: 'range' | 'asof';
}

function isoToday(): string {
  return new Date().toISOString().slice(0, 10);
}

function isoYearStart(): string {
  return `${new Date().getFullYear()}-01-01`;
}

/** Build the API params from the current filter state for a given mode. */
function buildParams(
  mode: 'range' | 'asof',
  from: string,
  to: string,
  asOf: string,
): ReportParams {
  return mode === 'asof' ? { asOf } : { from, to };
}

export function ReportPage({ config }: { config: ReportPageConfig }) {
  const { report, title, codeTag, mode } = config;

  const [from, setFrom] = React.useState(isoYearStart);
  const [to, setTo] = React.useState(isoToday);
  const [asOf, setAsOf] = React.useState(isoToday);

  const [doc, setDoc] = React.useState<ReportDocument | null>(null);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);
  const [exporting, setExporting] = React.useState(false);

  // Snapshot the params at fetch time so exports match what's on screen.
  const fetchedParams = React.useRef<ReportParams>(
    buildParams(mode, from, to, asOf),
  );

  const load = React.useCallback(async () => {
    const params = buildParams(mode, from, to, asOf);
    fetchedParams.current = params;
    setLoading(true);
    setError(null);
    try {
      const result = await getReport(report, params);
      setDoc(result);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Gagal memuat laporan';
      setError(msg);
      setDoc(null);
    } finally {
      setLoading(false);
    }
  }, [report, mode, from, to, asOf]);

  // Initial load on mount with default dates.
  React.useEffect(() => {
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleExport(format: ExportFormat) {
    setExporting(true);
    try {
      await downloadReport(report, fetchedParams.current, format);
    } catch (err) {
      const msg = err instanceof Error ? err.message : 'Export gagal';
      notify(msg, 'danger');
    } finally {
      setExporting(false);
    }
  }

  return (
    <div className="page">
      {/* Header */}
      <div className="page-header">
        <h1 className="page-title">
          {title}
          <span className="code-tag">{codeTag}</span>
        </h1>
        <div className="page-actions">
          <ReportExportBar onExport={handleExport} busy={exporting || loading} />
        </div>
      </div>

      {/* Filter bar */}
      <div className="filter-bar">
        {mode === 'range' ? (
          <>
            <label className="flex items-center gap-1.5 text-[11.5px] text-muted-foreground">
              Dari
              <div className="w-[150px]">
                <DateInput value={from} onChange={setFrom} />
              </div>
            </label>
            <label className="flex items-center gap-1.5 text-[11.5px] text-muted-foreground">
              Sampai
              <div className="w-[150px]">
                <DateInput value={to} onChange={setTo} />
              </div>
            </label>
          </>
        ) : (
          <label className="flex items-center gap-1.5 text-[11.5px] text-muted-foreground">
            Per Tanggal
            <div className="w-[150px]">
              <DateInput value={asOf} onChange={setAsOf} />
            </div>
          </label>
        )}
        <Button
          variant="primary"
          size="sm"
          onClick={() => void load()}
          disabled={loading}
        >
          <Icon name="refresh" size={12} />
          Tampilkan
        </Button>
      </div>

      {/* Body */}
      <div className="page-body">
        {error && <ErrorState message={error} onRetry={load} retrying={loading} />}
        {!error && loading && (
          <div className="p-6 text-center text-xs text-muted-foreground">
            Memuat...
          </div>
        )}
        {!error && !loading && doc && <ReportView doc={doc} />}
      </div>
    </div>
  );
}
