'use client';

/**
 * Customer Advance (AS) list — slim filter bar (§2.40): inline Status + Tanggal;
 * advanced filters (settlementStatus etc.) in a right-side drawer.
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

export interface SlsAsFilters {
  status: string;
  dateFrom: string;
  dateTo: string;
  docNumber: string;
  customerName: string;
  description: string;
  settlementStatus: string;
}

export const emptySlsAsFilters: SlsAsFilters = {
  status: '',
  dateFrom: '',
  dateTo: '',
  docNumber: '',
  customerName: '',
  description: '',
  settlementStatus: '',
};

const STATUS_OPTIONS = [
  { value: '', label: 'Semua status' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'NEED_APPROVE', label: 'Need Approve' },
  { value: 'APPROVED', label: 'Approved' },
  { value: 'REJECTED', label: 'Rejected' },
  { value: 'POSTED', label: 'Posted' },
];

const SETTLEMENT_OPTIONS = [
  { value: '', label: 'Semua' },
  { value: 'UNPAID', label: 'Belum Dipakai' },
  { value: 'PARTIAL', label: 'Sebagian' },
  { value: 'PAID', label: 'Lunas' },
];

export function hasActiveSlsAsFilters(f: SlsAsFilters): boolean {
  return !!(
    f.status ||
    f.dateFrom ||
    f.dateTo ||
    f.docNumber ||
    f.customerName ||
    f.description ||
    f.settlementStatus
  );
}

export function advancedSlsAsCount(f: SlsAsFilters): number {
  return [f.docNumber, f.customerName, f.description, f.settlementStatus].filter(Boolean).length;
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}

export function SlsCustomerAdvanceFilters({
  value,
  onChange,
}: {
  value: SlsAsFilters;
  onChange: (f: SlsAsFilters) => void;
}) {
  const [open, setOpen] = React.useState(false);
  const [draft, setDraft] = React.useState<SlsAsFilters>(value);

  const openDrawer = () => {
    setDraft(value);
    setOpen(true);
  };
  const apply = () => {
    onChange(draft);
    setOpen(false);
  };

  const advCount = advancedSlsAsCount(value);
  const anyActive = hasActiveSlsAsFilters(value);
  const set = (p: Partial<SlsAsFilters>) => setDraft((d) => ({ ...d, ...p }));

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
        <button
          type="button"
          className="btn ghost sm"
          onClick={() => onChange(emptySlsAsFilters)}
          title="Reset semua filter"
        >
          <Icon name="x" size={11} /> Reset
        </button>
      )}

      <Drawer open={open} onOpenChange={setOpen}>
        <DrawerContent>
          <DrawerHeader>
            <Icon name="filter" size={14} />
            <DrawerTitle>Filter Customer Advance</DrawerTitle>
          </DrawerHeader>
          <DrawerBody>
            <div className="flex flex-col gap-3.5">
              <Field label="No AS">
                <Input
                  value={draft.docNumber}
                  onChange={(e) => set({ docNumber: e.target.value })}
                  placeholder="Cari nomor AS…"
                />
              </Field>
              <Field label="Pelanggan">
                <Input
                  value={draft.customerName}
                  onChange={(e) => set({ customerName: e.target.value })}
                  placeholder="Nama pelanggan…"
                />
              </Field>
              <Field label="Keterangan">
                <Input
                  value={draft.description}
                  onChange={(e) => set({ description: e.target.value })}
                  placeholder="Cari keterangan…"
                />
              </Field>
              <Field label="Status Lunas">
                <Select
                  value={draft.settlementStatus || '_all'}
                  onValueChange={(v) => set({ settlementStatus: v === '_all' ? '' : v })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Semua" />
                  </SelectTrigger>
                  <SelectContent>
                    {SETTLEMENT_OPTIONS.map((o) => (
                      <SelectItem key={o.value || '_all'} value={o.value || '_all'}>
                        {o.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </Field>
            </div>
          </DrawerBody>
          <DrawerFooter>
            <button
              type="button"
              className="btn ghost sm"
              onClick={() => setDraft(emptySlsAsFilters)}
            >
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
