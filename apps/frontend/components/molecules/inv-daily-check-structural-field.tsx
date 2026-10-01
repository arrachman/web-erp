'use client';

/**
 * Renders a single STRUCTURAL header field for the inventory daily-check form.
 * Each structural fieldKey is bound to a real column on InvDailyCheckFormData
 * (and downstream a DB column) — this switch IS the field→column binding.
 * Everything else (label, order, visibility, placeholder, required, readonly)
 * comes from Form Builder config. Atomic tier: Molecule.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import {
  loadBranchOptions,
  loadLocationOptions,
} from '@/components/pages/inv-daily-check-form-lookups';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { InvDailyCheckFormData } from '@/components/pages/inv-daily-check-form-model';

export interface InvDailyCheckStructuralFieldCtx {
  data: InvDailyCheckFormData;
  set: (p: Partial<InvDailyCheckFormData>) => void;
  ph: (key: string, fallback: string) => string;
  ro: (key: string) => boolean;
  locked: boolean;
}

function Picker({
  value, initialLabel, ph, ro, loader, onChange,
}: {
  value: string;
  initialLabel?: string;
  ph: string;
  ro: boolean;
  loader: SearchSelectProps['loadOptions'];
  onChange: (v: string, label?: string) => void;
}) {
  return (
    <SearchSelect
      placeholder={ph}
      value={value}
      initialLabel={initialLabel}
      disabled={ro}
      onValueChange={onChange}
      onPick={(o) => onChange(o.value, o.label)}
      loadOptions={loader}
    />
  );
}

/** Renders the bound control for one structural DC field, wrapped in a label row. */
export function InvDailyCheckStructuralField({
  field,
  ctx,
}: {
  field: ErpFormField;
  ctx: InvDailyCheckStructuralFieldCtx;
}) {
  const { data, set, ph, ro, locked } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>
      {children}
    </FormFieldRow>
  );

  switch (key) {
    case 'description':
      return row(
        <Input
          value={data.description}
          placeholder={field.placeholder || undefined}
          disabled={ro(key)}
          onChange={(e) => set({ description: e.target.value })}
        />,
      );
    case 'machineRef':
      return row(
        <Input
          value={data.machineRef}
          placeholder={ph(key, 'Ref mesin')}
          disabled={ro(key)}
          onChange={(e) => set({ machineRef: e.target.value })}
        />,
      );
    case 'operatorRef':
      return row(
        <Input
          value={data.operatorRef}
          placeholder={ph(key, 'Ref operator')}
          disabled={ro(key)}
          onChange={(e) => set({ operatorRef: e.target.value })}
        />,
      );
    case 'branchId':
      return row(
        <Picker
          value={data.branchId}
          initialLabel={data.branchLabel}
          ph={ph(key, 'Pilih cabang…')}
          ro={ro(key)}
          loader={loadBranchOptions}
          onChange={(v, label) =>
            set({ branchId: v, branchLabel: label ?? data.branchLabel })
          }
        />,
      );
    case 'locationId':
      return row(
        <Picker
          value={data.locationId}
          initialLabel={data.locationLabel}
          ph={ph(key, 'Pilih lokasi…')}
          ro={ro(key)}
          loader={loadLocationOptions}
          onChange={(v, label) =>
            set({ locationId: v, locationLabel: label ?? data.locationLabel })
          }
        />,
      );
    case 'checkDate':
      return row(
        <DateInput
          value={data.checkDate}
          placeholder={field.placeholder || undefined}
          disabled={ro(key)}
          onChange={(v) => set({ checkDate: v })}
        />,
      );
    case 'docNumber':
      return row(
        <div className="flex items-center gap-2">
          <Input
            className="flex-1 min-w-0"
            value={data.auto ? '(otomatis saat simpan)' : data.docNumber}
            placeholder={ph(key, 'No transaksi')}
            disabled={data.auto || ro(key)}
            onChange={(e) => set({ docNumber: e.target.value })}
          />
          <label className="flex items-center gap-1 text-xs text-muted-foreground shrink-0 cursor-pointer">
            <input
              type="checkbox"
              checked={data.auto}
              disabled={locked}
              onChange={(e) => set({ auto: e.target.checked })}
            />
            Auto
          </label>
        </div>,
      );
    default:
      return null;
  }
}
