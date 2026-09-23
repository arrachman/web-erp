'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listSubClasses, createErpSubClass, updateErpSubClass,
  deleteErpSubClass, bulkUpdateErpSubClassStatus,
  bulkDeleteErpSubClasses,
  type ErpSubClass, type CreateErpSubClassPayload,
} from '@/lib/api/sub-classes';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({ code: '', name: '', isActive: true });
const fromRecord = (r: ErpSubClass): FormData => ({ code: r.code, name: r.name, isActive: r.isActive });
const toPayload = (f: FormData): CreateErpSubClassPayload => ({ code: f.code, name: f.name, isActive: f.isActive });

const validateSubClass = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="SCLS-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Sub Class" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpSubClassesPage() {
  return (
    <SimpleMasterPage<ErpSubClass, FormData>
      title="Sub Class"
      code="SCLS"
      entityLabel="sub class"
      storageKey="sub-classes"
      auditEntityName="ErpSubClass"
      list={listSubClasses}
      create={createErpSubClass}
      update={updateErpSubClass}
      remove={deleteErpSubClass}
      bulkStatus={bulkUpdateErpSubClassStatus}
      bulkDelete={bulkDeleteErpSubClasses}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateSubClass}
    />
  );
}
