'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SimpleMasterPage, type ExtraFilterDef } from '@/components/organisms/simple-master-page';
import {
  listPartnerTypes,
  createPartnerType,
  updatePartnerType,
  deletePartnerType,
  bulkUpdateErpPartnerTypeStatus,
  bulkDeleteErpPartnerTypes,
  PARTNER_TYPE_KINDS,
  PARTNER_TYPE_KIND_LABEL,
  derivePartnerTypeKindFromCode,
  type ErpPartnerType,
  type CreatePartnerTypePayload,
} from '@/lib/api/partner-types';
import { validateForm, type FormErrors } from '@/lib/form-validation';

const PROTECTED_CODES = ['CUST', 'SUP', 'SLS'];

interface FormData {
  code: string;
  name: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  isActive: true,
});

const fromRecord = (r: ErpPartnerType): FormData => ({
  code: r.code,
  name: r.name,
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreatePartnerTypePayload => ({
  code: f.code,
  name: f.name,
  kind: derivePartnerTypeKindFromCode(f.code),
  isActive: f.isActive,
});

const validatePartnerType = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
  ]);

function FormFields({ data, onChange, errors = {}, isProtected = false }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData>; isProtected?: boolean }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="pt-code" required error={errors.code}>
        <Input
          id="pt-code"
          value={data.code}
          onChange={(e) => set('code', e.target.value)}
          placeholder="CUST"
          aria-invalid={!!errors.code}
          disabled={isProtected}
          readOnly={isProtected}
          title={isProtected ? 'Kode tipe yang terkunci tidak bisa diubah' : undefined}
        />
      </FormField>
      {isProtected && (
        <p className="mb-2 text-[12px] text-muted-foreground bg-amber-50 border border-amber-200 rounded px-2 py-1">
          🔒 Tipe ini terkunci — kode tidak bisa diubah dan tidak bisa dihapus karena digunakan oleh alur transaksi sistem.
        </p>
      )}
      <FormField label="Nama" htmlFor="pt-name" required error={errors.name}>
        <Input id="pt-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Customer" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Status" htmlFor="pt-active">
        <BooleanRadio id="pt-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const kindFilters: ExtraFilterDef[] = [
  {
    key: 'kind',
    label: 'Kind',
    options: PARTNER_TYPE_KINDS.map((kind) => ({
      label: PARTNER_TYPE_KIND_LABEL[kind],
      value: kind,
    })),
  },
];

export function ErpPartnerTypesPage() {
  // Build FormFields component that knows if the current record is protected.
  // We wrap it in useMemo so it's stable across renders but reflects record changes.
  const [editingRecord, setEditingRecord] = React.useState<ErpPartnerType | null>(null);

  const BoundFormFields = React.useMemo(() => {
    const isProtected = editingRecord ? PROTECTED_CODES.includes(editingRecord.code) : false;
    return function PartnerTypeFormFields(
      props: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }
    ) {
      return <FormFields {...props} isProtected={isProtected} />;
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [editingRecord?.code]);

  return (
    <SimpleMasterPage<ErpPartnerType, FormData>
      title="Tipe Partner"
      code="PTYP"
      entityLabel="tipe partner"
      storageKey="partner-types"
      auditEntityName="ErpPartnerType"
      list={listPartnerTypes}
      create={createPartnerType}
      update={updatePartnerType}
      remove={deletePartnerType}
      bulkStatus={bulkUpdateErpPartnerTypeStatus}
      bulkDelete={bulkDeleteErpPartnerTypes}
      defaultForm={() => { setEditingRecord(null); return defaultForm(); }}
      fromRecord={(r) => { setEditingRecord(r); return fromRecord(r); }}
      toPayload={toPayload}
      FormFields={BoundFormFields}
      validate={validatePartnerType}
      extraColumns={[
        { key: 'kind', label: 'Jenis', render: (row) => PARTNER_TYPE_KIND_LABEL[row.kind] },
        {
          key: 'protected',
          label: '',
          render: (row) => PROTECTED_CODES.includes(row.code)
            ? <span title="Terkunci — kode tidak bisa diubah, tidak bisa dihapus" className="text-amber-600 text-[11px] font-medium">🔒 Terkunci</span>
            : null,
        },
      ]}
      extraFilters={kindFilters}
    />
  );
}
