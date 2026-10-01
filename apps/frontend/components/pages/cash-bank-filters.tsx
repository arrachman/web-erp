'use client';

/**
 * Cash/bank transaction — shared slim filter bar (§2.40), reused by Kas Masuk
 * (CR) / Kas Keluar (CD) / Bank Keluar (BD). Inline quick filters (Status,
 * Tanggal) apply live; everything else lives in a right-side drawer (staged
 * draft → "Terapkan"). Caller passes `entityName` (drawer title) + `partnerLabel`.
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
import {
  Drawer,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerBody,
  DrawerFooter,
} from '@/components/organisms/drawer';
import {
  CashBankFilterFields,
  STATUS_OPTIONS,
  emptyCashBankFilters,
  hasActiveCashBankFilters,
  advancedCashBankCount,
  type CashBankFilters,
} from './cash-bank-filter-fields';

export function CashBankFiltersBar({
  value,
  onChange,
  entityName,
  partnerLabel,
}: {
  value: CashBankFilters;
  onChange: (f: CashBankFilters) => void;
  entityName: string;
  partnerLabel: string;
}) {
  const [open, setOpen] = React.useState(false);
  const [draft, setDraft] = React.useState<CashBankFilters>(value);

  const openDrawer = () => {
    setDraft(value);
    setOpen(true);
  };
  const apply = () => {
    onChange(draft);
    setOpen(false);
  };

  const advCount = advancedCashBankCount(value);
  const anyActive = hasActiveCashBankFilters(value);

  return (
    <>
      <label className="flex items-center gap-1.5">
        <span className="text-xs text-muted-foreground whitespace-nowrap">Status</span>
        <Select value={value.status || '_all'} onValueChange={(v) => onChange({ ...value, status: v === '_all' ? '' : v })}>
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

      <button type="button" className="btn ghost sm" onClick={openDrawer} title="Filter lanjutan">
        <Icon name="filter" size={12} /> Filter
        {advCount > 0 && (
          <span className="ml-1 inline-flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-primary-foreground">
            {advCount}
          </span>
        )}
      </button>

      {anyActive && (
        <button type="button" className="btn ghost sm" onClick={() => onChange(emptyCashBankFilters)} title="Reset semua filter">
          <Icon name="x" size={11} /> Reset
        </button>
      )}

      <Drawer open={open} onOpenChange={setOpen}>
        <DrawerContent>
          <DrawerHeader>
            <Icon name="filter" size={14} />
            <DrawerTitle>Filter {entityName}</DrawerTitle>
          </DrawerHeader>
          <DrawerBody>
            <CashBankFilterFields value={draft} onChange={setDraft} partnerLabel={partnerLabel} />
          </DrawerBody>
          <DrawerFooter>
            <button type="button" className="btn ghost sm" onClick={() => setDraft(emptyCashBankFilters)}>
              Atur ulang
            </button>
            <button type="button" className="btn primary sm" onClick={apply}>
              Terapkan
            </button>
          </DrawerFooter>
        </DrawerContent>
      </Drawer>
    </>
  );
}
