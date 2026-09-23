'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect, type SearchSelectOption } from '@/components/molecules/search-select';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listProvinces, createErpProvince, updateErpProvince, deleteErpProvince,
  bulkUpdateErpProvinceStatus, bulkDeleteErpProvinces,
  type ErpProvince, type CreateErpProvincePayload,
} from '@/lib/api/provinces';
import { listCountries } from '@/lib/api/countries';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  countryId: string;
  countryName: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  countryId: '',
  countryName: '',
  isActive: true,
});

const fromRecord = (r: ErpProvince): FormData => ({
  code: r.code,
  name: r.name,
  countryId: r.countryId == null ? '' : String(r.countryId),
  countryName: r.countryName ?? '',
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpProvincePayload => ({
  code: f.code,
  name: f.name,
  countryId: f.countryId,
  isActive: f.isActive,
});

const validateProvince = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'countryId', label: 'Negara', required: true },
  ]);

const loadCountryOptions = async (search: string, page: number, limit: number) => {
  const res = await listCountries({ page, limit, search: search || undefined, isActive: true });
  return { data: res.data.map((c) => ({ value: c.id, label: c.name, code: c.isoCode ?? undefined })), total: res.meta.total };
};

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });

  // Auto-default ke Indonesia saat create baru (countryId kosong)
  React.useEffect(() => {
    if (data.countryId) return;
    listCountries({ limit: 50, isActive: true }).then((res) => {
      const indonesia = res.data.find((c) => c.isoCode === 'ID' || c.code === 'ID');
      if (indonesia) onChange({ ...data, countryId: indonesia.id });
    }).catch(() => {});
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="PRV-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Province" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Negara" htmlFor="ef-countryId" required error={errors.countryId}>
        <SearchSelect
          id="ef-countryId"
          value={data.countryId}
          onValueChange={(v) => onChange({ ...data, countryId: v, countryName: '' })}
          placeholder="Pilih negara…"
          loadOptions={loadCountryOptions}
          initialLabel={data.countryName || undefined}
          error={!!errors.countryId}
        />
      </FormField>
      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpProvincesPage() {
  return (
    <SimpleMasterPage<ErpProvince, FormData>
      title="Province"
      code="PRV"
      entityLabel="province"
      storageKey="provinces"
      auditEntityName="ErpProvince"
      list={listProvinces}
      create={createErpProvince}
      update={updateErpProvince}
      remove={deleteErpProvince}
      bulkStatus={bulkUpdateErpProvinceStatus}
      bulkDelete={bulkDeleteErpProvinces}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateProvince}
    />
  );
}
