'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import type { ErpBranch } from '@/lib/api/branches';
import type { CreateBranchPayload, UpdateBranchPayload } from '@/lib/api/branches';
import { validateForm, type FormErrors } from '@/lib/form-validation';

export const validateBranch = (form: BranchForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

export interface BranchForm {
  code: string;
  name: string;
  addressLine1: string;
  city: string;
  phone: string;
  isActive: boolean;
}

export const defaultBranchForm = (): BranchForm => ({
  code: '',
  name: '',
  addressLine1: '',
  city: '',
  phone: '',
  isActive: true,
});

export function branchFromRecord(b: ErpBranch): BranchForm {
  return {
    code: b.code,
    name: b.name,
    addressLine1: b.addressLine1 ?? '',
    city: b.city ?? '',
    phone: b.phone ?? '',
    isActive: b.isActive,
  };
}

export function branchToPayload(f: BranchForm): CreateBranchPayload & UpdateBranchPayload {
  return {
    code: f.code,
    name: f.name,
    addressLine1: f.addressLine1 || undefined,
    city: f.city || undefined,
    phone: f.phone || undefined,
    isActive: f.isActive,
  };
}

export function BranchFormFields({
  data,
  onChange,
  errors = {},
}: {
  data: BranchForm;
  onChange: (d: BranchForm) => void;
  errors?: FormErrors<BranchForm>;
}) {
  const set = (k: keyof BranchForm, v: string | boolean) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="bf-code" required error={errors.code}>
        <Input id="bf-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="HQ" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="bf-name" required error={errors.name}>
        <Input id="bf-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Head Quarter Jakarta" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Alamat" htmlFor="bf-addr">
        <Input id="bf-addr" value={data.addressLine1} onChange={(e) => set('addressLine1', e.target.value)} placeholder="Jl. Sudirman No. 1" />
      </FormField>
      <FormField label="Kota" htmlFor="bf-city">
        <Input id="bf-city" value={data.city} onChange={(e) => set('city', e.target.value)} placeholder="Jakarta" />
      </FormField>
      <FormField label="Telepon" htmlFor="bf-phone">
        <Input id="bf-phone" value={data.phone} onChange={(e) => set('phone', e.target.value)} placeholder="021-5551234" />
      </FormField>
      <FormField label="Status" htmlFor="bf-active">
        <BooleanRadio id="bf-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}
