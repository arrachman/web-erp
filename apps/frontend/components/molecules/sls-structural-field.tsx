'use client';

/**
 * Renders a single STRUCTURAL header field for the sales transaction form.
 * Each structural fieldKey is bound to a real column on SlsOrderFormData (and,
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
  loadDivisionOptions,
  loadCurrencyOptions,
} from '@/components/pages/items-form-lookups';
import { loadCustomerOptions, loadPaymentTermOptions } from '@/components/pages/sls-form-lookups';
import { getPartnerForAutoFill } from '@/lib/api/sls-orders';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { SlsOrderFormData } from '@/components/pages/sls-order-form-model';

export interface SlsStructuralFieldCtx {
  data: SlsOrderFormData;
  set: (p: Partial<SlsOrderFormData>) => void;
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
export function SlsStructuralField({ field, ctx }: { field: ErpFormField; ctx: SlsStructuralFieldCtx }) {
  const { data, set, ph, ro, locked } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>{children}</FormFieldRow>
  );

  switch (key) {
    case 'customerId':
      return row(
        <Picker value={data.customerId} initialLabel={data.customerLabel}
          ph={ph(key, 'Pilih pelanggan…')} ro={ro(key)} loader={loadCustomerOptions}
          onChange={async (v, label) => {
            const patch: Partial<SlsOrderFormData> = { customerId: v, customerLabel: label ?? data.customerLabel, salesTier: null };
            if (v) {
              const p = await getPartnerForAutoFill(v).catch(() => null);
              if (p) {
                patch.salesTier = p.category?.salesTier ?? null;
                if (p.saleTermId && !data.paymentTermId) {
                  patch.paymentTermId = p.saleTermId;
                  patch.paymentTermLabel = p.saleTerm?.name;
                  // Derive dueDate if docDate known
                  if (data.docDate && p.saleTerm?.netDays) {
                    const d = new Date(data.docDate);
                    d.setDate(d.getDate() + p.saleTerm.netDays);
                    patch.dueDate = d.toISOString().slice(0, 10);
                  }
                }
                if (p.currencyId && !data.currencyId) patch.currencyId = p.currencyId;
                if (p.receivableAccountId && !data.receivableAccountId)
                  patch.receivableAccountId = p.receivableAccountId;
              }
            }
            set(patch);
          }} />,
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
    case 'salesDeptId':
      return row(
        <Picker value={data.salesDeptId} initialLabel={data.salesDeptLabel}
          ph={ph(key, 'Pilih divisi…')} ro={ro(key)} loader={loadDivisionOptions}
          onChange={(v, label) => set({ salesDeptId: v, salesDeptLabel: label ?? data.salesDeptLabel })} />,
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
