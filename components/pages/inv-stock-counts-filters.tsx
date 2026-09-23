'use client';

/**
 * Inventory stock-count list — slim filter bar (§2.40): inline Status + Tanggal
 * range that apply live, plus a Reset when active.
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

export interface InvCountFilters {
  status: string;
  dateFrom: string;
  dateTo: string;
}

export const emptyInvCountFilters: InvCountFilters = { status: '', dateFrom: '', dateTo: '' };

const STATUS_OPTIONS = [
  { value: '', label: 'Semua status' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'NEED_APPROVE', label: 'Need Approve' },
  { value: 'APPROVED', label: 'Approved' },
  { value: 'REJECTED', label: 'Rejected' },
  { value: 'POSTED', label: 'Posted' },
];

export function hasActiveInvCountFilters(f: InvCountFilters): boolean {
  return !!(f.status || f.dateFrom || f.dateTo);
}

export function InvCountFilters({
  value,
  onChange,
}: {
  value: InvCountFilters;
  onChange: (f: InvCountFilters) => void;
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

      {hasActiveInvCountFilters(value) && (
        <button type="button" className="btn ghost sm" onClick={() => onChange(emptyInvCountFilters)} title="Reset semua filter">
          <Icon name="x" size={11} /> Reset
        </button>
      )}
    </>
  );
}
