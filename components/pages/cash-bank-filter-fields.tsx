'use client';

/**
 * Cash/bank transaction — shared advanced-filter form (drawer body), reused by
 * Kas Masuk (CR) / Kas Keluar (CD) / Bank Keluar (BD). Only the party field
 * label ("Terima Dari" vs "Bayar Ke") differs, passed via `partnerLabel`.
 * Edits a staged draft; the parent commits on "Terapkan". Atomic tier: organism.
 */

import * as React from 'react';
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
  SearchSelect,
  type SearchSelectOption,
} from '@/components/molecules/search-select';
import {
  loadPartnerOptions,
  loadLocationOptions,
  loadBranchOptions,
} from './items-form-lookups';
import { listUsers } from '@/lib/api/users';

export interface CashBankFilters {
  noFrom: string;
  noTo: string;
  status: string;
  dateFrom: string;
  dateTo: string;
  partnerId: string;
  partnerLabel?: string;
  locationId: string;
  locationLabel?: string;
  branchId: string;
  branchLabel?: string;
  uraian: string;
  catatan: string;
  userId: string;
  userLabel?: string;
}

export const emptyCashBankFilters: CashBankFilters = {
  noFrom: '',
  noTo: '',
  status: '',
  dateFrom: '',
  dateTo: '',
  partnerId: '',
  locationId: '',
  branchId: '',
  uraian: '',
  catatan: '',
  userId: '',
};

export const hasActiveCashBankFilters = (f: CashBankFilters): boolean =>
  !!(
    f.noFrom || f.noTo || f.status || f.dateFrom || f.dateTo ||
    f.partnerId || f.locationId || f.branchId || f.uraian || f.catatan || f.userId
  );

/** Count of active *advanced* filters (Status & Tanggal stay inline → excluded). */
export const advancedCashBankCount = (f: CashBankFilters): number =>
  [f.noFrom || f.noTo, f.partnerId, f.locationId, f.branchId, f.uraian, f.catatan, f.userId]
    .filter(Boolean).length;

export const STATUS_OPTIONS = [
  { label: 'Semua', value: '' },
  { label: 'Draft', value: 'DRAFT' },
  { label: 'Need Approve', value: 'NEED_APPROVE' },
  { label: 'Approved', value: 'APPROVED' },
  { label: 'Rejected', value: 'REJECTED' },
  { label: 'Posted', value: 'POSTED' },
];

type Loader = (
  search: string,
  page: number,
  limit: number,
) => Promise<{ data: SearchSelectOption[]; total: number }>;

const loadUserOptions: Loader = async (search, page, limit) => {
  const res = await listUsers({ search: search || undefined, page, limit });
  return {
    data: res.data.map((u) => ({ value: u.id, label: u.fullName, code: u.username })),
    total: res.meta.total,
  };
};

/** Value→label cache, populated as options load — used for chip display. */
const LABEL_CACHE = new Map<string, string>();
const cachedLabel = (v: string) => (v ? LABEL_CACHE.get(v) : undefined);

/** Wrap a loader so every option's label is cached by value (for chip display). */
function withLabelCache(loader: Loader): Loader {
  return async (search, page, limit) => {
    const res = await loader(search, page, limit);
    res.data.forEach((o) => LABEL_CACHE.set(o.value, o.label));
    return res;
  };
}

const LOADERS = {
  partner: withLabelCache(loadPartnerOptions),
  location: withLabelCache(loadLocationOptions),
  branch: withLabelCache(loadBranchOptions),
  user: withLabelCache(loadUserOptions),
};

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="flex flex-col gap-1">
      <span className="text-xs text-muted-foreground">{label}</span>
      {children}
    </label>
  );
}

export function CashBankFilterFields({
  value,
  onChange,
  partnerLabel,
}: {
  value: CashBankFilters;
  onChange: (f: CashBankFilters) => void;
  partnerLabel: string;
}) {
  const set = (p: Partial<CashBankFilters>) => onChange({ ...value, ...p });
  const lbl = cachedLabel;

  return (
    <div className="flex flex-col gap-3.5">
      <Field label="No Transaksi">
        <div className="flex items-center gap-1">
          <Input value={value.noFrom} onChange={(e) => set({ noFrom: e.target.value })} placeholder="dari" />
          <span className="text-xs text-muted-foreground">s.d</span>
          <Input value={value.noTo} onChange={(e) => set({ noTo: e.target.value })} placeholder="s.d" />
        </div>
      </Field>
      <Field label="Status">
        <Select value={value.status || '_all'} onValueChange={(v) => set({ status: v === '_all' ? '' : v })}>
          <SelectTrigger>
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {STATUS_OPTIONS.map((o) => (
              <SelectItem key={o.value || '_all'} value={o.value || '_all'}>
                {o.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </Field>
      <Field label="Tanggal">
        <DateRangePicker
          from={value.dateFrom}
          to={value.dateTo}
          onChangeFrom={(v) => set({ dateFrom: v })}
          onChangeTo={(v) => set({ dateTo: v })}
        />
      </Field>
      <Field label={partnerLabel}>
        <SearchSelect
          placeholder="Semua partner"
          value={value.partnerId}
          initialLabel={value.partnerLabel}
          onValueChange={(v) => set({ partnerId: v, partnerLabel: lbl(v) })}
          loadOptions={LOADERS.partner}
        />
      </Field>
      <Field label="Lokasi">
        <SearchSelect
          placeholder="Semua lokasi"
          value={value.locationId}
          initialLabel={value.locationLabel}
          onValueChange={(v) => set({ locationId: v, locationLabel: lbl(v) })}
          loadOptions={LOADERS.location}
        />
      </Field>
      <Field label="Cabang">
        <SearchSelect
          placeholder="Semua cabang"
          value={value.branchId}
          initialLabel={value.branchLabel}
          onValueChange={(v) => set({ branchId: v, branchLabel: lbl(v) })}
          loadOptions={LOADERS.branch}
        />
      </Field>
      <Field label="Uraian">
        <Input value={value.uraian} onChange={(e) => set({ uraian: e.target.value })} placeholder="Cari uraian…" />
      </Field>
      <Field label="Catatan">
        <Input value={value.catatan} onChange={(e) => set({ catatan: e.target.value })} placeholder="Cari catatan…" />
      </Field>
      <Field label="User">
        <SearchSelect
          placeholder="Semua user"
          value={value.userId}
          initialLabel={value.userLabel}
          onValueChange={(v) => set({ userId: v, userLabel: lbl(v) })}
          loadOptions={LOADERS.user}
        />
      </Field>
    </div>
  );
}
