// Form Builder API — per-transaction-type header form field configuration.
// Mirrors the backend erp-form-fields module. Endpoints: /:code/fields (GET + PUT).

import { apiGet, apiPut } from './client';

export const FORM_FIELD_KINDS = ['STRUCTURAL', 'CUSTOM'] as const;
export const FORM_FIELD_TYPES = [
  'TEXT', 'NUMBER', 'DATE',
  'PARTNER', 'ACCOUNT', 'BRANCH', 'LOCATION', 'CURRENCY', 'LOOKUP',
] as const;
export const FORM_COLUMN_SLOTS = ['LEFT', 'CENTER', 'RIGHT'] as const;

/**
 * Sentinel stored in a DATE field's `defaultValue` meaning "resolve to the
 * current date each time a new record opens" (dynamic), as opposed to a fixed
 * ISO date string. Resolved at form-fill time — never persisted as a real date.
 */
export const TODAY_DEFAULT = '@today';

export type FormFieldKind = (typeof FORM_FIELD_KINDS)[number];
export type FormFieldType = (typeof FORM_FIELD_TYPES)[number];
export type FormColumnSlot = (typeof FORM_COLUMN_SLOTS)[number];

export const FORM_FIELD_TYPE_LABELS: Record<FormFieldType, string> = {
  TEXT: 'Teks',
  NUMBER: 'Angka',
  DATE: 'Tanggal',
  PARTNER: 'Partner (Lookup)',
  ACCOUNT: 'Akun GL (Lookup)',
  BRANCH: 'Cabang (Lookup)',
  LOCATION: 'Lokasi (Lookup)',
  CURRENCY: 'Mata Uang (Lookup)',
  LOOKUP: 'Lookup Kustom',
};

export const STRUCTURAL_FIELD_KEYS = [
  'partnerId', 'bankAccountId', 'description',
  'branchId', 'locationId', 'transactionDate', 'docNumber', 'currencyId',
] as const;

export interface ErpFormField {
  id?: string;
  fieldKey: string;
  kind: FormFieldKind;
  label: string;
  fieldType: FormFieldType;
  lookupSource?: string | null;
  lookupDefaultFilter?: Record<string, unknown> | null;
  lookupDefaultSort?: string | null;
  /** Input placeholder shown when empty; null/empty = form's built-in default. */
  placeholder?: string | null;
  /** Value prefilled on new records (lookup = id, others = raw string). */
  defaultValue?: string | null;
  /** Resolved "{code} - {name}" label for a lookup `defaultValue` (server-derived, read-only). */
  defaultValueLabel?: string | null;
  /** Always non-editable regardless of document workflow status. */
  isReadonly?: boolean;
  isRequired: boolean;
  isVisible: boolean;
  sortOrder: number;
  columnSlot: FormColumnSlot;
}

/**
 * Built-in structural layout used as a fallback when no Form Builder config has
 * loaded yet (or the transaction type has no saved config). The authoritative
 * layout always comes from the API; this only prevents an empty header flash and
 * keeps direction-agnostic forms working. Labels for partner/account are generic
 * here — callers override them with direction labels (e.g. "Terima Dari").
 */
export const DEFAULT_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'partnerId', kind: 'STRUCTURAL', label: 'Partner', fieldType: 'PARTNER', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'bankAccountId', kind: 'STRUCTURAL', label: 'Akun Kas/Bank', fieldType: 'ACCOUNT', isRequired: true, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: false, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'transactionDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
];

export interface FormFieldsResponse {
  code: string;
  fields: ErpFormField[];
}

export const getFormFields = (code: string) =>
  apiGet<FormFieldsResponse>(`/transaction-forms/${encodeURIComponent(code)}/fields`);

export const saveFormFields = (code: string, fields: ErpFormField[]) =>
  apiPut<FormFieldsResponse>(`/transaction-forms/${encodeURIComponent(code)}/fields`, {
    fields: fields.map((f, i) => ({
      fieldKey: f.fieldKey,
      kind: f.kind,
      label: f.label,
      fieldType: f.fieldType,
      lookupSource: f.lookupSource ?? undefined,
      lookupDefaultFilter: f.lookupDefaultFilter ?? undefined,
      lookupDefaultSort: f.lookupDefaultSort ?? undefined,
      placeholder: f.placeholder ?? undefined,
      defaultValue: f.defaultValue ?? undefined,
      isReadonly: f.isReadonly ?? false,
      isRequired: f.isRequired,
      isVisible: f.isVisible,
      sortOrder: i,
      columnSlot: f.columnSlot,
    })),
  });
