'use client';

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import { SimpleMasterPage } from '@/components/organisms/simple-master-page';
import {
  listTxnNoteDetails, createErpTxnNoteDetail, updateErpTxnNoteDetail, deleteErpTxnNoteDetail,
  bulkDeleteErpTxnNoteDetails,
  type ErpTxnNoteDetail, type CreateErpTxnNoteDetailPayload,
} from '@/lib/api/txn-note-details';
import { listTransactionNotes } from '@/lib/api/transaction-notes';
import { validateForm, type FormErrors } from '@/lib/form-validation';
import { SearchSelect } from '@/components/molecules/search-select';
import type { BaseEntity } from '@/components/organisms/simple-master-page';

interface TxnNoteDetailRecord extends BaseEntity {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  transactionNoteId: string;
  sortOrder: number;
  content: string;
  transactionNote?: { id: string; code: string; name: string } | null;
}

function toRecord(r: ErpTxnNoteDetail): TxnNoteDetailRecord {
  return {
    ...r,
    code: r.id,
    name: r.content.slice(0, 60) + (r.content.length > 60 ? '…' : ''),
    isActive: true,
  };
}

interface FormData {
  transactionNoteId: string;
  sortOrder: number;
  content: string;
}

const defaultForm = (): FormData => ({
  transactionNoteId: '',
  sortOrder: 0,
  content: '',
});

const fromRecord = (r: TxnNoteDetailRecord): FormData => ({
  transactionNoteId: r.transactionNoteId,
  sortOrder: r.sortOrder,
  content: r.content,
});

const toPayload = (f: FormData): CreateErpTxnNoteDetailPayload => ({
  transactionNoteId: f.transactionNoteId,
  sortOrder: f.sortOrder,
  content: f.content,
});

const validateDetail = (form: FormData) =>
  validateForm(form, [
    { field: 'transactionNoteId', label: 'Catatan Transaksi', required: true },
    { field: 'content', label: 'Konten', required: true },
  ]);

async function loadNoteOptions(search: string, page: number, limit: number) {
  const res = await listTransactionNotes({ page, limit, search: search || undefined });
  return {
    data: (res.data ?? []).map((n) => ({ value: String(n.id), label: `${n.code} — ${n.name}` })),
    total: res.meta?.total ?? 0,
  };
}

function FormFields({ data, onChange, errors = {} }: { data: FormData; onChange: (d: FormData) => void; errors?: FormErrors<FormData> }) {
  const set = <K extends keyof FormData>(k: K, v: FormData[K]) => onChange({ ...data, [k]: v });
  return (
    <div className="p-4">
      <FormField label="Catatan Transaksi" htmlFor="tnd-note" required error={errors.transactionNoteId}>
        <SearchSelect
          id="tnd-note"
          value={data.transactionNoteId}
          onValueChange={(v: string) => set('transactionNoteId', v)}
          loadOptions={loadNoteOptions}
          placeholder="Pilih catatan transaksi…"
          error={!!errors.transactionNoteId}
        />
      </FormField>
      <FormField label="Sort Order" htmlFor="tnd-sort">
        <Input
          id="tnd-sort"
          type="number"
          value={String(data.sortOrder)}
          onChange={(e) => set('sortOrder', Number(e.target.value))}
          placeholder="0"
        />
      </FormField>
      <FormField label="Konten" htmlFor="tnd-content" required error={errors.content}>
        <textarea
          id="tnd-content"
          className="w-full rounded border border-[var(--border)] bg-[var(--bg-input)] px-3 py-2 text-sm resize-y min-h-[80px]"
          value={data.content}
          onChange={(e) => set('content', e.target.value)}
          placeholder="Isi detail catatan transaksi…"
          aria-invalid={!!errors.content}
        />
      </FormField>
    </div>
  );
}

async function noBulkStatus(_ids: string[], _isActive: boolean) {
  return { affected: 0 };
}

export function ErpTxnNoteDetailsPage() {
  return (
    <SimpleMasterPage<TxnNoteDetailRecord, FormData>
      title="Txn Note Detail"
      code="TND"
      entityLabel="detail catatan"
      storageKey="txn-note-details"
      auditEntityName="ErpTransactionNoteDetail"
      list={async (params) => {
        const res = await listTxnNoteDetails(params);
        return { ...res, data: (res.data ?? []).map(toRecord) };
      }}
      create={async (payload) => toRecord(await createErpTxnNoteDetail(payload))}
      update={async (id, payload) => toRecord(await updateErpTxnNoteDetail(id, payload))}
      remove={deleteErpTxnNoteDetail}
      bulkStatus={noBulkStatus}
      bulkDelete={bulkDeleteErpTxnNoteDetails}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload as any}
      FormFields={FormFields}
      validate={validateDetail}
      extraColumns={[
        {
          key: 'transactionNote',
          label: 'Catatan Transaksi',
          sortable: false,
          render: (row) => row.transactionNote ? `${row.transactionNote.code} — ${row.transactionNote.name}` : '—',
        },
        {
          key: 'sortOrder',
          label: 'Order',
          sortable: true,
          render: (row) => String(row.sortOrder),
        },
      ]}
      defaultSortBy="createdAt"
      defaultSortDir="desc"
    />
  );
}
