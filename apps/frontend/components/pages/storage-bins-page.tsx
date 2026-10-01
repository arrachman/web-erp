'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { BooleanRadio } from '@/components/ui/radio-group';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { SimpleMasterPage, type ExtraColumn } from '@/components/organisms/simple-master-page';
import {
  listStorageBins, createErpStorageBin, updateErpStorageBin, deleteErpStorageBin,
  bulkUpdateErpStorageBinStatus, bulkDeleteErpStorageBins,
  type ErpStorageBin, type CreateErpStorageBinPayload, type ErpBinType,
} from '@/lib/api/storage-bins';
import { validateForm, type FormErrors } from '@/lib/form-validation';
import { SearchSelect } from '@/components/molecules/search-select';
import { listWarehouses } from '@/lib/api/warehouses';

const BIN_TYPE_LABELS: Record<ErpBinType, string> = {
  ZONE: 'Zona',
  RACK: 'Rak',
  BIN: 'Bin',
};

interface FormData {
  code: string;
  name: string;
  warehouseId: string;
  parentId: string;
  binType: ErpBinType;
  notes: string;
  isActive: boolean;
}

const defaultForm = (): FormData => ({
  code: '',
  name: '',
  warehouseId: '',
  parentId: '',
  binType: 'BIN',
  notes: '',
  isActive: true,
});

const fromRecord = (r: ErpStorageBin): FormData => ({
  code: r.code,
  name: r.name,
  warehouseId: String(r.warehouseId),
  parentId: r.parentId == null ? '' : String(r.parentId),
  binType: r.binType,
  notes: r.notes ?? '',
  isActive: r.isActive,
});

const toPayload = (f: FormData): CreateErpStorageBinPayload => ({
  code: f.code,
  name: f.name,
  warehouseId: f.warehouseId,
  parentId: f.parentId || undefined,
  binType: f.binType,
  notes: f.notes || undefined,
  isActive: f.isActive,
});

const validateStorageBin = (form: FormData) =>
  validateForm(form, [
    { field: 'code', label: 'Kode', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'warehouseId', label: 'Gudang', required: true },
  ]);

async function loadWarehouseOptions(search: string, page: number, limit: number) {
  const res = await listWarehouses({ search: search || undefined, page, limit, isActive: true });
  return { data: res.data.map((w) => ({ value: w.id, label: w.name, code: w.code })), total: res.meta.total };
}

/** Loader induk dibatasi gudang terpilih supaya hierarki tidak lintas gudang. */
function makeParentLoader(warehouseId: string) {
  return async (search: string, page: number, limit: number) => {
    const res = await listStorageBins({ search: search || undefined, page, limit, isActive: true, warehouseId });
    return {
      data: res.data.map((b) => ({ value: b.id, label: `${b.name} (${BIN_TYPE_LABELS[b.binType]})`, code: b.code })),
      total: res.meta.total,
    };
  };
}

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = (k: keyof FormData, v: string | boolean) => onChange({ ...data, [k]: v });
  const parentLoader = React.useMemo(() => makeParentLoader(data.warehouseId), [data.warehouseId]);
  return (
    <div className="p-4">
      <FormField label="Gudang" htmlFor="sb-warehouse" required error={errors.warehouseId}>
        <SearchSelect
          id="sb-warehouse"
          placeholder="Cari gudang…"
          value={data.warehouseId}
          onValueChange={(v) => onChange({ ...data, warehouseId: v, parentId: '' })}
          loadOptions={loadWarehouseOptions}
          error={!!errors.warehouseId}
        />
      </FormField>
      <FormField label="Kode" htmlFor="sb-code" required error={errors.code}>
        <Input id="sb-code" value={data.code} onChange={(e) => set('code', e.target.value)} placeholder="A1-01" aria-invalid={!!errors.code} />
      </FormField>
      <FormField label="Nama" htmlFor="sb-name" required error={errors.name}>
        <Input id="sb-name" value={data.name} onChange={(e) => set('name', e.target.value)} placeholder="Rak A1-01" aria-invalid={!!errors.name} />
      </FormField>
      <FormField label="Tipe" htmlFor="sb-type">
        <Select value={data.binType} onValueChange={(v) => set('binType', v)}>
          <SelectTrigger id="sb-type">
            <SelectValue placeholder="Tipe lokasi" />
          </SelectTrigger>
          <SelectContent>
            {(Object.keys(BIN_TYPE_LABELS) as ErpBinType[]).map((t) => (
              <SelectItem key={t} value={t}>{BIN_TYPE_LABELS[t]}</SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>
      <FormField label="Induk (zona/rak)" htmlFor="sb-parent">
        <SearchSelect
          key={data.warehouseId || '_none'}
          id="sb-parent"
          placeholder={data.warehouseId ? 'Cari lokasi induk…' : 'Pilih gudang dulu'}
          value={data.parentId}
          onValueChange={(v) => set('parentId', v)}
          loadOptions={parentLoader}
          disabled={!data.warehouseId}
        />
      </FormField>
      <FormField label="Catatan" htmlFor="sb-notes">
        <Input id="sb-notes" value={data.notes} onChange={(e) => set('notes', e.target.value)} placeholder="Opsional" />
      </FormField>
      <FormField label="Status" htmlFor="sb-active">
        <BooleanRadio id="sb-active" value={data.isActive} onValueChange={(v) => set('isActive', v)} />
      </FormField>
    </div>
  );
}

const extraColumns: ExtraColumn<ErpStorageBin>[] = [
  { key: 'warehouse', label: 'Gudang', render: (r) => (r.warehouse ? `${r.warehouse.code} — ${r.warehouse.name}` : '-') },
  { key: 'parent', label: 'Induk', render: (r) => (r.parent ? `${r.parent.code} — ${r.parent.name}` : '-') },
  { key: 'binType', label: 'Tipe', render: (r) => <Badge variant="default">{BIN_TYPE_LABELS[r.binType] ?? r.binType}</Badge> },
];

export function ErpStorageBinsPage() {
  return (
    <SimpleMasterPage<ErpStorageBin, FormData>
      title="Lokasi Gudang"
      code="BIN"
      entityLabel="lokasi gudang"
      storageKey="storage-bins"
      auditEntityName="ErpStorageBin"
      list={listStorageBins}
      create={createErpStorageBin}
      update={updateErpStorageBin}
      remove={deleteErpStorageBin}
      bulkStatus={bulkUpdateErpStorageBinStatus}
      bulkDelete={bulkDeleteErpStorageBins}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateStorageBin}
      extraColumns={extraColumns}
    />
  );
}
