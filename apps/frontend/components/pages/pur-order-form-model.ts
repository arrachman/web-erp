// Form-data model for the Purchase Order transaction form. Header + item lines.
// One default/from/to mapping reused by the PO wrapper (and future purchasing
// docs that share the item-based shape). Mirrors the cash/bank form model pattern.

import { newPurItemLine, type PurItemLineRow } from '@/components/organisms/pur-item-lines';
import { TODAY_DEFAULT, type ErpFormField } from '@/lib/api/form-fields';
import type { FormFieldsConfig } from '@/lib/use-form-fields';
import type {
  CreatePurOrderPayload,
  ErpDocumentStatus,
  ErpPriceMode,
  ErpPurOrder,
} from '@/lib/api/pur-orders';

export interface PurOrderFormData {
  id?: string;
  docNumber: string;
  auto: boolean;
  docDate: string;
  dueDate: string;
  supplierId: string;
  supplierLabel?: string;
  branchId: string;
  branchLabel?: string;
  locationId: string;
  locationLabel?: string;
  warehouseId: string;
  warehouseLabel?: string;
  payableAccountId: string;
  payableAccountLabel?: string;
  paymentTermId: string;
  paymentTermLabel?: string;
  currencyId: string;
  exchangeRate: string;
  priceMode: ErpPriceMode;
  description: string;
  referenceNo: string;
  notes: string;
  status: ErpDocumentStatus;
  postedAt?: string | null;
  grandTotal?: string;
  lines: PurItemLineRow[];
  /** Values for custom header fields added via Form Builder, keyed by fieldKey. */
  customFields: Record<string, string | number | null>;
}

const todayIso = () => new Date().toISOString().slice(0, 10);

export function defaultPurOrderForm(): PurOrderFormData {
  return {
    docNumber: '',
    auto: true,
    // Date driven by Form Builder default (Kosong / Hari ini / Tanggal tetap).
    docDate: '',
    dueDate: '',
    supplierId: '',
    branchId: '',
    locationId: '',
    warehouseId: '',
    payableAccountId: '',
    paymentTermId: '',
    currencyId: '',
    exchangeRate: '1',
    priceMode: 'TAX_EXCLUSIVE',
    description: '',
    referenceNo: '',
    notes: '',
    status: 'DRAFT',
    lines: [newPurItemLine()],
    customFields: {},
  };
}

/**
 * Built-in fallback header layout used until Form Builder config loads (or when
 * the transaction type has no saved config) — prevents an empty-header flash.
 * The authoritative layout always comes from the API (seed-erp-purchasing-forms.ts).
 */
export const DEFAULT_PUR_FORM_FIELDS: ErpFormField[] = [
  { fieldKey: 'supplierId', kind: 'STRUCTURAL', label: 'Supplier', fieldType: 'PARTNER', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'LEFT' },
  { fieldKey: 'description', kind: 'STRUCTURAL', label: 'Uraian', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'LEFT' },
  { fieldKey: 'referenceNo', kind: 'STRUCTURAL', label: 'No Referensi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'LEFT' },
  { fieldKey: 'branchId', kind: 'STRUCTURAL', label: 'Cabang', fieldType: 'BRANCH', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'CENTER' },
  { fieldKey: 'locationId', kind: 'STRUCTURAL', label: 'Lokasi', fieldType: 'LOCATION', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'CENTER' },
  { fieldKey: 'warehouseId', kind: 'STRUCTURAL', label: 'Gudang', fieldType: 'LOOKUP', lookupSource: 'warehouses', isRequired: false, isVisible: true, sortOrder: 2, columnSlot: 'CENTER' },
  { fieldKey: 'payableAccountId', kind: 'STRUCTURAL', label: 'Akun Hutang', fieldType: 'ACCOUNT', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'CENTER' },
  { fieldKey: 'docDate', kind: 'STRUCTURAL', label: 'Tanggal', fieldType: 'DATE', isRequired: true, isVisible: true, sortOrder: 0, columnSlot: 'RIGHT' },
  { fieldKey: 'docNumber', kind: 'STRUCTURAL', label: 'No Transaksi', fieldType: 'TEXT', isRequired: false, isVisible: true, sortOrder: 1, columnSlot: 'RIGHT' },
  { fieldKey: 'currencyId', kind: 'STRUCTURAL', label: 'Uang', fieldType: 'CURRENCY', isRequired: true, isVisible: true, sortOrder: 2, columnSlot: 'RIGHT' },
  { fieldKey: 'paymentTermId', kind: 'STRUCTURAL', label: 'Termin', fieldType: 'LOOKUP', lookupSource: 'payment-terms', isRequired: false, isVisible: true, sortOrder: 3, columnSlot: 'RIGHT' },
  { fieldKey: 'dueDate', kind: 'STRUCTURAL', label: 'Jatuh Tempo', fieldType: 'DATE', isRequired: false, isVisible: true, sortOrder: 4, columnSlot: 'RIGHT' },
];

/** Structural keys whose default value maps straight onto PurOrderFormData. */
const STRUCTURAL_DEFAULT_KEYS = [
  'supplierId', 'branchId', 'locationId', 'warehouseId', 'payableAccountId', 'paymentTermId',
  'currencyId', 'docDate', 'docNumber', 'description', 'referenceNo', 'dueDate',
] as const;

const STRUCTURAL_LABEL_KEYS: Record<string, keyof PurOrderFormData> = {
  supplierId: 'supplierLabel',
  branchId: 'branchLabel',
  locationId: 'locationLabel',
  warehouseId: 'warehouseLabel',
  payableAccountId: 'payableAccountLabel',
  paymentTermId: 'paymentTermLabel',
};

/** Patch of default values for a NEW form, derived from Form Builder config (fill-empty only). */
export function formDefaultsPatch(
  data: PurOrderFormData,
  config: FormFieldsConfig,
): Partial<PurOrderFormData> {
  const patch: Partial<PurOrderFormData> = {};
  const customPatch: Record<string, string | number | null> = {};
  const isEmpty = (v: unknown) => v == null || v === '';

  for (const key of Object.keys(config.byKey)) {
    const f = config.byKey[key];
    if (isEmpty(f.defaultValue)) continue;
    const val = f.fieldType === 'DATE' && f.defaultValue === TODAY_DEFAULT ? todayIso() : f.defaultValue!;
    if (f.kind === 'CUSTOM') {
      if (isEmpty(data.customFields[key])) customPatch[key] = val;
    } else if ((STRUCTURAL_DEFAULT_KEYS as readonly string[]).includes(key)) {
      if (isEmpty((data as unknown as Record<string, unknown>)[key])) {
        (patch as Record<string, unknown>)[key] = val;
        const labelKey = STRUCTURAL_LABEL_KEYS[key];
        if (labelKey && f.defaultValueLabel) {
          (patch as Record<string, unknown>)[labelKey] = f.defaultValueLabel;
        }
      }
    }
  }
  if (Object.keys(customPatch).length > 0) {
    patch.customFields = { ...data.customFields, ...customPatch };
  }
  return patch;
}

export function fromPurOrder(r: ErpPurOrder): PurOrderFormData {
  return {
    id: r.id,
    docNumber: r.docNumber,
    auto: !!r.autoNumber,
    docDate: r.docDate.slice(0, 10),
    dueDate: r.dueDate ? r.dueDate.slice(0, 10) : '',
    supplierId: r.supplierId ?? '',
    supplierLabel: r.supplier?.name,
    branchId: r.branchId,
    branchLabel: r.branch?.name,
    locationId: r.locationId ?? '',
    locationLabel: r.location?.name,
    warehouseId: r.warehouseId ?? '',
    warehouseLabel: r.warehouse?.name,
    payableAccountId: r.payableAccountId ?? '',
    payableAccountLabel: r.payableAccount?.name,
    paymentTermId: r.paymentTermId ?? '',
    paymentTermLabel: r.paymentTerm?.name,
    currencyId: r.currencyId,
    exchangeRate: r.exchangeRate,
    priceMode: r.priceMode,
    description: r.description ?? '',
    referenceNo: r.referenceNo ?? '',
    notes: r.notes ?? '',
    status: r.status,
    postedAt: r.postedAt,
    grandTotal: r.grandTotal,
    customFields: {},
    lines: r.lines.map((l) => ({
      key: `pl-${l.id ?? l.lineNo}`,
      itemId: l.itemId,
      itemLabel: l.item?.name,
      quantity: l.quantity,
      unitId: l.unitId,
      unitLabel: l.unit?.name,
      unitPrice: l.unitPrice,
      discountPercent: l.discountPercent ?? undefined,
      discountAmount: l.discountAmount ?? undefined,
      tax1Id: l.tax1Id ?? undefined,
      tax1Label: l.tax1?.name,
      tax2Id: l.tax2Id ?? undefined,
      warehouseId: l.warehouseId ?? undefined,
      warehouseLabel: l.warehouse?.name,
      notes: l.notes ?? undefined,
      costCenterId: l.costCenterId ?? undefined,
      divisionId: l.divisionId ?? undefined,
      subdivisionId: l.subdivisionId ?? undefined,
      projectId: l.projectId ?? undefined,
    })),
  };
}

export function toPurOrderPayload(d: PurOrderFormData): CreatePurOrderPayload {
  return {
    auto: d.auto,
    docNumber: d.auto ? undefined : d.docNumber || undefined,
    docDate: d.docDate,
    dueDate: d.dueDate || undefined,
    branchId: d.branchId,
    locationId: d.locationId || undefined,
    warehouseId: d.warehouseId || undefined,
    supplierId: d.supplierId || undefined,
    paymentTermId: d.paymentTermId || undefined,
    payableAccountId: d.payableAccountId || undefined,
    currencyId: d.currencyId,
    exchangeRate: d.exchangeRate || '1',
    priceMode: d.priceMode,
    description: d.description || undefined,
    referenceNo: d.referenceNo || undefined,
    notes: d.notes || undefined,
    lines: d.lines
      .filter((l) => l.itemId && Number(l.quantity) > 0)
      .map((l, i) => ({
        itemId: l.itemId,
        quantity: l.quantity,
        unitId: l.unitId,
        unitPrice: l.unitPrice || '0',
        discountPercent: l.discountPercent || undefined,
        discountAmount: l.discountAmount || undefined,
        tax1Id: l.tax1Id || undefined,
        tax2Id: l.tax2Id || undefined,
        warehouseId: l.warehouseId || undefined,
        costCenterId: l.costCenterId || undefined,
        divisionId: l.divisionId || undefined,
        subdivisionId: l.subdivisionId || undefined,
        projectId: l.projectId || undefined,
        notes: l.notes || undefined,
        lineNo: i + 1,
      })),
  };
}
