'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listPriceIndices, createErpPriceIndex, updateErpPriceIndex, deleteErpPriceIndex,
  bulkUpdateErpPriceIndexStatus, bulkDeleteErpPriceIndices,
  type ErpPriceIndex, type CreateErpPriceIndexPayload,
} from '@/lib/api/price-indices';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  margin: number;
  notes: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  margin: 0,
  notes: '',
  isActive: true,
});

const fromRecord = (r: ErpPriceIndex): FormData => ({
  code: r.code,
  name: r.name,
  margin: parseFloat(r.margin) || 0,
  notes: r.notes ?? '',
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpPriceIndexPayload => ({
  code: f.code,
  name: f.name,
  margin: f.margin,
  notes: f.notes || undefined,
  isActive: f.isActive,
});

const validatePriceIndex = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | number | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="PRX-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Price Index" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Margin" htmlFor="ef-margin">
        <Input id="ef-margin" type="number" value={data.margin} onChange={(e) => set('margin', parseFloat(e.target.value) || 0)} placeholder="0.00" />
      </FormField>
      <FormField label="Catatan" htmlFor="ef-notes">
        <Input id="ef-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} placeholder="Catatan (opsional)" />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpPriceIndicesPage() {
  return (
    <SimpleMasterPage<ErpPriceIndex, FormData>
      title="Price Index"
      code="PRX"
      entityLabel="price index"
      storageKey="price-indices"
      auditEntityName="ErpPriceIndex"
      list={listPriceIndices}
      create={createErpPriceIndex}
      update={updateErpPriceIndex}
      remove={deleteErpPriceIndex}
      bulkStatus={bulkUpdateErpPriceIndexStatus}
      bulkDelete={bulkDeleteErpPriceIndices}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validatePriceIndex}
    />
  );
}
