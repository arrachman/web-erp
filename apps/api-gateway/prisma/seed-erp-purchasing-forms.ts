/**
 * Seed for Form Builder (header form fields) for the Purchasing (M4) transactions.
 * Upserts ErpFormField rows (sys_form_fields, unique [transactionTypeCode, fieldKey])
 * so each transaction header form opens with a curated structural layout
 * (LEFT · CENTER · RIGHT slots) instead of an empty form (or the cash-receipt fallback).
 *
 * Covers all 13 purchasing codes:
 *   item docs  → PUR.PR · PUR.RFQ · PUR.BS · PUR.PO · PUR.GRN · PUR.PI · PUR.DNR · PUR.PRT
 *   payment/ob → PUR.AP · PUR.PP · PUR.VPP · PUR.VP · PUR.OB (reuse finance domain)
 *
 * `fieldKey` is a FIXED CONTRACT — the frontend binds to these keys exactly (they map
 * to pur_* header columns). `update: {}` keeps admin edits on re-seed (only missing
 * defaults created), mirroring the grid-column + sales-form seed convention.
 */

import { PrismaClient } from '@prisma/client';

interface FieldDef {
  fieldKey: string;
  label: string;
  fieldType: string;
  columnSlot: 'LEFT' | 'CENTER' | 'RIGHT';
  sortOrder: number;
  isRequired?: boolean;
  lookupSource?: string;
  defaultValue?: string;
}

// Shared header field set for the item-based procurement docs (PO/GRN/PI/returns).
// supplier required; warehouse + payment term + payable account exposed.
const PURCHASE_FORM_FIELDS: FieldDef[] = [
  // LEFT — identity
  { fieldKey: 'supplierId', label: 'Supplier', fieldType: 'PARTNER', columnSlot: 'LEFT', sortOrder: 0, isRequired: true },
  { fieldKey: 'description', label: 'Uraian', fieldType: 'TEXT', columnSlot: 'LEFT', sortOrder: 1 },
  { fieldKey: 'referenceNo', label: 'No Referensi', fieldType: 'TEXT', columnSlot: 'LEFT', sortOrder: 2 },
  // CENTER — dimensions
  { fieldKey: 'branchId', label: 'Cabang', fieldType: 'BRANCH', columnSlot: 'CENTER', sortOrder: 0, isRequired: true },
  { fieldKey: 'locationId', label: 'Lokasi', fieldType: 'LOCATION', columnSlot: 'CENTER', sortOrder: 1 },
  { fieldKey: 'warehouseId', label: 'Gudang', fieldType: 'LOOKUP', columnSlot: 'CENTER', sortOrder: 2, lookupSource: 'warehouses' },
  { fieldKey: 'payableAccountId', label: 'Rek Hutang', fieldType: 'ACCOUNT', columnSlot: 'CENTER', sortOrder: 3 },
  // RIGHT — document meta
  { fieldKey: 'docDate', label: 'Tanggal', fieldType: 'DATE', columnSlot: 'RIGHT', sortOrder: 0, isRequired: true, defaultValue: '@today' },
  { fieldKey: 'docNumber', label: 'No Transaksi', fieldType: 'TEXT', columnSlot: 'RIGHT', sortOrder: 1 },
  { fieldKey: 'currencyId', label: 'Uang', fieldType: 'CURRENCY', columnSlot: 'RIGHT', sortOrder: 2, isRequired: true, defaultValue: '1' },
  { fieldKey: 'paymentTermId', label: 'Termin', fieldType: 'LOOKUP', columnSlot: 'RIGHT', sortOrder: 3, lookupSource: 'payment-terms' },
  { fieldKey: 'dueDate', label: 'Jatuh Tempo', fieldType: 'DATE', columnSlot: 'RIGHT', sortOrder: 4 },
];

// Requisition / RFQ / Bid: pre-sourcing (supplier not yet committed → optional).
// RFQ invites suppliers in its line grid; PR has no supplier; BS picks the winner.
const PURCHASE_REQUEST_FORM_FIELDS: FieldDef[] = PURCHASE_FORM_FIELDS.map((f) =>
  f.fieldKey === 'supplierId' ? { ...f, isRequired: false } : f,
);

// Payment / opening-balance docs (AP/PP/VPP/VP/OB reuse the finance domain): no
// warehouse/location/item grid — just party, amount context, and document meta.
const PURCHASE_PAYMENT_FORM_FIELDS: FieldDef[] = [
  // LEFT — identity
  { fieldKey: 'supplierId', label: 'Supplier', fieldType: 'PARTNER', columnSlot: 'LEFT', sortOrder: 0, isRequired: true },
  { fieldKey: 'description', label: 'Uraian', fieldType: 'TEXT', columnSlot: 'LEFT', sortOrder: 1 },
  { fieldKey: 'referenceNo', label: 'No Referensi', fieldType: 'TEXT', columnSlot: 'LEFT', sortOrder: 2 },
  // CENTER — dimensions
  { fieldKey: 'branchId', label: 'Cabang', fieldType: 'BRANCH', columnSlot: 'CENTER', sortOrder: 0, isRequired: true },
  { fieldKey: 'payableAccountId', label: 'Rek Hutang', fieldType: 'ACCOUNT', columnSlot: 'CENTER', sortOrder: 1 },
  // RIGHT — document meta
  { fieldKey: 'docDate', label: 'Tanggal', fieldType: 'DATE', columnSlot: 'RIGHT', sortOrder: 0, isRequired: true, defaultValue: '@today' },
  { fieldKey: 'docNumber', label: 'No Transaksi', fieldType: 'TEXT', columnSlot: 'RIGHT', sortOrder: 1 },
  { fieldKey: 'currencyId', label: 'Uang', fieldType: 'CURRENCY', columnSlot: 'RIGHT', sortOrder: 2, isRequired: true, defaultValue: '1' },
  { fieldKey: 'dueDate', label: 'Jatuh Tempo', fieldType: 'DATE', columnSlot: 'RIGHT', sortOrder: 3 },
];

// transaction code → its header field set.
const FIELDS_BY_CODE: Record<string, FieldDef[]> = {
  'PUR.PR': PURCHASE_REQUEST_FORM_FIELDS,
  'PUR.RFQ': PURCHASE_REQUEST_FORM_FIELDS,
  'PUR.BS': PURCHASE_REQUEST_FORM_FIELDS,
  'PUR.PO': PURCHASE_FORM_FIELDS,
  'PUR.GRN': PURCHASE_FORM_FIELDS,
  'PUR.PI': PURCHASE_FORM_FIELDS,
  'PUR.DNR': PURCHASE_FORM_FIELDS,
  'PUR.PRT': PURCHASE_FORM_FIELDS,
  'PUR.AP': PURCHASE_PAYMENT_FORM_FIELDS,
  'PUR.PP': PURCHASE_PAYMENT_FORM_FIELDS,
  'PUR.VPP': PURCHASE_PAYMENT_FORM_FIELDS,
  'PUR.VP': PURCHASE_PAYMENT_FORM_FIELDS,
  'PUR.OB': PURCHASE_PAYMENT_FORM_FIELDS,
};

export async function seedPurchasingForms(prisma: PrismaClient): Promise<void> {
  let fieldCount = 0;
  for (const [code, fields] of Object.entries(FIELDS_BY_CODE)) {
    for (const f of fields) {
      await prisma.erpFormField.upsert({
        where: { transactionTypeCode_fieldKey: { transactionTypeCode: code, fieldKey: f.fieldKey } },
        create: {
          transactionTypeCode: code,
          fieldKey: f.fieldKey,
          kind: 'STRUCTURAL',
          label: f.label,
          fieldType: f.fieldType,
          lookupSource: f.lookupSource ?? null,
          defaultValue: f.defaultValue ?? null,
          isRequired: f.isRequired ?? false,
          isVisible: true,
          sortOrder: f.sortOrder,
          columnSlot: f.columnSlot,
        },
        update: {}, // keep admin edits on re-seed; only create missing defaults
      });
      fieldCount += 1;
    }
  }

  console.log(
    `✓ sys_form_fields — purchasing header forms (${Object.keys(FIELDS_BY_CODE).length} codes, ${fieldCount} fields)`,
  );
}
