'use client';

/**
 * Master Data — Location page.
 * Lists md_locations; supports create, edit, delete (bulk-aware).
 * Branch FK rendered via SearchSelect.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { BooleanRadio } from '@/components/ui/radio-group';
import { SearchSelect } from '@/components/molecules/search-select';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listLocations,
  createLocation,
  updateLocation,
  deleteLocation,
  bulkUpdateLocationStatus,
  bulkDeleteLocations,
  type ErpLocation,
  type CreateLocationPayload,
} from '@/lib/api/locations';
import { listBranches } from '@/lib/api/branches';
import { validateForm, type FormErrors } from '@/lib/form-validation';

// ─── Form state ───────────────────────────────────────────────────────────────

interface LocationForm {
  code: string;
  name: string;
  branchId: string;
  addressLine1: string;
  city: string;
  postalCode: string;
  phone: string;
  notes: string;
  isActive: boolean;
}

const defaultForm = (): LocationForm => ({
  code: '',
  name: '',
  branchId: '',
  addressLine1: '',
  city: '',
  postalCode: '',
  phone: '',
  notes: '',
  isActive: true,
});

const fromRecord = (l: ErpLocation): LocationForm => ({
  code: l.code,
  name: l.name,
  branchId: l.branchId ?? '',
  addressLine1: l.addressLine1 ?? '',
  city: l.city ?? '',
  postalCode: l.postalCode ?? '',
  phone: l.phone ?? '',
  notes: l.notes ?? '',
  isActive: l.isActive,
});

const toPayload = (f: LocationForm): CreateLocationPayload => ({
  code: f.code,
  name: f.name,
  branchId: f.branchId,
  addressLine1: f.addressLine1 || undefined,
  city: f.city || undefined,
  postalCode: f.postalCode || undefined,
  phone: f.phone || undefined,
  notes: f.notes || undefined,
  isActive: f.isActive,
});

// ─── Validation ───────────────────────────────────────────────────────────────

const validateLocation = (form: LocationForm) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'branchId', label: 'Cabang', required: true },
  ]);

// ─── Branch loader for SearchSelect ───────────────────────────────────────────

async function loadBranchOptions(search: string, page: number, limit: number) {
  const res = await listBranches({ search: search || undefined, page, limit, isActive: true });
  return { data: res.data.map((b) => ({ value: b.id, label: b.name, code: b.code })), total: res.meta.total };
}

// ─── Form ─────────────────────────────────────────────────────────────────────

function LocationFormFields({ data, onChange, errors = {} }: { data: LocationForm; onChange: (d: LocationForm) => void; errors?: FormErrors<LocationForm> }) {
  const set = <K extends keyof LocationForm>(k: K, v: LocationForm[K]) =>
    onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Kode" htmlFor="lf-code" required error={errors.code}>
        <Input id="lf-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="LOC-001" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="lf-name" required error={errors.name}>
        <Input id="lf-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Gudang Utara" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Cabang" htmlFor="lf-branch" required error={errors.branchId}>
        <SearchSelect
          id="lf-branch"
          placeholder="Cari cabang…"
          value={data.branchId}
          onValueChange={(v) => set('branchId', v)}
          loadOptions={loadBranchOptions}
          error={!!errors.branchId}
        />
      </FormField>
      <FormField label="Alamat" htmlFor="lf-addr">
        <Input id="lf-addr" value={data.addressLine1} onChange={(e) => set('addressLine1', e.target.value)} placeholder="Jl. Industri No. 10" />
      </FormField>
      <FormField label="Kota" htmlFor="lf-city">
        <Input id="lf-city" value={data.city} onChange={(e) => set('city', e.target.value)} placeholder="Bekasi" />
      </FormField>
      <FormField label="Kode Pos" htmlFor="lf-zip">
        <Input id="lf-zip" value={data.postalCode} onChange={(e) => set('postalCode', e.target.value)} placeholder="17141" />
      </FormField>
      <FormField label="Telepon" htmlFor="lf-phone">
        <Input id="lf-phone" value={data.phone} onChange={(e) => set('phone', e.target.value)} placeholder="021-8881234" />
      </FormField>
      <FormField label="Catatan" htmlFor="lf-notes">
        <Input id="lf-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} placeholder="Lokasi penyimpanan bahan baku" />
      </FormField>
      <FormField label="Status" htmlFor="lf-active">
        <BooleanRadio id="lf-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

// ─── Extra columns ────────────────────────────────────────────────────────────

const extraColumns: ExtraColumn<ErpLocation>[] = [
  { key: 'branch', label: 'Cabang', render: (r) => r.branch ? `${r.branch.code} — ${r.branch.name}` : '—' },
  { key: 'city', label: 'Kota', render: (r) => r.city ?? '—' },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpLocationsPage() {
  return (
    <SimpleMasterPage<ErpLocation, LocationForm>
      title="Location"
      code="LOC"
      entityLabel="location"
      storageKey="locations"
      auditEntityName="ErpLocation"
      list={listLocations}
      create={createLocation}
      update={updateLocation}
      remove={deleteLocation}
      bulkStatus={bulkUpdateLocationStatus}
      bulkDelete={bulkDeleteLocations}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={LocationFormFields}
      validate={validateLocation}
      extraColumns={extraColumns}
      defaultSortBy="code"
      defaultSortDir="asc"
    />
  );
}
