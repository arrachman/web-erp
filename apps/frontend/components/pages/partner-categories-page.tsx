'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage, type ExtraFilterDef } from '@/components/organisms/simple-master-page';
import {
  listPartnerCategories,
  createPartnerCategory,
  updatePartnerCategory,
  deletePartnerCategory,
  bulkUpdateErpPartnerCategoryStatus,
  bulkDeleteErpPartnerCategories,
  PARTNER_CATEGORY_KINDS,
  type ErpPartnerCategory,
  type ErpPartnerCategoryKind,
  type CreatePartnerCategoryPayload,
} from '@/lib/api/partner-categories';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  kind: ErpPartnerCategoryKind;
  salesTier: string; // "1".."10" — tingkat Harga/Diskon Jual (hanya relevan untuk CUSTOMER)
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  kind: 'CUSTOMER',
  salesTier: '',
  isActive: true,
});

const fromRecord = (r: ErpPartnerCategory): FormData => ({
  code: r.code,
  name: r.name,
  kind: r.kind,
  salesTier: r.salesTier != null ? String(r.salesTier) : '',
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreatePartnerCategoryPayload => ({
  code: f.code,
  name: f.name,
  kind: f.kind,
  // Tier hanya untuk pelanggan; kategori lain kirim null.
  salesTier: f.kind === 'CUSTOMER' && f.salesTier.trim() !== '' ? Number(f.salesTier) : null,
  isActive: f.isActive,
});

const validatePartnerCategory = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    {
      field: 'salesTier',
      label: 'Tingkat Jual',
      validate: (value, f) => {
        if (f.kind !== 'CUSTOMER' || String(value ?? '').trim() === '') return undefined;
        const n = Number(value);
        return Number.isInteger(n) && n >= 1 && n <= 10 ? undefined : 'Tingkat jual harus 1–10';
      },
    },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="pc-code" required error={errors.code}>
        <Input id="pc-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="CUST-RETAIL" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="pc-name" required error={errors.name}>
        <Input id="pc-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Retail Customer" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Jenis" htmlFor="pc-kind" required>
        <Select value={data.kind} onValueChange={(v) => set('kind', v as ErpPartnerCategoryKind)}>
          <SelectTrigger id="pc-kind">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {PARTNER_CATEGORY_KINDS.map((k) => (
              <SelectItem key={k} value={k}>{k}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>
      {data.kind === 'CUSTOMER' && (
        <FormField label="Tingkat Jual" htmlFor="pc-tier" error={errors.salesTier} help="Tingkat Harga/Diskon Jual (1–10) yang dipakai pelanggan kategori ini">
          <Input
            id="pc-tier"
            type="number"
            min={1}
            max={10}
            value={data.salesTier}
            onChange={(e) => set('salesTier', e.target.value)}
            placeholder="mis. 1"
            aria-invalid={!!errors.salesTier}
          />
        </FormField>
      )}
      <FormField label="Status" htmlFor="pc-active">
        <BooleanRadio id="pc-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const kindFilters: ExtraFilterDef[] = [
  { key: 'kind', label: 'Kind', options: [
    { label: 'Customer', value: 'CUSTOMER' },
    { label: 'Supplier', value: 'SUPPLIER' },
    { label: 'Salesman', value: 'SALESMAN' },
  ]},
];

export function ErpPartnerCategoriesPage() {
  return (
    <SimpleMasterPage<ErpPartnerCategory, FormData>
      title="Kategori Partner"
      code="PCAT"
      entityLabel="kategori partner"
      storageKey="partner-categories"
      auditEntityName="ErpPartnerCategory"
      list={listPartnerCategories}
      create={createPartnerCategory}
      update={updatePartnerCategory}
      remove={deletePartnerCategory}
      bulkStatus={bulkUpdateErpPartnerCategoryStatus}
      bulkDelete={bulkDeleteErpPartnerCategories}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validatePartnerCategory}
      extraColumns={[
        { key: 'kind', label: 'Jenis', render: (row) => row.kind },
        { key: 'salesTier', label: 'Tingkat Jual', render: (row) => (row.salesTier != null ? String(row.salesTier) : '—') },
      ]}
      extraFilters={kindFilters}
    />
  );
}
