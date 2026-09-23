'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect, type SearchSelectOption } from '@/components/molecules/search-select';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listSubDivisions, createSubDivision, updateSubDivision, deleteSubDivision,
  bulkUpdateSubDivisionStatus, bulkDeleteSubDivisions,
  type ErpSubDivision, type CreateSubDivisionPayload,
} from '@/lib/api/sub-divisions';
import { listDivisions } from '@/lib/api/divisions';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface SubDivisionForm {
  code: string;
  name: string;
  divisionId: string;
  isActive: boolean;
}

const defaultForm = (): SubDivisionForm => ({ code: '', name: '', divisionId: '', isActive: true });

const fromRecord = (r: ErpSubDivision): SubDivisionForm => ({
  code: r.code, name: r.name, divisionId: r.divisionId, isActive: r.isActive,
});

const toPayload = (f: SubDivisionForm): CreateSubDivisionPayload => ({
  code: f.code, name: f.name, divisionId: f.divisionId, isActive: f.isActive,
});

const validateSubDivision = (form: SubDivisionForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'divisionId', label: 'Division', required: true },
  ]);

async function loadDivisionOptions(search: string, page: number, limit: number) {
  const res = await listDivisions({ search: search || undefined, page, limit, isActive: true });
  return { data: res.data.map((d) => ({ value: d.id, label: d.name, code: d.code })), total: res.meta.total };
}

function SubDivisionFormFields({ data, onChange, errors = {} }: { data: SubDivisionForm; onChange: (d: SubDivisionForm) => void; errors?: FormErrors<SubDivisionForm> }) {
  const set = <K extends keyof SubDivisionForm>(k: K, v: SubDivisionForm[K]) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="sd-code" required error={errors.code}>
        <Input id="sd-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="SUB-OPS-A" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="sd-name" required error={errors.name}>
        <Input id="sd-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Operations A" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Division" htmlFor="sd-div" required error={errors.divisionId}>
        <SearchSelect
          id="sd-div"
          placeholder="Cari division…"
          value={data.divisionId}
          onValueChange={(v) => set('divisionId', v)}
          loadOptions={loadDivisionOptions}
          error={!!errors.divisionId}
        />
      </FormField>
      <FormField label="Status" htmlFor="sd-active">
        <BooleanRadio id="sd-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const extraColumns: ExtraColumn<ErpSubDivision>[] = [
  { key: 'division', label: 'Division', render: (r) => r.division ? `${r.division.code} — ${r.division.name}` : '—' },
];

export function ErpSubDivisionsPage() {
  return (
    <SimpleMasterPage<ErpSubDivision, SubDivisionForm>
      title="Sub Division" code="SDIV" entityLabel="sub division"
      storageKey="sub-divisions" auditEntityName="ErpSubdivision"
      list={listSubDivisions} create={createSubDivision} update={updateSubDivision} remove={deleteSubDivision}
      bulkStatus={bulkUpdateSubDivisionStatus} bulkDelete={bulkDeleteSubDivisions}
      defaultForm={defaultForm} fromRecord={fromRecord} toPayload={toPayload}
      FormFields={SubDivisionFormFields}
      extraColumns={extraColumns}
      validate={validateSubDivision}
    />
  );
}
