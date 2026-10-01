'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import { loadPostableAccountOptionsCoded } from '@/components/pages/items-form-lookups';
import {
  listStockAdjustmentTypes, createErpStockAdjustmentType, updateErpStockAdjustmentType, deleteErpStockAdjustmentType,
  bulkUpdateErpStockAdjustmentTypeStatus, bulkDeleteErpStockAdjustmentTypes,
  type ErpStockAdjustmentType, type CreateErpStockAdjustmentTypePayload, type StockAdjustmentTypeAccountSummary,
} from '@/lib/api/stock-adjustment-types';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  direction: string;
  accountId: string;
  accountLabel: string;
  isActive: boolean;
}

const accountLabel = (account?: StockAdjustmentTypeAccountSummary | null) => (
  account ? `${account.code} - ${account.name}` : ''
);

const displayAccount = (account?: StockAdjustmentTypeAccountSummary | null) => (
  account ? `${account.code} — ${account.name}` : '—'
);

const optionLabel = (opt: { label: string; code?: string }) => (
  `${opt.code ? `${opt.code} - ` : ''}${opt.label}`
);

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  direction: '',
  accountId: '',
  accountLabel: '',
  isActive: true,
});

const fromRecord = (r: ErpStockAdjustmentType): FormData => ({
  code: r.code,
  name: r.name,
  direction: r.direction == null ? '' : String(r.direction),
  accountId: r.accountId ?? '',
  accountLabel: accountLabel(r.account),
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpStockAdjustmentTypePayload => ({
  code: f.code,
  name: f.name,
  direction: f.direction || undefined,
  accountId: f.accountId || null,
  isActive: f.isActive,
});

const validateStockAdjustmentType = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (patch: Partial<FormData>) => onChange({ ...data, ...patch });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="sat-code" required error={errors.code}>
        <Input id="sat-code" value={data.code} onChange={(e) => set({ code: e.target.value })} placeholder="SAT-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="sat-name" required error={errors.name}>
        <Input id="sat-name" value={data.name} onChange={(e) => set({ name: e.target.value })} placeholder="Stock Adjustment Type" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Direction" htmlFor="sat-direction">
        <Input id="sat-direction" value={data.direction ?? ''} onChange={(e) => set({ direction: e.target.value })} placeholder="IN | OUT | TRANSFER" />
      </FormField>
      <FormField label="No Akun" htmlFor="sat-account" error={errors.accountId}>
        <SearchSelect
          id="sat-account"
          value={data.accountId}
          onValueChange={(value) => set({ accountId: value })}
          onPick={(opt) => set({ accountId: opt.value, accountLabel: optionLabel(opt) })}
          placeholder="Pilih akun postable…"
          loadOptions={loadPostableAccountOptionsCoded}
          initialLabel={data.accountLabel}
          title="No Akun"
          error={!!errors.accountId}
        />
      </FormField>
      <FormField label="Status" htmlFor="sat-active">
        <BooleanRadio id="sat-active" value={data.isActive} onValueChange={(v) => set({ isActive: v })} />
      </FormField>
    </div>
  );
}

export function ErpStockAdjustmentTypesPage() {
  return (
    <SimpleMasterPage<ErpStockAdjustmentType, FormData>
      title="Stock Adjustment Type"
      code="SAT"
      entityLabel="stock adjustment type"
      storageKey="stock-adjustment-types"
      auditEntityName="ErpStockAdjustmentType"
      list={listStockAdjustmentTypes}
      create={createErpStockAdjustmentType}
      update={updateErpStockAdjustmentType}
      remove={deleteErpStockAdjustmentType}
      bulkStatus={bulkUpdateErpStockAdjustmentTypeStatus}
      bulkDelete={bulkDeleteErpStockAdjustmentTypes}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateStockAdjustmentType}
      modalSize="lg"
      extraColumns={[
        {
          key: 'direction',
          label: 'Direction',
          render: (row) => row.direction || '—',
        },
        {
          key: 'accountId',
          label: 'No Akun',
          render: (row) => displayAccount(row.account),
        },
      ]}
    />
  );
}
