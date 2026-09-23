'use client';

/**
 * F2 Admin — Document Numbering form fields.
 * Used by document-numberings-page.tsx via SimpleMasterPage FormFields prop.
 */

import * as React from 'react';
import { FormField } from '@/components/ui/form-field';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { BooleanRadio } from '@/components/ui/radio-group';
import {
  NUMBERING_RESET_OPTIONS,
  type ErpDocumentNumbering,
  type ErpNumberingReset,
  type CreateDocumentNumberingPayload,
} from '@/lib/api/document-numberings';
import type { FormErrors } from '@/lib/form-validation';

// ─── Form shape ───────────────────────────────────────────────────────────────

export interface NumberingForm {
  documentCode: string;
  name: string;
  prefix: string;
  digitCount: string;
  resetPolicy: ErpNumberingReset;
  nextNumber: string;
  affectsLedger: boolean;
  affectsInventory: boolean;
  affectsCost: boolean;
  notes: string;
}

// ─── Adapters ─────────────────────────────────────────────────────────────────

const RESET_LABEL: Record<ErpNumberingReset, string> = {
  NEVER: 'Tidak pernah reset',
  YEARLY: 'Reset tiap tahun',
  MONTHLY: 'Reset tiap bulan',
};

export const defaultNumberingForm = (): NumberingForm => ({
  documentCode: '',
  name: '',
  prefix: '',
  digitCount: '6',
  resetPolicy: 'YEARLY',
  nextNumber: '1',
  affectsLedger: false,
  affectsInventory: false,
  affectsCost: false,
  notes: '',
});

export function fromNumbering(n: ErpDocumentNumbering): NumberingForm {
  return {
    documentCode: n.documentCode,
    name: n.name,
    prefix: n.prefix,
    digitCount: String(n.digitCount),
    resetPolicy: n.resetPolicy,
    nextNumber: String(n.nextNumber),
    affectsLedger: n.affectsLedger,
    affectsInventory: n.affectsInventory,
    affectsCost: n.affectsCost,
    notes: n.notes ?? '',
  };
}

export function toNumberingPayload(
  f: NumberingForm,
): CreateDocumentNumberingPayload {
  return {
    documentCode: f.documentCode,
    name: f.name,
    prefix: f.prefix,
    digitCount: Number(f.digitCount),
    resetPolicy: f.resetPolicy,
    nextNumber: Number(f.nextNumber),
    affectsLedger: f.affectsLedger,
    affectsInventory: f.affectsInventory,
    affectsCost: f.affectsCost,
    notes: f.notes || undefined,
  };
}

// ─── FormFields component ─────────────────────────────────────────────────────

interface NumberingFormFieldsProps {
  data: NumberingForm;
  onChange: (d: NumberingForm) => void;
  errors?: FormErrors<NumberingForm>;
  /** True when editing an existing record — documentCode is read-only. */
  isEditing?: boolean;
}

export function NumberingFormFields({
  data,
  onChange,
  errors = {},
  isEditing = false,
}: NumberingFormFieldsProps) {
  const set = (k: keyof NumberingForm, v: string | boolean) =>
    onChange({ ...data, [k]: v });

  const flagField = (
    key: 'affectsLedger' | 'affectsInventory' | 'affectsCost',
    label: string,
  ) => (
    <FormField label={label} htmlFor={`dn-${key}`}>
      <BooleanRadio
        id={`dn-${key}`}
        value={data[key]}
        onValueChange={(v) => set(key, v)}
        trueLabel="Ya"
        falseLabel="Tidak"
      />
    </FormField>
  );

  return (
    <div className="p-4">
      <FormField label="Kode Dokumen" htmlFor="dn-code" required error={errors.documentCode}>
        <Input
          id="dn-code"
          value={data.documentCode}
          onChange={(e) => set('documentCode', e.target.value)}
          placeholder="INV-OUT"
          disabled={isEditing}
          aria-invalid={!!errors.documentCode}
        />
      </FormField>
      <FormField label="Nama" htmlFor="dn-name" required error={errors.name}>
        <Input
          id="dn-name"
          value={data.name}
          onChange={(e) => set('name', e.target.value)}
          placeholder="Sales Invoice"
          aria-invalid={!!errors.name}
        />
      </FormField>
      <FormField label="Prefix" htmlFor="dn-prefix" required error={errors.prefix}>
        <Input
          id="dn-prefix"
          value={data.prefix}
          onChange={(e) => set('prefix', e.target.value)}
          placeholder="INV"
          aria-invalid={!!errors.prefix}
        />
      </FormField>
      <FormField label="Jumlah Digit" htmlFor="dn-digits" required error={errors.digitCount}>
        <Input
          id="dn-digits"
          type="number"
          value={data.digitCount}
          onChange={(e) => set('digitCount', e.target.value)}
          placeholder="6"
          aria-invalid={!!errors.digitCount}
        />
      </FormField>
      <FormField label="Kebijakan Reset" htmlFor="dn-reset" required>
        <Select
          value={data.resetPolicy}
          onValueChange={(v) => set('resetPolicy', v as ErpNumberingReset)}
        >
          <SelectTrigger id="dn-reset">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {NUMBERING_RESET_OPTIONS.map((opt) => (
              <SelectItem key={opt} value={opt}>
                {RESET_LABEL[opt]}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </FormField>
      <FormField label="Nomor Berikutnya" htmlFor="dn-next" required error={errors.nextNumber}>
        <Input
          id="dn-next"
          type="number"
          value={data.nextNumber}
          onChange={(e) => set('nextNumber', e.target.value)}
          placeholder="1"
          aria-invalid={!!errors.nextNumber}
        />
      </FormField>
      {flagField('affectsLedger', 'Pengaruhi Buku Besar')}
      {flagField('affectsInventory', 'Pengaruhi Persediaan')}
      {flagField('affectsCost', 'Pengaruhi Biaya')}
      <FormField label="Catatan" htmlFor="dn-notes">
        <Input
          id="dn-notes"
          value={data.notes}
          onChange={(e) => set('notes', e.target.value)}
          placeholder="Opsional"
        />
      </FormField>
    </div>
  );
}
