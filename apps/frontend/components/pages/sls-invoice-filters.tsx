'use client';

/**
 * Sales Invoice list — slim filter bar (§2.40): inline Status + Tanggal range
 * apply live; advanced filters (No SI, Pelanggan, Uraian, settlementStatus) in
 * a right-side drawer (staged draft → "Terapkan").
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
import type { SlsSettlementStatus } from '@/lib/api/sls-invoices';

export interface SlsSiFilters {
  status: string;
  dateFrom: string;
  dateTo: string;
  /** No dokumen — text search (from/to range or single prefix). */
  docNumber: string;
  /** Customer name free-text. */
  customerName: string;
  /** Description / uraian free-text. */
  uraian: string;
  /** Settlement / pelunasan status. */
  settlementStatus: SlsSettlementStatus | '';
}

export const emptySlsSiFilters: SlsSiFilters = {
  status: '',
  dateFrom: '',
  dateTo: '',
  docNumber: '',
  customerName: '',
  uraian: '',
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
  { value: '', label: 'Semua pelunasan' },
  { value: 'UNPAID', label: 'Belum Bayar' },
  { value: 'PARTIAL', label: 'Sebagian' },
  { value: 'PAID', label: 'Lunas' },
];

export function hasActiveSlsSiFilters(f: SlsSiFilters): boolean {
  return !!(
    f.status ||
    f.dateFrom ||
    f.dateTo ||
    f.docNumber ||
    f.customerName ||
    f.uraian ||
    f.settlementStatus
  );
}

/** Count of active *advanced* (drawer-only) filters. */
export function advancedSlsSiCount(f: SlsSiFilters): number {
  return [f.docNumber, f.customerName, f.uraian, f.settlementStatus].filter(Boolean).length;
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}

export function SlsInvoiceFilters({
  value,
  onChange,
}: {
  value: SlsSiFilters;
  onChange: (f: SlsSiFilters) => void;
}) {
  const [open, setOpen] = React.useState(false);
  const [draft, setDraft] = React.useState<SlsSiFilters>(value);

  const openDrawer = () => {
    setDraft(value);
    setOpen(true);
  };
  const apply = () => {
    onChange(draft);
    setOpen(false);
  };

  const advCount = advancedSlsSiCount(value);
  const anyActive = hasActiveSlsSiFilters(value);
  const set = (p: Partial<SlsSiFilters>) => setDraft((d) => ({ ...d, ...p }));

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
          onClick={() => onChange(emptySlsSiFilters)}
          title="Reset semua filter"
        >
          <Icon name="x" size={11} /> Reset
        </button>
      )}

      <Drawer open={open} onOpenChange={setOpen}>
        <DrawerContent>
          <DrawerHeader>
            <Icon name="filter" size={14} />
            <DrawerTitle>Filter Sales Invoice</DrawerTitle>
          </DrawerHeader>
          <DrawerBody>
            <div className="flex flex-col gap-3.5">
              <Field label="No SI">
                <Input
                  value={draft.docNumber}
                  onChange={(e) => set({ docNumber: e.target.value })}
                  placeholder="Cari nomor SI…"
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
              <Field label="Pelunasan">
                <Select
                  value={draft.settlementStatus || '_all'}
                  onValueChange={(v) =>
                    set({ settlementStatus: v === '_all' ? '' : (v as SlsSettlementStatus) })
                  }
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Semua pelunasan" />
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
            <button type="button" className="btn ghost sm" onClick={() => setDraft(emptySlsSiFilters)}>
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
