'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { Badge } from '@/components/ui/badge';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listBankAccounts,
  createBankAccount,
  updateBankAccount,
  deleteBankAccount,
  bulkUpdateErpBankAccountStatus,
  bulkDeleteErpBankAccounts,
  type ErpBankAccount,
  type CreateBankAccountPayload,
} from '@/lib/api/bank-accounts';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface BankAccountForm {
  id?: string;
  code: string;
  name: string;
  bankName: string;
  accountNumber: string;
  accountHolder: string;
  branch: string;
  swiftCode: string;
  isPrimary: boolean;
  notes: string;
  isActive: boolean;
}

const defaultForm = (): BankAccountForm => ({
  code: '',
  name: '',
  bankName: '',
  accountNumber: '',
  accountHolder: '',
  branch: '',
  swiftCode: '',
  isPrimary: false,
  notes: '',
  isActive: true,
});

const fromRecord = (r: ErpBankAccount): BankAccountForm => ({
  id: r.id,
  code: r.code,
  name: r.name,
  bankName: r.bankName,
  accountNumber: r.accountNumber,
  accountHolder: r.accountHolder,
  branch: r.branch ?? '',
  swiftCode: r.swiftCode ?? '',
  isPrimary: r.isPrimary,
  notes: r.notes ?? '',
  isActive: r.isActive,
});

const toPayload = ({ id: _id, ...f }: BankAccountForm): CreateBankAccountPayload => ({
  code: f.code,
  name: f.name,
  bankName: f.bankName,
  accountNumber: f.accountNumber,
  accountHolder: f.accountHolder,
  branch: f.branch || undefined,
  swiftCode: f.swiftCode || undefined,
  isPrimary: f.isPrimary,
  notes: f.notes || undefined,
  isActive: f.isActive,
});

const validateBankAccount = (form: BankAccountForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'bankName', label: 'Bank', required: true },
    { field: 'accountNumber', label: 'No. Rekening', required: true },
    { field: 'accountHolder', label: 'Atas Nama', required: true },
  ]);

function FormFields({
  data,
  onChange,
  errors = {},
}: {
  data: BankAccountForm;
  onChange: (d: BankAccountForm) => void;
  errors?: FormErrors<BankAccountForm>;
}) {
  const set = (k: keyof BankAccountForm, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="bnk-code" required error={errors.code}>
        <Input id="bnk-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="BNK-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="bnk-name" required error={errors.name}>
        <Input id="bnk-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Rekening Operasional" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Bank" htmlFor="bnk-bank" required error={errors.bankName}>
        <Input id="bnk-bank" value={data.bankName} onChange={(e) => set('bankName', e.target.value)} placeholder="Bank Central Asia" aria-invalid={!!errors.bankName} />
      </FormField>
      <FormField label="No. Rekening" htmlFor="bnk-acctno" required error={errors.accountNumber}>
        <Input id="bnk-acctno" value={data.accountNumber} onChange={(e) => set('accountNumber', e.target.value)} placeholder="1234567890" aria-invalid={!!errors.accountNumber} />
      </FormField>
      <FormField label="Atas Nama" htmlFor="bnk-holder" required error={errors.accountHolder}>
        <Input id="bnk-holder" value={data.accountHolder} onChange={(e) => set('accountHolder', e.target.value)} placeholder="PT Sentient Factory" aria-invalid={!!errors.accountHolder} />
      </FormField>
      <FormField label="Cabang Bank" htmlFor="bnk-branch">
        <Input id="bnk-branch" value={data.branch} onChange={(e) => set('branch', e.target.value)} placeholder="KCP Sudirman" />
      </FormField>
      <FormField label="SWIFT" htmlFor="bnk-swift">
        <Input id="bnk-swift" value={data.swiftCode} onChange={(e) => set('swiftCode', e.target.value)} placeholder="CENAIDJA" />
      </FormField>
      <FormField label="Rekening Utama" htmlFor="bnk-primary">
        <BooleanRadio id="bnk-primary" value={data.isPrimary} onValueChange={(v) => set('isPrimary', v)} trueLabel="Ya" falseLabel="Tidak" />
      </FormField>
      <FormField label="Catatan" htmlFor="bnk-notes">
        <Input id="bnk-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} placeholder="Catatan tambahan" />
      </FormField>
      <FormField label="Status" htmlFor="bnk-active">
        <BooleanRadio id="bnk-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

export function ErpBankAccountsPage() {
  return (
    <SimpleMasterPage<ErpBankAccount, BankAccountForm>
      title="Rekening Bank Perusahaan"
      code="BNK"
      entityLabel="rekening bank"
      storageKey="bank-accounts"
      auditEntityName="ErpBankAccount"
      list={listBankAccounts}
      create={createBankAccount}
      update={updateBankAccount}
      remove={deleteBankAccount}
      bulkStatus={bulkUpdateErpBankAccountStatus}
      bulkDelete={bulkDeleteErpBankAccounts}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateBankAccount}
      extraColumns={[
        { key: 'bankName', label: 'Bank', render: (row) => row.bankName },
        { key: 'accountNumber', label: 'No. Rekening', render: (row) => row.accountNumber },
        {
          key: 'isPrimary',
          label: 'Utama',
          render: (row) =>
            row.isPrimary ? <Badge variant="success">Utama</Badge> : '—',
        },
      ]}
    />
  );
}
