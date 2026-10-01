'use client';

/**
 * Renders a single STRUCTURAL header field for the purchasing transaction form.
 * Each structural fieldKey is bound to a real column on PurOrderFormData (and,
 * downstream, a DB column), so the input component per key is fixed here — this
 * switch IS the field→column binding. Everything else (label, order, visibility,
 * placeholder, required, readonly) comes from Form Builder config (§ Header form
 * transaksi render dinamis).
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { formatNumber } from '@/lib/format';
import {
  loadBranchOptions,
  loadLocationOptions,
  loadWarehouseOptions,
  loadAccountOptions,
  loadCurrencyOptions,
} from '@/components/pages/items-form-lookups';
import { loadSupplierOptions, loadPaymentTermOptions } from '@/components/pages/pur-form-lookups';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { PurOrderFormData } from '@/components/pages/pur-order-form-model';

export interface PurStructuralFieldCtx {
  data: PurOrderFormData;
  set: (p: Partial<PurOrderFormData>) => void;
  /** Placeholder resolver: config placeholder or the given fallback. */
  ph: (key: string, fallback: string) => string;
  /** Read-only resolver: locked document or config isReadonly. */
  ro: (key: string) => boolean;
  /** Whole document locked (non-editable status). */
  locked: boolean;
  /** Trigger label for the selected currency ("IDR - Rupiah"). */
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

/** Renders the bound control for one structural field, wrapped in a label row. */
export function PurStructuralField({ field, ctx }: { field: ErpFormField; ctx: PurStructuralFieldCtx }) {
  const { data, set, ph, ro, locked } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>{children}</FormFieldRow>
  );

  switch (key) {
    case 'supplierId':
      return row(
        <Picker value={data.supplierId} initialLabel={data.supplierLabel}
          ph={ph(key, 'Pilih supplier…')} ro={ro(key)} loader={loadSupplierOptions}
          onChange={(v, label) => set({ supplierId: v, supplierLabel: label ?? data.supplierLabel })} />,
      );
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
    case 'warehouseId':
      return row(
        <Picker value={data.warehouseId} initialLabel={data.warehouseLabel}
          ph={ph(key, 'Pilih gudang…')} ro={ro(key)} loader={loadWarehouseOptions}
          onChange={(v, label) => set({ warehouseId: v, warehouseLabel: label ?? data.warehouseLabel })} />,
      );
    case 'payableAccountId':
      return row(
        <Picker value={data.payableAccountId} initialLabel={data.payableAccountLabel}
          ph={ph(key, 'Pilih akun hutang…')} ro={ro(key)} loader={loadAccountOptions}
          onChange={(v, label) => set({ payableAccountId: v, payableAccountLabel: label ?? data.payableAccountLabel })} />,
      );
    case 'paymentTermId':
      return row(
        <Picker value={data.paymentTermId} initialLabel={data.paymentTermLabel}
          ph={ph(key, 'Pilih termin…')} ro={ro(key)} loader={loadPaymentTermOptions}
          onChange={(v, label) => set({ paymentTermId: v, paymentTermLabel: label ?? data.paymentTermLabel })} />,
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
    case 'docDate':
      return row(
        <DateInput value={data.docDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ docDate: v })} />,
      );
    case 'dueDate':
      return row(
        <DateInput value={data.dueDate} placeholder={field.placeholder || undefined}
          disabled={ro(key)} onChange={(v) => set({ dueDate: v })} />,
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
