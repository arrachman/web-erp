'use client';

/**
 * Sales Order list — slim filter bar (§2.40): inline Status + Tanggal range that
 * apply live; advanced filters (No SO, Pelanggan, Uraian) in a right-side drawer
 * (staged draft → "Terapkan"). Reset appears when any filter is active.
 */

import * as React from 'react';
import { Icon } from '@/components/ui/icons';
import { Input } from '@/components/ui/input';
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

export interface SlsFilters {
  status: string;
  dateFrom: string;
  dateTo: string;
  /** No dokumen — text search (from/to range or single prefix). */
  docNumber: string;
  /** Customer name free-text (maps to API `description` / server-side search). */
  customerName: string;
  /** Description / uraian free-text. */
  uraian: string;
}

export const emptySlsFilters: SlsFilters = {
  status: '',
  dateFrom: '',
  dateTo: '',
  docNumber: '',
  customerName: '',
  uraian: '',
};

const STATUS_OPTIONS = [
  { value: '', label: 'Semua status' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'NEED_APPROVE', label: 'Need Approve' },
  { value: 'APPROVED', label: 'Approved' },
  { value: 'REJECTED', label: 'Rejected' },
  { value: 'POSTED', label: 'Posted' },
];

export function hasActiveSlsFilters(f: SlsFilters): boolean {
  return !!(f.status || f.dateFrom || f.dateTo || f.docNumber || f.customerName || f.uraian);
}

/** Count of active *advanced* (drawer-only) filters. */
export function advancedSlsCount(f: SlsFilters): number {
  return [f.docNumber, f.customerName, f.uraian].filter(Boolean).length;
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}

export function SlsOrderFilters({
  value,
  onChange,
}: {
  value: SlsFilters;
  onChange: (f: SlsFilters) => void;
}) {
  const [open, setOpen] = React.useState(false);
  const [draft, setDraft] = React.useState<SlsFilters>(value);

  const openDrawer = () => {
    setDraft(value);
    setOpen(true);
  };
  const apply = () => {
    onChange(draft);
    setOpen(false);
  };

  const advCount = advancedSlsCount(value);
  const anyActive = hasActiveSlsFilters(value);
  const set = (p: Partial<SlsFilters>) => setDraft((d) => ({ ...d, ...p }));

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

      <button type="button" className="btn ghost sm" onClick={openDrawer} title="Filter lanjutan">
        <Icon name="filter" size={12} /> Filter
        {advCount > 0 && (
          <span className="ml-1 inline-flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-semibold text-primary-foreground">
            {advCount}
          </span>
        )}
      </button>

      {anyActive && (
        <button type="button" className="btn ghost sm" onClick={() => onChange(emptySlsFilters)} title="Reset semua filter">
          <Icon name="x" size={11} /> Reset
        </button>
      )}

      <Drawer open={open} onOpenChange={setOpen}>
        <DrawerContent>
          <DrawerHeader>
            <Icon name="filter" size={14} />
            <DrawerTitle>Filter Sales Order</DrawerTitle>
          </DrawerHeader>
          <DrawerBody>
            <div className="flex flex-col gap-3.5">
              <Field label="No SO">
                <Input
                  value={draft.docNumber}
                  onChange={(e) => set({ docNumber: e.target.value })}
                  placeholder="Cari nomor SO…"
                />
              </Field>
              <Field label="Pelanggan">
                <Input
                  value={draft.customerName}
                  onChange={(e) => set({ customerName: e.target.value })}
                  placeholder="Nama pelanggan…"
                />
              </Field>
              <Field label="Uraian">
                <Input
                  value={draft.uraian}
                  onChange={(e) => set({ uraian: e.target.value })}
                  placeholder="Cari uraian…"
                />
              </Field>
            </div>
          </DrawerBody>
          <DrawerFooter>
            <button type="button" className="btn ghost sm" onClick={() => setDraft(emptySlsFilters)}>
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
