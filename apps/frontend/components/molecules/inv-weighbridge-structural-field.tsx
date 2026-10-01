'use client';

/**
 * Renders a single STRUCTURAL header field for the inventory weighbridge-ticket
 * form. Each fieldKey binds to a real column on InvWeighbridgeTicketFormData.
 * This switch IS the field→column binding. Everything else (label, order,
 * visibility, placeholder, required, readonly) comes from Form Builder config.
 * Atomic tier: Molecule.
 */

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { DateInput } from '@/components/ui/date-input';
import { NumInput } from '@/components/molecules/num-input';
import { SearchSelect } from '@/components/molecules/search-select';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import { buildLookupLoader } from '@/lib/lookup-source-registry';
import {
  loadBranchOptions,
  loadLocationOptions,
  loadPartnerOptions,
} from '@/components/pages/items-form-lookups';
import type { SearchSelectProps } from '@/components/molecules/search-select-types';
import type { ErpFormField } from '@/lib/api/form-fields';
import type { InvWeighbridgeTicketFormData } from '@/components/pages/inv-weighbridge-ticket-form-model';

export interface InvWeighbridgeStructuralFieldCtx {
  data: InvWeighbridgeTicketFormData;
  set: (p: Partial<InvWeighbridgeTicketFormData>) => void;
  ph: (key: string, fallback: string) => string;
  ro: (key: string) => boolean;
  locked: boolean;
}

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

/** Renders the bound control for one structural RW field, wrapped in a label row. */
export function InvWeighbridgeStructuralField({
  field,
  ctx,
}: {
  field: ErpFormField;
  ctx: InvWeighbridgeStructuralFieldCtx;
}) {
  const { data, set, ph, ro, locked } = ctx;
  const key = field.fieldKey;
  const row = (children: React.ReactNode) => (
    <FormFieldRow label={field.label} required={field.isRequired}>
      {children}
    </FormFieldRow>
  );

  switch (key) {
    case 'partnerId':
      return row(
        <Picker
          value={data.partnerId}
          initialLabel={data.partnerLabel}
          ph={ph(key, 'Pilih partner…')}
          ro={ro(key)}
          loader={loadPartnerOptions}
          onChange={(v, label) =>
            set({ partnerId: v, partnerLabel: label ?? data.partnerLabel })
          }
        />,
      );
    case 'vehiclePlate':
      return row(
        <Input
          value={data.vehiclePlate}
          placeholder={ph(key, 'No kendaraan')}
          disabled={ro(key)}
          onChange={(e) => set({ vehiclePlate: e.target.value })}
        />,
      );
    case 'driverName':
      return row(
        <Input
          value={data.driverName}
          placeholder={ph(key, 'Nama pengemudi')}
          disabled={ro(key)}
          onChange={(e) => set({ driverName: e.target.value })}
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
    case 'grossWeight':
      return row(
        <NumInput
          value={data.grossWeight}
          placeholder={ph(key, '0')}
          disabled={ro(key)}
          onChange={(v) => set({ grossWeight: v })}
        />,
      );
    case 'tareWeight':
      return row(
        <NumInput
          value={data.tareWeight}
          placeholder={ph(key, '0')}
          disabled={ro(key)}
          onChange={(v) => set({ tareWeight: v })}
        />,
      );
    case 'ticketDate':
      return row(
        <DateInput
          value={data.ticketDate}
          placeholder={field.placeholder || undefined}
          disabled={ro(key)}
          onChange={(v) => set({ ticketDate: v })}
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
    case 'unitPrice':
      return row(
        <NumInput
          value={data.unitPrice}
          placeholder={ph(key, '0')}
          disabled={ro(key)}
          onChange={(v) => set({ unitPrice: v })}
        />,
      );
    default:
      return null;
  }
}
