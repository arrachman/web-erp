/**
 * Seed for Form Builder (header form fields) for the Sales (M5) item-based
 * transactions. Upserts ErpFormField rows (sys_form_fields, unique
 * [transactionTypeCode, fieldKey]) so the transaction header form opens with a
 * curated structural layout (LEFT · CENTER · RIGHT slots) instead of an empty form.
 *
 * Covers the 9 item-based sales codes (those with a grid):
 *   SLS.SQ · SLS.SO · SLS.PI · SLS.PL · SLS.DO · SLS.DR · SLS.SI · SLS.RNR · SLS.SR
 *
 * `fieldKey` is a FIXED CONTRACT — the frontend binds to these keys exactly.
 * `update: {}` keeps admin edits on re-seed (only missing defaults created),
 * mirroring the grid-column seed convention.
 */

import { PrismaClient } from '@prisma/client';

// Item-based sales codes that get the shared header field set.
const SALES_FORM_CODES = [
  'SLS.SQ', 'SLS.SO', 'SLS.PI', 'SLS.PL', 'SLS.DO', 'SLS.DR', 'SLS.SI', 'SLS.RNR', 'SLS.SR',
];

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

// Shared header field set (same for every sales code). kind = STRUCTURAL.
const SALES_FORM_FIELDS: FieldDef[] = [
  // LEFT — identity
  { fieldKey: 'customerId', label: 'Pelanggan', fieldType: 'PARTNER', columnSlot: 'LEFT', sortOrder: 0, isRequired: true },
  { fieldKey: 'description', label: 'Uraian', fieldType: 'TEXT', columnSlot: 'LEFT', sortOrder: 1 },
  { fieldKey: 'referenceNo', label: 'No Referensi', fieldType: 'TEXT', columnSlot: 'LEFT', sortOrder: 2 },
  // CENTER — dimensions
  { fieldKey: 'branchId', label: 'Cabang', fieldType: 'BRANCH', columnSlot: 'CENTER', sortOrder: 0, isRequired: true },
  { fieldKey: 'locationId', label: 'Lokasi', fieldType: 'LOCATION', columnSlot: 'CENTER', sortOrder: 1 },
  { fieldKey: 'warehouseId', label: 'Gudang', fieldType: 'LOOKUP', columnSlot: 'CENTER', sortOrder: 2, lookupSource: 'warehouses' },
  { fieldKey: 'salesDeptId', label: 'Divisi Jual', fieldType: 'LOOKUP', columnSlot: 'CENTER', sortOrder: 3, lookupSource: 'divisions' },
  // RIGHT — document meta
  { fieldKey: 'docDate', label: 'Tanggal', fieldType: 'DATE', columnSlot: 'RIGHT', sortOrder: 0, isRequired: true, defaultValue: '@today' },
  { fieldKey: 'docNumber', label: 'No Transaksi', fieldType: 'TEXT', columnSlot: 'RIGHT', sortOrder: 1 },
  { fieldKey: 'currencyId', label: 'Uang', fieldType: 'CURRENCY', columnSlot: 'RIGHT', sortOrder: 2, isRequired: true, defaultValue: '1' },
  { fieldKey: 'paymentTermId', label: 'Termin', fieldType: 'LOOKUP', columnSlot: 'RIGHT', sortOrder: 3, lookupSource: 'payment-terms' },
  { fieldKey: 'dueDate', label: 'Jatuh Tempo', fieldType: 'DATE', columnSlot: 'RIGHT', sortOrder: 4 },
];

export async function seedSalesForms(prisma: PrismaClient): Promise<void> {
  for (const code of SALES_FORM_CODES) {
    for (const f of SALES_FORM_FIELDS) {
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
    }
  }

  console.log(
    `✓ sys_form_fields — sales header forms (${SALES_FORM_CODES.length} codes × ${SALES_FORM_FIELDS.length} fields)`,
  );
}
