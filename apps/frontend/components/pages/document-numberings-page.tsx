'use client';

/**
 * F2 Admin — Document Numbering page (sys_document_numberings).
 * Refactored to use SimpleMasterPage organism.
 */

import * as React from 'react';
import { Badge } from '@/components/ui/badge';
import { SimpleMasterPage, type BaseEntity } from '@/components/organisms/simple-master-page';
import {
  listDocumentNumberings,
  createDocumentNumbering,
  updateDocumentNumbering,
  deleteDocumentNumbering,
  type ErpDocumentNumbering,
  type ErpNumberingReset,
} from '@/lib/api/document-numberings';
import { validateForm, type FormErrors } from '@/lib/form-validation';
import type { PaginatedResponse, PaginationParams } from '@/lib/api/types';
import {
  NumberingFormFields,
  defaultNumberingForm,
  fromNumbering,
  toNumberingPayload,
  type NumberingForm,
} from './document-numberings-form';

// ─── Adapter type ──────────────────────────────────────────────────────────────
// SimpleMasterPage requires BaseEntity (id, code, name, isActive).
// ErpDocumentNumbering uses documentCode instead of code, and has no isActive.

interface DocNumRow extends ErpDocumentNumbering, BaseEntity {
  code: string;      // mirrors documentCode
  isActive: boolean; // always true — no status concept for numbering records
}

// ─── Bulk stubs ────────────────────────────────────────────────────────────────
// Document numberings have no bulk status/delete API.
// SimpleMasterPage requires these props — stubs notify via thrown error so
// the organism's confirmAction/catch path shows a user-friendly message.

async function bulkStatusStub(): Promise<{ affected: number }> {
  throw new Error('Operasi batch tidak didukung untuk Penomoran Dokumen');
}

async function bulkDeleteStub(): Promise<{ affected: number }> {
  throw new Error('Operasi batch tidak didukung untuk Penomoran Dokumen');
}

// ─── List adapter ──────────────────────────────────────────────────────────────

async function listDocNumRows(
  params?: PaginationParams,
): Promise<PaginatedResponse<DocNumRow>> {
  const res = await listDocumentNumberings(params);
  return {
    ...res,
    data: res.data.map((r) => ({
      ...r,
      code: r.documentCode,
      isActive: true,
    })),
  };
}

// ─── Form adapters ─────────────────────────────────────────────────────────────

const defaultForm = defaultNumberingForm;

const fromRecord = (row: DocNumRow): NumberingForm => fromNumbering(row);

const toPayload = toNumberingPayload;

// ─── Validation ────────────────────────────────────────────────────────────────

function validateNumbering(form: NumberingForm): FormErrors<NumberingForm> {
  return validateForm(form, [
    { field: 'documentCode', label: 'Kode Dokumen', required: true },
    { field: 'name', label: 'Nama', required: true },
    { field: 'prefix', label: 'Prefix', required: true },
    { field: 'digitCount', label: 'Jumlah Digit', required: true },
    { field: 'nextNumber', label: 'Nomor Berikutnya', required: true },
  ]);
}

// ─── Extra columns ─────────────────────────────────────────────────────────────

const RESET_LABEL: Record<ErpNumberingReset, string> = {
  NEVER: 'Tidak pernah',
  YEARLY: 'Tiap tahun',
  MONTHLY: 'Tiap bulan',
};

const extraColumns = [
  {
    key: 'prefix',
    label: 'Prefix',
    sortable: false,
    render: (row: DocNumRow) => <span className="mono">{row.prefix}</span>,
  },
  {
    key: 'digitCount',
    label: 'Digit',
    sortable: false,
    render: (row: DocNumRow) => <span className="mono">{row.digitCount}</span>,
  },
  {
    key: 'resetPolicy',
    label: 'Reset',
    sortable: false,
    render: (row: DocNumRow) => (
      <Badge variant="default">{RESET_LABEL[row.resetPolicy]}</Badge>
    ),
  },
];

// ─── Page ──────────────────────────────────────────────────────────────────────

export function ErpDocumentNumberingsPage() {
  return (
    <SimpleMasterPage<DocNumRow, NumberingForm>
      title="Penomoran Dokumen"
      code="DOCNUM"
      entityLabel="penomoran"
      storageKey="document-numberings"
      auditEntityName="ErpDocumentNumbering"
      list={listDocNumRows}
      create={(payload) => createDocumentNumbering(payload).then((r) => ({ ...r, code: r.documentCode, isActive: true }))}
      update={(id, payload) => updateDocumentNumbering(id, payload).then((r) => ({ ...r, code: r.documentCode, isActive: true }))}
      remove={deleteDocumentNumbering}
      bulkStatus={bulkStatusStub}
      bulkDelete={bulkDeleteStub}
      defaultForm={defaultForm}
      fromRecord={fromRecord}
      toPayload={toPayload}
      FormFields={NumberingFormFields}
      validate={validateNumbering}
      defaultSortBy="documentCode"
      defaultSortDir="asc"
      extraColumns={extraColumns}
    />
  );
}
