'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import { loadPostableAccountOptionsCoded } from '@/components/pages/items-form-lookups';
import {
  listOtherCosts, createErpOtherCost, updateErpOtherCost, deleteErpOtherCost,
  bulkUpdateErpOtherCostStatus, bulkDeleteErpOtherCosts,
  type ErpOtherCost, type CreateErpOtherCostPayload, type OtherCostAccountSummary,
} from '@/lib/api/other-costs';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  debitAccountId: string;
  debitAccountLabel: string;
  creditAccountId: string;
  creditAccountLabel: string;
  isHPP: boolean;
  isActive: boolean;
}

const accountLabel = (account?: OtherCostAccountSummary | null) => (
  account ? `${account.code} - ${account.name}` : ''
);

const displayAccount = (account?: OtherCostAccountSummary | null) => (
  account ? `${account.code} — ${account.name}` : '—'
);

const optionLabel = (opt: { label: string; code?: string }) => (
  `${opt.code ? `${opt.code} - ` : ''}${opt.label}`
);

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  debitAccountId: '',
  debitAccountLabel: '',
  creditAccountId: '',
  creditAccountLabel: '',
  isHPP: false,
  isActive: true,
});

const fromRecord = (r: ErpOtherCost): FormData => ({
  code: r.code,
  name: r.name,
  debitAccountId: r.isHPP ? '' : r.debitAccountId ?? '',
  debitAccountLabel: r.isHPP ? '' : accountLabel(r.debitAccount),
  creditAccountId: r.creditAccountId ?? '',
  creditAccountLabel: accountLabel(r.creditAccount),
  isHPP: r.isHPP,
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpOtherCostPayload => ({
  code: f.code,
  name: f.name,
  debitAccountId: f.isHPP ? null : f.debitAccountId || null,
  creditAccountId: f.creditAccountId || null,
  isHPP: f.isHPP,
  isActive: f.isActive,
});

const validateOtherCost = (form: FormData) => validateForm(form, [
  { field: 'code', label: 'Kode', required: true },
  { field: 'name', label: 'Nama', required: true },
  {
    field: 'debitAccountId',
    label: 'Akun Debit',
    validate: (value, data) => (!data.isHPP && !value ? 'Akun Debit wajib diisi' : undefined),
  },
  { field: 'creditAccountId', label: 'Akun Kredit', required: true },
]);

function FormFields({
  data,
  onChange,
  errors = {},
}: {
  data: FormData;
  onChange: (d: FormData) => void;
  errors?: FormErrors<FormData>;
}) {
  const set = (patch: Partial<FormData>) => onChange({ ...data, ...patch });
  const setIsHPP = (isHPP: boolean) => set({
    isHPP,
    debitAccountId: isHPP ? '' : data.debitAccountId,
    debitAccountLabel: isHPP ? '' : data.debitAccountLabel,
  });

  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="oc-code" required error={errors.code}>
        <Input id="oc-code" value={data.code} onChange={(e) => set({ code: e.target.value })} placeholder="OC-01" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="oc-name" required error={errors.name}>
        <Input id="oc-name" value={data.name} onChange={(e) => set({ name: e.target.value })} placeholder="Biaya pengiriman" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Alokasi proporsional ke barang (HPP)" htmlFor="oc-is-hpp">
        <BooleanRadio id="oc-is-hpp" value={data.isHPP} onValueChange={setIsHPP} trueLabel="Ya" falseLabel="Tidak" />
      </FormField>
      <FormField label="Akun Debit" htmlFor="oc-debit-account" required={!data.isHPP} error={errors.debitAccountId}>
        <SearchSelect
          id="oc-debit-account"
          value={data.debitAccountId}
          onValueChange={(value) => set({ debitAccountId: value })}
          onPick={(opt) => set({ debitAccountId: opt.value, debitAccountLabel: optionLabel(opt) })}
          placeholder={data.isHPP ? 'Dialokasikan proporsional ke barang' : 'Pilih akun debit…'}
          loadOptions={loadPostableAccountOptionsCoded}
          initialLabel={data.debitAccountLabel}
          title="Akun Debit"
          disabled={data.isHPP}
          error={!!errors.debitAccountId}
        />
      </FormField>
      <FormField label="Akun Kredit" htmlFor="oc-credit-account" required error={errors.creditAccountId}>
        <SearchSelect
          id="oc-credit-account"
          value={data.creditAccountId}
          onValueChange={(value) => set({ creditAccountId: value })}
          onPick={(opt) => set({ creditAccountId: opt.value, creditAccountLabel: optionLabel(opt) })}
          placeholder="Pilih akun kredit…"
          loadOptions={loadPostableAccountOptionsCoded}
          initialLabel={data.creditAccountLabel}
          title="Akun Kredit"
          error={!!errors.creditAccountId}
        />
      </FormField>
      <FormField label="Status" htmlFor="oc-active">
        <BooleanRadio id="oc-active" value={data.isActive} onValueChange={(v) => set({ isActive: v })} />
      </FormField>
    </div>
  );
}

export function ErpOtherCostsPage() {
  return (
    <SimpleMasterPage<ErpOtherCost, FormData>
      title="Other Cost"
      code="OCT"
      entityLabel="other cost"
      storageKey="other-costs"
      auditEntityName="ErpOtherCost"
      list={listOtherCosts}
      create={createErpOtherCost}
      update={updateErpOtherCost}
      remove={deleteErpOtherCost}
      bulkStatus={bulkUpdateErpOtherCostStatus}
      bulkDelete={bulkDeleteErpOtherCosts}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateOtherCost}
      modalSize="lg"
      extraColumns={[
        {
          key: 'debitAccountId',
          label: 'Akun Debit',
          render: (row) => row.isHPP ? <Badge variant="success">Proporsional</Badge> : displayAccount(row.debitAccount),
        },
        {
          key: 'creditAccountId',
          label: 'Akun Kredit',
          render: (row) => displayAccount(row.creditAccount),
        },
        {
          key: 'isHPP',
          label: 'HPP',
          render: (row) => row.isHPP ? <Badge variant="success">HPP</Badge> : '—',
        },
      ]}
    />
  );
}
