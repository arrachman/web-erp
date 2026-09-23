'use client';

/**
 * Inventory price-adjustment form. Header-only (no item-line grid) — PA is a
 * process trigger that fires a backend re-costing job. Config-driven header via
 * Form Builder (useFormFields('INV.PA')); structural fields rendered by
 * InvPriceAdjustmentStructuralField. Create-only: no update/edit mode. After
 * create the result status (PENDING → COMPLETED | FAILED) is shown read-only.
 * Accepts TrxFormPageProps for registry compatibility; only 'create' supported.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { FormFieldRow } from '@/components/molecules/form-field-row';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Icon } from '@/components/ui/icons';
import { notify } from '@/lib/feedback';
import { statusBadgeVariant } from '@/lib/status';
import { type FormColumnSlot } from '@/lib/api/form-fields';
import { useFormFields, buildFormConfig } from '@/lib/use-form-fields';
import {
  DEFAULT_INV_PA_FORM_FIELDS,
  type InvPriceAdjustmentFormData,
} from './inv-price-adjustment-form-model';
import {
  InvPriceAdjustmentStructuralField,
  type InvPriceAdjustmentStructuralFieldCtx,
} from '@/components/molecules/inv-price-adjustment-structural-field';
import { CashBankCustomField } from '@/components/molecules/cash-bank-custom-fields';
import type { TrxFormPageProps } from '@/lib/trx-route';

const SLOTS: FormColumnSlot[] = ['LEFT', 'CENTER', 'RIGHT'];

const COSTING_OPTIONS = [
  { value: 'AVG', label: 'Moving Average (AVG)' },
  { value: 'FIFO', label: 'FIFO' },
  { value: 'STD', label: 'Standard Cost (STD)' },
];

const PA_STATUS_BADGE: Record<string, 'default' | 'warn' | 'success' | 'danger' | 'info'> = {
  PENDING: 'warn',
  COMPLETED: 'success',
  FAILED: 'danger',
};

function paBadgeVariant(status: string): 'default' | 'warn' | 'success' | 'danger' | 'info' {
  return PA_STATUS_BADGE[status] ?? 'default';
}

export interface InvPriceAdjustmentFormOwnProps {
  data: InvPriceAdjustmentFormData;
  onChange: (d: InvPriceAdjustmentFormData) => void;
  transactionCode?: string;
  saving?: boolean;
  onSave: () => void;
  onReset: () => void;
}

// Accept both own props and TrxFormPageProps for registry compatibility.
export type InvPriceAdjustmentFormProps = InvPriceAdjustmentFormOwnProps & TrxFormPageProps;

export function InvPriceAdjustmentForm({
  data,
  onChange,
  transactionCode,
  saving,
  onSave,
  onReset,
}: InvPriceAdjustmentFormOwnProps) {
  const set = (p: Partial<InvPriceAdjustmentFormData>) => onChange({ ...data, ...p });

  const fallbackConfig = React.useMemo(
    () => buildFormConfig(DEFAULT_INV_PA_FORM_FIELDS),
    [],
  );
  const loaded = useFormFields(transactionCode);
  const formConfig = Object.keys(loaded.byKey).length ? loaded : fallbackConfig;

  const isExisting = !!data.id;

  const ph = (key: string, fallback: string) => formConfig.byKey[key]?.placeholder || fallback;
  const ro = (key: string) => isExisting || formConfig.byKey[key]?.isReadonly === true;

  const ctx: InvPriceAdjustmentStructuralFieldCtx = { data, set, ph, ro };

  const setCustomField = (key: string, value: string | number | null) =>
    set({ customFields: { ...((data as unknown as Record<string, unknown>).customFields as Record<string, string | number | null> | undefined ?? {}), [key]: value } } as unknown as Partial<InvPriceAdjustmentFormData>);

  const renderSlot = (slot: FormColumnSlot) =>
    formConfig.slotFields[slot]
      .filter((f) => f.isVisible)
      .map((f) =>
        f.kind === 'STRUCTURAL' ? (
          <InvPriceAdjustmentStructuralField key={f.fieldKey} field={f} ctx={ctx} />
        ) : (
          <CashBankCustomField
            key={f.fieldKey}
            field={f}
            value={null}
            onChange={(v) => setCustomField(f.fieldKey, v)}
            disabled={isExisting}
          />
        ),
      );

  const handleSave = () => {
    if (!data.fromDate) {
      notify('Dari Tanggal wajib diisi.', 'warn');
      return;
    }
    onSave();
  };

  return (
    <div className="inv-price-adjustment-form flex flex-col gap-4">
      {/* Toolbar */}
      <div className="flex items-center gap-2 flex-wrap">
        {!isExisting && (
          <button
            type="button"
            className="btn primary"
            onClick={handleSave}
            disabled={saving}
          >
            <Icon name="save" size={13} /> Jalankan
          </button>
        )}
        <button
          type="button"
          className="btn ghost"
          onClick={onReset}
          disabled={saving}
        >
          <Icon name="refresh" size={13} /> Reset
        </button>
        <div className="flex-1" />
        {isExisting && (
          <Badge variant={paBadgeVariant(data.status)} dot>
            {data.status}
          </Badge>
        )}
      </div>

      {/* Costing Method — always shown outside Form Builder slots (enum ≥3 values) */}
      <div className="rounded-lg border border-border p-4">
        <FormFieldRow label="Metode Costing" required>
          <Select
            value={data.costingMethod}
            onValueChange={(v) => set({ costingMethod: v as InvPriceAdjustmentFormData['costingMethod'] })}
            disabled={isExisting}
          >
            <SelectTrigger>
              <SelectValue placeholder="Pilih metode costing…" />
            </SelectTrigger>
            <SelectContent>
              {COSTING_OPTIONS.map((o) => (
                <SelectItem key={o.value} value={o.value}>
                  {o.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </FormFieldRow>
      </div>

      {/* Header — config-driven; LEFT/CENTER/RIGHT slots */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-x-6 gap-y-3 rounded-lg border border-border p-4">
        {SLOTS.map((slot) => (
          <div key={slot} className="flex flex-col gap-3">
            {renderSlot(slot)}
          </div>
        ))}
      </div>

      {/* Result — shown when record exists */}
      {isExisting && data.status === 'COMPLETED' && (
        <dl className="grid grid-cols-2 gap-y-2 gap-x-6 text-sm max-w-xl">
          <dt className="text-muted-foreground">No Transaksi</dt>
          <dd className="mono">{data.docNumber || '—'}</dd>
          <dt className="text-muted-foreground">Status</dt>
          <dd>
            <Badge variant={paBadgeVariant(data.status)} dot>
              {data.status}
            </Badge>
          </dd>
        </dl>
      )}
    </div>
  );
}

/** Re-export statusBadgeVariant alias for PA statuses. */
export { paBadgeVariant };
