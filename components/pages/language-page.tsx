'use client';

/**
 * Administrator — Languages page.
 * CRUD for sys_languages via SimpleMasterPage organism.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listLanguages,
  createErpLanguage,
  updateErpLanguage,
  deleteErpLanguage,
  bulkUpdateErpLanguageStatus,
  bulkDeleteErpLanguages,
  type ErpLanguage,
  type CreateErpLanguagePayload,
} from '@/lib/api/languages';
import { validateForm, type FormErrors } from '@/lib/form-validation';

// ─── Form types ───────────────────────────────────────────────────────────────

interface LanguageForm {
  code: string;
  name: string;
  nativeName: string;
  isRtl: boolean;
  isDefault: boolean;
  isActive: boolean;
}

const defaultForm = (): LanguageForm => ({
  code: '',
  name: '',
  nativeName: '',
  isRtl: false,
  isDefault: false,
  isActive: true,
});

const fromRecord = (r: ErpLanguage): LanguageForm => ({
  code: r.code,
  name: r.name,
  nativeName: r.nativeName,
  isRtl: r.isRtl,
  isDefault: r.isDefault,
  isActive: r.isActive,
});

const toPayload = (f: LanguageForm): CreateErpLanguagePayload => ({
  code: f.code,
  name: f.name,
  nativeName: f.nativeName,
  isRtl: f.isRtl,
  isDefault: f.isDefault,
  isActive: f.isActive,
});

const validateLanguage = (form: LanguageForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'nativeName', label: 'Nama Asli', required: true },
  ]);

// ─── Extra columns ────────────────────────────────────────────────────────────

const extraColumns = [
  {
    key: 'nativeName',
    label: 'Nama Asli',
    render: (r: ErpLanguage) => r.nativeName,
  },
  {
    key: 'isRtl',
    label: 'RTL',
    render: (r: ErpLanguage) => (
      <Badge variant={r.isRtl ? 'info' : 'default'}>{r.isRtl ? 'RTL' : 'LTR'}</Badge>
    ),
  },
  {
    key: 'isDefault',
    label: 'Default',
    render: (r: ErpLanguage) =>
      r.isDefault ? <Badge variant="success">Default</Badge> : null,
  },
];

// ─── Form fields ──────────────────────────────────────────────────────────────

function FormFields({
  data,
  onChange,
  errors = {},
}: {
  data: LanguageForm;
  onChange: (d: LanguageForm) => void;
  errors?: FormErrors<LanguageForm>;
}) {
  const set = (k: keyof LanguageForm, v: string | boolean) =>
    onChange({ ...data, [k]: v });

  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="lf-code" required error={errors.code}>
        <Input
          id="lf-code"
          value={data.code}
          onChange={(e) => set('code', e.target.value)}
          placeholder="id"
          aria-invalid={!!errors.code}
        />
      </FormField>
      <FormField label="Nama" htmlFor="lf-name" required error={errors.name}>
        <Input
          id="lf-name"
          value={data.name}
          onChange={(e) => set('name', e.target.value)}
          placeholder="Indonesian"
          aria-invalid={!!errors.name}
        />
      </FormField>
      <FormField label="Nama Asli" htmlFor="lf-native" required error={errors.nativeName}>
        <Input
          id="lf-native"
          value={data.nativeName}
          onChange={(e) => set('nativeName', e.target.value)}
          placeholder="Bahasa Indonesia"
          aria-invalid={!!errors.nativeName}
        />
      </FormField>
      <FormField label="Arah Teks" htmlFor="lf-rtl">
        <BooleanRadio
          id="lf-rtl"
          value={data.isRtl}
          onValueChange={(v) => set('isRtl', v)}
          trueLabel="RTL"
          falseLabel="LTR"
        />
      </FormField>
      <FormField label="Default" htmlFor="lf-default">
        <BooleanRadio
          id="lf-default"
          value={data.isDefault}
          onValueChange={(v) => set('isDefault', v)}
          trueLabel="Ya"
          falseLabel="Tidak"
        />
      </FormField>
      <FormField label="Status" htmlFor="lf-active">
        <BooleanRadio
          id="lf-active"
          value={data.isActive}
          onValueChange={(v) => set('isActive', v)}
        />
      </FormField>
    </div>
  );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpLanguagePage() {
  return (
    <SimpleMasterPage<ErpLanguage, LanguageForm>
      title="Bahasa"
      code="LANG"
      entityLabel="bahasa"
      storageKey="languages"
      auditEntityName="ErpLanguage"
      list={listLanguages}
      create={createErpLanguage}
      update={updateErpLanguage}
      remove={deleteErpLanguage}
      bulkStatus={bulkUpdateErpLanguageStatus}
      bulkDelete={bulkDeleteErpLanguages}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateLanguage}
      extraColumns={extraColumns}
    />
  );
}
