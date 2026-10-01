'use client';

/**
 * Filter + export toolbar for the generic purchasing report page.
 *
 * Controlled entirely by props (the page owns filter state). Renders: date-from
 * / date-to (DateInput), vendor + item SearchSelect pickers (loaders from the
 * shared lookup-source registry), a status select, a free-text search input, a
 * "Tampilkan/Refresh" button, and three export buttons (Excel / PDF / Word) that
 * call `downloadReport` and surface progress + result via `notify`.
 *
 * Atomic tier: Organism.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { buildLookupLoader } from '@/lib/lookup-source-registry';
import { notify } from '@/lib/feedback';
import { tGlobal } from '@/lib/mock';
import {
  downloadReport,
  type ReportExportFormat,
  type ReportFilters,
} from '@/lib/api/pur-reports';

export interface StatusOption {
  label: string;
  value: string;
}

interface PurReportToolbarProps {
  reportKey: string;
  filters: ReportFilters;
  onChange: (patch: Partial<ReportFilters>) => void;
  /** Re-run the query with the current filters. */
  onRefresh: () => void;
  /** Show the as-of date field instead of the date range (balance-style reports). */
  asOfMode?: boolean;
  /** Show the vendor picker. */
  showVendor?: boolean;
  /** Show the item picker (optional — some reports are vendor-only). */
  showItem?: boolean;
  /** Status dropdown options. When omitted, the status filter is hidden. */
  statusOptions?: StatusOption[];
  /** Disable inputs while the page is loading. */
  busy?: boolean;
}

const ALL_VAL = '_all';

const VENDOR_LOADER = buildLookupLoader('partners');
const ITEM_LOADER = buildLookupLoader('items');

const EXPORTS: { format: ReportExportFormat; label: string; icon: 'download' }[] = [
  { format: 'xlsx', label: 'Excel', icon: 'download' },
  { format: 'pdf', label: 'PDF', icon: 'download' },
  { format: 'docx', label: 'Word', icon: 'download' },
];

export function PurReportToolbar({
  reportKey,
  filters,
  onChange,
  onRefresh,
  asOfMode,
  showVendor,
  showItem,
  statusOptions,
  busy,
}: PurReportToolbarProps) {
  const [exporting, setExporting] = React.useState<ReportExportFormat | null>(null);

  const handleExport = React.useCallback(
    async (format: ReportExportFormat) => {
      if (exporting) return;
      setExporting(format);
      try {
        await downloadReport(reportKey, format, filters);
        notify(tGlobal('Berkas berhasil diunduh'), 'success');
      } catch (err) {
        const msg = err instanceof Error ? err.message : tGlobal('Unduhan gagal');
        notify(msg, 'danger');
      } finally {
        setExporting(null);
      }
    },
    [exporting, reportKey, filters],
  );

  return (
    <div className="flex flex-wrap items-center gap-2">
      {asOfMode ? (
        <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
          {tGlobal('Per tanggal')}
          <DateInput
            value={filters.asOfDate ?? ''}
            onChange={(iso) => onChange({ asOfDate: iso || undefined })}
            disabled={busy}
          />
        </label>
      ) : (
        <>
          <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
            {tGlobal('Dari')}
            <DateInput
              value={filters.dateFrom ?? ''}
              onChange={(iso) => onChange({ dateFrom: iso || undefined })}
              disabled={busy}
            />
          </label>
          <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
            {tGlobal('Sampai')}
            <DateInput
              value={filters.dateTo ?? ''}
              onChange={(iso) => onChange({ dateTo: iso || undefined })}
              disabled={busy}
            />
          </label>
        </>
      )}

      {showVendor && VENDOR_LOADER && (
        <div className="min-w-[12rem]">
          <SearchSelect
            placeholder={tGlobal('Semua vendor')}
            title={tGlobal('Pilih Vendor')}
            value={filters.vendorId ?? ''}
            onValueChange={(v) => onChange({ vendorId: v || undefined })}
            loadOptions={VENDOR_LOADER}
            disabled={busy}
          />
        </div>
      )}

      {showItem && ITEM_LOADER && (
        <div className="min-w-[12rem]">
          <SearchSelect
            placeholder={tGlobal('Semua barang')}
            title={tGlobal('Pilih Barang')}
            value={filters.itemId ?? ''}
            onValueChange={(v) => onChange({ itemId: v || undefined })}
            loadOptions={ITEM_LOADER}
            disabled={busy}
          />
        </div>
      )}

      {statusOptions && statusOptions.length > 0 && (
        <Select
          value={filters.status || ALL_VAL}
          onValueChange={(v) => onChange({ status: v === ALL_VAL ? undefined : v })}
        >
          <SelectTrigger style={{ width: 'auto', minWidth: '8rem' }}>
            <span
              style={{
                color: 'var(--fg-faint)',
                fontSize: 'calc(11px * var(--font-scale, 1))',
                marginRight: 2,
              }}
            >
              {tGlobal('Status')}:
            </span>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VAL}>{tGlobal('Semua')}</SelectItem>
            {statusOptions.map((o) => (
              <SelectItem key={o.value} value={o.value}>
                {tGlobal(o.label)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      )}

      <div className="search-input">
        <Icon name="search" size={12} />
        <Input
          className="border-0 bg-transparent px-0 shadow-none focus-visible:ring-0"
          placeholder={tGlobal('Cari...')}
          value={filters.search ?? ''}
          onChange={(e) => onChange({ search: e.target.value || undefined })}
          disabled={busy}
        />
      </div>

      <Button
        variant="primary"
        className="cursor-pointer"
        onClick={onRefresh}
        disabled={busy}
      >
        <Icon name="refresh" size={12} />
        {tGlobal('Tampilkan')}
      </Button>

      <div className="ml-auto flex items-center gap-1.5">
        {EXPORTS.map((e) => (
          <Button
            key={e.format}
            variant="default"
            className="cursor-pointer"
            onClick={() => handleExport(e.format)}
            disabled={exporting !== null}
            title={tGlobal(`Ekspor ${e.label}`)}
          >
            <Icon name={exporting === e.format ? 'refresh' : e.icon} size={12} />
            {tGlobal(e.label)}
          </Button>
        ))}
      </div>
    </div>
  );
}
