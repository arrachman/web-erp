'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listDivisions, createDivision, updateDivision, deleteDivision,
  bulkUpdateDivisionStatus, bulkDeleteDivisions,
  type ErpDivision, type CreateDivisionPayload,
} from '@/lib/api/divisions';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface DivisionForm {
  code: string;
  name: string;
  isActive: boolean;
  barcode: string;
  note: string;
}

const defaultForm = (): DivisionForm => ({ code: '', name: '', isActive: true, barcode: '', note: '' });

const fromRecord = (r: ErpDivision): DivisionForm => ({
  code: r.code, name: r.name, isActive: r.isActive,
  barcode: r.barcode ?? '', note: r.note ?? '',
});

const toPayload = (f: DivisionForm): CreateDivisionPayload => ({
  code: f.code, name: f.name, isActive: f.isActive,
  barcode: f.barcode || undefined, note: f.note || undefined,
});

const validateDivision = (form: DivisionForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function DivisionFormFields({ data, onChange, errors = {} }: { data: DivisionForm; onChange: (d: DivisionForm) => void; errors?: FormErrors<DivisionForm> }) {
  const set = (k: keyof DivisionForm, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="dv-code" required error={errors.code}>
        <Input id="dv-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="DIV-OPS" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="dv-name" required error={errors.name}>
        <Input id="dv-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Operations" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Barcode" htmlFor="dv-barcode">
        <Input id="dv-barcode" value={data.barcode} onChange={(e) => set('barcode', e.target.value)} placeholder="DIV-OPS-001" />
      </FormField>
      <FormField label="Catatan" htmlFor="dv-note">
        <Input id="dv-note" value={data.note} onChange={(e) => set('note', e.target.value)} placeholder="Catatan tambahan" />
      </FormField>
      <FormField label="Status" htmlFor="dv-active">
        <BooleanRadio id="dv-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpDivisionsPage() {
  return (
    <SimpleMasterPage<ErpDivision, DivisionForm>
      title="Division"
      code="DIV"
      entityLabel="division"
      storageKey="divisions"
      auditEntityName="ErpDivision"
      list={listDivisions}
      create={createDivision}
      update={updateDivision}
      remove={deleteDivision}
      bulkStatus={bulkUpdateDivisionStatus}
      bulkDelete={bulkDeleteDivisions}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={DivisionFormFields}
      validate={validateDivision}
      defaultSortBy="code"
      defaultSortDir="asc"
    />
  );
}
