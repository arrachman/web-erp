'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listCostCenters, createCostCenter, updateCostCenter, deleteCostCenter,
  bulkUpdateCostCenterStatus, bulkDeleteCostCenters,
  type ErpCostCenter, type CreateCostCenterPayload,
} from '@/lib/api/cost-centers';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface CostCenterForm {
  code: string;
  name: string;
  isActive: boolean;
}

const defaultForm = (): CostCenterForm => ({ code: '', name: '', isActive: true });
const fromRecord = (r: ErpCostCenter): CostCenterForm => ({ code: r.code, name: r.name, isActive: r.isActive });
const toPayload = (f: CostCenterForm): CreateCostCenterPayload => ({ code: f.code, name: f.name, isActive: f.isActive });

const validateCostCenter = (form: CostCenterForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function CostCenterFormFields({ data, onChange, errors = {} }: { data: CostCenterForm; onChange: (d: CostCenterForm) => void; errors?: FormErrors<CostCenterForm> }) {
  const set = (k: keyof CostCenterForm, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="cc-code" required error={errors.code}>
        <Input id="cc-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="CC-OPS" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="cc-name" required error={errors.name}>
        <Input id="cc-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Operations Cost Center" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Status" htmlFor="cc-active">
        <BooleanRadio id="cc-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpCostCentersPage() {
  return (
    <SimpleMasterPage<ErpCostCenter, CostCenterForm>
      title="Cost Center" code="CC" entityLabel="cost center"
      storageKey="cost-centers" auditEntityName="ErpCostCenter"
      list={listCostCenters} create={createCostCenter} update={updateCostCenter} remove={deleteCostCenter}
      bulkStatus={bulkUpdateCostCenterStatus} bulkDelete={bulkDeleteCostCenters}
      defaultForm={defaultForm} fromRecord={fromRecord} toPayload={toPayload}
      FormFields={CostCenterFormFields}
      validate={validateCostCenter}
    />
  );
}
