'use client';

/**
 * Renders a single STRUCTURAL header field for the inventory stock-movement form.
 * Each structural fieldKey is bound to a real column on InvStockMovementFormData
 * (and, downstream, a DB column), so the input component per key is fixed here —
 * this switch IS the field→column binding. Everything else (label, order,
 * visibility, placeholder, required, readonly) comes from Form Builder config
 * (§ Header form transaksi render dinamis). No currency on inventory movements.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { buildLookupLoader, BUILTIN_SOURCE } from '@/lib/lookup-source-registry';
import {
  loadBranchOptions,
  loadLocationOptions,
  loadRequestedPartnerOptions,
} from '@/components/pages/inv-stock-movement-form-lookups';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { InvStockMovementFormData } from '@/components/pages/inv-stock-movement-form-model';

export interface InvStructuralFieldCtx {
  data: InvStockMovementFormData;
  set: (p: Partial<InvStockMovementFormData>) => void;
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
export function InvStructuralField({ field, ctx }: { field: ErpFormField; ctx: InvStructuralFieldCtx }) {
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
    case 'locationId':
      return row(
        <Picker value={data.locationId} initialLabel={data.locationLabel}
          ph={ph(key, 'Pilih lokasi…')} ro={ro(key)} loader={loadLocationOptions}
          onChange={(v, label) => set({ locationId: v, locationLabel: label ?? data.locationLabel })} />,
      );
    case 'sourceWarehouseId':
      return row(
        <Picker value={data.sourceWarehouseId} initialLabel={data.sourceWarehouseLabel}
          ph={ph(key, 'Pilih gudang asal…')} ro={ro(key)} loader={lookupLoaderFor(field)}
          onChange={(v, label) => set({ sourceWarehouseId: v, sourceWarehouseLabel: label ?? data.sourceWarehouseLabel })} />,
      );
    case 'transitWarehouseId':
      return row(
        <Picker value={data.transitWarehouseId} initialLabel={data.transitWarehouseLabel}
          ph={ph(key, 'Pilih gudang transit…')} ro={ro(key)} loader={lookupLoaderFor(field)}
          onChange={(v, label) => set({ transitWarehouseId: v, transitWarehouseLabel: label ?? data.transitWarehouseLabel })} />,
      );
    case 'destinationWarehouseId':
      return row(
        <Picker value={data.destinationWarehouseId} initialLabel={data.destinationWarehouseLabel}
          ph={ph(key, 'Pilih gudang tujuan…')} ro={ro(key)} loader={lookupLoaderFor(field)}
          onChange={(v, label) => set({ destinationWarehouseId: v, destinationWarehouseLabel: label ?? data.destinationWarehouseLabel })} />,
      );
    case 'requestedPartnerId':
      return row(
        <Picker value={data.requestedPartnerId} initialLabel={data.requestedPartnerLabel}
          ph={ph(key, 'Pilih partner…')} ro={ro(key)} loader={loadRequestedPartnerOptions}
          onChange={(v, label) => set({ requestedPartnerId: v, requestedPartnerLabel: label ?? data.requestedPartnerLabel })} />,
      );
    case 'requestedTo':
      return row(
        <Input value={data.requestedTo} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(e) => set({ requestedTo: e.target.value })} />,
      );
    case 'description':
      return row(
        <Input value={data.description} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(e) => set({ description: e.target.value })} />,
      );
    case 'referenceNo':
      return row(
        <Input value={data.referenceNo} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(e) => set({ referenceNo: e.target.value })} />,
      );
    case 'notes':
      return row(
        <Input value={data.notes} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(e) => set({ notes: e.target.value })} />,
      );
    case 'movementDate':
      return row(
        <DateInput value={data.movementDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ movementDate: v })} />,
      );
    case 'neededDate':
      return row(
        <DateInput value={data.neededDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ neededDate: v })} />,
      );
    case 'referenceDate':
      return row(
        <DateInput value={data.referenceDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ referenceDate: v })} />,
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
