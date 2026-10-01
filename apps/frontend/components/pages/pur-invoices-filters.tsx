'use client';

/**
 * Purchase Invoice list — slim filter bar (§2.40): inline Status + Tanggal range
 * that apply live, plus a Reset when active. Mirrors the PO filter bar.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { DateRangePicker } from '@/components/ui/date-range-picker';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

export interface PurInvoiceFilters {
  status: string;
  dateFrom: string;
  dateTo: string;
}

export const emptyPurInvoiceFilters: PurInvoiceFilters = { status: '', dateFrom: '', dateTo: '' };

const STATUS_OPTIONS = [
  { value: '', label: 'Semua status' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'NEED_APPROVE', label: 'Need Approve' },
  { value: 'APPROVED', label: 'Approved' },
  { value: 'REJECTED', label: 'Rejected' },
  { value: 'POSTED', label: 'Posted' },
];

export function hasActivePurInvoiceFilters(f: PurInvoiceFilters): boolean {
  return !!(f.status || f.dateFrom || f.dateTo);
}

export function PurInvoiceFiltersBar({
  value,
  onChange,
}: {
  value: PurInvoiceFilters;
  onChange: (f: PurInvoiceFilters) => void;
}) {
  return (
    <>
      <label className="flex items-center gap-1.5">
        <span className="text-xs text-muted-foreground whitespace-nowrap">Status</span>
        <Select
          value={value.status || '_all'}
          onValueChange={(v) => onChange({ ...value, status: v === '_all' ? '' : v })}
        >
          <SelectTrigger style={{ width: 'auto', minWidth: '7rem' }}>
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            {STATUS_OPTIONS.map((o) => (
              <SelectItem key={o.value || '_all'} value={o.value || '_all'}>
                {o.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </label>

      <label className="flex items-center gap-1.5">
        <span className="text-xs text-muted-foreground whitespace-nowrap">Tanggal</span>
        <DateRangePicker
          fullWidth={false}
          from={value.dateFrom}
          to={value.dateTo}
          onChangeFrom={(v) => onChange({ ...value, dateFrom: v })}
          onChangeTo={(v) => onChange({ ...value, dateTo: v })}
        />
      </label>

      {hasActivePurInvoiceFilters(value) && (
        <button type="button" className="btn ghost sm" onClick={() => onChange(emptyPurInvoiceFilters)} title="Reset semua filter">
          <Icon name="x" size={11} /> Reset
        </button>
      )}
    </>
  );
}
