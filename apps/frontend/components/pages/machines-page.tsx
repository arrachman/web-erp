'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listMachines, createErpMachine, updateErpMachine,
  deleteErpMachine, bulkUpdateErpMachineStatus,
  bulkDeleteErpMachines,
  type ErpMachine, type CreateErpMachinePayload,
} from '@/lib/api/machines';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({ code: '', name: '', isActive: true });
const fromRecord = (r: ErpMachine): FormData => ({ code: r.code, name: r.name, isActive: r.isActive });
const toPayload = (f: FormData): CreateErpMachinePayload => ({ code: f.code, name: f.name, isActive: f.isActive });

const validateMachine = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="MCH-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Machine" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpMachinesPage() {
  return (
    <SimpleMasterPage<ErpMachine, FormData>
      title="Machine"
      code="MCH"
      entityLabel="machine"
      storageKey="machines"
      auditEntityName="ErpMachine"
      list={listMachines}
      create={createErpMachine}
      update={updateErpMachine}
      remove={deleteErpMachine}
      bulkStatus={bulkUpdateErpMachineStatus}
      bulkDelete={bulkDeleteErpMachines}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateMachine}
    />
  );
}
