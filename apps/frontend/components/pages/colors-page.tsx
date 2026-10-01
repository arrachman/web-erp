'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listColors, createErpColor, updateErpColor, deleteErpColor,
  bulkUpdateErpColorStatus, bulkDeleteErpColors,
  type ErpColor, type CreateErpColorPayload,
} from '@/lib/api/colors';
import { validateForm, type FormErrors } from '@/lib/form-validation';

interface FormData {
  code: string;
  name: string;
  hexCode: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  hexCode: '',
  isActive: true,
});

const fromRecord = (r: ErpColor): FormData => ({
  code: r.code,
  name: r.name,
  hexCode: r.hexCode ?? '',
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpColorPayload => ({
  code: f.code,
  name: f.name,
  hexCode: f.hexCode || undefined,
  isActive: f.isActive,
});

const validateColor = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function ColorSwatch({ hex }: { hex: string }) {
  if (!hex) return null;
  return (
    <span
      style={{
        display: 'inline-block',
        width: 16,
        height: 16,
        borderRadius: 3,
        background: hex,
        border: '1px solid var(--border)',
        verticalAlign: 'middle',
        marginRight: 6,
        cursor: 'default',
        flexShrink: 0,
      }}
      title={hex}
    />
  );
}

function FormFields({
  data,
  onChange,
  errors = {},
}: {
  data: FormData;
  onChange: (d: FormData) => void;
  errors?: FormErrors<FormData>;
}) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="ef-code" required error={errors.code}>
        <Input
          id="ef-code"
          value={data.code}
          onChange={(e) => set('code', e.target.value)}
          placeholder="CLR-001"
          aria-invalid={!!errors.code}
        />
      </FormField>

      <FormField label="Nama" htmlFor="ef-name" required error={errors.name}>
        <Input
          id="ef-name"
          value={data.name}
          onChange={(e) => set('name', e.target.value)}
          placeholder="Merah"
          aria-invalid={!!errors.name}
        />
      </FormField>

      <FormField label="Kode Warna (Hex)" htmlFor="ef-hex">
        <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <input
            id="ef-hex-picker"
            type="color"
            value={data.hexCode || '#000000'}
            onChange={(e) => set('hexCode', e.target.value)}
            style={{
              width: 36,
              height: 32,
              padding: 2,
              border: '1px solid var(--border)',
              borderRadius: 4,
              cursor: 'pointer',
              background: 'transparent',
              flexShrink: 0,
            }}
            aria-label="Pilih warna"
          />
          <Input
            id="ef-hex"
            value={data.hexCode}
            onChange={(e) => set('hexCode', e.target.value)}
            placeholder="#FF0000"
            style={{ fontFamily: 'monospace' }}
          />
        </div>
      </FormField>

      <FormField label="Status" htmlFor="ef-active">
        <BooleanRadio id="ef-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const extraColumns = [
  {
    key: 'hexCode',
    label: 'Hex',
    render: (r: ErpColor) =>
      r.hexCode ? (
        <span style={{ display: 'flex', alignItems: 'center' }}>
          <ColorSwatch hex={r.hexCode} />
          <span style={{ fontFamily: 'monospace', fontSize: 'calc(12px * var(--font-scale, 1))' }}>
            {r.hexCode}
          </span>
        </span>
      ) : (
        <span className="muted">—</span>
      ),
  },
];

export function ErpColorsPage() {
  return (
    <SimpleMasterPage<ErpColor, FormData>
      title="Warna"
      code="CLR"
      entityLabel="warna"
      storageKey="colors"
      auditEntityName="ErpColor"
      list={listColors}
      create={createErpColor}
      update={updateErpColor}
      remove={deleteErpColor}
      bulkStatus={bulkUpdateErpColorStatus}
      bulkDelete={bulkDeleteErpColors}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateColor}
      extraColumns={extraColumns}
    />
  );
}
