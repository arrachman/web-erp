'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect, type SearchSelectOption } from '@/components/molecules/search-select';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listAreas, createErpArea, updateErpArea, deleteErpArea,
  bulkUpdateErpAreaStatus, bulkDeleteErpAreas,
  type ErpArea, type CreateErpAreaPayload,
} from '@/lib/api/areas';
import { listCities } from '@/lib/api/cities';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  cityId: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  cityId: '',
  isActive: true,
});

const fromRecord = (r: ErpArea): FormData => ({
  code: r.code,
  name: r.name,
  cityId: String(r.cityId),
  isActive: r.isActive,
});

const loadCityOptions = async (search: string, page: number, limit: number) => {
  const res = await listCities({ page, limit, search: search || undefined, isActive: true });
  return { data: res.data.map((c) => ({ value: c.id, label: c.name, code: c.code })), total: res.meta.total };
};

const toPayload = (f: FormData): CreateErpAreaPayload => ({
  code: f.code,
  name: f.name,
  cityId: f.cityId,
  isActive: f.isActive,
});

const validateArea = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'cityId', label: 'Kota', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="AREA-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Area" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Kota" htmlFor="ef-cityId" required error={errors.cityId}>
        <SearchSelect
          id="ef-cityId"
          value={data.cityId}
          onValueChange={(v) => set('cityId', v)}
          placeholder="Pilih kota…"
          loadOptions={loadCityOptions}
          error={!!errors.cityId}
        />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpAreasPage() {
  return (
    <SimpleMasterPage<ErpArea, FormData>
      title="Area"
      code="AREA"
      entityLabel="area"
      storageKey="areas"
      auditEntityName="ErpArea"
      list={listAreas}
      create={createErpArea}
      update={updateErpArea}
      remove={deleteErpArea}
      bulkStatus={bulkUpdateErpAreaStatus}
      bulkDelete={bulkDeleteErpAreas}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateArea}
    />
  );
}
