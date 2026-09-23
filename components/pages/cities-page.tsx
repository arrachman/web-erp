'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect, type SearchSelectOption } from '@/components/molecules/search-select';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listCities, createErpCity, updateErpCity, deleteErpCity,
  bulkUpdateErpCityStatus, bulkDeleteErpCities,
  type ErpCity, type CreateErpCityPayload,
} from '@/lib/api/cities';
import { listProvinces } from '@/lib/api/provinces';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  provinceId: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  provinceId: '',
  isActive: true,
});

const fromRecord = (r: ErpCity): FormData => ({
  code: r.code,
  name: r.name,
  provinceId: r.provinceId == null ? '' : String(r.provinceId),
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpCityPayload => ({
  code: f.code,
  name: f.name,
  provinceId: f.provinceId,
  isActive: f.isActive,
});

const loadProvinceOptions = async (search: string, page: number, limit: number) => {
  const res = await listProvinces({ page, limit, search: search || undefined, isActive: true });
  return { data: res.data.map((p) => ({ value: p.id, label: p.name, code: p.code })), total: res.meta.total };
};

const validateCity = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'provinceId', label: 'Provinsi', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="CTYC-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="City" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Provinsi" htmlFor="ef-provinceId" required error={errors.provinceId}>
        <SearchSelect
          id="ef-provinceId"
          value={data.provinceId}
          onValueChange={(v) => set('provinceId', v)}
          placeholder="Pilih provinsi…"
          loadOptions={loadProvinceOptions}
          error={!!errors.provinceId}
        />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpCitiesPage() {
  return (
    <SimpleMasterPage<ErpCity, FormData>
      title="City"
      code="CTYC"
      entityLabel="city"
      storageKey="cities"
      auditEntityName="ErpCity"
      list={listCities}
      create={createErpCity}
      update={updateErpCity}
      remove={deleteErpCity}
      bulkStatus={bulkUpdateErpCityStatus}
      bulkDelete={bulkDeleteErpCities}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateCity}
    />
  );
}
