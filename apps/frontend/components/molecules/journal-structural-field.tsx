'use client';

/**
 * Renders a single STRUCTURAL header field for the journal transaction form.
 * Each structural fieldKey is bound to a real column on JournalFormData (and a DB
 * column + GL posting), so the input per key is fixed here — this switch IS the
 * field→column binding. Label/order/slot/visibility/placeholder/required/readonly
 * come from Form Builder config.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { formatNumber } from '@/lib/format';
import {
  loadPartnerOptions,
  loadBranchOptions,
  loadLocationOptions,
  loadCurrencyOptions,
} from '@/components/pages/items-form-lookups';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { JournalFormData } from '@/components/pages/journal-form-model';

export interface JournalStructuralFieldCtx {
  data: JournalFormData;
  set: (p: Partial<JournalFormData>) => void;
  ph: (key: string, fallback: string) => string;
  ro: (key: string) => boolean;
  locked: boolean;
  currencyLabel?: string;
}

function Picker({
  value, initialLabel, ph, ro, loader, onChange,
}: {
  value: string;
  initialLabel?: string;
  ph: string;
  ro: boolean;
  loader: SearchSelectProps['loadOptions'];
  onChange: (v: string) => void;
}) {
  return (
    <SearchSelect
      placeholder={ph}
      value={value}
      initialLabel={initialLabel}
      disabled={ro}
      onValueChange={onChange}
      loadOptions={loader}
    />
  );
}

export function JournalStructuralField({
  field,
  ctx,
}: {
  field: ErpFormField;
  ctx: JournalStructuralFieldCtx;
}) {
  const { data, set, ph, ro, locked } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>{children}</FormFieldRow>
  );

  switch (key) {
    case 'partnerId':
      return row(
        <Picker value={data.partnerId} initialLabel={data.partnerLabel}
          ph={ph(key, 'Pilih partner…')} ro={ro(key)} loader={loadPartnerOptions}
          onChange={(v) => set({ partnerId: v })} />,
      );
    case 'branchId':
      return row(
        <Picker value={data.branchId} initialLabel={data.branchLabel}
          ph={ph(key, 'Pilih cabang…')} ro={ro(key)} loader={loadBranchOptions}
          onChange={(v) => set({ branchId: v })} />,
      );
    case 'locationId':
      return row(
        <Picker value={data.locationId} initialLabel={data.locationLabel}
          ph={ph(key, 'Pilih lokasi…')} ro={ro(key)} loader={loadLocationOptions}
          onChange={(v) => set({ locationId: v })} />,
      );
    case 'description':
      return row(
        <Input value={data.description} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(e) => set({ description: e.target.value })} />,
      );
    case 'notes':
      return row(
        <Input value={data.notes} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(e) => set({ notes: e.target.value })} />,
      );
    case 'entryDate':
      return row(
        <DateInput value={data.entryDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ entryDate: v })} />,
      );
    case 'docNumber':
      return row(
        <div className="flex items-center gap-2">
          <Input className="flex-1 min-w-0"
            value={data.auto ? '(otomatis saat simpan)' : data.docNumber}
            placeholder={ph(key, 'No transaksi')} disabled={data.auto || ro(key)}
            onChange={(e) => set({ docNumber: e.target.value })} />
          <label className="flex items-center gap-1 text-xs text-muted-foreground shrink-0 cursor-pointer">
            <input type="checkbox" checked={data.auto} disabled={locked}
              onChange={(e) => set({ auto: e.target.checked })} />
            Auto
          </label>
        </div>,
      );
    case 'currencyId':
      return row(
        <div className="flex items-center gap-2">
          <div className="flex-1 min-w-0">
            <Picker value={data.currencyId} initialLabel={ctx.currencyLabel}
              ph={ph(key, 'Mata uang')} ro={ro(key)} loader={loadCurrencyOptions}
              onChange={(v) => set({ currencyId: v })} />
          </div>
          <span className="flex items-center gap-1 text-xs text-muted-foreground shrink-0 whitespace-nowrap"
            title="Kurs ke mata uang dasar (read-only)">
            Kurs
            <span className="tabular-nums font-medium text-foreground">
              {formatNumber(Number(data.exchangeRate) || 1, 2)}
            </span>
          </span>
        </div>,
      );
    default:
      return null;
  }
}
