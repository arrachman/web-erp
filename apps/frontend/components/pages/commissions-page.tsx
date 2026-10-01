'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listCommissions, createErpCommission, updateErpCommission, deleteErpCommission,
  bulkUpdateErpCommissionStatus, bulkDeleteErpCommissions,
  type ErpCommission, type CreateErpCommissionPayload,
} from '@/lib/api/commissions';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  amount: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  amount: '',
  isActive: true,
});

const fromRecord = (r: ErpCommission): FormData => ({
  code: r.code,
  name: r.name,
  amount: r.amount == null ? '' : String(r.amount),
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpCommissionPayload => ({
  code: f.code,
  name: f.name,
  amount: f.amount || undefined,
  isActive: f.isActive,
});

const validateCommission = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="CMS-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Commission" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Amount" htmlFor="ef-amount">
        <Input id="ef-amount" value={data.amount ?? ''} onChange={(e) => set('amount', e.target.value)} />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpCommissionsPage() {
  return (
    <SimpleMasterPage<ErpCommission, FormData>
      title="Commission"
      code="CMS"
      entityLabel="commission"
      storageKey="commissions"
      auditEntityName="ErpCommission"
      list={listCommissions}
      create={createErpCommission}
      update={updateErpCommission}
      remove={deleteErpCommission}
      bulkStatus={bulkUpdateErpCommissionStatus}
      bulkDelete={bulkDeleteErpCommissions}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateCommission}
    />
  );
}
