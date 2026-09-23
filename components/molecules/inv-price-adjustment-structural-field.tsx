'use client';

/**
 * Renders a single STRUCTURAL header field for the inventory price-adjustment
 * form. Each structural fieldKey binds to a real column on
 * InvPriceAdjustmentFormData (and downstream a DB column) — this switch IS the
 * field→column binding. PA has no auto-numbering checkbox (docNumber is always
 * manual or left blank). Everything else (label, order, visibility, placeholder,
 * required, readonly) comes from Form Builder config. Atomic tier: Molecule.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { buildLookupLoader } from '@/lib/lookup-source-registry';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { InvPriceAdjustmentFormData } from '@/components/pages/inv-price-adjustment-form-model';

export interface InvPriceAdjustmentStructuralFieldCtx {
  data: InvPriceAdjustmentFormData;
  set: (p: Partial<InvPriceAdjustmentFormData>) => void;
  ph: (key: string, fallback: string) => string;
  ro: (key: string) => boolean;
}

const warehouseLoader = buildLookupLoader('warehouses');
const itemLoader = buildLookupLoader('items');

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

/** Renders the bound control for one structural PA field, wrapped in a label row. */
export function InvPriceAdjustmentStructuralField({
  field,
  ctx,
}: {
  field: ErpFormField;
  ctx: InvPriceAdjustmentStructuralFieldCtx;
}) {
  const { data, set, ph, ro } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>
      {children}
    </FormFieldRow>
  );

  switch (key) {
    case 'warehouseId':
      return row(
        <Picker
          value={data.warehouseId}
          initialLabel={data.warehouseLabel}
          ph={ph(key, 'Pilih gudang…')}
          ro={ro(key)}
          loader={warehouseLoader as SearchSelectProps['loadOptions']}
          onChange={(v, label) =>
            set({ warehouseId: v, warehouseLabel: label ?? data.warehouseLabel })
          }
        />,
      );
    case 'itemId':
      return row(
        <Picker
          value={data.itemId}
          initialLabel={data.itemLabel}
          ph={ph(key, 'Pilih item…')}
          ro={ro(key)}
          loader={itemLoader as SearchSelectProps['loadOptions']}
          onChange={(v, label) =>
            set({ itemId: v, itemLabel: label ?? data.itemLabel })
          }
        />,
      );
    case 'fromDate':
      return row(
        <DateInput
          value={data.fromDate}
          placeholder={field.placeholder || undefined}
          disabled={ro(key)}
          onChange={(v) => set({ fromDate: v })}
        />,
      );
    case 'toDate':
      return row(
        <DateInput
          value={data.toDate}
          placeholder={field.placeholder || undefined}
          disabled={ro(key)}
          onChange={(v) => set({ toDate: v })}
        />,
      );
    case 'docNumber':
      return row(
        <Input
          value={data.docNumber}
          placeholder={ph(key, 'No transaksi')}
          disabled={ro(key)}
          onChange={(e) => set({ docNumber: e.target.value })}
        />,
      );
    case 'notes':
      return row(
        <Input
          value={data.notes}
          placeholder={field.placeholder || undefined}
          disabled={ro(key)}
          onChange={(e) => set({ notes: e.target.value })}
        />,
      );
    default:
      return null;
  }
}
