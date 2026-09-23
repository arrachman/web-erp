'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listPartnerSubCategories, createErpPartnerSubCategory, updateErpPartnerSubCategory, deleteErpPartnerSubCategory,
  bulkUpdateErpPartnerSubCategoryStatus, bulkDeleteErpPartnerSubCategories,
  type ErpPartnerSubCategory, type CreateErpPartnerSubCategoryPayload, type PartnerSubCategoryType,
} from '@/lib/api/partner-sub-categories';
import type { PaginationParams } from '@/lib/api/types';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  type: PartnerSubCategoryType;
  isActive: boolean;
}

const PAGE_META: Record<PartnerSubCategoryType, { title: string; code: string; entityLabel: string; storageKey: string; placeholder: string }> = {
  CUSTOMER: { title: 'Kategori Customer', code: 'CSCAT', entityLabel: 'kategori customer', storageKey: 'customer-sub-categories', placeholder: 'CSCAT-001' },
  SUPPLIER: { title: 'Kategori Supplier', code: 'SPCAT', entityLabel: 'kategori supplier', storageKey: 'supplier-sub-categories', placeholder: 'SPCAT-001' },
  SALESMAN: { title: 'Kategori Salesman', code: 'SLCAT', entityLabel: 'kategori salesman', storageKey: 'salesman-categories', placeholder: 'SLCAT-001' },
};

interface Props {
  type?: PartnerSubCategoryType;
}

export function ErpPartnerSubCategoriesPage({ type = 'CUSTOMER' }: Props) {
  const meta = PAGE_META[type];

  const defaultForm = React.useCallback((): FormData => ({ code: '', name: '', type, isActive: true }), [type]);
  const fromRecord = (r: ErpPartnerSubCategory): FormData => ({ code: r.code, name: r.name, type: (r.type ?? type) as PartnerSubCategoryType, isActive: r.isActive });
  const toPayload = (f: FormData): CreateErpPartnerSubCategoryPayload => ({ code: f.code, name: f.name, type: f.type, isActive: f.isActive });

  const listFn = React.useCallback(
    (params: PaginationParams) => listPartnerSubCategories({ ...params, type }),
    [type],
  );

  const validateFn = (form: FormData) =>
    validateForm(form, [
      { field: 'code', label: 'Kode', required: true },
      { field: 'name', label: 'Nama', required: true },
    ]);

  function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
    const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
    return (
      <div className="p-4">
        <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
          <Input id="ef-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder={meta.placeholder} aria-invalid={!!errors.code} />
        </FormField>
        <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
          <Input id="ef-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder={meta.title} aria-invalid={!!errors.name} />
        </FormField>
        <FormField label="Status" htmlFor="ef-active">
          <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
        </FormField>
      </div>
    );
  }

  return (
    <SimpleMasterPage<ErpPartnerSubCategory, FormData>
      title={meta.title}
      code={meta.code}
      entityLabel={meta.entityLabel}
      storageKey={meta.storageKey}
      auditEntityName="ErpPartnerSubCategory"
      list={listFn}
      create={createErpPartnerSubCategory}
      update={updateErpPartnerSubCategory}
      remove={deleteErpPartnerSubCategory}
      bulkStatus={bulkUpdateErpPartnerSubCategoryStatus}
      bulkDelete={bulkDeleteErpPartnerSubCategories}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateFn}
    />
  );
}
