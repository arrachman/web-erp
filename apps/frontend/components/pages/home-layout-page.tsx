'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio, RadioGroup } from '@/components/ui/radio-group';
import { NumInput } from '@/components/molecules/num-input';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listHomeWidgets,
  createHomeWidget,
  updateHomeWidget,
  deleteHomeWidget,
  bulkUpdateErpHomeWidgetStatus,
  bulkDeleteErpHomeWidgets,
  type ErpHomeWidget,
  type HomeWidgetForm,
  type HomeWidgetPayload,
} from '@/lib/api/home-widgets';
import { validateForm, type FormErrors } from '@/lib/form-validation';

const COL_SPAN_OPTIONS = [
  { value: '1', label: '1' },
  { value: '2', label: '2' },
  { value: '3', label: '3' },
  { value: '4', label: '4' },
] as const;

const defaultForm = (): HomeWidgetForm => ({
  widgetKey: '',
  title: '',
  description: '',
  enabled: true,
  sortOrder: 0,
  colSpan: 1,
});

const fromRecord = (r: ErpHomeWidget): HomeWidgetForm => ({
  id: r.id,
  widgetKey: r.widgetKey,
  title: r.title,
  description: r.description ?? '',
  enabled: r.enabled,
  sortOrder: r.sortOrder,
  colSpan: r.colSpan,
});

const toPayload = ({ id: _id, ...f }: HomeWidgetForm): HomeWidgetPayload => ({
  widgetKey: f.widgetKey,
  title: f.title,
  description: f.description || undefined,
  enabled: f.enabled,
  sortOrder: f.sortOrder,
  colSpan: f.colSpan,
});

const validateHomeWidget = (form: HomeWidgetForm) =>
  validateForm(form, [
    { field: 'widgetKey', label: 'Kunci Widget', required: true },
    { field: 'title', label: 'Judul', required: true },
  ]);

function FormFields({
  data,
  onChange,
  errors = {},
}: {
  data: HomeWidgetForm;
  onChange: (d: HomeWidgetForm) => void;
  errors?: FormErrors<HomeWidgetForm>;
}) {
  const set = (k: keyof HomeWidgetForm, v: string | boolean | number) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kunci Widget" htmlFor="hw-key" required error={errors.widgetKey}>
        <Input
          id="hw-key"
          value={data.widgetKey}
          onChange={(e) => set('widgetKey', e.target.value)}
          placeholder="sales-summary"
          aria-invalid={!!errors.widgetKey}
        />
      </FormField>
      <FormField label="Judul" htmlFor="hw-title" required error={errors.title}>
        <Input
          id="hw-title"
          value={data.title}
          onChange={(e) => set('title', e.target.value)}
          placeholder="Ringkasan Penjualan"
          aria-invalid={!!errors.title}
        />
      </FormField>
      <FormField label="Deskripsi" htmlFor="hw-desc">
        <Input
          id="hw-desc"
          value={data.description}
          onChange={(e) => set('description', e.target.value)}
        />
      </FormField>
      <FormField label="Aktif" htmlFor="hw-enabled">
        <BooleanRadio
          id="hw-enabled"
          value={data.enabled}
          onValueChange={(v) => set('enabled', v)}
          trueLabel="Ya"
          falseLabel="Tidak"
        />
      </FormField>
      <FormField label="Urutan" htmlFor="hw-sort">
        <NumInput
          id="hw-sort"
          value={String(data.sortOrder)}
          onChange={(raw) => set('sortOrder', Number(raw || 0))}
          decimals={0}
        />
      </FormField>
      <FormField label="Lebar Kolom" htmlFor="hw-colspan">
        <RadioGroup
          id="hw-colspan"
          value={String(data.colSpan)}
          onValueChange={(v) => set('colSpan', Number(v))}
          options={COL_SPAN_OPTIONS}
        />
      </FormField>
    </div>
  );
}

export function ErpHomeLayoutPage() {
  return (
    <SimpleMasterPage<ErpHomeWidget, HomeWidgetForm>
      title="Pengaturan Beranda"
      code="HOME"
      entityLabel="widget"
      storageKey="home-widgets"
      auditEntityName="ErpHomeWidget"
      list={listHomeWidgets}
      create={createHomeWidget}
      update={updateHomeWidget}
      remove={deleteHomeWidget}
      bulkStatus={bulkUpdateErpHomeWidgetStatus}
      bulkDelete={bulkDeleteErpHomeWidgets}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateHomeWidget}
      defaultSortBy="sortOrder"
      defaultSortDir="asc"
      extraColumns={[
        {
          key: 'sortOrder',
          label: 'Urutan',
          sortable: true,
          render: (row) => <span className="block text-right tabular-nums">{row.sortOrder}</span>,
        },
        {
          key: 'colSpan',
          label: 'Kolom',
          render: (row) => <span className="block text-right tabular-nums">{row.colSpan}</span>,
        },
      ]}
    />
  );
}
