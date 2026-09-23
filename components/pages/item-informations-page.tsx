'use client';

/**
 * M1 Master Data — Item Information page (`/master/item-info`).
 * 1:1 extension of md_items: manufacturer, country, warranty, long
 * description, specifications, tags, notes.
 * Atomic tier: Page.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input, Textarea } from '@/components/ui/input';
import { SearchSelect } from '@/components/molecules/search-select';
import { SimpleMasterPage, type ExtraColumn, type BaseEntity } from '@/components/organisms/simple-master-page';
import {
  listItemInformations, createErpItemInformation, updateErpItemInformation, deleteErpItemInformation,
  type ErpItemInformation, type CreateErpItemInformationPayload,
} from '@/lib/api/item-informations';
import { listItems } from '@/lib/api/items';
import { validateForm, type FormErrors } from '@/lib/form-validation';

// ─── Record adapter ───────────────────────────────────────────────────────────

interface ItemInfoRecord extends BaseEntity {
  itemId: string;
  itemCode?: string;
  itemName?: string;
  longDescription?: string;
  specifications?: string;
  manufacturer?: string;
  countryOfOrigin?: string;
  warrantyPeriodMonths?: number;
  tags?: string;
  notes?: string;
}

function toRecord(r: ErpItemInformation): ItemInfoRecord {
  return {
    ...r,
    code: r.item?.code ?? `INF-${r.id}`,
    name: r.item?.name ?? `Item #${r.itemId}`,
    isActive: true,
    itemCode: r.item?.code,
    itemName: r.item?.name,
  };
}

// ─── Form state ───────────────────────────────────────────────────────────────

interface FormData {
  id?: string;
  itemId: string;
  itemLabel?: string;
  longDescription: string;
  specifications: string;
  manufacturer: string;
  countryOfOrigin: string;
  warrantyPeriodMonths: number;
  tags: string;
  notes: string;
}

const defaultForm = (): FormData => ({
  itemId: '',
  itemLabel: '',
  longDescription: '',
  specifications: '',
  manufacturer: '',
  countryOfOrigin: '',
  warrantyPeriodMonths: 0,
  tags: '',
  notes: '',
});

const fromRecord = (r: ItemInfoRecord): FormData => ({
  id: r.id,
  itemId: r.itemId,
  itemLabel: r.itemName ?? r.itemId,
  longDescription: r.longDescription ?? '',
  specifications: r.specifications ?? '',
  manufacturer: r.manufacturer ?? '',
  countryOfOrigin: r.countryOfOrigin ?? '',
  warrantyPeriodMonths: r.warrantyPeriodMonths ?? 0,
  tags: r.tags ?? '',
  notes: r.notes ?? '',
});

const toPayload = (f: FormData): CreateErpItemInformationPayload => ({
  itemId: f.itemId,
  longDescription: f.longDescription || undefined,
  specifications: f.specifications || undefined,
  manufacturer: f.manufacturer || undefined,
  countryOfOrigin: f.countryOfOrigin || undefined,
  warrantyPeriodMonths: f.warrantyPeriodMonths || undefined,
  tags: f.tags || undefined,
  notes: f.notes || undefined,
});

const validateItemInformation = (form: FormData) =>
  validateForm(form, [
    { field: 'itemId', label: 'Item', required: true },
  ]);

// ─── Form fields ──────────────────────────────────────────────────────────────

function FormFields({
  data, onChange, errors = {},
}: {
  data: FormData;
  onChange: (d: FormData) => void;
  errors?: FormErrors<FormData>;
}) {
  const set = <K extends keyof FormData>(k: K, v: FormData[K]) =>
    onChange({ ...data, [k]: v });

  const isEdit = Boolean(data.id);

  const loadItemOptions = React.useCallback(
    async (search: string, page: number, limit: number) => {
      const res = await listItems({
        search: search || undefined, page, limit, isActive: true,
      });
      return {
        data: res.data.map((it) => ({
          value: it.id, label: it.name, code: it.code,
        })),
        total: res.meta.total,
      };
    },
    [],
  );

  return (
    <div className="p-4">
      <FormField label="Item" htmlFor="ef-item" required error={errors.itemId}>
        <SearchSelect
          id="ef-item"
          placeholder={isEdit ? 'Item tidak dapat diubah' : 'Cari item…'}
          value={data.itemId}
          onValueChange={(v) => set('itemId', v)}
          loadOptions={loadItemOptions}
          initialLabel={data.itemLabel}
          title="Pilih Item"
          disabled={isEdit}
          error={!!errors.itemId}
        />
      </FormField>
      <FormField label="Produsen" htmlFor="ef-manufacturer">
        <Input
          id="ef-manufacturer" value={data.manufacturer}
          onChange={(e) => set('manufacturer', e.target.value)}
          placeholder="Nama produsen (opsional)"
        />
      </FormField>
      <FormField label="Negara Asal" htmlFor="ef-origin">
        <Input
          id="ef-origin" value={data.countryOfOrigin}
          onChange={(e) => set('countryOfOrigin', e.target.value)}
          placeholder="Negara asal (opsional)"
        />
      </FormField>
      <FormField label="Garansi (Bulan)" htmlFor="ef-warranty">
        <Input
          id="ef-warranty" type="number" min={0} value={data.warrantyPeriodMonths}
          onChange={(e) => set('warrantyPeriodMonths', parseInt(e.target.value, 10) || 0)}
          placeholder="0"
        />
      </FormField>
      <FormField label="Deskripsi Panjang" htmlFor="ef-long">
        <Textarea
          id="ef-long" value={data.longDescription} rows={3}
          onChange={(e) => set('longDescription', e.target.value)}
          placeholder="Deskripsi lengkap (opsional)"
        />
      </FormField>
      <FormField label="Spesifikasi" htmlFor="ef-spec">
        <Textarea
          id="ef-spec" value={data.specifications} rows={3}
          onChange={(e) => set('specifications', e.target.value)}
          placeholder="Spesifikasi teknis (opsional)"
        />
      </FormField>
      <FormField label="Tags" htmlFor="ef-tags">
        <Input
          id="ef-tags" value={data.tags}
          onChange={(e) => set('tags', e.target.value)}
          placeholder="comma,separated (opsional)"
        />
      </FormField>
      <FormField label="Catatan" htmlFor="ef-notes">
        <Textarea
          id="ef-notes" value={data.notes} rows={2}
          onChange={(e) => set('notes', e.target.value)}
          placeholder="Catatan tambahan (opsional)"
        />
      </FormField>
    </div>
  );
}

// ─── API adapters ─────────────────────────────────────────────────────────────

async function listAdapter(
  params: Parameters<typeof listItemInformations>[0],
): Promise<{
  success: boolean; data: ItemInfoRecord[];
  meta: { total: number; page: number; limit: number; totalPages: number };
}> {
  const res = await listItemInformations(params);
  return { ...res, data: res.data.map(toRecord) };
}

async function createAdapter(payload: CreateErpItemInformationPayload): Promise<ItemInfoRecord> {
  return toRecord(await createErpItemInformation(payload));
}

async function updateAdapter(id: string, payload: Partial<CreateErpItemInformationPayload>): Promise<ItemInfoRecord> {
  return toRecord(await updateErpItemInformation(id, payload));
}

function noopBulkStatus(_ids: string[], _isActive: boolean): Promise<{ affected: number }> {
  return Promise.resolve({ affected: 0 });
}

async function bulkDeleteAdapter(ids: string[]): Promise<{ affected: number }> {
  let affected = 0;
  for (const id of ids) {
    try {
      await deleteErpItemInformation(id);
      affected += 1;
    } catch {
      // continue on individual failure; aggregate count reflects success
    }
  }
  return { affected };
}

// ─── Extra columns ────────────────────────────────────────────────────────────

const extraColumns: ExtraColumn<ItemInfoRecord>[] = [
  { key: 'manufacturer', label: 'Produsen', render: (r) => r.manufacturer ?? '—' },
  { key: 'countryOfOrigin', label: 'Negara Asal', render: (r) => r.countryOfOrigin ?? '—' },
  {
    key: 'warranty', label: 'Garansi',
    render: (r) => (r.warrantyPeriodMonths ? `${r.warrantyPeriodMonths} bln` : '—'),
  },
];

// ─── Page ─────────────────────────────────────────────────────────────────────

export function ErpItemInformationsPage() {
  return (
    <SimpleMasterPage<ItemInfoRecord, FormData>
      title="Item Information"
      code="INF"
      entityLabel="item information"
      storageKey="item-informations"
      auditEntityName="ErpItemInformation"
      list={listAdapter}
      create={createAdapter}
      update={updateAdapter}
      remove={deleteErpItemInformation}
      bulkStatus={noopBulkStatus}
      bulkDelete={bulkDeleteAdapter}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={FormFields}
      validate={validateItemInformation}
      extraColumns={extraColumns}
    />
  );
}
