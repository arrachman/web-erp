'use client';

/**
 * F3 Master Data — Item Category page.
 * Uses SimpleMasterPage organism for full-feature CRUD.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import { loadAccountOptionsCoded } from '@/components/pages/items-form-lookups';
import {
  listItemCategories, createItemCategory, updateItemCategory, deleteItemCategory,
  bulkUpdateErpItemCategoryStatus, bulkDeleteErpItemCategories,
  type ErpItemCategory, type CreateItemCategoryPayload, type ItemCategoryAccountIds,
} from '@/lib/api/item-categories';
import { validateForm, type FormErrors } from '@/lib/form-validation';

type AccountFieldKey = keyof ItemCategoryAccountIds;

/** 8 GL account pickers — order mirrors legacy MyERP+ layout (left/right columns). */
const ACCOUNT_FIELDS: { key: AccountFieldKey; label: string }[] = [
  { key: 'inventoryAccountId', label: 'Persediaan' },
  { key: 'salesAccountId', label: 'Penjualan' },
  { key: 'salesReturnAccountId', label: 'Retur Penjualan' },
  { key: 'salesDiscountAccountId', label: 'Diskon Penjualan' },
  { key: 'cogsAccountId', label: 'HPP' },
  { key: 'purchaseReturnAccountId', label: 'Retur Pembelian' },
  { key: 'purchaseDiscountAccountId', label: 'Diskon Pembelian' },
  { key: 'consignmentAccountId', label: 'Konsinyasi' },
];

const ACCOUNT_RELATIONS: { idKey: AccountFieldKey; relKey: keyof ErpItemCategory }[] = [
  { idKey: 'inventoryAccountId', relKey: 'inventoryAccount' },
  { idKey: 'salesAccountId', relKey: 'salesAccount' },
  { idKey: 'salesReturnAccountId', relKey: 'salesReturnAccount' },
  { idKey: 'salesDiscountAccountId', relKey: 'salesDiscountAccount' },
  { idKey: 'cogsAccountId', relKey: 'cogsAccount' },
  { idKey: 'purchaseReturnAccountId', relKey: 'purchaseReturnAccount' },
  { idKey: 'purchaseDiscountAccountId', relKey: 'purchaseDiscountAccount' },
  { idKey: 'consignmentAccountId', relKey: 'consignmentAccount' },
];

type AccountFormState = Record<AccountFieldKey, string> & Record<`${AccountFieldKey}Label`, string>;

interface CatForm extends AccountFormState {
  id?: string;
  code: string;
  name: string;
  isActive: boolean;
  parentId: string;
  parentLabel?: string;
}

const emptyAccounts = (): AccountFormState => {
  const acc = {} as AccountFormState;
  for (const f of ACCOUNT_FIELDS) {
    acc[f.key] = '';
    acc[`${f.key}Label`] = '';
  }
  return acc;
};

const defaultForm = (): CatForm => ({
  code: '', name: '', isActive: true, parentId: '', parentLabel: '', ...emptyAccounts(),
});

const fromRecord = (r: ErpItemCategory): CatForm => {
  const acc = emptyAccounts();
  for (const { idKey, relKey } of ACCOUNT_RELATIONS) {
    const rel = r[relKey] as { code: string; name: string } | null | undefined;
    acc[idKey] = (r[idKey] as string | null | undefined) ?? '';
    acc[`${idKey}Label`] = rel ? `${rel.code} - ${rel.name}` : '';
  }
  return {
    id: r.id,
    code: r.code,
    name: r.name,
    isActive: r.isActive,
    parentId: r.parentId ?? '',
    parentLabel: r.parent?.name ?? '',
    ...acc,
  };
};

const toPayload = (f: CatForm): CreateItemCategoryPayload => {
  const acc: ItemCategoryAccountIds = {};
  for (const { key } of ACCOUNT_FIELDS) {
    acc[key] = f[key] || null;
  }
  return {
    code: f.code,
    name: f.name,
    isActive: f.isActive,
    parentId: f.parentId || null,
    ...acc,
  };
};

const validateCategory = (form: CatForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {} }: { data: CatForm; onChange: (d: CatForm) => void; errors?: FormErrors<CatForm> }) {
  const set = (k: keyof CatForm, v: string | boolean) => onChange({ ...data, [k]: v });

  const loadParentOptions = React.useCallback(
    async (search: string, page: number, limit: number) => {
      const res = await listItemCategories({ search: search || undefined, page, limit, isActive: true });
      return {
        data: res.data
          .filter((c) => c.id !== data.id)
          .map((c) => ({ value: c.id, label: c.name, code: c.code })),
        total: res.meta.total,
      };
    },
    [data.id],
  );

  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="cf-code" required error={errors.code}>
        <Input id="cf-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="RAW-MAT" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="cf-name" required error={errors.name}>
        <Input id="cf-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Bahan Baku" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Status" htmlFor="cf-active">
        <BooleanRadio id="cf-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
      <FormField label="Parent" htmlFor="cf-parent">
        <SearchSelect
          id="cf-parent"
          placeholder="Cari kategori parent…"
          value={data.parentId}
          onValueChange={(v) => set('parentId', v)}
          loadOptions={loadParentOptions}
          initialLabel={data.parentLabel}
          title="Kategori Parent"
        />
      </FormField>

      <div className="mt-2 mb-1 text-xs font-semibold uppercase tracking-wide text-[var(--text-muted)]">
        Akun GL
      </div>
      <div className="grid grid-cols-2 gap-x-4">
        {ACCOUNT_FIELDS.map(({ key, label }) => (
          <FormField key={key} label={label} htmlFor={`cf-${key}`}>
            <SearchSelect
              id={`cf-${key}`}
              placeholder="Pilih akun…"
              value={data[key]}
              onValueChange={(v) => set(key, v)}
              loadOptions={loadAccountOptionsCoded}
              initialLabel={data[`${key}Label`]}
              title={`Akun ${label}`}
            />
          </FormField>
        ))}
      </div>
    </div>
  );
}

export function ErpItemCategoriesPage() {
  return (
    <SimpleMasterPage<ErpItemCategory, CatForm>
      title="Kategori Item"
      code="ICAT"
      entityLabel="kategori"
      storageKey="item-categories"
      auditEntityName="ErpItemCategory"
      list={listItemCategories}
      create={createItemCategory}
      update={updateItemCategory}
      remove={deleteItemCategory}
      bulkStatus={bulkUpdateErpItemCategoryStatus}
      bulkDelete={bulkDeleteErpItemCategories}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateCategory}
      extraColumns={[{ key: 'parent', label: 'Parent', render: (row) => row.parent?.name ?? '—' }]}
    />
  );
}
