'use client';

/**
 * F3 Master Data — Unit of Measure page.
 * Uses SimpleMasterPage organism for full-feature CRUD.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { NumInput } from '@/components/molecules/num-input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listUnits, createUnit, updateUnit, deleteUnit,
  bulkUpdateErpUnitStatus, bulkDeleteErpUnits,
  type ErpUnit, type CreateUnitPayload,
} from '@/lib/api/units';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface UnitForm {
  code: string;
  name: string;
  isActive: boolean;
  conversionFactor: string;
}

const defaultForm = (): UnitForm => ({ code: '', name: '', isActive: true, conversionFactor: '1' });

const fromRecord = (r: ErpUnit): UnitForm => ({
  code: r.code,
  name: r.name,
  isActive: r.isActive,
  conversionFactor: r.conversionFactor ?? '1',
});

const toPayload = (f: UnitForm): CreateUnitPayload => ({
  code: f.code,
  name: f.name,
  isActive: f.isActive,
  conversionFactor: f.conversionFactor,
});

const validateUnit = (form: UnitForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: UnitForm; onChange: (d: UnitForm) => void; errors?: FormErrors<UnitForm> }) {
  const set = (k: keyof UnitForm, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="uf-code" required error={errors.code}>
        <Input id="uf-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="KG" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="uf-name" required error={errors.name}>
        <Input id="uf-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Kilogram" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Faktor Konversi" htmlFor="uf-factor" help="1 satuan ini = N satuan dasar (mis. Kwintal = 100, Lusin = 12)">
        <NumInput id="uf-factor" value={data.conversionFactor} onChange={(v) => set('conversionFactor', v)} placeholder="1" />
      </FormField>
      <FormField label="Status" htmlFor="uf-active">
        <BooleanRadio id="uf-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpUnitsPage() {
  return (
    <SimpleMasterPage<ErpUnit, UnitForm>
      title="Satuan"
      code="UOM"
      entityLabel="satuan"
      storageKey="units"
      auditEntityName="ErpUnit"
      list={listUnits}
      create={createUnit}
      update={updateUnit}
      remove={deleteUnit}
      bulkStatus={bulkUpdateErpUnitStatus}
      bulkDelete={bulkDeleteErpUnits}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateUnit}
    />
  );
}
