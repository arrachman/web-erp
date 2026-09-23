'use client';

/**
 * Renders a single STRUCTURAL header field for the inventory stock-count form.
 * Each structural fieldKey is bound to a real column on InvStockCountFormData
 * (and, downstream, a DB column), so the input component per key is fixed here —
 * this switch IS the field→column binding. Everything else (label, order,
 * visibility, placeholder, required, readonly) comes from Form Builder config
 * (§ Header form transaksi render dinamis). No currency on inventory counts.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { buildLookupLoader, BUILTIN_SOURCE } from '@/lib/lookup-source-registry';
import { loadBranchOptions } from '@/components/pages/inv-stock-count-form-lookups';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { InvStockCountFormData } from '@/components/pages/inv-stock-count-form-model';

export interface InvCountStructuralFieldCtx {
  data: InvStockCountFormData;
  set: (p: Partial<InvStockCountFormData>) => void;
  /** Placeholder resolver: config placeholder or the given fallback. */
  ph: (key: string, fallback: string) => string;
  /** Read-only resolver: locked document or config isReadonly. */
  ro: (key: string) => boolean;
  /** Whole document locked (non-editable status). */
  locked: boolean;
}

/** Loader for a custom LOOKUP field, resolved from its configured source slug. */
const warehouseLoader = buildLookupLoader('warehouses');

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

/** Resolve the loader for a LOOKUP field from its config source (default: warehouses). */
function lookupLoaderFor(field: ErpFormField): SearchSelectProps['loadOptions'] {
  const source = field.lookupSource || BUILTIN_SOURCE[field.fieldType];
  return (
    buildLookupLoader(source, field.lookupDefaultFilter, field.lookupDefaultSort)
      ?? warehouseLoader
  ) as SearchSelectProps['loadOptions'];
}

/** Renders the bound control for one structural field, wrapped in a label row. */
export function InvCountStructuralField({ field, ctx }: { field: ErpFormField; ctx: InvCountStructuralFieldCtx }) {
  const { data, set, ph, ro, locked } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>{children}</FormFieldRow>
  );

  switch (key) {
    case 'branchId':
      return row(
        <Picker value={data.branchId} initialLabel={data.branchLabel}
          ph={ph(key, 'Pilih cabang…')} ro={ro(key)} loader={loadBranchOptions}
          onChange={(v, label) => set({ branchId: v, branchLabel: label ?? data.branchLabel })} />,
      );
    case 'warehouseId':
      return row(
        <Picker value={data.warehouseId} initialLabel={data.warehouseLabel}
          ph={ph(key, 'Pilih gudang…')} ro={ro(key)} loader={lookupLoaderFor(field)}
          onChange={(v, label) => set({ warehouseId: v, warehouseLabel: label ?? data.warehouseLabel })} />,
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
    case 'countDate':
      return row(
        <DateInput value={data.countDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ countDate: v })} />,
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
    default:
      return null;
  }
}
