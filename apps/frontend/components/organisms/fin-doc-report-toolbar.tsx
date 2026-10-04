'use client';

import * as React from 'react';
import { ReportOutputActions, type ReportOutputData } from '@/components/molecules/report-output-actions';
import { SearchSelect } from '@/components/molecules/search-select';
import { Button } from '@/components/ui/button';
import { DateInput } from '@/components/ui/date-input';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
import { notify } from '@/lib/feedback';
import { buildLookupLoader } from '@/lib/lookup-source-registry';
import { tGlobal } from '@/lib/mock';
import { downloadReport, type ReportExportFormat, type ReportFilters } from '@/lib/api/fin-doc-reports';

interface FinDocReportToolbarProps {
  reportKey: string;
  filters: ReportFilters;
  dataset: ReportOutputData | null;
  busy?: boolean;
  onChange: (patch: Partial<ReportFilters>) => void;
  onRefresh: () => void;
}

const PARTNER_LOADER = buildLookupLoader('partners');
const EXPORTS: Array<{ format: ReportExportFormat; label: string }> = [
  { format: 'xlsx', label: 'Excel' },
  { format: 'pdf', label: 'PDF' },
  { format: 'docx', label: 'Word' },
];

export function FinDocReportToolbar({
  reportKey,
  filters,
  dataset,
  busy,
  onChange,
  onRefresh,
}: FinDocReportToolbarProps) {
  const [exporting, setExporting] = React.useState<ReportExportFormat | null>(null);
  const handleExport = React.useCallback(async (format: ReportExportFormat) => {
    if (exporting) return;
    setExporting(format);
    try {
      await downloadReport(reportKey, format, filters);
      notify(tGlobal('Berkas berhasil diunduh'), 'success');
    } catch (error) {
      notify(error instanceof Error ? error.message : tGlobal('Unduhan gagal'), 'danger');
    } finally {
      setExporting(null);
    }
  }, [exporting, filters, reportKey]);

  return (
    <div data-report-toolbar className="flex flex-wrap items-center gap-2">
      <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
        {tGlobal('Dari')}
        <DateInput
          value={filters.dateFrom ?? ''}
          onChange={(dateFrom) => onChange({ dateFrom: dateFrom || undefined })}
          disabled={busy}
        />
      </label>
      <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
        {tGlobal('Sampai')}
        <DateInput
          value={filters.dateTo ?? ''}
          onChange={(dateTo) => onChange({ dateTo: dateTo || undefined })}
          disabled={busy}
        />
      </label>
      {PARTNER_LOADER && (
        <div className="min-w-[12rem]">
          <SearchSelect
            placeholder={tGlobal('Semua partner')}
            title={tGlobal('Pilih Partner')}
            value={filters.partnerId ?? ''}
            onValueChange={(partnerId) => onChange({ partnerId: partnerId || undefined })}
            loadOptions={PARTNER_LOADER}
            disabled={busy}
          />
        </div>
      )}
      <div className="search-input">
        <Icon name="search" size={12} />
        <Input
          className="border-0 bg-transparent px-0 shadow-none focus-visible:ring-0"
          placeholder={tGlobal('Cari nomor dokumen...')}
          value={filters.search ?? ''}
          onChange={(event) => onChange({ search: event.target.value || undefined })}
          disabled={busy}
        />
      </div>
      <Button variant="primary" className="cursor-pointer" onClick={onRefresh} disabled={busy}>
        <Icon name="refresh" size={12} />
        {tGlobal('Tampilkan')}
      </Button>
      <div className="ml-auto flex items-center gap-1.5">
        {EXPORTS.map(({ format, label }) => (
          <Button
            key={format}
            variant="default"
            className="cursor-pointer"
            onClick={() => void handleExport(format)}
            disabled={exporting !== null}
          >
            <Icon name={exporting === format ? 'refresh' : 'download'} size={12} />
            {tGlobal(label)}
          </Button>
        ))}
        <ReportOutputActions dataset={dataset} disabled={busy || exporting !== null} />
      </div>
    </div>
  );
}
